using System;
using System.Configuration;
using PolmanAstra_SIA.Classes;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;
using System.Web.Configuration;

namespace PolmanAstra_SIA.Reports
{
    public partial class SK_Drop_Out : System.Web.UI.Page
    {
        PolmanAstraLibrary.PolmanAstraLibrary lib = new PolmanAstraLibrary.PolmanAstraLibrary(PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString(), "PoliteknikAstra_ConfigurationKey"));
        LDAPAuthentication adAuth = new LDAPAuthentication();
        DataTable dt = new DataTable();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                using (ReportDocument reportdocument = new ReportDocument())
                {
                    String id = PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(Request.QueryString["token"].ToString(), "PolmanAstra_SIA").Split('#')[0];
                    dt = lib.CallProcedure("sia_detailDO", new string[] { id });
                    String rep = lib.CallProcedure("sia_checkReportDropOut", new string[] { id }).Rows[0][0].ToString();
                    
                    //reportdocument.Load(Server.MapPath("Report_SK_Drop_Out" + rep));
                    reportdocument.Load(Server.MapPath("Report_SK_Drop_Out_2.rpt"));
                    reportdocument.SetDatabaseLogon(
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkUserID"], "PoliteknikAstra_ConfigurationKey"), 
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkPassword"], "PoliteknikAstra_ConfigurationKey"), 
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkServerName"], "PoliteknikAstra_ConfigurationKey"), 
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkDatabaseName"], "PoliteknikAstra_ConfigurationKey")
                    );

                    // Set parameters for main report (p1-p50)
                    reportdocument.SetParameterValue("@p1", id);
                    for (int i = 2; i <= 50; i++)
                    {
                        reportdocument.SetParameterValue($"@p{i}", "");
                    }

                    // Set parameters for subreport (p1-p50)
                    reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
                    for (int i = 2; i <= 50; i++)
                    {
                        reportdocument.SetParameterValue($"@p{i}", "", reportdocument.Subreports[0].Name);
                    }

                    reportdocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, "SK_Drop_Out_No." + dt.Rows[0][9].ToString());
                }
            }
            catch
            {
                err.Text = "Error:<br>- Terjadi kesalahan dalam pembuatan surat. Mohon hubungi MIS!";
            }
        }
    }
}
