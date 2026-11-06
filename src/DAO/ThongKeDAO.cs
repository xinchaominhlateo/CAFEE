using System;
using System.Collections.Generic;
using System.Linq;
using DTO;

namespace DAO
{
    public class ThongKeDAO
    {
        public List<LoaiSPDTO> GetLoaiSP()
        {
            using (var db = DatabaseContextFactory.Create())
                return db.LOAISP.OrderBy(x => x.Maloai).Select(x => new LoaiSPDTO { MaLoai = x.Maloai, TenLoai = x.Tenloai }).ToList();
        }

        public List<HoaDonDTO> GetHoaDonList(DateTime? tuNgay, DateTime? denNgay, int? maLoai)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from h in db.HOADON
                            join nv in db.NHANVIEN on h.Manv equals nv.Manv
                            join k in db.KHACHHANG on h.Makh equals (int?)k.Makh into customers
                            from k in customers.DefaultIfEmpty()
                            join ap in db.APMAVC on h.Mahd equals ap.Mahd into applied
                            from ap in applied.DefaultIfEmpty()
                            join v in db.VOUCHER on ap.Mavc equals v.Mavc into vouchers
                            from v in vouchers.DefaultIfEmpty()
                            select new { h, nv, k, v };
                if (tuNgay.HasValue) query = query.Where(x => x.h.Ngaylap >= tuNgay.Value.Date);
                if (denNgay.HasValue) query = query.Where(x => x.h.Ngaylap < denNgay.Value.Date.AddDays(1));
                return query.OrderByDescending(x => x.h.Ngaylap).AsEnumerable().Select(x => new HoaDonDTO
                {
                    MaHD = x.h.Mahd,
                    NgayLap = x.h.Ngaylap,
                    TenNhanVien = x.nv.Hoten,
                    TenKH = x.k == null ? "Khách vãng lai" : x.k.Tenkh,
                    SDTKH = x.k == null ? string.Empty : x.k.Sdt ?? string.Empty,
                    TongTienGoc = x.h.TongTienGoc,
                    TienGiam = x.h.TienGiam,
                    TongTien = x.h.TongTien,
                    MaVoucher = x.v == null ? null : x.v.Code,
                    PhanTramGiam = x.v != null && x.v.Maloaivc == 1 ? (int?)Convert.ToInt32(x.v.Giatri) : null,
                    LoaiVoucher = x.v == null ? (int?)null : x.v.Maloaivc,
                    SanPhamMua = GetSanPham(db, x.h.Mahd, false),
                    SanPhamTang = GetSanPham(db, x.h.Mahd, true),
                    SanPhamDuocGiam = GetSanPhamDuocGiam(db, x.h.Mahd)
                }).ToList();
            }
        }

        private static List<DanhSachSanPhamDTO> GetSanPham(DataClasses1DataContext db, int mahd, bool isTang)
        {
            return (from ct in db.CHITIETHD join sp in db.SANPHAM on ct.Masp equals sp.Masp where ct.Mahd == mahd && ct.IsTang == isTang select new DanhSachSanPhamDTO { MaSP = sp.Masp.ToString(), TenSP = sp.Tensp, Maloai = sp.Maloai, DuongDanAnh = sp.Duongdananh, SoLuong = ct.Soluong, LaSanPhamTang = isTang }).ToList();
        }

        private static List<DanhSachSanPhamDTO> GetSanPhamDuocGiam(DataClasses1DataContext db, int mahd)
        {
            return (from ct in db.CHITIETHD
                    join sp in db.SANPHAM on ct.Masp equals sp.Masp
                    join ap in db.APMAVC on ct.Mahd equals ap.Mahd
                    join v in db.VOUCHER on ap.Mavc equals v.Mavc
                    where ct.Mahd == mahd && !ct.IsTang && (v.Maloaivc == 1 || v.Maloai == sp.Maloai)
                    select new DanhSachSanPhamDTO { MaSP = sp.Masp.ToString(), TenSP = sp.Tensp, Maloai = sp.Maloai, DuongDanAnh = sp.Duongdananh, SoLuong = ct.Soluong, LaSanPhamTang = false, PhanTramGiam = v.Maloaivc == 1 ? (decimal?)v.Giatri : null, LoaiVoucher = v.Maloaivc }).ToList();
        }

        public List<DoanhThuChartDTO> GetDoanhThuData(DateTime? tuNgay, DateTime? denNgay, int? maLoai)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from h in db.HOADON join ct in db.CHITIETHD on h.Mahd equals ct.Mahd join sp in db.SANPHAM on ct.Masp equals sp.Masp where !ct.IsTang select new { h, ct, sp };
                if (tuNgay.HasValue) query = query.Where(x => x.h.Ngaylap >= tuNgay.Value.Date);
                if (denNgay.HasValue) query = query.Where(x => x.h.Ngaylap < denNgay.Value.Date.AddDays(1));
                if (maLoai.HasValue) query = query.Where(x => x.sp.Maloai == maLoai.Value);
                return query.AsEnumerable().GroupBy(x => x.h.Ngaylap.Date).OrderBy(x => x.Key).Select(g => new DoanhThuChartDTO { Ngay = g.Key, TongDoanhThu = g.Sum(x => x.ct.Soluong * x.ct.Dongia) }).ToList();
            }
        }

        public List<SanPhamBanChayDTO> GetSanPhamBanChay(DateTime? tuNgay, DateTime? denNgay, int? maLoai)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from ct in db.CHITIETHD join sp in db.SANPHAM on ct.Masp equals sp.Masp join h in db.HOADON on ct.Mahd equals h.Mahd where !ct.IsTang select new { ct, sp, h };
                if (tuNgay.HasValue) query = query.Where(x => x.h.Ngaylap >= tuNgay.Value.Date);
                if (denNgay.HasValue) query = query.Where(x => x.h.Ngaylap < denNgay.Value.Date.AddDays(1));
                if (maLoai.HasValue) query = query.Where(x => x.sp.Maloai == maLoai.Value);
                return query.AsEnumerable().GroupBy(x => x.sp.Masp).OrderByDescending(g => g.Sum(x => x.ct.Soluong)).Take(10).Select(g => new SanPhamBanChayDTO { TenSP = g.First().sp.Tensp, SoLuongBan = g.Sum(x => x.ct.Soluong), TongDoanhThu = g.Sum(x => x.ct.Soluong * x.ct.Dongia) }).ToList();
            }
        }
    }
}
