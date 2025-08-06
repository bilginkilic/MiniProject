<%@ WebHandler Language="C#" Class="SaveSignature" %>

using System;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;

public class SaveSignature : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            // CDN klasörünü kontrol et veya oluştur
            string cdnPath = Path.Combine(context.Server.MapPath("~/"), "cdn", "signatures");
            if (!Directory.Exists(cdnPath))
            {
                Directory.CreateDirectory(cdnPath);
            }

            // Dosya adını oluştur (timestamp ekleyerek benzersiz yap)
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            string fileName = $"signature_{timestamp}.png";
            string filePath = Path.Combine(cdnPath, fileName);

            // Dosyayı kaydet
            var file = context.Request.Files["signatureImage"];
            if (file != null && file.ContentLength > 0)
            {
                file.SaveAs(filePath);

                // Yanıt oluştur
                var response = new
                {
                    success = true,
                    imagePath = $"/cdn/signatures/{fileName}",
                    sourcePdfPath = context.Request.Form["sourcePdfPath"],
                    page = int.Parse(context.Request.Form["page"]),
                    x = int.Parse(context.Request.Form["x"]),
                    y = int.Parse(context.Request.Form["y"]),
                    width = int.Parse(context.Request.Form["width"]),
                    height = int.Parse(context.Request.Form["height"])
                };

                // JSON yanıtı gönder
                context.Response.ContentType = "application/json";
                context.Response.Write(new JavaScriptSerializer().Serialize(response));
            }
            else
            {
                throw new Exception("Dosya bulunamadı");
            }
        }
        catch (Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write(new JavaScriptSerializer().Serialize(new
            {
                success = false,
                error = ex.Message
            }));
        }
    }

    public bool IsReusable
    {
        get { return false; }
    }
}