// Created: 2024.01.17 14:30 - v1
using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace AspxExamples
{
    public class SignatureData
    {
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Image { get; set; }
        public string SourcePdfPath { get; set; }
    }

    public class SavedSignature
    {
        public string Path { get; set; }
        public int Page { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class YetkiliImza
    {
        public string Base64Image { get; set; }
        public int SlotIndex { get; set; }
    }

    public class YetkiliKayit
    {
        public string YetkiliKontakt { get; set; }
        public string YetkiliAdi { get; set; }
        public string YetkiSekli { get; set; }
        public string YetkiTarihi { get; set; }
        public bool AksiKararaKadar { get; set; }
        public string SinirliYetkiDetaylari { get; set; }
        public string YetkiTurleri { get; set; }
        public List<YetkiliImza> Imzalar { get; set; }
        public string YetkiTutari { get; set; }
        public string YetkiDovizCinsi { get; set; }
        public string YetkiDurumu { get; set; }
        public string IslemTipi { get; set; } // "Ekle", "Guncelle", "Sil"
    }

    public class YetkiliKayitResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Error { get; set; }
        public YetkiliKayit Data { get; set; }
    }

    public class SignatureAuthData
    {
        public string KaynakPdfAdi { get; set; }
        public List<YetkiliData> Yetkililer { get; set; }
    }

    public class YetkiliData
    {
        public string YetkiliKontakt { get; set; }
        public string YetkiliAdi { get; set; }
        public string YetkiSekli { get; set; }
        public string YetkiTarihi { get; set; }
        public string YetkiBitisTarihi { get; set; }
        public string YetkiGrubu { get; set; }
        public string SinirliYetkiDetaylari { get; set; }
        public string YetkiTurleri { get; set; }
        public decimal YetkiTutari { get; set; }
        public string YetkiDovizCinsi { get; set; }
        public string YetkiDurumu { get; set; }
        public List<SignatureImage> Imzalar { get; set; }
    }

    public class SignatureImage
    {
        public string ImageData { get; set; }
        public int SiraNo { get; set; }
        public string SourcePdfPath { get; set; }
    }

namespace AspxExamples
{
    public partial class Circular : System.Web.UI.Page
    {
        private string _cdn = @"\\trrgap3027\files\circular\cdn";
        private string _cdnVirtualPath = "/cdn"; // Web'den erişim için virtual path

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadCirculars();
                InitializeForm();
            }
        }

        private void LoadCirculars()
        {
            try
            {
                // TODO: Implement actual data loading from database
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("Detail", typeof(string)),
                    new DataColumn("SirkulerRef", typeof(string)),
                    new DataColumn("MusteriNo", typeof(string)),
                    new DataColumn("FirmaUnvani", typeof(string)),
                    new DataColumn("DuzenlemeTarihi", typeof(DateTime)),
                    new DataColumn("GecerlilikTarihi", typeof(DateTime)),
                    new DataColumn("SirkulerTipi", typeof(string)),
                    new DataColumn("SirkulerNoterNo", typeof(string)),
                    new DataColumn("OzelDurumlar", typeof(string)),
                    new DataColumn("SirkulerDurumu", typeof(string)),
                    new DataColumn("SirkulerBelge", typeof(string))
                });

                // Add sample data
                dt.Rows.Add(
                    "Detail",                                  // Detail
                    "IS-285",                                 // SirkulerRef
                    "316",                                    // MusteriNo
                    "NIPPIT OTABAATE A.A.",                  // FirmaUnvani
                    new DateTime(2025, 08, 15),             // DuzenlemeTarihi
                    new DateTime(2025, 08, 15),             // GecerlilikTarihi
                    "Ana Sirküler",                          // SirkulerTipi
                    "03104",                                 // SirkulerNoterNo
                    "Birinci Grup imza yetkililerinden bir kişinin veya, İkinci Grup imza yetkililerinden bir kişinin imzası yeterlidir", // OzelDurumlar
                    "Aktif",                                 // SirkulerDurumu
                    "Görüntüle"                             // SirkulerBelge
                );
                
                gvCirculars.DataSource = dt;
                gvCirculars.DataBind();
            }
            catch (Exception ex)
            {
                // Log error
                ShowError(string.Format("Sirküler listesi yüklenirken hata oluştu: {0}", ex.Message));
            }
        }

        private void InitializeForm()
        {
            txtDuzenlemeTarihi.Text = DateTime.Now.ToString("yyyy-MM-dd");
            txtGecerlilikTarihi.Text = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
            ddlSirkulerDurumu.SelectedValue = "Aktif";
        }

        protected void BtnNewCircular_Click(object sender, EventArgs e)
        {
            try
            {
                ClearForm();
                InitializeForm();
                Session["CurrentSirkulerRef"] = null; // Clear current sirkuler ref
                hdnCurrentView.Value = "detail";
                ScriptManager.RegisterStartupScript(this, GetType(), "switchView", "switchView('detail');", true);
                ShowSuccess("Yeni sirküler formu açıldı");
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Yeni sirküler formu açılırken hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form
                if (string.IsNullOrEmpty(txtMusteriNo.Text) || string.IsNullOrEmpty(txtFirmaUnvani.Text))
                {
                    ShowError("Müşteri No ve Firma Ünvanı alanları zorunludur.");
                    return;
                }

                // TODO: Save data to database
                
                // Save signature selections if any
                SaveSignatureSelections();

                // Show success message and return to list
                ShowSuccess("Sirküler başarıyla kaydedildi.");
                LoadCirculars();
                hdnCurrentView.Value = "list";
                ScriptManager.RegisterStartupScript(this, GetType(), "switchView", "switchView('list');", true);
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Kayıt sırasında hata oluştu: {0}", ex.Message));
            }
        }

        protected void BtnCancel_Click(object sender, EventArgs e)
        {
            ClearForm();
            hdnCurrentView.Value = "list";
            ScriptManager.RegisterStartupScript(this, GetType(), "switchView", "switchView('list');", true);
        }

        protected void GvCirculars_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = gvCirculars.Rows[index];

                if (e.CommandName == "Detail")
                {
                    // Load data into form
                    // Load data into form
                    txtMusteriNo.Text = row.Cells[2].Text; // MusteriNo
                    txtFirmaUnvani.Text = row.Cells[3].Text; // FirmaUnvani
                    txtDuzenlemeTarihi.Text = DateTime.Parse(row.Cells[4].Text).ToString("yyyy-MM-dd"); // DuzenlemeTarihi
                    txtGecerlilikTarihi.Text = DateTime.Parse(row.Cells[5].Text).ToString("yyyy-MM-dd"); // GecerlilikTarihi
                    ddlSirkulerTipi.SelectedValue = row.Cells[6].Text; // SirkulerTipi
                    txtSirkulerNoterNo.Text = row.Cells[7].Text; // SirkulerNoterNo
                    txtOzelDurumlar.Text = row.Cells[8].Text; // OzelDurumlar
                    ddlSirkulerDurumu.SelectedValue = row.Cells[9].Text; // SirkulerDurumu

                    // Store current sirkuler ref for signature operations
                    Session["CurrentSirkulerRef"] = row.Cells[1].Text; // SirkulerRef

                    // Switch to detail view
                    hdnCurrentView.Value = "detail";
                    ScriptManager.RegisterStartupScript(this, GetType(), "switchView", "switchView('detail');", true);
                }
                else if (e.CommandName == "DeleteCircular")
                {
                    // TODO: Implement delete functionality
                    ShowSuccess("Kayıt silindi.");
                    LoadCirculars();
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("İşlem sırasında hata oluştu: {0}", ex.Message));
            }
        }

        protected void GvCirculars_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Add any row-specific formatting here
            }
        }





        private void ClearForm()
        {
            txtMusteriNo.Text = string.Empty;
            txtFirmaUnvani.Text = string.Empty;
            txtDuzenlemeTarihi.Text = string.Empty;
            txtGecerlilikTarihi.Text = string.Empty;
            txtIcYonergePGTarihi.Text = string.Empty;
            txtTSGNo.Text = string.Empty;
            txtOzelDurumlar.Text = string.Empty;
            txtSirkulerNoterNo.Text = string.Empty;
            txtAciklama.Text = string.Empty;
            ddlSirkulerTipi.SelectedIndex = 0;
            ddlSirkulerDurumu.SelectedIndex = 0;
            chkYKKGerekli.Checked = false;
            hdnSelectedSignatures.Value = string.Empty;
        }

        private void ShowError(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showError", 
                string.Format("showNotification('{0}', 'error');", message), true);
        }

        private void ShowSuccess(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showSuccess", 
                string.Format("showNotification('{0}', 'success');", message), true);
        }

        protected void BtnExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // Get filtered data
                DataTable dt = GetFilteredData();

                // Create Excel file
                using (var workbook = new System.Web.UI.WebControls.GridView())
                {
                    workbook.DataSource = dt;
                    workbook.DataBind();

                    // Configure response
                    Response.Clear();
                    Response.Buffer = true;
                    Response.AddHeader("content-disposition", "attachment;filename=Sirkuler_Listesi.xls");
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.ContentEncoding = System.Text.Encoding.UTF8;
                    Response.BinaryWrite(System.Text.Encoding.UTF8.GetPreamble());

                    // Create StringWriter
                    using (StringWriter sw = new StringWriter())
                    {
                        using (HtmlTextWriter htw = new HtmlTextWriter(sw))
                        {
                            // Add Excel styling
                            Response.Write("<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                            Response.Write("<style>td { mso-number-format:\\@; } </style>");

                            // Render grid to Excel
                            workbook.RenderControl(htw);
                            Response.Write(sw.ToString());
                            Response.Flush();
                            Response.End();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Excel dosyası oluşturulurken hata oluştu: {0}", ex.Message));
            }
        }

        private DataTable GetFilteredData()
        {
            DataTable dt = new DataTable();

            try
            {
                // Add columns
                dt.Columns.AddRange(new DataColumn[]
                {
                    new DataColumn("SirkulerRef", typeof(string)),
                    new DataColumn("MusteriNo", typeof(string)),
                    new DataColumn("FirmaUnvani", typeof(string)),
                    new DataColumn("DuzenlemeTarihi", typeof(DateTime)),
                    new DataColumn("GecerlilikTarihi", typeof(DateTime)),
                    new DataColumn("SirkulerTipi", typeof(string)),
                    new DataColumn("SirkulerNoterNo", typeof(string)),
                    new DataColumn("OzelDurumlar", typeof(string)),
                    new DataColumn("SirkulerDurumu", typeof(string)),
                    new DataColumn("YetkiTurleri", typeof(string)),
                    new DataColumn("YetkiSekli", typeof(string))
                });

                // TODO: Get actual data from database with filters
                // For now, adding sample data
                dt.Rows.Add(
                    "IS-285",
                    "316",
                    "NIPPIT OTABAATE A.A.",
                    DateTime.Parse("05/09/2019"),
                    DateTime.Parse("14/07/2025"),
                    "Ana Sirküler",
                    "03104",
                    "Birinci Grup imza yetkililerinden bir kişinin veya, İkinci Grup imza yetkililerinden bir kişinin imzası yeterlidir",
                    "Aktif",
                    "Kredi İşlemleri",
                    "Müştereken"
                );

                // Apply filters
                if (!string.IsNullOrEmpty(txtFilterMusteriNo.Text))
                {
                    string musteriNo = txtFilterMusteriNo.Text.Trim();
                    dt.DefaultView.RowFilter = string.Format("MusteriNo LIKE '%{0}%'", musteriNo);
                }

                if (!string.IsNullOrEmpty(txtFilterSirkulerRef.Text))
                {
                    string sirkulerRef = txtFilterSirkulerRef.Text.Trim();
                    string currentFilter = dt.DefaultView.RowFilter;
                    dt.DefaultView.RowFilter = string.IsNullOrEmpty(currentFilter) 
                        ? string.Format("SirkulerRef LIKE '%{0}%'", sirkulerRef)
                        : string.Format("{0} AND SirkulerRef LIKE '%{1}%'", currentFilter, sirkulerRef);
                }

                // Add more filters as needed...

                return dt.DefaultView.ToTable();
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Veri filtrelenirken hata oluştu: {0}", ex.Message));
                return dt;
            }
        }
    }
}
