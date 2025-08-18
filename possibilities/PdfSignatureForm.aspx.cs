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
/* net x kulaklık */

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

    public partial class PdfSignatureForm : System.Web.UI.Page
    {
        private string _cdn = @"\\trrgap3027\files\circular\cdn";
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

                // Başlangıç mesajını göster
                ShowMessage("PDF formatında imza sirkülerinizi yükleyerek başlayabilirsiniz.", "info");
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
                System.Diagnostics.Debug.WriteLine("Gelen yetkiliKayitlarJson: " + yetkiliKayitlarJson);
                System.Diagnostics.Debug.WriteLine("Gelen signatureDataJson: " + signatureDataJson);

                // Test amaçlı hemen başarılı yanıt dön
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
                    // Web servise gönderilecek veriyi hazırla
                    var requestData = new
                    {
                        referenceId = Request.QueryString["ref"], // URL'den ref parametresini al
                        authData = authData
                    };

                    // ASMX web servis çağrısı
                    var service = new SignatureService();
                    var response = service.SaveSignature(requestData.referenceId, authData);
                        
                        // Başarılı yanıt döndür
                        var response = new { success = true, message = "Veriler başarıyla kaydedildi", referenceId = requestData.referenceId };
                        var jsonResponse = serializer.Serialize(response);
                        
                        Response.Clear();
                        Response.ContentType = "application/json";
                        Response.Write(jsonResponse);
                        Response.Flush();
                        HttpContext.Current.ApplicationInstance.CompleteRequest();
                    }
                catch (Exception ex)
                {
                    var errorResponse = new { success = false, error = ex.Message };
                    var errorSerializer = new JavaScriptSerializer();
                    var jsonError = errorSerializer.Serialize(errorResponse);
                    
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