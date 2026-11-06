using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAO
{
    public class Voucher1tang1DAO
    {
        private static Voucher1tang1DAO instance;
        public static Voucher1tang1DAO Instance => instance ??= new Voucher1tang1DAO();
        private Voucher1tang1DAO() { }

        public static string GetCode(int mavc)
        {
            using (var db = DatabaseContextFactory.Create()) return db.VOUCHER.Where(x => x.Mavc == mavc).Select(x => x.Code).FirstOrDefault() ?? string.Empty;
        }

        public bool CheckCodeExists(string code, int? excludeMavc = null)
        {
            using (var db = DatabaseContextFactory.Create()) return db.VOUCHER.Any(x => x.Code == code && (!excludeMavc.HasValue || x.Mavc != excludeMavc.Value));
        }

        public int InsertVoucher(string code, string tenMa, int loaiVC, int? maloai, decimal? dieuKien, System.DateTime ngaybd, System.DateTime ngaykt, decimal giaTri = 0m)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var voucher = new VOUCHER { Code = code, TenMaGiamGia = tenMa, Giatri = giaTri, Ngaybd = ngaybd, Ngaykt = ngaykt, DieuKien = dieuKien, Maloaivc = loaiVC, Maloai = maloai };
                db.VOUCHER.InsertOnSubmit(voucher); db.SubmitChanges(); return voucher.Mavc;
            }
        }

        public int GetVoucherId(string code)
        {
            using (var db = DatabaseContextFactory.Create()) return db.VOUCHER.Where(x => x.Code == code).Select(x => (int?)x.Mavc).FirstOrDefault() ?? -1;
        }

        public int InsertChiTietVC(int mavc, int masp)
        {
            // VOUCHER_SANPHAM is not present in the authoritative DBML, so no mapped entity can be inserted here.
            return -1;
        }

        public int GetMasp(string maSP)
        {
            if (string.IsNullOrWhiteSpace(maSP)) return -1;
            using (var db = DatabaseContextFactory.Create())
            {
                int id;
                if (int.TryParse(maSP, out id)) return db.SANPHAM.Any(x => x.Masp == id) ? id : -1;
                return db.SANPHAM.Where(x => x.Tensp == maSP).Select(x => (int?)x.Masp).FirstOrDefault() ?? -1;
            }
        }

        public DataRow GetVoucherByID(int mavc)
        {
            using (var db = DatabaseContextFactory.Create()) return ToTable(db.VOUCHER.Where(x => x.Mavc == mavc).ToList()).Rows.Cast<DataRow>().FirstOrDefault();
        }

        public DataTable GetSanPhamTangByVoucher(int mavc)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var category = db.VOUCHER.Where(x => x.Mavc == mavc).Select(x => x.Maloai).FirstOrDefault();
                return ProductTable(db.SANPHAM.Where(x => !category.HasValue || x.Maloai == category.Value).ToList());
            }
        }

        public DataTable SearchSanPhamTang(string keyword)
        {
            keyword = keyword ?? string.Empty;
            using (var db = DatabaseContextFactory.Create()) return ProductTable(db.SANPHAM.Where(x => x.Tensp.Contains(keyword)).ToList());
        }

        public bool UpdateVoucher(int mavc, string code, string tenMaGiamGia, int loaiVC, int? maloai, decimal? dieuKien, decimal giaTri = 0m, System.DateTime? ngaybd = null, System.DateTime? ngaykt = null)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var voucher = db.VOUCHER.FirstOrDefault(x => x.Mavc == mavc); if (voucher == null) return false;
                voucher.Code = code; voucher.TenMaGiamGia = tenMaGiamGia; voucher.Giatri = giaTri; voucher.Ngaybd = ngaybd ?? System.DateTime.Today; voucher.Ngaykt = ngaykt ?? System.DateTime.Today; voucher.DieuKien = dieuKien; voucher.Maloaivc = loaiVC; voucher.Maloai = maloai;
                db.SubmitChanges(); return true;
            }
        }

        public bool DeleteChiTietVC(int mavc) => true;

        public DataTable TimSanPhamTangTheoLoai(int maloai)
        {
            using (var db = DatabaseContextFactory.Create()) return ProductTable(db.SANPHAM.Where(x => x.Maloai == maloai).ToList());
        }

        private static DataTable ToTable(List<VOUCHER> vouchers)
        {
            var table = new DataTable(); table.Columns.Add("Mavc", typeof(int)); table.Columns.Add("Code"); table.Columns.Add("TenMaGiamGia"); table.Columns.Add("Giatri", typeof(decimal)); table.Columns.Add("Ngaybd", typeof(System.DateTime)); table.Columns.Add("Ngaykt", typeof(System.DateTime)); table.Columns.Add("DieuKien", typeof(decimal)); table.Columns.Add("Maloaivc", typeof(int)); table.Columns.Add("maloai", typeof(int));
            foreach (var x in vouchers) table.Rows.Add(x.Mavc, x.Code, x.TenMaGiamGia, x.Giatri, x.Ngaybd, x.Ngaykt, x.DieuKien ?? (object)DBNull.Value, x.Maloaivc, x.Maloai ?? (object)DBNull.Value);
            return table;
        }

        private static DataTable ProductTable(List<SANPHAM> products)
        {
            var table = new DataTable(); table.Columns.Add("Masp", typeof(int)); table.Columns.Add("MaSP"); table.Columns.Add("Tensp"); table.Columns.Add("tensp"); table.Columns.Add("Maloai", typeof(int)); table.Columns.Add("Giaban", typeof(decimal)); table.Columns.Add("Soluongton", typeof(int));
            foreach (var x in products) table.Rows.Add(x.Masp, x.Masp.ToString(), x.Tensp, x.Tensp, x.Maloai, x.Giaban, x.Soluongton);
            return table;
        }
    }
}