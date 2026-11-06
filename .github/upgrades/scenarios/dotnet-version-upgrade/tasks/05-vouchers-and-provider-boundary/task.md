# 05-vouchers-and-provider-boundary: Migrate voucher DAOs and define remaining provider boundary

Migrate VoucherDAO and Voucher1tang1DAO operations backed by VOUCHER, APMAVC, LOAISP, KIEUVC, and SANPHAM entities. Preserve DataTable compatibility and explicitly document unsupported VOUCHER_SANPHAM-dependent procedures. Remove DataProvider callers only after repository-wide verification.

**Done when**: voucher functionality supported by DBML is LINQ-backed, remaining provider calls are documented and intentional, and the group builds warning-free.
