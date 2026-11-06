using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using System.Data.SqlClient;
using System.Data;
using System.Windows.Forms;

namespace DAO
{
    public class NhanVienDAO
    {
        public static List<NhanVienDTO> layDSNhanVien()
        {
            List<NhanVienDTO> dsNV = new List<NhanVienDTO>();
            try
            {
                string spName = "sp_LayDanhSachNhanVien"; // create this proc in DB if not exists
                DataTable data = DataProvider.Instance.ExecuteStoredProcedure(spName);

                if (data != null && data.Rows.Count >0)
                {
                    foreach (DataRow row in data.Rows)
                    {
                        NhanVienDTO nv = new NhanVienDTO(row);

                        dsNV.Add(nv);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi lấy danh sách nhân viên (List): " + ex.Message);
            }
            return dsNV;
        }

        public static void XoaNV(string maNV)
        {
            string spName = "sp_XoaNhanVien"; // should delete by Manv
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Manv", maNV)
            };
            DataProvider.Instance.ExecuteNonQueryStoredProcedure(spName, parameters);
        }

        public static void SuaNV(NhanVienDTO nv)
        {
            string spName = "sp_SuaNhanVien"; // update proc

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Manv", nv.Mand),
                new SqlParameter("@Hoten", nv.Hoten),
                new SqlParameter("@Sdt", nv.Sdt ?? (object)DBNull.Value),
                new SqlParameter("@Email", nv.Email ?? (object)DBNull.Value),
                new SqlParameter("@Vitri", nv.Pos ?? (object)DBNull.Value),
                new SqlParameter("@Luong", nv.Luong),
                new SqlParameter("@Tk", nv.Tk ?? (object)DBNull.Value),
                new SqlParameter("@Mk", nv.Mk ?? (object)DBNull.Value),
                new SqlParameter("@Bank", nv.Bank ?? (object)DBNull.Value),
                new SqlParameter("@Stk", nv.Stk ?? (object)DBNull.Value),
                new SqlParameter("@Ngsinh", nv.NgaySinh == DateTime.MinValue ? (object)DBNull.Value : nv.NgaySinh)
            };
            DataProvider.Instance.ExecuteNonQueryStoredProcedure(spName, parameters);
        }

        public static DataTable LayDanhSachNhanVien()
        {
            string spName = "sp_LayDanhSachNhanVienForGrid"; // proc for grid
            return DataProvider.Instance.ExecuteStoredProcedure(spName);
        }

        public static DataTable TimKiemNhanVien(string keyword)
        {
            string spName = "sp_TimKiemNhanVien";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TuKhoa", keyword)
            };
            try
            {
                return DataProvider.Instance.ExecuteStoredProcedure(spName, parameters);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi tìm kiếm nhân viên: " + ex.Message);
                return null;
            }
        }

