using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class CustomAspxPopup : Form
    {
        private HtmlBox htmlBox;
        private Panel headerPanel;
        private Label titleLabel;
        private Button closeButton;

        public CustomAspxPopup(string aspxFileName, string title, Size size)
        {
            InitializeComponent(title, size);
            LoadAspxContent(aspxFileName);
        }

        private void InitializeComponent(string title, Size size)
        {
            // Form ayarları
            this.Size = size;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            
            // Header panel
            headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 30;
            headerPanel.BackColor = Color.FromArgb(51, 51, 51);

            // Başlık
            titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.ForeColor = Color.White;
            titleLabel.Location = new Point(10, 8);
            titleLabel.AutoSize = true;

            // Kapatma butonu
            closeButton = new Button();
            closeButton.Text = "X";
            closeButton.Size = new Size(25, 25);
            closeButton.Location = new Point(size.Width - 30, 2);
            closeButton.FlatStyle = FlatStyle.Flat;
            closeButton.ForeColor = Color.White;
            closeButton.Click += (s, e) => this.Close();

            // HtmlBox
            htmlBox = new HtmlBox();
            htmlBox.Dock = DockStyle.Fill;
            
            // Kontrolleri ekleme
            headerPanel.Controls.AddRange(new Control[] { titleLabel, closeButton });
            this.Controls.AddRange(new Control[] { headerPanel, htmlBox });
        }

        private void LoadAspxContent(string aspxFileName)
        {
            try
            {
                string aspxUrl = AspxUrlHelper.GetAspxUrl(aspxFileName);
                htmlBox.Url = aspxUrl;
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

        // Sürükleme işlemleri için
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && e.Y <= headerPanel.Height)
            {
                this.Capture = false;
                headerPanel.Capture = true;
                Message msg = Message.Create(this.Handle, 0xA1, new IntPtr(2), IntPtr.Zero);
                this.WndProc(ref msg);
            }
            base.OnMouseDown(e);
        }
    }

    // Örnek kullanım sınıfı
    public class CustomPopupExample
    {
        public static void ShowCustomPopup()
        {
            try
            {
                var popup = new CustomAspxPopup(
                    "ExamplePage.aspx",
                    "Özel Popup",
                    new Size(800, 600)
                );
                popup.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("Özel popup açılırken hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}