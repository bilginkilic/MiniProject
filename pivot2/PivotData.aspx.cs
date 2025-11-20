using System;
using System.Data;
using System.Web.Services;
using System.Web.UI;
using System.IO;
using System.Text;
using Aspose.Cells;
using PivotViewer;

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
        public static string ExportToExcel(string tableHtml)
        {
            try
            {
                if (string.IsNullOrEmpty(tableHtml))
                {
                    return string.Empty;
                }

                // HTML tabloyu DataTable'a çevir
                DataTable dt = HtmlHelper.ParsePivotTable(tableHtml);

                if (dt == null || dt.Rows.Count == 0)
                {
                    return string.Empty;
                }

                // Excel dosyası oluştur
                Workbook workbook = new Workbook();
                Worksheet worksheet = workbook.Worksheets[0];
                worksheet.Name = "Pivot Table";

                // Stil tanımlamaları
                Style headerStyle = workbook.CreateStyle();
                headerStyle.Font.IsBold = true;
                headerStyle.ForegroundColor = System.Drawing.Color.LightGray;
                headerStyle.Pattern = BackgroundType.Solid;
                headerStyle.HorizontalAlignment = TextAlignmentType.Center;
                headerStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                headerStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                headerStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                headerStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);

                // Header satırını yaz
                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    Cell cell = worksheet.Cells[0, col];
                    cell.PutValue(dt.Columns[col].ColumnName);
                    cell.SetStyle(headerStyle);
                }

                // Data satırlarını yaz
                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        object value = dt.Rows[row][col];
                        worksheet.Cells[row + 1, col].PutValue(value != null ? value.ToString() : string.Empty);
                    }
                }

                // Kenarlıkları ayarla
                Range dataRange = worksheet.Cells.CreateRange(0, 0, dt.Rows.Count + 1, dt.Columns.Count);
                dataRange.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataRange.SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataRange.SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataRange.SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataRange.SetInsideBorder(BorderType.VerticalBorder, CellBorderType.Thin, System.Drawing.Color.Black);
                dataRange.SetInsideBorder(BorderType.HorizontalBorder, CellBorderType.Thin, System.Drawing.Color.Black);

                // Alternatif satır renklendirmesi
                Style altRowStyle = workbook.CreateStyle();
                altRowStyle.ForegroundColor = System.Drawing.Color.FromArgb(242, 242, 242);
                altRowStyle.Pattern = BackgroundType.Solid;

                for (int row = 1; row <= dt.Rows.Count; row++)
                {
                    if (row % 2 == 0)
                    {
                        Range altRange = worksheet.Cells.CreateRange(row, 0, 1, dt.Columns.Count);
                        altRange.SetStyle(altRowStyle);
                    }
                }

                // Otomatik sütun genişliği
                worksheet.AutoFitColumns();

                // Excel dosyasını memory stream'e yaz
                MemoryStream stream = new MemoryStream();
                workbook.Save(stream, SaveFormat.Xlsx);
                stream.Position = 0;

                // Base64 string'e çevir
                byte[] buffer = stream.ToArray();
                string base64String = Convert.ToBase64String(buffer);

                stream.Close();
                stream.Dispose();
                workbook.Dispose();

                return base64String;
            }
            catch (Exception ex)
            {
                // Hata durumunda boş string döndür
                return string.Empty;
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

