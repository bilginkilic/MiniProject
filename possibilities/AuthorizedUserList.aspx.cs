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
            System.Diagnostics.Debug.WriteLine(string.Format("İmza yolu güncellendi: {0}", signaturePath));
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
                    YetkiSuresi =new DateTime(2024,1,1)),
                    YetkiBitisTarihi = new DateTime(2024,1,1)),
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
            // Form alanlarını temizle
            txtYetkiliKontNo.Text = string.Empty;
            txtYetkiBitisTarihi.Text = string.Empty;
            ddlYetkiSekli.SelectedIndex = 0;
            ddlYetkiGrubu.SelectedIndex = 0;
            txtSinirliYetkiDetaylari.Text = string.Empty;
            ddlYetkiTurleri.SelectedIndex = 0;
            txtYetkiTutari.Text = string.Empty;
            ddlYetkiDovizCinsi.SelectedIndex = 0;
            ddlYetkiDurumu.SelectedIndex = 0;

            // JavaScript ile modal'ı aç
            ScriptManager.RegisterStartupScript(this, GetType(), "OpenUserForm", "openUserFormModal();", true);
        }

        protected void BtnSelectSignature_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                // Mevcut form verilerini Session'da sakla
                SaveFormDataToSession();
                
                // İmza formunu aç
                OpenSignatureForm();
            }
        }

        protected void BtnSaveUser_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    // Form verilerini al
                    var user = new AuthorizedUser
                    {
                        YetkiliKontNo = txtYetkiliKontNo.Text,
                        YetkiSekli = ddlYetkiSekli.SelectedValue,
                        YetkiSuresi = DateTime.Parse(txtYetkiBitisTarihi.Text),
                        YetkiBitisTarihi = DateTime.Parse(txtYetkiBitisTarihi.Text),
                        YetkiGrubu = ddlYetkiGrubu.SelectedValue,
                        SinirliYetkiDetaylari = txtSinirliYetkiDetaylari.Text,
                        YetkiTurleri = ddlYetkiTurleri.SelectedValue,
                        YetkiTutari = decimal.Parse(txtYetkiTutari.Text),
                        YetkiDovizCinsi = ddlYetkiDovizCinsi.SelectedValue,
                        YetkiDurumu = ddlYetkiDurumu.SelectedValue
                    };

                    // TODO: Veritabanına kaydet
                    // SaveUserToDatabase(user);

                    // Başarılı mesajı göster
                    ScriptManager.RegisterStartupScript(this, GetType(), "SaveSuccess", 
                        string.Format("showNotification('Yetkili kullanıcı başarıyla kaydedildi.', 'success'); closeUserFormModal();"), true);

                    // Listeyi yenile
                    LoadAuthorizedUsers();
                }
                catch (Exception ex)
                {
                    // Hata mesajı göster
                    ScriptManager.RegisterStartupScript(this, GetType(), "SaveError",
                        string.Format("showNotification('Hata oluştu: {0}', 'error');", ex.Message), true);
                }
            }
        }

        private void SaveFormDataToSession()
        {
            Session["TempUserData"] = new AuthorizedUser
            {
                YetkiliKontNo = txtYetkiliKontNo.Text,
                YetkiSekli = ddlYetkiSekli.SelectedValue,
                YetkiSuresi = DateTime.Parse(txtYetkiBitisTarihi.Text),
                YetkiBitisTarihi = DateTime.Parse(txtYetkiBitisTarihi.Text),
                YetkiGrubu = ddlYetkiGrubu.SelectedValue,
                SinirliYetkiDetaylari = txtSinirliYetkiDetaylari.Text,
                YetkiTurleri = ddlYetkiTurleri.SelectedValue,
                YetkiTutari = decimal.Parse(txtYetkiTutari.Text),
                YetkiDovizCinsi = ddlYetkiDovizCinsi.SelectedValue,
                YetkiDurumu = ddlYetkiDurumu.SelectedValue
            };
        }

        protected void GvAuthorizedUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string yetkiliKontNo = e.CommandArgument.ToString();
            
            if (e.CommandName == "EditUser")
            {
                // Düzenleme modunu aktifleştir
                hdnEditMode.Value = "true";
                hdnEditId.Value = yetkiliKontNo;
                
                // Kullanıcı bilgilerini getir
                var user = GetUserByKontNo(yetkiliKontNo);
                if (user != null)
                {
                    // Form alanlarını doldur
                    txtYetkiliKontNo.Text = user.YetkiliKontNo;
                    txtYetkiBitisTarihi.Text = user.YetkiBitisTarihi.ToString("yyyy-MM-dd");
                    ddlYetkiSekli.SelectedValue = user.YetkiSekli;
                    ddlYetkiGrubu.SelectedValue = user.YetkiGrubu;
                    txtSinirliYetkiDetaylari.Text = user.SinirliYetkiDetaylari;
                    ddlYetkiTurleri.SelectedValue = user.YetkiTurleri;
                    txtYetkiTutari.Text = user.YetkiTutari.ToString();
                    ddlYetkiDovizCinsi.SelectedValue = user.YetkiDovizCinsi;
                    ddlYetkiDurumu.SelectedValue = user.YetkiDurumu;
                    
                    // Modal'ı aç
                    ScriptManager.RegisterStartupScript(this, GetType(), "OpenUserForm", "openUserFormModal();", true);
                }
            }
            else if (e.CommandName == "DeleteUser")
            {
                try
                {
                    // TODO: Silme işlemi
                    // DeleteUser(yetkiliKontNo);
                    
                    // Başarılı mesajı göster
                    ScriptManager.RegisterStartupScript(this, GetType(), "DeleteSuccess",
                        string.Format("showNotification('Yetkili kullanıcı başarıyla silindi.', 'success');"), true);
                    
                    // Listeyi yenile
                    LoadAuthorizedUsers();
                }
                catch (Exception ex)
                {
                    // Hata mesajı göster
                    ScriptManager.RegisterStartupScript(this, GetType(), "DeleteError",
                        string.Format("showNotification('Silme işlemi sırasında hata oluştu: {0}', 'error');", ex.Message), true);
                }
            }
        }

        private AuthorizedUser GetUserByKontNo(string kontNo)
        {
            // TODO: Veritabanından kullanıcı bilgilerini getir
            // Bu örnek için statik veri dönüyoruz
            if (kontNo == "5000711")
            {
                return new AuthorizedUser
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
                };
            }
            return null;
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