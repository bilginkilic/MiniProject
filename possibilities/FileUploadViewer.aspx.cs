using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Web.Script.Services;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AspxExamples
{
    public partial class FileUploadViewer : System.Web.UI.Page
    {
        private string _cdn = @"\\trrgap3027\files\circular\cdn";
        private string _cdnVirtualPath = "/cdn"; // Web'den erişim için virtual path

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünü kontrol et
                if (!Directory.Exists(_cdn))
                {
                    try
                    {
                        Directory.CreateDirectory(_cdn);
                        System.Diagnostics.Debug.WriteLine($"CDN klasörü oluşturuldu: {_cdn}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CDN klasörü oluşturma hatası: {ex.Message}");
                        ShowError("Sistem hazırlığı sırasında bir hata oluştu. Lütfen yöneticinize başvurun.");
                        return;
                    }
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
                    if (Path.GetExtension(fileName).ToLower() != ".pdf")
                    {
                        ShowError("Lütfen sadece PDF formatında dosya yükleyiniz.");
                        return;
                    }

                    // Önceki dosyaları temizle
                    CleanupOldFiles();

                    string pdfPath = Path.Combine(_cdn, fileName);
                    fuPdfUpload.SaveAs(pdfPath);

                    // JavaScript'e dosya yolunu gönder
                    var fileData = new { filePath = pdfPath };
                    var script = $"fileList.push('{pdfPath.Replace("\\", "\\\\")}'); viewFile('{pdfPath.Replace("\\", "\\\\")}'); enableSaveButton();";
                    ScriptManager.RegisterStartupScript(this, GetType(), "uploadComplete", script, true);

                    ShowMessage("Dosya başarıyla yüklendi.", "success");
                }
                else
                {
                    ShowError("Lütfen bir dosya seçiniz.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Dosya yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        private void CleanupOldFiles()
        {
            try
            {
                // Tüm PDF dosyalarını temizle
                foreach (string file in Directory.GetFiles(_cdn, "*.pdf"))
                {
                    try { File.Delete(file); } catch { }
                }

                System.Diagnostics.Debug.WriteLine("Eski dosyalar temizlendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dosya temizleme hatası: {ex.Message}");
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return new { success = true };
                }
                return new { success = false, message = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object SaveAndReturn(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return new { success = true };
                }
                return new { success = false, error = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private void ShowError(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                $"showNotification('{HttpUtility.JavaScriptStringEncode(message)}', 'error');",
                true);
        }

        private void ShowMessage(string message, string type = "info")
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                $"showNotification('{HttpUtility.JavaScriptStringEncode(message)}', '{type}');",
                true);
        }
    }
}