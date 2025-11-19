# Excel Export Entegrasyon Rehberi

Bu rehber, mevcut Windows Forms uygulamanızdaki "Excel Data" butonuna pivot tablo export özelliği eklemek için adım adım talimatlar içerir.

## Gereksinimler

1. **Aspose.Cells** kütüphanesi (projede zaten olmalı)
2. **HtmlHelper.cs** sınıfı (pivot2 klasöründen kopyalayın)
3. **WebBrowser** kontrolü mevcut formunuzda olmalı

## Adım 1: HtmlHelper.cs'yi Projenize Ekleyin

`pivot2/HtmlHelper.cs` dosyasını mevcut projenize kopyalayın. Bu sınıf HTML tabloyu DataTable'a dönüştürür.

## Adım 2: C# Kodunu Ekleyin

### 2.1. ScriptingHelper Sınıfını Ekleyin

Mevcut form dosyanıza (örneğin `YourForm.cs`) aşağıdaki sınıfı ekleyin:

```csharp
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

### 2.2. Form Sınıfınıza Alanlar Ekleyin

```csharp
private ScriptingHelper scriptingHelper;
```

### 2.3. Form Constructor veya Load Event'inde

```csharp
private void InitializeExcelExport()
{
    scriptingHelper = new ScriptingHelper(this);
    webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
}

private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
{
    try
    {
        webBrowser.ObjectForScripting = scriptingHelper;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"ObjectForScripting ayarlanamadı: {ex.Message}");
    }
}
```

### 2.4. Export Metodunu Ekleyin

`ExcelExportHelper.cs` dosyasındaki `ExportPivotTableToExcel`, `GetTableHtmlFromBrowser` ve `SaveToExcel` metodlarını formunuza ekleyin.

## Adım 3: HTML Sayfasını Güncelleyin

### 3.1. JavaScript Fonksiyonunu Ekleyin

HTML sayfanıza (pivot tablosunun olduğu sayfa) aşağıdaki JavaScript kodunu ekleyin:

```javascript
function exportPivotToExcel() {
    try {
        var table = document.querySelector('table.pvtTable');
        
        if (!table) {
            alert('Pivot tablosu bulunamadı. Lütfen önce veri yükleyin.');
            return;
        }
        
        var tableHtml = table.outerHTML;
        
        if (window.external && typeof window.external.ExportToExcel === 'function') {
            window.external.ExportToExcel(tableHtml);
        } else {
            window.pivotTableHtml = tableHtml;
            alert('Excel export başlatılıyor...');
        }
    } catch (ex) {
        alert('Excel export sırasında hata oluştu: ' + ex.message);
    }
}
```

### 3.2. "Excel Data" Butonunu Güncelleyin

Mevcut "Excel Data" butonunuza `onclick="exportPivotToExcel()"` ekleyin:

```html
<button onclick="exportPivotToExcel()">Excel Data</button>
```

veya mevcut butonunuz zaten bir event handler kullanıyorsa:

```html
<button id="btnExcelData" onclick="exportPivotToExcel()">Excel Data</button>
```

## Adım 4: Proje Referanslarını Kontrol Edin

1. **System.Web** referansının eklendiğinden emin olun (HtmlHelper için)
2. **Aspose.Cells** NuGet paketinin yüklü olduğundan emin olun
3. **System.Runtime.InteropServices** using direktifi eklendiğinden emin olun

## Adım 5: Test Edin

1. Uygulamayı çalıştırın
2. Pivot tablosunu yükleyin
3. "Excel Data" butonuna tıklayın
4. Excel dosyasının kaydedildiğini kontrol edin

## Sorun Giderme

### "ObjectForScripting ayarlanamadı" hatası
- Eski browser (IE11) kullanıyorsanız bu normal olabilir
- Alternatif olarak, WebBrowser'dan doğrudan HTML'i alabilirsiniz

### "pvtTable class'ına sahip tablo bulunamadı" hatası
- Pivot tablosunun yüklendiğinden emin olun
- "Load Data" butonuna tıklayın ve tablonun göründüğünü kontrol edin

### Excel dosyası boş geliyor
- HtmlHelper.ParsePivotTable metodunun doğru çalıştığından emin olun
- Tablo HTML'inin doğru parse edildiğini kontrol edin

## Notlar

- Bu çözüm eski browser (IE11) uyumludur
- Aspose.Cells kütüphanesi lisanslı olmalıdır
- Büyük pivot tabloları için performans testi yapın

