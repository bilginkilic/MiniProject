using System;
using System.Drawing;
using System.Collections.Generic;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;
// v1 - ModernAspxPopup.cs - Gizmox WebGUI için ModernAspxPopup sınıfı eklendi
namespace AspxExamples
{
    public class ModernAspxPopup : Form
    {
        private HtmlBox htmlBox;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private Timer closeCheckTimer;
        private const int CHECK_INTERVAL = 500; // 500ms

        public ModernAspxPopup(string aspxFileName)
        {
            InitializeComponent();
            LoadAspxContent(aspxFileName);
        }

        private void InitializeComponent()
        {
            // Timer ayarları
            closeCheckTimer = new Timer();
            closeCheckTimer.Interval = CHECK_INTERVAL;
            closeCheckTimer.Tick += new EventHandler(CheckCloseWindow);
            closeCheckTimer.Start();

            // Form ayarları
            this.Text = "Modern ASPX Viewer";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            this.MinimumSize = new Size(800, 600);

            // StatusStrip oluşturma
            CreateStatusStrip();

            // HtmlBox ayarları
            htmlBox = new HtmlBox();
            htmlBox.Dock = DockStyle.Fill;
            htmlBox.Margin = new Padding(10);
            htmlBox.BorderStyle = BorderStyle.None;
            htmlBox.BackColor = Color.White;
            htmlBox.AllowNavigation = false;  // Yeni pencerede açılmasını engelle
            htmlBox.WebBrowserShortcutsEnabled = false;  // Kısayolları devre dışı bırak
            htmlBox.ScriptErrorsSuppressed = true;  // Script hatalarını gösterme

            // Container panel
            Panel contentPanel = new Panel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.Padding = new Padding(10);
            contentPanel.Controls.Add(htmlBox);

            this.Controls.AddRange(new Control[] { contentPanel, statusStrip });
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
                
                // IE kontrolü yap
                if (IsInternetExplorer())
                {
                    // Edge'de aç
                    OpenInEdge(aspxUrl);
                }
                else
                {
                    // Normal HtmlBox'ta aç
                    htmlBox.Url = aspxUrl;
                    UpdateStatus("Sayfa yüklendi");
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
                UpdateStatus("Hata: Sayfa yüklenemedi");
            }
        }

        private bool IsInternetExplorer()
        {
            try
            {
                // Web tarayıcısı kontrolü
                var webBrowser = new System.Windows.Forms.WebBrowser();
                string userAgent = webBrowser.Version.ToString();
                return userAgent.Contains("Trident") || userAgent.Contains("MSIE");
            }
            catch
            {
                return false;
            }
        }

        private void OpenInEdge(string url)
        {
            try
            {
                // Edge'de açmak için ProcessStartInfo hazırla
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "microsoft-edge:" + url,
                    UseShellExecute = true
                };

                // Edge'i başlat
                System.Diagnostics.Process.Start(psi);

                // Form'u gizle
                this.Hide();

                // Mevcut closeCheckTimer zaten çalışıyor
                UpdateStatus("Sayfa Edge'de açıldı");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Edge'de açılırken hata: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // Edge açılamazsa normal HtmlBox'ta aç
                htmlBox.Url = url;
                UpdateStatus("Sayfa normal modda yüklendi");
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

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                if (htmlBox?.Url != null)
                {
                    // URL'e göre session'dan veriyi al
                    if (htmlBox.Url.Contains("PdfSignatureForm"))
                    {
                        var signatureData = SessionHelper.GetSignatureAuthData();
                        if (signatureData != null)
                        {
                            this.ResultData = signatureData;
                           // SessionHelper.ClearSignatureAuthData();
                        }
                    }
                    else if (htmlBox.Url.Contains("PdfViewer"))
                    {
                        var uploadedFile = SessionHelper.GetUploadedFile();
                        if (uploadedFile != null)
                        {
                            this.ResultData = uploadedFile;
                            //SessionHelper.ClearUploadedFile();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Session okuma hatası: {0}", ex.Message));
            }
            finally
            {
                // Tüm session değerlerini temizle
                SessionHelper.ClearCloseWindow();
                SessionHelper.ClearInitialYetkiliData();
                SessionHelper.ClearSignatureAuthData();
                SessionHelper.ClearUploadedFile();
            }

            base.OnClose(e);
        }


        private void UpdateStatus(string message)
        {
            statusLabel.Text = message;
        }


        private void CheckCloseWindow(object sender, EventArgs e)
        {
            try
            {
                // Session'dan closeWindow değerini kontrol et
                var closeWindow = SessionHelper.GetCloseWindow();
                if (closeWindow)
                {
                    // Session'ı temizle
                    SessionHelper.ClearCloseWindow();

                    // Sayfa tipine göre session'dan veriyi al
                    if (htmlBox.Url.Contains("PdfSignatureForm"))
                    {
                        var signatureData = SessionHelper.GetSignatureAuthData();
                        if (signatureData != null)
                        {
                            this.ResultData = signatureData;
                           // SessionHelper.ClearSignatureAuthData();
                        }
                    }
                    else if (htmlBox.Url.Contains("PdfViewer"))
                    {
                        var uploadedFile = SessionHelper.GetUploadedFile();
                        if (uploadedFile != null)
                        {
                            this.ResultData = uploadedFile;
                           // SessionHelper.ClearUploadedFile();
                        }
                    }

                    // Timer'ı durdur ve formu kapat
                    closeCheckTimer.Stop();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Close window kontrolü hatası: {0}", ex.Message));
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (closeCheckTimer != null)
            {
                closeCheckTimer.Stop();
                closeCheckTimer.Dispose();
            }
            base.OnFormClosing(e);
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
                    // Session'a veriyi kaydet
                    SessionHelper.SetInitialYetkiliData(initialDataList);
                    url = string.Format("PdfSignatureForm.aspx?ref={0}", circularRefNumber);
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