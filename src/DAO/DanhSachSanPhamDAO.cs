using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAO
{
    public class DanhSachSanPhamDAO
    {
        private static DanhSachSanPhamDAO instance;
        public static DanhSachSanPhamDAO Instance => instance ??= new DanhSachSanPhamDAO();

        public DataTable GetLoaiSanPham()
        {
            var table = new DataTable();
            table.Columns.Add("Maloai", typeof(int));
            table.Columns.Add("Tenloai");
            using (var db = DatabaseContextFactory.Create())
            {
                foreach (var category in db.LOAISP.OrderBy(x => x.Maloai))
                    table.Rows.Add(category.Maloai, category.Tenloai);
            }
            return table;
        }

        private List<DanhSachSanPhamDTO> QueryProducts(string searchTerm = null)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from sp in db.SANPHAM
                            join loai in db.LOAISP on sp.Maloai equals loai.Maloai
                            select new { sp, loai };
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    int productId;
                    if (int.TryParse(term, out productId))
                        query = query.Where(x => x.sp.Masp == productId || x.sp.Tensp.Contains(term));
                    else
                        query = query.Where(x => x.sp.Tensp.Contains(term) || x.loai.Tenloai.Contains(term));
                }

                return query.OrderBy(x => x.sp.Masp).Select(x => new DanhSachSanPhamDTO
                {
                    Masp = x.sp.Masp,
                    MaSP = x.sp.Masp.ToString(),
                    TenSP = x.sp.Tensp,
                    TenLoai = x.loai.Tenloai,
                    GiaBan = x.sp.Giaban,
                    SoLuongTon = x.sp.Soluongton,
                    TrangThaiText = x.sp.Trangthaisp ? "Đang bán" : "Ngừng bán",
                    DuongDanAnh = x.sp.Duongdananh,
                    Maloai = x.sp.Maloai
                }).ToList();
            }
        }

        public List<DanhSachSanPhamDTO> GetAllSanPham() => QueryProducts();

        public DataTable GetSanPhamTable()
        {
            var dt = CreateProductTable();
            foreach (var sp in GetAllSanPham())
                dt.Rows.Add(sp.MaSP, sp.TenSP, sp.TenLoai, string.Empty, sp.GiaBan, sp.SoLuongTon, sp.TrangThaiText);
            return dt;
        }

        public DataTable GetSanPhamWithVoucher() => GetSanPhamTable();

        public List<DanhSachSanPhamDTO> SearchSanPham(string searchType, string searchTerm) => QueryProducts(searchTerm);

        public bool ToggleTrangThaiSanPham(int masp)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var product = db.SANPHAM.FirstOrDefault(x => x.Masp == masp);
                if (product == null) return false;
                product.Trangthaisp = !product.Trangthaisp;
                db.SubmitChanges();
                return true;
            }
        }

        public List<int> GetChiTietVoucher(int mavc)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var category = db.VOUCHER.Where(x => x.Mavc == mavc).Select(x => x.Maloai).FirstOrDefault();
                return db.SANPHAM.Where(x => category.HasValue && x.Maloai == category.Value).Select(x => x.Masp).ToList();
            }
        }

        public bool DeleteSanPham(int masp)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var product = db.SANPHAM.FirstOrDefault(x => x.Masp == masp);
                if (product == null) return false;
                db.SANPHAM.DeleteOnSubmit(product);
                db.SubmitChanges();
                return true;
            }
        }

        public int CountKichCoSP(string maSP) => 0;

        public bool DeleteSanPhamGoc(string maSP)
        {
            var masp = GetMasp(maSP);
            return masp != 0 && DeleteSanPham(masp);
        }

        public SanPhamDTO GetSanPhamTheoMaVaKichCo(string maSP, string kichco)
        {
            int masp;
            if (!int.TryParse(maSP, out masp)) masp = GetMasp(maSP);
            if (masp == 0) return null;
            using (var db = DatabaseContextFactory.Create())
            {
                var product = db.SANPHAM.FirstOrDefault(x => x.Masp == masp);
                if (product == null) return null;
                return ToSanPhamDTO(product);
            }
        }

        public int GetMasp(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP)) return 0;
            int masp;
            using (var db = DatabaseContextFactory.Create())
            {
                if (int.TryParse(maSP, out masp))
                    return db.SANPHAM.Any(x => x.Masp == masp) ? masp : 0;
                return db.SANPHAM.Where(x => x.Tensp == maSP).Select(x => x.Masp).FirstOrDefault();
            }
        }

        public bool InsertSanPham(SanPhamDTO sp)
        {
            if (sp == null) return false;
            using (var db = DatabaseContextFactory.Create())
            {
                db.SANPHAM.InsertOnSubmit(new SANPHAM
                {
                    Tensp = sp.TenSP ?? string.Empty,
                    Maloai = sp.MaLoai,
                    Giaban = sp.GiaBan,
                    Canhbaotonkho = sp.CanhBaoTonKho,
                    Duongdananh = sp.DuongDanAnh,
                    Soluongton = sp.SoLuongTon,
                    Trangthaisp = sp.TrangThaiText == "Đang bán"
                });
                db.SubmitChanges();
                return true;
            }
        }

        public bool UpdateSanPham(SanPhamDTO sp)
        {
            if (sp == null) return false;
            using (var db = DatabaseContextFactory.Create())
            {
                var product = db.SANPHAM.FirstOrDefault(x => x.Masp == sp.Masp);
                if (product == null) return false;
                product.Tensp = sp.TenSP ?? string.Empty;
                product.Maloai = sp.MaLoai;
                product.Giaban = sp.GiaBan;
                product.Canhbaotonkho = sp.CanhBaoTonKho;
                product.Duongdananh = sp.DuongDanAnh;
                product.Soluongton = sp.SoLuongTon;
                product.Trangthaisp = sp.TrangThaiText == "Đang bán";
                db.SubmitChanges();
                return true;
            }
        }

        private static DataTable CreateProductTable()
        {
            var dt = new DataTable();
            dt.Columns.Add("Mã sản phẩm");
            dt.Columns.Add("Tên sản phẩm");
            dt.Columns.Add("Loại");
            dt.Columns.Add("Size");
            dt.Columns.Add("Giá bán", typeof(decimal));
            dt.Columns.Add("Số lượng", typeof(int));
            dt.Columns.Add("Trạng thái");
            return dt;
        }

        private static SanPhamDTO ToSanPhamDTO(SANPHAM product)
        {
            return new SanPhamDTO
            {
                Masp = product.Masp,
                MaSP = product.Masp.ToString(),
                TenSP = product.Tensp,
                MaLoai = product.Maloai,
                SoLuongTon = product.Soluongton,
                TrangThaiText = product.Trangthaisp ? "Đang bán" : "Ngừng bán",
                DuongDanAnh = product.Duongdananh,
                GiaBan = product.Giaban,
                CanhBaoTonKho = product.Canhbaotonkho
            };
        }
    }
}