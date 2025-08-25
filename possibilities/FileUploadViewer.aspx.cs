/* v9 - Created: 2024.01.17 - Simplified to show PDF directly */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.Services;
using System.Web.Script.Services;
using Newtonsoft.Json;

namespace AspxExamples
{
    public partial class FileUploadViewer : System.Web.UI.Page
    {
        private const string CDN_PATH = @"\\trrgap3027\files\circular\cdn";
        private const string ALLOWED_EXTENSION = ".pdf";
        private const string CDN_WEB_PATH = "http://trrgap3027/circular/cdn";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!Directory.Exists(CDN_PATH))
                {
                    try 
                    {
                        Directory.CreateDirectory(CDN_PATH);
                    }
                    catch (Exception ex)
                    {
                        ShowError("Sistem hazırlığı sırasında bir hata oluştu. Lütfen yöneticinize başvurun.");
                        return;
                    }
                }

                string filePath = Request.QueryString["file"];
                if (!string.IsNullOrEmpty(filePath))
                {
                    hdnCurrentFile.Value = filePath;
                }

                btnSave.Enabled = false;
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
        {
            try
            {
                if (fuPdfUpload.HasFile)
                {
                    string fileName = Path.GetFileName(fuPdfUpload.FileName);
                    
                    if (!fileName.EndsWith(ALLOWED_EXTENSION, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception(string.Format("Sadece {0} dosyaları yüklenebilir.", ALLOWED_EXTENSION));
                    }

                    string uniqueFileName = string.Format("{0}_{1}", 
                        DateTime.Now.Ticks.ToString(), 
                        fileName);

                    string cdnFilePath = Path.Combine(CDN_PATH, uniqueFileName);
                    fuPdfUpload.SaveAs(cdnFilePath);

                    string webUrl = string.Format("{0}/{1}", 
                        CDN_WEB_PATH.TrimEnd('/'), 
                        uniqueFileName);
                    
                    string script = string.Format(@"
                        fileList.push('{0}');
                        updateFileList();
                        viewFile('{0}');
                        enableSaveButton();
                        showNotification('Dosya başarıyla yüklendi', 'success');
                    ", webUrl);
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "uploadSuccess", script, true);
                }
            }
            catch (Exception ex)
            {
                string errorScript = string.Format(
                    "showNotification('{0}', 'error');", 
                    HttpUtility.JavaScriptStringEncode(ex.Message)
                );
                ScriptManager.RegisterStartupScript(this, GetType(), "uploadError", errorScript, true);
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object SaveAndReturn(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    throw new Exception("Dosya yolu belirtilmedi.");
                }

                string fileName = Path.GetFileName(filePath);
                string fullPath = Path.Combine(CDN_PATH, fileName);

                if (!File.Exists(fullPath))
                {
                    throw new Exception("Dosya bulunamadı.");
                }

                return new { 
                    success = true, 
                    filePath = filePath 
                };
            }
            catch (Exception ex)
            {
                return new { 
                    success = false, 
                    error = ex.Message 
                };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object DeleteFile(string filePath)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                string fullPath = Path.Combine(CDN_PATH, fileName);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    return new { success = true };
                }
                
                return new { success = false, message = "Dosya bulunamadı." };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }
        }

        private void ShowError(string message)
        {
            string script = string.Format(
                "showNotification('{0}', 'error');",
                HttpUtility.JavaScriptStringEncode(message)
            );
            ScriptManager.RegisterStartupScript(this, GetType(), "error", script, true);
        }
    }
}