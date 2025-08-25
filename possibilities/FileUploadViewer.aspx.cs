/* v11 - Created: 2024.01.17 - Fixed physical path issue */

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
        // CDN klasör yolu - fiziksel yol
        private const string CDN_PATH = @"\\trrgap3027\files\circular\cdn";
        // Web'den erişim için virtual path
        private const string CDN_VIRTUAL_PATH = "http://trrgap3027/circular/cdn";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!Directory.Exists(CDN_PATH))
                {
                    try 
                    {
                        Directory.CreateDirectory(CDN_PATH);
                    }
                    catch (Exception ex)
                    {
                        ShowError("Sistem hazırlığı sırasında bir hata oluştu. Lütfen yöneticinize başvurun.");
                        return;
                    }
                }

                string filePath = Request.QueryString["file"];
                if (!string.IsNullOrEmpty(filePath))
                {
                    hdnCurrentFile.Value = filePath;
                }

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
                    
                    if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("Sadece PDF dosyaları yüklenebilir.");
                    }

                    // Benzersiz dosya adı oluştur
                    string uniqueFileName = string.Format("{0}_{1}", 
                        DateTime.Now.Ticks.ToString(), 
                        fileName);

                    // CDN klasörüne kaydet
                    string pdfPath = Path.Combine(CDN_PATH, uniqueFileName);
                    fuPdfUpload.SaveAs(pdfPath);

                    System.Diagnostics.Debug.WriteLine(string.Format("Dosya kaydedildi: {0}", pdfPath));

                    // Web URL'i oluştur
                    string webUrl = string.Format("{0}/{1}", 
                        CDN_VIRTUAL_PATH.TrimEnd('/'), 
                        uniqueFileName);

                    System.Diagnostics.Debug.WriteLine(string.Format("Web URL: {0}", webUrl));
                    
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
                System.Diagnostics.Debug.WriteLine(string.Format("Hata: {0}", ex.Message));
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
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new Exception("Dosya yolu belirtilmedi.");
                }

                // URL'den dosya adını al
                string fileName = filePath.Substring(filePath.LastIndexOf('/') + 1);
                
                // Fiziksel yolu kontrol et
                string fullPath = Path.Combine(CDN_PATH, fileName);
                if (!File.Exists(fullPath))
                {
                    throw new Exception("Dosya bulunamadı.");
                }

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
                
                // CDN klasöründeki tam yolu oluştur
                string fullPath = Path.Combine(CDN_PATH, fileName);

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