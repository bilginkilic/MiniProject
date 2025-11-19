using System;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;

namespace PivotViewer
{
    public static class HtmlHelper
    {
        /// <summary>
        /// HTML string'den pvtTable class'ına sahip tabloyu bulur ve DataTable'a dönüştürür
        /// </summary>
        public static DataTable ParsePivotTable(string html)
        {
            if (string.IsNullOrEmpty(html))
                throw new ArgumentException("HTML boş olamaz", nameof(html));

            // pvtTable class'ına sahip tabloyu bul
            string tablePattern = @"<table[^>]*class\s*=\s*[""']?[^""']*pvtTable[^""']*[""']?[^>]*>(.*?)</table>";
            Match tableMatch = Regex.Match(html, tablePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (!tableMatch.Success)
                throw new Exception("pvtTable class'ına sahip tablo bulunamadı");

            string tableHtml = tableMatch.Value;
            return ParseHtmlTable(tableHtml);
        }

        /// <summary>
        /// HTML tablo string'ini DataTable'a dönüştürür
        /// </summary>
        public static DataTable ParseHtmlTable(string tableHtml)
        {
            DataTable dt = new DataTable();

            // Tbody içeriğini al (eğer varsa)
            string tbodyPattern = @"<tbody[^>]*>(.*?)</tbody>";
            Match tbodyMatch = Regex.Match(tableHtml, tbodyPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string bodyContent = tbodyMatch.Success ? tbodyMatch.Groups[1].Value : tableHtml;

            // Thead içeriğini al (eğer varsa)
            string theadPattern = @"<thead[^>]*>(.*?)</thead>";
            Match theadMatch = Regex.Match(tableHtml, theadPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            string headerContent = theadMatch.Success ? theadMatch.Groups[1].Value : string.Empty;

            // Tüm satırları bul
            string rowPattern = @"<tr[^>]*>(.*?)</tr>";
            MatchCollection rowMatches = Regex.Matches(tableHtml, rowPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (rowMatches.Count == 0)
                return dt;

            // İlk satırı header olarak kullan
            bool isFirstRow = true;
            int maxColumns = 0;

            foreach (Match rowMatch in rowMatches)
            {
                string rowHtml = rowMatch.Groups[1].Value;
                string[] cells = ExtractCells(rowHtml);

                if (isFirstRow)
                {
                    // Header satırı - sütunları oluştur
                    foreach (string cell in cells)
                    {
                        string cellText = CleanHtml(cell);
                        if (string.IsNullOrEmpty(cellText))
                            cellText = "Column" + (dt.Columns.Count + 1);
                        dt.Columns.Add(cellText, typeof(string));
                    }
                    maxColumns = dt.Columns.Count;
                    isFirstRow = false;
                }
                else
                {
                    // Data satırı
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < cells.Length && i < maxColumns; i++)
                    {
                        string cellText = CleanHtml(cells[i]);
                        dr[i] = cellText;
                    }
                    // Eksik hücreleri boş string ile doldur
                    for (int i = cells.Length; i < maxColumns; i++)
                    {
                        dr[i] = string.Empty;
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        /// <summary>
        /// Satır HTML'inden hücreleri çıkarır (th ve td etiketleri)
        /// </summary>
        private static string[] ExtractCells(string rowHtml)
        {
            System.Collections.Generic.List<string> cells = new System.Collections.Generic.List<string>();

            // th ve td etiketlerini bul
            string cellPattern = @"<(th|td)[^>]*(?:colspan\s*=\s*[""']?(\d+)[""']?)?[^>]*>(.*?)</\1>";
            MatchCollection cellMatches = Regex.Matches(rowHtml, cellPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match cellMatch in cellMatches)
            {
                string cellContent = cellMatch.Groups[3].Value;
                string colspanStr = cellMatch.Groups[2].Value;
                int colspan = 1;

                if (!string.IsNullOrEmpty(colspanStr) && int.TryParse(colspanStr, out int col))
                {
                    colspan = col;
                }

                // Hücreyi ekle
                cells.Add(cellContent);

                // Colspan varsa boş hücreler ekle
                for (int i = 1; i < colspan; i++)
                {
                    cells.Add(string.Empty);
                }
            }

            return cells.ToArray();
        }

        /// <summary>
        /// HTML etiketlerini temizler ve metin içeriğini döndürür
        /// </summary>
        private static string CleanHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            // HTML etiketlerini kaldır
            string cleaned = Regex.Replace(html, @"<[^>]+>", string.Empty, RegexOptions.IgnoreCase);

            // HTML entity'leri decode et
            cleaned = System.Web.HttpUtility.HtmlDecode(cleaned);

            // Fazla boşlukları temizle
            cleaned = Regex.Replace(cleaned, @"\s+", " ", RegexOptions.Multiline);
            cleaned = cleaned.Trim();

            return cleaned;
        }
    }
}

