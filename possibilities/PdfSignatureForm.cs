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
        private System.Drawing.Image originalDisplayImage; // Orijinal resmi tutacak alan

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
            this.Size = new System.Drawing.Size(900, 700);

            // uploadControl
            this.uploadControl.Location = new System.Drawing.Point(20, 20);
            this.uploadControl.Width = 300;
            this.uploadControl.UploadComplete += UploadControl_UploadComplete;

            // btnShowPdf
            this.btnShowPdf.Text = "PDF'i Göster";
            this.btnShowPdf.Location = new System.Drawing.Point(340, 20);
            this.btnShowPdf.Click += BtnShowPdf_Click;
            this.btnShowPdf.Enabled = false;

            // pictureBoxPdfPage
            this.pictureBoxPdfPage.Location = new System.Drawing.Point(20, 60);
            this.pictureBoxPdfPage.Size = new System.Drawing.Size(800, 500);
            this.pictureBoxPdfPage.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBoxPdfPage.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBoxPdfPage.MouseDown += PictureBox_MouseDown;
            this.pictureBoxPdfPage.MouseMove += PictureBox_MouseMove;
            this.pictureBoxPdfPage.MouseUp += PictureBox_MouseUp;
            // this.pictureBoxPdfPage.Paint += PictureBox_Paint; // Bu satırı kaldırıyoruz!

            // btnSaveSignature
            this.btnSaveSignature.Text = "Seçimi İmza Olarak Kaydet";
            this.btnSaveSignature.Location = new System.Drawing.Point(20, 580);
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
            lastUploadedPdfPath = e.FilePath;
            btnShowPdf.Enabled = true;
            MessageBox.Show("PDF başarıyla yüklendi. Şimdi göster butonuna basabilirsiniz.");
        }

        private void BtnShowPdf_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastUploadedPdfPath))
            {
                string cdnFolder = "possibilities/cdn";
                System.IO.Directory.CreateDirectory(cdnFolder);
                string imagePath = System.IO.Path.Combine(cdnFolder, "page_1.png");
                PdfToImageAndCrop.ConvertPdfToImages(lastUploadedPdfPath, cdnFolder);
                if (System.IO.File.Exists(imagePath))
                {
                    originalDisplayImage = System.Drawing.Image.FromFile(imagePath);
                    pictureBoxPdfPage.Image = (System.Drawing.Image)originalDisplayImage.Clone(); // Orijinal resmi klonla ve göster
                    lastRenderedImagePath = imagePath;
                    btnSaveSignature.Enabled = false;
                    selectionRect = System.Drawing.Rectangle.Empty;
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
            selectionRect = new System.Drawing.Rectangle(e.Location, new System.Drawing.Size(0, 0));
            btnSaveSignature.Enabled = false;
            // Seçime başlarken resmi orijinal haline geri döndür
            pictureBoxPdfPage.Image = (System.Drawing.Image)originalDisplayImage.Clone();
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && pictureBoxPdfPage.Image != null)
            {
                int x = System.Math.Min(selectionStart.X, e.X);
                int y = System.Math.Min(selectionStart.Y, e.Y);
                int w = System.Math.Abs(selectionStart.X - e.X);
                int h = System.Math.Abs(selectionStart.Y - e.Y);
                selectionRect = new System.Drawing.Rectangle(x, y, w, h);

                // Orijinal resim üzerine çizim yap ve yeni resmi PictureBox'a ata
                if (originalDisplayImage != null)
                {
                    System.Drawing.Image tempImage = (System.Drawing.Image)originalDisplayImage.Clone();
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tempImage))
                    {
                        using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2))
                        {
                            g.DrawRectangle(pen, selectionRect);
                        }
                    }
                    pictureBoxPdfPage.Image = tempImage;
                }
            }
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            btnSaveSignature.Enabled = selectionRect.Width > 0 && selectionRect.Height > 0;
            // Seçim bittiğinde son dikdörtgeni çizili olarak bırak
            if (originalDisplayImage != null)
            {
                System.Drawing.Image tempImage = (System.Drawing.Image)originalDisplayImage.Clone();
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(tempImage))
                {
                    using (System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2))
                    {
                        g.DrawRectangle(pen, selectionRect);
                    }
                }
                pictureBoxPdfPage.Image = tempImage;
            }
        }
        
        // PictureBox_Paint metodunu kaldırıyoruz, çünkü artık Paint eventini kullanmıyoruz.
        /*
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
        */

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(lastRenderedImagePath) && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                // Orijinal resmin boyutuna göre seçim oranını hesapla
                using (var img = System.Drawing.Image.FromFile(lastRenderedImagePath))
                {
                    float scaleX = (float)img.Width / pictureBoxPdfPage.Width;
                    float scaleY = (float)img.Height / pictureBoxPdfPage.Height;
                    System.Drawing.Rectangle cropRect = new System.Drawing.Rectangle(
                        (int)(selectionRect.X * scaleX),
                        (int)(selectionRect.Y * scaleY),
                        (int)(selectionRect.Width * scaleX),
                        (int)(selectionRect.Height * scaleY)
                    );
                    string outputPath = System.IO.Path.Combine("possibilities/cdn", $"signature_{System.DateTime.Now.Ticks}.png");
                    PdfToImageAndCrop.CropImageAndSave(lastRenderedImagePath, cropRect, outputPath);
                    MessageBox.Show("Seçim imza olarak kaydedildi!
" + outputPath);
                }
            }
        }
    }
} 