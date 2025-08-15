// Created: 2024.01.17 14:30 - v1
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//metx
namespace AspxExamples
{
    public partial class Circular : System.Web.UI.Page
    {
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
                    DateTime.Parse("05/09/2019"),             // DuzenlemeTarihi
                    DateTime.Parse("14/07/2025"),             // GecerlilikTarihi
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
            ClearForm();
            InitializeForm();
            hdnCurrentView.Value = "detail";
            ScriptManager.RegisterStartupScript(this, GetType(), "switchView", "switchView('detail');", true);
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

                if (e.CommandName == "EditCircular")
                {
                    // Load data into form
                    txtMusteriNo.Text = row.Cells[0].Text;
                    txtFirmaUnvani.Text = row.Cells[1].Text;
                    // ... load other fields

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

        protected void BtnUploadSignature_Click(object sender, EventArgs e)
        {
            try
            {
                if (fuSignature.HasFile)
                {
                    string fileName = Path.GetFileName(fuSignature.FileName);
                    string extension = Path.GetExtension(fileName).ToLower();

                    // Validate file type
                    if (extension != ".pdf")
                    {
                        ShowError("Lütfen sadece PDF dosyası yükleyin.");
                        return;
                    }

                    // Save file and convert to images
                    string filePath = Server.MapPath("~/Uploads/") + fileName;
                    fuSignature.SaveAs(filePath);

                    // Convert PDF to images using PdfToImageAndCrop
                    ConvertPdfToImages(filePath);

                    ShowSuccess("Dosya başarıyla yüklendi.");
                }
                else
                {
                    ShowError("Lütfen bir dosya seçin.");
                }
            }
            catch (Exception ex)
            {
                ShowError(string.Format("Dosya yükleme sırasında hata oluştu: {0}", ex.Message));
            }
        }

        private void ConvertPdfToImages(string pdfPath)
        {
            // TODO: Implement PDF to image conversion using PdfToImageAndCrop class
        }

        private void SaveSignatureSelections()
        {
            string selections = hdnSelectedSignatures.Value;
            if (!string.IsNullOrEmpty(selections))
            {
                // TODO: Process and save signature selections
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
