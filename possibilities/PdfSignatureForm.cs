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
                            // HTML içeriğini oluştur - iframe kullanarak
                            string imageUrl = VirtualPathUtility.ToAbsolute("~/cdn/page_1.png");
                            string html = $@"
                                <html>
                                <head>
                                    <meta http-equiv='Content-Type' content='application/png' />
                                    <style>
                                        body {{ 
                                            margin: 0; 
                                            padding: 0; 
                                            overflow: hidden; 
                                            width: 100%; 
                                            height: 100%; 
                                        }}
                                        iframe {{ 
                                            width: 100%; 
                                            height: 100%; 
                                            border: none;
                                            display: block;
                                        }}
                                    </style>
                                </head>
                                <body>
                                    <iframe src='{imageUrl}' type='application/png'></iframe>
                                </body>
                                </html>";
                            
                            imageBox.Html = html;
                            lastRenderedImagePath = imagePath;
                            btnSaveSignature.Enabled = true;
                            
                            MessageBox.Show("PDF'nin ilk sayfası ekranda gösteriliyor.");
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

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastRenderedImagePath))
            {
                try
                {
                    string outputPath = System.IO.Path.Combine(_cdn, string.Format("signature_{0}.png", DateTime.Now.Ticks));
                    System.IO.File.Copy(lastRenderedImagePath, outputPath);
                    MessageBox.Show(string.Format("Resim kaydedildi!\n{0}", outputPath));
                    Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] Resim başarıyla kaydedildi: {0}", outputPath));
                }
                catch (Exception ex)
                {
                    Logger.Instance.Debug(string.Format("[BtnSaveSignature_Click] Kaydetme hatası: {0}", ex.Message));
                    MessageBox.Show(string.Format("Resim kaydedilirken hata oluştu: {0}", ex.Message), "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
} 