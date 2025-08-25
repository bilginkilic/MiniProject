/* v3 - Created: 2024.01.17 - Changed upload location to network CDN folder */

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
        // CDN klasör yolu
        private const string CDN_PATH = @"\\trrgap3027\files\circular\cdn";
        private const string ALLOWED_EXTENSION = ".pdf";
        private const string CDN_WEB_PATH = "/cdn";  // Web'den erişim için relative path

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünün varlığını kontrol et
                if (!Directory.Exists(CDN_PATH))
                {
                    throw new DirectoryNotFoundException(string.Format("CDN klasörü bulunamadı: {0}", CDN_PATH));
                }

                // URL'den dosya yolu parametresini kontrol et ve hidden field'a ata
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
                    
                    // Dosya uzantısı kontrolü
                    if (!fileName.EndsWith(ALLOWED_EXTENSION, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(string.Format("Sadece {0} dosyaları yüklenebilir.", ALLOWED_EXTENSION));
                    }

                    // Benzersiz dosya adı oluştur (timestamp_filename.pdf)
                    string uniqueFileName = string.Format("{0}_{1}", 
                        DateTime.Now.Ticks.ToString(), 
                        fileName);

                    // CDN klasöründe tam dosya yolu oluştur
                    string cdnFilePath = Path.Combine(CDN_PATH, uniqueFileName);
                    
                    // Dosyayı CDN klasörüne kaydet
                    fuPdfUpload.SaveAs(cdnFilePath);

                    // Web erişimi için relative path oluştur
                    string webPath = string.Format("{0}/{1}", 
                        CDN_WEB_PATH, 
                        uniqueFileName);
                    
                    // Client-side script ile dosya listesini güncelle
                    string script = string.Format(@"
                        fileList.push('{0}');
                        updateFileList();
                        viewFile('{0}');
                        showNotification('Dosya başarıyla yüklendi', 'success');
                    ", webPath);
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "uploadSuccess", script, true);
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda kullanıcıya bildir
                string errorScript = string.Format(
                    "showNotification('{0}', 'error');", 
                    HttpUtility.JavaScriptStringEncode(ex.Message)
                );
                ScriptManager.RegisterStartupScript(this, GetType(), "uploadError", errorScript, true);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object DeleteFile(string filePath)
        {
            try
            {
                // Web path'i dosya sistemindeki path'e çevir
                string fileName = Path.GetFileName(filePath);
                string fullPath = Path.Combine(CDN_PATH, fileName);

                // Güvenlik kontrolü - sadece CDN klasöründeki dosyaların silinmesine izin ver
                if (!fullPath.StartsWith(CDN_PATH, StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Geçersiz dosya yolu. Sadece CDN klasöründeki dosyalar silinebilir.");
                }

                // Dosyayı sil ve sonucu döndür
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