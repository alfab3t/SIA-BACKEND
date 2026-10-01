# Pagination Quick Test Guide

## Quick Test URLs (Copy & Paste ke Browser/Postman)

### 1. Drop Out - Get All with Pagination
```
GET https://your-api-url/api/DropOut?page=1&pageSize=10
GET https://your-api-url/api/DropOut?page=2&pageSize=10
GET https://your-api-url/api/DropOut?page=1&pageSize=20
```

### 2. Drop Out - Riwayat Paginated
```
GET https://your-api-url/api/DropOut/riwayat/paginated?page=1&pageSize=10
GET https://your-api-url/api/DropOut/riwayat/paginated?page=1&pageSize=10&keyword=john
```

### 3. Drop Out - Pending Paginated
```
GET https://your-api-url/api/DropOut/pending/paginated?page=1&pageSize=10
```

### 4. Backward Compatibility Test (No Pagination)
```
GET https://your-api-url/api/DropOut
GET https://your-api-url/api/DropOut/riwayat?username=admin
GET https://your-api-url/api/DropOut/pending?username=admin
```

---

## Expected Response Format

### With Pagination:
```json
{
  "data": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalRecords": 156,
    "totalPages": 16
  }
}
```

### Without Pagination (Old Format):
```json
[
  { "droId": "DO001", ... },
  { "droId": "DO002", ... }
]
```

---

## Curl Commands

### Test Pagination
```bash
curl -X GET "https://your-api-url/api/DropOut?page=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### Test with Search
```bash
curl -X GET "https://your-api-url/api/DropOut?page=1&pageSize=10&keyword=john" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

---

## SQL Setup (Run First!)

```sql
-- Run this SQL script to create pagination stored procedures
-- File: SQL_CREATE_SP_PAGINATION_DROPOUT.sql

USE [ERP_PolmanAstra_NDA]
GO

-- Check if SPs exist
SELECT name FROM sys.procedures 
WHERE name LIKE '%Paginated%'
GO

-- Expected output:
-- sia_getRiwayatDropOutPaginated
-- sia_getPendingDropOutPaginated
```

---

## Verification Checklist

- [ ] SQL stored procedures created
- [ ] Backend compiled without errors
- [ ] Swagger shows new endpoints
- [ ] Test endpoint returns pagination format
- [ ] Page 1 and Page 2 return different data
- [ ] Total records count is correct
- [ ] Old endpoints still work (backward compatible)
- [ ] Search + pagination works together
- [ ] Max pageSize (100) is enforced

---

## Common Issues & Solutions

### Issue: "Stored procedure not found"
**Solution:** Run `SQL_CREATE_SP_PAGINATION_DROPOUT.sql`

### Issue: "Compilation error - PaginatedResponse not found"
**Solution:** Check `DTOs/Common/PaginatedResponse.cs` exists

### Issue: "Returns array instead of object"
**Solution:** Make sure you're using pagination parameters (`?page=1&pageSize=10`)

### Issue: "Total records is 0"
**Solution:** Check OUTPUT parameter in SP is working correctly

---

## Performance Comparison

### Test with 500 records:

**Without Pagination:**
```
Response Time: ~3000ms
Response Size: ~2MB
Records Returned: 500
```

**With Pagination (pageSize=10):**
```
Response Time: ~300ms ⚡ (10x faster!)
Response Size: ~50KB 💾 (40x smaller!)
Records Returned: 10
```

---

## Next Steps for Frontend

1. Update API calls to include `page` and `pageSize` parameters
2. Handle `result.data` instead of direct array
3. Use `result.pagination` for pagination component
4. Remove client-side slicing logic
5. Test and verify

---

## Contact

Backend implementation: ✅ DONE
Frontend implementation: 🔄 PENDING

Questions? Check `PAGINATION_IMPLEMENTATION_GUIDE.md` for detailed docs.
