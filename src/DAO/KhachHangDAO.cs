using System;
using System.Collections.Generic;
using System.Linq;
using DTO;
namespace DAO;

public sealed class KhachHangDAO
{
    private static readonly Lazy<KhachHangDAO> LazyInstance =
        new(() => new KhachHangDAO());

    public static KhachHangDAO Instance => LazyInstance.Value;

    private KhachHangDAO() { }

        public static List<KhachHangDTO> layDSKH()
        {
            using var db = DatabaseContextFactory.Create();

            return db.KHACHHANG
                .Select(x => new KhachHangDTO
                {
                    Makh = x.Makh,
                    Tenkh = x.Tenkh,
                    Sdt = x.Sdt,
                    Tichdiem = x.Tichdiem
                })
                .ToList();
        }
        public static string GetTenKhachHang(int maKH)
        {
            using var db = DatabaseContextFactory.Create();

            return db.KHACHHANG
                .Where(x => x.Makh == maKH)
                .Select(x => x.Tenkh)
                .FirstOrDefault() ?? "Khách lẻ";
        }

        public static string GetSDTKhachHang(int maKH)
        {
            using var db = DatabaseContextFactory.Create();

            return db.KHACHHANG
                .Where(x => x.Makh == maKH)
                .Select(x => x.Sdt)
                .FirstOrDefault() ?? string.Empty;
        }
        public static string LayTenKhachHangTheoSDT(string sdt)
        {
            using var db = DatabaseContextFactory.Create();

            return db.KHACHHANG
                .Where(x => x.Sdt == sdt)
                .Select(x => x.Tenkh)
                .FirstOrDefault() ?? "Khách lẻ";
        }
        public static void themKH(KhachHangDTO kh)
        {
            using var db = DatabaseContextFactory.Create();

            var entity = new KHACHHANG
            {
                Tenkh = kh.Tenkh,
                Sdt = kh.Sdt,
                Tichdiem = kh.Tichdiem
            };

            db.KHACHHANG.InsertOnSubmit(entity);
            db.SubmitChanges();
            kh.Makh = entity.Makh;
        }
        public static void xoaKH(int makh)
        {
            using var db = DatabaseContextFactory.Create();

            var entity = db.KHACHHANG.FirstOrDefault(x => x.Makh == makh);
            if (entity is null)
                return;

            db.KHACHHANG.DeleteOnSubmit(entity);
            db.SubmitChanges();
        }

        public static void suaKH(KhachHangDTO kh)
        {
            using var db = DatabaseContextFactory.Create();

            var entity = db.KHACHHANG.FirstOrDefault(x => x.Makh == kh.Makh);
            if (entity is null)
                throw new InvalidOperationException($"Không tìm thấy khách hàng có mã {kh.Makh}.");

            entity.Tenkh = kh.Tenkh;
            entity.Sdt = kh.Sdt;
            entity.Tichdiem = kh.Tichdiem;
            db.SubmitChanges();
        }
        public static List<KhachHangDTO> timTheoTenHoacSDT(string keyword)
        {
            keyword ??= string.Empty;
            using var db = DatabaseContextFactory.Create();

            return db.KHACHHANG
                .Where(x => x.Tenkh.Contains(keyword) || x.Sdt.Contains(keyword))
                .Select(x => new KhachHangDTO
                {
                    Makh = x.Makh,
                    Tenkh = x.Tenkh,
                    Sdt = x.Sdt,
                    Tichdiem = x.Tichdiem
                })
                .ToList();
        }
        public List<KhachHangDTO> TimKiemTheoSDT(string sdt)
        {
            sdt ??= string.Empty;
            using var db = DatabaseContextFactory.Create();

            return db.KHACHHANG
                .Where(x => x.Sdt.Contains(sdt))
                .Select(x => new KhachHangDTO
                {
                    Makh = x.Makh,
                    Tenkh = x.Tenkh,
                    Sdt = x.Sdt,
                    Tichdiem = x.Tichdiem
                })
                .ToList();
        }
    }
