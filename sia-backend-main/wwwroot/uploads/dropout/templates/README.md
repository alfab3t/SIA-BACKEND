# Template SK Drop Out - Crystal Reports

## Overview
Template ini digunakan untuk generate PDF SK (Surat Keputusan) Drop Out menggunakan Crystal Reports di sistem lama (ASP.NET WebForms).

## Files Structure
```
wwwroot/uploads/dropout/templates/
├── Report_SK_Drop_Out_2.rpt          # Crystal Reports template
├── Report_SK_Drop_Out_2.cs           # Wrapper class untuk .rpt
├── SK_Drop_Out.aspx                  # ✅ ASP.NET WebForms page
├── SK_Drop_Out.aspx.cs               # ✅ Code-behind untuk generate PDF
├── SK_Drop_Out.aspx.designer.cs     # ✅ Designer generated code
└── README.md                         # This file
```

## ASP.NET WebForms Components

### 1. SK_Drop_Out.aspx
```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SK_Drop_Out.aspx.cs" Inherits="PolmanAstra_SIA.Reports.SK_Drop_Out" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SK Drop Out - PDF Generator</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Label ID="err" runat="server" Text="" ForeColor="Red"></asp:Label>
        </div>
    </form>
</body>
</html>
```

### 2. SK_Drop_Out.aspx.designer.cs
Auto-generated designer file yang mendefinisikan controls:
- `form1` - HtmlForm control
- `err` - Label control untuk error messages

### 3. SK_Drop_Out.aspx.cs
Code-behind file dengan logic Crystal Reports untuk generate PDF.

## How It Works

### 1. Crystal Reports Template (`Report_SK_Drop_Out_2.rpt`)
- Template utama untuk format SK Drop Out
- Menggunakan stored procedure untuk data
- Support 50 parameters (@p1 - @p50)
- Ada main report dan subreport

### 2. ASP.NET WebForms (`SK_Drop_Out.aspx.cs`)
- Load Crystal Reports template
- Set database connection (encrypted)
- Set parameters untuk main report dan subreport
- Export ke PDF format
- Return PDF ke browser

## Key Components

### Database Connection
```csharp
reportdocument.SetDatabaseLogon(
    PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkUserID"], "PoliteknikAstra_ConfigurationKey"), 
    PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkPassword"], "PoliteknikAstra_ConfigurationKey"), 
    PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkServerName"], "PoliteknikAstra_ConfigurationKey"), 
    PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkDatabaseName"], "PoliteknikAstra_ConfigurationKey")
);
```

### Parameters Setup
```csharp
// Main report parameters
reportdocument.SetParameterValue("@p1", id);  // Drop Out ID
for (int i = 2; i <= 50; i++)
{
    reportdocument.SetParameterValue($"@p{i}", "");
}

// Subreport parameters
reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
for (int i = 2; i <= 50; i++)
{
    reportdocument.SetParameterValue($"@p{i}", "", reportdocument.Subreports[0].Name);
}
```

### PDF Export
```csharp
reportdocument.ExportToHttpResponse(
    ExportFormatType.PortableDocFormat, 
    Response, 
    false, 
    "SK_Drop_Out_No." + dt.Rows[0][9].ToString()
);
```

## Stored Procedures Used

### 1. `sia_detailDO`
- Get detail data Drop Out
- Parameter: Drop Out ID
- Returns: Complete dropout information

### 2. `sia_checkReportDropOut`
- Check report suffix/version
- Parameter: Drop Out ID  
- Returns: Report suffix (currently not used, hardcoded to "_2")

## Usage in New System

### Current Implementation
Backend .NET Core menggunakan HTTP client untuk call service report:

```csharp
var requestBody = new
{
    reportName = "Report_SK_Drop_Out_2",
    parameters = new
    {
        droId = id
    }
};

var response = await client.PostAsync(reportServiceUrl, content);
```

### Template Download Endpoints
```csharp
// Download .rpt file
GET /api/dropout/template-sk?type=rpt

// Download .cs wrapper
GET /api/dropout/template-sk?type=cs

// Download .aspx page
GET /api/dropout/template-sk?type=aspx

// Download .aspx.cs code-behind
GET /api/dropout/template-sk?type=aspx-cs

// Download .aspx.designer.cs
GET /api/dropout/template-sk?type=aspx-designer
```

## Security Features

### 1. Encrypted Token
```csharp
String id = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(
    Request.QueryString["token"].ToString(), 
    "PolmanAstra_SIA"
).Split('#')[0];
```

### 2. Encrypted Database Credentials
All database connection parameters are encrypted using `PoliteknikAstra_ConfigurationKey`.

### 3. Error Handling
```csharp
catch
{
    err.Text = "Error:<br>- Terjadi kesalahan dalam pembuatan surat. Mohon hubungi MIS!";
}
```

## Dependencies

### Required Libraries
- `CrystalDecisions.CrystalReports.Engine`
- `CrystalDecisions.Shared`
- `PolmanAstraLibrary.PolmanAstraLibrary`
- `PolmanAstra_SIA.Classes.LDAPAuthentication`

### Configuration Required
```xml
<appSettings>
    <add key="linkUserID" value="[encrypted_username]" />
    <add key="linkPassword" value="[encrypted_password]" />
    <add key="linkServerName" value="[encrypted_server]" />
    <add key="linkDatabaseName" value="[encrypted_database]" />
</appSettings>

<connectionStrings>
    <add name="DefaultConnection" connectionString="[encrypted_connection_string]" />
</connectionStrings>
```

## Migration Notes

### From Old System to New System
1. **Report Service**: New system calls external report service instead of direct Crystal Reports
2. **Parameters**: Same parameter structure (@p1 = droId)
3. **Output**: Same PDF format and filename pattern
4. **Security**: New system uses JWT tokens instead of encrypted query strings

### Backward Compatibility
- Template files maintained for reference
- Same stored procedures used
- Same parameter structure
- Same PDF output format

---

**Created**: March 30, 2026
**System**: ASP.NET WebForms + Crystal Reports
**Purpose**: Generate SK Drop Out PDF documents