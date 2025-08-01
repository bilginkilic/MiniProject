# ASPX Popup Örnekleri

Bu klasör, Gizmox WebGUI uygulamasında ASPX sayfalarını popup olarak açmak için çeşitli örnekler içerir.

## Dosyalar

1. `AspxUrlHelper.cs`: ASPX URL'lerini oluşturmak için yardımcı sınıf
2. `SimpleAspxPopup.cs`: Basit bir popup form örneği
3. `JavaScriptPopup.cs`: JavaScript window.open kullanarak popup açma örneği
4. `ModalAspxPopup.cs`: Modal dialog şeklinde ASPX popup örneği
5. `CustomAspxPopup.cs`: Özelleştirilmiş tasarıma sahip popup örneği
6. `ExamplePage.aspx`: Test için örnek ASPX sayfası

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

## URL Yönetimi

Tüm örnekler `AspxUrlHelper` sınıfını kullanarak ASPX sayfalarının URL'lerini otomatik olarak oluşturur:

```csharp
// URL alma
string aspxUrl = AspxUrlHelper.GetAspxUrl("ExamplePage.aspx");

// Relative URL alma
string relativeUrl = AspxUrlHelper.GetRelativeAspxUrl("ExamplePage.aspx");
```

## Önemli Notlar

1. URL'ler otomatik olarak proje yapısına göre oluşturulur
2. Cross-origin istekleri için gerekli güvenlik ayarlarını yapın
3. HTTPS kullanıyorsanız, URL'ler otomatik olarak HTTPS protokolünü kullanır
4. Popup boyutlarını içeriğe göre ayarlayın

## Gereksinimler

- Gizmox.WebGUI.Forms
- Gizmox.WebGUI.Common
- .NET Framework 4.5.1 veya üzeri