using BUS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ABC
{
    public partial class LichSuChamCong : Form
    {
        public LichSuChamCong()
        {
            InitializeComponent();
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LichSuChamCong_Load(object sender, EventArgs e)
        {
            dtpTuNgay.Value = DateTime.Today.AddDays(-7);
            dtpDenNgay.Value = DateTime.Today;
            FilterChamCong();
            UIButton.ReplaceStandardButtonsWithIcons(this, Properties.Resources.exit, Properties.Resources.delete, Properties.Resources.refresh, Properties.Resources.done);
            UIText.ApplyButtonTextStyle(this);
            UIDataGridView.FormatDataGridView(dgvLSChamCong);
        }
        private void FilterChamCong()
        {
            string keyword = txtTimKiem.Text.Trim();
            DateTime tuNgay = dtpTuNgay.Value.Date;
            DateTime denNgay = dtpDenNgay.Value.Date;

            var danhSach = ChamCongBUS.Instance.LayLichSuChamCong(keyword, tuNgay, denNgay);
            dgvLSChamCong.DataSource = danhSach;
            UIDataGridView.FormatDataGridView(dgvLSChamCong);
            if (dgvLSChamCong.Columns.Count > 0)
            {
                dgvLSChamCong.Columns["Manv"].HeaderText = "Mã nhân viên";
                dgvLSChamCong.Columns["TenNhanVien"].HeaderText = "Tên nhân viên";
                dgvLSChamCong.Columns["Luong"].HeaderText = "Lương theo giờ";
                dgvLSChamCong.Columns["Ngay"].HeaderText = "Ngày";
                dgvLSChamCong.Columns["GioBatDau"].HeaderText = "Giờ bắt đầu";
                dgvLSChamCong.Columns["GioKetThuc"].HeaderText = "Giờ kết thúc";
                dgvLSChamCong.Columns["TongThoiGian"].HeaderText = "Tổng phút làm";
                dgvLSChamCong.Columns["TongLuong"].HeaderText = "Tổng lương";
                dgvLSChamCong.Columns["TongLuong"].DefaultCellStyle.Format = "N0";
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e)
        {
            FilterChamCong();
        }

        private void dtpTuNgay_ValueChanged(object sender, EventArgs e)
        {
            FilterChamCong();
        }

        private void dtpDenNgay_ValueChanged(object sender, EventArgs e)
        {
            FilterChamCong();
        }
    }
}
