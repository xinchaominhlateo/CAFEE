using DTO;
using System.Data;
using DTO;
using System.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAO
{
    public class DanhSachSanPhamDAO
    {
        private static DataProvider provider = DataProvider.Instance;
        private static DanhSachSanPhamDAO instance;
        public static DanhSachSanPhamDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new DanhSachSanPhamDAO();
                return instance;
            }
        }

        // Use stored procedure sp_LayDanhSachLoaiSP
        public DataTable GetLoaiSanPham()
        {
            return provider.ExecuteStoredProcedure("sp_LayDanhSachLoaiSP");
        }

        // Use stored procedure sp_LayDanhSachSanPham_Admin without parameters
        public List<DanhSachSanPhamDTO> GetAllSanPham()
        {
            DataTable data = provider.ExecuteStoredProcedure("sp_LayDanhSachSanPham_Admin");
            var list = new List<DanhSachSanPhamDTO>();

            foreach (DataRow row in data.Rows)
            {
                int masp = row.Table.Columns.Contains("Masp") ? Convert.ToInt32(row["Masp"]) : 0;
                list.Add(new DanhSachSanPhamDTO
                {
                    Masp = masp,
                    MaSP = masp.ToString(),
                    TenSP = row.Table.Columns.Contains("TenSP") ? row["TenSP"].ToString() : null,
                    TenLoai = row.Table.Columns.Contains("TenLoai") ? row["TenLoai"].ToString() : null,
                    GiaBan = row.Table.Columns.Contains("GiaBan") ? Convert.ToDecimal(row["GiaBan"]) : 0m,
                    SoLuongTon = row.Table.Columns.Contains("SoLuongTon") ? Convert.ToInt32(row["SoLuongTon"]) : 0,
                    TrangThaiText = row.Table.Columns.Contains("TrangThai") ? (Convert.ToInt32(row["TrangThai"]) == 1 ? "Đang bán" : "Ngừng bán") : null,
                    DuongDanAnh = row.Table.Columns.Contains("DuongDanAnh") ? row["DuongDanAnh"]?.ToString() : null,
                    Maloai = row.Table.Columns.Contains("Maloai") ? Convert.ToInt32(row["Maloai"]) : 0
                });
            }

            return list;
        }

        // Build product table for UI by calling the admin proc (keeps compatibility)
        public DataTable GetSanPhamTable()
        {
            var list = GetAllSanPham();
            DataTable dt = new DataTable();
            dt.Columns.Add("Mã sản phẩm");
            dt.Columns.Add("Tên sản phẩm");
            dt.Columns.Add("Loại");
            dt.Columns.Add("Size"); // kept for UI compatibility (will be empty)
            dt.Columns.Add("Giá bán", typeof(decimal));
            dt.Columns.Add("Số lượng", typeof(int));
            dt.Columns.Add("Trạng thái");

            foreach (var sp in list)
            {
                dt.Rows.Add(sp.MaSP, sp.TenSP, sp.TenLoai, string.Empty, sp.GiaBan, sp.SoLuongTon, sp.TrangThaiText);
            }

            return dt;
        }

        // Return DataTable of products (admin proc) — kept for codepaths that expect DataTable
        public DataTable GetSanPhamWithVoucher()
        {
            // There's no single proc returning the exact same shape as previous SQL.
            // Use the admin proc as base and let callers join/inspect vouchers if needed.
            return provider.ExecuteStoredProcedure("sp_LayDanhSachSanPham_Admin");
        }

        // Use stored procedure sp_LayDanhSachSanPham_Admin to search (pass keyword)
        public List<DanhSachSanPhamDTO> SearchSanPham(string searchType, string searchTerm)
        {
            // The stored procedure supports @TuKhoa and @MaLoai.
            // We'll set @TuKhoa for MaSP/TenSP searches and also for general search.
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TuKhoa", string.IsNullOrWhiteSpace(searchTerm) ? (object)DBNull.Value : searchTerm),
                new SqlParameter("@MaLoai", 0)
            };

            // If caller asked to search by LoaiSP (text), we can't map name->id reliably here.
            // Keep simple: pass searchTerm to @TuKhoa (stored proc filters Tensp and Masp string).
            DataTable data = provider.ExecuteStoredProcedure("sp_LayDanhSachSanPham_Admin", parameters);
            var list = new List<DanhSachSanPhamDTO>();

            foreach (DataRow row in data.Rows)
            {
                int masp = row.Table.Columns.Contains("Masp") ? Convert.ToInt32(row["Masp"]) : 0;
                list.Add(new DanhSachSanPhamDTO
                {
                    Masp = masp,
                    MaSP = masp.ToString(),
                    TenSP = row.Table.Columns.Contains("TenSP") ? row["TenSP"].ToString() : null,
                    Maloai = row.Table.Columns.Contains("Maloai") ? Convert.ToInt32(row["Maloai"]) : 0,
                    TenLoai = row.Table.Columns.Contains("TenLoai") ? row["TenLoai"].ToString() : null,
                    GiaBan = row.Table.Columns.Contains("GiaBan") ? Convert.ToDecimal(row["GiaBan"]) : 0m,
                    SoLuongTon = row.Table.Columns.Contains("SoLuongTon") ? Convert.ToInt32(row["SoLuongTon"]) : 0,
                    TrangThaiText = row.Table.Columns.Contains("TrangThai") ? (Convert.ToInt32(row["TrangThai"]) == 1 ? "Đang bán" : "Ngừng bán") : null,
                    DuongDanAnh = row.Table.Columns.Contains("DuongDanAnh") ? row["DuongDanAnh"]?.ToString() : null
                });
            }

            return list;
        }

        // Toggle product status via stored procedure sp_DoiTrangThaiSP
        public bool ToggleTrangThaiSanPham(int masp)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSP", masp)
            };
            int res = provider.ExecuteNonQueryStoredProcedure("sp_DoiTrangThaiSP", parameters);
            return res > 0;
        }

        // Get products mapped to a voucher through stored proc sp_GetChiTietVoucher (returns Masp)
        public List<int> GetChiTietVoucher(int mavc)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Mavc", mavc)
            };

            DataTable dt = provider.ExecuteStoredProcedure("sp_GetChiTietVoucher", parameters);
            var result = new List<int>();
            foreach (DataRow row in dt.Rows)
            {
                if (row.Table.Columns.Contains("Masp"))
                    result.Add(Convert.ToInt32(row["Masp"]));
            }
            return result;
        }

        // Delete product by Masp using sp_XoaCungSanPham
        public bool DeleteSanPham(int masp)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSP", masp)
            };
            int res = provider.ExecuteNonQueryStoredProcedure("sp_XoaCungSanPham", parameters);
            return res > 0;
        }

        // Legacy/compat: sizes removed — return 0
        public int CountKichCoSP(string maSP)
        {
            // sizes were removed. keep returning 0 for compatibility
            return 0;
        }

        public bool DeleteSanPhamGoc(string maSP)
        {
            // attempt to resolve Masp from maSP then delete
            int masp = GetMasp(maSP);
            if (masp == 0) return false;
            return DeleteSanPham(masp);
        }

        // Get product info via sp_LayThongTinSanPham_ById
        public SanPhamDTO GetSanPhamTheoMaVaKichCo(string maSP, string kichco)
        {
            if (string.IsNullOrWhiteSpace(maSP)) return null;

            int maspInt = 0;
            if (!int.TryParse(maSP, out maspInt))
            {
                maspInt = GetMasp(maSP);
            }
            if (maspInt == 0) return null;

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSP", maspInt)
            };

            DataTable dt = provider.ExecuteStoredProcedure("sp_LayThongTinSanPham_ById", parameters);
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            return new SanPhamDTO
            {
                Masp = Convert.ToInt32(row["Masp"]),
                MaSP = row["Masp"].ToString(),
                TenSP = row["TenSP"].ToString(),
                MaLoai = row.Table.Columns.Contains("Maloai") ? Convert.ToInt32(row["Maloai"]) : 0,
                SoLuongTon = row.Table.Columns.Contains("SoLuongTon") ? Convert.ToInt32(row["SoLuongTon"]) : 0,
                TrangThaiText = row.Table.Columns.Contains("TrangThai") ? (Convert.ToInt32(row["TrangThai"]) == 1 ? "Đang bán" : "Ngừng bán") : null,
                DuongDanAnh = row.Table.Columns.Contains("DuongDanAnh") ? row["DuongDanAnh"]?.ToString() : null,
                GiaBan = row.Table.Columns.Contains("GiaBan") ? Convert.ToDecimal(row["GiaBan"]) : 0m,
                CanhBaoTonKho = row.Table.Columns.Contains("canhbaotonkho") ? Convert.ToInt32(row["canhbaotonkho"]) : (row.Table.Columns.Contains("CanhBaoTon") ? Convert.ToInt32(row["CanhBaoTon"]) : 0)
            };
        }

        // Resolve Masp from a MaSP string or product name via stored procedure sp_GetMaspFromMaSP
        public int GetMasp(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP)) return 0;

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSP", maSP)
            };

            DataTable dt = provider.ExecuteStoredProcedure("sp_GetMaspFromMaSP", parameters);
            if (dt.Rows.Count == 0) return 0;
            if (dt.Columns.Contains("Masp") && int.TryParse(dt.Rows[0]["Masp"].ToString(), out int masp))
                return masp;
            return 0;
        }

        // ---------------------------
        // New: product create/update (moved from ThemSanPhamDAO / SuaSanPhamDAO)
        // Use stored procedures: sp_ThemSanPham_Moi and sp_SuaSanPham_Moi
        // ---------------------------

        public bool InsertSanPham(SanPhamDTO sp)
        {
            if (sp == null) return false;

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@TenSP", sp.TenSP ?? string.Empty),
                new SqlParameter("@MaLoai", sp.MaLoai),
                new SqlParameter("@GiaBan", sp.GiaBan),
                // use CanhBaoTonKho if provided
                new SqlParameter("@CanhBao", sp.CanhBaoTonKho),
                new SqlParameter("@DuongDanAnh", sp.DuongDanAnh ?? string.Empty)
            };

            int res = provider.ExecuteNonQueryStoredProcedure("sp_ThemSanPham_Moi", parameters);
            return res > 0;
        }

        public bool UpdateSanPham(SanPhamDTO sp)
        {
            if (sp == null) return false;

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSP", sp.Masp),
                new SqlParameter("@TenSP", sp.TenSP ?? string.Empty),
                new SqlParameter("@MaLoai", sp.MaLoai),
                new SqlParameter("@GiaBan", sp.GiaBan),
                new SqlParameter("@CanhBao", sp.CanhBaoTonKho),
                new SqlParameter("@DuongDanAnh", sp.DuongDanAnh ?? string.Empty)
            };

            int res = provider.ExecuteNonQueryStoredProcedure("sp_SuaSanPham_Moi", parameters);
            return res > 0;
        }
    }
}