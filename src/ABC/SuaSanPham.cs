using BUS;
using DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ABC
{
    public partial class SuaSanPham : Form
    {
        private DanhSachSanPhamBUS sanPhamBUS = DanhSachSanPhamBUS.Instance;
        private string maSP; // Mã sản phẩm đang sửa
        private string selectedImagePath = null; // Đường dẫn ảnh mới (nếu có)
        private string currentImagePath = null;  // Đường dẫn tương đối ảnh hiện tại trong DB
        public SuaSanPham(string maSP)
        {
            InitializeComponent();
            this.maSP = maSP;
        }

        private void SuaSanPham_Load(object sender, EventArgs e)
        {
            txtMa.ReadOnly = true; // Không cho sửa Mã SP
            txtMa.Text = this.maSP;

            LoadComboBoxes();
            LoadProductDetails();
            LoadProductImage();
            UIButton.ReplaceStandardButtonsWithIcons(this, Properties.Resources.exit, Properties.Resources.delete, Properties.Resources.refresh, Properties.Resources.done);
            UIText.ApplyButtonTextStyle(this);
        }
        private void LoadComboBoxes()
        {
            try
            {
                // GetLoaiSanPham trả về DataTable từ DAO -> bind trực tiếp
                cbbLoaiSanPham.DataSource = sanPhamBUS.GetLoaiSanPham();
                cbbLoaiSanPham.DisplayMember = "Tenloai";
                cbbLoaiSanPham.ValueMember = "Maloai";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải loại sản phẩm: " + ex.Message);
            }
        }

        // Hàm chính tải thông tin lên form
        private void LoadProductDetails()
        {
            try
            {
                // Lấy thông tin sản phẩm (kích cỡ removed)
                SanPhamDTO info = sanPhamBUS.GetSanPhamTheoMaVaKichCo(this.maSP, null);
                if (info == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin sản phẩm.");
                    this.Close();
                    return;
                }

                // 1. Tải thông tin cơ bản
                txtTen.Text = info.TenSP;
                // MaLoai trong DTO mapped as MaLoai
                if (info.MaLoai != 0 && cbbLoaiSanPham.Items.Count > 0)
                {
                    try { cbbLoaiSanPham.SelectedValue = info.MaLoai; } catch { }
                }

                // Nếu DB có CanhBaoTonKho thì hiển thị (SanPhamDTO có trường CanhBaoTonKho)
                txtSoLuongCanhBao.Text = info.CanhBaoTonKho.ToString();
                currentImagePath = info.DuongDanAnh;

                // 2. Vì kích cỡ đã bị remove, dùng single price control (txtSuaGiaS) to show GiaBan
                try
                {
                    txtGia.Text = info.GiaBan.ToString("F0");
                }
                catch { /* Ignore if control missing */ }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải chi tiết sản phẩm: " + ex.Message);
            }
        }

        // Tải ảnh: ưu tiên đường dẫn trong DB, fallback ảnh kiểu cũ images/products/{maSP}.*
        private void LoadProductImage()
        {
            Image img = null;
            string rel = ProductImageHelper.ResolveRelativePath(currentImagePath, this.maSP);
            if (rel != null)
                img = ProductImageHelper.LoadFromFile(ProductImageHelper.ToFullPath(rel));
            ProductImageHelper.ShowImage(picAnhSua, img);
        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            try
            {
                // (BỔ SUNG) Đọc số lượng cảnh báo
                if (!int.TryParse(txtSoLuongCanhBao.Text, out int canhBao) || canhBao < 0)
                {
                    MessageBox.Show("Số lượng cảnh báo không hợp lệ.");
                    return;
                }

                // Read price from single control txtSuaGiaS
                if (!decimal.TryParse(txtGia.Text.Trim(), out decimal giaBan) || giaBan < 0)
                {
                    MessageBox.Show("Giá bán không hợp lệ.");
                    return;
                }

                // Resolve numeric Masp
                int maspInt = sanPhamBUS.GetMasp(this.maSP);
                if (maspInt == 0)
                {
                    // try parse directly if user provided numeric string
                    int.TryParse(this.maSP, out maspInt);
                }

                if (maspInt == 0)
                {
                    MessageBox.Show("Không xác định được Mã sản phẩm để cập nhật.");
                    return;
                }

                // Có ảnh mới -> lưu file & cập nhật đường dẫn; không có -> giữ nguyên ảnh cũ
                string newImagePath = null;
                if (!string.IsNullOrEmpty(selectedImagePath))
                    newImagePath = ProductImageHelper.SaveProductImage(selectedImagePath, this.maSP);

                SanPhamDTO sp = new SanPhamDTO
                {
                    Masp = maspInt,
                    TenSP = txtTen.Text.Trim(),
                    MaLoai = cbbLoaiSanPham.SelectedValue != null ? Convert.ToInt32(cbbLoaiSanPham.SelectedValue) : 0,
                    GiaBan = giaBan,
                    DuongDanAnh = newImagePath ?? currentImagePath,
                    CanhBaoTonKho = canhBao
                };

                bool ok = sanPhamBUS.UpdateSanPham(sp);

                if (!ok && newImagePath != null) ProductImageHelper.TryDelete(newImagePath);

                if (ok)
                {
                    MessageBox.Show("Cập nhật sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Cập nhật thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void btnSuaAnh_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.jpg, *.jpeg, *.png, *.gif)|*.jpg;*.jpeg;*.png;*.gif";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    selectedImagePath = openFileDialog.FileName; // Lưu đường dẫn

                    Image img = ProductImageHelper.LoadFromFile(selectedImagePath);
                    if (img == null) throw new InvalidDataException("File không phải ảnh hợp lệ.");
                    ProductImageHelper.ShowImage(picAnhSua, img);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể tải ảnh: " + ex.Message);
                    selectedImagePath = null;
                }
            }
        }
    }
}
