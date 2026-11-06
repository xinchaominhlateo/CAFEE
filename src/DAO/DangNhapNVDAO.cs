using DTO;
using System.Linq;

namespace DAO
{
    public class DangNhapNVDAO
    {
        public DangNhapNVDTO DangNhap(string username)
        {
            using var db = DatabaseContextFactory.Create();

            return db.NHANVIEN
                .Where(x => x.Tk == username)
                .Select(x => new DangNhapNVDTO
                {
                    Manv = x.Manv,
                    Tk = x.Tk,
                    Mk = x.Mk,
                    Hoten = x.Hoten,
                    Vitri = x.Vitri
                })
                .FirstOrDefault();
        }
    }
}
