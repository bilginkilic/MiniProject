# PivotData.aspx Excel Export Entegrasyonu

Bu rehber, mevcut `PivotData.aspx` sayfanızdaki "Excel Data" butonuna pivot tablo export özelliği eklemek için adım adım talimatlar içerir.

## Senaryo

- Windows Forms uygulamanızda WebBrowser kontrolü var
- WebBrowser içinde `PivotData.aspx` sayfası açılıyor
- Sayfada pivot.js ile pivot tablosu oluşturuluyor
- "Excel Data" butonuna tıklandığında pivot tablosu Excel'e export edilmeli

## Adım 1: HTML Sayfasına JavaScript Ekleyin

Mevcut `PivotData.aspx` sayfanıza aşağıdaki JavaScript fonksiyonunu ekleyin:

```javascript
// Excel'e export fonksiyonu
function exportPivotToExcel() {
    try {
        // Pivot tablosunu bul
        var table = document.querySelector('table.pvtTable');
        
        if (!table) {
            alert('Pivot tablosu bulunamadı. Lütfen önce veri yükleyin.');
            return;
        }
        
        // Tablo HTML'ini al
        var tableHtml = table.outerHTML;
        
        // Windows Forms uygulaması için: window.external kullanarak C# tarafına sinyal gönder
        if (window.external && typeof window.external.ExportToExcel === 'function') {
            // C# tarafında ExportToExcel metodu varsa onu çağır
            window.external.ExportToExcel(tableHtml);
        } else {
            // Alternatif: Tablo HTML'ini global değişkene kaydet
            window.pivotTableHtml = tableHtml;
            alert('Excel export başlatılıyor...\n\nEğer export başlamazsa, Windows Forms uygulamasındaki export butonunu kullanın.');
        }
    } catch (ex) {
        alert('Excel export sırasında hata oluştu: ' + ex.message);
    }
}
```

## Adım 2: "Excel Data" Butonunu Güncelleyin

Mevcut "Excel Data" butonunuza `onclick="exportPivotToExcel()"` ekleyin:

```html
<button type="button" class="btn btn-success" onclick="exportPivotToExcel()">Excel Data</button>
```

## Adım 3: Windows Forms Formuna C# Kodu Ekleyin

WebBrowser kontrolünün olduğu Windows Forms formunuza aşağıdaki kodu ekleyin:

### 3.1. ScriptingHelper Sınıfı

```csharp
using System.Runtime.InteropServices;

[ComVisible(true)]
public class ScriptingHelper
{
    private YourForm parentForm;

    public ScriptingHelper(YourForm form)
    {
        parentForm = form;
    }

    public void ExportToExcel(string tableHtml)
    {
        if (parentForm != null)
        {
            parentForm.ExportPivotTableToExcel(tableHtml);
        }
    }
}
```

### 3.2. Form Sınıfına Alanlar

```csharp
private ScriptingHelper scriptingHelper;
```

### 3.3. WebBrowser DocumentCompleted Event

```csharp
private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
{
    try
    {
        if (scriptingHelper == null)
        {
            scriptingHelper = new ScriptingHelper(this);
        }
        webBrowser.ObjectForScripting = scriptingHelper;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"ObjectForScripting ayarlanamadı: {ex.Message}");
    }
}
```

### 3.4. Export Metodu

```csharp
public void ExportPivotTableToExcel(string tableHtml)
{
    try
    {
        if (string.IsNullOrEmpty(tableHtml))
        {
            tableHtml = GetTableHtmlFromBrowser();
        }

        if (string.IsNullOrEmpty(tableHtml))
        {
            MessageBox.Show(
                "Pivot tablosu bulunamadı. Lütfen sayfanın tamamen yüklendiğinden emin olun.",
                "Tablo Bulunamadı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        // HTML'i parse et (HtmlHelper.cs'yi pivot2 klasöründen kopyalayın)
        DataTable dt = HtmlHelper.ParsePivotTable(tableHtml);

        if (dt == null || dt.Rows.Count == 0)
        {
            MessageBox.Show(
                "Tablo verisi bulunamadı veya boş.",
                "Veri Yok",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        // Excel'e kaydet
        SaveToExcel(dt);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            $"Excel export sırasında hata oluştu: {ex.Message}",
            "Hata",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error
        );
    }
}

private string GetTableHtmlFromBrowser()
{
    try
    {
        if (webBrowser.Document == null || webBrowser.Document.Body == null)
        {
            return null;
        }

        return webBrowser.Document.Body.InnerHtml;
    }
    catch (Exception ex)
    {
        throw new Exception($"WebBrowser'dan HTML alınamadı: {ex.Message}");
    }
}

private void SaveToExcel(DataTable dt)
{
    using (SaveFileDialog sfd = new SaveFileDialog())
    {
        sfd.Filter = "Excel Dosyası|*.xlsx";
        sfd.FileName = $"PivotTable_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        sfd.DefaultExt = "xlsx";

        if (sfd.ShowDialog() == DialogResult.OK)
        {
            // Aspose.Cells ile Excel oluştur
            Workbook workbook = new Workbook();
            Worksheet worksheet = workbook.Worksheets[0];
            worksheet.Name = "Pivot Table";

            // Stil tanımlamaları
            Style headerStyle = workbook.CreateStyle();
            headerStyle.Font.IsBold = true;
            headerStyle.ForegroundColor = Color.LightGray;
            headerStyle.Pattern = BackgroundType.Solid;
            headerStyle.HorizontalAlignment = TextAlignmentType.Center;

            // Header satırını yaz
            for (int col = 0; col < dt.Columns.Count; col++)
            {
                worksheet.Cells[0, col].PutValue(dt.Columns[col].ColumnName);
                worksheet.Cells[0, col].SetStyle(headerStyle);
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

            // Otomatik sütun genişliği
            worksheet.AutoFitColumns();

            // Excel dosyasını kaydet
            workbook.Save(sfd.FileName, SaveFormat.Xlsx);

            MessageBox.Show(
                $"Excel dosyası başarıyla kaydedildi:\n{sfd.FileName}",
                "Başarılı",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }
}
```

## Adım 4: HtmlHelper.cs'yi Ekleyin

`pivot2/HtmlHelper.cs` dosyasını projenize kopyalayın. Bu sınıf HTML tabloyu DataTable'a dönüştürür.

## Adım 5: Gereksinimler

1. **Aspose.Cells** NuGet paketi
2. **System.Web** referansı (HtmlHelper için)
3. **System.Runtime.InteropServices** using direktifi

## Test

1. Windows Forms uygulamanızı çalıştırın
2. WebBrowser içinde PivotData.aspx sayfasını açın
3. "Load Data" butonuna tıklayın
4. Pivot tablosu oluşturulduktan sonra "Excel Data" butonuna tıklayın
5. Excel dosyasının kaydedildiğini kontrol edin

## Sorun Giderme

### window.external çalışmıyor
- WebBrowser kontrolünün `ObjectForScripting` özelliğinin ayarlandığından emin olun
- `DocumentCompleted` event'inde ayarlayın

### Tablo bulunamıyor
- Pivot tablosunun `pvtTable` class'ına sahip olduğundan emin olun
- Sayfanın tamamen yüklendiğinden emin olun

### Excel dosyası boş
- HtmlHelper.ParsePivotTable metodunun doğru çalıştığını kontrol edin
- Tablo HTML'inin doğru parse edildiğini kontrol edin

