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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // PictureBox'taki resmi dispose et
                if (pictureBoxPdfPage != null && pictureBoxPdfPage.Image != null)
                {
                    pictureBoxPdfPage.Image.Dispose();
                    pictureBoxPdfPage.Image = null;
                }
            }
            base.Dispose(disposing);
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
            this.pictureBoxPdfPage.Paint += PictureBox_Paint;

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
                Logger.Instance.Debug(string.Format("[UploadControl_UploadComplete] Yükleme Hatası: {0}", e.Error.Message));
                if (e.Error.StackTrace != null)
                {
                    Logger.Instance.Debug(string.Format("[UploadControl_UploadComplete] StackTrace: {0}", e.Error.StackTrace));
                }
                MessageBox.Show(string.Format("Dosya yüklenirken hata oluştu:\n{0}", e.Error.Message), "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lastUploadedPdfPath = null;
                btnShowPdf.Enabled = false;
            }
            else
            {
                lastUploadedPdfPath = e.FilePath;
                btnShowPdf.Enabled = true;
                MessageBox.Show("PDF başarıyla yüklendi. Şimdi göster butonuna basabilirsiniz.");
                Logger.Instance.Debug(string.Format("[UploadControl_UploadComplete] PDF başarıyla yüklendi: {0}", lastUploadedPdfPath));
            }
        }

        private void BtnShowPdf_Click(object sender, EventArgs e)
        {
            Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] PDF gösterme işlemi başlatıldı. Yüklenen PDF yolu: {0}", lastUploadedPdfPath));
            if (!string.IsNullOrEmpty(lastUploadedPdfPath))
            {
                string cdnFolder = @"\\trrfap2034\files\cdn";
                Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] CDN klasör yolu: {0}", cdnFolder));

                try
                {
                    System.IO.Directory.CreateDirectory(cdnFolder);
                    string imagePath = System.IO.Path.Combine(cdnFolder, "page_1.png");
                    Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] PDF'i resme çeviriliyor. Hedef resim yolu: {0}", imagePath));

                    PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, cdnFolder);

                    if (System.IO.File.Exists(imagePath))
                    {
                        Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Resim dosyası bulundu: {0}. Resim yükleniyor.", imagePath));
                        
                        try
                        {
                            // Önceki resmi dispose et
                            if (pictureBoxPdfPage.Image != null)
                            {
                                pictureBoxPdfPage.Image.Dispose();
                                pictureBoxPdfPage.Image = null;
                            }
                            
                            // Dosya boyutunu kontrol et
                            var fileInfo = new System.IO.FileInfo(imagePath);
                            Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Dosya boyutu: {0} bytes", fileInfo.Length));
                            
                            if (fileInfo.Length == 0)
                            {
                                Logger.Instance.Debug("[BtnShowPdf_Click] Hata: Dosya boş!");
                                MessageBox.Show("Oluşturulan resim dosyası boş!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            
                            // Resmi doğrudan yükle - stream'i kapatmadan önce Image'i oluştur
                            System.Drawing.Image loadedImage = null;
                            using (var stream = new System.IO.FileStream(imagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                            {
                                loadedImage = System.Drawing.Image.FromStream(stream);
                            }
                            
                            if (loadedImage == null)
                            {
                                Logger.Instance.Debug("[BtnShowPdf_Click] Hata: Image.FromStream null döndü!");
                                MessageBox.Show("Resim yüklenemedi - dosya formatı desteklenmiyor olabilir.", "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return;
                            }
                            
                            pictureBoxPdfPage.Image = loadedImage;
                            pictureBoxPdfPage.SizeMode = PictureBoxSizeMode.Zoom;
                            
                            Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Resim başarıyla yüklendi. Boyut: {0}x{1}", loadedImage.Width, loadedImage.Height));
                            
                            lastRenderedImagePath = imagePath;
                            btnSaveSignature.Enabled = false;
                            selectionRect = System.Drawing.Rectangle.Empty;
                            MessageBox.Show("PDF'nin ilk sayfası ekranda gösteriliyor. Seçim yapabilirsiniz.");
                        }
                        catch (Exception loadEx)
                        {
                            Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Resim yükleme hatası: {0}", loadEx.Message));
                            MessageBox.Show(string.Format("Resim yüklenirken hata oluştu: {0}", loadEx.Message), "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Hata: Resim dosyası oluşturulamadı veya bulunamadı: {0}", imagePath));
                        MessageBox.Show("PDF'den resim oluşturulamadı.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] PDF gösterme işleminde genel hata: {0}", ex.Message));
                    Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] StackTrace: {0}", ex.StackTrace));
                    MessageBox.Show(string.Format("PDF gösterilirken bir hata oluştu: {0}", ex.Message), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logger.Instance.Debug("[BtnShowPdf_Click] Hata: Yüklenen PDF yolu boş.");
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            Logger.Instance.Debug(string.Format("[PictureBox_MouseDown] MouseDown olayı tetiklendi. Konum: {0}", e.Location));
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
            Logger.Instance.Debug(string.Format("[PictureBox_MouseUp] MouseUp olayı tetiklendi. Seçim tamamlandı. Seçim dikdörtgeni: {0}. Kaydet butonu aktif: {1}", selectionRect, btnSaveSignature.Enabled));
            pictureBoxPdfPage.Invalidate(); // Yeniden çizim için
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            // Seçim dikdörtgenini çiz
            if (selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] İmza kaydetme işlemi başlatıldı. Render edilmiş resim yolu: {0}. Seçim dikdörtgeni: {1}", lastRenderedImagePath, selectionRect));
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
                        string outputPath = System.IO.Path.Combine(@"\\trrfap2034\files\cdn", string.Format("signature_{0}.png", System.DateTime.Now.Ticks));
                        
                        Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] Crop edilecek alan (orijinal boyutlara göre): {0}", cropRect));
                        Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] Kaydedilecek imza yolu: {0}", outputPath));

                        PdfToImageAndCrop.CropImageAndSave(lastRenderedImagePath, cropRect, outputPath);
                        MessageBox.Show(string.Format("Seçim imza olarak kaydedildi!\n{0}", outputPath));
                        Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] İmza başarıyla kaydedildi: {0}", outputPath));
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] İmza kaydetme işleminde hata: {0}", ex.Message));
                    Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] StackTrace: {0}", ex.StackTrace));
                    MessageBox.Show(string.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Logger.Instance.Debug("[BtnSaveSignature_Click] Hata: Render edilmiş resim yolu boş veya seçim alanı geçersiz.");
            }
        }
    }
} 