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
                string selectionData = hdnSelection.Value;
                System.Diagnostics.Debug.WriteLine(String.Format("Seçim verisi alındı: {0}", selectionData));

                if (string.IsNullOrEmpty(selectionData))
                {
                    ShowError("Lütfen önce bir imza alanı seçiniz.");
                    return;
                }

                string[] parts = selectionData.Split(',');
                if (parts.Length >= 5) // page,x,y,width,height
                {
                    int page = int.Parse(parts[0]);
                    int x = int.Parse(parts[1]);
                    int y = int.Parse(parts[2]);
                    int width = int.Parse(parts[3]);
                    int height = int.Parse(parts[4]);

                    System.Diagnostics.Debug.WriteLine(String.Format("Seçim koordinatları: Sayfa={0}, X={1}, Y={2}, Genişlik={3}, Yükseklik={4}", 
                        page, x, y, width, height));

                    if (width <= 0 || height <= 0)
                    {
                        ShowError("Geçersiz seçim boyutları. Lütfen tekrar seçim yapınız.");
                        return;
                    }

                    string imagePath = Path.Combine(_cdn, String.Format("page_{0}.png", page));
                    System.Diagnostics.Debug.WriteLine(String.Format("Kaynak resim yolu: {0}", imagePath));

                    if (!File.Exists(imagePath))
                    {
                        ShowError("Seçilen sayfanın görüntüsü bulunamadı. Lütfen sayfayı yenileyin.");
                        return;
                    }

                    using (var sourceImage = new System.Drawing.Bitmap(imagePath))
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("Kaynak resim boyutları: {0}x{1}", sourceImage.Width, sourceImage.Height));

                        if (x < 0 || y < 0 || x + width > sourceImage.Width || y + height > sourceImage.Height)
                        {
                            ShowError("Seçilen alan resim sınırları dışında. Lütfen tekrar seçim yapınız.");
                            return;
                        }

                        string outputFileName = String.Format("signature_{0}.png", DateTime.Now.Ticks);
                        string outputPath = Path.Combine(_cdn, outputFileName);
                        System.Diagnostics.Debug.WriteLine(String.Format("Hedef resim yolu: {0}", outputPath));

                        try
                        {
                            // Kırpma işlemi
                            using (var bitmap = new System.Drawing.Bitmap(width, height))
                            {
                                bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                                {
                                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                    var sourceRect = new System.Drawing.Rectangle(x, y, width, height);
                                    var destRect = new System.Drawing.Rectangle(0, 0, width, height);

                                    System.Diagnostics.Debug.WriteLine(String.Format("Kırpma koordinatları: Kaynak={0}, Hedef={1}", 
                                        sourceRect.ToString(), destRect.ToString()));

                                    graphics.DrawImage(sourceImage, destRect, sourceRect, System.Drawing.GraphicsUnit.Pixel);
                                }

                                // Önce geçici dosyaya kaydet
                                string tempPath = Path.Combine(_cdn, String.Format("temp_{0}", outputFileName));
                                bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                                // Başarılı olursa asıl konuma taşı
                                if (File.Exists(outputPath))
                                {
                                    File.Delete(outputPath);
                                }
                                File.Move(tempPath, outputPath);

                                var fileInfo = new FileInfo(outputPath);
                                System.Diagnostics.Debug.WriteLine(String.Format("İmza kaydedildi: {0}, Boyut: {1} bytes", outputPath, fileInfo.Length));

                                                    // AJAX yanıtı gönder
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        var response = new {
                            success = true,
                            path = outputFileName,
                            message = "OK"
                        };
                        
                        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
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
                                    // Normal postback için eski davranış
                                    ScriptManager.RegisterStartupScript(this, GetType(),
                                        "saveSuccess",
                                        String.Format(@"
                                            showNotification('İmza başarıyla kaydedildi: {0}', 'success');
                                            if(typeof clearSelection === 'function') {{ clearSelection(); }}
                                            btnSaveSignature.disabled = false;
                                        ", outputFileName),
                                        true);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(String.Format("Dosya kaydetme hatası: {0}", ex.Message));
                            throw;
                        }
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(String.Format("Geçersiz seçim verisi parça sayısı: {0}", parts.Length));
                    ShowError("Seçim verileri geçersiz. Lütfen tekrar seçim yapınız.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("İmza kaydetme hatası: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace));
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var response = new {
                        success = false,
                        error = String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message)
                    };
                    
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    var jsonError = serializer.Serialize(response);
                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.Write(jsonError);
                    Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ShowError(String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message));
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