using System.Data;
using System.Data;
using DTO;
using System;
using System.Data.SqlClient;

namespace DAO
{
    public class Voucher1tang1DAO
    {
        private static Voucher1tang1DAO instance;
        public static Voucher1tang1DAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new Voucher1tang1DAO();
                return instance;
            }
        }

        private DataProvider provider = DataProvider.Instance;

        private Voucher1tang1DAO() { }

        public static string GetCode(int mavc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Mavc", mavc) };
            DataTable dt = DataProvider.Instance.ExecuteStoredProcedure("sp_GetVoucherById", parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0]["Code"].ToString() : "";
        }

        public bool CheckCodeExists(string code, int? excludeMavc = null)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Code", code),
                new SqlParameter("@ExcludeMavc", (object)excludeMavc ?? DBNull.Value)
            };

            object result = provider.ExecuteScalarStoredProcedure("sp_CheckVoucherCodeExists", parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // Insert voucher — return inserted id (sp_InsertVoucher uses OUTPUT INSERTED.Mavc)
        public int InsertVoucher(string code, string tenMa, int loaiVC, int? maloai, decimal? dieuKien, DateTime ngaybd, DateTime ngaykt, decimal giaTri = 0m)
        {
            // Use the single parameter name the proc defines: @Giatri
            SqlParameter[] parameters = {
                new SqlParameter("@Code", code),
                new SqlParameter("@TenMaGiamGia", (object)tenMa ?? DBNull.Value),
                new SqlParameter("@Giatri", giaTri),
                new SqlParameter("@Ngaybd", ngaybd),
                new SqlParameter("@Ngaykt", ngaykt),
                new SqlParameter("@DieuKien", (object)dieuKien ?? DBNull.Value),
                new SqlParameter("@Maloaivc", loaiVC),
                new SqlParameter("@Maloai", (object)maloai ?? DBNull.Value)
            };

            object result = provider.ExecuteScalarStoredProcedure("sp_InsertVoucher", parameters);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public int GetVoucherId(string code)
        {
            SqlParameter[] parameters = { new SqlParameter("@Code", code) };
            object result = provider.ExecuteScalarStoredProcedure("sp_GetIdFromCode", parameters);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public int InsertChiTietVC(int mavc, int masp)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@Masp", masp)
            };
            object result = provider.ExecuteScalarStoredProcedure("sp_InsertChiTietVC", parameters);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public int GetMasp(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP)) return -1;

            SqlParameter[] parameters = { new SqlParameter("@MaSP", maSP) };
            DataTable dt = provider.ExecuteStoredProcedure("sp_GetMaspFromMaSP", parameters);
            if (dt.Rows.Count == 0) return -1;
            if (dt.Columns.Contains("Masp") && int.TryParse(dt.Rows[0]["Masp"].ToString(), out int masp))
                return masp;
            return -1;
        }

        public DataRow GetVoucherByID(int mavc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Mavc", mavc) };
            DataTable dt = provider.ExecuteStoredProcedure("sp_GetVoucherById", parameters);
            return dt.Rows.Count > 0 ? dt.Rows[0] : null;
        }

        public DataTable GetSanPhamTangByVoucher(int mavc)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@MaLoai", DBNull.Value),
                new SqlParameter("@LoaiVC", DBNull.Value)
            };
            return provider.ExecuteStoredProcedure("sp_GetSanPhamTangByVoucher", parameters);
        }

        public DataTable SearchSanPhamTang(string keyword)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Keyword", keyword ?? string.Empty)
            };
            return provider.ExecuteStoredProcedure("sp_SearchSanPhamTang", parameters);
        }

        public bool UpdateVoucher(int mavc, string code, string tenMaGiamGia, int loaiVC, int? maloai, decimal? dieuKien, decimal giaTri = 0m, DateTime? ngaybd = null, DateTime? ngaykt = null)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@Code", code),
                new SqlParameter("@TenMaGiamGia", (object)tenMaGiamGia ?? DBNull.Value),
                new SqlParameter("@Giatri", giaTri),
                new SqlParameter("@Ngaybd", (object)ngaybd ?? DateTime.Today),
                new SqlParameter("@Ngaykt", (object)ngaykt ?? DateTime.Today),
                new SqlParameter("@DieuKien", (object)dieuKien ?? DBNull.Value),
                new SqlParameter("@Maloaivc", loaiVC),
                new SqlParameter("@Maloai", (object)maloai ?? DBNull.Value)
            };

            object res = provider.ExecuteScalarStoredProcedure("sp_UpdateVoucher", parameters);
            return res != null && Convert.ToInt32(res) > 0;
        }

        public bool DeleteChiTietVC(int mavc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Mavc", mavc) };
            provider.ExecuteNonQueryStoredProcedure("sp_DeleteChiTietVC", parameters);
            return true;
        }

        public DataTable TimSanPhamTangTheoLoai(int maloai)
        {
            SqlParameter[] parameters = { new SqlParameter("@Maloai", maloai) };
            return provider.ExecuteStoredProcedure("sp_TimSanPhamTangTheoLoai", parameters);
        }
    }
}