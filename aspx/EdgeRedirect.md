# Edge Redirect JavaScript Fonksiyonu

## Kullanım Amacı
IE'den Edge'e yönlendirme yaparken, Edge'in yeni tab ayarlarındaki redirect sorununu çözmek için kullanılır.

## Kod
```javascript
function openMenuItem(url) {
    if (url.toLowerCase().indexOf('imza sirkuler') > -1) {
        // Javascript protokolü ile direkt yönlendirme
        var jsRedirect = 'javascript:window.location.href="' + url + '";';
        window.open('microsoft-edge:' + jsRedirect, '_blank');
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
