using System;
using System.Collections.Generic;
using System.Web;

namespace AspxExamples.Common.Models
{
    public class UploadedFileResult
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public class PdfFileInfo
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
    }

    public class PdfListResult
    {
        public List<PdfFileInfo> PdfFiles { get; set; }
        public DateTime SaveDate { get; set; }
        
        public PdfListResult()
        {
            PdfFiles = new List<PdfFileInfo>();
            SaveDate = DateTime.Now;
        }
    }

    public static class SessionHelper
    {
        private const string SIGNATURE_AUTH_KEY = "SignatureAuthData";
        private const string UPLOADED_FILE_KEY = "UploadedFileResult";
        private const string CLOSE_WINDOW_KEY = "CloseWindow";
        private const string INITIAL_YETKILI_DATA_KEY = "InitialYetkiliData";
        private const string SELECTED_PDF_KEY = "SelectedPdfFileName";
        private const string UPLOADED_PDF_PATH_KEY = "LastUploadedPdf";
        private const string PDF_LIST_KEY = "PdfListResult";
        
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

        public static void SetInitialYetkiliData(List<YetkiliKayit> data)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[INITIAL_YETKILI_DATA_KEY] = data;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }

        public static List<YetkiliKayit> GetInitialYetkiliData()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[INITIAL_YETKILI_DATA_KEY] != null)
            {
                return (List<YetkiliKayit>)HttpContext.Current.Session[INITIAL_YETKILI_DATA_KEY];
            }
            return null;
        }

        public static void ClearInitialYetkiliData()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(INITIAL_YETKILI_DATA_KEY);
            }
        }

        // PDF işlemleri için metodlar
        public static void SetSelectedPdfFileName(string fileName)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[SELECTED_PDF_KEY] = fileName;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }

        public static string GetSelectedPdfFileName()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[SELECTED_PDF_KEY] != null)
            {
                return HttpContext.Current.Session[SELECTED_PDF_KEY].ToString();
            }
            return null;
        }

        public static void SetUploadedPdfPath(string path)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[UPLOADED_PDF_PATH_KEY] = path;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }

        public static string GetUploadedPdfPath()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[UPLOADED_PDF_PATH_KEY] != null)
            {
                return HttpContext.Current.Session[UPLOADED_PDF_PATH_KEY].ToString();
            }
            return null;
        }

        public static void ClearPdfData()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(SELECTED_PDF_KEY);
                HttpContext.Current.Session.Remove(UPLOADED_PDF_PATH_KEY);
            }
        }

        // PDF Listesi işlemleri için metodlar
        public static void SetPdfList(PdfListResult pdfList)
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session[PDF_LIST_KEY] = pdfList;
                HttpContext.Current.Session.Timeout = 30; // 30 dakika
            }
        }

        public static PdfListResult GetPdfList()
        {
            if (HttpContext.Current != null && 
                HttpContext.Current.Session != null && 
                HttpContext.Current.Session[PDF_LIST_KEY] != null)
            {
                return (PdfListResult)HttpContext.Current.Session[PDF_LIST_KEY];
            }
            return null;
        }

        public static void ClearPdfList()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(PDF_LIST_KEY);
            }
        }

        public static void ClearAllPdfData()
        {
            if (HttpContext.Current != null && HttpContext.Current.Session != null)
            {
                HttpContext.Current.Session.Remove(SELECTED_PDF_KEY);
                HttpContext.Current.Session.Remove(UPLOADED_PDF_PATH_KEY);
                HttpContext.Current.Session.Remove(PDF_LIST_KEY);
            }
        }
    }
}
