/* v1 - Created: 2024.01.17 - Initial version */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Services;
using System.Web.Script.Services;
using Newtonsoft.Json;

namespace AspxExamples
{
    public partial class FileUploadViewer : System.Web.UI.Page
    {
        private const string UPLOAD_FOLDER = "~/Uploads/PDFs/";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Upload klasörünün varlığını kontrol et ve oluştur
                string uploadPath = Server.MapPath(UPLOAD_FOLDER);
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // URL'den dosya yolu parametresini kontrol et
                string filePath = Request.QueryString["file"];
                if (!string.IsNullOrEmpty(filePath))
                {
                    hdnCurrentFile.Value = filePath;
                }
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (fuPdfUpload.HasFile)
                {
                    string fileName = Path.GetFileName(fuPdfUpload.FileName);
                    
                    // Sadece PDF dosyalarına izin ver
                    if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("Sadece PDF dosyaları yüklenebilir.");
                    }

                    // Dosya adını benzersiz yap
                    string uniqueFileName = string.Format("{0}_{1}", DateTime.Now.Ticks, fileName);
                    string filePath = Path.Combine(Server.MapPath(UPLOAD_FOLDER), uniqueFileName);
                    
                    // Dosyayı kaydet
                    fuPdfUpload.SaveAs(filePath);

                    // Relative path oluştur
                    string relativePath = string.Format("{0}{1}", UPLOAD_FOLDER.TrimStart('~'), uniqueFileName);
                    
                    // Client-side script ile dosya listesini güncelle
                    string script = string.Format(@"
                        fileList.push('{0}');
                        updateFileList();
                        viewFile('{0}');
                        showNotification('Dosya başarıyla yüklendi', 'success');
                    ", relativePath);
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "uploadSuccess", script, true);
                }
            }
            catch (Exception ex)
            {
                string errorScript = string.Format("showNotification('{0}', 'error');", 
                    HttpUtility.JavaScriptStringEncode(ex.Message));
                ScriptManager.RegisterStartupScript(this, GetType(), "uploadError", errorScript, true);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object DeleteFile(string filePath)
        {
            try
            {
                // Güvenlik kontrolü - sadece upload klasöründeki dosyaların silinmesine izin ver
                string normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar);
                if (!normalizedPath.StartsWith("/Uploads/PDFs/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Geçersiz dosya yolu.");
                }

                string fullPath = HttpContext.Current.Server.MapPath(string.Format("~{0}", filePath));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return new { success = true };
                }
                
                return new { success = false, message = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }
    }
}