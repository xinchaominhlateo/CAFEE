using System;
using System.Collections.Generic;
using System.Linq;
using DTO;

namespace DAO
{
    public class LichSuHoaDonDAO
    {
        public List<LichSuHoaDonDTO> SearchHoaDon(string timKiem, string maNV, DateTime? tuNgay, DateTime? denNgay)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from h in db.HOADON
                            join nv in db.NHANVIEN on h.Manv equals nv.Manv
                            join k in db.KHACHHANG on h.Makh equals (int?)k.Makh into customers
                            from k in customers.DefaultIfEmpty()
                            select new { h, nv, k };
                if (!string.IsNullOrWhiteSpace(timKiem))
                {
                    var term = timKiem.Trim();
                    query = query.Where(x => (x.k != null && (x.k.Tenkh.Contains(term) || x.k.Sdt.Contains(term))) || x.h.Mahd.ToString() == term);
                }
                if (!string.IsNullOrWhiteSpace(maNV)) query = query.Where(x => x.h.Manv == maNV);
                if (tuNgay.HasValue) query = query.Where(x => x.h.Ngaylap >= tuNgay.Value.Date);
                if (denNgay.HasValue) query = query.Where(x => x.h.Ngaylap < denNgay.Value.Date.AddDays(1));
                return query.OrderByDescending(x => x.h.Ngaylap).Select(x => new LichSuHoaDonDTO
                {
                    MaHD = x.h.Mahd,
                    NgayLap = x.h.Ngaylap,
                    TenNhanVien = x.nv.Hoten,
                    TenKhachHang = x.k == null ? "Khách vãng lai" : x.k.Tenkh,
                    TongTien = x.h.TongTien
                }).ToList();
            }
        }

        public List<NhanVienDTO> GetNhanVienList()
        {
            using (var db = DatabaseContextFactory.Create())
                return db.NHANVIEN.Select(n => new NhanVienDTO { Mand = n.Manv, Hoten = n.Hoten, Vitri = n.Vitri }).ToList();
        }

        public List<ChiTietLichSuDTO> GetChiTietHoaDon(int maHD)
        {
            using (var db = DatabaseContextFactory.Create())
                return (from ct in db.CHITIETHD
                        join sp in db.SANPHAM on ct.Masp equals sp.Masp
                        where ct.Mahd == maHD
                        select new ChiTietLichSuDTO
                        {
                            TenSP = sp.Tensp,
                            SoLuong = ct.Soluong,
                            DonGia = ct.Dongia,
                            ThanhTien = ct.Thanhtien ?? ct.Soluong * ct.Dongia
                        }).ToList();
        }

        public HoaDonDayDuDTO GetThongTinCoBanHD(int maHD)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var row = (from h in db.HOADON
                           join nv in db.NHANVIEN on h.Manv equals nv.Manv
                           join k in db.KHACHHANG on h.Makh equals (int?)k.Makh into customers
                           from k in customers.DefaultIfEmpty()
                           where h.Mahd == maHD
                           select new { h, nv, k }).FirstOrDefault();
                if (row == null) return null;
                return new HoaDonDayDuDTO
                {
                    MaHD = row.h.Mahd,
                    NgayLap = row.h.Ngaylap,
                    TenNhanVien = row.nv.Hoten,
                    TenKhachHang = row.k == null ? "Khách vãng lai" : row.k.Tenkh,
                    SdtKhachHang = row.k == null ? null : row.k.Sdt,
                    TichDiem = row.k == null ? 0 : row.k.Tichdiem,
                    TongTienCuoiCung = row.h.TongTien
                };
            }
        }

        public List<string> GetVouchersSuDung(int maHD)
        {
            using (var db = DatabaseContextFactory.Create())
                return (from ap in db.APMAVC join v in db.VOUCHER on ap.Mavc equals v.Mavc where ap.Mahd == maHD select v.Code).ToList();
        }

        public List<KhachHangCuaNVDTO> GetKhachHangCuaNhanVien(string maNV)
        {
            using (var db = DatabaseContextFactory.Create())
                return (from h in db.HOADON
                        join k in db.KHACHHANG on h.Makh equals (int?)k.Makh
                        where h.Manv == maNV
                        group h by new { k.Tenkh, k.Sdt } into g
                        orderby g.Sum(x => x.TongTien) descending
                        select new KhachHangCuaNVDTO { TenKH = g.Key.Tenkh, Sdt = g.Key.Sdt, TongChiTieu = g.Sum(x => x.TongTien) }).ToList();
        }
    }
}
