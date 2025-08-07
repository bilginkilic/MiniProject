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
            // Yeni boş satır ekle
            var users = GetCurrentUsers();
            users.Insert(0, new AuthorizedUser
            {
                YetkiliKontNo = string.Format("{0:D7}", (DateTime.Now.Ticks % 10000000)),
                YetkiSekli = "Müştereken",
                YetkiSuresi = DateTime.Now.AddYears(1),
                YetkiBitisTarihi = DateTime.Now.AddYears(1),
                YetkiGrubu = "A Grubu",
                YetkiTurleri = "Kredi İşlemleri, Hazine İşlemleri",
                YetkiTutari = 0,
                YetkiDovizCinsi = "USD",
                YetkiDurumu = "Aktif"
            });

            gvAuthorizedUsers.DataSource = users;
            gvAuthorizedUsers.DataBind();
        }

        private List<AuthorizedUser> GetCurrentUsers()
        {
            // GridView'dan mevcut verileri al
            var users = new List<AuthorizedUser>();
            foreach (GridViewRow row in gvAuthorizedUsers.Rows)
            {
                var user = new AuthorizedUser
                {
                    YetkiliKontNo = ((TextBox)row.FindControl("txtYetkiliKontNo")).Text,
                    YetkiSekli = ((DropDownList)row.FindControl("ddlYetkiSekli")).SelectedValue,
                    YetkiBitisTarihi = DateTime.Parse(((TextBox)row.FindControl("txtYetkiBitisTarihi")).Text),
                    YetkiSuresi = DateTime.Parse(((TextBox)row.FindControl("txtYetkiBitisTarihi")).Text),
                    YetkiGrubu = ((DropDownList)row.FindControl("ddlYetkiGrubu")).SelectedValue,
                    SinirliYetkiDetaylari = ((TextBox)row.FindControl("txtSinirliYetkiDetaylari")).Text,
                    YetkiTurleri = ((DropDownList)row.FindControl("ddlYetkiTurleri")).SelectedValue,
                    YetkiTutari = decimal.Parse(((TextBox)row.FindControl("txtYetkiTutari")).Text),
                    YetkiDovizCinsi = ((DropDownList)row.FindControl("ddlYetkiDovizCinsi")).SelectedValue,
                    YetkiDurumu = ((DropDownList)row.FindControl("ddlYetkiDurumu")).SelectedValue
                };
                users.Add(user);
            }
            return users;
        }

        protected void GvAuthorizedUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            string yetkiliKontNo = e.CommandArgument.ToString();
            
            if (e.CommandName == "SaveUser")
            {
                try
                {
                    // Grid satırından verileri al
                    GridViewRow row = (GridViewRow)((Button)e.CommandSource).NamingContainer;
                    var user = new AuthorizedUser
                    {
                        YetkiliKontNo = ((TextBox)row.FindControl("txtYetkiliKontNo")).Text,
                        YetkiSekli = ((DropDownList)row.FindControl("ddlYetkiSekli")).SelectedValue,
                        YetkiBitisTarihi = DateTime.Parse(((TextBox)row.FindControl("txtYetkiBitisTarihi")).Text),
                        YetkiSuresi = DateTime.Parse(((TextBox)row.FindControl("txtYetkiBitisTarihi")).Text),
                        YetkiGrubu = ((DropDownList)row.FindControl("ddlYetkiGrubu")).SelectedValue,
                        SinirliYetkiDetaylari = ((TextBox)row.FindControl("txtSinirliYetkiDetaylari")).Text,
                        YetkiTurleri = ((DropDownList)row.FindControl("ddlYetkiTurleri")).SelectedValue,
                        YetkiTutari = decimal.Parse(((TextBox)row.FindControl("txtYetkiTutari")).Text),
                        YetkiDovizCinsi = ((DropDownList)row.FindControl("ddlYetkiDovizCinsi")).SelectedValue,
                        YetkiDurumu = ((DropDownList)row.FindControl("ddlYetkiDurumu")).SelectedValue
                    };

                    // TODO: Veritabanına kaydet
                    // SaveUserToDatabase(user);
                    
                    // Başarılı mesajı göster
                    ScriptManager.RegisterStartupScript(this, GetType(), "SaveSuccess",
                        string.Format("showNotification('Yetkili kullanıcı başarıyla kaydedildi.', 'success');"), true);
                    
                    // Listeyi yenile
                    LoadAuthorizedUsers();
                }
                catch (Exception ex)
                {
                    // Hata mesajı göster
                    ScriptManager.RegisterStartupScript(this, GetType(), "SaveError",
                        string.Format("showNotification('Kaydetme işlemi sırasında hata oluştu: {0}', 'error');", ex.Message), true);
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
            else if (e.CommandName == "SelectSignature")
            {
                // İmza seçim formunu aç
                OpenSignatureForm(yetkiliKontNo);
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