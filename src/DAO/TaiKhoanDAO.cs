using DTO;
using System.Data.SqlClient;
using System;
using System.Data;

namespace DAO
{
    public class TaiKhoanDAO
    {
        // ✅ Kiểm tra mật khẩu cũ có đúng không (so sánh SHA-256)
        public bool KiemTraMatKhauCu(string username, string hashedPassword)
        {
            // sửa: đổi tên bảng thành NHANVIEN (schema dbo) vì NGUOIDUNG không tồn tại
            string query = "SELECT Mk FROM dbo.NHANVIEN WHERE Tk = @Tk";
            SqlParameter[] parameters = {
                new SqlParameter("@Tk", username)
            };

            DataTable dt = DataProvider.Instance.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return false; // Không tìm thấy tài khoản

            string storedHash = dt.Rows[0]["Mk"].ToString();

            // So sánh hash SHA-256 (không phân biệt hoa thường)
            return string.Equals(storedHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }

        // ✅ Cập nhật mật khẩu mới (lưu SHA-256 hash)
        public bool CapNhatMatKhau(string username, string newHashedPassword)
        {
            // sửa: đổi tên bảng thành NHANVIEN (schema dbo)
            string query = "UPDATE dbo.NHANVIEN SET Mk = @Mk WHERE Tk = @Tk";
            SqlParameter[] parameters = {
                new SqlParameter("@Mk", newHashedPassword),
                new SqlParameter("@Tk", username)
            };

            int result = DataProvider.Instance.ExecuteNonQuery(query, parameters);
            return result > 0;
        }

        // Xác thực đăng nhập
        public NhanVienDTO DangNhap(string tk, string hashedPassword)
        {
            string query = "EXEC dbo.sp_CheckDangNhap @Tk, @Mk";
            SqlParameter[] parameters =
            {
                new SqlParameter("@Tk", tk),
                new SqlParameter("@Mk", hashedPassword)
            };

            DataTable dt = DataProvider.Instance.ExecuteQuery(query, parameters);

            if (dt == null || dt.Rows.Count == 0)
                return null;

            var r = dt.Rows[0];
            var nv = new NhanVienDTO
            {
                Manv = r["Manv"]?.ToString(),
                Tk = r["Tk"]?.ToString(),
                Hoten = r["Hoten"]?.ToString(),
                Vitri = r["Vitri"]?.ToString(),
                Sdt = r.Table.Columns.Contains("Sdt") ? r["Sdt"]?.ToString() : null,
                Email = r.Table.Columns.Contains("email") ? r["email"]?.ToString() : null,
                Bank = r.Table.Columns.Contains("Bank") ? r["Bank"]?.ToString() : null,
                Stk = r.Table.Columns.Contains("stk") ? r["stk"]?.ToString() : null,
                Luong = (r.Table.Columns.Contains("Luong") && r["Luong"] != DBNull.Value) ? Convert.ToDecimal(r["Luong"]) : 0m,
                NgaySinh = (r.Table.Columns.Contains("Ngsinh") && r["Ngsinh"] != DBNull.Value) ? Convert.ToDateTime(r["Ngsinh"]) : default
            };

            return nv;
        }
    }
}