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

        public ModalAspxPopup(string aspxFileName)
        {
            InitializeComponent();
            LoadAspxContent(aspxFileName);
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
                    $"ASPX sayfası yüklenirken hata: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }

    // Örnek kullanım sınıfı
    public class ModalPopupExample
    {
        public static void ShowModalPopup()
        {
            try
            {
                using (var modal = new ModalAspxPopup("ExamplePage.aspx"))
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