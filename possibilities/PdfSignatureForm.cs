using System;
using System.Drawing;
using System.IO;
using Gizmox.WebGUI.Forms;

namespace Possibilities
{
    public class PdfSignatureForm : Form
    {
        private UploadControl uploadControl;
        private Button btnShowPdf;
        private PictureBox pictureBoxPdfPage;
        private Button btnSaveSignature;
        private Rectangle selectionRect;
        private bool isSelecting = false;
        private Point selectionStart;
        private string lastUploadedPdfPath;
        private string lastRenderedImagePath;

        public PdfSignatureForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.uploadControl = new UploadControl();
            this.btnShowPdf = new Button();
            this.pictureBoxPdfPage = new PictureBox();
            this.btnSaveSignature = new Button();

            // Form
            this.Text = "PDF İmza Seçimi";
            this.Size = new System.Drawing.Size(900, 700);

            // uploadControl
            this.uploadControl.Location = new System.Drawing.Point(20, 20);
            this.uploadControl.Width = 300;
            this.uploadControl.UploadComplete += UploadControl_UploadComplete;
            this.uploadControl.UploadFilePath = @"\\trrfap2034\files\cdn"; 

            // btnShowPdf
            this.btnShowPdf.Text = "PDF'i Göster";
            this.btnShowPdf.Location = new System.Drawing.Point(340, 20);
            this.btnShowPdf.Click += BtnShowPdf_Click;
            this.btnShowPdf.Enabled = false;

            // pictureBoxPdfPage
            this.pictureBoxPdfPage.Location = new System.Drawing.Point(20, 60);
            this.pictureBoxPdfPage.Size = new System.Drawing.Size(800, 500);
            this.pictureBoxPdfPage.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBoxPdfPage.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBoxPdfPage.MouseDown += PictureBox_MouseDown;
            this.pictureBoxPdfPage.MouseMove += PictureBox_MouseMove;
            this.pictureBoxPdfPage.MouseUp += PictureBox_MouseUp;

            // btnSaveSignature
            this.btnSaveSignature.Text = "Seçimi İmza Olarak Kaydet";
            this.btnSaveSignature.Location = new System.Drawing.Point(20, 580);
            this.btnSaveSignature.Click += BtnSaveSignature_Click;
            this.btnSaveSignature.Enabled = false;

            // Controls
            this.Controls.Add(this.uploadControl);
            this.Controls.Add(this.btnShowPdf);
            this.Controls.Add(this.pictureBoxPdfPage);
            this.Controls.Add(this.btnSaveSignature);
        }

        private void UploadControl_UploadComplete(object sender, UploadCompleteEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.Instance.Debug($"[UploadControl_UploadComplete] Yükleme Hatası: {e.Error.Message}");
                if (e.Error.StackTrace != null)
                {
                    Logger.Instance.Debug($"[UploadControl_UploadComplete] StackTrace: {e.Error.StackTrace}");
                }
                MessageBox.Show($"Dosya yüklenirken hata oluştu:\n{e.Error.Message}", "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lastUploadedPdfPath = null;
                btnShowPdf.Enabled = false;
            }
            else
            {
                lastUploadedPdfPath = e.FilePath;
                btnShowPdf.Enabled = true;
                MessageBox.Show("PDF başarıyla yüklendi. Şimdi göster butonuna basabilirsiniz.");
                Logger.Instance.Debug($"[UploadControl_UploadComplete] PDF başarıyla yüklendi: {lastUploadedPdfPath}");
            }
        }

