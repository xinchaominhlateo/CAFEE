using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ABC
{
    /// <summary>
    /// Tiện ích dùng chung để đọc / lưu / hiển thị ảnh sản phẩm.
    /// Quy ước của dự án: cột SANPHAM.duongdananh lưu đường dẫn TƯƠNG ĐỐI
    /// (ví dụ "images/banhmi.jpg") tính từ thư mục gốc project ABC (src/ABC),
    /// tức là Application.StartupPath + "..\..\..".
    /// </summary>
    internal static class ProductImageHelper
    {
        /// <summary>Thư mục (tương đối) chứa ảnh sản phẩm được upload từ form Thêm/Sửa.</summary>
        public const string ProductFolder = "images/products";

        private static readonly string[] SupportedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".bmp" };

        public static string RootPath =>
            Path.GetFullPath(Path.Combine(Application.StartupPath, @"..\..\.."));

        /// <summary>Chuyển đường dẫn tương đối trong DB thành đường dẫn tuyệt đối.</summary>
        public static string ToFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return null;
            string normalized = relativePath.Trim().Replace('/', Path.DirectorySeparatorChar);
            // DB lưu dạng "/images/x.jpg" (có dấu / đầu) -> coi là tương đối so với thư mục gốc project
            if (normalized.StartsWith("\\") && !normalized.StartsWith("\\\\")) normalized = normalized.TrimStart('\\');
            return Path.IsPathRooted(normalized) ? normalized : Path.Combine(RootPath, normalized);
        }

        /// <summary>
        /// Đọc ảnh vào bộ nhớ (không khóa file trên đĩa). Trả về null nếu không đọc được.
        /// </summary>
        public static Image LoadFromFile(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath)) return null;
            try
            {
                using (var ms = new MemoryStream(File.ReadAllBytes(fullPath)))
                using (var tmp = Image.FromStream(ms))
                {
                    // Tạo bản sao độc lập với stream (GDI+ yêu cầu stream sống cùng Image)
                    return new Bitmap(tmp);
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Tìm đường dẫn tương đối của ảnh sản phẩm: ưu tiên giá trị trong DB,
        /// sau đó tới ảnh kiểu cũ "images/products/{maSP}.{ext}". Trả về null nếu không có.
        /// </summary>
        public static string ResolveRelativePath(string dbPath, string legacyKey = null)
        {
            string full = ToFullPath(dbPath);
            if (full != null && File.Exists(full)) return dbPath.Trim();

            if (!string.IsNullOrWhiteSpace(legacyKey))
            {
                foreach (string ext in SupportedExtensions)
                {
                    string rel = ProductFolder + "/" + legacyKey.Trim() + ext;
                    if (File.Exists(ToFullPath(rel))) return rel;
                }
            }
            return null;
        }

        /// <summary>
        /// Sao chép ảnh người dùng chọn vào images/products và trả về đường dẫn tương đối để lưu DB.
        /// Dùng tên file duy nhất để không ghi đè lên ảnh đang được hiển thị / bị khóa.
        /// </summary>
        public static string SaveProductImage(string sourcePath, string baseName)
        {
            string folder = ToFullPath(ProductFolder);
            Directory.CreateDirectory(folder);

            string safeName = new string((baseName ?? "sp")
                .Select(c => Path.GetInvalidFileNameChars().Contains(c) || char.IsWhiteSpace(c) ? '_' : c)
                .ToArray());
            if (string.IsNullOrEmpty(safeName)) safeName = "sp";

            string fileName = $"{safeName}_{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(sourcePath).ToLowerInvariant()}";
            File.Copy(sourcePath, Path.Combine(folder, fileName), true);
            return ProductFolder + "/" + fileName;
        }

        /// <summary>Xóa file ảnh đã sao chép (dùng khi lưu DB thất bại).</summary>
        public static void TryDelete(string relativePath)
        {
            try
            {
                string full = ToFullPath(relativePath);
                if (full != null && File.Exists(full)) File.Delete(full);
            }
            catch { /* bỏ qua */ }
        }

        /// <summary>
        /// Gán ảnh cho PictureBox: ảnh thật hiển thị dạng Zoom (giữ tỉ lệ),
        /// nếu không có ảnh thì hiển thị icon "no image" ở giữa, không bị kéo giãn.
        /// </summary>
        public static void ShowImage(PictureBox pic, Image image)
        {
            Image old = pic.Image;
            if (image != null)
            {
                pic.SizeMode = PictureBoxSizeMode.Zoom;
                pic.Image = image;
            }
            else
            {
                pic.SizeMode = PictureBoxSizeMode.CenterImage;
                pic.Image = Properties.Resources.no_image;
            }
            old?.Dispose();
        }

        /// <summary>Style chung cho khung ảnh sản phẩm trong các form.</summary>
        public static void StylePreview(PictureBox pic)
        {
            pic.BorderStyle = BorderStyle.FixedSingle;
            pic.BackColor = Color.White;
            pic.Dock = DockStyle.Fill;
            pic.Margin = new Padding(6);
        }
    }
}
