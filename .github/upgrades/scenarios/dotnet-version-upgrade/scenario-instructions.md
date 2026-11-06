# DAO LINQ to SQL Modernization

## Strategy
Complete the remaining DAO migration incrementally by logical DAO groups, using only entities and tables represented by `DataClasses1DataContext`; preserve .NET Framework 4.8 and public APIs.

## Preferences
- **Flow Mode**: Automatic
- **Commit Strategy**: After Each Task
- **Target Framework**: net48 (preserve; no framework or package changes)
- **Migration approach**: Replace direct CRUD/DataProvider/ADO.NET access with LINQ to SQL where safely representable; document unsupported stored procedures and schema gaps.
- **Generated model**: Do not manually edit `src/DAO/DataClasses1.designer.cs`.
- **Validation**: Build the full solution after each logical group and finish with a warning-free full-solution build.

## Source Control
- **Source Branch**: feature/LINQ02
- **Working Branch**: feature/LINQ02
- **Pending Changes**: Preserved as found at workflow initialization
- **Commit Strategy**: After Each Task

## Decisions
- Keep `DataProvider` until repository-wide caller search confirms no callers remain.
- Pay special attention to `VOUCHER_SANPHAM`; do not assume it exists in QLCH unless represented in the DBML schema.
