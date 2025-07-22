using System;
using System.Drawing;
using System.IO;
using Gizmox.WebGUI.Forms;

namespace Possibilities
{
    public class PdfSignatureForm : Form
    {
        private UploadControl uploadControl;
        private Button btnShowPdf;
        private PictureBox pictureBoxPdfPage;
        private Button btnSaveSignature;
        private Rectangle selectionRect;
        private bool isSelecting = false;
        private Point selectionStart;
        private string lastUploadedPdfPath;
        private string lastRenderedImagePath;

        public PdfSignatureForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.uploadControl = new UploadControl();
            this.btnShowPdf = new Button();
            this.pictureBoxPdfPage = new PictureBox();
            this.btnSaveSignature = new Button();

            // Form
            this.Text = "PDF İmza Seçimi";
            this.Size = new Size(900, 700);

            // uploadControl
            this.uploadControl.Location = new Point(20, 20);
            this.uploadControl.Width = 300;
            this.uploadControl.UploadComplete += UploadControl_UploadComplete;

            // btnShowPdf
            this.btnShowPdf.Text = "PDF'i Göster";
            this.btnShowPdf.Location = new Point(340, 20);
            this.btnShowPdf.Click += BtnShowPdf_Click;
            this.btnShowPdf.Enabled = false;

            // pictureBoxPdfPage
            this.pictureBoxPdfPage.Location = new Point(20, 60);
            this.pictureBoxPdfPage.Size = new Size(800, 500);
            this.pictureBoxPdfPage.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBoxPdfPage.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBoxPdfPage.MouseDown += PictureBox_MouseDown;
            this.pictureBoxPdfPage.MouseMove += PictureBox_MouseMove;
            this.pictureBoxPdfPage.MouseUp += PictureBox_MouseUp;
            this.pictureBoxPdfPage.Paint += PictureBox_Paint;

            // btnSaveSignature
            this.btnSaveSignature.Text = "Seçimi İmza Olarak Kaydet";
            this.btnSaveSignature.Location = new Point(20, 580);
            this.btnSaveSignature.Click += BtnSaveSignature_Click;
            this.btnSaveSignature.Enabled = false;

            // Controls
            this.Controls.Add(this.uploadControl);
            this.Controls.Add(this.btnShowPdf);
            this.Controls.Add(this.pictureBoxPdfPage);
            this.Controls.Add(this.btnSaveSignature);
        }

        private void UploadControl_UploadComplete(object sender, UploadCompleteEventArgs e)
        {
            // Dosya başarıyla yüklendiğinde path'i kaydet
            lastUploadedPdfPath = e.FilePath;
            btnShowPdf.Enabled = true;
            MessageBox.Show("PDF başarıyla yüklendi. Şimdi göster butonuna basabilirsiniz.");
        }

        private void BtnShowPdf_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastUploadedPdfPath))
            {
                // Sadece ilk sayfayı resme çevir
                string cdnFolder = "possibilities/cdn";
                Directory.CreateDirectory(cdnFolder);
                string imagePath = Path.Combine(cdnFolder, "page_1.png");
                PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, cdnFolder);
                if (File.Exists(imagePath))
                {
                    pictureBoxPdfPage.Image = Image.FromFile(imagePath);
                    lastRenderedImagePath = imagePath;
                    btnSaveSignature.Enabled = false;
                    selectionRect = Rectangle.Empty;
                }
                else
                {
                    MessageBox.Show("PDF'den resim oluşturulamadı.");
                }
            }
        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (pictureBoxPdfPage.Image == null) return;
            isSelecting = true;
            selectionStart = e.Location;
            selectionRect = new Rectangle(e.Location, new Size(0, 0));
            btnSaveSignature.Enabled = false;
            pictureBoxPdfPage.Invalidate();
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && pictureBoxPdfPage.Image != null)
            {
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int w = Math.Abs(selectionStart.X - e.X);
                int h = Math.Abs(selectionStart.Y - e.Y);
                selectionRect = new Rectangle(x, y, w, h);
                pictureBoxPdfPage.Invalidate();
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            btnSaveSignature.Enabled = selectionRect.Width > 0 && selectionRect.Height > 0;
            pictureBoxPdfPage.Invalidate();
        }

        private void PictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBoxPdfPage.Image != null && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastRenderedImagePath) && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                // Orijinal resmin boyutuna göre seçim oranını hesapla
                using (var img = Image.FromFile(lastRenderedImagePath))
                {
                    float scaleX = (float)img.Width / pictureBoxPdfPage.Width;
                    float scaleY = (float)img.Height / pictureBoxPdfPage.Height;
                    Rectangle cropRect = new Rectangle(
                        (int)(selectionRect.X * scaleX),
                        (int)(selectionRect.Y * scaleY),
                        (int)(selectionRect.Width * scaleX),
                        (int)(selectionRect.Height * scaleY)
                    );
                    string outputPath = Path.Combine("possibilities/cdn", $"signature_{DateTime.Now.Ticks}.png");
                    PdfToImageAndCrop.CropImageAndSave(lastRenderedImagePath, cropRect, outputPath);
                    MessageBox.Show("Seçim imza olarak kaydedildi!\n" + outputPath);
                }
            }
        }
    }
} 