using System.Collections.Generic;
using System.Data;
using DAO;
using DTO;

namespace BUS
{
    public class TraHangBUS
    {
        private TraHangDAO dao = new TraHangDAO();

        public DataTable GetDanhSachHoaDon(string tuKhoa = "")
        {
            return dao.GetDanhSachHoaDon(tuKhoa);
        }

        public List<ChiTietTraHangDTO> GetChiTietHoaDon(int maHD)
        {
            return dao.GetChiTietHoaDon(maHD);
        }

        public int TaoTraHang(int maHD, string manv, string lyDo, List<ChiTietTraHangDTO> chiTiet)
        {
            return dao.TaoTraHang(maHD, manv, lyDo, chiTiet);
        }

        public DataTable GetHoaDonChoTraHang(string tuKhoa = "") => dao.GetHoaDonChoTraHang(tuKhoa);
        public DataTable GetChiTietHoaDonTable(int maHD) => dao.GetChiTietHoaDonTable(maHD);
        public DataTable GetChiTietTraHangTable(int maHD) => dao.GetChiTietTraHangTable(maHD);
        public DataTable GetDanhSachTraHangTable(string tuKhoa = "") => dao.GetDanhSachTraHangTable(tuKhoa);
        public DataTable GetChiTietTraHangByIdTable(int maTra) => dao.GetChiTietTraHangByIdTable(maTra);
    }
}
