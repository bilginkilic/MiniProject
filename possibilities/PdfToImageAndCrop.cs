using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using PdfiumViewer;

namespace Possibilities
{
    public class PdfToImageAndCrop
    {
        // PDF'i sayfa sayfa resimlere çevirir ve cdn klasörüne kaydeder
        public static void ConvertPdfToImages(string pdfPath, string cdnFolder)
        {
            using (var document = PdfDocument.Load(pdfPath))
            {
                for (int i = 0; i < document.PageCount; i++)
                {
                    using (var image = document.Render(i, 300, 300, true))
                    {
                        string imagePath = Path.Combine(cdnFolder, $"page_{i + 1}.png");
                        image.Save(imagePath, ImageFormat.Png);
                        Console.WriteLine($"Saved: {imagePath}");
                    }
                }
            }
        }

        // Seçilen alanı crop'lar ve cdn klasörüne kaydeder
        public static void CropImageAndSave(string imagePath, Rectangle cropArea, string outputImagePath)
        {
            using (var sourceImage = Image.FromFile(imagePath))
            using (var bmp = new Bitmap(cropArea.Width, cropArea.Height))
            using (var g = Graphics.FromImage(bmp))
            {
                g.DrawImage(sourceImage, 0, 0, cropArea, GraphicsUnit.Pixel);
                bmp.Save(outputImagePath, ImageFormat.Png);
                Console.WriteLine($"Cropped and saved: {outputImagePath}");
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