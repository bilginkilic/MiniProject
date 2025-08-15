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

        protected void BtnUploadSignature_Click(object sender, EventArgs e)
        {
            try
            {
                if (fuSignature.HasFile)
                {
                    string fileName = Path.GetFileName(fuSignature.FileName);
                    if (Path.GetExtension(fileName).ToLower() != ".pdf")
                    {
                        ShowError("Lütfen sadece PDF formatında dosya yükleyiniz.", true);
                        return;
                    }

                    // Önceki dosyaları temizle
                    CleanupOldFiles();

                    string pdfPath = Path.Combine(_cdn, fileName);
                    fuSignature.SaveAs(pdfPath);
                    Session["LastUploadedPdf"] = pdfPath;

                    // PDF'i hemen göster
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü başlıyor. PDF yolu: {0}", pdfPath));
                        System.Diagnostics.Debug.WriteLine(String.Format("CDN klasörü: {0}", _cdn));
                        
                        // PDF'yi PNG'ye çevir
                        int pageCount = PdfToImageAndCrop.ConvertPdfToImages(pdfPath, _cdn);
                        System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü tamamlandı. Sayfa sayısı: {0}", pageCount));

                        // Her sayfanın oluşturulduğunu kontrol et ve base64'e çevir
                        var imageDataList = new System.Collections.Generic.List<string>();
                        bool allPagesExist = true;

                        for (int i = 1; i <= pageCount; i++)
                        {
                            string imagePath = Path.Combine(_cdn, String.Format("page_{0}.png", i));
                            if (!File.Exists(imagePath))
                            {
                                System.Diagnostics.Debug.WriteLine(String.Format("Sayfa bulunamadı: {0}", imagePath));
                                allPagesExist = false;
                                break;
                            }
                            else
                            {
                                try
                                {
                                    using (var image = System.Drawing.Image.FromFile(imagePath))
                                    using (var ms = new MemoryStream())
                                    {
                                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        byte[] imageBytes = ms.ToArray();
                                        string base64String = Convert.ToBase64String(imageBytes);
                                        imageDataList.Add(String.Format("data:image/png;base64,{0}", base64String));
                                        
                                        System.Diagnostics.Debug.WriteLine(String.Format("Sayfa {0} base64'e çevrildi, Boyut: {1} bytes", i, imageBytes.Length));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine(String.Format("Base64 dönüşüm hatası: {0}", ex.Message));
                                    allPagesExist = false;
                                    break;
                                }
                            }
                        }

                        if (!allPagesExist)
                        {
                            ShowError("PDF sayfaları dönüştürülürken bir hata oluştu. Lütfen tekrar deneyiniz.");
                            return;
                        }

                        // Sayfa sayısını hidden field'a kaydet
                        hdnPageCount.Value = pageCount.ToString();
                        
                        // JavaScript'e sayfa sayısını ve resim verilerini gönder
                        var imageDataJson = String.Format("[{0}]", String.Join(",", imageDataList.Select(x => String.Format("'{0}'", x))));
                        
                        ScriptManager.RegisterStartupScript(this, GetType(),
                            "initTabs",
                            String.Format("var imageDataList = {0}; console.log('Image data loaded, count:', {1}); initializeTabs({1});", 
                                imageDataJson, pageCount),
                            true);

                        ShowMessage("İmza sirküleri yüklendi ve görüntüleniyor. İmza alanını seçmek için tıklayıp sürükleyin.", "success");
                        btnSaveSignature.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("PDF dönüşümü hatası: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace));
                        ShowError(String.Format("İmza sirkülerini görüntülerken bir hata oluştu: {0}", ex.Message));
                    }
                }
                else
                {
                    ShowError("Lütfen bir PDF dosyası seçiniz.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Dosya yükleme hatası: {0}", ex.Message));
                ShowError(String.Format("Dosya yüklenirken bir hata oluştu: {0}", ex.Message));
            }
        }

        private void CleanupOldFiles()
        {
            try
            {
                // Tüm PNG dosyalarını temizle
                foreach (string file in Directory.GetFiles(_cdn, "*.png"))
                {
                    try { File.Delete(file); } catch { }
                }

                // Tüm PDF dosyalarını temizle
                foreach (string file in Directory.GetFiles(_cdn, "*.pdf"))
                {
                    try { File.Delete(file); } catch { }
                }

                System.Diagnostics.Debug.WriteLine("Eski dosyalar temizlendi");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("Dosya temizleme hatası: {0}", ex.Message));
            }
        }

        private void SaveSignatureSelections()
        {
            try
            {
                // Form verilerini topla
                var authData = new SignatureAuthData
                {
                    KaynakPdfAdi = hdnCurrentPdfList.Value,
                    Yetkililer = new List<YetkiliData>()
                };

                // Tablodaki tüm yetkilileri al
                var yetkiliKayitJson = Request.Form["hdnYetkiliKayitlar"];
                if (!string.IsNullOrEmpty(yetkiliKayitJson))
                {
                    var serializer = new JavaScriptSerializer();
                    var yetkiliKayitlar = serializer.Deserialize<List<YetkiliKayit>>(yetkiliKayitJson);

                    foreach (var kayit in yetkiliKayitlar)
                    {
                        var yetkiliData = new YetkiliData
                        {
                            YetkiliKontakt = kayit.YetkiliKontakt,
                            YetkiliAdi = kayit.YetkiliAdi,
                            YetkiSekli = kayit.YetkiSekli,
                            YetkiTarihi = kayit.YetkiTarihi,
                            YetkiBitisTarihi = kayit.AksiKararaKadar ? "Aksi Karara Kadar" : kayit.YetkiTarihi,
                            YetkiGrubu = kayit.YetkiSekli,
                            SinirliYetkiDetaylari = kayit.SinirliYetkiDetaylari,
                            YetkiTurleri = kayit.YetkiTurleri,
                            YetkiTutari = decimal.Parse(kayit.YetkiTutari),
                            YetkiDovizCinsi = kayit.YetkiDovizCinsi,
                            YetkiDurumu = kayit.YetkiDurumu,
                            Imzalar = new List<SignatureImage>()
                        };
                        authData.Yetkililer.Add(yetkiliData);
                    }
                }

                string signaturesJson = Request.Form["hdnSignatures"];
                System.Diagnostics.Debug.WriteLine(String.Format("İmza verileri alındı: {0}", signaturesJson));

                if (string.IsNullOrEmpty(signaturesJson))
                {
                    ShowWarning("Lütfen en az bir imza seçiniz.", true);
                    return;
                }

                var serializer = new JavaScriptSerializer();
                var signatures = serializer.Deserialize<List<SignatureData>>(signaturesJson);

                if (signatures == null || signatures.Count == 0)
                {
                    ShowError("Geçersiz imza verisi.", true);
                    return;
                }

                var savedSignatures = new List<SavedSignature>();

                foreach (var signature in signatures)
                {
                    string imagePath = Path.Combine(_cdn, String.Format("page_{0}.png", signature.Page));
                    System.Diagnostics.Debug.WriteLine(String.Format("Kaynak resim yolu: {0}", imagePath));

                    if (!File.Exists(imagePath))
                    {
                        ShowError(String.Format("Sayfa {0} için görüntü bulunamadı.", signature.Page));
                        continue;
                    }

                    using (var sourceImage = new System.Drawing.Bitmap(imagePath))
                    {
                        if (signature.X < 0 || signature.Y < 0 || 
                            signature.X + signature.Width > sourceImage.Width || 
                            signature.Y + signature.Height > sourceImage.Height)
                        {
                            ShowError(String.Format("Sayfa {0} için seçilen alan resim sınırları dışında.", signature.Page));
                            continue;
                        }

                        string outputFileName = String.Format("signature_{0}_{1}.png", DateTime.Now.Ticks, signatures.IndexOf(signature));
                        string outputPath = Path.Combine(_cdn, outputFileName);

                        try
                        {
                            using (var bitmap = new System.Drawing.Bitmap(signature.Width, signature.Height))
                            {
                                bitmap.SetResolution(sourceImage.HorizontalResolution, sourceImage.VerticalResolution);

                                using (var graphics = System.Drawing.Graphics.FromImage(bitmap))
                                {
                                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                                    graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                    var sourceRect = new System.Drawing.Rectangle(signature.X, signature.Y, signature.Width, signature.Height);
                                    var destRect = new System.Drawing.Rectangle(0, 0, signature.Width, signature.Height);

                                    graphics.DrawImage(sourceImage, destRect, sourceRect, System.Drawing.GraphicsUnit.Pixel);
                                }

                                string tempPath = Path.Combine(_cdn, String.Format("temp_{0}", outputFileName));
                                bitmap.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                                if (File.Exists(outputPath))
                                {
                                    File.Delete(outputPath);
                                }
                                File.Move(tempPath, outputPath);

                                savedSignatures.Add(new SavedSignature
                                {
                                    Path = _cdnVirtualPath + "/" + outputFileName,
                                    Page = signature.Page,
                                    X = signature.X,
                                    Y = signature.Y,
                                    Width = signature.Width,
                                    Height = signature.Height
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine(String.Format("İmza kaydetme hatası: {0}", ex.Message));
                            ShowError(String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message));
                            continue;
                        }
                    }
                }

                // İmzaları ilgili yetkililere ekle
                var yetkiliImzaEslesmesi = Request.Form["hdnYetkiliImzaEslesmesi"];
                if (!string.IsNullOrEmpty(yetkiliImzaEslesmesi))
                {
                    var serializer = new JavaScriptSerializer();
                    var eslesmeler = serializer.Deserialize<Dictionary<int, int>>(yetkiliImzaEslesmesi);

                    foreach (var eslesme in eslesmeler)
                    {
                        if (eslesme.Key < authData.Yetkililer.Count && eslesme.Value < savedSignatures.Count)
                        {
                            var savedSig = savedSignatures[eslesme.Value];
                            authData.Yetkililer[eslesme.Key].Imzalar.Add(new SignatureImage
                            {
                                ImageData = savedSig.Path,
                                SiraNo = authData.Yetkililer[eslesme.Key].Imzalar.Count + 1,
                                SourcePdfPath = authData.KaynakPdfAdi
                            });
                        }
                    }
                }
                else
                {
                    // Eğer eşleşme bilgisi yoksa, tüm imzaları ilk yetkiliye ekle
                    foreach (var savedSig in savedSignatures)
                    {
                        if (authData.Yetkililer.Count > 0)
                        {
                            authData.Yetkililer[0].Imzalar.Add(new SignatureImage
                            {
                                ImageData = savedSig.Path,
                                SiraNo = authData.Yetkililer[0].Imzalar.Count + 1,
                                SourcePdfPath = authData.KaynakPdfAdi
                            });
                        }
                    }
                }

                ShowSuccess("İmzalar başarıyla kaydedildi.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("İmza kaydetme hatası: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace));
                ShowError(String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message));
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
