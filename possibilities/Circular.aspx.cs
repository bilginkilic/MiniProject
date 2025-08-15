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
                    new DataColumn("MusteriNo", typeof(string)),
                    new DataColumn("FirmaUnvani", typeof(string)),
                    new DataColumn("DuzenlemeTarihi", typeof(DateTime)),
                    new DataColumn("GecerlilikTarihi", typeof(DateTime)),
                    new DataColumn("SirkulerTipi", typeof(string)),
                    new DataColumn("SirkulerNoter", typeof(string)),
                    new DataColumn("Durum", typeof(string))
                });

                // Add sample data
                dt.Rows.Add("1001", "Test Firma A.Ş.", DateTime.Now, DateTime.Now.AddYears(1), "Ana Sirküler", "123456", "Aktif");
                
                gvCirculars.DataSource = dt;
                gvCirculars.DataBind();
            }
            catch (Exception ex)
            {
                // Log error
                ShowError("Sirküler listesi yüklenirken hata oluştu: " + ex.Message);
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
                ShowError("Kayıt sırasında hata oluştu: " + ex.Message);
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
                ShowError("İşlem sırasında hata oluştu: " + ex.Message);
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
                ShowError("Dosya yükleme sırasında hata oluştu: " + ex.Message);
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
                $"showNotification('{message}', 'error');", true);
        }

        private void ShowSuccess(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showSuccess", 
                $"showNotification('{message}', 'success');", true);
        }
    }
}
