using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace AspxExamples
{
    public partial class PdfViewer : System.Web.UI.Page
    {
        private readonly string _cdn = @"\\trrgap3027\files\circular\cdn";
        private readonly string _cdnVirtualPath = "/cdn";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünü kontrol et ve oluştur
                if (!Directory.Exists(_cdn))
                {
                    try
                    {
                        Directory.CreateDirectory(_cdn);
                        Debug.WriteLine($"CDN klasörü oluşturuldu: {_cdn}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"CDN klasörü oluşturma hatası: {ex.Message}");
                        ShowError("Sistem hazırlığı sırasında bir hata oluştu. Lütfen yöneticinize başvurun.");
                        return;
                    }
                }

                // Başlangıç mesajını göster
                ShowMessage("PDF dosyanızı yükleyerek başlayabilirsiniz.", "info");
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (fuPdf.HasFile)
                {
                    string fileName = Path.GetFileName(fuPdf.FileName);
                    if (Path.GetExtension(fileName).ToLower() != ".pdf")
                    {
                        ShowError("Lütfen sadece PDF formatında dosya yükleyiniz.");
                        return;
                    }

                    // Önceki dosyaları temizle
                    CleanupOldFiles();

                    // Yeni dosya adı oluştur (timestamp ekleyerek)
                    string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    string uniqueFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_{timestamp}.pdf";
                    string pdfPath = Path.Combine(_cdn, uniqueFileName);
                    
                    // Dosyayı kaydet
                    fuPdf.SaveAs(pdfPath);
                    Debug.WriteLine($"PDF dosyası kaydedildi: {pdfPath}");

                    // Session'a kaydet
                    Session["LastUploadedPdf"] = pdfPath;

                    // PDF'i görüntüle
                    string virtualPath = $"{_cdnVirtualPath}/{uniqueFileName}";
                    pdfViewer.Attributes["src"] = virtualPath;
                    Debug.WriteLine($"PDF görüntüleme için virtual path: {virtualPath}");

                    ShowMessage("PDF dosyası başarıyla yüklendi.", "success");
                }
                else
                {
                    ShowError("Lütfen bir PDF dosyası seçiniz.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dosya yükleme hatası: {ex.Message}");
                ShowError($"Dosya yüklenirken bir hata oluştu: {ex.Message}");
            }
        }

        private void CleanupOldFiles()
        {
            try
            {
                // 24 saatten eski PDF dosyalarını temizle
                var directory = new DirectoryInfo(_cdn);
                var oldFiles = directory.GetFiles("*.pdf")
                    .Where(f => f.CreationTime < DateTime.Now.AddHours(-24));

                foreach (var file in oldFiles)
                {
                    try 
                    { 
                        file.Delete();
                        Debug.WriteLine($"Eski dosya silindi: {file.Name}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Dosya silme hatası: {file.Name} - {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Dosya temizleme hatası: {ex.Message}");
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
