using System;
using System.Drawing;
using Gizmox.WebGUI.Forms;
using Gizmox.WebGUI.Common;

namespace AspxExamples
{
    public class JavaScriptPopup : Form
    {
        private Button btnOpenPopup;
        private HtmlBox scriptBox;

        public JavaScriptPopup()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form ayarları
            this.Text = "JavaScript Popup Example";
            this.Size = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Button ayarları
            btnOpenPopup = new Button();
            btnOpenPopup.Text = "ASPX Sayfasını Aç";
            btnOpenPopup.Location = new Point(20, 20);
            btnOpenPopup.Size = new Size(150, 30);
            btnOpenPopup.Click += BtnOpenPopup_Click;

            // Script için HtmlBox
            scriptBox = new HtmlBox();
            scriptBox.Visible = false;

            this.Controls.AddRange(new Control[] { btnOpenPopup, scriptBox });
        }

        private void BtnOpenPopup_Click(object sender, EventArgs e)
        {
            try
            {
                OpenJavaScriptPopup("ExamplePage.aspx");
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

        private void OpenJavaScriptPopup(string aspxFileName)
        {
            string aspxUrl = AspxUrlHelper.GetAspxUrl(aspxFileName);
            string script = String.Format(@"
                window.open('{0}', 'AspxPopup', 
                'width=800,height=600,status=0,toolbar=0,menubar=0,location=0,scrollbars=1');
            ", aspxUrl);
            
            scriptBox.Html = String.Format("<script>{0}</script>", script);
        }
    }
}