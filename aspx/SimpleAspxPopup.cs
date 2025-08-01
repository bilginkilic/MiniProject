using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class SimpleAspxPopup : Form
    {
        private HtmlBox htmlBox;

        public SimpleAspxPopup(string aspxFileName)
        {
            InitializeComponent();
            LoadAspxContent(aspxFileName);
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
    public class SimplePopupExample
    {
        public static void ShowPopup()
        {
            try
            {
                using (var popup = new SimpleAspxPopup("ExamplePage.aspx"))
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