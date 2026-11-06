using System;
using System.Data;
using System.Windows.Forms;
using BUS;
using DTO;

namespace ABC
{
    public partial class TraHang : Form
    {
        private readonly TraHangBUS bus = new TraHangBUS();

        public TraHang()
        {
            InitializeComponent();
            LoadHoaDon(); // Load danh sách hóa đơn khi mở form
        }

        // Load danh sách hóa đơn
        private void LoadHoaDon()
        {
            dgvHoaDon.DataSource = bus.GetHoaDonChoTraHang();

            // dgvChiTietTraHang.DataSource = null; // Reset chi tiết trả hàng
        }

        // Khi chọn 1 hóa đơn
        private void dgvHoaDon_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvHoaDon.SelectedRows.Count > 0)
            {
                int mahd = Convert.ToInt32(dgvHoaDon.SelectedRows[0].Cells["Mahd"].Value);
                LoadChiTietHoaDon(mahd);     // Hiển thị chi tiết hóa đơn
                LoadChiTietTraHang(mahd);    // Hiển thị chi tiết trả hàng
            }
        }

        // Load chi tiết hóa đơn
        private void LoadChiTietHoaDon(int mahd)
        {
            bus.GetChiTietHoaDonTable(mahd);
        }

        // Load chi tiết trả hàng
        private void LoadChiTietTraHang(int mahd)
        {
            bus.GetChiTietTraHangTable(mahd);
        }

        // Tìm kiếm hóa đơn
        private void btnTimKiem_Click(object sender, EventArgs e)
        {
            string tuKhoa = txtTimKiem.Text.Trim();
            if (string.IsNullOrEmpty(tuKhoa))
            {
                LoadHoaDon();
                return;
            }

            dgvHoaDon.DataSource = bus.GetHoaDonChoTraHang(tuKhoa);
        }

        // Thực hiện trả hàng
        private void btnTraHang_Click(object sender, EventArgs e)
        {
            if (dgvHoaDon.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn hóa đơn để trả hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int mahd = Convert.ToInt32(dgvHoaDon.SelectedRows[0].Cells["Mahd"].Value);
            string lyDo = txtLyDo.Text.Trim();
            string manv = "NV01"; // Có thể lấy từ login hiện tại

            if (string.IsNullOrEmpty(lyDo))
            {
                MessageBox.Show("Vui lòng nhập lý do trả hàng.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var details = bus.GetChiTietHoaDon(mahd);
                bus.TaoTraHang(mahd, manv, lyDo, details);
                    MessageBox.Show("Trả hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadHoaDon();
                    // Sau khi load lại danh sách, load chi tiết trả hàng mới
                    if (dgvHoaDon.SelectedRows.Count > 0)
                    {
                        int selectedMahd = Convert.ToInt32(dgvHoaDon.SelectedRows[0].Cells["Mahd"].Value);
                        LoadChiTietTraHang(selectedMahd);
                    }

                    txtLyDo.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtTimKiem_TextChanged(object sender, EventArgs e) { }
        private void dgvChiTietTraHang_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void dgvHoaDon_CellContentClick(object sender, DataGridViewCellEventArgs e) { }
        private void lblLyDo_Click(object sender, EventArgs e) { }
        private void txtLyDo_TextChanged(object sender, EventArgs e) { }

        private void tlpContent_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
