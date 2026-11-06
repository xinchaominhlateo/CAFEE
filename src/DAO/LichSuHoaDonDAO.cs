using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAO
{
    public class LichSuHoaDonDAO
    {
        private DataProvider provider = DataProvider.Instance;

        // 1. Tìm kiếm / lọc hóa đơn
        public List<LichSuHoaDonDTO> SearchHoaDon(string timKiem, string maNV, DateTime? tuNgay, DateTime? denNgay)
        {
            List<LichSuHoaDonDTO> list = new List<LichSuHoaDonDTO>();

            string query = @"
                SELECT 
                    h.Mahd,
                    h.Ngaylap,
                    nv.Hoten AS TenNhanVien,
                    ISNULL(k.Tenkh, N'Khách vãng lai') AS TenKhachHang,
                    h.TongTien
                FROM HOADON h
                JOIN NHANVIEN nv ON h.Manv = nv.Manv
                LEFT JOIN KHACHHANG k ON h.Makh = k.Makh
                WHERE
                    (@timKiem IS NULL 
                     OR k.Tenkh LIKE @timKiemLike
                     OR k.Sdt LIKE @timKiemLike
                     OR CAST(h.Mahd AS NVARCHAR) = @timKiem)
                AND (@maNV IS NULL OR h.Manv = @maNV)
                AND (@tuNgay IS NULL OR CAST(h.Ngaylap AS DATE) >= @tuNgay)
                AND (@denNgay IS NULL OR CAST(h.Ngaylap AS DATE) <= @denNgay)
                ORDER BY h.Ngaylap DESC";

            SqlParameter[] parameters =
            {
                new SqlParameter("@timKiem", (object)timKiem ?? DBNull.Value),
                new SqlParameter("@timKiemLike", timKiem == null ? DBNull.Value : $"%{timKiem}%"),
                new SqlParameter("@maNV", (object)maNV ?? DBNull.Value),
                new SqlParameter("@tuNgay", (object)tuNgay ?? DBNull.Value),
                new SqlParameter("@denNgay", (object)denNgay ?? DBNull.Value)
            };

            DataTable data = provider.ExecuteQuery(query, parameters);

            foreach (DataRow row in data.Rows)
            {
                list.Add(new LichSuHoaDonDTO
                {
                    MaHD = (int)row["Mahd"],
                    NgayLap = (DateTime)row["Ngaylap"],
                    TenNhanVien = row["TenNhanVien"].ToString(),
                    TenKhachHang = row["TenKhachHang"].ToString(),
                    TongTien = (decimal)row["TongTien"]
                });
            }
            return list;
        }

        // 2. Danh sách nhân viên
        public List<NhanVienDTO> GetNhanVienList()
        {
            List<NhanVienDTO> list = new List<NhanVienDTO>();
            string query = "SELECT Manv, Hoten, Vitri FROM NHANVIEN";

            DataTable data = provider.ExecuteQuery(query);
            foreach (DataRow row in data.Rows)
            {
                list.Add(new NhanVienDTO
                {
                    Mand = row["Manv"].ToString(),
                    Hoten = row["Hoten"].ToString(),
                    Vitri = row["Vitri"].ToString()
                });
            }
            return list;
        }

        // 3. Chi tiết hóa đơn
        public List<ChiTietLichSuDTO> GetChiTietHoaDon(int maHD)
        {
            List<ChiTietLichSuDTO> list = new List<ChiTietLichSuDTO>();

            string query = @"
                SELECT 
                    sp.Tensp,
                    ct.Soluong,
                    ct.Dongia,
                    ct.Thanhtien
                FROM CHITIETHD ct
                JOIN SANPHAM sp ON ct.Masp = sp.Masp
                WHERE ct.Mahd = @maHD";

            SqlParameter[] param = { new SqlParameter("@maHD", maHD) };
            DataTable data = provider.ExecuteQuery(query, param);

            foreach (DataRow row in data.Rows)
            {
                list.Add(new ChiTietLichSuDTO
                {
                    TenSP = row["Tensp"].ToString(),
                    SoLuong = (int)row["Soluong"],
                    DonGia = (decimal)row["Dongia"],
                    ThanhTien = (decimal)row["Thanhtien"]
                });
            }
            return list;
        }

        // 4. Thông tin cơ bản hóa đơn
        public HoaDonDayDuDTO GetThongTinCoBanHD(int maHD)
        {
            string query = @"
                SELECT 
                    h.Mahd,
                    h.Ngaylap,
                    h.TongTien,
                    nv.Hoten AS TenNhanVien,
                    ISNULL(k.Tenkh, N'Khách vãng lai') AS TenKhachHang,
                    k.Sdt,
                    ISNULL(k.Tichdiem, 0) AS TichDiem
                FROM HOADON h
                JOIN NHANVIEN nv ON h.Manv = nv.Manv
                LEFT JOIN KHACHHANG k ON h.Makh = k.Makh
                WHERE h.Mahd = @maHD";

            SqlParameter[] param = { new SqlParameter("@maHD", maHD) };
            DataTable data = provider.ExecuteQuery(query, param);

            if (data.Rows.Count == 0) return null;

            DataRow r = data.Rows[0];
            return new HoaDonDayDuDTO
            {
                MaHD = (int)r["Mahd"],
                NgayLap = (DateTime)r["Ngaylap"],
                TenNhanVien = r["TenNhanVien"].ToString(),
                TenKhachHang = r["TenKhachHang"].ToString(),
                SdtKhachHang = r["Sdt"]?.ToString(),
                TichDiem = (int)r["TichDiem"],
                TongTienCuoiCung = (decimal)r["TongTien"]
            };
        }

        // 5. Voucher đã dùng
        public List<string> GetVouchersSuDung(int maHD)
        {
            List<string> list = new List<string>();
            string query = @"
                SELECT v.Code
                FROM APMAVC a
                JOIN VOUCHER v ON a.Mavc = v.Mavc
                WHERE a.Mahd = @maHD";

            SqlParameter[] param = { new SqlParameter("@maHD", maHD) };
            DataTable data = provider.ExecuteQuery(query, param);

            foreach (DataRow row in data.Rows)
                list.Add(row["Code"].ToString());

            return list;
        }

        // 6. Lấy danh sách khách hàng theo nhân viên
        public List<KhachHangCuaNVDTO> GetKhachHangCuaNhanVien(string maNV)
        {
            List<KhachHangCuaNVDTO> list = new List<KhachHangCuaNVDTO>();

            string query = @"
        SELECT 
            k.Tenkh,
            k.Sdt,
            SUM(h.TongTien) AS TongChiTieu
        FROM HOADON h
        JOIN KHACHHANG k ON h.Makh = k.Makh
        WHERE h.Manv = @maNV
          AND h.Makh IS NOT NULL
        GROUP BY k.Tenkh, k.Sdt
        ORDER BY TongChiTieu DESC";

            SqlParameter[] param =
            {
        new SqlParameter("@maNV", maNV)
    };

            DataTable data = provider.ExecuteQuery(query, param);

            foreach (DataRow row in data.Rows)
            {
                list.Add(new KhachHangCuaNVDTO
                {
                    TenKH = row["Tenkh"].ToString(),
                    Sdt = row["Sdt"].ToString(),
                    TongChiTieu = (decimal)row["TongChiTieu"]
                });
            }

            return list;
        }

    }
}
