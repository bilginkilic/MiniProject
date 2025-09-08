/* v4 - Created: 2024.01.17 - Kaydet/İptal mantığı eklendi */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using AspxExamples.Common.Models;

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
                        Debug.WriteLine(string.Format("CDN klasörü oluşturuldu: {0}", _cdn));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("CDN klasörü oluşturma hatası: {0}", ex.Message));
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
                    string uniqueFileName = string.Format("{0}_{1}.pdf", Path.GetFileNameWithoutExtension(fileName), timestamp);
                    string pdfPath = Path.Combine(_cdn, uniqueFileName);
                    
                    // Dosyayı kaydet
                    fuPdf.SaveAs(pdfPath);
                    Debug.WriteLine(string.Format("PDF dosyası kaydedildi: {0}", pdfPath));

                    // Session'a kaydet
                    SessionHelper.SetUploadedPdfPath(pdfPath);

                    // PDF'i iframe'de göster ve butonları aktifleştir
                    string virtualPath = string.Format("{0}/{1}", _cdnVirtualPath, uniqueFileName);
                    pdfViewer.Attributes["src"] = virtualPath;
                    btnSave.Visible = true;
                    btnCancel.Visible = true;
                    Debug.WriteLine(string.Format("PDF önizleme için yüklendi: {0}", pdfPath));

                    ShowMessage("PDF dosyası yüklendi. Kaydetmek için 'Kaydet' butonuna basın.", "info");
                }
                else
                {
                    ShowError("Lütfen bir PDF dosyası seçiniz.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Dosya yükleme hatası: {0}", ex.Message));
                ShowError(string.Format("Dosya yüklenirken bir hata oluştu: {0}", ex.Message));
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
                        Debug.WriteLine(string.Format("Eski dosya silindi: {0}", file.Name));
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(string.Format("Dosya silme hatası: {0} - {1}", file.Name, ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Dosya temizleme hatası: {0}", ex.Message));
            }
        }

        private void ShowError(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                string.Format("showNotification('{0}', 'error');", HttpUtility.JavaScriptStringEncode(message)),
                true);
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string lastPdfPath = SessionHelper.GetUploadedPdfPath();
                if (string.IsNullOrEmpty(lastPdfPath) || !File.Exists(lastPdfPath))
                {
                    ShowError("PDF dosyası bulunamadı.");
                    return;
                }

                // Session'a filename'i kaydet
                SessionHelper.SetSelectedPdfFileName(Path.GetFileName(lastPdfPath));
                Debug.WriteLine(string.Format("PDF dosyası kaydedildi: {0}", lastPdfPath));

                // Sayfayı kapat/geri dön
                ScriptManager.RegisterStartupScript(this, GetType(), "closeScript", 
                    "if (window.opener && !window.opener.closed) { window.close(); } else { window.location.href = document.referrer; }", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("PDF kaydetme hatası: {0}", ex.Message));
                ShowError(string.Format("PDF kaydedilirken bir hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                string lastPdfPath = SessionHelper.GetUploadedPdfPath();
                if (!string.IsNullOrEmpty(lastPdfPath) && File.Exists(lastPdfPath))
                {
                    File.Delete(lastPdfPath);
                    Debug.WriteLine(string.Format("PDF dosyası silindi: {0}", lastPdfPath));
                }

                // Session'daki değerleri temizle
                SessionHelper.ClearPdfData();

                // Sayfayı kapat/geri dön
                ScriptManager.RegisterStartupScript(this, GetType(), "closeScript", 
                    "if (window.opener && !window.opener.closed) { window.close(); } else { window.location.href = document.referrer; }", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("PDF silme hatası: {0}", ex.Message));
                ShowError(string.Format("PDF silinirken bir hata oluştu: {0}", ex.Message));
            }
        }

        private void ShowMessage(string message, string type = "info")
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                string.Format("showNotification('{0}', '{1}');", HttpUtility.JavaScriptStringEncode(message), type),
                true);
        }
    }
}