        private void BtnShowPdf_Click(object sender, EventArgs e)
        {
            Logger.Instance.Debug($"[BtnShowPdf_Click] PDF gösterme işlemi başlatıldı. Yüklenen PDF yolu: {lastUploadedPdfPath}");
            if (!string.IsNullOrEmpty(lastUploadedPdfPath))
            {
                string cdnFolder = @"\\trrfap2034\files\cdn";
                Logger.Instance.Debug($"[BtnShowPdf_Click] CDN klasör yolu: {cdnFolder}");

                try
                {
                    System.IO.Directory.CreateDirectory(cdnFolder);
                    string imagePath = System.IO.Path.Combine(cdnFolder, "page_1.png");
                    Logger.Instance.Debug($"[BtnShowPdf_Click] PDF'i resme çeviriliyor. Hedef resim yolu: {imagePath}");

                    PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, cdnFolder);

                    if (System.IO.File.Exists(imagePath))
                    {
                        Logger.Instance.Debug($"[BtnShowPdf_Click] Resim dosyası bulundu: {imagePath}. Resim yükleniyor.");
                        
                        // Doğrudan UNC yolunu ImageLocation olarak kullan
                        pictureBoxPdfPage.ImageLocation = imagePath;
                        pictureBoxPdfPage.SizeMode = PictureBoxSizeMode.Zoom;
                        
                        lastRenderedImagePath = imagePath;
                        btnSaveSignature.Enabled = false;
                        selectionRect = System.Drawing.Rectangle.Empty;
                        MessageBox.Show("PDF'nin ilk sayfası ekranda gösteriliyor. Seçim yapabilirsiniz.");
                    }
                    else
                    {
                        Logger.Instance.Debug($"[BtnShowPdf_Click] Hata: Resim dosyası oluşturulamadı veya bulunamadı: {imagePath}");
                        MessageBox.Show("PDF'den resim oluşturulamadı.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Debug($"[BtnShowPdf_Click] PDF gösterme işleminde genel hata: {ex.Message}");
                    Logger.Instance.Debug($"[BtnShowPdf_Click] StackTrace: {ex.StackTrace}");
                    MessageBox.Show($"PDF gösterilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logger.Instance.Debug($"[BtnShowPdf_Click] Hata: Yüklenen PDF yolu boş.");
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            Logger.Instance.Debug($"[PictureBox_MouseDown] MouseDown olayı tetiklendi. Konum: {e.Location}");
            if (pictureBoxPdfPage.Image == null) return;
            isSelecting = true;
            selectionStart = e.Location;
            selectionRect = new System.Drawing.Rectangle(e.Location, new System.Drawing.Size(0, 0));
            btnSaveSignature.Enabled = false;
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && pictureBoxPdfPage.Image != null)
            {
                int x = System.Math.Min(selectionStart.X, e.X);
                int y = System.Math.Min(selectionStart.Y, e.Y);
                int w = System.Math.Abs(selectionStart.X - e.X);
                int h = System.Math.Abs(selectionStart.Y - e.Y);
                selectionRect = new System.Drawing.Rectangle(x, y, w, h);
                pictureBoxPdfPage.Invalidate(); // Yeniden çizim için
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            btnSaveSignature.Enabled = selectionRect.Width > 0 && selectionRect.Height > 0;
            Logger.Instance.Debug($"[PictureBox_MouseUp] MouseUp olayı tetiklendi. Seçim tamamlandı. Seçim dikdörtgeni: {selectionRect}. Kaydet butonu aktif: {btnSaveSignature.Enabled}");
            pictureBoxPdfPage.Invalidate(); // Yeniden çizim için
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            Logger.Instance.Debug($"[BtnSaveSignature_Click] İmza kaydetme işlemi başlatıldı. Render edilmiş resim yolu: {lastRenderedImagePath}. Seçim dikdörtgeni: {selectionRect}");
            if (!string.IsNullOrEmpty(lastRenderedImagePath) && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                try
                {
                    using (var img = System.Drawing.Image.FromFile(lastRenderedImagePath))
                    {
                        float scaleX = (float)img.Width / pictureBoxPdfPage.Width;
                        float scaleY = (float)img.Height / pictureBoxPdfPage.Height;
                        System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(
                            (int)(selectionRect.X * scaleX),
                            (int)(selectionRect.Y * scaleY),
                            (int)(selectionRect.Width * scaleX),
                            (int)(selectionRect.Height * scaleY)
                        );
                        string outputPath = System.IO.Path.Combine(@"\\trrfap2034\files\cdn", $"signature_{System.DateTime.Now.Ticks}.png");
                        
                        Logger.Instance.Debug($"[BtnSaveSignature_Click] Crop edilecek alan (orijinal boyutlara göre): {cropRect}");
                        Logger.Instance.Debug($"[BtnSaveSignature_Click] Kaydedilecek imza yolu: {outputPath}");

                        PdfToImageAndCrop.CropImageAndSave(lastRenderedImagePath, cropRect, outputPath);
                        MessageBox.Show("Seçim imza olarak kaydedildi!\n" + outputPath);
                        Logger.Instance.Debug($"[BtnSaveSignature_Click] İmza başarıyla kaydedildi: {outputPath}");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Debug($"[BtnSaveSignature_Click] İmza kaydetme işleminde hata: {ex.Message}");
                    Logger.Instance.Debug($"[BtnSaveSignature_Click] StackTrace: {ex.StackTrace}");
                    MessageBox.Show($"İmza kaydedilirken bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logger.Instance.Debug($"[BtnSaveSignature_Click] Hata: Render edilmiş resim yolu boş veya seçim alanı geçersiz.");
            }
        }
    }
} 