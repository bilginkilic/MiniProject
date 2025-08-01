using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class ModalAspxPopup : Form
    {
        private HtmlBox htmlBox;
        private Button btnClose;

        public ModalAspxPopup(string aspxUrl)
        {
            InitializeComponent();
            LoadAspxContent(aspxUrl);
        }

        private void InitializeComponent()
        {
            // Form ayarları
            this.Text = "Modal ASPX";
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // HtmlBox ayarları
            htmlBox = new HtmlBox();
            htmlBox.Size = new Size(780, 570);
            htmlBox.Location = new Point(10, 10);
            htmlBox.BorderStyle = BorderStyle.FixedSingle;

            // Kapatma butonu
            btnClose = new Button();
            btnClose.Text = "Kapat";
            btnClose.Size = new Size(100, 30);
            btnClose.Location = new Point(350, 590);
            btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { htmlBox, btnClose });
        }

        private void LoadAspxContent(string aspxUrl)
        {
            htmlBox.Url = aspxUrl;
        }
    }

    // Örnek kullanım sınıfı
    public class ModalPopupExample
    {
        public static void ShowModalPopup()
        {
            try
            {
                string aspxUrl = "http://yourserver/yourpage.aspx";
                using (var modal = new ModalAspxPopup(aspxUrl))
                {
                    modal.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Modal popup açılırken hata oluştu: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}