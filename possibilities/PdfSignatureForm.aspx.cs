using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Drawing.Imaging;

namespace AspxExamples
{
    public partial class PdfSignatureForm : System.Web.UI.Page
    {
        private readonly string _cdn = Path.Combine(HttpRuntime.AppDomainAppPath, "cdn");

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
                string imagePath = Path.Combine(_cdn, "page_1.png");
                
                // PDF'yi PNG'ye çevir
                PdfToImageAndCrop.ConvertPdfToImages(pdfPath, _cdn);

                if (File.Exists(imagePath))
                {
                    // Resmi göster
                    imgSignature.ImageUrl = String.Format("~/cdn/page_1.png?t={0}", DateTime.Now.Ticks);
                    Session["LastRenderedImage"] = imagePath;

                    ShowMessage("İmza sirkülerini görüntüleniyor. İmza alanını seçmek için tıklayıp sürükleyin.", "info");
                    btnSaveSignature.Enabled = true; // Kaydet butonu aktif olsun
                }
                else
                {
                    ShowError("İmza sirkülerini görüntülerken bir hata oluştu. Lütfen dosyanın geçerli bir PDF olduğundan emin olun.");
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("İmza sirkülerini görüntülerken bir hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            string imagePath = Session["LastRenderedImage"] as string;
            string selectionData = hdnSelection.Value;

            if (string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(selectionData))
            {
                ShowError("Lütfen önce bir imza alanı seçiniz.");
                return;
            }

            try
            {
                string[] parts = selectionData.Split(',');
                if (parts.Length >= 4)
                {
                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    int width = int.Parse(parts[2]);
                    int height = int.Parse(parts[3]);

                    if (width <= 0 || height <= 0)
                    {
                        ShowError("Geçersiz seçim boyutları. Lütfen tekrar seçim yapınız.");
                        return;
                    }

                    Rectangle selectionRect = new Rectangle(x, y, width, height);

                    // Optimize edilmiş resim işleme
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

                        // Performans için GraphicsUnit.Pixel kullan ve kalite ayarlarını optimize et
                        using (var cropImage = new Bitmap(cropRect.Width, cropRect.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                        {
                            cropImage.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                            using (var g = Graphics.FromImage(cropImage))
                            {
                                // Kalite ayarları
                                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                g.DrawImage(sourceImage, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                                          cropRect, GraphicsUnit.Pixel);
                            }

                            string outputFileName = String.Format("signature_{0}.png", DateTime.Now.Ticks);
                            string outputPath = Path.Combine(_cdn, outputFileName);

                            // PNG encoder ile optimize edilmiş kayıt
                            var pngEncoder = GetPngEncoder();
                            var encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                            cropImage.Save(outputPath, pngEncoder, encoderParams);

                            ShowMessage(String.Format("İmza başarıyla kaydedildi: {0}. Yeni bir seçim yapmak için görüntü üzerine tıklayabilirsiniz.", 
                                outputFileName), "success");
                        }
                    }
                }
                else
                {
                    ShowError("Seçim verileri geçersiz. Lütfen tekrar seçim yapınız.");
                }
            }
            catch (OutOfMemoryException)
            {
                ShowError("Resim işlenirken bellek yetersiz. Lütfen daha küçük bir alan seçin veya sayfayı yenileyin.");
            }
            catch (Exception ex)
            {
                ShowError(String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message));
            }
        }

        private ImageCodecInfo GetPngEncoder()
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            for (int i = 0; i < codecs.Length; i++)
            {
                if (codecs[i].FormatID == ImageFormat.Png.Guid)
                {
                    return codecs[i];
                }
            }
            return null;
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
}