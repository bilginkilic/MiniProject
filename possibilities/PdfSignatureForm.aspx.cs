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
using System.Web.Services;
using System.Web.Script.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/* v2 - axoi.aspx7.cs - Debug logları eklendi ve string.Format kullanımına geçildi */

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
        public int ID { get; set; }
        public int AuthDetailID { get; set; }
        public string Base64Image { get; set; }
        public int SlotIndex { get; set; }
    }

    public class YetkiliKayit
    {
        public int ID { get; set; }
        public int CircularID { get; set; }
        public string YetkiliKontakt { get; set; }
        public string YetkiliAdi { get; set; }
        public string YetkiSekli { get; set; }
        public string YetkiTarihi { get; set; }
        public string YetkiBitisTarihi { get; set; }
        public bool AksiKararaKadar { get; set; }
        public string YetkiGrubu { get; set; }
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

    public partial class PdfSignatureForm : System.Web.UI.Page
    {
        protected string GetInitialYetkiliDataJson()
        {
            try
            {
                var initialData = SessionHelper.GetInitialYetkiliData();
                if (initialData != null)
                {
                    var serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = int.MaxValue;
                    return serializer.Serialize(initialData);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetInitialYetkiliDataJson error: {ex.Message}");
            }
            return "null";
        }
        private static List<YetkiliKayit> yetkiliKayitlar;
        private static List<SignatureData> signatures;
        private string _cdn = @"\\trrgap3027\files\circular\cdn";
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            MaxDepth = 128,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };
        private string _cdnVirtualPath = "/cdn"; // Web'den erişim için virtual path

        // Form kontrolleri
        protected TextBox txtYetkiliKontakt;
        protected TextBox txtYetkiliAdi;
        protected DropDownList selYetkiSekli;
        protected TextBox yetkiBitisTarihi;
        protected CheckBox chkAksiKarar;
        protected DropDownList selYetkiGrubu;
        protected TextBox txtSinirliYetkiDetaylari;
        protected DropDownList selYetkiTurleri;
        protected TextBox txtYetkiTutari;
        protected DropDownList selYetkiDovizCinsi;
        protected DropDownList selYetkiDurumu;
        protected Button btnSaveSignature;
        protected HiddenField hdnCurrentPdfList;
        protected HiddenField hdnPageCount;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünü kontrol et
                if (!Directory.Exists(_cdn))
                {
                    try
                    {
                        Directory.CreateDirectory(_cdn);
                        System.Diagnostics.Debug.WriteLine(String.Format("CDN klasörü oluşturuldu: {0}", _cdn));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(String.Format("CDN klasörü oluşturma hatası: {0}", ex.Message));
                        ShowError("Sistem hazırlığı sırasında bir hata oluştu. Lütfen yöneticinize başvurun.");
                        return;
                    }
                }

                // Session'dan veriyi al
                var yetkiliDataList = SessionHelper.GetInitialYetkiliData();
                int yetkiliCount = 0;
                if (yetkiliDataList != null)
                {
                    yetkiliCount = yetkiliDataList.Count;
                }
                System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm Page_Load: Session'dan alınan yetkili sayısı: {0}", yetkiliCount));
                
                // Session'dan gelen veriyi ve PDF adresini hazırla
                if (yetkiliDataList != null && yetkiliDataList.Count > 0)
                {
                    // PDF adresini al
                    string pdfPath = string.Empty;
                    if (Session["CurrentPdfPath"] != null)
                    {
                        pdfPath = Session["CurrentPdfPath"].ToString();
                    }
                    else if (!string.IsNullOrEmpty(hdnCurrentPdfList.Value))
                    {
                        pdfPath = hdnCurrentPdfList.Value;
                    }

                    // Yetkili kayıtları hazırla
                    yetkiliKayitlar = yetkiliDataList;
                    
                    // Signature verilerini hazırla
                    signatures = new List<SignatureData>();
                    foreach (var yetkili in yetkiliKayitlar)
                    {
                        if (yetkili.Imzalar != null)
                        {
                            foreach (var imza in yetkili.Imzalar)
                            {
                                var signatureData = new SignatureData
                                {
                                    SourcePdfPath = pdfPath,
                                    Image = imza.Base64Image
                                };
                                signatures.Add(signatureData);
                            }
                        }
                    }

                    // Hidden field'a kaydet
                    var serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = int.MaxValue;
                    hdnYetkiliKayitlar.Value = serializer.Serialize(yetkiliKayitlar);
                    System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm Page_Load: Session'dan grid için hazırlanan veri: {0}", hdnYetkiliKayitlar.Value));
                }
                if (yetkiliDataList != null && yetkiliDataList.Any())
                {
                    try
                    {

                        if (yetkiliDataList != null && yetkiliDataList.Any())
                        {
                            // Grid'e eklemek için yetkili kayıtlarını hazırla
                            var gridData = new List<object>();
                            foreach (var yetkiliData in yetkiliDataList)
                            {
                                var rowData = new
                                {
                                    YetkiliKontakt = yetkiliData.YetkiliKontakt,
                                    YetkiliAdi = yetkiliData.YetkiliAdi,
                                    YetkiSekli = yetkiliData.YetkiSekli,
                                    YetkiTarihi = yetkiliData.YetkiTarihi,
                                    YetkiBitisTarihi = yetkiliData.AksiKararaKadar ? "Aksi Karara Kadar" : yetkiliData.YetkiTarihi,
                                    YetkiGrubu = yetkiliData.YetkiSekli,
                                    SinirliYetkiDetaylari = yetkiliData.SinirliYetkiDetaylari,
                                    YetkiTurleri = yetkiliData.YetkiTurleri,
                                    YetkiTutari = yetkiliData.YetkiTutari,
                                    YetkiDovizCinsi = yetkiliData.YetkiDovizCinsi,
                                    YetkiDurumu = yetkiliData.YetkiDurumu,
                                    Imzalar = yetkiliData.Imzalar?.Select(i => i.Base64Image).ToList() ?? new List<string>()
                                };
                                gridData.Add(rowData);
                            }

                            // Grid verilerini hidden field'a kaydet
                            // JSON.NET ile serialize et
                            var serializedData = JsonConvert.SerializeObject(gridData, Formatting.None,
                                new JsonSerializerSettings { 
                                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                    MaxDepth = 32
                                });
                                
                            // ViewState ve hidden field'a kaydet
                            ViewState["YetkiliKayitlar"] = serializedData;
                            System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Serialize edilecek veri sayısı: {0}", gridData.Count));
                            foreach (var item in gridData)
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Grid item: {0}", JsonConvert.SerializeObject(item)));
                            }
                            
                            var serializedData = serializer.Serialize(gridData);
                            
                            // Serialize edilen veriyi parse ederek kontrol et
                            try
                            {
                                var deserializedData = serializer.Deserialize<List<object>>(serializedData);
                                System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Deserialize kontrol başarılı, eleman sayısı: {0}", deserializedData.Count));
                            }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Deserialize kontrol hatası: {0}", ex.Message));
                            }
                            System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Hidden field'a kaydedilen veri: {0}", serializedData));
                            System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Hidden field ID: {0}", hdnYetkiliKayitlar.ClientID));
                            hdnYetkiliKayitlar.Value = serializedData;
                            
                            // Değer atandıktan sonra kontrol et
                            System.Diagnostics.Debug.WriteLine(string.Format("PdfSignatureForm: Hidden field değeri atandıktan sonra: {0}", hdnYetkiliKayitlar.Value));

                            // İlk yetkiliyi form alanlarına doldur
                            var ilkYetkili = yetkiliDataList[0];
                            txtYetkiliKontakt.Text = ilkYetkili.YetkiliKontakt;
                            txtYetkiliAdi.Text = ilkYetkili.YetkiliAdi;
                            selYetkiSekli.SelectedValue = ilkYetkili.YetkiSekli;
                            txtSinirliYetkiDetaylari.Text = ilkYetkili.SinirliYetkiDetaylari;
                            selYetkiTurleri.SelectedValue = ilkYetkili.YetkiTurleri;
                            txtYetkiTutari.Text = ilkYetkili.YetkiTutari;
                            selYetkiDovizCinsi.SelectedValue = ilkYetkili.YetkiDovizCinsi;
                            selYetkiDurumu.SelectedValue = ilkYetkili.YetkiDurumu;

                            // Tarih ayarları
                            if (ilkYetkili.AksiKararaKadar)
                            {
                                chkAksiKarar.Checked = true;
                                yetkiBitisTarihi.Enabled = false;
                            }
                            else
                            {
                                yetkiBitisTarihi.Text = ilkYetkili.YetkiTarihi;
                            }

                            // Tüm imzaları topla
                            var allSignatures = new List<string>();
                            foreach (var yetkili in yetkiliDataList)
                            {
                                if (yetkili.Imzalar != null)
                                {
                                    allSignatures.AddRange(yetkili.Imzalar.Select(i => i.Base64Image));
                                }
                            }
                            hdnSignatures.Value = serializer.Serialize(allSignatures);

                            // JavaScript'i çağırarak grid ve imzaları göster
                            ScriptManager.RegisterStartupScript(this, GetType(),
                                "initializeData",
                                @"if(typeof updateSignatureSlots === 'function') { 
                                    updateSignatureSlots(); 
                                }
                                if(typeof initializeGrid === 'function') {
                                    initializeGrid();
                                }",
                                true);

                            ShowMessage("Veriler başarıyla yüklendi.", "success");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Veri yükleme hatası: " + ex.Message);
                        ShowError("Veriler yüklenirken bir hata oluştu: " + ex.Message);
                    }
                }
                else
                {
                    // Başlangıç mesajını göster
                    ShowMessage("PDF formatında imza sirkülerinizi yükleyerek başlayabilirsiniz.", "info");
                }
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
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



        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static YetkiliKayitResponse SaveYetkiliKayit(YetkiliKayit kayit)
        {
            try
            {
                // TODO: Burada web servis çağrısı yapılacak
                // Şimdilik dummy response dönüyoruz
                return new YetkiliKayitResponse
                {
                    Success = true,
                    Message = "Kayıt başarıyla " + 
                        (kayit.IslemTipi == "Sil" ? "silindi" : 
                         kayit.IslemTipi == "Guncelle" ? "güncellendi" : "eklendi"),
                    Data = kayit
                };
            }
            catch (Exception ex)
            {
                return new YetkiliKayitResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        public class CustomerSearchResult
        {
            public string No { get; set; }
            public string Name { get; set; }
            public string Status { get; set; }
        }

        public class CustomerSearchResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public List<CustomerSearchResult> Data { get; set; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static CustomerSearchResponse SearchCustomers(string searchTerm)
        {
            try
            {
                // TODO: Burada gerçek veritabanı sorgusu yapılacak
                // Şimdilik örnek veri dönüyoruz
                var customers = new List<CustomerSearchResult>();

                // Eğer arama terimi boş değilse filtreleme yap
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    customers.AddRange(new[]
                    {
                        new CustomerSearchResult { No = "1001", Name = "Test Müşteri 1", Status = "Aktif" },
                        new CustomerSearchResult { No = "1002", Name = "Test Müşteri 2", Status = "Aktif" },
                        new CustomerSearchResult { No = "1003", Name = "Test Müşteri 3", Status = "Pasif" }
                    }.Where(c => 
                        c.No.Contains(searchTerm) || 
                        c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                    ));
                }

                return new CustomerSearchResponse
                {
                    Success = true,
                    Message = customers.Any() ? "Müşteriler bulundu" : "Müşteri bulunamadı",
                    Data = customers
                };
            }
            catch (Exception ex)
            {
                return new CustomerSearchResponse
                {
                    Success = false,
                    Message = "Arama sırasında bir hata oluştu: " + ex.Message,
                    Data = new List<CustomerSearchResult>()
                };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static YetkiliKayitResponse SearchYetkili(string yetkiliNo)
        {
            try
            {
                // TODO: Burada yetkili arama web servis çağrısı yapılacak
                // Şimdilik dummy response dönüyoruz
                var dummyYetkili = new YetkiliKayit
                {
                    YetkiliKontakt = yetkiliNo,
                    YetkiliAdi = "Test Yetkili",
                    YetkiSekli = "Müştereken",
                    YetkiTarihi = "14.07.2024",
                    AksiKararaKadar = true,
                    SinirliYetkiDetaylari = "Test detayları",
                    YetkiTurleri = "Kredi İşlemleri",
                    YetkiTutari = "100000",
                    YetkiDovizCinsi = "USD",
                    YetkiDurumu = "Aktif",
                    Imzalar = new List<YetkiliImza>()
                };

                return new YetkiliKayitResponse
                {
                    Success = true,
                    Message = "Yetkili bulundu",
                    Data = dummyYetkili
                };
            }
            catch (Exception ex)
            {
                return new YetkiliKayitResponse
                {
                    Success = false,
                    Error = ex.Message
                };
            }
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public static object SaveSignatureWithAjax(string yetkiliKayitlarJson, string signatureDataJson)
        {
            try
            {
                // Debug için gelen verileri logla
                System.Diagnostics.Debug.WriteLine("SaveSignatureWithAjax başladı");
                System.Diagnostics.Debug.WriteLine("Gelen yetkiliKayitlarJson uzunluğu: " + (yetkiliKayitlarJson?.Length ?? 0));
                System.Diagnostics.Debug.WriteLine("Gelen yetkiliKayitlarJson içeriği: " + yetkiliKayitlarJson);
                System.Diagnostics.Debug.WriteLine("Gelen signatureDataJson içeriği: " + signatureDataJson);
                System.Diagnostics.Debug.WriteLine("Gelen signatureDataJson uzunluğu: " + (signatureDataJson?.Length ?? 0));
                
                // Gelen verileri parse etmeyi dene
                try {
                    var serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue; // Maksimum JSON uzunluğunu artır
                    try {
                        // JObject ile parse et - büyük JSON'lar için
                        var yetkiliKayitlarArray = JArray.Parse(yetkiliKayitlarJson);
                        var signaturesArray = JArray.Parse(signatureDataJson);

                        yetkiliKayitlar = new List<YetkiliKayit>();
                        signatures = new List<SignatureData>();

                        foreach (JObject kayitObj in yetkiliKayitlarArray)
                        {
                            var kayit = new YetkiliKayit();
                            
                            var kontaktToken = kayitObj["YetkiliKontakt"];
                            kayit.YetkiliKontakt = kontaktToken != null ? kontaktToken.ToString() : null;
                            
                            var adiToken = kayitObj["YetkiliAdi"];
                            kayit.YetkiliAdi = adiToken != null ? adiToken.ToString() : null;
                            
                            var sekliToken = kayitObj["YetkiSekli"];
                            kayit.YetkiSekli = sekliToken != null ? sekliToken.ToString() : null;
                            
                            var tarihToken = kayitObj["YetkiTarihi"];
                            kayit.YetkiTarihi = tarihToken != null ? tarihToken.ToString() : null;
                            
                            var aksiKararToken = kayitObj["AksiKararaKadar"];
                            kayit.AksiKararaKadar = aksiKararToken != null ? aksiKararToken.Value<bool>() : false;
                            
                            var detayToken = kayitObj["SinirliYetkiDetaylari"];
                            kayit.SinirliYetkiDetaylari = detayToken != null ? detayToken.ToString() : null;
                            
                            var turToken = kayitObj["YetkiTurleri"];
                            kayit.YetkiTurleri = turToken != null ? turToken.ToString() : null;
                            
                            var tutarToken = kayitObj["YetkiTutari"];
                            kayit.YetkiTutari = tutarToken != null ? tutarToken.ToString() : null;
                            
                            var dovizToken = kayitObj["YetkiDovizCinsi"];
                            kayit.YetkiDovizCinsi = dovizToken != null ? dovizToken.ToString() : null;
                            
                            var durumToken = kayitObj["YetkiDurumu"];
                            kayit.YetkiDurumu = durumToken != null ? durumToken.ToString() : null;
                            
                            var islemToken = kayitObj["IslemTipi"];
                            kayit.IslemTipi = islemToken != null ? islemToken.ToString() : null;
                            
                            kayit.Imzalar = new List<YetkiliImza>();

                            // İmzaları parse et
                            var imzalarToken = kayitObj["Imzalar"];
                            if (imzalarToken != null)
                            {
                                // Token tipini kontrol et
                                if (imzalarToken.Type == JTokenType.Array)
                                {
                                    var imzalarArray = (JArray)imzalarToken;
                                    foreach (JToken imzaToken in imzalarArray)
                                    {
                                        var imza = new YetkiliImza();
                                        
                                        if (imzaToken.Type == JTokenType.Object)
                                        {
                                            var imzaObj = (JObject)imzaToken;
                                            var base64Token = imzaObj["Base64Image"];
                                            imza.Base64Image = base64Token?.ToString();
                                            
                                            var slotToken = imzaObj["SlotIndex"];
                                            imza.SlotIndex = slotToken != null ? slotToken.Value<int>() : 0;
                                        }
                                        else
                                        {
                                            // Eğer obje değilse, direkt string olarak al
                                            imza.Base64Image = imzaToken.ToString();
                                            imza.SlotIndex = 0;
                                        }
                                        
                                        kayit.Imzalar.Add(imza);
                                    }
                                }
                                else if (imzalarToken.Type == JTokenType.String || imzalarToken.Type == JTokenType.Object)
                                {
                                    // Tek bir imza varsa
                                    var imza = new YetkiliImza();
                                    if (imzalarToken.Type == JTokenType.Object)
                                    {
                                        var imzaObj = (JObject)imzalarToken;
                                        var base64Token = imzaObj["Base64Image"];
                                        imza.Base64Image = base64Token?.ToString();
                                        
                                        var slotToken = imzaObj["SlotIndex"];
                                        imza.SlotIndex = slotToken != null ? slotToken.Value<int>() : 0;
                                    }
                                    else
                                    {
                                        imza.Base64Image = imzalarToken.ToString();
                                        imza.SlotIndex = 0;
                                    }
                                    kayit.Imzalar.Add(imza);
                                }
                            }

                            yetkiliKayitlar.Add(kayit);
                        }

                        foreach (JObject sigObj in signaturesArray)
                        {
                            var signatureData = new SignatureData();
                            
                            var pageToken = sigObj["Page"];
                            signatureData.Page = pageToken != null ? pageToken.Value<int>() : 0;
                            
                            var xToken = sigObj["X"];
                            signatureData.X = xToken != null ? xToken.Value<int>() : 0;
                            
                            var yToken = sigObj["Y"];
                            signatureData.Y = yToken != null ? yToken.Value<int>() : 0;
                            
                            var widthToken = sigObj["Width"];
                            signatureData.Width = widthToken != null ? widthToken.Value<int>() : 0;
                            
                            var heightToken = sigObj["Height"];
                            signatureData.Height = heightToken != null ? heightToken.Value<int>() : 0;
                            
                            var imageToken = sigObj["Image"];
                            signatureData.Image = imageToken != null ? imageToken.ToString() : null;
                            
                            var sourcePathToken = sigObj["SourcePdfPath"];
                            signatureData.SourcePdfPath = sourcePathToken != null ? sourcePathToken.ToString() : null;
                            
                            signatures.Add(signatureData);
                        }

                        System.Diagnostics.Debug.WriteLine("JSON başarıyla parse edildi");
                    }
                    catch (JsonReaderException ex)
                    {
                        System.Diagnostics.Debug.WriteLine("JSON parse hatası: " + ex.Message);
                        throw new Exception("JSON verisi geçerli değil: " + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Beklenmeyen hata: " + ex.Message);
                        throw;
                    }
                    
                    System.Diagnostics.Debug.WriteLine("Yetkili kayıt sayısı: " + yetkiliKayitlar?.Count);
                    System.Diagnostics.Debug.WriteLine("İmza sayısı: " + signatures?.Count);
                } catch (Exception parseEx) {
                    System.Diagnostics.Debug.WriteLine("JSON parse hatası: " + parseEx.Message);
                    throw;
                }

                // SignatureAuthData nesnesini oluştur
                var signatureAuthData = new SignatureAuthData
                {
                    KaynakPdfAdi = signatures.FirstOrDefault()?.SourcePdfPath,
                    Yetkililer = new List<YetkiliData>()
                };

                // Yetkili kayıtlarını dönüştür
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
                        YetkiTutari = decimal.Parse(kayit.YetkiTutari ?? "0"),
                        YetkiDovizCinsi = kayit.YetkiDovizCinsi,
                        YetkiDurumu = kayit.YetkiDurumu,
                        Imzalar = new List<SignatureImage>()
                    };

                    // İmzaları ekle
                    if (kayit.Imzalar != null)
                    {
                        foreach (var imza in kayit.Imzalar)
                        {
                            yetkiliData.Imzalar.Add(new SignatureImage
                            {
                                ImageData = imza.Base64Image,
                                SiraNo = imza.SlotIndex,
                                SourcePdfPath = signatures.FirstOrDefault()?.SourcePdfPath
                            });
                        }
                    }

                    signatureAuthData.Yetkililer.Add(yetkiliData);
                }

                // Session'a kaydet
                SessionHelper.SetSignatureAuthData(signatureAuthData);

                return new { success = true, message = "Veriler başarıyla kaydedildi" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SaveSignatureWithAjax hatası: " + ex.Message);
                System.Diagnostics.Debug.WriteLine("Stack trace: " + ex.StackTrace);
                return new { success = false, error = ex.Message };
            }
        }

        protected void BtnSaveSignature_Click(object sender, EventArgs e)
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
                var yetkiliKayitJson = Request.Form["hdnYetkiliKayitlar"]; // Bu hidden field'ı frontend'e eklememiz gerekecek
                if (!string.IsNullOrEmpty(yetkiliKayitJson))
                {
                    var yetkiliKayitSerializer = new JavaScriptSerializer();
                    var yetkiliKayitlar = yetkiliKayitSerializer.Deserialize<List<YetkiliKayit>>(yetkiliKayitJson);

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
                else
                {
                    // Eğer tablo boşsa, formdaki mevcut veriyi ekle
                    var yetkiliData = new YetkiliData
                    {
                        YetkiliKontakt = txtYetkiliKontakt.Text,
                        YetkiliAdi = txtYetkiliAdi.Text,
                        YetkiSekli = selYetkiSekli.SelectedValue,
                        YetkiTarihi = DateTime.Now.ToString("dd.MM.yyyy"),
                        YetkiBitisTarihi = chkAksiKarar.Checked ? "Aksi Karara Kadar" : yetkiBitisTarihi.Text,
                        YetkiGrubu = selYetkiGrubu.SelectedValue,
                        SinirliYetkiDetaylari = txtSinirliYetkiDetaylari.Text,
                        YetkiTurleri = selYetkiTurleri.SelectedValue,
                        YetkiTutari = decimal.Parse(txtYetkiTutari.Text),
                        YetkiDovizCinsi = selYetkiDovizCinsi.SelectedValue,
                        YetkiDurumu = selYetkiDurumu.SelectedValue,
                        Imzalar = new List<SignatureImage>()
                    };
                    authData.Yetkililer.Add(yetkiliData);
                }

                string signaturesJson = Request.Form["hdnSignatures"];
                System.Diagnostics.Debug.WriteLine(String.Format("İmza verileri alındı: {0}", signaturesJson));

                if (string.IsNullOrEmpty(signaturesJson))
                {
                    ShowWarning("Lütfen en az bir imza seçiniz.", true);
                    return;
                }

                var signatureSerializer = new JavaScriptSerializer();
                var signatures = signatureSerializer.Deserialize<List<SignatureData>>(signaturesJson);

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

                // İmzaları kaydet ve yanıt gönder
                // İmzaları ilgili yetkililere ekle
                var yetkiliImzaEslesmesi = Request.Form["hdnYetkiliImzaEslesmesi"]; // Bu hidden field'ı frontend'e eklememiz gerekecek
                if (!string.IsNullOrEmpty(yetkiliImzaEslesmesi))
                {
                    var eslesmeSerializer = new JavaScriptSerializer();
                    var eslesmeler = eslesmeSerializer.Deserialize<Dictionary<int, int>>(yetkiliImzaEslesmesi); // yetkiliIndex -> imzaIndex eşleşmesi

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

                try
                {
                    // Session'a kaydet
                    SessionHelper.SetSignatureAuthData(authData);
                        
                    // Başarılı yanıt döndür
                    var successResponse = new { success = true, message = "Veriler başarıyla kaydedildi" };
                    var jsonResponse = JsonConvert.SerializeObject(successResponse, jsonSettings);
                    
                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.Write(jsonResponse);
                    Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                catch (Exception ex)
                {
                    var errorResponse = new { success = false, error = ex.Message };
                    var jsonError = JsonConvert.SerializeObject(errorResponse, jsonSettings);
                    
                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.Write(jsonError);
                    Response.End();
                }
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("İmza kaydetme hatası: {0}\nStack Trace: {1}", ex.Message, ex.StackTrace));
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var response = new {
                        success = false,
                        error = String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message)
                    };
                    
                    var responseSerializer = new JavaScriptSerializer();
                    var jsonError = responseSerializer.Serialize(response);
                    Response.Clear();
                    Response.ContentType = "application/json";
                    Response.Write(jsonError);
                    Response.Flush();
                    HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
                else
                {
                    ShowError(String.Format("İmza kaydedilirken bir hata oluştu: {0}", ex.Message));
                }
            }
        }

        private void ShowError(string message, bool persistent = false)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                String.Format("showNotification('{0}', 'error', {1});", 
                    HttpUtility.JavaScriptStringEncode(message),
                    persistent.ToString().ToLower()),
                true);
        }

        private void ShowWarning(string message, bool persistent = false)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                String.Format("showNotification('{0}', 'warning', {1});", 
                    HttpUtility.JavaScriptStringEncode(message),
                    persistent.ToString().ToLower()),
                true);
        }

        private void ShowMessage(string message, string type = "info", bool persistent = false)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                String.Format("showNotification('{0}', '{1}', {2});", 
                    HttpUtility.JavaScriptStringEncode(message),
                    type,
                    persistent.ToString().ToLower()),
                true);
        }
    }
}