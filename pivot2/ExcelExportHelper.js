// Excel Export Helper - Mevcut HTML sayfanıza ekleyin
// "Excel Data" butonuna onclick="exportPivotToExcel()" ekleyin

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
            // Eski browser uyumluluğu için alternatif yöntem
            // Tablo HTML'ini global değişkene kaydet (C# tarafından erişilebilir)
            window.pivotTableHtml = tableHtml;
            
            // C# tarafından bu değişkeni okuyabilir
            alert('Excel export başlatılıyor...');
        }
    } catch (ex) {
        alert('Excel export sırasında hata oluştu: ' + ex.message);
    }
}

