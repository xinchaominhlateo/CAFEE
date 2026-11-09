namespace DTO
{
    // Dữ liệu bảng lương của 1 nhân viên trong khoảng ngày
    public class BangLuongDTO
    {
        public string Manv { get; set; }
        public string Hoten { get; set; }
        public string Email { get; set; }
        public int TongPhut { get; set; }
        public decimal LuongGio { get; set; }
        public decimal TongLuong { get; set; }
    }
}
