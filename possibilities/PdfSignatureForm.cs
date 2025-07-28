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
        private string lastUploadedPdfPath;
        private string lastRenderedImagePath;
        private Rectangle selectionRect;
        private bool isSelecting;
        private Point startPoint;

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
            this.Text = "İmza Sirkülerinden İmza Seçimi";
            this.Size = new System.Drawing.Size(900, 700);

            // uploadControl
            this.uploadControl.Location = new System.Drawing.Point(20, 20);
            this.uploadControl.Width = 300;
            this.uploadControl.UploadComplete += UploadControl_UploadComplete;
            this.uploadControl.UploadFilePath = _cdn;

            // btnShowPdf
            this.btnShowPdf.Text = "İmza Sirküleri Göster";
            this.btnShowPdf.Location = new System.Drawing.Point(340, 20);
            this.btnShowPdf.Click += BtnShowPdf_Click;
            this.btnShowPdf.Enabled = false;

            // imageBox
            this.imageBox.Location = new System.Drawing.Point(20, 60);
            this.imageBox.Size = new System.Drawing.Size(800, 500);
            this.imageBox.BorderStyle = BorderStyle.FixedSingle;
            this.imageBox.BackColor = Color.White;

            // btnSaveSignature
            this.btnSaveSignature.Text = "Seçilen İmzayı Kaydet";
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
                Logger.Instance.Debug($"[UploadControl_UploadComplete] Yükleme Hatası: {e.Error.Message}");
                MessageBox.Show($"İmza sirkülerini yüklerken hata oluştu:\n{e.Error.Message}", "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lastUploadedPdfPath = null;
                btnShowPdf.Enabled = false;
            }
            else
            {
                lastUploadedPdfPath = e.FilePath;
                btnShowPdf.Enabled = true;
                MessageBox.Show("İmza sirkülerini yüklendi. Göster butonuna tıklayarak imzaları seçebilirsiniz.");
                Logger.Instance.Debug($"[UploadControl_UploadComplete] İmza sirkülerini yüklendi: {lastUploadedPdfPath}");
            }
        }

        private void BtnShowPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastUploadedPdfPath))
            {
                MessageBox.Show("Lütfen önce bir imza sirkülerini yükleyin.");
                return;
            }

            try
            {
                Directory.CreateDirectory(_cdn);
                string imagePath = Path.Combine(_cdn, "signature_page.png");
                
                // PDF'yi yüksek kalitede PNG'ye çevir
                PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, _cdn, 300); // 300 DPI kalite

                if (File.Exists(imagePath))
                {
                    string imageUrl = VirtualPathUtility.ToAbsolute("~/cdn/signature_page.png");
                    string html = $@"
                        <html>
                        <head>
                            <style>
                                body {{ 
                                    margin: 0; 
                                    padding: 0; 
                                    overflow: hidden;
                                    background-color: #f0f0f0;
                                }}
                                .container {{
                                    width: 100%;
                                    height: 100%;
                                    display: flex;
                                    justify-content: center;
                                    align-items: center;
                                }}
                                .image-wrapper {{
                                    position: relative;
                                    display: inline-block;
                                }}
                                img {{
                                    max-width: 100%;
                                    max-height: 100%;
                                    border: 1px solid #ccc;
                                    box-shadow: 0 0 10px rgba(0,0,0,0.1);
                                }}
                                #selection {{
                                    position: absolute;
                                    border: 2px solid red;
                                    background-color: rgba(255,0,0,0.1);
                                    pointer-events: none;
                                    display: none;
                                }}
                            </style>
                            <script>
                                var isSelecting = false;
                                var startX, startY;
                                
                                function startSelection(e) {{
                                    isSelecting = true;
                                    var rect = e.target.getBoundingClientRect();
                                    startX = e.clientX - rect.left;
                                    startY = e.clientY - rect.top;
                                    
                                    var sel = document.getElementById('selection');
                                    sel.style.left = startX + 'px';
                                    sel.style.top = startY + 'px';
                                    sel.style.width = '0px';
                                    sel.style.height = '0px';
                                    sel.style.display = 'block';
                                }}
                                
                                function updateSelection(e) {{
                                    if (!isSelecting) return;
                                    
                                    var img = document.querySelector('.image-wrapper img');
                                    var rect = img.getBoundingClientRect();
                                    var currentX = e.clientX - rect.left;
                                    var currentY = e.clientY - rect.top;
                                    
                                    var x = Math.min(startX, currentX);
                                    var y = Math.min(startY, currentY);
                                    var w = Math.abs(currentX - startX);
                                    var h = Math.abs(currentY - startY);
                                    
                                    var sel = document.getElementById('selection');
                                    sel.style.left = x + 'px';
                                    sel.style.top = y + 'px';
                                    sel.style.width = w + 'px';
                                    sel.style.height = h + 'px';
                                    
                                    // Seçim koordinatlarını C# tarafına gönder
                                    window.external.SendSelectionToServer(x, y, w, h);
                                }}
                                
                                function endSelection() {{
                                    isSelecting = false;
                                }}
                                
                                window.onload = function() {{
                                    var img = document.querySelector('.image-wrapper img');
                                    img.addEventListener('mousedown', startSelection);
                                    img.addEventListener('mousemove', updateSelection);
                                    img.addEventListener('mouseup', endSelection);
                                }};
                            </script>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='image-wrapper'>
                                    <img src='{imageUrl}' alt='İmza Sirkülerini' />
                                    <div id='selection'></div>
                                </div>
                            </div>
                        </body>
                        </html>";

                    imageBox.Html = html;
                    lastRenderedImagePath = imagePath;
                    
                    // JavaScript'ten gelen seçim koordinatlarını almak için event handler ekle
                    imageBox.AttachEvent("SendSelectionToServer", OnSelectionUpdate);
                    
                    MessageBox.Show("İmza sirkülerini görüntüleniyor. İmzaları seçmek için mouse ile seçim yapabilirsiniz.");
                }
                else
                {
                    MessageBox.Show("İmza sirkülerini görüntülenemedi. Lütfen dosyanın PDF formatında olduğundan emin olun.");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug($"[BtnShowPdf_Click] Hata: {ex.Message}");
                MessageBox.Show($"İmza sirkülerini görüntülerken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnSelectionUpdate(object sender, EventArgs e)
        {
            // JavaScript'ten gelen seçim koordinatlarını al
            var args = e as CallBackEventArgs;
            if (args != null && args.Parameters.Length >= 4)
            {
                int x = Convert.ToInt32(args.Parameters[0]);
                int y = Convert.ToInt32(args.Parameters[1]);
                int width = Convert.ToInt32(args.Parameters[2]);
                int height = Convert.ToInt32(args.Parameters[3]);

                selectionRect = new Rectangle(x, y, width, height);
                btnSaveSignature.Enabled = width > 10 && height > 10; // Minimum seçim boyutu kontrolü
            }
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastRenderedImagePath) || selectionRect.IsEmpty)
            {
                MessageBox.Show("Lütfen önce bir imza seçin.");
                return;
            }

            try
            {
                using (var sourceImage = Image.FromFile(lastRenderedImagePath))
                {
                    // Seçim koordinatlarını orijinal resim boyutuna göre ölçekle
                    float scaleX = (float)sourceImage.Width / imageBox.Width;
                    float scaleY = (float)sourceImage.Height / imageBox.Height;

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

                        string outputPath = Path.Combine(_cdn, $"signature_{DateTime.Now.Ticks}.png");
                        cropImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);

                        MessageBox.Show($"İmza başarıyla kaydedildi:\n{outputPath}");
                        Logger.Instance.Debug($"[BtnSaveSignature_Click] İmza kaydedildi: {outputPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug($"[BtnSaveSignature_Click] Hata: {ex.Message}");
                MessageBox.Show($"İmza kaydedilirken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 