/* v1 - 2024.01.17 14:30 - FileUploadViewer.aspx.cs - PDF Dosya Yükleme ve Görüntüleme */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Services;
using System.Web.Script.Services;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AspxExamples
{
    public partial class FileUploadViewer : System.Web.UI.Page
    {
        private const string NETWORK_PATH = @"\\trrgap3027\files\circular\cdn";
        private const string WEB_PATH = "http://trrgap3027/BTMUAppsUI/cdn";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // CDN klasörünü kontrol et
                if (!Directory.Exists(NETWORK_PATH))
                {
                    try
                    {
                        Directory.CreateDirectory(NETWORK_PATH);
                    }
                    catch (Exception ex)
                    {
                        ShowError(String.Format("CDN klasörü oluşturma hatası: {0}", ex.Message));
                        return;
                    }
                }
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (fuPdfUpload.HasFile)
                {
                    string fileName = Path.GetFileName(fuPdfUpload.FileName);
                    if (Path.GetExtension(fileName).ToLower() != ".pdf")
                    {
                        ShowError("Lütfen sadece PDF formatında dosya yükleyiniz.");
                        return;
                    }

                    string networkPath = Path.Combine(NETWORK_PATH, fileName);
                    string webPath = String.Format("{0}/{1}", WEB_PATH, fileName);

                    // Dosyayı network path'e kaydet
                    fuPdfUpload.SaveAs(networkPath);

                    // PDF görüntüleyiciyi ayarla
                    var iframe = new System.Web.UI.HtmlControls.HtmlGenericControl("iframe");
                    iframe.Attributes["src"] = webPath;
                    iframe.Attributes["style"] = "width:100%; height:100%; border:none;";
                    pageContents.Controls.Clear();
                    pageContents.Controls.Add(iframe);

                    // Hidden field'a dosya yolunu kaydet
                    hdnSelectedFile.Value = networkPath;

                    ShowMessage("Dosya başarıyla yüklendi.", "success");
                }
                else
                {
                    ShowError("Lütfen bir dosya seçiniz.");
                }
            }
            catch (Exception ex)
            {
                ShowError(String.Format("Dosya yüklenirken bir hata oluştu: {0}", ex.Message));
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object SaveAndReturn(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string fileName = Path.GetFileName(filePath);
                    string webPath = String.Format("{0}/{1}", WEB_PATH, fileName);

                    // Session'a dosya bilgilerini kaydet
                    var uploadResult = new UploadedFileResult
                    {
                        FilePath = webPath,
                        FileName = fileName,
                        UploadDate = DateTime.Now
                    };
                    SessionHelper.SetUploadedFile(uploadResult);
                    return new { success = true };
                }
                return new { success = false, error = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object SetSelectedFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // Session'a dosya yolunu kaydet
                    HttpContext.Current.Session["SelectedFilePath"] = filePath;
                    return new { success = true };
                }
                return new { success = false, error = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, error = ex.Message };
            }
        }

        private void ShowError(string message)
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                String.Format("showNotification('{0}', 'error');", HttpUtility.JavaScriptStringEncode(message)),
                true);
        }

        private void ShowMessage(string message, string type = "info")
        {
            ScriptManager.RegisterStartupScript(this, GetType(),
                "showNotification",
                String.Format("showNotification('{0}', '{1}');", HttpUtility.JavaScriptStringEncode(message), type),
                true);
        }
    }
}