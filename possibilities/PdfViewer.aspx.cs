/* v4 - Created: 2024.01.17 - Kaydet/İptal mantığı eklendi */

using System;
using System.IO;
using System.Linq;
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

                // Mevcut PDF dosyalarını yükle
                LoadExistingPdfs();

                // Silinecek dosya varsa Kaydet/Vazgeç butonlarını görünür yap
                if (SessionHelper.HasPendingDeletes())
                {
                    btnSave.Visible = true;
                    btnCancel.Visible = true;
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

                    // PDF listesine ekle (en üste ekle)
                    AddPdfToList(uniqueFileName, pdfPath, true);
                    
                    btnSave.Visible = true;
                    btnCancel.Visible = true;
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
                // Session'daki silinecek dosyaları fiziksel olarak sil
                var pendingDeletes = SessionHelper.GetPendingDeletes();
                if (pendingDeletes != null && pendingDeletes.Count > 0)
                {
                    int deletedCount = 0;
                    foreach (var fileName in pendingDeletes)
                    {
                        try
                        {
                            string pdfPath = Path.Combine(_cdn, fileName);
                            if (File.Exists(pdfPath))
                            {
                                File.Delete(pdfPath);
                                deletedCount++;
                                Debug.WriteLine(string.Format("PDF dosyası silindi: {0}", pdfPath));
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(string.Format("PDF silme hatası ({0}): {1}", fileName, ex.Message));
                        }
                    }
                    
                    // Session'daki silinecek dosyalar listesini temizle
                    SessionHelper.ClearPendingDeletes();
                    
                    Debug.WriteLine(string.Format("Toplam {0} PDF dosyası silindi.", deletedCount));
                }

                // Listedeki tüm PDF dosyalarını al (silinenler hariç)
                var pdfListResult = GetCurrentPdfList();
                
                if (pdfListResult.PdfFiles.Count == 0)
                {
                    ShowError("Kaydedilecek PDF dosyası bulunamadı.");
                    return;
                }

                // Session'a PDF listesini kaydet
                SessionHelper.SetPdfList(pdfListResult);

                // Geriye uyumluluk için son yüklenen dosyayı da kaydet
                string lastPdfPath = SessionHelper.GetUploadedPdfPath();
                if (!string.IsNullOrEmpty(lastPdfPath) && File.Exists(lastPdfPath))
                {
                    SessionHelper.SetSelectedPdfFileName(lastPdfPath);
                    
                    var uploadResult = new UploadedFileResult
                    {
                        FilePath = lastPdfPath,
                        FileName = Path.GetFileName(lastPdfPath),
                        UploadDate = DateTime.Now
                    };
                    SessionHelper.SetUploadedFile(uploadResult);
                }

                Debug.WriteLine(string.Format("Toplam {0} PDF dosyası kaydedildi.", pdfListResult.PdfFiles.Count));
                SessionHelper.SetCloseWindow(true);
                
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
                // Session'daki silinecek dosyalar listesini temizle (hiçbir dosya fiziksel olarak silinmeyecek)
                SessionHelper.ClearPendingDeletes();

                // Son yüklenen dosyayı sil (eğer varsa) - bu sadece yeni yüklenen dosya için geçerli
                string lastPdfPath = SessionHelper.GetUploadedPdfPath();
                if (!string.IsNullOrEmpty(lastPdfPath) && File.Exists(lastPdfPath))
                {
                    File.Delete(lastPdfPath);
                    Debug.WriteLine(string.Format("Son yüklenen PDF dosyası silindi: {0}", lastPdfPath));
                }

                // Session'daki tüm PDF verilerini temizle
                SessionHelper.ClearAllPdfData();

                // Kaydet/İptal butonlarını gizle
                btnSave.Visible = false;
                btnCancel.Visible = false;

                // Sayfayı yenile (liste güncellensin, işaretlemeler kalkacak)
                Response.Redirect(Request.RawUrl);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("PDF silme hatası: {0}", ex.Message));
                ShowError(string.Format("PDF silinirken bir hata oluştu: {0}", ex.Message));
            }
        }

        private void AddPdfToList(string fileName, string filePath, bool addToTop = false, bool isPendingDelete = false)
        {
            // PDF listesi öğesi oluştur
            var pdfItem = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
            pdfItem.Attributes["class"] = "pdf-item";
            if (isPendingDelete)
            {
                pdfItem.Attributes["class"] += " pending-delete";
            }
            pdfItem.Attributes["data-filename"] = fileName;

            // PDF adı ve link
            var nameLink = new System.Web.UI.HtmlControls.HtmlAnchor();
            nameLink.Attributes["class"] = "pdf-item-name";
            nameLink.HRef = "javascript:void(0);";
            nameLink.Attributes["onclick"] = string.Format("openPdfInNewTab('{0}');", HttpUtility.JavaScriptStringEncode(fileName));
            nameLink.InnerText = fileName;
            pdfItem.Controls.Add(nameLink);

            // Butonlar için container
            var actions = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
            actions.Attributes["class"] = "pdf-item-actions";

            // Önizleme butonu
            var previewButton = new System.Web.UI.HtmlControls.HtmlButton();
            previewButton.Attributes["type"] = "button";
            previewButton.Attributes["class"] = "pdf-item-button preview";
            previewButton.Attributes["onclick"] = string.Format("previewPdf('{0}');", HttpUtility.JavaScriptStringEncode(fileName));
            previewButton.InnerText = "Önizle";
            actions.Controls.Add(previewButton);

            // Silme butonu
            var deleteButton = new System.Web.UI.HtmlControls.HtmlButton();
            deleteButton.Attributes["type"] = "button";
            deleteButton.Attributes["class"] = "pdf-item-button delete";
            deleteButton.Attributes["onclick"] = string.Format("deletePdf('{0}', this);", HttpUtility.JavaScriptStringEncode(fileName));
            deleteButton.InnerText = "Sil";
            actions.Controls.Add(deleteButton);

            pdfItem.Controls.Add(actions);
            
            // En üste ekle veya normal sırayla ekle
            if (addToTop && pdfList.Controls.Count > 0)
            {
                pdfList.Controls.AddAt(0, pdfItem);
            }
            else
            {
                pdfList.Controls.Add(pdfItem);
            }
        }

        [System.Web.Services.WebMethod]
        public static object DeletePdf(string fileName)
        {
            try
            {
                // ÖNEMLİ: Dosyayı fiziksel olarak silmeyecek, sadece Session'daki array'e ekleyecek
                var pendingDeletes = SessionHelper.GetPendingDeletes();
                if (pendingDeletes == null)
                {
                    pendingDeletes = new System.Collections.Generic.List<string>();
                }
                
                if (!pendingDeletes.Contains(fileName))
                {
                    pendingDeletes.Add(fileName);
                    SessionHelper.SetPendingDeletes(pendingDeletes);
                }
                
                return new { success = true, pendingDeletes = pendingDeletes.ToArray() };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private void LoadExistingPdfs()
        {
            try
            {
                // Session'daki silinecek dosyaları kontrol et
                var pendingDeletes = SessionHelper.GetPendingDeletes();
                
                // CDN klasöründeki tüm PDF dosyalarını al
                if (Directory.Exists(_cdn))
                {
                    var pdfFiles = Directory.GetFiles(_cdn, "*.pdf")
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .ToArray();

                    if (pdfFiles.Length > 0)
                    {
                        foreach (var pdfFile in pdfFiles)
                        {
                            string fileName = Path.GetFileName(pdfFile);
                            bool isPendingDelete = pendingDeletes != null && pendingDeletes.Contains(fileName);
                            AddPdfToList(fileName, pdfFile, false, isPendingDelete);
                        }
                        
                        Debug.WriteLine(string.Format("Toplam {0} PDF dosyası yüklendi.", pdfFiles.Length));
                    }
                    else
                    {
                        // Hiç PDF dosyası yoksa bilgi mesajı göster
                        var noFilesMessage = new System.Web.UI.HtmlControls.HtmlGenericControl("div");
                        noFilesMessage.Attributes["class"] = "pdf-item";
                        noFilesMessage.InnerText = "Henüz yüklenmiş PDF dosyası bulunmuyor.";
                        pdfList.Controls.Add(noFilesMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Mevcut PDF dosyaları yüklenirken hata: {0}", ex.Message));
                ShowError("Mevcut PDF dosyaları yüklenirken bir hata oluştu.");
            }
        }

        private PdfListResult GetCurrentPdfList()
        {
            var result = new PdfListResult();
            
            try
            {
                // CDN klasöründeki mevcut PDF dosyalarını al
                if (Directory.Exists(_cdn))
                {
                    var pdfFiles = Directory.GetFiles(_cdn, "*.pdf")
                        .OrderByDescending(f => File.GetCreationTime(f))
                        .ToArray();

                    foreach (var pdfFile in pdfFiles)
                    {
                        var pdfInfo = new PdfFileInfo
                        {
                            FileName = Path.GetFileName(pdfFile),
                            FilePath = pdfFile,
                            UploadDate = File.GetCreationTime(pdfFile)
                        };
                        result.PdfFiles.Add(pdfInfo);
                    }
                }
                
                Debug.WriteLine(string.Format("Kaydetme için {0} PDF dosyası bulundu.", result.PdfFiles.Count));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("PDF listesi alınırken hata: {0}", ex.Message));
                ShowError("PDF listesi alınırken bir hata oluştu.");
            }
            
            return result;
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
