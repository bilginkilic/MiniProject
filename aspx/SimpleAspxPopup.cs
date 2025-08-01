using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class SimpleAspxPopup : Form
    {
        private HtmlBox htmlBox;
        private Label lblDebug;

        public SimpleAspxPopup(string aspxFileName)
        {
            InitializeComponent();
            LoadAspxContent(aspxFileName);
        }

        private void InitializeComponent()
        {
            // Form ayarları
            this.Text = "ASPX Popup";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Debug label
            lblDebug = new Label();
            lblDebug.Location = new Point(10, 610);
            lblDebug.Size = new Size(780, 80);
            lblDebug.AutoSize = false;
            lblDebug.BorderStyle = BorderStyle.FixedSingle;

            // HtmlBox ayarları
            htmlBox = new HtmlBox();
            htmlBox.Size = new Size(780, 590);
            htmlBox.Location = new Point(10, 10);
            htmlBox.BorderStyle = BorderStyle.None;
            
            this.Controls.AddRange(new Control[] { htmlBox, lblDebug });
        }

        private void LoadAspxContent(string aspxFileName)
        {
            try
            {
                // Debug bilgisini al
                string debugInfo = AspxUrlHelper.GetDebugUrl(aspxFileName);
                lblDebug.Text = debugInfo;

                // URL'yi al ve yükle
                string aspxUrl = AspxUrlHelper.GetAspxUrl(aspxFileName);
                htmlBox.Url = aspxUrl;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    String.Format("ASPX sayfası yüklenirken hata: {0}\n\nDebug Info:\n{1}", ex.Message, lblDebug.Text),
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
                    String.Format("Popup açılırken hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}