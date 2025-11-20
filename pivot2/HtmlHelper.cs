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
        /// HTML tablo string'ini DataTable'a dönüştürür (rowspan desteği ile)
        /// </summary>
        public static DataTable ParseHtmlTable(string tableHtml)
        {
            DataTable dt = new DataTable();

            // Tüm satırları bul
            string rowPattern = @"<tr[^>]*>(.*?)</tr>";
            MatchCollection rowMatches = Regex.Matches(tableHtml, rowPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (rowMatches.Count == 0)
                return dt;

            // Rowspan tracking için: Her sütun için kaç satır daha devam edecek
            System.Collections.Generic.List<int> rowspanTracker = new System.Collections.Generic.List<int>();

            // İlk satırı header olarak kullan
            bool isFirstRow = true;
            int maxColumns = 0;

            foreach (Match rowMatch in rowMatches)
            {
                string rowHtml = rowMatch.Groups[1].Value;
                var cellData = ExtractCellsWithSpan(rowHtml);

                // Önceki satırlardan devam eden rowspan'leri kontrol et
                System.Collections.Generic.List<string> cells = new System.Collections.Generic.List<string>();
                int currentCol = 0;

                // Rowspan tracker'dan devam eden hücreleri ekle
                while (currentCol < rowspanTracker.Count && rowspanTracker[currentCol] > 0)
                {
                    // Bu sütunda rowspan devam ediyor, boş hücre ekle
                    cells.Add(string.Empty);
                    rowspanTracker[currentCol]--; // Rowspan sayacını azalt
                    currentCol++;
                }

                // Yeni hücreleri ekle
                foreach (var cell in cellData)
                {
                    // Eğer rowspan tracker'da bu sütun için yer yoksa, boş hücreler ekle
                    while (currentCol >= rowspanTracker.Count)
                    {
                        rowspanTracker.Add(0);
                    }

                    cells.Add(cell.Content);

                    // Colspan varsa boş hücreler ekle
                    for (int i = 1; i < cell.Colspan; i++)
                    {
                        cells.Add(string.Empty);
                        currentCol++;
                        if (currentCol >= rowspanTracker.Count)
                        {
                            rowspanTracker.Add(0);
                        }
                    }

                    // Rowspan varsa tracker'a ekle
                    if (cell.Rowspan > 1)
                    {
                        // Bu sütundan sonraki rowspan-1 satır için boş hücre olacak
                        rowspanTracker[currentCol] = cell.Rowspan - 1;
                    }

                    currentCol++;
                }

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
                    for (int i = 0; i < cells.Count && i < maxColumns; i++)
                    {
                        string cellText = CleanHtml(cells[i]);
                        dr[i] = cellText;
                    }
                    // Eksik hücreleri boş string ile doldur
                    for (int i = cells.Count; i < maxColumns; i++)
                    {
                        dr[i] = string.Empty;
                    }
                    dt.Rows.Add(dr);
                }
            }

            return dt;
        }

        /// <summary>
        /// Hücre bilgisi için yardımcı sınıf
        /// </summary>
        private class CellInfo
        {
            public string Content { get; set; }
            public int Colspan { get; set; }
            public int Rowspan { get; set; }
        }

        /// <summary>
        /// Satır HTML'inden hücreleri çıkarır (th ve td etiketleri, rowspan ve colspan desteği ile)
        /// </summary>
        private static System.Collections.Generic.List<CellInfo> ExtractCellsWithSpan(string rowHtml)
        {
            System.Collections.Generic.List<CellInfo> cells = new System.Collections.Generic.List<CellInfo>();

            // th ve td etiketlerini bul (rowspan ve colspan desteği ile)
            // Pattern: rowspan ve colspan'in sırası önemli değil
            string cellPattern = @"<(th|td)([^>]*)>(.*?)</\1>";
            MatchCollection cellMatches = Regex.Matches(rowHtml, cellPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);

            foreach (Match cellMatch in cellMatches)
            {
                string cellContent = cellMatch.Groups[3].Value;
                string attributes = cellMatch.Groups[2].Value;
                
                // Rowspan ve colspan'i attributes'tan çıkar
                string rowspanStr = string.Empty;
                string colspanStr = string.Empty;
                
                Match rowspanMatch = Regex.Match(attributes, @"rowspan\s*=\s*[""']?(\d+)[""']?", RegexOptions.IgnoreCase);
                if (rowspanMatch.Success)
                {
                    rowspanStr = rowspanMatch.Groups[1].Value;
                }
                
                Match colspanMatch = Regex.Match(attributes, @"colspan\s*=\s*[""']?(\d+)[""']?", RegexOptions.IgnoreCase);
                if (colspanMatch.Success)
                {
                    colspanStr = colspanMatch.Groups[1].Value;
                }
                
                int rowspan = 1;
                int colspan = 1;

                if (!string.IsNullOrEmpty(rowspanStr) && int.TryParse(rowspanStr, out int row))
                {
                    rowspan = row;
                }

                if (!string.IsNullOrEmpty(colspanStr) && int.TryParse(colspanStr, out int col))
                {
                    colspan = col;
                }

                cells.Add(new CellInfo
                {
                    Content = cellContent,
                    Colspan = colspan,
                    Rowspan = rowspan
                });
            }

            return cells;
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

