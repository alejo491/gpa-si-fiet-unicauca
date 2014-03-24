using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;

namespace MvcSEDOC.Models
{
    public class Utilities
    {
        public static void ManageException(Exception ex, string path = "")
        {            
            string strConfigValue = System.Configuration.ConfigurationManager.AppSettings.Get("RegisterOnLog");
            if (strConfigValue == "true")
            {
                if (path == "") path = @"\generalLog.log";
                System.IO.StreamWriter oLog = new System.IO.StreamWriter(path, true);
                oLog.WriteLine("----------------------------------------------------------------");
                oLog.WriteLine(string.Format("Message: {0}", ex.Message));
                oLog.WriteLine(string.Format("Source: {0}", ex.Source));
                oLog.WriteLine(string.Format("StackTrace: {0}", ex.StackTrace));
                oLog.Close();
            }
        }

        public static object ExecuteScalar(string sSQL)
        {
            string sConString = System.Configuration.ConfigurationManager.ConnectionStrings["SEDOCConection"].ConnectionString;
            SqlConnection oConection = new SqlConnection(sConString);
            SqlCommand oComando = new SqlCommand(sSQL,oConection);
            oConection.Open();
            object oResult = oComando.ExecuteScalar();
            oConection.Close();
            return oResult;
        }


        public iTextSharp.text.Document StartPdfWriter(string sUserId, string sEvalType, string sPeriod, string sWorkDescription, string sMapPath)
        {
            try
            {
                Document oDocument = new iTextSharp.text.Document();
                string sDescription = sWorkDescription;
                if (sDescription.Length > 40)
                {
                    sDescription = sDescription.Substring(0, 40);
                }
                string sPath = sMapPath
                    + sUserId + "_" + sPeriod + "_" + sEvalType + "_" + sDescription.Replace("_"," ") + "_"
                    + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";
                System.IO.FileStream file =
                       new System.IO.FileStream(sPath,
                       System.IO.FileMode.OpenOrCreate);
                PdfWriter oWriter = PdfWriter.GetInstance(oDocument, file);
                oDocument.AddTitle("SEDOC - Reporte Evaluación");
                oDocument.AddAuthor("FIET - Universidad del Cauca");
                oDocument.SetMargins(40, 40, 35, 35);
                oDocument.Open();
                Font oHeaderFont = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD);

                Paragraph oUcauca = new Paragraph("Universidad del Cauca\nFacultad de Ingeniería Electrónica y Telecomunicaciones\nSistema de Evaluación Docente");
                oUcauca.Font = oHeaderFont;
                oUcauca.Alignment = Element.ALIGN_CENTER;
                oDocument.Add(oUcauca);
                

                return oDocument;
            }
            catch
            {
                return null;
            }
        }

    }
}