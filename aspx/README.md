# ASPX Popup Örnekleri

Bu klasör, Gizmox WebGUI uygulamasında ASPX sayfalarını popup olarak açmak için çeşitli örnekler içerir.

## Dosyalar

1. `SimpleAspxPopup.cs`: Basit bir popup form örneği
2. `JavaScriptPopup.cs`: JavaScript window.open kullanarak popup açma örneği
3. `ModalAspxPopup.cs`: Modal dialog şeklinde ASPX popup örneği
4. `CustomAspxPopup.cs`: Özelleştirilmiş tasarıma sahip popup örneği
5. `ExamplePage.aspx`: Test için örnek ASPX sayfası

## Kullanım

Her örnek sınıf kendi içinde kullanım örneği içermektedir. Genel olarak:

```csharp
// Basit popup
SimplePopupExample.ShowPopup();

// JavaScript popup
var jsPopup = new JavaScriptPopup();
jsPopup.Show();

// Modal popup
ModalPopupExample.ShowModalPopup();

// Özelleştirilmiş popup
CustomPopupExample.ShowCustomPopup();
```

## Önemli Notlar

1. ASPX URL'lerini kendi projenize göre düzenleyin
2. Cross-origin istekleri için gerekli güvenlik ayarlarını yapın
3. HTTPS kullanıyorsanız, tüm kaynakların HTTPS olduğundan emin olun
4. Popup boyutlarını içeriğe göre ayarlayın

## Gereksinimler

- Gizmox.WebGUI.Forms
- Gizmox.WebGUI.Common
- .NET Framework 4.5.1 veya üzeri