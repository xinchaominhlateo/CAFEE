using DTO;
using System;
using System.Linq;

namespace DAO
{
    public class TaiKhoanDAO
    {
        public bool KiemTraMatKhauCu(string username, string hashedPassword)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var storedHash = db.NHANVIEN.Where(n => n.Tk == username).Select(n => n.Mk).FirstOrDefault();
                return storedHash != null && string.Equals(storedHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool CapNhatMatKhau(string username, string newHashedPassword)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var employee = db.NHANVIEN.FirstOrDefault(n => n.Tk == username);
                if (employee == null) return false;
                employee.Mk = newHashedPassword;
                db.SubmitChanges();
                return true;
            }
        }

        public NhanVienDTO DangNhap(string tk, string hashedPassword)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var employee = db.NHANVIEN.FirstOrDefault(n => n.Tk == tk && n.Mk == hashedPassword);
                if (employee == null) return null;
                return new NhanVienDTO
                {
                    Manv = employee.Manv,
                    Tk = employee.Tk,
                    Hoten = employee.Hoten,
                    Vitri = employee.Vitri,
                    Sdt = employee.Sdt,
                    Email = employee.Email,
                    Bank = employee.Bank,
                    Stk = employee.Stk,
                    Luong = employee.Luong ?? 0m,
                    NgaySinh = employee.Ngsinh ?? default(DateTime)
                };
            }
        }
    }
}