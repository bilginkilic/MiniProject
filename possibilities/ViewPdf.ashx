<%@ WebHandler Language="C#" Class="AspxExamples.ViewPdf" %>

using System;
using System.Web;
using AspxExamples.Common.Models;

namespace AspxExamples
{
    public class ViewPdf : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            try
            {
                string fileName = context.Request.QueryString["file"];
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new ArgumentException("Dosya adı belirtilmedi.");
                }

                string pdfPath = SessionHelper.GetUploadedPdfPath();
                if (string.IsNullOrEmpty(pdfPath) || !System.IO.File.Exists(pdfPath))
                {
                    throw new System.IO.FileNotFoundException("PDF dosyası bulunamadı.");
                }

                context.Response.Clear();
                context.Response.ContentType = "application/pdf";
                context.Response.AppendHeader("Content-Disposition", "inline; filename=" + fileName);
                context.Response.TransmitFile(pdfPath);
                context.Response.End();
            }
            catch (Exception ex)
            {
                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.Write("Hata: " + ex.Message);
                context.Response.End();
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}
