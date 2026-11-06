using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DTO;

namespace DAO
{
    public class KhoDAO
    {
        public static List<SanPhamTonKhoDTO> LayDanhSachTonKho()
        {
            using (var db = DatabaseContextFactory.Create())
                return db.SANPHAM.OrderBy(x => x.Masp).Select(ToStockDto).ToList();
        }

        public static List<SanPhamTonKhoDTO> TimKiemTonKho(string keyword)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var term = keyword ?? string.Empty;
                return db.SANPHAM.Where(x => x.Tensp.Contains(term) || x.Masp.ToString().Contains(term)).OrderBy(x => x.Masp).Select(ToStockDto).ToList();
            }
        }

        public static DataTable LayNhaCungCap()
        {
            var table = new DataTable();
            table.Columns.Add("Manhacc", typeof(int));
            table.Columns.Add("Tennhacc");
            using (var db = DatabaseContextFactory.Create())
                foreach (var supplier in db.NHACUNGCAP.OrderBy(x => x.Tennhacc)) table.Rows.Add(supplier.Manhacc, supplier.Tennhacc);
            return table;
        }

        public static bool NhapKho(int maNCC, string maNV, List<CartItemDTO> listnhapkho)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                db.Connection.Open();
                using (var transaction = db.Connection.BeginTransaction())
                {
                    db.Transaction = transaction;
                    var receipt = new NHAPKHO { Manhacc = maNCC, Manv = maNV, Ngaynhap = DateTime.Now };
                    db.NHAPKHO.InsertOnSubmit(receipt);
                    db.SubmitChanges();
                    foreach (var item in listnhapkho ?? new List<CartItemDTO>())
                    {
                        if (item.SoLuong <= 0) throw new ArgumentException("Số lượng nhập không hợp lệ.");
                        db.CHITIETNHAPKHO.InsertOnSubmit(new CHITIETNHAPKHO { Mank = receipt.Mank, Masp = item.MaSP, Soluongnhap = item.SoLuong, Gianhap = item.DonGia });
                        var product = db.SANPHAM.FirstOrDefault(x => x.Masp == item.MaSP);
                        if (product == null) throw new InvalidOperationException("Không tìm thấy sản phẩm.");
                        product.Soluongton += item.SoLuong;
                    }
                    db.SubmitChanges();
                    transaction.Commit();
                    return true;
                }
            }
        }

        public static bool XuatKho(string maNV, string lyDo, List<CartItemDTO> listxuatkho)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                db.Connection.Open();
                using (var transaction = db.Connection.BeginTransaction())
                {
                    db.Transaction = transaction;
                    var issue = new XUATKHO { Manv = maNV, LyDoxuat = lyDo, NgayXuat = DateTime.Now };
                    db.XUATKHO.InsertOnSubmit(issue);
                    db.SubmitChanges();
                    foreach (var item in listxuatkho ?? new List<CartItemDTO>())
                    {
                        if (item.SoLuong <= 0) throw new ArgumentException("Số lượng xuất không hợp lệ.");
                        var product = db.SANPHAM.FirstOrDefault(x => x.Masp == item.MaSP);
                        if (product == null || product.Soluongton < item.SoLuong) throw new InvalidOperationException("Tồn kho không đủ.");
                        db.CHITIETXUATKHO.InsertOnSubmit(new CHITIETXUATKHO { MaXK = issue.MaXK, Masp = item.MaSP, SoLuongXuat = item.SoLuong });
                        product.Soluongton -= item.SoLuong;
                    }
                    db.SubmitChanges();
                    transaction.Commit();
                    return true;
                }
            }
        }

        public static List<PhieuNhapDTO> LayLichSuNhap(DateTime? tuNgay, DateTime? denNgay, string tuKhoa)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from nk in db.NHAPKHO join ncc in db.NHACUNGCAP on nk.Manhacc equals ncc.Manhacc join nv in db.NHANVIEN on nk.Manv equals nv.Manv select new { nk, ncc, nv };
                if (tuNgay.HasValue) query = query.Where(x => x.nk.Ngaynhap >= tuNgay.Value.Date);
                if (denNgay.HasValue) query = query.Where(x => x.nk.Ngaynhap < denNgay.Value.Date.AddDays(1));
                if (!string.IsNullOrWhiteSpace(tuKhoa)) query = query.Where(x => x.ncc.Tennhacc.Contains(tuKhoa) || x.nv.Hoten.Contains(tuKhoa));
                return query.OrderByDescending(x => x.nk.Ngaynhap).Select(x => new PhieuNhapDTO { MaNK = x.nk.Mank, NgayNhap = x.nk.Ngaynhap, TenNCC = x.ncc.Tennhacc, TenNhanVien = x.nv.Hoten, TongTien = db.CHITIETNHAPKHO.Where(c => c.Mank == x.nk.Mank).Sum(c => (decimal?)c.Soluongnhap * c.Gianhap) ?? 0m }).ToList();
            }
        }

        public static List<ChiTietKhoDTO> LayChiTietPhieuNhap(int maNK)
        {
            using (var db = DatabaseContextFactory.Create())
                return (from c in db.CHITIETNHAPKHO join sp in db.SANPHAM on c.Masp equals sp.Masp join nk in db.NHAPKHO on c.Mank equals nk.Mank join nv in db.NHANVIEN on nk.Manv equals nv.Manv where c.Mank == maNK select new ChiTietKhoDTO { MaPhieu = c.Mank, MaSP = c.Masp, TenSP = sp.Tensp, SoLuong = c.Soluongnhap, DonGia = c.Gianhap, TenNhanVien = nv.Hoten }).ToList();
        }

        public static List<PhieuXuatDTO> LayLichSuXuat(DateTime? tuNgay, DateTime? denNgay, string tuKhoa)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from xk in db.XUATKHO join nv in db.NHANVIEN on xk.Manv equals nv.Manv select new { xk, nv };
                if (tuNgay.HasValue) query = query.Where(x => x.xk.NgayXuat >= tuNgay.Value.Date);
                if (denNgay.HasValue) query = query.Where(x => x.xk.NgayXuat < denNgay.Value.Date.AddDays(1));
                if (!string.IsNullOrWhiteSpace(tuKhoa)) query = query.Where(x => x.nv.Hoten.Contains(tuKhoa) || x.xk.LyDoxuat.Contains(tuKhoa));
                return query.OrderByDescending(x => x.xk.NgayXuat).Select(x => new PhieuXuatDTO { MaXK = x.xk.MaXK, NgayXuat = x.xk.NgayXuat, TenNhanVien = x.nv.Hoten, LyDo = x.xk.LyDoxuat }).ToList();
            }
        }

        public static List<ChiTietKhoDTO> LayChiTietPhieuXuat(int maXK)
        {
            using (var db = DatabaseContextFactory.Create())
                return (from c in db.CHITIETXUATKHO join sp in db.SANPHAM on c.Masp equals sp.Masp where c.MaXK == maXK select new ChiTietKhoDTO { MaPhieu = c.MaXK, MaSP = c.Masp, TenSP = sp.Tensp, SoLuong = c.SoLuongXuat, DonGia = 0m, TenNhanVien = string.Empty }).ToList();
        }

        private static SanPhamTonKhoDTO ToStockDto(SANPHAM product) => new SanPhamTonKhoDTO { MaSP = product.Masp, TenSP = product.Tensp, SoLuongTon = product.Soluongton, CanhBaoTon = product.Canhbaotonkho, GiaBan = product.Giaban };
    }
}