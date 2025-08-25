/* v7 - Created: 2024.01.17 - Fixed CDN URL */

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
        private const string CDN_WEB_PATH = "http://trrgap3027/circular/cdn";  // Tam URL olarak CDN adresi

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünün varlığını kontrol et
                if (!Directory.Exists(CDN_PATH))
                {
                    try 
                    {
                        Directory.CreateDirectory(CDN_PATH);
                        System.Diagnostics.Debug.WriteLine(string.Format("CDN klasörü oluşturuldu: {0}", CDN_PATH));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("CDN klasörü oluşturma hatası: {0}", ex.Message));
                        ShowError("Sistem hazırlığı sırasında bir hata oluştu. Lütfen yöneticinize başvurun.");
                        return;
                    }
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

                    // Web erişimi için tam URL oluştur
                    string webUrl = string.Format("{0}/{1}", 
                        CDN_WEB_PATH.TrimEnd('/'), 
                        uniqueFileName);

                    System.Diagnostics.Debug.WriteLine(string.Format("Dosya yüklendi. URL: {0}", webUrl));
                    
                    // Client-side script ile dosya listesini güncelle ve kaydet butonunu aktif et
                    string script = string.Format(@"
                        fileList.push('{0}');
                        updateFileList();
                        viewFile('{0}');
                        enableSaveButton();
                        showNotification('Dosya başarıyla yüklendi', 'success');
                    ", webUrl);
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "uploadSuccess", script, true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Dosya yükleme hatası: {0}", ex.Message));
                
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

                // URL'den dosya adını al
                string fileName = filePath.Substring(filePath.LastIndexOf('/') + 1);
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
                // URL'den dosya adını al
                string fileName = filePath.Substring(filePath.LastIndexOf('/') + 1);
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

        private void ShowError(string message)
        {
            string script = string.Format(
                "showNotification('{0}', 'error');",
                HttpUtility.JavaScriptStringEncode(message)
            );
            ScriptManager.RegisterStartupScript(this, GetType(), "error", script, true);
        }
    }
}