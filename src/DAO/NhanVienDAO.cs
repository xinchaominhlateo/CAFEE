using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DTO;

namespace DAO
{
    public class NhanVienDAO
    {
        public static List<NhanVienDTO> layDSNhanVien()
        {
            using (var db = DatabaseContextFactory.Create())
                return db.NHANVIEN.Select(ToDto).ToList();
        }

        public static void XoaNV(string maNV)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var employee = db.NHANVIEN.FirstOrDefault(n => n.Manv == maNV);
                if (employee == null) return;
                db.NHANVIEN.DeleteOnSubmit(employee);
                db.SubmitChanges();
            }
        }

        public static void SuaNV(NhanVienDTO nv)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var employee = db.NHANVIEN.FirstOrDefault(n => n.Manv == nv.Mand);
                if (employee == null) return;
                Apply(employee, nv);
                db.SubmitChanges();
            }
        }

        public static DataTable LayDanhSachNhanVien() => ToTable(layDSNhanVien());

        public static DataTable TimKiemNhanVien(string keyword)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var term = keyword ?? string.Empty;
                return ToTable(db.NHANVIEN.Where(n => n.Manv.Contains(term) || n.Hoten.Contains(term) || n.Tk.Contains(term)).Select(ToDto).ToList());
            }
        }

        public static List<NhanVienDTO> LayNhanVienTheoGio()
        {
            using (var db = DatabaseContextFactory.Create())
                return db.NHANVIEN.Where(n => n.Vitri.Contains("giờ") || n.Vitri.Contains("gio")).Select(ToDto).ToList();
        }

        public static void themNV(NhanVienDTO hs)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var employee = new NHANVIEN();
                Apply(employee, hs);
                db.NHANVIEN.InsertOnSubmit(employee);
                db.SubmitChanges();
            }
        }

        public static NhanVienDTO LayNhanVienTheoID(string id)
        {
            using (var db = DatabaseContextFactory.Create())
                return db.NHANVIEN.Where(n => n.Manv == id).Select(ToDto).FirstOrDefault();
        }

        public static string LayTenNhanVien(string mand)
        {
            using (var db = DatabaseContextFactory.Create())
                return db.NHANVIEN.Where(n => n.Manv == mand).Select(n => n.Hoten).FirstOrDefault() ?? "Không xác định";
        }

        public static bool KiemTraTonTaiMaNV(string maNV) => Exists(n => n.Manv == maNV);
        public static bool KiemTraTonTaiTenTaiKhoan(string tenTaiKhoan) => Exists(n => n.Tk == tenTaiKhoan);
        public static bool KiemTraTonTaiSdt(string sdt) => Exists(n => n.Sdt == sdt);
        public static bool KiemTraTonTaiEmail(string email) => Exists(n => n.Email == email);
        public static bool KiemTraTonTaiTenTaiKhoanKhac(string value, string excluded) => Exists(n => n.Tk == value && n.Manv != excluded);
        public static bool KiemTraTonTaiSdtKhac(string value, string excluded) => Exists(n => n.Sdt == value && n.Manv != excluded);
        public static bool KiemTraTonTaiEmailKhac(string value, string excluded) => Exists(n => n.Email == value && n.Manv != excluded);

        private static bool Exists(Func<NHANVIEN, bool> predicate)
        {
            using (var db = DatabaseContextFactory.Create()) return db.NHANVIEN.AsEnumerable().Any(predicate);
        }

        private static NhanVienDTO ToDto(NHANVIEN n) => new NhanVienDTO
        {
            Mand = n.Manv,
            Manv = n.Manv,
            Tk = n.Tk,
            Mk = n.Mk,
            Hoten = n.Hoten,
            Vitri = n.Vitri,
            Pos = n.Vitri,
            Sdt = n.Sdt,
            Email = n.Email,
            Bank = n.Bank,
            Stk = n.Stk,
            Luong = n.Luong ?? 0m,
            NgaySinh = n.Ngsinh ?? DateTime.MinValue
        };

        private static void Apply(NHANVIEN target, NhanVienDTO source)
        {
            target.Manv = source.Mand;
            target.Tk = source.Tk ?? string.Empty;
            target.Mk = source.Mk ?? string.Empty;
            target.Hoten = source.Hoten ?? string.Empty;
            target.Vitri = source.Pos ?? source.Vitri ?? string.Empty;
            target.Sdt = source.Sdt;
            target.Email = source.Email;
            target.Bank = source.Bank;
            target.Stk = source.Stk;
            target.Luong = source.Luong;
            target.Ngsinh = source.NgaySinh == DateTime.MinValue ? (DateTime?)null : source.NgaySinh;
        }

        private static DataTable ToTable(List<NhanVienDTO> employees)
        {
            var table = new DataTable();
            table.Columns.Add("Manv"); table.Columns.Add("Hoten"); table.Columns.Add("Sdt"); table.Columns.Add("Email");
            table.Columns.Add("Vitri"); table.Columns.Add("Luong", typeof(decimal)); table.Columns.Add("Tk"); table.Columns.Add("Mk");
            table.Columns.Add("Bank"); table.Columns.Add("Stk"); table.Columns.Add("Ngsinh", typeof(DateTime));
            foreach (var n in employees) table.Rows.Add(n.Mand, n.Hoten, n.Sdt, n.Email, n.Vitri, n.Luong, n.Tk, n.Mk, n.Bank, n.Stk, n.NgaySinh == DateTime.MinValue ? (object)DBNull.Value : n.NgaySinh);
            return table;
        }
    }
}
