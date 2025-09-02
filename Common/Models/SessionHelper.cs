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
    }
}
