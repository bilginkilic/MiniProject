using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace AspxExamples
{
    public partial class PdfSignatureForm : System.Web.UI.Page
    {
        private readonly string _cdn = Path.Combine(HttpRuntime.AppDomainAppPath, "cdn");
        protected HiddenField hdnPageCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünü oluştur
                if (!Directory.Exists(_cdn))
                {
                    Directory.CreateDirectory(_cdn);
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
                    foreach (string file in Directory.GetFiles(_cdn, "page_*.png"))
                    {
                        try { File.Delete(file); } catch { }
                    }

                    string pdfPath = Path.Combine(_cdn, fileName);
                    fileUpload.SaveAs(pdfPath);

                    btnShowPdf.Enabled = true;
                    Session["LastUploadedPdf"] = pdfPath;

                    ShowMessage("İmza sirküleri başarıyla yüklendi. Şimdi 'İmza Sirküleri Göster' butonuna tıklayarak devam edebilirsiniz.", "success");
                }
                else
                {
                    ShowError("Lütfen bir PDF dosyası seçiniz.");
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("Dosya yüklenirken bir hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnShowPdf_Click(object sender, EventArgs e)
        {
            string pdfPath = Session["LastUploadedPdf"] as string;
            if (string.IsNullOrEmpty(pdfPath))
            {
                ShowError("Lütfen önce bir imza sirkülerini yükleyiniz.");
                return;
            }

            try
            {
                // PDF'yi PNG'ye çevir
                System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü başlıyor. PDF yolu: {0}", pdfPath));
                
                int pageCount = PdfToImageAndCrop.ConvertPdfToImages(pdfPath, _cdn);
                System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü tamamlandı. Sayfa sayısı: {0}", pageCount));

                // Her sayfanın oluşturulduğunu kontrol et
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
                        var fileInfo = new FileInfo(imagePath);
                        System.Diagnostics.Debug.WriteLine(String.Format("Sayfa oluşturuldu: {0}, Boyut: {1} bytes", imagePath, fileInfo.Length));
                    }
                }

                if (!allPagesExist)
                {
                    ShowError("PDF sayfaları dönüştürülürken bir hata oluştu. Lütfen tekrar deneyiniz.");
                    return;
                }

                // Sayfa sayısını hidden field'a kaydet
                hdnPageCount.Value = pageCount.ToString();
                
                // JavaScript'e sayfa sayısını gönder
                ScriptManager.RegisterStartupScript(this, GetType(),
                    "initTabs",
                    String.Format("initializeTabs({0});", pageCount),
                    true);

                ShowMessage("İmza sirkülerini görüntüleniyor. İmza alanını seçmek için tıklayıp sürükleyin.", "info");
                btnSaveSignature.Enabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü hatası: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace));
                ShowError(String.Format("İmza sirkülerini görüntülerken bir hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            try
            {
                string selectionData = hdnSelection.Value;
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

                    if (width <= 0 || height <= 0)
                    {
                        ShowError("Geçersiz seçim boyutları. Lütfen tekrar seçim yapınız.");
                        return;
                    }

                    string imagePath = Path.Combine(_cdn, String.Format("page_{0}.png", page));
                    if (!File.Exists(imagePath))
                    {
                        ShowError("Seçilen sayfanın görüntüsü bulunamadı. Lütfen sayfayı yenileyin.");
                        return;
                    }

                    Rectangle selectionRect = new Rectangle(x, y, width, height);

                    using (var sourceImage = new Bitmap(imagePath))
                    {
                        // Seçim koordinatlarını orijinal resim boyutuna göre ölçekle
                        float scaleX = (float)sourceImage.Width / sourceImage.Width;
                        float scaleY = (float)sourceImage.Height / sourceImage.Height;

                        Rectangle cropRect = new Rectangle(
                            (int)(selectionRect.X * scaleX),
                            (int)(selectionRect.Y * scaleY),
                            (int)(selectionRect.Width * scaleX),
                            (int)(selectionRect.Height * scaleY)
                        );

                        // Kırpma alanının resim sınırları içinde olduğunu kontrol et
                        if (cropRect.X < 0 || cropRect.Y < 0 ||
                            cropRect.Right > sourceImage.Width ||
                            cropRect.Bottom > sourceImage.Height)
                        {
                            ShowError("Seçilen alan resim sınırları dışında. Lütfen tekrar seçim yapınız.");
                            return;
                        }

                        string outputFileName = String.Format("signature_{0}.png", DateTime.Now.Ticks);
                        string outputPath = Path.Combine(_cdn, outputFileName);

                        // Kırpma işlemi
                        using (var cropImage = new Bitmap(cropRect.Width, cropRect.Height, PixelFormat.Format32bppArgb))
                        {
                            cropImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                            using (var g = Graphics.FromImage(cropImage))
                            {
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                g.DrawImage(sourceImage, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                                          cropRect, GraphicsUnit.Pixel);
                            }

                            // Dosyayı kaydet
                            cropImage.Save(outputPath, ImageFormat.Png);

                            // Dosyanın oluşturulduğunu kontrol et
                            if (File.Exists(outputPath))
                            {
                                ShowMessage(String.Format("İmza başarıyla kaydedildi: {0}. Yeni bir seçim yapmak için görüntü üzerine tıklayabilirsiniz.",
                                    outputFileName), "success");

                                // Debug bilgisi
                                Debug.WriteLine(String.Format("İmza dosyası oluşturuldu: {0}", outputPath));
                                Debug.WriteLine(String.Format("Dosya boyutu: {0} bytes", new FileInfo(outputPath).Length));
                            }
                            else
                            {
                                ShowError("İmza dosyası oluşturulamadı. Lütfen tekrar deneyiniz.");
                            }
                        }
                    }
                }
                else
                {
                    ShowError("Seçim verileri geçersiz. Lütfen tekrar seçim yapınız.");
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message));
                Debug.WriteLine(String.Format("Hata detayı: {0}", ex.ToString()));
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