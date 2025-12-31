# DataTable'ı UI Katmanından App Katmanına Güvenli Gönderme

## Problem
DataTable'ı XML serialization ile göndermeye çalışırken hata alınması. DataTable `ISerializable` olsa bile, içindeki veri tipleri (binary, complex types, circular references) XML serialization'ı desteklemeyebilir.

## Çözüm 1: JSON Serialization (ÖNERİLEN - En Güvenli)

### UI Katmanında (Serialize)

```csharp
using System.Web.Script.Serialization;
using System.Collections.Generic;

// Yöntem 1: JavaScriptSerializer ile (ASP.NET built-in)
public static string DataTableToJson(DataTable dt)
{
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    serializer.MaxJsonLength = int.MaxValue; // Büyük veri setleri için
    
    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
    
    foreach (DataRow dr in dt.Rows)
    {
        Dictionary<string, object> row = new Dictionary<string, object>();
        foreach (DataColumn col in dt.Columns)
        {
            // Null ve DBNull kontrolü
            object value = dr[col];
            if (value == DBNull.Value || value == null)
            {
                row[col.ColumnName] = null;
            }
            else if (value is DateTime)
            {
                // DateTime'ı ISO format string'e çevir
                row[col.ColumnName] = ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            }
            else if (value is byte[])
            {
                // Binary data'yı Base64'e çevir
                row[col.ColumnName] = Convert.ToBase64String((byte[])value);
            }
            else
            {
                row[col.ColumnName] = value;
            }
        }
        rows.Add(row);
    }
    
    return serializer.Serialize(rows);
}

// Kullanım
string jsonData = DataTableToJson(myDataTable);
```

### Yöntem 2: Newtonsoft.Json ile (Daha Esnek)

```csharp
using Newtonsoft.Json;
using System.Collections.Generic;

public static string DataTableToJsonSafe(DataTable dt)
{
    List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
    
    foreach (DataRow dr in dt.Rows)
    {
        Dictionary<string, object> row = new Dictionary<string, object>();
        foreach (DataColumn col in dt.Columns)
        {
            object value = dr[col];
            
            if (value == DBNull.Value || value == null)
            {
                row[col.ColumnName] = null;
            }
            else if (value is DateTime)
            {
                row[col.ColumnName] = ((DateTime)value).ToString("O"); // ISO 8601
            }
            else if (value is byte[])
            {
                row[col.ColumnName] = Convert.ToBase64String((byte[])value);
            }
            else
            {
                row[col.ColumnName] = value;
            }
        }
        rows.Add(row);
    }
    
    return JsonConvert.SerializeObject(rows, new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        MaxDepth = 32,
        DateFormatHandling = DateFormatHandling.IsoDateFormat
    });
}
```

### App Katmanında (Deserialize)

```csharp
using System.Web.Script.Serialization;
using System.Data;

public static DataTable JsonToDataTable(string jsonData)
{
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    serializer.MaxJsonLength = int.MaxValue;
    
    List<Dictionary<string, object>> rows = 
        serializer.Deserialize<List<Dictionary<string, object>>>(jsonData);
    
    if (rows == null || rows.Count == 0)
    {
        return new DataTable();
    }
    
    DataTable dt = new DataTable();
    
    // İlk satırdan sütunları oluştur
    foreach (var key in rows[0].Keys)
    {
        dt.Columns.Add(key, typeof(object));
    }
    
    // Satırları ekle
    foreach (var row in rows)
    {
        DataRow dr = dt.NewRow();
        foreach (var kvp in row)
        {
            if (kvp.Value != null)
            {
                // Base64 string'leri byte[]'e çevir
                if (kvp.Value is string strValue && 
                    strValue.Length > 0 && 
                    IsBase64String(strValue))
                {
                    try
                    {
                        dr[kvp.Key] = Convert.FromBase64String(strValue);
                    }
                    catch
                    {
                        dr[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    dr[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                dr[kvp.Key] = DBNull.Value;
            }
        }
        dt.Rows.Add(dr);
    }
    
    return dt;
}

private static bool IsBase64String(string s)
{
    if (string.IsNullOrEmpty(s) || s.Length % 4 != 0)
        return false;
    
    try
    {
        Convert.FromBase64String(s);
        return true;
    }
    catch
    {
        return false;
    }
}
```

