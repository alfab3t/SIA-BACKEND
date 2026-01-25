using System;
using System.Configuration;
using PolmanAstra_SIA.Classes;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Data;
using System.Web.Configuration;

namespace PolmanAstra_SIA.Reports
{
    public partial class SK_Pengunduran_Diri : System.Web.UI.Page
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
                    String rep = lib.CallProcedure("sia_checkReportPengunduranDIri", new string[] { id }).Rows[0][0].ToString();
                    
                    reportdocument.Load(Server.MapPath("Report_SK_Pengunduran_Diri_2.rpt"));
                    reportdocument.SetDatabaseLogon(
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkUserID"], "PoliteknikAstra_ConfigurationKey"), 
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkPassword"], "PoliteknikAstra_ConfigurationKey"), 
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkServerName"], "PoliteknikAstra_ConfigurationKey"), 
                        PolmanAstraLibrary.PolmanAstraLibrary.Decrypt(WebConfigurationManager.AppSettings["linkDatabaseName"], "PoliteknikAstra_ConfigurationKey")
                    );

                    reportdocument.SetParameterValue("@p1", id);
                    reportdocument.ExportToHttpResponse(ExportFormatType.PortableDocFormat, Response, false, "SK_Pengunduran_Diri_No." + id);
                }
            }
            catch (Exception ex)
            {
                err.Text = ex.ToString();
            }
        }
    }
}
