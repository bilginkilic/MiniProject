using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Devices;
using System.Linq; // Added for FirstOrDefault

namespace Possibilities
{
    public class PdfToImageAndCrop
    {
        // PDF'i sayfa sayfa resimlere çevirir ve cdn klasörüne kaydeder (Aspose ile)
        public static int ConvertPdfToImages(string pdfPath, string cdnFolder)
        {
            if (!File.Exists(pdfPath))
            {
                throw new FileNotFoundException("PDF dosyası bulunamadı.", pdfPath);
            }

            if (!Directory.Exists(cdnFolder))
            {
                Directory.CreateDirectory(cdnFolder);
            }

            try
            {
                using (var pdfDocument = new Document(pdfPath))
                {
                    int pageCount = pdfDocument.Pages.Count;
                    
                    // Önceki dosyaları temizle
                    foreach (string file in Directory.GetFiles(cdnFolder, "page_*.png"))
                    {
                        try { File.Delete(file); } catch { }
                    }

                    for (int pageNumber = 1; pageNumber <= pageCount; pageNumber++)
                    {
                        string imagePath = Path.Combine(cdnFolder, String.Format("page_{0}.png", pageNumber));
                        using (FileStream imageStream = new FileStream(imagePath, FileMode.Create))
                        {
                            // Yüksek kaliteli çıktı için çözünürlük ayarları
                            var resolution = new Resolution(300);
                            var pngDevice = new PngDevice(resolution);
                            
                            // Sayfa işleme kalitesi ayarları
                            pngDevice.Process(pdfDocument.Pages[pageNumber], imageStream);
                            imageStream.Close();

                            // Oluşturulan resmi optimize et
                            OptimizeImage(imagePath);
                        }
                    }

                    return pageCount;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("PDF dosyası resme çevrilirken hata oluştu: {0}", ex.Message), ex);
            }
        }

        private static void OptimizeImage(string imagePath)
        {
            try
            {
                // Önce dosyanın serbest olduğundan emin ol
                using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    fs.Close();
                }

                // Geçici bir dosya adı oluştur
                string tempPath = Path.Combine(
                    Path.GetDirectoryName(imagePath),
                    "temp_" + Path.GetFileName(imagePath)
                );

                // Resmi geçici dosyaya optimize et
                using (var image = Image.FromFile(imagePath))
                using (var bitmap = new Bitmap(image))
                {
                    // Resim kalitesi ayarları
                    var encoderParameters = new EncoderParameters(1);
                    encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

                    // PNG codec'ini bul
                    ImageCodecInfo pngEncoder = GetPngEncoder();
                    if (pngEncoder != null)
                    {
                        // Önce geçici dosyaya kaydet
                        bitmap.Save(tempPath, pngEncoder, encoderParameters);
                    }
                }

                // Orijinal dosyayı sil ve geçici dosyayı yerine koy
                if (File.Exists(tempPath))
                {
                    File.Delete(imagePath);
                    File.Move(tempPath, imagePath);
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda geçici dosyayı temizle
                try
                {
                    string tempPath = Path.Combine(
                        Path.GetDirectoryName(imagePath),
                        "temp_" + Path.GetFileName(imagePath)
                    );
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch { }

                // Hatayı logla
                System.Diagnostics.Debug.WriteLine($"Resim optimizasyonu başarısız: {ex.Message}");
                // Optimizasyon başarısız olursa orijinal resmi koru - sessizce devam et
            }
        }

        private static ImageCodecInfo GetPngEncoder()
        {
            try
            {
                ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
                return codecs.FirstOrDefault(codec => codec.FormatID == ImageFormat.Png.Guid);
            }
            catch
            {
                return null;
            }
        }

        // Seçilen alanı crop'lar ve cdn klasörüne kaydeder
        public static void CropImageAndSave(string imagePath, Rectangle section, string outputImagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException("Kaynak resim dosyası bulunamadı.", imagePath);
            }

            // Geçici dosya yolu oluştur
            string tempPath = Path.Combine(
                Path.GetDirectoryName(outputImagePath),
                "temp_" + Path.GetFileName(outputImagePath)
            );

            try
            {
                using (var sourceImage = Image.FromFile(imagePath))
                {
                    // Seçim alanının resim sınırları içinde olduğunu kontrol et
                    if (section.X < 0 || section.Y < 0 ||
                        section.Right > sourceImage.Width ||
                        section.Bottom > sourceImage.Height)
                    {
                        throw new ArgumentException("Seçilen alan resim sınırları dışında.");
                    }

                    using (var bitmap = new Bitmap(section.Width, section.Height, PixelFormat.Format32bppArgb))
                    {
                        bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            // Yüksek kaliteli çıktı için grafik ayarları
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            graphics.DrawImage(sourceImage,
                                new Rectangle(0, 0, section.Width, section.Height),
                                section,
                                GraphicsUnit.Pixel);
                        }

                        // Önce geçici dosyaya kaydet
                        bitmap.Save(tempPath, ImageFormat.Png);
                    }
                }

                // Başarılı olursa, geçici dosyayı hedef konuma taşı
                if (File.Exists(outputImagePath))
                {
                    File.Delete(outputImagePath);
                }
                File.Move(tempPath, outputImagePath);
            }
            catch (Exception ex)
            {
                // Hata durumunda geçici dosyayı temizle
                try
                {
                    if (File.Exists(tempPath))
                    {
                        File.Delete(tempPath);
                    }
                }
                catch { }

                throw new Exception($"Resim kırpma işlemi sırasında hata oluştu: {ex.Message}", ex);
            }
        }

        // Örnek kullanım
        public static void Example()
        {
            string pdfPath = @"C:\\path\\to\\input.pdf";
            string cdnFolder = @"C:\\path\\to\\MiniProject\\possibilities\\cdn";
            Directory.CreateDirectory(cdnFolder);

            // 1. PDF'i resimlere çevir
            ConvertPdfToImages(pdfPath, cdnFolder);

            // 2. Kullanıcıdan crop koordinatları alındığını varsayalım:
            // Örneğin, 1. sayfa için (x=100, y=150, w=200, h=100)
            string pageImagePath = Path.Combine(cdnFolder, "page_1.png");
            Rectangle cropArea = new Rectangle(100, 150, 200, 100);
            string croppedImagePath = Path.Combine(cdnFolder, "cropped_signature.png");

            // 3. Crop ve kaydet
            CropImageAndSave(pageImagePath, cropArea, croppedImagePath);
        }
    }
} 