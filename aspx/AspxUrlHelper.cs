using System;
using System.Web;
using System.IO;

namespace AspxExamples
{
    public static class AspxUrlHelper
    {
        public static string GetAspxUrl(string fileName)
        {
            try
            {
                var context = HttpContext.Current;
                if (context == null)
                {
                    // Eğer HttpContext yoksa, relative URL döndür
                    return GetRelativeAspxUrl(fileName);
                }

                var request = context.Request;
                string baseUrl = String.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);
                string applicationPath = request.ApplicationPath.TrimEnd('/');
                
                // Query string parametrelerini ayır
                string baseFileName = fileName;
                string queryString = "";
                
                int queryIndex = fileName.IndexOf('?');
                if (queryIndex >= 0)
                {
                    baseFileName = fileName.Substring(0, queryIndex);
                    queryString = fileName.Substring(queryIndex);
                }

                // Fiziksel yolu kontrol et
                string physicalPath = context.Server.MapPath(String.Format("~/aspx/{0}", baseFileName));
                if (!File.Exists(physicalPath))
                {
                    throw new FileNotFoundException(String.Format("ASPX dosyası bulunamadı: {0}", fileName));
                }

                // URL'yi oluştur
                return String.Format("{0}{1}/aspx/{2}{3}", baseUrl, applicationPath, baseFileName, queryString);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("ASPX URL oluşturulurken hata: {0}", ex.Message), ex);
            }
        }

        public static string GetRelativeAspxUrl(string fileName)
        {
            // Relative URL'yi oluştur
            return VirtualPathUtility.ToAbsolute(String.Format("~/aspx/{0}", fileName));
        }

        public static string GetDebugUrl(string fileName)
        {
            try
            {
                var context = HttpContext.Current;
                if (context == null)
                {
                    return "HttpContext.Current is null";
                }

                var request = context.Request;
                string baseUrl = String.Format("{0}://{1}", request.Url.Scheme, request.Url.Authority);
                string applicationPath = request.ApplicationPath;
                string physicalPath = context.Server.MapPath(String.Format("~/aspx/{0}", fileName));
                
                return String.Format(
                    "Debug Info:\nBase URL: {0}\nApp Path: {1}\nPhysical Path: {2}\nFile Exists: {3}",
                    baseUrl,
                    applicationPath,
                    physicalPath,
                    File.Exists(physicalPath)
                );
            }
            catch (Exception ex)
            {
                return String.Format("Debug Error: {0}", ex.Message);
            }
        }
    }
}