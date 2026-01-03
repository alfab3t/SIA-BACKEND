# MeninggalDunia Approve & Reject Endpoints Fix

## Issue Summary
Both the approve and reject endpoints for MeninggalDunia were failing with 400 Bad Request errors, even though the stored procedures were actually working correctly. The problem was that the stored procedures `sia_setujuiMeninggalDunia` and `sia_tolakMeninggalDunia` were returning -1 rows affected despite successfully updating the records.

## Root Cause
The stored procedures were working correctly (changing status from "Belum Disetujui Wadir 1" to "Menunggu Upload SK" for approvals, or to rejected status), but the backend code was only checking the `rows affected` count returned by `ExecuteNonQueryAsync()`. Since the stored procedures returned -1, the methods returned `false`, causing the endpoints to return 400 Bad Request.

## Solution Implemented

### 1. Fixed ApproveAsync Method
- **File**: `sia-backend-main/Repositories/Implementations/MeninggalDuniaRepository.cs`
- **Changes**:
  - Added status checking before and after stored procedure execution
  - Now considers approval successful if either:
    - Status changed from original status, OR
    - Rows affected > 0
  - Added comprehensive logging for debugging
  - Added error handling with try-catch

### 2. Fixed RejectAsync Method
- **File**: `sia-backend-main/Repositories/Implementations/MeninggalDuniaRepository.cs`
- **Changes**:
  - Applied same fix as ApproveAsync method
  - Added status checking before and after stored procedure execution
  - Now considers rejection successful if either:
    - Status changed from original status, OR
    - Rows affected > 0
  - Added comprehensive logging for debugging
  - Added error handling with try-catch

### 3. Enhanced Approve Controller Endpoint
- **File**: `sia-backend-main/Controllers/MeninggalDuniaController.cs`
- **Changes**:
  - Already had automatic role detection using `DetectUserRoleAsync`
  - Role mapping: str_main_id values "27", "23", "28" → "finance", others → "wadir1"
  - Enhanced error messages with more details

### 4. Enhanced Reject Controller Endpoint
- **File**: `sia-backend-main/Controllers/MeninggalDuniaController.cs`
- **Changes**:
  - Added automatic role detection using `DetectUserRoleAsync` (same as approve)
  - Role mapping: str_main_id values "27", "23", "28" → "finance", others → "wadir1"
  - Enhanced error messages with more details
  - Added comprehensive logging for debugging

## Key Features

### Automatic Role Detection
Both endpoints now automatically detect user roles based on username:
- Uses `all_getIdentityByUser` stored procedure
- Maps `str_main_id` values to roles:
  - "27", "23", "28" → "finance"
  - All others → "wadir1"
- No need to manually specify role in request

### Status Change Verification
Instead of relying solely on rows affected count, both methods now:
1. Get current status before operation
2. Execute stored procedure
3. Get status after operation
4. Compare statuses to verify change occurred
5. Return success if status changed OR rows affected > 0

### Enhanced Logging
Added comprehensive console logging for debugging:
- Parameters being sent to stored procedures
- Status before and after operations
- Success/failure reasons
- Error details with stack traces

## Testing
The fix has been implemented and compiled successfully. The endpoints should now work correctly:

### Approve Endpoint
```
PUT /api/MeninggalDunia/approve/{id}
{
  "username": "tonny.pongoh"
}
```

### Reject Endpoint
```
PUT /api/MeninggalDunia/reject/{id}
{
  "username": "tonny.pongoh"
}
```

Both endpoints will:
1. Automatically detect user role
2. Execute the appropriate stored procedure
3. Verify status change occurred
4. Return success with detailed response

## Expected Behavior
- **Approve**: Status changes from "Belum Disetujui Wadir 1" to "Menunggu Upload SK"
- **Reject**: Status changes to appropriate rejected status
- Both return detailed success/error messages
- Automatic role detection eliminates need to specify role manually