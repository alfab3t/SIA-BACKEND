# Finance Approval Test Guide

## Problem
User `user_finance` with role `KARYAWAN` cannot approve cuti akademik requests. The API returns "Gagal menyetujui cuti akademik" and status doesn't change.

## Solution Applied
1. Enhanced `ApproveCutiAsync` method with specific finance approval logic
2. Added role mapping from `KARYAWAN` to `finance` in controller
3. Added comprehensive logging for debugging

## Testing Steps

### Step 1: Check Current Record Status
First, check the current status of the cuti akademik record:

```bash
GET http://localhost:5234/api/CutiAkademik/debug/check-record/013/PMA/CA/IX/2025
```

Expected response should show:
- Record exists
- Current status (should be "Belum Disetujui Finance" for finance approval)

### Step 2: Test Finance Approval (Debug Endpoint)
Use the debug endpoint to test finance approval with detailed logging:

```bash
POST http://localhost:5234/api/CutiAkademik/debug/test-finance-approval
Content-Type: application/json

{
  "id": "013/PMA/CA/IX/2025",
  "role": "KARYAWAN",
  "approvedBy": "user_finance"
}
```

Expected response:
```json
{
  "success": true,
  "message": "Finance approval berhasil",
  "originalRole": "finance",
  "parameters": {
    "id": "013/PMA/CA/IX/2025",
    "role": "finance",
    "approvedBy": "user_finance"
  }
}
```

### Step 3: Test Production Endpoint
Test the actual production endpoint:

```bash
PUT http://localhost:5234/api/CutiAkademik/approve
Content-Type: application/json

{
  "id": "013/PMA/CA/IX/2025",
  "role": "KARYAWAN",
  "approvedBy": "user_finance"
}
```

Expected response:
```json
{
  "message": "Cuti akademik berhasil disetujui."
}
```

### Step 4: Verify Status Change
Check the record again to verify the status changed:

```bash
GET http://localhost:5234/api/CutiAkademik/debug/check-record/013/PMA/CA/IX/2025
```

Expected changes:
- `cak_status` should be "Menunggu Upload SK"
- `cak_approval_dakap` should be "user_finance"
- `cak_app_dakap_date` should be current timestamp

## Troubleshooting

### If Status is Not "Belum Disetujui Finance"
The record might be in a different status. Check what the current status is and ensure the workflow is correct:
1. Draft → Belum Disetujui Prodi
2. Belum Disetujui Prodi → Belum Disetujui Wadir 1 (after prodi approval)
3. Belum Disetujui Wadir 1 → Belum Disetujui Finance (after wadir1 approval)
4. Belum Disetujui Finance → Menunggu Upload SK (after finance approval) ✅

### If Record Not Found
Verify the ID is correct. The ID should match exactly what's in the database.

### If Still Getting "Gagal menyetujui cuti akademik"
Check the console logs for detailed error messages. The enhanced logging will show:
- Database connection status
- Record existence check
- Current status validation
- SQL execution results
- Any exceptions

## Console Log Examples

Successful approval logs:
```
[ApproveCutiAsync] === STARTING APPROVAL ===
[ApproveCutiAsync] ID: '013/PMA/CA/IX/2025'
[ApproveCutiAsync] Role: 'KARYAWAN'
[ApproveCutiAsync] ApprovedBy: 'user_finance'
[ApproveCutiAsync] Database connection opened successfully
[ApproveCutiAsync] Checking if record exists...
[ApproveCutiAsync] Record found!
[ApproveCutiAsync] Current status: 'Belum Disetujui Finance'
[ApproveCutiAsync] MHS ID: 'MHS001'
[ApproveCutiAsync] Processing finance approval...
[ApproveCutiAsync] Status validation passed, executing finance approval update...
[ApproveCutiAsync] Executing SQL update...
[ApproveCutiAsync] Parameters: @id='013/PMA/CA/IX/2025', @approvedBy='user_finance'
[ApproveCutiAsync] Finance approval rows affected: 1
[ApproveCutiAsync] Finance approval successful!
[ApproveCutiAsync] Verification - New status: 'Menunggu Upload SK'
[ApproveCutiAsync] Verification - New approval: 'user_finance'
```

## Database Changes Made

The fix updates these fields when finance approves:
- `cak_approval_dakap` = approver username
- `cak_status` = 'Menunggu Upload SK'  
- `cak_app_dakap_date` = current timestamp
- `cak_modif_date` = current timestamp
- `cak_modif_by` = approver username

## Next Steps After Finance Approval

Once finance approval is successful:
1. Status becomes "Menunggu Upload SK"
2. Admin can create SK using `/api/CutiAkademik/create-sk`
3. Admin can upload SK file using `/api/CutiAkademik/upload-sk`
4. Final status becomes "Disetujui"