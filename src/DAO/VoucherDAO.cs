using DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DAO
{
    public class VoucherDAO
    {
        private static VoucherDAO instance;
        public static VoucherDAO Instance => instance ??= new VoucherDAO();

        public static string GetCode(int mavc)
        {
            using (var db = DatabaseContextFactory.Create()) return db.VOUCHER.Where(x => x.Mavc == mavc).Select(x => x.Code).FirstOrDefault() ?? string.Empty;
        }

        public static int? GetIdFromCode(string code)
        {
            using (var db = DatabaseContextFactory.Create()) return db.VOUCHER.Where(x => x.Code == code).Select(x => (int?)x.Mavc).FirstOrDefault();
        }

        public bool AddVoucher(VoucherDTO voucher) => AddVoucherAndReturnID(voucher) > 0;

        public bool AddVoucherChiTiet(int mavc, int masp)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var voucher = db.VOUCHER.FirstOrDefault(x => x.Mavc == mavc);
                var product = db.SANPHAM.FirstOrDefault(x => x.Masp == masp);
                return voucher != null && product != null && voucher.Maloai.HasValue && voucher.Maloai.Value == product.Maloai;
            }
        }

        public bool CheckChiTietVoucher(int mavc, int masp)
        {
            return AddVoucherChiTiet(mavc, masp);
        }

        public VoucherDTO GetVoucherByID(int mavc)
        {
            using (var db = DatabaseContextFactory.Create()) return db.VOUCHER.Where(x => x.Mavc == mavc).Select(ToDto).FirstOrDefault();
        }

        public bool UpdateVoucher(VoucherDTO voucher) => UpdateVoucherAndReturnAffectedRows(voucher) > 0;

        public bool DeleteVoucher(int mavc)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var voucher = db.VOUCHER.FirstOrDefault(x => x.Mavc == mavc);
                if (voucher == null) return false;
                db.VOUCHER.DeleteOnSubmit(voucher);
                db.SubmitChanges();
                return true;
            }
        }

        public DataTable GetAllVouchers() { using (var db = DatabaseContextFactory.Create()) return ToTable(db.VOUCHER.OrderBy(x => x.Mavc).Select(ToDto).ToList()); }
        public DataTable GetAllVouchersWithJoin() => GetJoinedVouchers(null);
        public DataTable GetVouchersByType(int maloaivc) { using (var db = DatabaseContextFactory.Create()) return ToTable(db.VOUCHER.Where(x => x.Maloaivc == maloaivc).Select(ToDto).ToList()); }
        public DataTable GetVouchersByTypeWithJoin(int maloaivc) => GetJoinedVouchers(maloaivc);

        public DataTable GetVouchersByDateRange(DateTime from, DateTime to)
        {
            using (var db = DatabaseContextFactory.Create()) return ToTable(db.VOUCHER.Where(x => x.Ngaybd <= to && x.Ngaykt >= from).Select(ToDto).ToList());
        }

        public DataTable GetVoucherTypes()
        {
            var table = new DataTable(); table.Columns.Add("Maloaivc", typeof(int)); table.Columns.Add("Tenloai");
            using (var db = DatabaseContextFactory.Create()) foreach (var type in db.KIEUVC.OrderBy(x => x.Maloaivc)) table.Rows.Add(type.Maloaivc, type.Tenloai);
            return table;
        }

        public int AddVoucherAndReturnID(VoucherDTO voucher)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var entity = new VOUCHER { Code = voucher.Code, TenMaGiamGia = voucher.TenMaGiamGia, Giatri = voucher.Giatri, Ngaybd = voucher.Ngaybd, Ngaykt = voucher.Ngaykt, DieuKien = voucher.DieuKien, Maloaivc = voucher.Maloaivc, Maloai = voucher.Maloai };
                db.VOUCHER.InsertOnSubmit(entity); db.SubmitChanges(); return entity.Mavc;
            }
        }

        public int UpdateVoucherAndReturnAffectedRows(VoucherDTO voucher)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var entity = db.VOUCHER.FirstOrDefault(x => x.Mavc == voucher.Mavc); if (entity == null) return 0;
                Apply(entity, voucher); db.SubmitChanges(); return 1;
            }
        }

        public bool UpdateVoucherChiTiet(int mavc, List<int> maspList)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var voucher = db.VOUCHER.FirstOrDefault(x => x.Mavc == mavc);
                if (voucher == null) return false;
                var products = maspList ?? new List<int>();
                return products.All(masp => db.SANPHAM.Any(x => x.Masp == masp && voucher.Maloai.HasValue && x.Maloai == voucher.Maloai.Value));
            }
        }

        private DataTable GetJoinedVouchers(int? type)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from v in db.VOUCHER join l in db.LOAISP on v.Maloai equals (int?)l.Maloai into categories from l in categories.DefaultIfEmpty() join k in db.KIEUVC on v.Maloaivc equals k.Maloaivc select new { v, l, k };
                if (type.HasValue) query = query.Where(x => x.v.Maloaivc == type.Value);
                var table = ToTable(query.AsEnumerable().Select(x => ToDto(x.v)).ToList());
                if (!table.Columns.Contains("Tenloai")) table.Columns.Add("Tenloai");
                return table;
            }
        }

        private static VoucherDTO ToDto(VOUCHER x) => new VoucherDTO { Mavc = x.Mavc, Code = x.Code, TenMaGiamGia = x.TenMaGiamGia, Giatri = x.Giatri, Ngaybd = x.Ngaybd, Ngaykt = x.Ngaykt, DieuKien = x.DieuKien, Maloaivc = x.Maloaivc, Maloai = x.Maloai };
        private static void Apply(VOUCHER x, VoucherDTO v) { x.Code = v.Code; x.TenMaGiamGia = v.TenMaGiamGia; x.Giatri = v.Giatri; x.Ngaybd = v.Ngaybd; x.Ngaykt = v.Ngaykt; x.DieuKien = v.DieuKien; x.Maloaivc = v.Maloaivc; x.Maloai = v.Maloai; }

        private static DataTable ToTable(List<VoucherDTO> vouchers)
        {
            var table = new DataTable(); table.Columns.Add("Mavc", typeof(int)); table.Columns.Add("Code"); table.Columns.Add("TenMaGiamGia"); table.Columns.Add("Giatri", typeof(decimal)); table.Columns.Add("Ngaybd", typeof(DateTime)); table.Columns.Add("Ngaykt", typeof(DateTime)); table.Columns.Add("DieuKien", typeof(decimal)); table.Columns.Add("Maloaivc", typeof(int)); table.Columns.Add("maloai", typeof(int));
            foreach (var v in vouchers) table.Rows.Add(v.Mavc, v.Code, v.TenMaGiamGia, v.Giatri, v.Ngaybd, v.Ngaykt, v.DieuKien ?? (object)DBNull.Value, v.Maloaivc, v.Maloai ?? (object)DBNull.Value);
            return table;
        }
    }
}