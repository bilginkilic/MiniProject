var authData = ModernPopupExample.ShowAuthorizedUserList("REF001");
if (authData != null)
{
    // Tüm veriler burada
    string yetkiliAdi = authData.YetkiliAdi;
    string yetkiSekli = authData.YetkiSekli;
    decimal yetkiTutari = authData.YetkiTutari;
    
    // İmzalar
    foreach (var imza in authData.Imzalar)
    {
        string imageData = imza.ImageData;  // İmza resmi (path)
        int siraNo = imza.SiraNo;          // İmza sırası
        string pdfKaynak = imza.SourcePdfPath; // İmzanın alındığı PDF
    }
    
    // Diğer bilgiler...
    string kaynakPdf = authData.KaynakPdfAdi;
}