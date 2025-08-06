using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class SignatureResult
    {
        public List<SignatureInfo> SelectedSignatures { get; set; }
        public List<string> AddedPdfPaths { get; set; }
        public List<string> RemovedPdfPaths { get; set; }
    }

    public class SignatureInfo
    {
        public string SourcePdfPath { get; set; }
        public string SavedImagePath { get; set; }
        public string SignatureImageUrl { get; set; }  // Base64 imza resmi URL'si
        public Rectangle SignatureArea { get; set; }
        public int PageNumber { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class ModernAspxPopup : Form
    {
        private HtmlBox htmlBox;
        private ToolStrip toolStrip;
        private ToolStripButton btnFullScreen;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnZoomOut;
        private ToolStripButton btnPrint;
        private float currentZoom = 1.0f;
        private bool isFullScreen = false;
        private Size previousSize;
        private Point previousLocation;
        private FormWindowState previousState;
        private List<string> initialPdfPaths;
        private List<string> currentPdfPaths;
        public SignatureResult Result { get; private set; }
        
        // Event to notify when signatures are selected
        public event EventHandler<SignatureResult> SignaturesSelected;

        public ModernAspxPopup(string aspxFileName, List<string> pdfPaths = null)
        {
            initialPdfPaths = pdfPaths ?? new List<string>();
            currentPdfPaths = new List<string>(initialPdfPaths);
            Result = new SignatureResult
            {
                SelectedSignatures = new List<SignatureInfo>(),
                AddedPdfPaths = new List<string>(),
                RemovedPdfPaths = new List<string>()
            };

            InitializeComponent();
            LoadAspxContent(aspxFileName);
        }

        private void InitializeComponent()
        {
            // Form ayarları
            this.Text = "Modern ASPX Viewer";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new Size(800, 600);

            // ToolStrip oluşturma
            CreateToolStrip();

            // HtmlBox ayarları
            htmlBox = new HtmlBox();
            htmlBox.Dock = DockStyle.Fill;
            htmlBox.Margin = new Padding(10);
            htmlBox.BorderStyle = BorderStyle.None;
            htmlBox.BackColor = Color.White;

            // Container panel
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(10);
            contentPanel.Controls.Add(htmlBox);

            this.Controls.AddRange(new Control[] { toolStrip, contentPanel });
        }

        private void CreateToolStrip()
        {
            toolStrip = new ToolStrip();
            toolStrip.RenderMode = ToolStripRenderMode.System;
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;

            // Tam Ekran butonu
            btnFullScreen = new ToolStripButton();
            btnFullScreen.Text = "⛶";
            btnFullScreen.ToolTipText = "Tam Ekran";
            btnFullScreen.Click += ToggleFullScreen;

            // Zoom butonları
            btnZoomIn = new ToolStripButton();
            btnZoomIn.Text = "🔍+";
            btnZoomIn.ToolTipText = "Yakınlaştır";
            btnZoomIn.Click += (s, e) => AdjustZoom(0.1f);

            btnZoomOut = new ToolStripButton();
            btnZoomOut.Text = "🔍-";
            btnZoomOut.ToolTipText = "Uzaklaştır";
            btnZoomOut.Click += (s, e) => AdjustZoom(-0.1f);

            // Yazdırma butonu
            btnPrint = new ToolStripButton();
            btnPrint.Text = "🖨️";
            btnPrint.ToolTipText = "Yazdır";
            btnPrint.Click += (s, e) => PrintContent();

            // Ayırıcı
            ToolStripSeparator separator = new ToolStripSeparator();

            // Butonları ToolStrip'e ekleme
            toolStrip.Items.AddRange(new ToolStripItem[] {
                btnFullScreen,
                separator,
                btnZoomIn,
                btnZoomOut,
                new ToolStripSeparator(),
                btnPrint
            });
        }



        private void LoadAspxContent(string aspxFileName)
        {
            try
            {
                string aspxUrl = AspxUrlHelper.GetAspxUrl(aspxFileName);
                
                // PDF listesini URL parametresi olarak ekle
                if (currentPdfPaths != null && currentPdfPaths.Count > 0)
                {
                    string pdfListParam = string.Join(",", currentPdfPaths);
                    aspxUrl += (aspxUrl.Contains("?") ? "&" : "?") + "pdfList=" + Uri.EscapeDataString(pdfListParam);
                }
                
                htmlBox.Url = aspxUrl;

                // Form kapanırken sonuçları topla
                this.FormClosing += (s, e) => 
                {
                    CollectResults();
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("ASPX sayfası yüklenirken hata: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void CollectResults()
        {
            try
            {
                // Seçilen imzaları al
                string signaturesScript = @"
                    (function() {
                        try {
                            var hdnSignatures = document.getElementById(""hdnSignatures"");
                            if (!hdnSignatures) return ""[]"";
                            
                            var signatures = JSON.parse(hdnSignatures.value || ""[]"");
                            if (!Array.isArray(signatures)) return ""[]"";

                            for (var i = 0; i < signatures.length; i++) {
                                var sig = signatures[i];
                                var slot = document.querySelector("".signature-slot[data-slot='"" + (i + 1) + ""']"");
                                if (!slot) continue;

                                var image = slot.querySelector("".slot-image"");
                                if (!image) continue;

                                var style = window.getComputedStyle(image);
                                var bgImage = style.backgroundImage || """";
                                
                                if (bgImage.indexOf(""url("") === 0) {
                                    var urlContent = bgImage.substring(4, bgImage.length - 1).replace(/[""']/g, """");
                                    if (urlContent.indexOf(""data:image"") === 0) {
                                        sig.SignatureImageUrl = urlContent;
                                    }
                                }
                            }
                            return JSON.stringify(signatures);
                        } catch(e) {
                            console.error(""Signature script error:"", e);
                            return ""[]"";
                        }
                    })();
                ";
                string signatures = htmlBox.EvaluateScript(signaturesScript)?.ToString();
                if (!string.IsNullOrEmpty(signatures))
                {
                    Result.SelectedSignatures = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SignatureInfo>>(signatures);
                }
                
                // PDF listesindeki değişiklikleri kontrol et
                string pdfListScript = "document.getElementById('hdnCurrentPdfList') ? document.getElementById('hdnCurrentPdfList').value : ''";
                string currentPdfList = htmlBox.EvaluateScript(pdfListScript)?.ToString();
                if (!string.IsNullOrEmpty(currentPdfList))
                {
                    var finalPdfPaths = currentPdfList.Split(',').ToList();
                    
                    // Eklenen PDFler
                    Result.AddedPdfPaths = finalPdfPaths.Except(initialPdfPaths).ToList();
                    
                    // Silinen PDFler
                    Result.RemovedPdfPaths = initialPdfPaths.Except(finalPdfPaths).ToList();
                }

                // Event'i tetikle
                SignaturesSelected?.Invoke(this, Result);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Sonuçlar alınırken hata: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("ASPX sayfası yüklenirken hata: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void ToggleFullScreen(object sender, EventArgs e)
        {
            if (!isFullScreen)
            {
                // Mevcut durumu kaydet
                previousSize = this.Size;
                previousLocation = this.Location;
                previousState = this.WindowState;

                // Tam ekran yap
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Maximized;
                btnFullScreen.Text = "⛶";
            }
            else
            {
                // Önceki duruma geri dön
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Size = previousSize;
                this.Location = previousLocation;
                this.WindowState = previousState;
                btnFullScreen.Text = "⛶";
            }

            isFullScreen = !isFullScreen;
        }

        private void AdjustZoom(float delta)
        {
            currentZoom = Math.Max(0.5f, Math.Min(2.0f, currentZoom + delta));
            htmlBox.ZoomFactor = currentZoom;
        }

        private void PrintContent()
        {
            try
            {
                htmlBox.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Yazdırma sırasında hata: {0}", ex.Message),
                    "Yazdırma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }


    }

    // Örnek kullanım sınıfı
    public class ModernPopupExample
    {
        public static SignatureResult ShowPdfSignatureForm(List<string> pdfPaths)
        {
            try
            {
                using (var popup = new ModernAspxPopup("PdfSignatureForm.aspx", pdfPaths))
                {
                    // İmza seçildiğinde çalışacak event handler
                    popup.SignaturesSelected += (sender, result) =>
                    {
                        // İmzalar seçildiğinde yapılacak işlemler
                        if (result.SelectedSignatures != null && result.SelectedSignatures.Count > 0)
                        {
                            foreach (var signature in result.SelectedSignatures)
                            {
                                // İmza bilgilerini kullan
                                var sourcePdf = signature.SourcePdfPath;
                                var savedImage = signature.SavedImagePath;
                                var area = signature.SignatureArea;
                                var page = signature.PageNumber;
                            }
                        }

                        // PDF listesi değişikliklerini kontrol et
                        if (result.AddedPdfPaths != null && result.AddedPdfPaths.Count > 0)
                        {
                            // Yeni eklenen PDF'ler
                        }

                        if (result.RemovedPdfPaths != null && result.RemovedPdfPaths.Count > 0)
                        {
                            // Silinen PDF'ler
                        }
                    };

                    popup.ShowDialog();
                    return popup.Result;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("İmza seçim penceresi açılırken hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return null;
            }
        }
    }
}