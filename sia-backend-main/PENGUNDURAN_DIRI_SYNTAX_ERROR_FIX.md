# Pengunduran Diri SP Syntax Error Fix

## Issue
User reported SQL syntax error: "Incorrect syntax near the keyword 'AS'" when calling the `sia_getDataPengunduranDiri` stored procedure.

## Root Cause
The error was caused by improper dynamic SQL construction in the stored procedure. The original implementation had:

1. **Dynamic SQL Syntax Issues**: Incorrect string concatenation and escaping in the WHERE clause construction
2. **Complex Dynamic Query**: The stored procedure was trying to build dynamic SQL which is error-prone
3. **Missing SQL Injection Protection**: The dynamic SQL didn't properly escape single quotes

## Error Details
```
System.Exception: Database error: Incorrect syntax near the keyword 'AS'.
Microsoft.Data.SqlClient.SqlException (0x80131904): Incorrect syntax near the keyword 'AS'.
Error Number:156,State:1,Class:15
```

## Solutions Provided

### 1. SQL_FIX_SP_SYNTAX_ERROR_PENGUNDURAN_DIRI.sql
- Fixed the dynamic SQL construction
- Added proper quote escaping using REPLACE function
- Maintained the original dynamic approach but with correct syntax

### 2. SQL_CREATE_SIMPLE_SP_PENGUNDURAN_DIRI.sql  
- Replaced dynamic SQL with static CTE-based query
- Simpler and more maintainable approach
- Better performance due to query plan caching

### 3. SQL_COMPREHENSIVE_FIX_PENGUNDURAN_DIRI.sql (RECOMMENDED)
- Complete rewrite using CTE (Common Table Expression)
- Proper kon_id filtering that works with both pro_id and pro_nama
- Support for multiple status filtering (comma-separated)
- Added debug output for troubleshooting
- Better SQL injection protection
- Cleaner code structure

## Key Improvements

### kon_id Filter Fix
The kon_id filter now properly checks:
- `d.pro_id = @kon_id` (exact match with program ID)
- `d.pro_nama LIKE '%' + @kon_id + '%'` (partial match with program name)
- `c.kon_id = @kon_id` (exact match with concentration ID)
- `c.kon_nama LIKE '%' + @kon_id + '%'` (partial match with concentration name)

### Multiple Status Support
```sql
AND (@status IS NULL OR @status = '' OR 
     a.pdi_status IN (SELECT LTRIM(RTRIM(value)) FROM STRING_SPLIT(@status, ',')))
```

### Pagination
Uses ROW_NUMBER() with proper BETWEEN clause for efficient pagination.

## Testing
Execute any of these files to fix the issue:
1. `SQL_COMPREHENSIVE_FIX_PENGUNDURAN_DIRI.sql` (recommended)
2. `SQL_CREATE_SIMPLE_SP_PENGUNDURAN_DIRI.sql` (simpler alternative)
3. `SQL_FIX_SP_SYNTAX_ERROR_PENGUNDURAN_DIRI.sql` (minimal fix)

## Backend Compatibility
The fix maintains full compatibility with the existing C# repository methods:
- `GetAllAsync()` 
- `GetAllPaginatedAsync()`
- All parameter names and return columns remain the same

## Next Steps
1. Execute the comprehensive fix SQL
2. Test the kon_id filter functionality
3. Verify pagination works correctly
4. Test with various parameter combinations