using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class SimpleAspxPopup : Form
    {
        private HtmlBox htmlBox;

        public SimpleAspxPopup(string aspxUrl)
        {
            InitializeComponent();
            LoadAspxContent(aspxUrl);
        }

        private void InitializeComponent()
        {
            // Form ayarları
            this.Text = "ASPX Popup";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // HtmlBox ayarları
            htmlBox = new HtmlBox();
            htmlBox.Dock = DockStyle.Fill;
            htmlBox.BorderStyle = BorderStyle.None;
            
            this.Controls.Add(htmlBox);
        }

        private void LoadAspxContent(string aspxUrl)
        {
            htmlBox.Url = aspxUrl;
        }
    }

    // Örnek kullanım sınıfı
    public class SimplePopupExample
    {
        public static void ShowPopup()
        {
            try
            {
                string aspxUrl = "http://yourserver/yourpage.aspx";
                using (var popup = new SimpleAspxPopup(aspxUrl))
                {
                    popup.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Popup açılırken hata oluştu: " + ex.Message,
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}