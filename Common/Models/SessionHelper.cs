using System;
using System.Web;

namespace AspxExamples.Common.Models
{
    public class UploadedFileResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public static class SessionHelper
    {
        private const string SIGNATURE_AUTH_KEY = "SignatureAuthData";
        private const string UPLOADED_FILE_KEY = "UploadedFileResult";
        private const string CLOSE_WINDOW_KEY = "CloseWindow";
        
        public static void SetSignatureAuthData(SignatureAuthData data)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[SIGNATURE_AUTH_KEY] = data;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }
        
        public static SignatureAuthData GetSignatureAuthData()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[SIGNATURE_AUTH_KEY] != null)
            {
                return (SignatureAuthData)HttpContext.Current.Session[SIGNATURE_AUTH_KEY];
            }
            return null;
        }
        
        public static void ClearSignatureAuthData()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(SIGNATURE_AUTH_KEY);
            }
        }

        public static void SetUploadedFile(UploadedFileResult data)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[UPLOADED_FILE_KEY] = data;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }

        public static UploadedFileResult GetUploadedFile()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[UPLOADED_FILE_KEY] != null)
            {
                return (UploadedFileResult)HttpContext.Current.Session[UPLOADED_FILE_KEY];
            }
            return null;
        }

        public static void ClearUploadedFile()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(UPLOADED_FILE_KEY);
            }
        }

        public static void SetCloseWindow(bool value)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[CLOSE_WINDOW_KEY] = value;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }

        public static bool GetCloseWindow()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[CLOSE_WINDOW_KEY] != null)
            {
                return (bool)HttpContext.Current.Session[CLOSE_WINDOW_KEY];
            }
            return false;
        }

        public static void ClearCloseWindow()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(CLOSE_WINDOW_KEY);
            }
        }
    }
}
