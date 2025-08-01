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
                string baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}";
                string applicationPath = request.ApplicationPath.TrimEnd('/');
                
                // Fiziksel yolu kontrol et
                string physicalPath = context.Server.MapPath($"~/aspx/{fileName}");
                if (!File.Exists(physicalPath))
                {
                    throw new FileNotFoundException($"ASPX dosyası bulunamadı: {fileName}");
                }

                // URL'yi oluştur
                return $"{baseUrl}{applicationPath}/aspx/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"ASPX URL oluşturulurken hata: {ex.Message}", ex);
            }
        }

        public static string GetRelativeAspxUrl(string fileName)
        {
            // Relative URL'yi oluştur
            return VirtualPathUtility.ToAbsolute($"~/aspx/{fileName}");
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
                string baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}";
                string applicationPath = request.ApplicationPath;
                string physicalPath = context.Server.MapPath($"~/aspx/{fileName}");
                
                return $"Debug Info:\n" +
                       $"Base URL: {baseUrl}\n" +
                       $"App Path: {applicationPath}\n" +
                       $"Physical Path: {physicalPath}\n" +
                       $"File Exists: {File.Exists(physicalPath)}";
            }
            catch (Exception ex)
            {
                return $"Debug Error: {ex.Message}";
            }
        }
    }
}