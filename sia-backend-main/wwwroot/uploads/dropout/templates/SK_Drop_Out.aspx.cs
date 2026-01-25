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

                    reportdocument.SetParameterValue("@p1", id);
                    reportdocument.SetParameterValue("@p2", "");
                    reportdocument.SetParameterValue("@p3", "");
                    reportdocument.SetParameterValue("@p4", "");
                    reportdocument.SetParameterValue("@p5", "");
                    reportdocument.SetParameterValue("@p6", "");
                    reportdocument.SetParameterValue("@p7", "");
                    reportdocument.SetParameterValue("@p8", "");
                    reportdocument.SetParameterValue("@p9", "");
                    reportdocument.SetParameterValue("@p10", "");
                    reportdocument.SetParameterValue("@p11", "");
                    reportdocument.SetParameterValue("@p12", "");
                    reportdocument.SetParameterValue("@p13", "");
                    reportdocument.SetParameterValue("@p14", "");
                    reportdocument.SetParameterValue("@p15", "");
                    reportdocument.SetParameterValue("@p16", "");
                    reportdocument.SetParameterValue("@p17", "");
                    reportdocument.SetParameterValue("@p18", "");
                    reportdocument.SetParameterValue("@p19", "");
                    reportdocument.SetParameterValue("@p20", "");
                    reportdocument.SetParameterValue("@p21", "");
                    reportdocument.SetParameterValue("@p22", "");
                    reportdocument.SetParameterValue("@p23", "");
                    reportdocument.SetParameterValue("@p24", "");
                    reportdocument.SetParameterValue("@p25", "");
                    reportdocument.SetParameterValue("@p26", "");
                    reportdocument.SetParameterValue("@p27", "");
                    reportdocument.SetParameterValue("@p28", "");
                    reportdocument.SetParameterValue("@p29", "");
                    reportdocument.SetParameterValue("@p30", "");
                    reportdocument.SetParameterValue("@p31", "");
                    reportdocument.SetParameterValue("@p32", "");
                    reportdocument.SetParameterValue("@p33", "");
                    reportdocument.SetParameterValue("@p34", "");
                    reportdocument.SetParameterValue("@p35", "");
                    reportdocument.SetParameterValue("@p36", "");
                    reportdocument.SetParameterValue("@p37", "");
                    reportdocument.SetParameterValue("@p38", "");
                    reportdocument.SetParameterValue("@p39", "");
                    reportdocument.SetParameterValue("@p40", "");
                    reportdocument.SetParameterValue("@p41", "");
                    reportdocument.SetParameterValue("@p42", "");
                    reportdocument.SetParameterValue("@p43", "");
                    reportdocument.SetParameterValue("@p44", "");
                    reportdocument.SetParameterValue("@p45", "");
                    reportdocument.SetParameterValue("@p46", "");
                    reportdocument.SetParameterValue("@p47", "");
                    reportdocument.SetParameterValue("@p48", "");
                    reportdocument.SetParameterValue("@p49", "");
                    reportdocument.SetParameterValue("@p50", "");

                    reportdocument.SetParameterValue("@p1", id, reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p2", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p3", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p4", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p5", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p6", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p7", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p8", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p9", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p10", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p11", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p12", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p13", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p14", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p15", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p16", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p17", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p18", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p19", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p20", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p21", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p22", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p23", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p24", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p25", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p26", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p27", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p28", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p29", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p30", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p31", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p32", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p33", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p34", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p35", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p36", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p37", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p38", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p39", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p40", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p41", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p42", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p43", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p44", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p45", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p46", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p47", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p48", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p49", "", reportdocument.Subreports[0].Name);
                    reportdocument.SetParameterValue("@p50", "", reportdocument.Subreports[0].Name);

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
