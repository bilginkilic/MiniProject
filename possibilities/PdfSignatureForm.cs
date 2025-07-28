using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;
using Gizmox.WebGUI.Forms.Skins;

namespace BtmuApps.UI.Forms.SIGN
{
    public class PdfSignatureForm : Form
    {
        private readonly string _cdn = @"\\trrgap3027\files\circular\cdn";
        private UploadControl uploadControl;
        private Button btnShowPdf;
        private HtmlBox imageBox;
        private Button btnSaveSignature;
        private bool isSelecting = false;
        private Point selectionStart;
        private string lastUploadedPdfPath;
        private string lastRenderedImagePath;
        private Rectangle currentSelection;

        public PdfSignatureForm()
        {
            InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Cleanup if needed
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.uploadControl = new UploadControl();
            this.btnShowPdf = new Button();
            this.imageBox = new HtmlBox();
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

            // imageBox
            this.imageBox.Location = new System.Drawing.Point(20, 60);
            this.imageBox.Size = new System.Drawing.Size(800, 500);
            this.imageBox.BorderStyle = BorderStyle.FixedSingle;
            this.imageBox.BackColor = Color.White;
            this.imageBox.MouseDown += ImageBox_MouseDown;
            this.imageBox.MouseMove += ImageBox_MouseMove;
            this.imageBox.MouseUp += ImageBox_MouseUp;

            // btnSaveSignature
            this.btnSaveSignature.Text = "Seçimi İmza Olarak Kaydet";
            this.btnSaveSignature.Location = new System.Drawing.Point(20, 580);
            this.btnSaveSignature.Click += BtnSaveSignature_Click;
            this.btnSaveSignature.Enabled = false;

            // Controls
            this.Controls.Add(this.uploadControl);
            this.Controls.Add(this.btnShowPdf);
            this.Controls.Add(this.imageBox);
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
                            // HTML içeriğini oluştur
                            string imageUrl = VirtualPathUtility.ToAbsolute("~/cdn/page_1.png");
                            string html = $@"
                                <div style='width:100%;height:100%;position:relative;'>
                                    <img src='{imageUrl}' style='max-width:100%;max-height:100%;' />
                                    <div id='selection' style='position:absolute;border:2px solid red;display:none;'></div>
                                </div>
                                <script>
                                    var isSelecting = false;
                                    var startX, startY;
                                    
                                    function updateSelection(x, y, w, h) {{
                                        var sel = document.getElementById('selection');
                                        sel.style.left = x + 'px';
                                        sel.style.top = y + 'px';
                                        sel.style.width = w + 'px';
                                        sel.style.height = h + 'px';
                                        sel.style.display = 'block';
                                    }}
                                </script>";
                            
                            imageBox.Html = html;
                            lastRenderedImagePath = imagePath;
                            currentSelection = Rectangle.Empty;
                            btnSaveSignature.Enabled = false;
                            
                            MessageBox.Show("PDF'nin ilk sayfası ekranda gösteriliyor. Seçim yapabilirsiniz.");
                            Logger.Instance.Debug("[BtnShowPdf_Click] Resim başarıyla yüklendi ve gösterildi.");
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

        private void ImageBox_MouseDown(object sender, MouseEventArgs e)
        {
            isSelecting = true;
            selectionStart = e.Location;
            currentSelection = Rectangle.Empty;
            btnSaveSignature.Enabled = false;
            
            // JavaScript ile seçim başlat
            imageBox.EvalScript($"startX = {e.X}; startY = {e.Y}; isSelecting = true;");
        }

        private void ImageBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting)
            {
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int w = Math.Abs(selectionStart.X - e.X);
                int h = Math.Abs(selectionStart.Y - e.Y);
                
                currentSelection = new Rectangle(x, y, w, h);
                
                // JavaScript ile seçim güncelle
                imageBox.EvalScript($"updateSelection({x}, {y}, {w}, {h});");
            }
        }

        private void ImageBox_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            btnSaveSignature.Enabled = currentSelection.Width > 0 && currentSelection.Height > 0;
            Logger.Instance.Debug(string.Format("[ImageBox_MouseUp] Seçim tamamlandı. Seçim dikdörtgeni: {0}", currentSelection));
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] İmza kaydetme işlemi başlatıldı. Render edilmiş resim yolu: {0}. Seçim dikdörtgeni: {1}", 
                lastRenderedImagePath, currentSelection));
                
            if (!string.IsNullOrEmpty(lastRenderedImagePath) && currentSelection.Width > 0 && currentSelection.Height > 0)
            {
                try
                {
                    using (var img = System.Drawing.Image.FromFile(lastRenderedImagePath))
                    {
                        float scaleX = (float)img.Width / imageBox.Width;
                        float scaleY = (float)img.Height / imageBox.Height;
                        System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(
                            (int)(currentSelection.X * scaleX),
                            (int)(currentSelection.Y * scaleY),
                            (int)(currentSelection.Width * scaleX),
                            (int)(currentSelection.Height * scaleY)
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