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
                        // 300 DPI, PNG formatında render
                        var resolution = new Resolution(300);
                        var pngDevice = new PngDevice(resolution);
                        pngDevice.Process(pdfDocument.Pages[pageCount], imageStream);
                        imageStream.Close();
                    }
                }
            }
        }

        // Seçilen alanı crop'lar ve cdn klasörüne kaydeder
        public static void CropImageAndSave(string imagePath, Rectangle section, string outputImagePath)
        {
            using (var sourceImage = Image.FromFile(imagePath))
            using (var bmp = new Bitmap(section.Width, section.Height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(sourceImage, 0, 0, section, GraphicsUnit.Pixel);
                bmp.Save(outputImagePath, ImageFormat.Png);
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