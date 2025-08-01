<%@ WebHandler Language="C#" Class="ImageHandler" %>

using System;
using System.Web;
using System.IO;

public class ImageHandler : IHttpHandler
{
    private readonly string _cdn = @"\\trrgap3027\files\circular\cdn";
    
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            string fileName = context.Request.QueryString["file"];
            if (string.IsNullOrEmpty(fileName))
            {
                context.Response.StatusCode = 400;
                return;
            }

            // Güvenlik kontrolü - sadece PNG dosyalarına izin ver
            if (!fileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 400;
                return;
            }

            // Path injection önleme
            fileName = Path.GetFileName(fileName);
            string filePath = Path.Combine(_cdn, fileName);

            if (!File.Exists(filePath))
            {
                context.Response.StatusCode = 404;
                return;
            }

            // Resmi oku ve gönder
            context.Response.ContentType = "image/png";
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(30));
            context.Response.Cache.SetMaxAge(new TimeSpan(0, 30, 0));
            
            using (FileStream fs = File.OpenRead(filePath))
            {
                fs.CopyTo(context.Response.OutputStream);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            System.Diagnostics.Debug.WriteLine(String.Format("Image handler error: {0}", ex.Message));
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
} 