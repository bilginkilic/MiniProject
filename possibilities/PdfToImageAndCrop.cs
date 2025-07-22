using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Aspose.Pdf;
using Aspose.Pdf.Devices;

namespace Possibilities
{
    public class PdfToImageAndCrop
    {
        // PDF'i sayfa sayfa resimlere çevirir ve cdn klasörüne kaydeder (Aspose ile)
        public static void ConvertPdfToImages(string pdfPath, string cdnFolder)
        {
            using (var pdfDocument = new Document(pdfPath))
            {
                for (int pageCount = 1; pageCount <= pdfDocument.Pages.Count; pageCount++)
                {
                    string imagePath = Path.Combine(cdnFolder, $"page_{pageCount}.png");
                    using (FileStream imageStream = new FileStream(imagePath, FileMode.Create))
                    {
                        var resolution = new Resolution(300);
                        var pngDevice = new PngDevice(resolution);
                        pngDevice.Process(pdfDocument.Pages[pageCount], imageStream);
                        imageStream.Close();
                    }
                }
            }
        }

        // Seçilen alanı crop'lar ve cdn klasörüne kaydeder
        public static void CropImageAndSave(string imagePath, System.Drawing.Rectangle section, string outputImagePath)
        {
            // System.Drawing.Image.FromFile kullanıyoruz
            using (var sourceImage = System.Drawing.Image.FromFile(imagePath))
            {
                // Bitmap constructor'ı için 3. parametre olarak PixelFormat.Format32bppArgb belirtiyoruz.
                // Bu, .NET Framework 4.5.2'de geçerli bir constructor'dır.
                using (var bmp = new System.Drawing.Bitmap(section.Width, section.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    // Graphics nesnesini System.Drawing.Graphics.FromImage ile alıyoruz
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        // DrawImage metodunu 7 parametreli overload ile kullanıyoruz
                        // destination rectangle (nereye çizileceği): (0,0) konumunda, kırpılan alanın genişliği ve yüksekliği kadar
                        // source rectangle (nereden alınacağı): section.X, section.Y konumundan section.Width, section.Height kadar
                        g.DrawImage(
                            sourceImage,
                            new System.Drawing.Rectangle(0, 0, section.Width, section.Height), // Hedef dikdörtgen
                            section.X, section.Y, section.Width, section.Height, // Kaynak dikdörtgen parametreleri
                            System.Drawing.GraphicsUnit.Pixel // Ölçü birimi
                        );
                    }
                    // Resimi PNG formatında kaydediyoruz
                    bmp.Save(outputImagePath, System.Drawing.Imaging.ImageFormat.Png);
                }
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