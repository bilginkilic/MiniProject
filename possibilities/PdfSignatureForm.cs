using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;
using System.Drawing.Imaging;

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
        private HiddenField selectionDataField;

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
            this.selectionDataField = new HiddenField();

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

            // selectionDataField
            this.selectionDataField.ID = "selectionData";
            this.selectionDataField.TextChanged += SelectionDataField_TextChanged;

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
            this.Controls.Add(this.selectionDataField);
        }

        private void UploadControl_UploadComplete(object sender, UploadCompleteEventArgs e)
        {
            if (e.Error != null)
            {
                Logger.Instance.Debug(string.Format("[UploadControl_UploadComplete] Yükleme Hatası: {0}", e.Error.Message));
                MessageBox.Show(string.Format("İmza sirkülerini yüklerken hata oluştu:\n{0}", e.Error.Message), "Yükleme Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lastUploadedPdfPath = null;
                btnShowPdf.Enabled = false;
            }
            else
            {
                lastUploadedPdfPath = e.FilePath;
                btnShowPdf.Enabled = true;
                MessageBox.Show("İmza sirkülerini yüklendi. Göster butonuna tıklayarak imzaları seçebilirsiniz.");
                Logger.Instance.Debug(string.Format("[UploadControl_UploadComplete] İmza sirkülerini yüklendi: {0}", lastUploadedPdfPath));
            }
        }

        private void SelectionDataField_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string[] parts = selectionDataField.Text.Split(',');
                if (parts.Length >= 4)
                {
                    int x = Convert.ToInt32(parts[0]);
                    int y = Convert.ToInt32(parts[1]);
                    int width = Convert.ToInt32(parts[2]);
                    int height = Convert.ToInt32(parts[3]);

                    selectionRect = new Rectangle(x, y, width, height);
                    btnSaveSignature.Enabled = width > 10 && height > 10;
                    
                    Logger.Instance.Debug(string.Format("[SelectionDataField_TextChanged] Yeni seçim: X={0}, Y={1}, W={2}, H={3}", x, y, width, height));
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug(string.Format("[SelectionDataField_TextChanged] Seçim verisi işlenirken hata: {0}", ex.Message));
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
                    // Resmi base64'e çevir
                    string base64Image = "";
                    using (Image image = Image.FromFile(imagePath))
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.Save(ms, ImageFormat.Png);
                        byte[] imageBytes = ms.ToArray();
                        base64Image = Convert.ToBase64String(imageBytes);
                    }

                    string html = string.Format(@"
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
                                    max-width: 95%;
                                    max-height: 95%;
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
                                    z-index: 1000;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='image-wrapper'>
                                    <img src='data:image/png;base64,{0}' alt='İmza Sirkülerini' />
                                    <div id='selection'></div>
                                </div>
                            </div>
                            <script>
                                var isSelecting = false;
                                var startX, startY;
                                var selectionBox = document.getElementById('selection');
                                var hiddenField = document.getElementById('{1}');
                                
                                function startSelection(e) {{
                                    isSelecting = true;
                                    var rect = e.target.getBoundingClientRect();
                                    startX = e.clientX - rect.left;
                                    startY = e.clientY - rect.top;
                                    
                                    selectionBox.style.left = startX + 'px';
                                    selectionBox.style.top = startY + 'px';
                                    selectionBox.style.width = '0px';
                                    selectionBox.style.height = '0px';
                                    selectionBox.style.display = 'block';
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
                                    
                                    selectionBox.style.left = x + 'px';
                                    selectionBox.style.top = y + 'px';
                                    selectionBox.style.width = w + 'px';
                                    selectionBox.style.height = h + 'px';
                                }}
                                
                                function endSelection(e) {{
                                    if (!isSelecting) return;
                                    isSelecting = false;
                                    
                                    var img = document.querySelector('.image-wrapper img');
                                    var rect = img.getBoundingClientRect();
                                    var currentX = e.clientX - rect.left;
                                    var currentY = e.clientY - rect.top;
                                    
                                    var x = Math.min(startX, currentX);
                                    var y = Math.min(startY, currentY);
                                    var w = Math.abs(currentX - startX);
                                    var h = Math.abs(currentY - startY);
                                    
                                    // Seçim verilerini hidden field'a gönder
                                    var selectionData = [x, y, w, h].join(',');
                                    __doPostBack('{1}', selectionData);
                                }}
                                
                                window.onload = function() {{
                                    var img = document.querySelector('.image-wrapper img');
                                    img.addEventListener('mousedown', startSelection);
                                    img.addEventListener('mousemove', updateSelection);
                                    img.addEventListener('mouseup', endSelection);
                                    img.addEventListener('mouseleave', function() {{
                                        if (isSelecting) {{
                                            endSelection(event);
                                        }}
                                    }});
                                }};
                            </script>
                        </body>
                        </html>", base64Image, selectionDataField.ID);

                    imageBox.Html = html;
                    lastRenderedImagePath = imagePath;

                    MessageBox.Show("İmza sirkülerini görüntüleniyor. İmzaları seçmek için mouse ile seçim yapabilirsiniz.");
                }
                else
                {
                    MessageBox.Show("İmza sirkülerini görüntülenemedi. Lütfen dosyanın PDF formatında olduğundan emin olun.");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] Hata: {0}", ex.Message));
                MessageBox.Show(string.Format("İmza sirkülerini görüntülerken hata oluştu: {0}", ex.Message), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                        string outputPath = Path.Combine(_cdn, string.Format("signature_{0}.png", DateTime.Now.Ticks));
                        cropImage.Save(outputPath, ImageFormat.Png);

                        MessageBox.Show(string.Format("İmza başarıyla kaydedildi:\n{0}", outputPath));
                        Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] İmza kaydedildi: {0}", outputPath));
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] Hata: {0}", ex.Message));
                MessageBox.Show(string.Format("İmza kaydedilirken hata oluştu: {0}", ex.Message), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
} 