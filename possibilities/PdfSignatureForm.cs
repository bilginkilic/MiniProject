using System;
using System.Drawing;
using System.IO;
using Gizmox.WebGUI.Forms;

namespace Possibilities
{
    public class PdfSignatureForm : Form
    {
        private FileUpload fileUpload;
        private Button btnUpload;
        private Panel panelImages;
        private Button btnSaveSignature;
        private PictureBox selectedPictureBox;
        private Rectangle selectionRect;
        private bool isSelecting = false;
        private Point selectionStart;

        public PdfSignatureForm()
        {
            this.Text = "PDF İmza Seçimi";
            this.Size = new Size(900, 700);

            fileUpload = new FileUpload();
            fileUpload.Location = new Point(20, 20);
            fileUpload.Width = 300;
            this.Controls.Add(fileUpload);

            btnUpload = new Button();
            btnUpload.Text = "PDF Yükle";
            btnUpload.Location = new Point(340, 20);
            btnUpload.Click += BtnUpload_Click;
            this.Controls.Add(btnUpload);

            panelImages = new Panel();
            panelImages.Location = new Point(20, 60);
            panelImages.Size = new Size(800, 500);
            panelImages.AutoScroll = true;
            this.Controls.Add(panelImages);

            btnSaveSignature = new Button();
            btnSaveSignature.Text = "Seçimi İmza Olarak Kaydet";
            btnSaveSignature.Location = new Point(20, 580);
            btnSaveSignature.Click += BtnSaveSignature_Click;
            btnSaveSignature.Enabled = false;
            this.Controls.Add(btnSaveSignature);
        }

        private void BtnUpload_Click(object sender, EventArgs e)
        {
            if (fileUpload.HasFile)
            {
                string pdfPath = Path.Combine("possibilities/cdn", Path.GetFileName(fileUpload.FileName));
                fileUpload.SaveAs(pdfPath);
                // PDF'i resimlere çevir
                PdfToImageAndCrop.ConvertPdfToImages(pdfPath, "possibilities/cdn");
                ShowImages();
            }
        }

        private void ShowImages()
        {
            panelImages.Controls.Clear();
            string[] images = Directory.GetFiles("possibilities/cdn", "page_*.png");
            int y = 10;
            foreach (var imgPath in images)
            {
                PictureBox pb = new PictureBox();
                pb.Image = Image.FromFile(imgPath);
                pb.SizeMode = PictureBoxSizeMode.AutoSize;
                pb.Location = new Point(10, y);
                pb.BorderStyle = BorderStyle.FixedSingle;
                pb.MouseDown += Pb_MouseDown;
                pb.MouseMove += Pb_MouseMove;
                pb.MouseUp += Pb_MouseUp;
                pb.Tag = imgPath;
                panelImages.Controls.Add(pb);
                y += pb.Height + 20;
            }
        }

        private void Pb_MouseDown(object sender, MouseEventArgs e)
        {
            selectedPictureBox = sender as PictureBox;
            isSelecting = true;
            selectionStart = e.Location;
            selectionRect = new Rectangle(e.Location, new Size(0, 0));
            btnSaveSignature.Enabled = false;
            selectedPictureBox.Invalidate();
        }

        private void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            if (isSelecting && selectedPictureBox != null)
            {
                int x = Math.Min(selectionStart.X, e.X);
                int y = Math.Min(selectionStart.Y, e.Y);
                int w = Math.Abs(selectionStart.X - e.X);
                int h = Math.Abs(selectionStart.Y - e.Y);
                selectionRect = new Rectangle(x, y, w, h);
                selectedPictureBox.Invalidate();
            }
        }

        private void Pb_MouseUp(object sender, MouseEventArgs e)
        {
            isSelecting = false;
            btnSaveSignature.Enabled = selectionRect.Width > 0 && selectionRect.Height > 0;
            selectedPictureBox.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (selectedPictureBox != null && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                using (Pen pen = new Pen(Color.Red, 2))
                {
                    e.Graphics.DrawRectangle(pen, selectionRect);
                }
            }
        }

        private void BtnSaveSignature_Click(object sender, EventArgs e)
        {
            if (selectedPictureBox != null && selectionRect.Width > 0 && selectionRect.Height > 0)
            {
                string imgPath = selectedPictureBox.Tag as string;
                string outputPath = Path.Combine("possibilities/cdn", $"signature_{DateTime.Now.Ticks}.png");
                PdfToImageAndCrop.CropImageAndSave(imgPath, selectionRect, outputPath);
                MessageBox.Show("Seçim imza olarak kaydedildi!\n" + outputPath);
            }
        }
    }
} 