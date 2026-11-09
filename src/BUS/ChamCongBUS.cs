                               using DAO;
using DTO;
using System;
using System.Collections.Generic;

namespace BUS
{
    public class ChamCongBUS
    {
        private static ChamCongBUS instance;
        public static ChamCongBUS Instance => instance ??= new ChamCongBUS();

        public bool BatDauLam(string manv)
        {
            return ChamCongDAO.Instance.InsertBatDauLam(manv, DateTime.Now);
        }

        public bool ChamCong(string manv)
        {
            DateTime ngay = DateTime.Today;
            DateTime? gioBD = ChamCongDAO.Instance.GetGioBatDauChuaChamCong(manv, ngay);
            if (gioBD == null) return false;

            DateTime gioKT = DateTime.Now;
            int tongPhut = (int)(gioKT - gioBD.Value).TotalMinutes;

            return ChamCongDAO.Instance.UpdateChamCong(manv, ngay, gioKT, tongPhut);
        }

        public int TinhTongGioLamTrongNgay(string manv, DateTime ngay)
        {
            return ChamCongDAO.Instance.GetTongPhutTrongNgay(manv, ngay);
        }

        public List<ChamCongDTO> LayLichSuChamCong(string keyword, DateTime tuNgay, DateTime denNgay)
        {
            return ChamCongDAO.Instance.GetLichSuChamCongChiTiet(keyword, tuNgay, denNgay);
        }

        // Gửi bảng lương qua mail cho từng nhân viên. Trả về (số mail gửi được, danh sách lỗi)
        public (int thanhCong, List<string> loi) GuiBangLuongQuaMail(DateTime tuNgay, DateTime denNgay)
        {
            int thanhCong = 0;
            var loi = new List<string>();

            foreach (var nv in ChamCongDAO.Instance.GetBangLuong(tuNgay, denNgay))
            {
                if (string.IsNullOrWhiteSpace(nv.Email))
                {
                    loi.Add(nv.Hoten + ": chưa có email");
                    continue;
                }
                try
                {
                    string tieuDe = $"Bảng lương {tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy}";
                    EmailService.Gui(nv.Email, tieuDe, TaoNoiDungMail(nv, tuNgay, denNgay));
                    thanhCong++;
                }
                catch (Exception ex)
                {
                    loi.Add(nv.Hoten + ": " + (ex.InnerException?.Message ?? ex.Message));
                }
            }
            return (thanhCong, loi);
        }

        private string TaoNoiDungMail(BangLuongDTO nv, DateTime tuNgay, DateTime denNgay)
        {
            return $@"<h3>Bảng lương {tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy}</h3>
<p>Xin chào <b>{nv.Hoten}</b> ({nv.Manv}),</p>
<table border='1' cellpadding='6' style='border-collapse:collapse'>
<tr><td>Tổng giờ làm</td><td>{nv.TongPhut / 60}h {nv.TongPhut % 60}p</td></tr>
<tr><td>Lương theo giờ</td><td>{nv.LuongGio:N0} đ</td></tr>
<tr><td><b>Tổng lương</b></td><td><b>{nv.TongLuong:N0} đ</b></td></tr>
</table>";
        }

        public decimal LayLuongTheoGio(string manv)
        {
            return ChamCongDAO.Instance.GetLuongTheoGio(manv);
        }

    }
}
