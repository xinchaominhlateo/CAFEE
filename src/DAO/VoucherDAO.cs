using DTO;
using System.Data;
using DTO;
using System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAO
{
    public class VoucherDAO
    {
        private static VoucherDAO instance;
        public static VoucherDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new VoucherDAO();
                return instance;
            }
        }

        private DataProvider provider = DataProvider.Instance;

        public static string GetCode(int mavc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Mavc", mavc) };
            DataTable dt = DataProvider.Instance.ExecuteStoredProcedure("sp_GetVoucherById", parameters);
            if (dt.Rows.Count == 0) return "";
            return dt.Rows[0]["Code"]?.ToString() ?? "";
        }

        public static int? GetIdFromCode(string code)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Code", code ?? string.Empty)
            };
            object result = DataProvider.Instance.ExecuteScalarStoredProcedure("sp_GetIdFromCode", parameters);
            return result != null ? (int?)Convert.ToInt32(result) : null;
        }

        public bool AddVoucher(VoucherDTO voucher)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Code", voucher.Code),
                new SqlParameter("@TenMaGiamGia", (object)voucher.TenMaGiamGia ?? DBNull.Value),
                new SqlParameter("@Giatri", voucher.Giatri),
                new SqlParameter("@Ngaybd", voucher.Ngaybd),
                new SqlParameter("@Ngaykt", voucher.Ngaykt),
                new SqlParameter("@DieuKien", (object)voucher.DieuKien ?? DBNull.Value),
                new SqlParameter("@Maloaivc", voucher.Maloaivc),
                new SqlParameter("@Maloai", (object)voucher.Maloai ?? DBNull.Value)
            };

            object result = provider.ExecuteScalarStoredProcedure("sp_InsertVoucher", parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool AddVoucherChiTiet(int mavc, int masp)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@Masp", masp)
            };
            object result = provider.ExecuteScalarStoredProcedure("sp_InsertChiTietVC", parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool CheckChiTietVoucher(int mavc, int masp)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@Masp", masp)
            };
            object result = provider.ExecuteScalarStoredProcedure("sp_CheckChiTietVC", parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        public VoucherDTO GetVoucherByID(int mavc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Mavc", mavc) };
            DataTable dt = provider.ExecuteStoredProcedure("sp_GetVoucherById", parameters);
            if (dt.Rows.Count == 0) return null;

            DataRow r = dt.Rows[0];
            return new VoucherDTO
            {
                Mavc = (int)r["Mavc"],
                Code = r["Code"].ToString(),
                TenMaGiamGia = r.IsNull("TenMaGiamGia") ? null : r["TenMaGiamGia"].ToString(),
                Giatri = (decimal)r["Giatri"],
                Ngaybd = (DateTime)r["Ngaybd"],
                Ngaykt = (DateTime)r["Ngaykt"],
                DieuKien = r["DieuKien"] != DBNull.Value ? (decimal?)r["DieuKien"] : null,
                Maloaivc = (int)r["Maloaivc"],
                Maloai = r["maloai"] != DBNull.Value ? (int?)r["maloai"] : null
            };
        }

        public bool UpdateVoucher(VoucherDTO voucher)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Mavc", voucher.Mavc),
                new SqlParameter("@Code", voucher.Code),
                new SqlParameter("@TenMaGiamGia", (object)voucher.TenMaGiamGia ?? DBNull.Value),
                new SqlParameter("@Giatri", voucher.Giatri),
                new SqlParameter("@Ngaybd", voucher.Ngaybd),
                new SqlParameter("@Ngaykt", voucher.Ngaykt),
                new SqlParameter("@DieuKien", (object)voucher.DieuKien ?? DBNull.Value),
                new SqlParameter("@Maloaivc", voucher.Maloaivc),
                new SqlParameter("@Maloai", (object)voucher.Maloai ?? DBNull.Value)
            };

            object res = provider.ExecuteScalarStoredProcedure("sp_UpdateVoucher", parameters);
            return res != null && Convert.ToInt32(res) > 0;
        }

        public bool DeleteVoucher(int mavc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Mavc", mavc) };
            object res = provider.ExecuteScalarStoredProcedure("sp_DeleteVoucher", parameters);
            return res != null && Convert.ToInt32(res) > 0;
        }

        public DataTable GetAllVouchers()
        {
            return provider.ExecuteStoredProcedure("sp_GetAllVouchers", null);
        }

        public DataTable GetAllVouchersWithJoin()
        {
            return provider.ExecuteStoredProcedure("sp_GetAllVouchersWithJoin", null);
        }

        public DataTable GetVouchersByType(int maloaivc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Maloaivc", maloaivc) };
            return provider.ExecuteStoredProcedure("sp_GetVouchersByType", parameters);
        }

        public DataTable GetVouchersByTypeWithJoin(int maloaivc)
        {
            SqlParameter[] parameters = { new SqlParameter("@Maloaivc", maloaivc) };
            return provider.ExecuteStoredProcedure("sp_GetVouchersByTypeWithJoin", parameters);
        }

        public DataTable GetVouchersByDateRange(DateTime from, DateTime to)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@FromDate", from),
                new SqlParameter("@ToDate", to)
            };
            return provider.ExecuteStoredProcedure("sp_GetVouchersByDateRange", parameters);
        }

        public DataTable GetVoucherTypes()
        {
            return provider.ExecuteStoredProcedure("sp_GetVoucherTypes", null);
        }

        public int AddVoucherAndReturnID(VoucherDTO voucher)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Code", voucher.Code),
                new SqlParameter("@TenMaGiamGia", (object)voucher.TenMaGiamGia ?? DBNull.Value),
                new SqlParameter("@Giatri", voucher.Giatri),
                new SqlParameter("@Ngaybd", voucher.Ngaybd),
                new SqlParameter("@Ngaykt", voucher.Ngaykt),
                new SqlParameter("@DieuKien", (object)voucher.DieuKien ?? DBNull.Value),
                new SqlParameter("@Maloaivc", voucher.Maloaivc),
                new SqlParameter("@Maloai", (object)voucher.Maloai ?? DBNull.Value)
            };

            object result = provider.ExecuteScalarStoredProcedure("sp_InsertVoucher", parameters);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        public int UpdateVoucherAndReturnAffectedRows(VoucherDTO voucher)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", voucher.Mavc),
                new SqlParameter("@Code", voucher.Code),
                new SqlParameter("@TenMaGiamGia", (object)voucher.TenMaGiamGia ?? DBNull.Value),
                new SqlParameter("@Giatri", voucher.Giatri),
                new SqlParameter("@Ngaybd", voucher.Ngaybd),
                new SqlParameter("@Ngaykt", voucher.Ngaykt),
                new SqlParameter("@DieuKien", (object)voucher.DieuKien ?? DBNull.Value),
                new SqlParameter("@Maloaivc", voucher.Maloaivc),
                new SqlParameter("@Maloai", (object)voucher.Maloai ?? DBNull.Value)
            };

            object res = provider.ExecuteScalarStoredProcedure("sp_UpdateVoucher", parameters);
            return res != null ? Convert.ToInt32(res) : 0;
        }

        public bool UpdateVoucherChiTiet(int mavc, List<int> maspList)
        {
            SqlParameter[] delParams = { new SqlParameter("@Mavc", mavc) };
            provider.ExecuteNonQueryStoredProcedure("sp_DeleteChiTietVC", delParams);

            foreach (int masp in maspList)
            {
                AddVoucherChiTiet(mavc, masp);
            }

            return true;
        }
    }
}