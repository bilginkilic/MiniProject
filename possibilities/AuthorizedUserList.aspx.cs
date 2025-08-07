using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;

namespace AspxExamples
{
    public partial class AuthorizedUserList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadAuthorizedUsers();
            }
        }

        private void LoadAuthorizedUsers()
        {
            // TODO: Veritabanından yetkili kullanıcıları yükle
            var users = new List<AuthorizedUser>
            {
                new AuthorizedUser
                {
                    YetkiliKontNo = "5000711",
                    YetkiliAdiSoyadi = "Toru Kawai",
                    YetkiSekli = "Müştereken",
                    YetkiSuresi = DateTime.Parse("14.07.2024"),
                    YetkiBitisTarihi = DateTime.Parse("14.07.2024"),
                    YetkiGrubu = "A Grubu",
                    SinirliYetkiDetaylari = "C İLE BİRLİKTE 1.000.000 USD",
                    YetkiTurleri = "Kredi Sözleşmeleri / Transfer İşlemleri",
                    ImzaOrnegi1 = "signature1.png",
                    ImzaOrnegi2 = "signature2.png",
                    ImzaOrnegi3 = "signature3.png",
                    YetkiTutari = 100000,
                    YetkiDovizCinsi = "USD",
                    YetkiDurumu = "Aktif"
                }
            };

            gvAuthorizedUsers.DataSource = users;
            gvAuthorizedUsers.DataBind();
        }

        protected void BtnAddNew_Click(object sender, EventArgs e)
        {
            OpenSignatureForm();
        }

        protected void GvAuthorizedUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditUser")
            {
                string yetkiliKontNo = e.CommandArgument.ToString();
                OpenSignatureForm(yetkiliKontNo);
            }
            else if (e.CommandName == "DeleteUser")
            {
                string yetkiliKontNo = e.CommandArgument.ToString();
                // TODO: Silme işlemi
            }
        }

        private void OpenSignatureForm(string yetkiliKontNo = null)
        {
            string url = "PdfSignatureForm.aspx";
            if (!string.IsNullOrEmpty(yetkiliKontNo))
            {
                url += string.Format("?yetkiliKontNo={0}", yetkiliKontNo);
            }

            // PdfSignatureForm'u normal pencerede aç
            string script = string.Format("window.open('{0}', '_blank', 'width=1024,height=768,scrollbars=yes,resizable=yes');", url);
            ScriptManager.RegisterStartupScript(this, GetType(), "OpenSignatureForm", script, true);
        }

        protected string GetSignatureUrl(object signaturePath)
        {
            if (signaturePath == null || string.IsNullOrEmpty(signaturePath.ToString()))
                return "";

            // TODO: CDN yolunu configden al
            return string.Format("/cdn/{0}", signaturePath);
        }
    }

    public class AuthorizedUser
    {
        public string YetkiliKontNo { get; set; }
        public string YetkiliAdiSoyadi { get; set; }
        public string YetkiSekli { get; set; }
        public DateTime YetkiSuresi { get; set; }
        public DateTime YetkiBitisTarihi { get; set; }
        public string YetkiGrubu { get; set; }
        public string SinirliYetkiDetaylari { get; set; }
        public string YetkiTurleri { get; set; }
        public string ImzaOrnegi1 { get; set; }
        public string ImzaOrnegi2 { get; set; }
        public string ImzaOrnegi3 { get; set; }
        public decimal YetkiTutari { get; set; }
        public string YetkiDovizCinsi { get; set; }
        public string YetkiDurumu { get; set; }
    }
}