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
            this.Size = new Size(1024, 768); // Daha büyük varsayılan boyut
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable; // Yeniden boyutlandırılabilir
            this.MaximizeBox = true;
            this.MinimizeBox = true;
            
            // Minimum boyut sınırlaması
            this.MinimumSize = new Size(800, 600);

            // HtmlBox ayarları - Otomatik boyutlandırma için Dock özelliği
            htmlBox = new HtmlBox();
            htmlBox.Dock = DockStyle.Fill;
            htmlBox.Margin = new Padding(10);
            htmlBox.BorderStyle = BorderStyle.FixedSingle;

            // Alt panel için container
            Panel bottomPanel = new Panel();
            bottomPanel.Height = 50;
            bottomPanel.Dock = DockStyle.Bottom;
            bottomPanel.Padding = new Padding(10);

            // Kapatma butonu
            btnClose = new Button();
            btnClose.Text = "Kapat";
            btnClose.Size = new Size(100, 30);
            btnClose.Anchor = AnchorStyles.None; // Ortalamak için
            btnClose.Location = new Point((bottomPanel.Width - btnClose.Width) / 2, (bottomPanel.Height - btnClose.Height) / 2);
            btnClose.Click += (s, e) => this.Close();

            bottomPanel.Controls.Add(btnClose);
            this.Controls.AddRange(new Control[] { htmlBox, bottomPanel });
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
                    String.Format("Modal popup açılırken hata oluştu: {0}", ex.Message),
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}