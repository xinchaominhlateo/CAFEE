using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DTO;

namespace DAO
{
    public class TraHangDAO
    {
        public DataTable GetDanhSachHoaDon(string tuKhoa = "")
        {
            var dt = new DataTable();
            dt.Columns.Add("Mahd", typeof(int));
            dt.Columns.Add("Ngaylap", typeof(DateTime));
            dt.Columns.Add("TenNhanVien");
            dt.Columns.Add("TenKhachHang");
            dt.Columns.Add("TongTien", typeof(decimal));
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from hd in db.HOADON
                            join nv in db.NHANVIEN on hd.Manv equals nv.Manv
                            join kh in db.KHACHHANG on hd.Makh equals (int?)kh.Makh into customers
                            from kh in customers.DefaultIfEmpty()
                            select new { hd, nv, kh };
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    var term = tuKhoa.Trim();
                    query = query.Where(x => x.hd.Mahd.ToString().Contains(term) || (x.kh != null && x.kh.Tenkh.Contains(term)));
                }
                foreach (var row in query.OrderByDescending(x => x.hd.Ngaylap))
                    dt.Rows.Add(row.hd.Mahd, row.hd.Ngaylap, row.nv.Hoten, row.kh == null ? null : row.kh.Tenkh, row.hd.TongTien);
            }
            return dt;
        }

        public List<ChiTietTraHangDTO> GetChiTietHoaDon(int maHD)
        {
            using (var db = DatabaseContextFactory.Create())
                return (from ct in db.CHITIETHD
                        join sp in db.SANPHAM on ct.Masp equals sp.Masp
                        where ct.Mahd == maHD
                        select new ChiTietTraHangDTO { Masp = ct.Masp, TenSP = sp.Tensp, SoLuong = ct.Soluong, DonGia = ct.Dongia }).ToList();
        }

        public int ThemTraHang(int maHD, string manv, string lyDo)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var row = new TRAHANG { Mahd = maHD, Manv = manv, Lydotra = lyDo, Ngaytra = DateTime.Now };
                db.TRAHANG.InsertOnSubmit(row);
                db.SubmitChanges();
                return row.Matra;
            }
        }

        public DataTable GetHoaDonChoTraHang(string tuKhoa = "")
        {
            var table = new DataTable();
            table.Columns.Add("Mahd", typeof(int));
            table.Columns.Add("Ngaylap", typeof(DateTime));
            table.Columns.Add("Makh", typeof(int));
            table.Columns.Add("Manv");
            table.Columns.Add("TongTien", typeof(decimal));
            using (var db = DatabaseContextFactory.Create())
            {
                var query = db.HOADON.AsQueryable();
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    var term = tuKhoa.Trim();
                    query = query.Where(x => x.Mahd.ToString().Contains(term));
                }

                foreach (var invoice in query.OrderByDescending(x => x.Ngaylap))
                    table.Rows.Add(invoice.Mahd, invoice.Ngaylap, invoice.Makh.HasValue ? (object)invoice.Makh.Value : DBNull.Value, invoice.Manv, invoice.TongTien);
            }
            return table;
        }

        public DataTable GetDanhSachTraHangTable(string tuKhoa = "")
        {
            var table = new DataTable();
            table.Columns.Add("Matra", typeof(int));
            table.Columns.Add("Ngaytra", typeof(DateTime));
            table.Columns.Add("TenNhanVien");
            table.Columns.Add("Makh", typeof(int));
            table.Columns.Add("Tenkh");
            table.Columns.Add("Lydotra");
            table.Columns.Add("TongTien", typeof(decimal));
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from th in db.TRAHANG
                            join nv in db.NHANVIEN on th.Manv equals nv.Manv
                            join hd in db.HOADON on th.Mahd equals hd.Mahd
                            join kh in db.KHACHHANG on hd.Makh equals (int?)kh.Makh into customers
                            from kh in customers.DefaultIfEmpty()
                            select new { th, nv, hd, kh };
                if (!string.IsNullOrWhiteSpace(tuKhoa))
                {
                    var term = tuKhoa.Trim();
                    query = query.Where(x => x.th.Matra.ToString().Contains(term)
                        || x.nv.Hoten.Contains(term)
                        || (x.kh != null && x.kh.Tenkh.Contains(term)));
                }
                foreach (var row in query.OrderByDescending(x => x.th.Ngaytra))
                    table.Rows.Add(row.th.Matra, row.th.Ngaytra, row.nv.Hoten,
                        row.hd.Makh.HasValue ? (object)row.hd.Makh.Value : DBNull.Value,
                        row.kh == null ? null : row.kh.Tenkh, row.th.Lydotra, row.hd.TongTien);
            }
            return table;
        }

        public DataTable GetChiTietTraHangByIdTable(int maTra)
        {
            var table = new DataTable();
            table.Columns.Add("Masp", typeof(int));
            table.Columns.Add("Tensp");
            table.Columns.Add("Soluong", typeof(int));
            table.Columns.Add("Dongia", typeof(decimal));
            table.Columns.Add("ThanhTien", typeof(decimal));
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from ct in db.CTTRAHANG
                            join sp in db.SANPHAM on ct.Masp equals sp.Masp
                            where ct.Matra == maTra
                            select new { ct, sp };
                foreach (var row in query)
                    table.Rows.Add(row.ct.Masp, row.sp.Tensp, row.ct.Soluong, row.ct.Dongia, row.ct.Soluong * row.ct.Dongia);
            }
            return table;
        }

        public DataTable GetChiTietHoaDonTable(int maHD)
        {
            var table = new DataTable();
            table.Columns.Add("Masp", typeof(int));
            table.Columns.Add("Tensp");
            table.Columns.Add("Soluong", typeof(int));
            table.Columns.Add("Dongia", typeof(decimal));
            table.Columns.Add("Thanhtien", typeof(decimal));
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from ct in db.CHITIETHD
                            join sp in db.SANPHAM on ct.Masp equals sp.Masp
                            where ct.Mahd == maHD
                            select new { ct, sp };
                foreach (var row in query)
                    table.Rows.Add(row.ct.Masp, row.sp.Tensp, row.ct.Soluong, row.ct.Dongia, row.ct.Soluong * row.ct.Dongia);
            }
            return table;
        }

        public DataTable GetChiTietTraHangTable(int maHD)
        {
            var table = new DataTable();
            table.Columns.Add("Matra", typeof(int));
            table.Columns.Add("Ngaytra", typeof(DateTime));
            table.Columns.Add("Lydotra");
            table.Columns.Add("Masp", typeof(int));
            table.Columns.Add("Tensp");
            table.Columns.Add("Soluong", typeof(int));
            table.Columns.Add("Dongia", typeof(decimal));
            table.Columns.Add("Thanhtien", typeof(decimal));
            using (var db = DatabaseContextFactory.Create())
            {
                var query = from th in db.TRAHANG
                            join ct in db.CTTRAHANG on th.Matra equals ct.Matra
                            join sp in db.SANPHAM on ct.Masp equals sp.Masp
                            where th.Mahd == maHD
                            select new { th, ct, sp };
                foreach (var row in query)
                    table.Rows.Add(row.th.Matra, row.th.Ngaytra, row.th.Lydotra, row.ct.Masp, row.sp.Tensp, row.ct.Soluong, row.ct.Dongia, row.ct.Soluong * row.ct.Dongia);
            }
            return table;
        }

        public int TaoTraHang(int maHD, string manv, string lyDo, List<ChiTietTraHangDTO> chiTiet)
        {
            if (chiTiet == null || chiTiet.Count == 0) throw new ArgumentException("Chi tiết trả hàng không được rỗng.", nameof(chiTiet));
            using (var db = DatabaseContextFactory.Create())
            {
                db.Connection.Open();
                using (var transaction = db.Connection.BeginTransaction())
                {
                    db.Transaction = transaction;
                    try
                    {
                        var returnRow = new TRAHANG { Mahd = maHD, Manv = manv, Lydotra = lyDo, Ngaytra = DateTime.Now };
                        db.TRAHANG.InsertOnSubmit(returnRow);
                        db.SubmitChanges();
                        foreach (var item in chiTiet)
                        {
                            var product = db.SANPHAM.FirstOrDefault(x => x.Masp == item.Masp);
                            if (product == null) throw new InvalidOperationException("Không tìm thấy sản phẩm.");
                            db.CTTRAHANG.InsertOnSubmit(new CTTRAHANG { Matra = returnRow.Matra, Masp = item.Masp, Soluong = item.SoLuong, Dongia = item.DonGia });
                            product.Soluongton += item.SoLuong;
                        }
                        db.SubmitChanges();
                        transaction.Commit();
                        return returnRow.Matra;
                    }
                    catch
                    {
                        try { transaction.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        public void ThemChiTietTraHang(int maTra, int masp, int soluong, decimal dongia)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                db.Connection.Open();
                using (var transaction = db.Connection.BeginTransaction())
                {
                    db.Transaction = transaction;
                    db.CTTRAHANG.InsertOnSubmit(new CTTRAHANG { Matra = maTra, Masp = masp, Soluong = soluong, Dongia = dongia });
                    var product = db.SANPHAM.FirstOrDefault(x => x.Masp == masp);
                    if (product == null) throw new InvalidOperationException("Không tìm thấy sản phẩm.");
                    product.Soluongton += soluong;
                    db.SubmitChanges();
                    transaction.Commit();
                }
            }
        }
    }
}
