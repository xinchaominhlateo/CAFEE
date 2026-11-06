using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAO
{
    public class ThongKeDAO
    {
        private DataProvider provider = DataProvider.Instance;

        // ================= LOẠI SẢN PHẨM =================
        public List<LoaiSPDTO> GetLoaiSP()
        {
            List<LoaiSPDTO> list = new List<LoaiSPDTO>();
            string query = "SELECT Maloai, Tenloai FROM LOAISP";
            DataTable data = provider.ExecuteQuery(query);

            foreach (DataRow row in data.Rows)
            {
                list.Add(new LoaiSPDTO
                {
                    MaLoai = (int)row["Maloai"],
                    TenLoai = row["Tenloai"].ToString()
                });
            }
            return list;
        }

        // ================= HÓA ĐƠN =================
        public List<HoaDonDTO> GetHoaDonList(DateTime? tuNgay, DateTime? denNgay, int? maLoai)
        {
            List<HoaDonDTO> list = new List<HoaDonDTO>();
            string query = @"
                SELECT 
                    h.Mahd,
                    h.Ngaylap,
                    h.TongTienGoc,
                    h.TienGiam,
                    h.TongTien,
                    ISNULL(k.Tenkh, N'Khách vãng lai') AS TenKH,
                    ISNULL(k.Sdt, '') AS SDTKH,
                    nv.Hoten AS TenNhanVien,
                    v.Code AS MaVoucher,
                    v.Giatri,
                    v.Maloaivc
                FROM HOADON h
                JOIN NHANVIEN nv ON h.Manv = nv.Manv
                LEFT JOIN KHACHHANG k ON h.Makh = k.Makh
                LEFT JOIN APMAVC ap ON h.Mahd = ap.Mahd
                LEFT JOIN VOUCHER v ON ap.Mavc = v.Mavc
                WHERE (@tuNgay IS NULL OR CAST(h.Ngaylap AS DATE) >= @tuNgay)
                  AND (@denNgay IS NULL OR CAST(h.Ngaylap AS DATE) <= @denNgay)
                ORDER BY h.Ngaylap DESC";

            SqlParameter[] param =
            {
                new SqlParameter("@tuNgay", tuNgay ?? (object)DBNull.Value),
                new SqlParameter("@denNgay", denNgay ?? (object)DBNull.Value)
            };

            DataTable data = provider.ExecuteQuery(query, param);

            foreach (DataRow row in data.Rows)
            {
                int mahd = (int)row["Mahd"];
                list.Add(new HoaDonDTO
                {
                    MaHD = mahd,
                    NgayLap = (DateTime)row["Ngaylap"],
                    TenNhanVien = row["TenNhanVien"].ToString(),
                    TenKH = row["TenKH"].ToString(),
                    SDTKH = row["SDTKH"].ToString(),
                    TongTienGoc = (decimal)row["TongTienGoc"],
                    TienGiam = (decimal)row["TienGiam"],
                    TongTien = (decimal)row["TongTien"],
                    MaVoucher = row["MaVoucher"]?.ToString(),
                    PhanTramGiam = row["Maloaivc"] != DBNull.Value && (int)row["Maloaivc"] == 1
                        ? Convert.ToInt32(row["Giatri"])
                        : null,
                    LoaiVoucher = row["Maloaivc"] != DBNull.Value ? (int?)row["Maloaivc"] : null,
                    SanPhamMua = GetSanPham(mahd, false),
                    SanPhamTang = GetSanPham(mahd, true),
                    SanPhamDuocGiam = GetSanPhamDuocGiam(mahd)
                });
            }
            return list;
        }

        // ================= SẢN PHẨM TRONG HÓA ĐƠN =================
        private List<DanhSachSanPhamDTO> GetSanPham(int mahd, bool isTang)
        {
            string query = @"
                SELECT 
                    sp.Masp,
                    sp.Tensp,
                    sp.Maloai,
                    sp.Duongdananh,
                    ct.Soluong
                FROM CHITIETHD ct
                JOIN SANPHAM sp ON ct.Masp = sp.Masp
                WHERE ct.Mahd = @mahd AND ct.IsTang = @isTang";

            SqlParameter[] param =
            {
                new SqlParameter("@mahd", mahd),
                new SqlParameter("@isTang", isTang)
            };

            DataTable data = provider.ExecuteQuery(query, param);
            List<DanhSachSanPhamDTO> list = new List<DanhSachSanPhamDTO>();

            foreach (DataRow row in data.Rows)
            {
                list.Add(new DanhSachSanPhamDTO
                {
                    MaSP = row["Masp"].ToString(),
                    TenSP = row["Tensp"].ToString(),
                    Maloai = (int)row["Maloai"],
                    DuongDanAnh = row["Duongdananh"].ToString(),
                    SoLuong = (int)row["Soluong"],
                    LaSanPhamTang = isTang
                });
            }
            return list;
        }

        // ================= SẢN PHẨM ĐƯỢC GIẢM =================
        private List<DanhSachSanPhamDTO> GetSanPhamDuocGiam(int mahd)
        {
            string query = @"
        SELECT 
            sp.Masp,
            sp.Tensp,
            sp.Maloai,
            sp.Duongdananh,
            ct.Soluong,
            v.Giatri,
            v.Maloaivc,
            v.maloai AS MaLoaiVoucher
        FROM CHITIETHD ct
        JOIN SANPHAM sp ON ct.Masp = sp.Masp
        LEFT JOIN APMAVC ap ON ct.Mahd = ap.Mahd
        LEFT JOIN VOUCHER v ON ap.Mavc = v.Mavc
        WHERE ct.Mahd = @mahd AND ct.IsTang = 0
          AND v.Mavc IS NOT NULL
          AND (v.Maloaivc = 1 OR v.maloai = sp.Maloai)"; // voucher % hoặc áp dụng cho loại sản phẩm

            SqlParameter[] param =
            {
        new SqlParameter("@mahd", mahd)
    };

            DataTable data = provider.ExecuteQuery(query, param);
            List<DanhSachSanPhamDTO> list = new List<DanhSachSanPhamDTO>();

            foreach (DataRow row in data.Rows)
            {
                decimal? phanTramGiam = null;
                if ((int)row["Maloaivc"] == 1) // giảm % theo hóa đơn
                    phanTramGiam = Convert.ToDecimal(row["Giatri"]);

                list.Add(new DanhSachSanPhamDTO
                {
                    MaSP = row["Masp"].ToString(),
                    TenSP = row["Tensp"].ToString(),
                    Maloai = (int)row["Maloai"],
                    DuongDanAnh = row["Duongdananh"].ToString(),
                    SoLuong = (int)row["Soluong"],
                    LaSanPhamTang = false,
                    PhanTramGiam = phanTramGiam,
                    LoaiVoucher = row["Maloaivc"] != DBNull.Value ? (int?)row["Maloaivc"] : null
                });
            }
            return list;
        }


        // ================= DOANH THU =================
        public List<DoanhThuChartDTO> GetDoanhThuData(DateTime? tuNgay, DateTime? denNgay, int? maLoai)
        {
            string query = @"
                SELECT 
                    CAST(h.Ngaylap AS DATE) AS Ngay,
                    SUM(ct.Soluong * ct.Dongia) AS TongDoanhThu
                FROM HOADON h
                JOIN CHITIETHD ct ON h.Mahd = ct.Mahd
                JOIN SANPHAM sp ON ct.Masp = sp.Masp
                WHERE ct.IsTang = 0
                  AND (@tuNgay IS NULL OR CAST(h.Ngaylap AS DATE) >= @tuNgay)
                  AND (@denNgay IS NULL OR CAST(h.Ngaylap AS DATE) <= @denNgay)
                  AND (@maLoai IS NULL OR sp.Maloai = @maLoai)
                GROUP BY CAST(h.Ngaylap AS DATE)
                ORDER BY Ngay";

            SqlParameter[] param =
            {
                new SqlParameter("@tuNgay", tuNgay ?? (object)DBNull.Value),
                new SqlParameter("@denNgay", denNgay ?? (object)DBNull.Value),
                new SqlParameter("@maLoai", maLoai ?? (object)DBNull.Value)
            };

            DataTable data = provider.ExecuteQuery(query, param);
            List<DoanhThuChartDTO> list = new List<DoanhThuChartDTO>();

            foreach (DataRow row in data.Rows)
            {
                list.Add(new DoanhThuChartDTO
                {
                    Ngay = (DateTime)row["Ngay"],
                    TongDoanhThu = (decimal)row["TongDoanhThu"]
                });
            }
            return list;
        }

        // ================= SẢN PHẨM BÁN CHẠY =================
        public List<SanPhamBanChayDTO> GetSanPhamBanChay(DateTime? tuNgay, DateTime? denNgay, int? maLoai)
        {
            string query = @"
                SELECT TOP 10 
                    sp.Masp,
                    sp.Tensp,
                    sp.Maloai,
                    sp.Duongdananh,
                    SUM(ct.Soluong) AS SoLuong,
                    SUM(ct.Soluong * ct.Dongia) AS TongDoanhThu
                FROM CHITIETHD ct
                JOIN SANPHAM sp ON ct.Masp = sp.Masp
                JOIN HOADON h ON ct.Mahd = h.Mahd
                WHERE ct.IsTang = 0
                  AND (@tuNgay IS NULL OR CAST(h.Ngaylap AS DATE) >= @tuNgay)
                  AND (@denNgay IS NULL OR CAST(h.Ngaylap AS DATE) <= @denNgay)
                  AND (@maLoai IS NULL OR sp.Maloai = @maLoai)
                GROUP BY sp.Masp, sp.Tensp, sp.Maloai, sp.Duongdananh
                ORDER BY SoLuong DESC";

            SqlParameter[] param =
            {
                new SqlParameter("@tuNgay", tuNgay ?? (object)DBNull.Value),
                new SqlParameter("@denNgay", denNgay ?? (object)DBNull.Value),
                new SqlParameter("@maLoai", maLoai ?? (object)DBNull.Value)
            };

            DataTable data = provider.ExecuteQuery(query, param);
            List<SanPhamBanChayDTO> list = new List<SanPhamBanChayDTO>();

            foreach (DataRow row in data.Rows)
            {
                list.Add(new SanPhamBanChayDTO
                {
                    TenSP = row["Tensp"].ToString(),
                    SoLuongBan = Convert.ToInt32(row["SoLuong"]),
                    TongDoanhThu = Convert.ToDecimal(row["TongDoanhThu"])
                });
            }

            return list;
        }
    }
}
