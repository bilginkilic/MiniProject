using System;
using System.Web;

namespace AspxExamples.Common.Models
{
    public static class SessionHelper
    {
        private const string SIGNATURE_AUTH_KEY = "SignatureAuthData";
        
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
    }
}
