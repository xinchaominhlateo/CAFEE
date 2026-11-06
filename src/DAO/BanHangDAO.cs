using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAO
{
    public class BanHangDAO
    {
        private static BanHangDAO instance;
        public static BanHangDAO Instance => instance ??= new BanHangDAO();

        public bool KiemTraSanPhamPhuHopTheoLoai(List<DanhSachSanPhamDTO> danhSach, int maloai)
        {
            return danhSach != null && danhSach.Any(sp => !sp.LaSanPhamTang && sp.Maloai == maloai);
        }

        public DataTable GetSanPhamTangByVoucher(int mavc, int maloaiGoc, int loaiVC)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var voucher = db.VOUCHER.FirstOrDefault(x => x.Mavc == mavc);
                var query = db.SANPHAM.AsQueryable();
                if (voucher != null && voucher.Maloai.HasValue) query = query.Where(x => x.Maloai == voucher.Maloai.Value);
                if (maloaiGoc > 0) query = query.Where(x => x.Maloai == maloaiGoc);
                return ToProductTable(query.Join(db.LOAISP, x => x.Maloai, x => x.Maloai, (sp, loai) => new ProductRow { Product = sp, Category = loai }).ToList());
            }
        }

        public DataTable GetSanPhamTangByVoucher(int mavc) => GetSanPhamTangByVoucher(mavc, 0, 0);

        public void ApDungVoucher(int mavc, int mahd)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                if (!db.VOUCHER.Any(x => x.Mavc == mavc) || !db.HOADON.Any(x => x.Mahd == mahd)) throw new InvalidOperationException("Voucher hoặc hóa đơn không tồn tại.");
                if (!db.APMAVC.Any(x => x.Mavc == mavc && x.Mahd == mahd))
                {
                    db.APMAVC.InsertOnSubmit(new APMAVC { Mavc = mavc, Mahd = mahd });
                    db.SubmitChanges();
                }
            }
        }

        public int TaoHoaDon(int? makh, string manv, decimal tongTienGoc, decimal tienGiam, decimal tongTienSauGiam)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var invoice = new HOADON { Makh = makh, Manv = manv, Ngaylap = DateTime.Now, TongTienGoc = tongTienGoc, TienGiam = tienGiam, TongTien = tongTienSauGiam };
                db.HOADON.InsertOnSubmit(invoice);
                db.SubmitChanges();
                return invoice.Mahd;
            }
        }

        public void ThemChiTietHoaDon(int mahd, BanHangDTO sp)
        {
            if (sp == null) throw new ArgumentNullException(nameof(sp));
            if (sp.Masp <= 0 || sp.SoLuong <= 0 || sp.GiaBan < 0) throw new ArgumentException("Dữ liệu sản phẩm không hợp lệ.");
            using (var db = DatabaseContextFactory.Create())
            {
                db.CHITIETHD.InsertOnSubmit(new CHITIETHD { Mahd = mahd, Masp = sp.Masp, Soluong = sp.SoLuong, Dongia = sp.GiaBan, IsTang = sp.LaSanPhamTang });
                db.SubmitChanges();
            }
        }

        public void TruTonKho(int masp, int soLuong)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var product = db.SANPHAM.FirstOrDefault(x => x.Masp == masp);
                if (product == null || soLuong <= 0 || product.Soluongton < soLuong) throw new InvalidOperationException("Tồn kho không đủ.");
                product.Soluongton -= soLuong;
                db.SubmitChanges();
            }
        }

        public int XuatHoaDon(int? makh, string manv, List<BanHangDTO> danhSachBanHang, int? mavc)
        {
            if (string.IsNullOrWhiteSpace(manv)) throw new ArgumentException("manv is required.", nameof(manv));
            if (danhSachBanHang == null || danhSachBanHang.Count == 0) throw new ArgumentException("No products.", nameof(danhSachBanHang));

            var tongTienGoc = danhSachBanHang.Where(x => !x.LaSanPhamTang).Sum(x => x.GiaGoc * x.SoLuong);
            var tienGiam = danhSachBanHang.Where(x => !x.LaSanPhamTang).Sum(x => x.TienGiam);
            var tongTienSauGiam = Math.Max(0, tongTienGoc - tienGiam);
            var groupedLines = danhSachBanHang
                .GroupBy(x => new { x.Masp, IsTang = x.LaSanPhamTang })
                .Select(group => new
                {
                    group.Key.Masp,
                    group.Key.IsTang,
                    Soluong = group.Sum(x => x.SoLuong),
                    Dongia = group.First().GiaBan
                })
                .ToList();

            using (var db = DatabaseContextFactory.Create())
            {
                db.Connection.Open();
                using (var transaction = db.Connection.BeginTransaction())
                {
                    db.Transaction = transaction;
                    try
                    {
                        var invoice = new HOADON
                        {
                            Makh = makh,
                            Manv = manv,
                            Ngaylap = DateTime.Now,
                            TongTienGoc = tongTienGoc,
                            TienGiam = tienGiam,
                            TongTien = tongTienSauGiam
                        };
                        db.HOADON.InsertOnSubmit(invoice);
                        db.SubmitChanges();

                        foreach (var line in groupedLines)
                        {
                            if (line.Masp <= 0 || line.Soluong <= 0 || line.Dongia < 0)
                                throw new ArgumentException("Dữ liệu sản phẩm không hợp lệ.");

                            db.CHITIETHD.InsertOnSubmit(new CHITIETHD
                            {
                                Mahd = invoice.Mahd,
                                Masp = line.Masp,
                                Soluong = line.Soluong,
                                Dongia = line.Dongia,
                                IsTang = line.IsTang
                            });
                        }

                        if (mavc.HasValue)
                        {
                            if (!db.VOUCHER.Any(x => x.Mavc == mavc.Value))
                                throw new InvalidOperationException("Voucher không tồn tại.");

                            db.APMAVC.InsertOnSubmit(new APMAVC { Mavc = mavc.Value, Mahd = invoice.Mahd });
                        }

                        foreach (var line in groupedLines.Where(x => !x.IsTang))
                        {
                            var product = db.SANPHAM.FirstOrDefault(x => x.Masp == line.Masp);
                            if (product == null) throw new InvalidOperationException("Không tìm thấy sản phẩm.");
                            if (product.Soluongton < line.Soluong) throw new InvalidOperationException("Tồn kho không đủ.");
                            product.Soluongton -= line.Soluong;
                        }

                        db.SubmitChanges();
                        transaction.Commit();
                        return invoice.Mahd;
                    }
                    catch
                    {
                        try { transaction.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public List<BanHangDTO> LayTatCaSanPham() => QueryProducts(null);
        public List<BanHangDTO> TimKiemSanPham(string searchType, string keyword) => QueryProducts(keyword);

        private static List<BanHangDTO> QueryProducts(string keyword)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from sp in db.SANPHAM join loai in db.LOAISP on sp.Maloai equals loai.Maloai select new { sp, loai };
                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var term = keyword.Trim();
                    query = query.Where(x => x.sp.Tensp.Contains(term) || x.sp.Masp.ToString().Contains(term));
                }
                return query.OrderBy(x => x.sp.Masp).Select(x => new BanHangDTO { Masp = x.sp.Masp, MaSP = x.sp.Masp.ToString(), TenSP = x.sp.Tensp, TenLoai = x.loai.Tenloai, GiaBan = x.sp.Giaban, DuongDanAnh = x.sp.Duongdananh, Maloai = x.sp.Maloai, SoLuongTon = x.sp.Soluongton, TrangThaiText = x.sp.Trangthaisp ? "Đang bán" : "Ngừng bán" }).ToList();
            }
        }

        private static DataTable ToProductTable(IEnumerable<ProductRow> rows)
        {
            var table = new DataTable();
            table.Columns.Add("Masp", typeof(int)); table.Columns.Add("Tensp"); table.Columns.Add("Maloai", typeof(int)); table.Columns.Add("Tenloai"); table.Columns.Add("Giaban", typeof(decimal)); table.Columns.Add("Soluongton", typeof(int));
            foreach (var row in rows) table.Rows.Add(row.Product.Masp, row.Product.Tensp, row.Product.Maloai, row.Category.Tenloai, row.Product.Giaban, row.Product.Soluongton);
            return table;
        }

        private sealed class ProductRow
        {
            public SANPHAM Product { get; set; }
            public LOAISP Category { get; set; }
        }
    }
}