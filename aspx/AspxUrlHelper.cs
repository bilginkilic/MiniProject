using System;
using System.Web;

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
                    throw new InvalidOperationException("HttpContext.Current is null");
                }

                var request = context.Request;
                string baseUrl = $"{request.Url.Scheme}://{request.Url.Authority}";
                string applicationPath = request.ApplicationPath.TrimEnd('/');
                
                // Namespace'i dizin yapısına çevir
                string currentDirectory = typeof(AspxUrlHelper).Namespace.Replace(".", "/");
                
                return $"{baseUrl}{applicationPath}/{currentDirectory}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"ASPX URL oluşturulurken hata: {ex.Message}", ex);
            }
        }

        public static string GetRelativeAspxUrl(string fileName)
        {
            return $"~/aspx/{fileName}";
        }
    }
}