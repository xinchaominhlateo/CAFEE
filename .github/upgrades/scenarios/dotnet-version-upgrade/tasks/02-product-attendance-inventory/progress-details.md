# Progress Details

## Files modified
- `src/DAO/ChamCongDAO.cs` — attendance insert/update/read/history/pay-rate queries now use `DataClasses1DataContext`.
- `src/DAO/DanhSachSanPhamDAO.cs` — category/product queries, search, status toggle, product lookup, and product insert/update now use LINQ to SQL; schema-dependent voucher-detail and cascade-delete procedures remain on `DataProvider`.
- `src/DAO/TaiKhoanDAO.cs` — account/password/login operations now use LINQ to SQL.
- `src/DAO/LichSuHoaDonDAO.cs` — invoice history, invoice details, invoice summary, voucher usage, employee list, and customer aggregation now use LINQ to SQL.
- `src/DAO/TraHangDAO.cs` — return invoice/detail queries and return creation/detail stock update now use LINQ to SQL.
- `src/DAO/NhanVienDAO.cs` — employee CRUD, lookup, grid/search output, and uniqueness checks now use LINQ to SQL.

## Validation
- DAO project build: successful.
- Full solution MSBuild build: successful with pre-existing nullable/unused-field warnings in `src/ABC`; no compilation errors.

## Remaining boundaries
- `KHODAO` import/export transactions and provider-backed inventory procedures were not changed because their database-side output IDs and side effects are not represented in the DBML model.
- `sp_GetChiTietVoucher` and product cascade deletion remain provider-backed because the DBML has no `VOUCHER_SANPHAM` mapping and does not describe the procedure’s cascade contract.
- The workflow state tool was unavailable in this continuation, so task completion state could not be advanced automatically.
