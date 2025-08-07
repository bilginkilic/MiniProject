using System;
using System.Web;
using System.Web.UI;
using Gizmox.WebGUI.Forms;

namespace AspxExamples
{
    public static class CircularRef
    {
        public static void ShowModernPopup(string aspxFileName)
        {
            try
            {
                // ASPX sayfasının tam URL'ini oluştur
                string aspxUrl = AspxUrlHelper.GetAspxUrl(aspxFileName);

                // ModernAspxPopup'ı oluştur ve göster
                using (var popup = new ModernAspxPopup(aspxFileName))
                {
                    // Form başlığını ayarla
                    popup.Text = "İmza Sirküler Yönetimi";
                    
                    // Minimum boyut ayarla
                    popup.MinimumSize = new System.Drawing.Size(1024, 768);
                    
                    // Başlangıç boyutu
                    popup.Size = new System.Drawing.Size(1280, 900);
                    
                    // Form özelliklerini ayarla
                    popup.StartPosition = FormStartPosition.CenterScreen;
                    popup.MaximizeBox = true;
                    popup.MinimizeBox = true;
                    popup.FormBorderStyle = FormBorderStyle.Sizable;

                    // Popup'ı göster
                    popup.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda JavaScript alert göster
                string errorMessage = string.Format("Popup açılırken hata oluştu: {0}", ex.Message);
                Page currentPage = HttpContext.Current.Handler as Page;
                if (currentPage != null)
                {
                    ScriptManager.RegisterStartupScript(currentPage, currentPage.GetType(), 
                        "PopupError", string.Format("alert('{0}');", 
                        HttpUtility.JavaScriptStringEncode(errorMessage)), true);
                }
            }
        }

        public static void ShowModernPopupWithSize(string aspxFileName, int width, int height)
        {
            try
            {
                using (var popup = new ModernAspxPopup(aspxFileName))
                {
                    popup.Text = "İmza Sirküler Yönetimi";
                    popup.Size = new System.Drawing.Size(width, height);
                    popup.StartPosition = FormStartPosition.CenterScreen;
                    popup.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = string.Format("Popup açılırken hata oluştu: {0}", ex.Message);
                Page currentPage = HttpContext.Current.Handler as Page;
                if (currentPage != null)
                {
                    ScriptManager.RegisterStartupScript(currentPage, currentPage.GetType(), 
                        "PopupError", string.Format("alert('{0}');", 
                        HttpUtility.JavaScriptStringEncode(errorMessage)), true);
                }
            }
        }
    }
}