/* v1 - FileUploadViewer.aspx.cs - PDF Dosya Yükleme ve Görüntüleme */

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
                        System.Diagnostics.Debug.WriteLine(String.Format("CDN klasörü oluşturuldu: {0}", _cdn));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("CDN klasörü oluşturma hatası: {0}", ex.Message));
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
                    var script = String.Format("fileList.push('{0}'); viewFile('{0}'); enableSaveButton();", pdfPath.Replace("\\", "\\\\"));
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
                ShowError(String.Format("Dosya yüklenirken bir hata oluştu: {0}", ex.Message));
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
                System.Diagnostics.Debug.WriteLine(String.Format("Dosya temizleme hatası: {0}", ex.Message));
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

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = false)]
        public static object SaveSignatureWithAjax(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // SignatureAuthData nesnesini oluştur
                    var signatureData = new SignatureAuthData
                    {
                        KaynakPdfAdi = Path.GetFileName(filePath),
                        Yetkililer = new List<YetkiliData>()
                    };

                    // Session'a kaydet
                    SessionHelper.SetSignatureAuthData(signatureData);

                    return new { success = true, message = "İmza bilgileri kaydedildi" };
                }
                return new { success = false, error = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        [System.Web.Services.WebMethod]
        [System.Web.Script.Services.ScriptMethod(ResponseFormat = ResponseFormat.Json, UseHttpGet = false)]
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
                String.Format("showNotification('{0}', 'error');", HttpUtility.JavaScriptStringEncode(message)),
                true);
        }

        private void ShowMessage(string message, string type = "info")
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                String.Format("showNotification('{0}', '{1}');", HttpUtility.JavaScriptStringEncode(message), type),
                true);
        }
    }
}