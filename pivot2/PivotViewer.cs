using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Aspose.Cells;

namespace PivotViewer
{
    [ComVisible(true)]
    public class ScriptingHelper
    {
        private PivotViewerForm parentForm;

        public ScriptingHelper(PivotViewerForm form)
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

    public partial class PivotViewerForm : Form
    {
        private WebBrowser webBrowser;
        private Button btnExportExcel;
        private Panel topPanel;
        private ScriptingHelper scriptingHelper;

        public PivotViewerForm()
        {
            InitializeCustomComponents();
            LoadHtmlPage();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Pivot Table Viewer - Excel Export";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;

            // Top Panel
            topPanel = new Panel();
            topPanel.Dock = DockStyle.Top;
            topPanel.Height = 50;
            topPanel.BackColor = Color.LightGray;
            topPanel.Padding = new Padding(10);

            // Export Button
            btnExportExcel = new Button();
            btnExportExcel.Text = "Excel'e Aktar";
            btnExportExcel.Size = new Size(150, 35);
            btnExportExcel.Location = new Point(10, 7);
            btnExportExcel.Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold);
            btnExportExcel.Click += BtnExportExcel_Click;
            topPanel.Controls.Add(btnExportExcel);

            // WebBrowser
            webBrowser = new WebBrowser();
            webBrowser.Dock = DockStyle.Fill;
            webBrowser.ScriptErrorsSuppressed = true;
            webBrowser.IsWebBrowserContextMenuEnabled = true;
            webBrowser.AllowWebBrowserDrop = true;
            webBrowser.DocumentCompleted += WebBrowser_DocumentCompleted;

            // Scripting helper oluştur
            scriptingHelper = new ScriptingHelper(this);

            this.Controls.Add(webBrowser);
            this.Controls.Add(topPanel);
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

        private void LoadHtmlPage()
        {
            try
            {
                // index.html dosyasının yolunu bul
                string htmlPath = Path.Combine(Application.StartupPath, "index.html");
                
                if (!File.Exists(htmlPath))
                {
                    MessageBox.Show(
                        $"index.html dosyası bulunamadı: {htmlPath}\n\nLütfen index.html dosyasının uygulama klasöründe olduğundan emin olun.",
                        "Dosya Bulunamadı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                webBrowser.Navigate(htmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"HTML sayfası yüklenirken hata oluştu: {ex.Message}",
                    "Hata",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
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

                // HTML'i parse et
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

        private void BtnExportExcel_Click(object sender, EventArgs e)
        {
            try
            {
                // WebBrowser'den HTML'i al
                string html = GetTableHtmlFromBrowser();

                if (string.IsNullOrEmpty(html))
                {
                    MessageBox.Show(
                        "Pivot tablosu bulunamadı. Lütfen sayfanın tamamen yüklendiğinden emin olun.",
                        "Tablo Bulunamadı",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                // HTML'i parse et
                DataTable dt = HtmlHelper.ParsePivotTable(html);

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

                // JavaScript ile tablo HTML'ini al (eski browser uyumlu)
                HtmlElement script = webBrowser.Document.CreateElement("script");
                script.SetAttribute("type", "text/javascript");
                
                // JavaScript kodu: pvtTable class'ına sahip tabloyu bul ve HTML'ini döndür
                string jsCode = @"
                    (function() {
                        var table = document.querySelector('table.pvtTable');
                        if (table) {
                            return table.outerHTML;
                        }
                        return '';
                    })();
                ";

                // Alternatif yöntem: Tüm body HTML'ini al ve C# tarafında parse et
                string bodyHtml = webBrowser.Document.Body.InnerHtml;
                
                // Eğer JavaScript çalışmıyorsa, body HTML'inden tabloyu bul
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

