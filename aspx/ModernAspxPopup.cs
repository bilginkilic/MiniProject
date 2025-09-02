using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class ModernAspxPopup : Form
    {
        public SignatureAuthData ResultData { get; private set; }
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

        public ModernAspxPopup(string aspxFileName)
        {
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
                htmlBox.Url = aspxUrl;
                UpdateStatus("Sayfa yüklendi");
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

                    private string referenceId;

                    public ModernAspxPopup(string aspxFileName, string refId = null)
        {
            this.referenceId = refId;
            this.aspxFileName = aspxFileName;
            InitializeComponent();
            LoadAspxContent(aspxFileName);
        }



        public object ResultData { get; private set; }
        private string aspxFileName;

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                // JavaScript'ten gelen kapanma isteğini kontrol et
                if (htmlBox.Document != null)
                {
                    var closeRequested = htmlBox.Document.GetElementById("hdnIsReturnRequested");
                    if (closeRequested != null && closeRequested.GetAttribute("value") == "true")
                    {
                        // Sayfa tipine göre session'dan veriyi al
                        if (aspxFileName.Contains("PdfSignatureForm"))
                        {
                            var signatureData = SessionHelper.GetSignatureAuthData();
                            if (signatureData != null)
                            {
                                this.ResultData = signatureData;
                                SessionHelper.ClearSignatureAuthData();
                            }
                        }
                        else if (aspxFileName.Contains("FileUploadViewer"))
                        {
                            var uploadedFile = SessionHelper.GetUploadedFile();
                            if (uploadedFile != null)
                            {
                                this.ResultData = uploadedFile;
                                SessionHelper.ClearUploadedFile();
                            }
                        }
                        e.Cancel = false;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Session okuma hatası: " + ex.Message);
            }

            base.OnClosing(e);
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
            UpdateStatus(String.Format("Zoom: {0:P0}", currentZoom));
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
        public static SignatureAuthData ShowAuthorizedUserList(string circularRefNumber, List<YetkiliKayit> initialDataList = null)
        {
            try
            {
                string url;
                if (initialDataList != null && initialDataList.Any())
                {
                    // Veriyi JSON'a çevir ve Base64 encode et
                    var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
                    string jsonData = serializer.Serialize(initialDataList);
                    string base64Data = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));
                    url = string.Format("PdfSignatureForm.aspx?ref={0}&data={1}", circularRefNumber, base64Data);
                }
                else
                {
                    url = string.Format("PdfSignatureForm.aspx?ref={0}", circularRefNumber);
                }

                using (var popup = new ModernAspxPopup(url, circularRefNumber))
                {
                    popup.Text = "İmza Sirkülerinden İmza Seçimi";
                    popup.Size = new Size(1280, 900);
                    popup.MinimumSize = new Size(1024, 768);
                    popup.ShowDialog();
                    return popup.ResultData as SignatureAuthData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("İmza seçimi açılırken hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return null;
            }
        }

        public static UploadedFileResult ShowFileUploader()
        {
            try
            {
                string url = "FileUploadViewer.aspx";
                using (var popup = new ModernAspxPopup(url))
                {
                    popup.Text = "Dosya Yükleme";
                    popup.Size = new Size(1280, 800);
                    popup.MinimumSize = new Size(1024, 768);
                    popup.ShowDialog();
                    return popup.ResultData as UploadedFileResult;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Dosya yükleme sırasında hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return null;
            }
        }
    }
}