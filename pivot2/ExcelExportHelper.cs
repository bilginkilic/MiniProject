// Excel Export Helper - Mevcut C# projenize ekleyin
// Bu kodu mevcut formunuza ekleyin

using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Aspose.Cells;

namespace YourNamespace
{
    // WebBrowser'dan JavaScript çağrıları için COM-visible sınıf
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

    // Mevcut formunuza ekleyeceğiniz metodlar:
    public partial class YourForm : Form
    {
        private ScriptingHelper scriptingHelper;
        private WebBrowser webBrowser; // Mevcut WebBrowser kontrolünüz

        // Form constructor'ında veya Load event'inde:
        private void InitializeExcelExport()
        {
            // Scripting helper oluştur
            scriptingHelper = new ScriptingHelper(this);
            
            // WebBrowser DocumentCompleted event'inde:
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                // JavaScript'ten C# metodlarına erişim için ObjectForScripting ayarla
                webBrowser.ObjectForScripting = scriptingHelper;
            }
            catch (Exception ex)
            {
                // Eski browser uyumluluğu için hata yok sayılabilir
                System.Diagnostics.Debug.WriteLine($"ObjectForScripting ayarlanamadı: {ex.Message}");
            }
        }

        // HTML sayfasındaki "Excel Data" butonundan çağrılacak metod
        public void ExportPivotTableToExcel(string tableHtml)
        {
            try
            {
                if (string.IsNullOrEmpty(tableHtml))
                {
                    // Eğer HTML gelmediyse, WebBrowser'dan al
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

                // HTML'i parse et (HtmlHelper sınıfını pivot2 klasöründen kopyalayın)
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
                    $"Excel export sırasında hata oluştu: {ex.Message}\n\nDetay: {ex.StackTrace}",
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
                // WebBrowser Document hazır mı kontrol et
                if (webBrowser.Document == null || webBrowser.Document.Body == null)
                {
                    return null;
                }

                // Tüm body HTML'ini al ve C# tarafında parse et
                string bodyHtml = webBrowser.Document.Body.InnerHtml;
                
                if (!string.IsNullOrEmpty(bodyHtml))
                {
                    return bodyHtml;
                }

                return null;
            }
            catch (Exception ex)
            {
                // Hata durumunda body HTML'ini döndür
                try
                {
                    if (webBrowser.Document != null && webBrowser.Document.Body != null)
                    {
                        return webBrowser.Document.Body.InnerHtml;
                    }
                }
                catch
                {
                    // İkinci hata durumunda null döndür
                }
                
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
                    // Yeni bir çalışma kitabı oluştur
                    Workbook workbook = new Workbook();
                    Worksheet worksheet = workbook.Worksheets[0];
                    worksheet.Name = "Pivot Table";

                    // Stil tanımlamaları
                    Style headerStyle = workbook.CreateStyle();
                    headerStyle.Font.IsBold = true;
                    headerStyle.ForegroundColor = Color.LightGray;
                    headerStyle.Pattern = BackgroundType.Solid;
                    headerStyle.HorizontalAlignment = TextAlignmentType.Center;
                    headerStyle.SetBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
                    headerStyle.SetBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
                    headerStyle.SetBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
                    headerStyle.SetBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);

                    // Header satırını yaz
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        Cell cell = worksheet.Cells[0, col];
                        cell.PutValue(dt.Columns[col].ColumnName);
                        cell.SetStyle(headerStyle);
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

                    // Kenarlıkları ayarla
                    Range dataRange = worksheet.Cells.CreateRange(0, 0, dt.Rows.Count + 1, dt.Columns.Count);
                    dataRange.SetOutlineBorder(BorderType.TopBorder, CellBorderType.Thin, Color.Black);
                    dataRange.SetOutlineBorder(BorderType.BottomBorder, CellBorderType.Thin, Color.Black);
                    dataRange.SetOutlineBorder(BorderType.LeftBorder, CellBorderType.Thin, Color.Black);
                    dataRange.SetOutlineBorder(BorderType.RightBorder, CellBorderType.Thin, Color.Black);
                    dataRange.SetInsideBorder(BorderType.VerticalBorder, CellBorderType.Thin, Color.Black);
                    dataRange.SetInsideBorder(BorderType.HorizontalBorder, CellBorderType.Thin, Color.Black);

                    // Alternatif satır renklendirmesi
                    Style altRowStyle = workbook.CreateStyle();
                    altRowStyle.ForegroundColor = Color.FromArgb(242, 242, 242);
                    altRowStyle.Pattern = BackgroundType.Solid;

                    for (int row = 1; row <= dt.Rows.Count; row++)
                    {
                        if (row % 2 == 0)
                        {
                            Range altRange = worksheet.Cells.CreateRange(row, 0, 1, dt.Columns.Count);
                            altRange.SetStyle(altRowStyle);
                        }
                    }

                    // Otomatik sütun genişliği
                    worksheet.AutoFitColumns();

                    // Filtreleme özelliğini ekle
                    worksheet.AutoFilter.Range = dataRange;

                    // Excel dosyasını kaydet
                    XlsSaveOptions saveOptions = new XlsSaveOptions(SaveFormat.Xlsx);
                    saveOptions.UpdateExternalLinks = false;
                    workbook.Save(sfd.FileName, saveOptions);

                    MessageBox.Show(
                        $"Excel dosyası başarıyla kaydedildi:\n{sfd.FileName}",
                        "Başarılı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
        }
    }
}

