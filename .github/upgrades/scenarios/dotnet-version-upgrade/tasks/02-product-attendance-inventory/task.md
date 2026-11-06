# 02-product-attendance-inventory: Migrate product, attendance, and inventory DAOs

## Objective
Replace direct CRUD queries in product and attendance DAOs, and migrate inventory reads where the DBML schema is sufficient, while preserving public signatures and UI-compatible results.

## Research findings
- `SANPHAM` exposes `Masp`, `Tensp`, `Maloai`, `Duongdananh`, `Giaban`, `Soluongton`, `Canhbaotonkho`, and `Trangthaisp`; `LOAISP` exposes `Maloai` and `Tenloai`.
- `CHAMCONG` exposes `Idcc`, `Manv`, `Ngay`, `GioBatDau`, nullable `GioKetThuc`, and nullable `TongThoiGian`, with a mapped `NHANVIEN` association. `NHANVIEN` exposes `Hoten` and nullable `Luong`.
- Product methods returning `DataTable` must continue to return the same column labels expected by Forms; LINQ projections can be materialized into those tables.
- Product CRUD, status toggling, product lookup, category lookup, voucher-to-product lookup through `APMAVC`/`CHITIETHD` only where represented, attendance CRUD/reporting, and stock reads are safe LINQ candidates.
- `KHODAO` import/export transactions and history procedures return database-calculated shapes and output IDs whose exact procedure semantics are not present in DBML; retain those procedure calls for now and document them. `SANPHAM` stock updates are safe only when the operation is isolated from the procedure’s other side effects.

## Files to modify
- `src/DAO/ChamCongDAO.cs`
- `src/DAO/DanhSachSanPhamDAO.cs`
- `src/DAO/KHODAO.cs` only for safe mapped reads if needed
- Do not modify `src/DAO/DataClasses1.designer.cs`.

## Validation
Build `src/DAO/DAO.csproj` and the full solution after this logical group. Treat all warnings as failures.

## Done when
- The group builds without errors or warnings.
- Public DAO signatures remain compatible.
- Unsupported inventory procedure paths are documented.
