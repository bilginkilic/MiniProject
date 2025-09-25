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

        // v2 - OpenInEdge metodu güncellendi - Process.Start için alternatif yöntemler eklendi
        private void OpenInEdge(string url)
        {
            try
            {
                // Detaylı loglama ekle
                System.Diagnostics.Debug.WriteLine(string.Format("Trying to open URL: {0}", url));
                System.Diagnostics.Debug.WriteLine(string.Format("Current User: {0}", 
                    System.Security.Principal.WindowsIdentity.GetCurrent().Name));
                System.Diagnostics.Debug.WriteLine(string.Format("Is Server: {0}", 
                    System.Environment.MachineName.Contains("SERVER")));
                System.Diagnostics.Debug.WriteLine(string.Format("Is Interactive: {0}", 
                    System.Environment.UserInteractive));

                // Farklı yöntemleri dene
                bool success = false;

                // 1. Yöntem: Edge protokolü
                try 
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = string.Format("microsoft-edge:{0}", url),
                        UseShellExecute = true,
                        Verb = "open",
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                    };
                    var process = System.Diagnostics.Process.Start(psi);
                    if (process != null)
                    {
                        System.Diagnostics.Debug.WriteLine("Edge protokolü başarılı");
                        success = true;
                    }
                }
                catch (Exception ex1)
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Edge protokolü hatası: {0}", ex1.Message));
                }

                // 2. Yöntem: CMD üzerinden
                if (!success)
                {
                    try 
                    {
                        var cmdPsi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            Arguments = string.Format("/c start microsoft-edge:{0}", url),
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };
                        var cmdProcess = System.Diagnostics.Process.Start(cmdPsi);
                        if (cmdProcess != null)
                        {
                            System.Diagnostics.Debug.WriteLine("CMD yöntemi başarılı");
                            success = true;
                        }
                    }
                    catch (Exception ex2)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("CMD yöntemi hatası: {0}", ex2.Message));
                    }
                }

                // 3. Yöntem: Default browser
                if (!success)
                {
                    try 
                    {
                        var defaultPsi = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = url,
                            UseShellExecute = true
                        };
                        var defaultProcess = System.Diagnostics.Process.Start(defaultPsi);
                        if (defaultProcess != null)
                        {
                            System.Diagnostics.Debug.WriteLine("Default browser yöntemi başarılı");
                            success = true;
                        }
                    }
                    catch (Exception ex3)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("Default browser hatası: {0}", ex3.Message));
                    }
                }

                if (!success)
                {
                    throw new Exception("Hiçbir açma yöntemi başarılı olmadı");
                }

                // Form'u gizle
                this.Hide();
                UpdateStatus("Sayfa açıldı");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Genel hata: {0}", ex.Message));
                MessageBox.Show(
                    String.Format("Sayfa açılırken hata: {0}", ex.Message),
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