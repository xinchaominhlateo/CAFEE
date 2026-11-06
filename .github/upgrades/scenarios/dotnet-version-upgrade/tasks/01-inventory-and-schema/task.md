# 01-inventory-and-schema: Inventory DAO schema and migration boundaries

## Objective
Inventory the remaining DAO data access and establish the safe LINQ-to-SQL migration boundary before implementation.

## Research findings
- Solution projects are SDK-style and target `net48`; no framework or package change is in scope.
- `src/DAO/DatabaseContextFactory.cs` creates `DataClasses1DataContext` from connection string `QUANLICHTL`.
- `DataClasses1.dbml` maps 17 tables: `APMAVC`, `CHAMCONG`, `CHITIETHD`, `CHITIETNHAPKHO`, `CHITIETXUATKHO`, `CTTRAHANG`, `HOADON`, `KHACHHANG`, `KIEUVC`, `LOAISP`, `NHACUNGCAP`, `NHANVIEN`, `NHAPKHO`, `SANPHAM`, `TRAHANG`, `VOUCHER`, and `XUATKHO`. There is no `VOUCHER_SANPHAM` table/entity.
- Remaining direct DataProvider/ADO.NET callers are in `BanHangDAO`, `ChamCongDAO`, `DanhSachSanPhamDAO`, `KHODAO`, `LichSuHoaDonDAO`, `NhanVienDAO`, `TaiKhoanDAO`, `ThongKeDAO`, `TraHangDAO`, `VoucherDAO`, and `Voucher1tang1DAO`.
- `DangNhapNVDAO`, `DangNhapQLDAO`, and `KhachHangDAO` are already LINQ-to-SQL based and are compatibility references for context creation and DTO mapping.
- Public DAO signatures include several `DataTable` returns used by Forms; those methods must keep their signatures and can materialize compatible tables from LINQ projections.
- Safe candidates include table CRUD, joins, filtering, grouping, aggregate reporting, stock updates, attendance, employee/account checks, invoice history, returns, and voucher/APMAVC operations where columns are present in DBML.
- Procedure behavior that depends on `VOUCHER_SANPHAM`, opaque computed result shapes, or database-side transaction/business rules not derivable from mapped tables must remain explicitly documented; do not invent a missing table mapping.
- `DataProvider` must remain until a repository-wide caller search proves no callers remain.

## Logical migration groups
1. Product, attendance, and inventory: `DanhSachSanPhamDAO`, `ChamCongDAO`, safe `KHODAO` operations.
2. Employee, account, invoice history, and returns: `NhanVienDAO`, `TaiKhoanDAO`, `LichSuHoaDonDAO`, `TraHangDAO`.
3. Sales and reporting: `BanHangDAO`, `ThongKeDAO`.
4. Vouchers and provider boundary: `VoucherDAO`, `Voucher1tang1DAO`, followed by remaining-caller verification.

## Done when
- The schema and DAO boundaries above are recorded.
- Implementation tasks are ordered by logical group.
- No generated `DataClasses1.designer.cs` edit is planned.
