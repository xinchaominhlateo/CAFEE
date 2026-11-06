using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using System.Data.SqlClient;
namespace DAO
{
    public class KhachHangDAO
    {
        private static DataProvider provider = DataProvider.Instance;

        private static KhachHangDAO instance;
        public static KhachHangDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new KhachHangDAO();
                return instance;
            }
        }

        public KhachHangDAO() { }
        public static List<KhachHangDTO> layDSKH()
        {
            List<KhachHangDTO> dsKH = new List<KhachHangDTO>();
            string query = "SELECT * FROM KHACHHANG";
            DataTable data = DataProvider.Instance.ExecuteQuery(query);
            foreach (DataRow row in data.Rows)
            {
                KhachHangDTO kh = new KhachHangDTO(row);
                dsKH.Add(kh);
            }
            return dsKH;
        }
        public static string GetTenKhachHang(int maKH)
        {
            string query = "SELECT tenkh FROM KHACHHANG WHERE makh = @makh";
            SqlParameter[] parameters = {
            new SqlParameter("@makh", maKH)
            };

            object result = DataProvider.Instance.ExecuteScalar(query, parameters);
            return result?.ToString() ?? "Khách lẻ";
        }

        public static string GetSDTKhachHang(int maKH)
        {
            string query = "SELECT sdt FROM KHACHHANG WHERE makh = @makh";
            SqlParameter[] parameters = {
            new SqlParameter("@makh", maKH)
            };

            object result = DataProvider.Instance.ExecuteScalar(query, parameters);
            return result?.ToString() ?? "";
        }
        public static string LayTenKhachHangTheoSDT(string sdt)
        {
            string query = "SELECT Tenkh FROM KHACHHANG WHERE sdt = @sdt";
            SqlParameter[] parameters = {
            new SqlParameter("@sdt", sdt)
            };

            DataTable dt = provider.ExecuteQuery(query, parameters);
            if (dt.Rows.Count > 0)
            {
                return dt.Rows[0]["Tenkh"].ToString();
            }
            return "Khách lẻ";
        }
        public static void themKH(KhachHangDTO kh)
        {
            string sql = "INSERT INTO KHACHHANG (Tenkh, Sdt, Tichdiem) VALUES (@Tenkh, @Sdt, @Tichdiem)";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Tenkh", kh.Tenkh),
                new SqlParameter("@Sdt", kh.Sdt),
                new SqlParameter("@Tichdiem", kh.Tichdiem)
            };
            DataProvider.Instance.ExecuteNonQuery(sql, parameters);
        }
        public static void xoaKH(int makh)
        {
            string sql = "DELETE FROM KHACHHANG WHERE Makh = @Makh";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Makh", makh)
            };
            DataProvider.Instance.ExecuteNonQuery(sql, parameters);
        }

        public static void suaKH(KhachHangDTO kh)
        {
            string sql = "UPDATE KHACHHANG SET Tenkh = @Tenkh, Sdt = @Sdt, Tichdiem = @Tichdiem WHERE Makh = @Makh";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Makh", kh.Makh),
                new SqlParameter("@Tenkh", kh.Tenkh),
                new SqlParameter("@Sdt", kh.Sdt),
                new SqlParameter("@Tichdiem", kh.Tichdiem)
            };
            DataProvider.Instance.ExecuteNonQuery(sql, parameters);
        }
        public static List<KhachHangDTO> timTheoTenHoacSDT(string keyword)
        {
            List<KhachHangDTO> dsKH = new List<KhachHangDTO>();
            string sql = "SELECT * FROM KHACHHANG WHERE Tenkh LIKE @Keyword OR Sdt LIKE @Keyword";
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Keyword", "%" + keyword + "%")
            };
            DataTable data = DataProvider.Instance.ExecuteQuery(sql, parameters);
            foreach (DataRow row in data.Rows)
            {
                KhachHangDTO kh = new KhachHangDTO(row);
                dsKH.Add(kh);
            }
            return dsKH;
        }
        public List<KhachHangDTO> TimKiemTheoSDT(string sdt)
        {
            string query = "SELECT makh, tenkh, sdt FROM KHACHHANG WHERE sdt LIKE @sdt";
            SqlParameter[] param = { new SqlParameter("@sdt", "%" + sdt + "%") };
            DataTable dt = provider.ExecuteQuery(query, param);

            List<KhachHangDTO> list = new List<KhachHangDTO>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new KhachHangDTO
                {
                    Makh = Convert.ToInt32(row["makh"]),
                    Tenkh = row["tenkh"].ToString(),
                    Sdt = row["sdt"].ToString()
                });
            }
            return list;
        }
    }
}
