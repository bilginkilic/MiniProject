<%@ WebHandler Language="C#" Class="ExcelExport" %>

using System;
using System.Web;
using System.IO;
using System.Web.Script.Serialization;
using Aspose.Cells;

public class ExcelExport : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        try
        {
            // JSON verisini al
            string jsonData = null;
            
            // POST body'den oku
            if (context.Request.HttpMethod == "POST")
            {
                // Önce Content-Type'ı kontrol et
                string contentType = context.Request.ContentType ?? "";
                
                // JSON content type ise (application/json)
                if (contentType.Contains("application/json"))
                {
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }
                // Form data ise (application/x-www-form-urlencoded veya multipart/form-data)
                else if (contentType.Contains("application/x-www-form-urlencoded") || 
                         contentType.Contains("multipart/form-data") ||
                         !string.IsNullOrEmpty(context.Request.Form["data"]))
                {
                    // Form'dan "data" field'ını al
                    jsonData = context.Request.Form["data"];
                    
                    // Eğer form'da yoksa, InputStream'den oku (form-encoded olabilir)
                    if (string.IsNullOrEmpty(jsonData))
                    {
                        using (var reader = new StreamReader(context.Request.InputStream))
                        {
                            string formData = reader.ReadToEnd();
                            // Form data formatından "data=" kısmını çıkar
                            if (formData.StartsWith("data="))
                            {
                                jsonData = HttpUtility.UrlDecode(formData.Substring(5));
                            }
                            else
                            {
                                jsonData = formData;
                            }
                        }
                    }
                }
                else
                {
                    // Content-Type belirtilmemişse, InputStream'den oku
                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }
            }
            // Query string'den oku (eski IE uyumluluğu için - sadece küçük veriler için)
            else if (!string.IsNullOrEmpty(context.Request.QueryString["data"]))
            {
                jsonData = context.Request.QueryString["data"];
            }
            
            if (string.IsNullOrEmpty(jsonData))
            {
                throw new Exception("Veri bulunamadı!");
            }
            
            // JSON'u deserialize et
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = int.MaxValue; // Büyük veri setleri için
            
            dynamic data = serializer.DeserializeObject(jsonData);
            
            // Veri tipini kontrol et
            if (data == null || !(data is System.Collections.Generic.Dictionary<string, object>))
            {
                throw new Exception("Geçersiz veri formatı!");
            }
            
            var dataDict = data as System.Collections.Generic.Dictionary<string, object>;
            
            // Export tipini belirle
            string exportType = dataDict.ContainsKey("exportType") ? dataDict["exportType"].ToString() : "pivotTable";
            string fileName = dataDict.ContainsKey("fileName") ? dataDict["fileName"].ToString() : "Export.xlsx";
            string sheetName = dataDict.ContainsKey("sheetName") ? dataDict["sheetName"].ToString() : "Sheet1";
            
            // Aspose.Cells Workbook oluştur
            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets[0];
            worksheet.Name = sheetName;
            
            // Export tipine göre işle
            if (exportType == "pivotTable" && dataDict.ContainsKey("tableData"))
            {
                // Pivot tablo verisi (2D array)
                var tableData = dataDict["tableData"] as System.Collections.ArrayList;
                if (tableData != null && tableData.Count > 0)
                {
                    int rowIndex = 0;
                    foreach (System.Collections.ArrayList row in tableData)
                    {
                        int colIndex = 0;
                        foreach (var cellValue in row)
                        {
                            if (cellValue != null)
                            {
                                worksheet.Cells[rowIndex, colIndex].PutValue(cellValue.ToString());
                            }
                            colIndex++;
                        }
                        rowIndex++;
                    }
                    
                    // Sütun genişliklerini otomatik ayarla
                    worksheet.AutoFitColumns();
                }
            }
            else if (exportType == "jsonData" && dataDict.ContainsKey("data"))
            {
                // JSON verisi (array of objects)
                var jsonArray = dataDict["data"] as System.Collections.ArrayList;
                if (jsonArray != null && jsonArray.Count > 0)
                {
                    // İlk satır: başlıklar
                    var firstRow = jsonArray[0] as System.Collections.Generic.Dictionary<string, object>;
                    if (firstRow != null)
                    {
                        int colIndex = 0;
                        foreach (var key in firstRow.Keys)
                        {
                            worksheet.Cells[0, colIndex].PutValue(key);
                            worksheet.Cells[0, colIndex].GetStyle().Font.IsBold = true;
                            colIndex++;
                        }
                        
                        // Veri satırları
                        int rowIndex = 1;
                        foreach (System.Collections.Generic.Dictionary<string, object> row in jsonArray)
                        {
                            colIndex = 0;
                            foreach (var key in firstRow.Keys)
                            {
                                if (row.ContainsKey(key) && row[key] != null)
                                {
                                    worksheet.Cells[rowIndex, colIndex].PutValue(row[key].ToString());
                                }
                                colIndex++;
                            }
                            rowIndex++;
                        }
                        
                        // Sütun genişliklerini otomatik ayarla
                        worksheet.AutoFitColumns();
                    }
                }
            }
            else if (exportType == "pivotData" && dataDict.ContainsKey("pivotData"))
            {
                // PivotData objesi (PivotTable.js formatı)
                var pivotData = dataDict["pivotData"] as System.Collections.Generic.Dictionary<string, object>;
                if (pivotData != null)
                {
                    // Row keys ve col keys'i al
                    var rowKeys = pivotData.ContainsKey("rowKeys") ? 
                        pivotData["rowKeys"] as System.Collections.ArrayList : new System.Collections.ArrayList();
                    var colKeys = pivotData.ContainsKey("colKeys") ? 
                        pivotData["colKeys"] as System.Collections.ArrayList : new System.Collections.ArrayList();
                    var dataMatrix = pivotData.ContainsKey("dataMatrix") ? 
                        pivotData["dataMatrix"] as System.Collections.ArrayList : new System.Collections.ArrayList();
                    
                    int rowIndex = 0;
                    
                    // Başlık satırı
                    if (colKeys.Count > 0)
                    {
                        int colIndex = 0;
                        if (rowKeys.Count > 0)
                        {
                            worksheet.Cells[rowIndex, colIndex].PutValue(""); // Boş hücre
                            colIndex++;
                        }
                        foreach (var colKey in colKeys)
                        {
                            worksheet.Cells[rowIndex, colIndex].PutValue(colKey != null ? colKey.ToString() : "");
                            worksheet.Cells[rowIndex, colIndex].GetStyle().Font.IsBold = true;
                            colIndex++;
                        }
                        rowIndex++;
                    }
                    
                    // Veri satırları
                    if (rowKeys.Count > 0 && dataMatrix.Count > 0)
                    {
                        for (int i = 0; i < rowKeys.Count && i < dataMatrix.Count; i++)
                        {
                            int colIndex = 0;
                            
                            // Row label
                            worksheet.Cells[rowIndex, colIndex].PutValue(rowKeys[i] != null ? rowKeys[i].ToString() : "");
                            colIndex++;
                            
                            // Veri değerleri
                            var rowData = dataMatrix[i] as System.Collections.ArrayList;
                            if (rowData != null)
                            {
                                foreach (var value in rowData)
                                {
                                    if (value != null)
                                    {
                                        // Sayısal değer kontrolü
                                        double numValue;
                                        if (double.TryParse(value.ToString(), out numValue))
                                        {
                                            worksheet.Cells[rowIndex, colIndex].PutValue(numValue);
                                        }
                                        else
                                        {
                                            worksheet.Cells[rowIndex, colIndex].PutValue(value.ToString());
                                        }
                                    }
                                    colIndex++;
                                }
                            }
                            rowIndex++;
                        }
                    }
                    
                    // Sütun genişliklerini otomatik ayarla
                    worksheet.AutoFitColumns();
                }
            }
            
            // Dosya adını temizle (geçersiz karakterleri kaldır)
            fileName = CleanFileName(fileName);
            if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".xlsx";
            }
            
            // Excel dosyasını memory stream'e yaz
            MemoryStream stream = new MemoryStream();
            workbook.Save(stream, SaveFormat.Xlsx);
            stream.Position = 0;
            
            // Response'u ayarla (IE uyumlu)
            context.Response.Clear();
            context.Response.ClearContent();
            context.Response.ClearHeaders();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            
            // IE için dosya adını URL encode et
            string encodedFileName = HttpUtility.UrlEncode(fileName);
            context.Response.AppendHeader("Content-Disposition", 
                string.Format("attachment; filename=\"{0}\"; filename*=UTF-8''{1}", fileName, encodedFileName));
            context.Response.AppendHeader("Content-Length", stream.Length.ToString());
            
            // Cache kontrolü (IE için)
            context.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            context.Response.Cache.SetNoStore();
            
            // Dosyayı gönder
            byte[] buffer = stream.ToArray();
            context.Response.BinaryWrite(buffer);
            context.Response.Flush();
            
            stream.Close();
            stream.Dispose();
            
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
            context.Response.Clear();
            context.Response.ClearContent();
            context.Response.ClearHeaders();
            context.Response.Buffer = true;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                success = false,
                error = ex.Message,
                stackTrace = ex.StackTrace
            };
            
            var serializer = new JavaScriptSerializer();
            context.Response.Write(serializer.Serialize(errorResponse));
            context.Response.Flush();
            
            HttpContext.Current.ApplicationInstance.CompleteRequest();
        }
    }
    
    private string CleanFileName(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return "Export.xlsx";
        
        // Geçersiz dosya adı karakterlerini kaldır
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            fileName = fileName.Replace(c, '_');
        }
        
        return fileName;
    }
    
    public bool IsReusable
    {
        get { return false; }
    }
}

