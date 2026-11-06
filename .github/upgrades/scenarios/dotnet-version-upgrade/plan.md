# DAO LINQ to SQL Modernization Plan

## Overview

**Target**: Remaining DAO data access in the .NET Framework 4.8 solution.
**Scope**: DAO, BUS/Form compatibility, and workflow documentation; no framework/package changes and no edits to generated DBML code.

## Tasks

### 01-inventory-and-schema: Inventory DAO schema and migration boundaries

Inspect DataClasses1.dbml/designer mappings, all remaining DAO implementations, and BUS/Form callers. Record available entities/tables, public API constraints, stored procedures that can be represented safely with LINQ to SQL, and procedures requiring retention because their result shape, transaction semantics, or schema (especially VOUCHER_SANPHAM) is not represented.

**Done when**: task documentation contains the researched inventory and migration boundaries, and the migration groups are identified.

---

### 02-product-attendance-inventory: Migrate product, attendance, and inventory DAOs

Replace direct CRUD queries in DanhSachSanPhamDAO, ChamCongDAO, and safe portions of KHODAO with DataClasses1DataContext queries and SubmitChanges while preserving signatures and UI-compatible results. Retain and document only procedures that cannot be safely reproduced from DBML.

**Done when**: the group builds without errors or warnings, public DAO signatures remain compatible, and unsupported inventory procedure paths are documented.

---

### 03-employee-account-history-returns: Migrate employee, account, invoice history, and returns

Migrate NhanVienDAO, TaiKhoanDAO, LichSuHoaDonDAO, and TraHangDAO to LINQ to SQL, preserving DTO mappings, null handling, and transaction behavior. Keep only explicitly unrepresentable result paths.

**Done when**: the group and dependents build warning-free and all listed DAO APIs remain callable by existing BUS/Form code.

---

### 04-sales-and-statistics: Migrate sales and reporting DAOs

Convert safe CRUD and query logic in BanHangDAO and ThongKeDAO to LINQ to SQL, preserving invoice, detail, stock, voucher application, and reporting behavior. Document procedure-dependent operations that cannot be safely inferred from the DBML schema.

**Done when**: the group builds warning-free and all converted methods preserve existing DTO/DataTable contracts.

---

### 05-vouchers-and-provider-boundary: Migrate voucher DAOs and define remaining provider boundary

Migrate VoucherDAO and Voucher1tang1DAO operations backed by VOUCHER, APMAVC, LOAISP, KIEUVC, and SANPHAM entities. Preserve DataTable compatibility and explicitly document unsupported VOUCHER_SANPHAM-dependent procedures. Remove DataProvider callers only after repository-wide verification.

**Done when**: voucher functionality supported by DBML is LINQ-backed, remaining provider calls are documented and intentional, and the group builds warning-free.

---

### 06-full-validation-and-reporting: Validate the complete solution and finalize migration documentation

Run a full-solution build and tests where available, fix all errors and warnings, search for remaining DataProvider callers, and record changed files, migrated functionality, unconvertible procedures/schema limits, and any remaining diagnostics.

**Done when**: the full solution builds successfully with no warnings, tests are run or documented unavailable, workflow progress-details is complete, and the final report is accurate.
