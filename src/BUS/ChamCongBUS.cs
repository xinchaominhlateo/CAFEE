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

        public decimal LayLuongTheoGio(string manv)
        {
            return ChamCongDAO.Instance.GetLuongTheoGio(manv);
        }

    }
}
