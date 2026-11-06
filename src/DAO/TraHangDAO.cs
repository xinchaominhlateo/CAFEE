using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAO
{
    public class TraHangDAO
    {
        private DataProvider provider = DataProvider.Instance;

        // Lấy danh sách hóa đơn có thể trả hàng
        public DataTable GetDanhSachHoaDon(string tuKhoa = "")
        {
            string query = "SELECT hd.Mahd, hd.Ngaylap, nv.Hoten AS TenNhanVien, kh.Tenkh AS TenKhachHang, hd.TongTien " +
                           "FROM HOADON hd " +
                           "JOIN NHANVIEN nv ON hd.Manv = nv.Manv " +
                           "LEFT JOIN KHACHHANG kh ON hd.Makh = kh.Makh " +
                           "WHERE (@TuKhoa = '' OR CAST(hd.Mahd AS NVARCHAR) LIKE '%' + @TuKhoa + '%' OR kh.Tenkh LIKE '%' + @TuKhoa + '%') " +
                           "ORDER BY hd.Ngaylap DESC";

            SqlParameter[] parameters = { new SqlParameter("@TuKhoa", tuKhoa) };
            return provider.ExecuteQuery(query, parameters);
        }

        // Lấy chi tiết các sản phẩm của hóa đơn
        public List<ChiTietTraHangDTO> GetChiTietHoaDon(int maHD)
        {
            List<ChiTietTraHangDTO> list = new List<ChiTietTraHangDTO>();
            string query = @"SELECT ct.Masp, sp.Tensp, ct.Soluong, ct.Dongia
                             FROM CHITIETHD ct
                             JOIN SANPHAM sp ON ct.Masp = sp.Masp
                             WHERE ct.Mahd = @MaHD";

            SqlParameter[] parameters = { new SqlParameter("@MaHD", maHD) };
            DataTable dt = provider.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ChiTietTraHangDTO
                {
                    Masp = Convert.ToInt32(row["Masp"]),
                    TenSP = row["Tensp"].ToString(),
                    SoLuong = Convert.ToInt32(row["Soluong"]),
                    DonGia = Convert.ToDecimal(row["Dongia"])
                });
            }

            return list;
        }

        // Thêm phiếu trả hàng
        public int ThemTraHang(int maHD, string manv, string lyDo)
        {
            int maTra = 0;
            string query = "INSERT INTO TRAHANG (Mahd, Manv, Lydotra) VALUES (@MaHD, @Manv, @LyDo); SET @MaTra = SCOPE_IDENTITY();";
            SqlParameter[] parameters =
            {
                new SqlParameter("@MaHD", maHD),
                new SqlParameter("@Manv", manv),
                new SqlParameter("@LyDo", lyDo),
                new SqlParameter("@MaTra", SqlDbType.Int) {Direction = ParameterDirection.Output}
            };
            provider.ExecuteNonQuery(query, parameters);
            maTra = Convert.ToInt32(parameters[3].Value);
            return maTra;
        }

        // Thêm chi tiết trả hàng và cập nhật tồn kho
        public void ThemChiTietTraHang(int maTra, int masp, int soluong, decimal dongia)
        {
            string query = "INSERT INTO CTTRAHANG (Matra, Masp, Soluong, Dongia) VALUES (@MaTra, @Masp, @SoLuong, @DonGia); " +
                           "UPDATE SANPHAM SET soluongton = soluongton + @SoLuong WHERE Masp = @Masp";

            SqlParameter[] parameters =
            {
                new SqlParameter("@MaTra", maTra),
                new SqlParameter("@Masp", masp),
                new SqlParameter("@SoLuong", soluong),
                new SqlParameter("@DonGia", dongia)
            };

            provider.ExecuteNonQuery(query, parameters);
        }
    }
}
