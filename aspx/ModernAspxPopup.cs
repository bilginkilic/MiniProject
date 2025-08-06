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
        public byte[] SignatureImage { get; set; }
        public Rectangle SignatureArea { get; set; }
        public int PageNumber { get; set; }
    }

    public class ModernAspxPopup : Form
    {
        private HtmlBox htmlBox;
        private ToolStrip toolStrip;
        private StatusStrip statusStrip;
        private ToolStripButton btnFullScreen;
        private ToolStripButton btnZoomIn;
        private ToolStripButton btnZoomOut;
        private ToolStripButton btnPrint;
        private ToolStripStatusLabel statusLabel;
        private float currentZoom = 1.0f;
        private bool isFullScreen = false;
        private Size previousSize;
        private Point previousLocation;
        private FormWindowState previousState;
        
        private List<string> initialPdfPaths;
        private List<string> currentPdfPaths;
        public SignatureResult Result { get; private set; }

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

            // StatusStrip oluşturma
            CreateStatusStrip();

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

            this.Controls.AddRange(new Control[] { toolStrip, contentPanel, statusStrip });
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

        private void CreateStatusStrip()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel();
            statusLabel.Text = "Hazır";
            statusStrip.Items.Add(statusLabel);
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
                UpdateStatus("Sayfa yüklendi");
                
                // Sayfa kapanırken sonuçları topla
                htmlBox.Disposed += (s, e) =>
                {
                    try
                    {
                        // Seçilen imzaları al
                        var signatures = htmlBox.Document.GetElementById("hdnSignatures")?.GetAttribute("value");
                        if (!string.IsNullOrEmpty(signatures))
                        {
                            Result.SelectedSignatures = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SignatureInfo>>(signatures);
                        }
                        
                        // PDF listesindeki değişiklikleri kontrol et
                        var currentPdfList = htmlBox.Document.GetElementById("hdnCurrentPdfList")?.GetAttribute("value");
                        if (!string.IsNullOrEmpty(currentPdfList))
                        {
                            var finalPdfPaths = currentPdfList.Split(',').ToList();
                            
                            // Eklenen PDFler
                            Result.AddedPdfPaths = finalPdfPaths.Except(initialPdfPaths).ToList();
                            
                            // Silinen PDFler
                            Result.RemovedPdfPaths = initialPdfPaths.Except(finalPdfPaths).ToList();
                        }
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
                UpdateStatus("Hata: Sayfa yüklenemedi");
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
            UpdateStatus(isFullScreen ? "Tam ekran modu" : "Normal mod");
        }

        private void AdjustZoom(float delta)
        {
            currentZoom = Math.Max(0.5f, Math.Min(2.0f, currentZoom + delta));
            htmlBox.ZoomFactor = currentZoom;
            UpdateStatus($"Zoom: {currentZoom:P0}");
        }

        private void PrintContent()
        {
            try
            {
                htmlBox.Print();
                UpdateStatus("Yazdırma işlemi başlatıldı");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Yazdırma sırasında hata: {0}", ex.Message),
                    "Yazdırma Hatası",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                UpdateStatus("Hata: Yazdırılamadı");
            }
        }

        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }
    }

    // Örnek kullanım sınıfı
    public class ModernPopupExample
    {
        public static void ShowModernPopup(string aspxFileName)
        {
            try
            {
                using (var popup = new ModernAspxPopup(aspxFileName))
                {
                    popup.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Modern popup açılırken hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}