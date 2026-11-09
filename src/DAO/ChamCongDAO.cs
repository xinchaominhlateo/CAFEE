using DTO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DAO
{
    public class ChamCongDAO
    {
        private static ChamCongDAO instance;
        public static ChamCongDAO Instance => instance ??= new ChamCongDAO();

        public bool InsertBatDauLam(string manv, DateTime gioBatDau)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                db.CHAMCONG.InsertOnSubmit(new CHAMCONG
                {
                    Manv = manv,
                    Ngay = gioBatDau.Date,
                    GioBatDau = gioBatDau
                });
                db.SubmitChanges();
                return true;
            }
        }

        public bool UpdateChamCong(string manv, DateTime ngay, DateTime gioKetThuc, int tongPhut)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                var record = db.CHAMCONG.FirstOrDefault(c => c.Manv == manv && c.Ngay == ngay.Date && c.GioKetThuc == null);
                if (record == null) return false;
                record.GioKetThuc = gioKetThuc;
                record.TongThoiGian = tongPhut;
                db.SubmitChanges();
                return true;
            }
        }

        public DateTime? GetGioBatDauChuaChamCong(string manv, DateTime ngay)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                return db.CHAMCONG
                    .Where(c => c.Manv == manv && c.Ngay == ngay.Date && c.GioKetThuc == null)
                    .OrderByDescending(c => c.Idcc)
                    .Select(c => (DateTime?)c.GioBatDau)
                    .FirstOrDefault();
            }
        }

        public int GetTongPhutTrongNgay(string manv, DateTime ngay)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                return db.CHAMCONG
                    .Where(c => c.Manv == manv && c.Ngay == ngay.Date)
                    .Select(c => c.TongThoiGian ?? 0)
                    .ToList()
                    .Sum();
            }
        }

        public List<ChamCongDTO> GetLichSuChamCongChiTiet(string keyword, DateTime tuNgay, DateTime denNgay)
        {
            var normalizedKeyword = keyword ?? string.Empty;
            using (var db = DatabaseContextFactory.Create())
            {
                return (from c in db.CHAMCONG
                        join n in db.NHANVIEN on c.Manv equals n.Manv
                        where c.Ngay >= tuNgay.Date && c.Ngay <= denNgay.Date
                           && (normalizedKeyword == string.Empty || n.Hoten.Contains(normalizedKeyword) || c.Manv.Contains(normalizedKeyword))
                        orderby c.Ngay descending
                        select new ChamCongDTO
                        {
                            Id = c.Idcc,
                            Manv = c.Manv,
                            TenNhanVien = n.Hoten,
                            Luong = n.Luong ?? 0m,
                            Ngay = c.Ngay,
                            GioBatDau = c.GioBatDau,
                            GioKetThuc = c.GioKetThuc,
                            TongThoiGian = c.TongThoiGian,
                            TongLuong = ((c.TongThoiGian ?? 0) / 60m) * (n.Luong ?? 0m)
                        }).ToList();
            }
        }

        // Gộp chấm công theo nhân viên trong khoảng ngày -> bảng lương
        public List<BangLuongDTO> GetBangLuong(DateTime tuNgay, DateTime denNgay)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                return (from c in db.CHAMCONG
                        join n in db.NHANVIEN on c.Manv equals n.Manv
                        where c.Ngay >= tuNgay.Date && c.Ngay <= denNgay.Date
                        group c by new { n.Manv, n.Hoten, n.Email, n.Luong } into g
                        select new BangLuongDTO
                        {
                            Manv = g.Key.Manv,
                            Hoten = g.Key.Hoten,
                            Email = g.Key.Email,
                            LuongGio = g.Key.Luong ?? 0m,
                            TongPhut = g.Sum(x => x.TongThoiGian ?? 0),
                            TongLuong = g.Sum(x => x.TongThoiGian ?? 0) / 60m * (g.Key.Luong ?? 0m)
                        }).ToList();
            }
        }

        public decimal GetLuongTheoGio(string manv)
        {
            using (var db = DatabaseContextFactory.Create())
            {
                return db.NHANVIEN.Where(n => n.Manv == manv).Select(n => n.Luong ?? 0m).FirstOrDefault();
            }
        }
    }
}
