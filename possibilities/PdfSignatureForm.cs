using System;
using System.Drawing;
using System.IO;
using System.Threading;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace BtmuApps.UI.Forms.SIGN
{
    public class PdfSignatureForm : Form
    {
        private readonly string _cdn = @"\\trrgap3027\files\circular\cdn";
        private UploadControl uploadControl;
        private Button btnShowPdf;
        private HtmlBox imageBox;
        private Button btnSaveSignature;
        private Button btnUpdateSelection;
        private string lastUploadedPdfPath;
        private string lastRenderedImagePath;
        private Rectangle selectionRect;
        private HtmlBox hiddenBox; // Veri transferi için gizli HtmlBox

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
            this.btnUpdateSelection = new Button();
            this.hiddenBox = new HtmlBox(); // Gizli HtmlBox ekle

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

            // hiddenBox
            this.hiddenBox.ID = "54321"; // Sadece rakam
            this.hiddenBox.Html = "<input type='hidden' id='54321' name='54321' value='' />";
            this.hiddenBox.Visible = true; // HtmlBox'ı görünür yap ama içeriği gizli
            this.hiddenBox.Width = 1;
            this.hiddenBox.Height = 1;

            // btnUpdateSelection - gizli buton
            this.btnUpdateSelection.ID = "12345"; // Sadece rakam
            this.btnUpdateSelection.Name = "12345"; // Name özelliği de aynı
            this.btnUpdateSelection.Style.Display = "none"; // Tamamen gizleme, sadece görünmez yap
            this.btnUpdateSelection.Click += BtnUpdateSelection_Click;

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
            this.Controls.Add(this.btnUpdateSelection);
            this.Controls.Add(this.hiddenBox);
        }

        private void BtnUpdateSelection_Click(object sender, EventArgs e)
        {
            try
            {
                // HtmlBox'dan veriyi al
                string selectionData = null;
                
                // Önce form verisi olarak almayı dene
                if (Context.Request.Form != null && Context.Request.Form["54321"] != null)
                {
                    selectionData = Context.Request.Form["54321"];
                    Logger.Instance.Debug("[BtnUpdateSelection_Click] Form verisi alındı");
                }
                // Form verisi yoksa HtmlBox'tan almayı dene
                else
                {
                    selectionData = hiddenBox.InnerHtml ?? hiddenBox.Text;
                    Logger.Instance.Debug("[BtnUpdateSelection_Click] HtmlBox verisi alındı");
                }
                Logger.Instance.Debug(string.Format("[BtnUpdateSelection_Click] HtmlBox verisi alındı: {0}", selectionData));

                if (!string.IsNullOrEmpty(selectionData))
                {
                    string[] parts = selectionData.Split(',');
                    if (parts.Length >= 4)
                    {
                        int x = Convert.ToInt32(parts[0]);
                        int y = Convert.ToInt32(parts[1]);
                        int width = Convert.ToInt32(parts[2]);
                        int height = Convert.ToInt32(parts[3]);

                        selectionRect = new Rectangle(x, y, width, height);
                        btnSaveSignature.Enabled = true; // Her zaman aktif yap
                        
                        // Seçim verilerini ViewState'de sakla
                        ViewState["LastSelection"] = selectionData;
                        
                        Logger.Instance.Debug(string.Format("[BtnUpdateSelection_Click] Yeni seçim: X={0}, Y={1}, W={2}, H={3}, Buton Aktif={4}", 
                            x, y, width, height, btnSaveSignature.Enabled));

                        if (btnSaveSignature.Enabled)
                        {
                            MessageBox.Show("İmza seçimi yapıldı. Kaydetmek için 'Seçilen İmzayı Kaydet' butonuna tıklayın.");
                        }
                    }
                    else
                    {
                        Logger.Instance.Debug("[BtnUpdateSelection_Click] Hatalı seçim verisi formatı");
                    }
                }
                else
                {
                    Logger.Instance.Debug("[BtnUpdateSelection_Click] HtmlBox verisi boş");
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Debug(string.Format("[BtnUpdateSelection_Click] Seçim verisi işlenirken hata: {0}", ex.Message));
            }
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
                string imagePath = Path.Combine(_cdn, "page_1.png"); // PdfToImageAndCrop sınıfının kullandığı dosya adı formatı
                
                Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] PDF dönüşümü başlıyor. PDF: {0}, Hedef: {1}", lastUploadedPdfPath, imagePath));
                
                // PDF'yi PNG'ye çevir
                PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, _cdn);
                
                Logger.Instance.Debug(string.Format("[BtnShowPdf_Click] PDF dönüşümü tamamlandı. Dosya mevcut mu: {0}", File.Exists(imagePath)));

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

                    // ViewState'den son seçimi al
                    string lastSelection = ViewState["LastSelection"] as string;
                    
                    string html = GetJavaScript(base64Image, "12345");
                    imageBox.Html = html;

                    // Son seçimi hidden input'a yükle
                    if (!string.IsNullOrEmpty(lastSelection))
                    {
                        hiddenBox.Html = string.Format("<input type='hidden' id='54321' name='54321' value='{0}' />", lastSelection);
                    }
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

        private string GetJavaScript(string base64Image, string btnId)
        {
            return string.Format(@"
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
                <div id='selection' class='selecting'></div>
            </div>
        </div>
        <input type='hidden' id='54321' name='54321' value='' />
        <script>
            var isSelecting = false;
            var startX, startY;
            var selectionBox = document.getElementById('selection');
            var hiddenInput = document.getElementById('54321');
            var lastSelection = null;
            
            function startSelection(e) {{
                isSelecting = true;
                selectionBox.className = 'selecting';
                
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
                
                // Koordinatları tam sayıya yuvarla
                x = Math.round(x);
                y = Math.round(y);
                w = Math.round(w);
                h = Math.round(h);
                
                // Seçim çok küçükse iptal et
                if (w < 10 || h < 10) {{
                    selectionBox.className = 'selecting';
                    selectionBox.style.display = 'none';
                    return;
                }}
                
                // Seçimi sakla
                lastSelection = {{x: x, y: y, width: w, height: h}};
                
                // Seçim kutusunu kalıcı yap
                selectionBox.className = 'selected';
                selectionBox.style.left = x + 'px';
                selectionBox.style.top = y + 'px';
                selectionBox.style.width = w + 'px';
                selectionBox.style.height = h + 'px';

                // Veriyi gizli input'a kaydet
                if (hiddenInput) {{
                    var selectionData = x + ',' + y + ',' + w + ',' + h;
                    hiddenInput.value = selectionData;
                    hiddenInput.innerHTML = selectionData; // Hem value hem innerHTML'e kaydet
                    console.log('Selection data saved:', selectionData);
                    
                    // Butonu bul ve tıkla
                    var updateButton = document.getElementsByName('12345')[0];
                    console.log('Update button found:', !!updateButton);
                    
                    if (updateButton) {
                        console.log('Clicking update button...');
                        updateButton.click();
                    } else {
                        console.error('Update button not found, trying form submit...');
                        // Form submit as fallback
                        var form = document.createElement('form');
                        form.method = 'POST';
                        form.action = window.location.href;
                        form.style.display = 'none';
                    
                    var input = document.createElement('input');
                    input.type = 'submit';
                    input.name = '12345';
                    form.appendChild(input);
                    
                    // Hidden input'u da forma ekle
                    var dataInput = document.createElement('input');
                    dataInput.type = 'hidden';
                    dataInput.name = '54321';
                    dataInput.value = hiddenInput.value;
                    form.appendChild(dataInput);
                    
                    document.body.appendChild(form);
                    input.click();
                    document.body.removeChild(form);
                }} else {{
                    console.error('Hidden input not found');
                }}
            }}
            
            // Son seçimi yeniden çiz
            function restoreLastSelection() {{
                var savedData = hiddenInput.value;
                console.log('Restoring last selection:', savedData);
                
                if (savedData) {{
                    var parts = savedData.split(',');
                    if (parts.length >= 4) {{
                        var x = parseInt(parts[0]);
                        var y = parseInt(parts[1]);
                        var w = parseInt(parts[2]);
                        var h = parseInt(parts[3]);
                        
                        selectionBox.className = 'selected';
                        selectionBox.style.left = x + 'px';
                        selectionBox.style.top = y + 'px';
                        selectionBox.style.width = w + 'px';
                        selectionBox.style.height = h + 'px';
                        console.log('Selection restored');
                    }}
                }}
            }}

            window.onload = function() {{
                var img = document.querySelector('.image-wrapper img');
                img.addEventListener('mousedown', startSelection);
                img.addEventListener('mousemove', updateSelection);
                img.addEventListener('mouseup', endSelection);
                img.addEventListener('mouseleave', function(e) {{
                    if (isSelecting) {{
                        endSelection(e);
                    }}
                }});
                
                // Sayfa yüklendiğinde son seçimi göster
                setTimeout(restoreLastSelection, 100);
                
                console.log('Elements initialized:', {{
                    hiddenInput: !!hiddenInput,
                    selectionBox: !!selectionBox
                }});
            }};
        </script>
    </body>
    </html>", base64Image);
        }
    }
} 