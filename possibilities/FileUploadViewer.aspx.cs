/* v4 - Created: 2024.01.17 - Added save and return functionality */

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

                // Kaydet butonunu devre dışı bırak
                btnSave.Enabled = false;
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
                    
                    // Client-side script ile dosya listesini güncelle ve kaydet butonunu aktif et
                    string script = string.Format(@"
                        fileList.push('{0}');
                        updateFileList();
                        viewFile('{0}');
                        enableSaveButton();
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
        public static object SaveAndReturn(string filePath)
        {
            try
            {
                // Dosya yolunu kontrol et
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new Exception("Dosya yolu belirtilmedi.");
                }

                // Dosyanın CDN klasöründe olduğunu kontrol et
                string fileName = Path.GetFileName(filePath);
                string fullPath = Path.Combine(CDN_PATH, fileName);

                if (!File.Exists(fullPath))
                {
                    throw new Exception("Dosya bulunamadı.");
                }

                // Başarılı yanıt döndür
                return new { 
                    success = true, 
                    filePath = filePath 
                };
            }
            catch (Exception ex)
            {
                return new { 
                    success = false, 
                    error = ex.Message 
                };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object DeleteFile(string filePath)
        {
            try
            {
                // Güvenlik kontrolü - sadece CDN klasöründeki dosyaların silinmesine izin ver
                string normalizedPath = filePath.Replace('/', Path.DirectorySeparatorChar);
                if (!normalizedPath.StartsWith("/cdn/", StringComparison.OrdinalIgnoreCase))
                {
                    throw new Exception("Geçersiz dosya yolu. Sadece CDN klasöründeki dosyalar silinebilir.");
                }

                // Dosya yolunu server path'e çevir
                string fileName = Path.GetFileName(filePath);
                string fullPath = Path.Combine(CDN_PATH, fileName);

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