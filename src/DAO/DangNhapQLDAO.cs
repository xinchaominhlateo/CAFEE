using DTO;
using System.Linq;

namespace DAO
{
    public class DangNhapQLDAO
    {
        public DangNhapQLDTO Dangnhap(string username)
        {
            using var db = DatabaseContextFactory.Create();

            return db.NHANVIEN
                .Where(x => x.Tk == username && x.Manv.StartsWith("AD"))
                .Select(x => new DangNhapQLDTO
                {
                    Manv = x.Manv,
                    Tk = x.Tk,
                    Mk = x.Mk,
                    Hoten = x.Hoten
                })
                .FirstOrDefault();
        }
    }
}
