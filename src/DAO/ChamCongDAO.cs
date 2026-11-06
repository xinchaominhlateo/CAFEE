using DTO;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DAO
{
    public class ChamCongDAO
    {
        private static ChamCongDAO instance;
        public static ChamCongDAO Instance => instance ??= new ChamCongDAO();

        private DataProvider provider = DataProvider.Instance;

        // Bắt đầu làm việc
        public bool InsertBatDauLam(string manv, DateTime gioBatDau)
        {
            string query = @"
                INSERT INTO CHAMCONG (Manv, Ngay, GioBatDau)
                VALUES (@Manv, @Ngay, @GioBatDau)";
            SqlParameter[] pr =
            {
                new SqlParameter("@Manv", manv),
                new SqlParameter("@Ngay", gioBatDau.Date),
                new SqlParameter("@GioBatDau", gioBatDau)
            };
            return provider.ExecuteNonQuery(query, pr) > 0;
        }

        // Kết thúc ca
        public bool UpdateChamCong(string manv, DateTime ngay, DateTime gioKetThuc, int tongPhut)
        {
            string query = @"
                UPDATE CHAMCONG
                SET GioKetThuc = @GioKT, TongThoiGian = @Tong
                WHERE Manv = @Manv AND Ngay = @Ngay AND GioKetThuc IS NULL";
            SqlParameter[] pr =
            {
                new SqlParameter("@Manv", manv),
                new SqlParameter("@Ngay", ngay),
                new SqlParameter("@GioKT", gioKetThuc),
                new SqlParameter("@Tong", tongPhut)
            };
            return provider.ExecuteNonQuery(query, pr) > 0;
        }

        // Lấy giờ bắt đầu chưa chấm công
        public DateTime? GetGioBatDauChuaChamCong(string manv, DateTime ngay)
        {
            string query = @"
                SELECT TOP 1 GioBatDau
                FROM CHAMCONG
                WHERE Manv = @Manv AND Ngay = @Ngay AND GioKetThuc IS NULL";
            SqlParameter[] pr =
            {
                new SqlParameter("@Manv", manv),
                new SqlParameter("@Ngay", ngay)
            };
            object rs = provider.ExecuteScalar(query, pr);
            return rs == DBNull.Value ? null : (DateTime?)rs;
        }

        // Tổng phút trong ngày
        public int GetTongPhutTrongNgay(string manv, DateTime ngay)
        {
            string query = @"
                SELECT ISNULL(SUM(TongThoiGian),0)
                FROM CHAMCONG
                WHERE Manv = @Manv AND Ngay = @Ngay";
            SqlParameter[] pr =
            {
                new SqlParameter("@Manv", manv),
                new SqlParameter("@Ngay", ngay)
            };
            return Convert.ToInt32(provider.ExecuteScalar(query, pr));
        }

        // Lịch sử chấm công
        public List<ChamCongDTO> GetLichSuChamCongChiTiet(string keyword, DateTime tuNgay, DateTime denNgay)
        {
            string query = @"
                SELECT 
                    c.Idcc,
                    c.Manv,
                    n.Hoten,
                    n.Luong,
                    c.Ngay,
                    c.GioBatDau,
                    c.GioKetThuc,
                    c.TongThoiGian,
                    (ISNULL(c.TongThoiGian,0) / 60.0) * n.Luong AS TongLuong
                FROM CHAMCONG c
                JOIN NHANVIEN n ON c.Manv = n.Manv
                WHERE c.Ngay BETWEEN @TuNgay AND @DenNgay
                  AND (@Kw = '' OR n.Hoten LIKE @Kw OR c.Manv LIKE @Kw)
                ORDER BY c.Ngay DESC";

            SqlParameter[] pr =
            {
                new SqlParameter("@TuNgay", tuNgay),
                new SqlParameter("@DenNgay", denNgay),
                new SqlParameter("@Kw", $"%{keyword}%")
            };

            DataTable dt = provider.ExecuteQuery(query, pr);
            List<ChamCongDTO> list = new();

            foreach (DataRow r in dt.Rows)
            {
                list.Add(new ChamCongDTO
                {
                    Id = Convert.ToInt32(r["Idcc"]),
                    Manv = r["Manv"].ToString(),
                    TenNhanVien = r["Hoten"].ToString(),
                    Luong = Convert.ToDecimal(r["Luong"]),
                    Ngay = Convert.ToDateTime(r["Ngay"]),
                    GioBatDau = Convert.ToDateTime(r["GioBatDau"]),
                    GioKetThuc = r["GioKetThuc"] == DBNull.Value ? null : (DateTime?)r["GioKetThuc"],
                    TongThoiGian = r["TongThoiGian"] == DBNull.Value ? null : Convert.ToInt32(r["TongThoiGian"]),
                    TongLuong = Convert.ToDecimal(r["TongLuong"])
                });
            }

            return list;
        }

        public decimal GetLuongTheoGio(string manv)
        {
            string query = "SELECT Luong FROM NHANVIEN WHERE Manv = @Manv";
            SqlParameter[] pr = { new SqlParameter("@Manv", manv) };
            object rs = provider.ExecuteScalar(query, pr);
            return rs == DBNull.Value ? 0 : Convert.ToDecimal(rs);
        }
    }
}
