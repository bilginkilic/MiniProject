using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Gizmox.WebGUI.Forms;

namespace BtmuApps.UI.Forms.SIGN
{
    public class CustomImagePanel : Panel
    {
        private Rectangle _selectionRect;
        
        public Rectangle SelectionRect
        {
            get => _selectionRect;
            set
            {
                _selectionRect = value;
                this.Refresh();
            }
        }

        protected override void RenderAttributes(IContext context)
        {
            base.RenderAttributes(context);
            
            if (_selectionRect.Width > 0 && _selectionRect.Height > 0)
            {
                using (var pen = new Pen(Color.Red, 2))
                {
                    context.Graphics.DrawRectangle(pen, _selectionRect);
                }
            }
        }
    }

    public class PdfSignatureForm : Form
    {
        private readonly string _cdn = @"\\trrgap3027\files\circular\cdn";
        private UploadControl uploadControl;
        private Button btnShowPdf;
        private CustomImagePanel imagePanel;
        private Button btnSaveSignature;
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
                if (imagePanel != null && imagePanel.BackgroundImage != null)
                {
                    imagePanel.BackgroundImage.Dispose();
                    imagePanel.BackgroundImage = null;
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.uploadControl = new UploadControl();
            this.btnShowPdf = new Button();
            this.imagePanel = new CustomImagePanel();
            this.btnSaveSignature = new Button();

            // Form
            this.Text = "PDF İmza Seçimi";
            this.Size = new System.Drawing.Size(900, 700);

            // uploadControl
            this.uploadControl.Location = new System.Drawing.Point(20, 20);
            this.uploadControl.Width = 300;
            this.uploadControl.UploadComplete += UploadControl_UploadComplete;
            this.uploadControl.UploadFilePath = _cdn;

            // btnShowPdf
            this.btnShowPdf.Text = "PDF'i Göster";
            this.btnShowPdf.Location = new System.Drawing.Point(340, 20);
            this.btnShowPdf.Click += BtnShowPdf_Click;
            this.btnShowPdf.Enabled = false;

            // imagePanel
            this.imagePanel.Location = new System.Drawing.Point(20, 60);
            this.imagePanel.Size = new System.Drawing.Size(800, 500);
            this.imagePanel.BackgroundImageLayout = ImageLayout.Zoom;
            this.imagePanel.BorderStyle = BorderStyle.FixedSingle;
            this.imagePanel.MouseDown += Panel_MouseDown;
            this.imagePanel.MouseMove += Panel_MouseMove;
            this.imagePanel.MouseUp += Panel_MouseUp;

            // btnSaveSignature
            this.btnSaveSignature.Text = "Seçimi İmza Olarak Kaydet";
            this.btnSaveSignature.Location = new System.Drawing.Point(20, 580);
            this.btnSaveSignature.Click += BtnSaveSignature_Click;
            this.btnSaveSignature.Enabled = false;

            // Controls
            this.Controls.Add(this.uploadControl);
            this.Controls.Add(this.btnShowPdf);
            this.Controls.Add(this.imagePanel);
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
                Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] CDN klasör yolu: {0}", _cdn));

                try
                {
                    System.IO.Directory.CreateDirectory(_cdn);
                    string imagePath = System.IO.Path.Combine(_cdn, "page_1.png");
                    Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] PDF'i resme çeviriliyor. Hedef resim yolu: {0}", imagePath));

                    PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, _cdn);

