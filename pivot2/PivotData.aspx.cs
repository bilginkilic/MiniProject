using System;
using System.Data;
using System.Web.Services;
using System.Web.UI;

namespace YourNamespace
{
    public partial class PivotData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Sayfa yükleme işlemleri
        }

        [WebMethod]
        public static string GetPivotData(string dateFrom, string dateTo)
        {
            try
            {
                // Veritabanından veya başka bir kaynaktan veri çek
                // Bu kısmı kendi veri kaynağınıza göre düzenleyin
                
                // Örnek: DataTable'dan JSON'a dönüştürme
                DataTable dt = GetDataFromDatabase(dateFrom, dateTo);
                
                // DataTable'ı JSON'a dönüştür
                string json = DataTableToJson(dt);
                
                return json;
            }
            catch (Exception ex)
            {
                return "[]"; // Hata durumunda boş array döndür
            }
        }

        [WebMethod]
        public static bool ExportToExcel(string tableHtml)
        {
            try
            {
                // Bu metod Windows Forms uygulamasından çağrılacak
                // Eğer WebBrowser kontrolü kullanıyorsanız, window.external.ExportToExcel() kullanın
                // Bu metod sadece fallback olarak kullanılabilir
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static DataTable GetDataFromDatabase(string dateFrom, string dateTo)
        {
            // Veritabanından veri çekme işlemi
            // Bu kısmı kendi veri kaynağınıza göre düzenleyin
            
            DataTable dt = new DataTable();
            // ... veritabanı sorgusu ...
            
            return dt;
        }

        private static string DataTableToJson(DataTable dt)
        {
            // DataTable'ı JSON formatına dönüştür
            System.Web.Script.Serialization.JavaScriptSerializer serializer = 
                new System.Web.Script.Serialization.JavaScriptSerializer();
            
            System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>> rows = 
                new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>();
            
            System.Collections.Generic.Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new System.Collections.Generic.Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            
            return serializer.Serialize(rows);
        }
    }
}