        public static List<NhanVienDTO> LayNhanVienTheoGio()
        {
            List<NhanVienDTO> dsNV = new List<NhanVienDTO>();
            try
            {
                string spName = "sp_LayNhanVienTheoGio";
                DataTable data = DataProvider.Instance.ExecuteStoredProcedure(spName);
                foreach (DataRow row in data.Rows)
                {
                    NhanVienDTO nv = new NhanVienDTO(row);
                    dsNV.Add(nv);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Đã xảy ra lỗi khi lấy danh sách nhân viên theo giờ: " + ex.Message);
            }
            return dsNV;
        }

        public static void themNV(NhanVienDTO hs)
        {
            string spName = "sp_ThemNhanVien";

            SqlParameter[] parameters = new SqlParameter[11];
            parameters[0] = new SqlParameter("@Manv", hs.Mand);
            parameters[1] = new SqlParameter("@Hoten", hs.Hoten);
            parameters[2] = new SqlParameter("@Sdt", hs.Sdt ?? (object)DBNull.Value);
            parameters[3] = new SqlParameter("@Email", hs.Email ?? (object)DBNull.Value);
            parameters[4] = new SqlParameter("@Vitri", hs.Pos ?? (object)DBNull.Value);
            parameters[5] = new SqlParameter("@Luong", hs.Luong);
            parameters[6] = new SqlParameter("@Tk", hs.Tk ?? (object)DBNull.Value);
            parameters[7] = new SqlParameter("@Mk", hs.Mk ?? (object)DBNull.Value);
            parameters[8] = new SqlParameter("@Bank", hs.Bank ?? (object)DBNull.Value);
            parameters[9] = new SqlParameter("@Stk", hs.Stk ?? (object)DBNull.Value);
            parameters[10] = new SqlParameter("@Ngsinh", hs.NgaySinh == DateTime.MinValue ? (object)DBNull.Value : hs.NgaySinh);

            DataProvider.Instance.ExecuteNonQueryStoredProcedure(spName, parameters);
        }

        public static NhanVienDTO LayNhanVienTheoID(string id)
        {
            string spName = "sp_LayNhanVienTheoID";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Manv", id)
            };
            DataTable data = DataProvider.Instance.ExecuteStoredProcedure(spName, parameters);
            if (data.Rows.Count >0)
            {
                return new NhanVienDTO(data.Rows[0]);
            }
            return null;
        }
        public static string LayTenNhanVien(string mand)
        {
            string spName = "sp_LayTenNhanVien";
            SqlParameter[] parameters = { new SqlParameter("@Manv", mand) };
            DataTable dt = DataProvider.Instance.ExecuteStoredProcedure(spName, parameters);

            if (dt.Rows.Count >0)
            {
                return dt.Rows[0]["Hoten"].ToString();
            }

            return "Không xác định";
        }
        // thêm nhân viên
        public static bool KiemTraTonTaiMaNV(string maNV)
        {
            string spName = "sp_KiemTraTonTaiMaNV";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Manv", maNV)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            if (result != null && Convert.ToInt32(result) >0)
            {
                return true;
            }
            return false;
        }
        public static bool KiemTraTonTaiTenTaiKhoan(string tenTaiKhoan)
        {
            string spName = "sp_KiemTraTonTaiTk";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Tk", tenTaiKhoan)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            if (result != null && Convert.ToInt32(result) >0)
            {
                return true;
            }
            return false;
        }
        public static bool KiemTraTonTaiSdt(string sdt)
        {
            string spName = "sp_KiemTraTonTaiSdt";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Sdt", sdt)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            return result != null && Convert.ToInt32(result) >0;
        }
        public static bool KiemTraTonTaiEmail(string email)
        {
            string spName = "sp_KiemTraTonTaiEmail";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            return result != null && Convert.ToInt32(result) >0;
        }
        // sửa nhân viên
        public static bool KiemTraTonTaiTenTaiKhoanKhac(string tenTaiKhoan, string mandCanLoaiTru)
        {
            string spName = "sp_KiemTraTonTaiTkKhac";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Tk", tenTaiKhoan),
                new SqlParameter("@ManvCanLoaiTru", mandCanLoaiTru)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            return result != null && Convert.ToInt32(result) >0;
        }
        public static bool KiemTraTonTaiSdtKhac(string sdt, string mandCanLoaiTru)
        {
            string spName = "sp_KiemTraTonTaiSdtKhac";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Sdt", sdt),
                new SqlParameter("@ManvCanLoaiTru", mandCanLoaiTru)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            return result != null && Convert.ToInt32(result) >0;
        }

        public static bool KiemTraTonTaiEmailKhac(string email, string mandCanLoaiTru)
        {
            string spName = "sp_KiemTraTonTaiEmailKhac";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Email", email),
                new SqlParameter("@ManvCanLoaiTru", mandCanLoaiTru)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure(spName, parameters);

            return result != null && Convert.ToInt32(result) >0;
        }
    }
}
