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
            else
            {
                // İmza yolu dönüşünü kontrol et
                string signaturePath = hdnSignaturePath.Value;
                if (!string.IsNullOrEmpty(signaturePath))
                {
                    // İmza yolunu temizle
                    hdnSignaturePath.Value = "";
                    
                    // İmza yolunu kullanarak veritabanını güncelle
                    UpdateSignaturePath(signaturePath);
                    
                    // Listeyi yenile
                    LoadAuthorizedUsers();
                }
            }
        }

        private void UpdateSignaturePath(string signaturePath)
        {
            // TODO: Veritabanında imza yolunu güncelle
            // Bu örnek için sadece debug log yazıyoruz
            System.Diagnostics.Debug.WriteLine($"İmza yolu güncellendi: {signaturePath}");
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
                    ImzaOrnegi1 = "http://example.com/signatures/signature1.png",
                    ImzaOrnegi2 = "http://example.com/signatures/signature2.png",
                    ImzaOrnegi3 = "http://example.com/signatures/signature3.png",
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

            // PdfSignatureForm'u modal olarak aç
            string script = String.Format("openSignatureModal('{0}');", yetkiliKontNo);
            ScriptManager.RegisterStartupScript(this, GetType(), "OpenSignatureForm", script, true);
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