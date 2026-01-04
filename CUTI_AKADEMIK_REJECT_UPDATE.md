# CutiAkademik Reject API Update - Keterangan Field Removed

## Summary
Completely removed the "keterangan" field from the CutiAkademik reject endpoint as it's not needed.

## Changes Made

### 1. DTO Update - Field Completely Removed
**File**: `sia-backend-main/DTOs/CutiAkademik/RejectCutiAkademikRequest.cs`

**Before**:
```csharp
[Required(ErrorMessage = "Keterangan/alasan penolakan harus diisi")]
[MinLength(5, ErrorMessage = "Keterangan minimal 5 karakter")]
public string Keterangan { get; set; } = "";
```

**After**: Field completely removed from DTO

### 2. Controller Validation Update
**File**: `sia-backend-main/Controllers/CutiAkademikController.cs`

**Removed validation**: All keterangan validation removed
**Updated log message**: Removed keterangan from log output

### 3. Repository Update
**File**: `sia-backend-main/Repositories/Implementations/CutiAkademikRepository.cs`

**Stored Procedure Call**:
```csharp
// Before: spCmd.Parameters.AddWithValue("@p3", dto.Keterangan ?? "");
// After:
spCmd.Parameters.AddWithValue("@p3", ""); // Empty keterangan
```

**Direct SQL Update**:
```csharp
// Before: SET cak_keterangan = @keterangan
// After: SET cak_keterangan = ''
```

## API Usage

### New Request Format (Simplified)
```json
{
  "id": "017/PMA/CA/I/2026",
  "role": "NDA-PRODI", 
  "username": "nda_prodi"
}
```

**Note**: The `keterangan` field has been completely removed and is no longer accepted.

### Breaking Change
This is a **breaking change** - any clients sending the `keterangan` field will need to remove it from their requests.

## Database Handling
- The system now always stores an empty string (`""`) for keterangan when rejecting
- Both stored procedure and direct SQL update use empty keterangan

## Testing
Build completed successfully with no compilation errors. The endpoint now only accepts the three required fields: `id`, `role`, and `username`.