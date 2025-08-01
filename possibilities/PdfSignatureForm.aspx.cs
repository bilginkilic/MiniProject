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
                        ShowError("Lütfen PDF dosyası seçin.");
                        return;
                    }

                    string pdfPath = Path.Combine(_cdn, fileName);
                    fileUpload.SaveAs(pdfPath);

                    // PDF yüklendi, göster butonunu aktif et
                    btnShowPdf.Enabled = true;
                    Session["LastUploadedPdf"] = pdfPath;

                    ShowMessage("İmza sirkülerini yüklendi. Göster butonuna tıklayarak imzaları seçebilirsiniz.");
                }
                else
                {
                    ShowError("Lütfen bir dosya seçin.");
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("Dosya yüklenirken hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnShowPdf_Click(object sender, EventArgs e)
        {
            string pdfPath = Session["LastUploadedPdf"] as string;
            if (string.IsNullOrEmpty(pdfPath))
            {
                ShowError("Lütfen önce bir imza sirkülerini yükleyin.");
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

                    ShowMessage("İmza sirkülerini görüntüleniyor. İmzaları seçmek için mouse ile seçim yapabilirsiniz.");
                }
                else
                {
                    ShowError("İmza sirkülerini görüntülenemedi. Lütfen dosyanın PDF formatında olduğundan emin olun.");
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("İmza sirkülerini görüntülerken hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            string imagePath = Session["LastRenderedImage"] as string;
            string selectionData = hdnSelection.Value;

            if (string.IsNullOrEmpty(imagePath) || string.IsNullOrEmpty(selectionData))
            {
                ShowError("Lütfen önce bir imza seçin.");
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

                    Rectangle selectionRect = new Rectangle(x, y, width, height);

                    using (var sourceImage = Image.FromFile(imagePath))
                    {
                        // Seçim koordinatlarını orijinal resim boyutuna göre ölçekle
                        float scaleX = (float)sourceImage.Width / imageContainer.ClientIDMode;
                        float scaleY = (float)sourceImage.Height / imageContainer.ClientIDMode;

                        Rectangle cropRect = new Rectangle(
                            (int)(selectionRect.X * scaleX),
                            (int)(selectionRect.Y * scaleY),
                            (int)(selectionRect.Width * scaleX),
                            (int)(selectionRect.Height * scaleY)
                        );

                        // Seçilen alanı yeni bir resim olarak kaydet
                        using (var cropImage = new Bitmap(cropRect.Width, cropRect.Height))
                        {
                            using (var g = Graphics.FromImage(cropImage))
                            {
                                g.DrawImage(sourceImage, new Rectangle(0, 0, cropRect.Width, cropRect.Height),
                                          cropRect, GraphicsUnit.Pixel);
                            }

                            string outputPath = Path.Combine(_cdn, String.Format("signature_{0}.png", DateTime.Now.Ticks));
                            cropImage.Save(outputPath, ImageFormat.Png);

                            ShowMessage(String.Format("İmza başarıyla kaydedildi:\n{0}", outputPath));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("İmza kaydedilirken hata oluştu: {0}", ex.Message));
            }
        }

        private void ShowError(string message)
        {
            lblMessage.ForeColor = Color.Red;
            lblMessage.Text = message;
        }

        private void ShowMessage(string message)
        {
            lblMessage.ForeColor = Color.Green;
            lblMessage.Text = message;
        }
    }
}