## Çözüm 2: AJAX ile JSON Gönderme (ASP.NET Web Forms)

### JavaScript (UI Katmanı)

```javascript
function sendDataTableToServer(dataTable) {
    // DataTable'ı JSON'a çevir (client-side)
    var jsonData = JSON.stringify(convertDataTableToJson(dataTable));
    
    // AJAX ile gönder
    $.ajax({
        type: "POST",
        url: "YourPage.aspx/ProcessDataTable",
        data: JSON.stringify({ dataTableJson: jsonData }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function(response) {
            if (response.d.Success) {
                console.log("Başarılı!");
            }
        },
        error: function(xhr, status, error) {
            console.error("Hata:", error);
        }
    });
}

// Client-side DataTable'ı JSON'a çevirme (jQuery DataTables için)
function convertDataTableToJson(table) {
    var data = [];
    table.rows().every(function() {
        var row = {};
        table.columns().every(function() {
            var col = table.column(this);
            row[col.header().textContent] = col.data()[this[0]];
        });
        data.push(row);
    });
    return data;
}
```

### Server-Side (Code-Behind)

```csharp
[WebMethod]
public static object ProcessDataTable(string dataTableJson)
{
    try
    {
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        serializer.MaxJsonLength = int.MaxValue;
        
        List<Dictionary<string, object>> rows = 
            serializer.Deserialize<List<Dictionary<string, object>>>(dataTableJson);
        
        DataTable dt = JsonToDataTable(dataTableJson);
        
        // App katmanına gönder
        YourAppLayer.ProcessData(dt);
        
        return new { Success = true, Message = "Başarılı" };
    }
    catch (Exception ex)
    {
        return new { Success = false, Message = ex.Message };
    }
}
```

## Çözüm 3: Hidden Field veya ViewState ile (Küçük Veriler İçin)

```csharp
// UI Katmanında
string jsonData = DataTableToJson(myDataTable);
ViewState["MyDataTable"] = jsonData;
// veya
hdnDataTable.Value = jsonData;

// App katmanında
string jsonData = ViewState["MyDataTable"] as string;
// veya
string jsonData = hdnDataTable.Value;
DataTable dt = JsonToDataTable(jsonData);
```

## Çözüm 4: Base64 Binary Serialization (Alternatif - Daha Az Tercih Edilir)

```csharp
public static string DataTableToBase64(DataTable dt)
{
    using (MemoryStream ms = new MemoryStream())
    {
        dt.WriteXml(ms, XmlWriteMode.WriteSchema);
        byte[] data = ms.ToArray();
        return Convert.ToBase64String(data);
    }
}

public static DataTable Base64ToDataTable(string base64String)
{
    byte[] data = Convert.FromBase64String(base64String);
    using (MemoryStream ms = new MemoryStream(data))
    {
        DataTable dt = new DataTable();
        dt.ReadXml(ms);
        return dt;
    }
}
```

**Not:** Bu yöntem XML serialization kullanır, bu yüzden aynı hataları verebilir.

## Önerilen Yaklaşım

1. **Küçük-Orta Veri Setleri (< 10MB):** JSON Serialization (Çözüm 1)
2. **Büyük Veri Setleri (> 10MB):** 
   - Veriyi sayfalara böl (pagination)
   - Veya doğrudan veritabanından app katmanında çek
3. **Binary Data İçeren:** Base64 encoding ile JSON içinde gönder

## XML Hatası Nedenleri

1. **Binary veri tipleri** (byte[], Image, vb.)
2. **Complex objects** (custom classes)
3. **Circular references**
4. **DateTime format sorunları**
5. **DBNull.Value** XML'de desteklenmez

## Güvenlik Notları

- JSON verisini gönderirken **HTTPS** kullanın
- Büyük veri setleri için **compression** düşünün
- **SQL Injection** riski için parametreli sorgular kullanın
- **XSS** koruması için JSON'u doğru şekilde escape edin

## Performans İpuçları

- `MaxJsonLength` değerini artırın (int.MaxValue)
- Büyük veri setleri için streaming kullanın
- Gereksiz sütunları filtreleyin
- Sadece gerekli satırları gönderin

