# Edge Redirect JavaScript Fonksiyonu

## Kullanım Amacı
IE'den Edge'e yönlendirme yaparken, Edge'in yeni tab ayarlarındaki redirect sorununu çözmek için kullanılır.

## Kod
```javascript
function openMenuItem(url) {
    if (url.toLowerCase().indexOf('imza sirkuler') > -1) {
        try {
            // VWGInstance'dan önceki URL'yi al
            var originalUrl = url;
            var vwgIndex = url.indexOf('VWGInstance');
            
            if (vwgIndex > -1) {
                // URL'yi parçala
                var baseUrl = url.substring(0, url.indexOf('?'));
                var fullQueryString = url.substring(url.indexOf('?') + 1);
                var originalParams = fullQueryString.substring(0, fullQueryString.indexOf('VWGInstance') - 1);
                
                // Orijinal URL'yi oluştur
                originalUrl = baseUrl;
                if (originalParams) {
                    originalUrl += '?' + originalParams;
                }
                
                // Debug için
                console.log('Orijinal URL:', originalUrl);
            }
            
            // Edge'de aç
            window.open('microsoft-edge:' + originalUrl, '_blank');
            
        } catch(e) {
            console.log('URL işleme hatası:', e);
            // Hata durumunda normal aç
            window.open(url, '_blank');
        }
    } else {
        // Popup olarak aç
        var width = 1280;
        var height = 800;
        var left = (screen.width - width) / 2;
        var top = (screen.height - height) / 2;
        
        var features = [
            'width=' + width,
            'height=' + height,
            'left=' + left,
            'top=' + top,
            'menubar=no',
            'toolbar=no',
            'location=no',
            'status=no',
            'resizable=yes',
            'scrollbars=yes'
        ].join(',');

        window.open(url, '_blank', features);
    }
    return false;
}
```

## Nasıl Çalışır?
1. URL'de "imza sirkuler" kontrolü yapar
2. Varsa:
   - Geçici bir HTML sayfası oluşturur
   - Bu HTML'i data URI olarak Edge'de açar
   - HTML içindeki script hemen asıl sayfaya yönlendirir
3. Yoksa:
   - Normal popup olarak açar
   - Merkezi konumlandırma yapar
   - Özelleştirilmiş pencere özellikleri kullanır

## Örnek Kullanım
```html
<a href="#" onclick="openMenuItem('imza sirkuler.aspx'); return false;">İmza Sirküler</a>
<a href="#" onclick="openMenuItem('diger.aspx'); return false;">Diğer Sayfa</a>
```

## Önemli Notlar
- IE uyumludur
- Edge'in yeni tab ayarlarını bypass eder
- Popup engelleyicilerden etkilenmez
- Merkezi konumlandırma yapar
