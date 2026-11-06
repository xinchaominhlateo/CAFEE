using DTO;
using System.Data;
using DTO;
using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace DAO
{
    public class BanHangDAO
    {
        private DataProvider provider = DataProvider.Instance;
        private static BanHangDAO instance;
        public static BanHangDAO Instance
        {
            get
            {
                if (instance == null)
                    instance = new BanHangDAO();
                return instance;
            }
        }

        public bool KiemTraSanPhamPhuHopTheoLoai(List<DanhSachSanPhamDTO> danhSach, int maloai)
        {
            foreach (var sp in danhSach)
            {
                if (sp.LaSanPhamTang) continue;

                if (sp.Maloai == maloai)
                    return true;
            }

            return false;
        }

        // Get giftable products by voucher (proc returns Masp-level rows)
        public DataTable GetSanPhamTangByVoucher(int mavc, int maloaiGoc, int loaiVC)
        {
            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@MaLoai", maloaiGoc),
                new SqlParameter("@LoaiVC", loaiVC)
            };

            return provider.ExecuteStoredProcedure("sp_GetSanPhamTangByVoucher", parameters.ToArray());
        }

        public DataTable GetSanPhamTangByVoucher(int mavc)
        {
            var parameters = new SqlParameter[] { new SqlParameter("@Mavc", mavc), new SqlParameter("@MaLoai", DBNull.Value), new SqlParameter("@LoaiVC", DBNull.Value) };
            return provider.ExecuteStoredProcedure("sp_GetSanPhamTangByVoucher", parameters);
        }

        // Apply voucher -> use stored proc sp_ApDungVoucher
        public void ApDungVoucher(int mavc, int mahd)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Mavc", mavc),
                new SqlParameter("@Mahd", mahd)
            };
            provider.ExecuteNonQueryStoredProcedure("sp_ApDungVoucher", parameters);
        }

        // Create invoice (sp_TaoHoaDon returns Mahd)
        public int TaoHoaDon(int? makh, string manv, decimal tongTienGoc, decimal tienGiam, decimal tongTienSauGiam)
        {
            SqlParameter[] parameters = {
                new SqlParameter("@Makh", (object)makh ?? DBNull.Value),
                new SqlParameter("@Manv", manv),
                new SqlParameter("@TongTienGoc", tongTienGoc),
                new SqlParameter("@TienGiam", tienGiam),
                new SqlParameter("@TongTien", tongTienSauGiam)
            };

            object result = provider.ExecuteScalarStoredProcedure("sp_TaoHoaDon", parameters);
            return result != null ? Convert.ToInt32(result) : -1;
        }

        // Add invoice detail - now pass Masp (product id) in BanHangDTO.Masp
        public void ThemChiTietHoaDon(int mahd, BanHangDTO sp)
        {
            if (sp == null) throw new ArgumentNullException(nameof(sp));
            if (sp.Masp <= 0 || sp.SoLuong <= 0 || sp.GiaBan < 0) throw new ArgumentException("Dữ liệu sản phẩm không hợp lệ.");

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Mahd", mahd),
                new SqlParameter("@Masp", sp.Masp),
                new SqlParameter("@Soluong", sp.SoLuong),
                new SqlParameter("@Dongia", sp.GiaBan),
                new SqlParameter("@IsTang", sp.LaSanPhamTang ? 1 : 0)
            };

            provider.ExecuteNonQueryStoredProcedure("sp_ThemChiTietHoaDon", parameters);
        }

        // Decrement stock by Masp (calls sp_TruTonKho)
        public void TruTonKho(int masp, int soLuong)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@Masp", masp),
                new SqlParameter("@SoLuong", soLuong)
            };

            provider.ExecuteNonQueryStoredProcedure("sp_TruTonKho", parameters);
        }

        // Load all products (uses sp_LayDanhSachSanPham_Admin)
        public List<BanHangDTO> LayTatCaSanPham()
        {
            List<BanHangDTO> list = new List<BanHangDTO>();

            SqlParameter[] parameters = {
                new SqlParameter("@TuKhoa", DBNull.Value),
                new SqlParameter("@MaLoai", 0)
            };

            DataTable dt = provider.ExecuteStoredProcedure("sp_LayDanhSachSanPham_Admin", parameters);
            foreach (DataRow row in dt.Rows)
            {
                int masp = row.Table.Columns.Contains("Masp") ? Convert.ToInt32(row["Masp"]) : 0;
                list.Add(new BanHangDTO
                {
                    Masp = masp,
                    MaSP = row["Masp"].ToString(),
                    TenSP = row["TenSP"].ToString(),
                    TenLoai = row["TenLoai"].ToString(),
                    GiaBan = row.Table.Columns.Contains("GiaBan") ? Convert.ToDecimal(row["GiaBan"]) : 0m,
                    DuongDanAnh = row.Table.Columns.Contains("DuongDanAnh") ? row["DuongDanAnh"].ToString() : string.Empty,
                    Maloai = row.Table.Columns.Contains("Maloai") ? Convert.ToInt32(row["Maloai"]) : 0,
                    SoLuongTon = row.Table.Columns.Contains("SoLuongTon") ? Convert.ToInt32(row["SoLuongTon"]) : 0,
                    TrangThaiText = row.Table.Columns.Contains("TrangThai") ? (Convert.ToInt32(row["TrangThai"]) == 1 ? "Đang bán" : "Ngừng bán") : string.Empty
                });
            }

            return list;
        }

        // Search uses same SP
        public List<BanHangDTO> TimKiemSanPham(string searchType, string keyword)
        {
            List<BanHangDTO> list = new List<BanHangDTO>();

            object tuKhoa = string.IsNullOrWhiteSpace(keyword) ? DBNull.Value : (object)keyword;
            object maLoai = 0;

            SqlParameter[] parameters = new SqlParameter[] { new SqlParameter("@TuKhoa", tuKhoa), new SqlParameter("@MaLoai", maLoai) };
            DataTable dt = provider.ExecuteStoredProcedure("sp_LayDanhSachSanPham_Admin", parameters);

            foreach (DataRow row in dt.Rows)
            {
                int masp = row.Table.Columns.Contains("Masp") ? Convert.ToInt32(row["Masp"]) : 0;
                list.Add(new BanHangDTO
                {
                    Masp = masp,
                    MaSP = row["Masp"].ToString(),
                    TenSP = row["TenSP"].ToString(),
                    TenLoai = row["TenLoai"].ToString(),
                    GiaBan = row.Table.Columns.Contains("GiaBan") ? Convert.ToDecimal(row["GiaBan"]) : 0m,
                    DuongDanAnh = row.Table.Columns.Contains("DuongDanAnh") ? row["DuongDanAnh"].ToString() : string.Empty,
                    Maloai = row.Table.Columns.Contains("Maloai") ? Convert.ToInt32(row["Maloai"]) : 0,
                    SoLuongTon = row.Table.Columns.Contains("SoLuongTon") ? Convert.ToInt32(row["SoLuongTon"]) : 0,
                    TrangThaiText = row.Table.Columns.Contains("TrangThai") ? (Convert.ToInt32(row["TrangThai"]) == 1 ? "Đang bán" : "Ngừng bán") : string.Empty
                });
            }

            return list;
        }
    }
}