                    if (System.IO.File.Exists(imagePath))
                    {
                        Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Resim dosyası bulundu: {0}. Resim yükleniyor.", imagePath));
                        
                        try
                        {
                            // Önceki resmi dispose et
                            if (imagePanel.BackgroundImage != null)
                            {
                                imagePanel.BackgroundImage.Dispose();
                                imagePanel.BackgroundImage = null;
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
                            
                            // Resmi doğrudan yükle
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
                            
                            Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] loadedImage başarıyla oluşturuldu. Boyut: {0}x{1}", loadedImage.Width, loadedImage.Height));
                            
                            // Thread-safe image assignment
                            if (this.InvokeRequired)
                            {
                                this.Invoke(new Action(() => {
                                    imagePanel.BackgroundImage = loadedImage;
                                    Logger.Instance.Debug("[BtnShowPdf_Click] Image thread-safe olarak atandı.");
                                }));
                            }
                            else
                            {
                                imagePanel.BackgroundImage = loadedImage;
                                Logger.Instance.Debug("[BtnShowPdf_Click] Image doğrudan atandı.");
                            }
                            
                            // Atama sonrası kontrol
                            System.Threading.Thread.Sleep(100); // Kısa bekleme
                            if (imagePanel.BackgroundImage == null)
                            {
                                Logger.Instance.Debug("[BtnShowPdf_Click] UYARI: Atama sonrası Panel.BackgroundImage hala null!");
                                // Alternatif yöntem dene
                                imagePanel.BackgroundImage = loadedImage;
                                imagePanel.Refresh();
                                Logger.Instance.Debug("[BtnShowPdf_Click] Alternatif atama yapıldı.");
                            }
                            else
                            {
                                Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Panel.BackgroundImage başarıyla atandı. Boyut: {0}x{1}", 
                                    imagePanel.BackgroundImage.Width, imagePanel.BackgroundImage.Height));
                            }
                            
                            lastRenderedImagePath = imagePath;
                            btnSaveSignature.Enabled = false;
                            imagePanel.SelectionRect = Rectangle.Empty;
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

        private void Panel_MouseDown(object sender, MouseEventArgs e)
        {
            Logger.Instance.Debug(string.Format("[Panel_MouseDown] MouseDown olayı tetiklendi. Konum: {0}", e.Location));
            if (imagePanel.BackgroundImage == null) return;
            isSelecting = true;
            selectionStart = e.Location;
            imagePanel.SelectionRect = new Rectangle(e.Location, new Size(0, 0));
            btnSaveSignature.Enabled = false;
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && imagePanel.BackgroundImage != null)
            {
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int w = Math.Abs(selectionStart.X - e.X);
                int h = Math.Abs(selectionStart.Y - e.Y);
                imagePanel.SelectionRect = new Rectangle(x, y, w, h);
            }
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            btnSaveSignature.Enabled = imagePanel.SelectionRect.Width > 0 && imagePanel.SelectionRect.Height > 0;
            Logger.Instance.Debug(string.Format("[Panel_MouseUp] MouseUp olayı tetiklendi. Seçim tamamlandı. Seçim dikdörtgeni: {0}. Kaydet butonu aktif: {1}", 
                imagePanel.SelectionRect, btnSaveSignature.Enabled));
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] İmza kaydetme işlemi başlatıldı. Render edilmiş resim yolu: {0}. Seçim dikdörtgeni: {1}", 
                lastRenderedImagePath, imagePanel.SelectionRect));
            if (!string.IsNullOrEmpty(lastRenderedImagePath) && imagePanel.SelectionRect.Width > 0 && imagePanel.SelectionRect.Height > 0)
            {
                try
                {
                    using (var img = System.Drawing.Image.FromFile(lastRenderedImagePath))
                    {
                        float scaleX = (float)img.Width / imagePanel.Width;
                        float scaleY = (float)img.Height / imagePanel.Height;
                        System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(
                            (int)(imagePanel.SelectionRect.X * scaleX),
                            (int)(imagePanel.SelectionRect.Y * scaleY),
                            (int)(imagePanel.SelectionRect.Width * scaleX),
                            (int)(imagePanel.SelectionRect.Height * scaleY)
                        );
                        string outputPath = System.IO.Path.Combine(_cdn, string.Format("signature_{0}.png", System.DateTime.Now.Ticks));
                        
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