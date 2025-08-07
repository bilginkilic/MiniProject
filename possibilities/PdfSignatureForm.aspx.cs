using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace AspxExamples
{
    public class SignatureData
    {
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Image { get; set; }
        public string SourcePdfPath { get; set; }
    }

    public class SavedSignature
    {
        public string Path { get; set; }
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public partial class PdfSignatureForm : System.Web.UI.Page
    {
        private string _cdn = @"\\trrgap3027\files\circular\cdn";
        private string _cdnVirtualPath = "/cdn"; // Web'den erişim için virtual path

        protected HiddenField hdnPageCount;

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

                // Başlangıç mesajını göster
                ShowMessage("PDF formatında imza sirkülerinizi yükleyerek başlayabilirsiniz.", "info");
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (fileUpload.HasFile)
                {
                    string fileName = Path.GetFileName(fileUpload.FileName);
                    if (Path.GetExtension(fileName).ToLower() != ".pdf")
                    {
                        ShowError("Lütfen sadece PDF formatında dosya yükleyiniz.");
                        return;
                    }

                    // Önceki dosyaları temizle
                    CleanupOldFiles();

                    string pdfPath = Path.Combine(_cdn, fileName);
                    fileUpload.SaveAs(pdfPath);
                    Session["LastUploadedPdf"] = pdfPath;

                    // PDF'i hemen göster
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü başlıyor. PDF yolu: {0}", pdfPath));
                        System.Diagnostics.Debug.WriteLine(String.Format("CDN klasörü: {0}", _cdn));
                        
                        // PDF'yi PNG'ye çevir
                        int pageCount = PdfToImageAndCrop.ConvertPdfToImages(pdfPath, _cdn);
                        System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü tamamlandı. Sayfa sayısı: {0}", pageCount));

                        // Her sayfanın oluşturulduğunu kontrol et ve base64'e çevir
                        var imageDataList = new System.Collections.Generic.List<string>();
                        bool allPagesExist = true;

                        for (int i = 1; i <= pageCount; i++)
                        {
                            string imagePath = Path.Combine(_cdn, String.Format("page_{0}.png", i));
                            if (!File.Exists(imagePath))
                            {
                                System.Diagnostics.Debug.WriteLine(String.Format("Sayfa bulunamadı: {0}", imagePath));
                                allPagesExist = false;
                                break;
                            }
                            else
                            {
                                try
                                {
                                    using (var image = System.Drawing.Image.FromFile(imagePath))
                                    using (var ms = new MemoryStream())
                                    {
                                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        byte[] imageBytes = ms.ToArray();
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        imageDataList.Add(String.Format("data:image/png;base64,{0}", base64String));
                                        
                                        System.Diagnostics.Debug.WriteLine(String.Format("Sayfa {0} base64'e çevrildi, Boyut: {1} bytes", i, imageBytes.Length));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(String.Format("Base64 dönüşüm hatası: {0}", ex.Message));
                                    allPagesExist = false;
                                    break;
                                }
                            }
                        }

                        if (!allPagesExist)
                        {
                            ShowError("PDF sayfaları dönüştürülürken bir hata oluştu. Lütfen tekrar deneyiniz.");
                            return;
                        }

                        // Sayfa sayısını hidden field'a kaydet
                        hdnPageCount.Value = pageCount.ToString();
                        
                        // JavaScript'e sayfa sayısını ve resim verilerini gönder
                        var imageDataJson = String.Format("[{0}]", String.Join(",", imageDataList.Select(x => String.Format("'{0}'", x))));
                        
                        ScriptManager.RegisterStartupScript(this, GetType(),
                            "initTabs",
                            String.Format("var imageDataList = {0}; console.log('Image data loaded, count:', {1}); initializeTabs({1});", 
                                imageDataJson, pageCount),
                            true);

                        ShowMessage("İmza sirküleri yüklendi ve görüntüleniyor. İmza alanını seçmek için tıklayıp sürükleyin.", "success");
                        btnSaveSignature.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü hatası: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace));
                        ShowError(String.Format("İmza sirkülerini görüntülerken bir hata oluştu: {0}", ex.Message));
                    }
                }
                else
                {
                    ShowError("Lütfen bir PDF dosyası seçiniz.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Dosya yükleme hatası: {0}", ex.Message));
                ShowError(String.Format("Dosya yüklenirken bir hata oluştu: {0}", ex.Message));
            }
        }

        private void CleanupOldFiles()
        {
            try
            {
                // Tüm PNG dosyalarını temizle
                foreach (string file in Directory.GetFiles(_cdn, "*.png"))
                {
                    try { File.Delete(file); } catch { }
                }

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



        protected void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            try
            {
                string signaturesJson = Request.Form["hdnSignatures"];
                System.Diagnostics.Debug.WriteLine($"İmza verileri alındı: {signaturesJson}");

                if (string.IsNullOrEmpty(signaturesJson))
                {
                    ShowError("Lütfen en az bir imza seçiniz.");
                    return;
                }

                var serializer = new JavaScriptSerializer();
                var signatures = serializer.Deserialize<List<SignatureData>>(signaturesJson);

                if (signatures == null || signatures.Count == 0)
                {
                    ShowError("Geçersiz imza verisi.");
                    return;
                }

                var savedSignatures = new List<SavedSignature>();

                foreach (var signature in signatures)
                {
                    string imagePath = Path.Combine(_cdn, $"page_{signature.Page}.png");
                    System.Diagnostics.Debug.WriteLine($"Kaynak resim yolu: {imagePath}");

                    if (!File.Exists(imagePath))
                    {
                        ShowError($"Sayfa {signature.Page} için görüntü bulunamadı.");
                        continue;
                    }

                    using (var sourceImage = new System.Drawing.Bitmap(imagePath))
                    {
                        if (signature.X < 0 || signature.Y < 0 || 
                            signature.X + signature.Width > sourceImage.Width || 
                            signature.Y + signature.Height > sourceImage.Height)
                        {
                            ShowError($"Sayfa {signature.Page} için seçilen alan resim sınırları dışında.");
                            continue;
                        }

                        string outputFileName = $"signature_{DateTime.Now.Ticks}_{signatures.IndexOf(signature)}.png";
                        string outputPath = Path.Combine(_cdn, outputFileName);

                        try
                        {
                            using (var bitmap = new System.Drawing.Bitmap(signature.Width, signature.Height))
                            {
                                bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                                {
                                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                    var sourceRect = new System.Drawing.Rectangle(signature.X, signature.Y, signature.Width, signature.Height);
                                    var destRect = new System.Drawing.Rectangle(0, 0, signature.Width, signature.Height);

                                    graphics.DrawImage(sourceImage, destRect, sourceRect, System.Drawing.GraphicsUnit.Pixel);
                                }

                                string tempPath = Path.Combine(_cdn, $"temp_{outputFileName}");
                                bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                                if (File.Exists(outputPath))
                                {
                                    File.Delete(outputPath);
                                }
                                File.Move(tempPath, outputPath);

                                savedSignatures.Add(new SavedSignature
                                {
                                    Path = _cdnVirtualPath + "/" + outputFileName,
                                    Page = signature.Page,
                                    X = signature.X,
                                    Y = signature.Y,
                                    Width = signature.Width,
                                    Height = signature.Height
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"İmza kaydetme hatası: {ex.Message}");
                            ShowError($"İmza kaydedilirken bir hata oluştu: {ex.Message}");
                            continue;
                        }
                    }
                }

                // İmzaları kaydet ve yanıt gönder
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var response = new {
                        success = savedSignatures.Count > 0,
                        signatures = savedSignatures,
                        message = savedSignatures.Count > 0 ? "İmzalar başarıyla kaydedildi" : "İmza kaydedilemedi"
                    };
                    
                    var jsonResponse = serializer.Serialize(response);
                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.Write(jsonResponse);
                    Response.Flush();
                    Response.Close();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    if (savedSignatures.Count > 0)
                    {
                        // Normal postback için yeni davranış
                        string virtualPath = savedSignatures[0].Path; // İlk imzayı kullan
                        ScriptManager.RegisterStartupScript(this, GetType(),
                            "saveSuccess",
                            String.Format(@"
                                if (window.opener && !window.opener.closed) {{
                                    if (typeof window.opener.handleSignatureReturn === 'function') {{
                                        window.opener.handleSignatureReturn('{0}');
                                    }}
                                }}
                                showNotification('İmzalar başarıyla kaydedildi', 'success');
                                setTimeout(function() {{ window.close(); }}, 1500);
                            ", virtualPath),
                            true);
                    }
                    else
                    {
                        ShowError("İmzalar kaydedilemedi. Lütfen tekrar deneyiniz.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"İmza kaydetme hatası: {ex.Message}\nStack Trace: {ex.StackTrace}");
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var response = new {
                        success = false,
                        error = $"İmza kaydedilirken bir hata oluştu: {ex.Message}"
                    };
                    
                    var serializer = new JavaScriptSerializer();
                    var jsonError = serializer.Serialize(response);
                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.Write(jsonError);
                    Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ShowError($"İmza kaydedilirken bir hata oluştu: {ex.Message}");
                }
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