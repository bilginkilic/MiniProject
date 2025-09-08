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
        // Intranet ortamında fiziksel adres kullanıyoruz

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

                    // Önceki bir dosya yüklenmişse ve iptal edilmemişse silinmeyecek

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
                    string fileUrl = string.Format("file://{0}", pdfPath.Replace("\\", "/").TrimStart('/'));
                    pdfViewer.Attributes["src"] = fileUrl;
                    btnSave.Visible = true;
                    btnCancel.Visible = true;
                    
                    // Debug logları
                    Debug.WriteLine(string.Format("PDF fiziksel yol: {0}", pdfPath));
                    Debug.WriteLine(string.Format("PDF file URL: {0}", fileUrl));

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
