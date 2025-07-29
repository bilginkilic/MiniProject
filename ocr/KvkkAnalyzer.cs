using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tesseract;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace KvkkTools
{
    public partial class KvkkAnalyzer : Form
    {
        private DataTable filesTable = new DataTable();
        private string connectionString = "Server=TRLVDB1012\\Turkish;Database=EBA;Integrated Security=True;";
        private TesseractEngine tesseract;
        private FileExtractor fileExtractor;
        private string outputDirectory = "output_files";
        
        public KvkkAnalyzer()
        {
            InitializeComponent();
            InitializeTesseract();
            fileExtractor = new FileExtractor(connectionString);
            SetupDataGrid();
            LoadFiles();
        }

        private void InitializeComponent()
        {
            this.Text = "KVKK Veri Analizi";
            this.Size = new Size(1200, 800);

            // DataGridView
            dataGridFiles = new DataGridView();
            dataGridFiles.Dock = DockStyle.Fill;
            dataGridFiles.AllowUserToAddRows = false;
            dataGridFiles.MultiSelect = true;
            dataGridFiles.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Filtre Paneli
            Panel filterPanel = new Panel();
            filterPanel.Dock = DockStyle.Top;
            filterPanel.Height = 100;

            // Filtre Kontrolleri
            txtSearch = new TextBox();
            txtSearch.Location = new Point(10, 10);
            txtSearch.Size = new Size(200, 20);
            txtSearch.TextChanged += TxtSearch_TextChanged;

            cmbFileType = new ComboBox();
            cmbFileType.Location = new Point(220, 10);
            cmbFileType.Size = new Size(100, 20);
            cmbFileType.Items.AddRange(new object[] { "Tümü", ".pdf", ".doc", ".xls", ".png" });
            cmbFileType.SelectedIndex = 0;
            cmbFileType.SelectedIndexChanged += CmbFileType_SelectedIndexChanged;

            btnAnalyze = new Button();
            btnAnalyze.Text = "Seçili Dosyaları Analiz Et";
            btnAnalyze.Location = new Point(330, 10);
            btnAnalyze.Size = new Size(150, 30);
            btnAnalyze.Click += BtnAnalyze_Click;

            btnExport = new Button();
            btnExport.Text = "Seçili Dosyaları İndir";
            btnExport.Location = new Point(490, 10);
            btnExport.Size = new Size(150, 30);
            btnExport.Click += BtnExport_Click;

            btnExportResults = new Button();
            btnExportResults.Text = "Sonuçları Excel'e Aktar";
            btnExportResults.Location = new Point(650, 10);
            btnExportResults.Size = new Size(150, 30);
            btnExportResults.Click += BtnExportResults_Click;

            // Sonuç Paneli
            txtResults = new RichTextBox();
            txtResults.Dock = DockStyle.Bottom;
            txtResults.Height = 200;
            txtResults.ReadOnly = true;

            // Progress Bar
            progressBar = new ProgressBar();
            progressBar.Dock = DockStyle.Bottom;
            progressBar.Height = 20;
            progressBar.Visible = false;

            // Kontrolleri forma ekle
            filterPanel.Controls.AddRange(new Control[] { txtSearch, cmbFileType, btnAnalyze, btnExport, btnExportResults });
            this.Controls.AddRange(new Control[] { filterPanel, dataGridFiles, progressBar, txtResults });
        }

        private void InitializeTesseract()
        {
            try
            {
                tesseract = new TesseractEngine(@"./tessdata", "tur", EngineMode.Default);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tesseract başlatılamadı. Lütfen tessdata klasörünün varlığını kontrol edin.\n" + ex.Message);
            }
        }

        private void SetupDataGrid()
        {
            filesTable.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("LIBRARYID", typeof(int)),
                new DataColumn("ID", typeof(int)),
                new DataColumn("CONTENTTYPE", typeof(string)),
                new DataColumn("FILESIZE", typeof(long)),
                new DataColumn("EXTENSION", typeof(string)),
                new DataColumn("MIMETYPE", typeof(string))
            });

            dataGridFiles.DataSource = filesTable;
        }

        private void LoadFiles()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT [LIBRARYID], [ID], [CONTENTTYPE], [FILESIZE], 
                                      [EXTENSION], [MIMETYPE]
                               FROM [EBA].[dbo].[EFSFILEDATA]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    filesTable.Load(reader);
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void CmbFileType_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            string searchText = txtSearch.Text.ToLower();
            string fileType = cmbFileType.SelectedItem.ToString();

            filesTable.DefaultView.RowFilter = "";

            if (!string.IsNullOrEmpty(searchText))
            {
                filesTable.DefaultView.RowFilter = string.Format("CONVERT(LIBRARYID, 'System.String') LIKE '%{0}%' OR " +
                                         "CONVERT(ID, 'System.String') LIKE '%{0}%'", searchText);
            }

            if (fileType != "Tümü")
            {
                string currentFilter = filesTable.DefaultView.RowFilter;
                string extensionFilter = string.Format("EXTENSION = '{0}'", fileType);
                
                filesTable.DefaultView.RowFilter = string.IsNullOrEmpty(currentFilter) 
                    ? extensionFilter 
                    : string.Format("{0} AND {1}", currentFilter, extensionFilter);
            }
        }

        private async void BtnAnalyze_Click(object sender, EventArgs e)
        {
            if (dataGridFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen analiz edilecek dosyaları seçin.");
                return;
            }

            txtResults.Clear();
            btnAnalyze.Enabled = false;

            try
            {
                foreach (DataGridViewRow row in dataGridFiles.SelectedRows)
                {
                    int libraryId = Convert.ToInt32(row.Cells["LIBRARYID"].Value);
                    int id = Convert.ToInt32(row.Cells["ID"].Value);
                    string extension = row.Cells["EXTENSION"].Value.ToString();

                    byte[] fileData = GetFileData(libraryId, id);
                    if (fileData != null)
                    {
                        string extractedText = await ExtractText(fileData, extension);
                        string fileInfoText = string.Format("LibraryID: {0}, ID: {1}", libraryId, id);
                        AnalyzeText(extractedText, fileInfoText);
                    }
                }
            }
            finally
            {
                btnAnalyze.Enabled = true;
            }
        }

        private byte[] GetFileData(int libraryId, int id)
        {
            using (MemoryStream ms = new MemoryStream())
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"SELECT [DATA] 
                               FROM [EBA].[dbo].[EFSFILEDATAPARTS]
                               WHERE [LIBRARYID] = @LibraryId AND [FILEDATAID] = @Id
                               ORDER BY [PARTINDEX]";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LibraryId", libraryId);
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] partData = (byte[])reader["DATA"];
                            ms.Write(partData, 0, partData.Length);
                        }
                    }
                }
                return ms.ToArray();
            }
        }

        private async Task<string> ExtractText(byte[] fileData, string extension)
        {
            string text = "";

            try
            {
                switch (extension.ToLower())
                {
                    case ".pdf":
                        using (MemoryStream ms = new MemoryStream(fileData))
                        using (PdfReader reader = new PdfReader(ms))
                        {
                            for (int page = 1; page <= reader.NumberOfPages; page++)
                            {
                                text += PdfTextExtractor.GetTextFromPage(reader, page);
                            }
                        }
                        break;

                    case ".png":
                    case ".jpg":
                    case ".jpeg":
                        using (MemoryStream ms = new MemoryStream(fileData))
                        using (var img = Bitmap.FromStream(ms))
                        using (var page = tesseract.Process((Bitmap)img))
                        {
                            text = page.GetText();
                        }
                        break;

                    // Word ve Excel için ayrı işleyiciler eklenebilir
                    default:
                        text = string.Format("[{0}] dosya tipi henüz desteklenmiyor.", extension);
                        break;
                }
            }
            catch (Exception ex)
            {
                text = string.Format("Metin çıkarma hatası: {0}", ex.Message);
            }

            return text;
        }

        private void AnalyzeText(string text, string fileInfo)
        {
            var patterns = new Dictionary<string, string>
            {
                { "TC Kimlik No", @"\b[1-9]{1}[0-9]{10}\b" },
                { "Kan Grubu", @"\b(A|B|AB|0)[+-]\b" },
                { "E-posta", @"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b" },
                { "Telefon", @"\b(0\s*)?([0-9]{3}\s*){2}[0-9]{4}\b" },
                { "Kredi Kartı", @"\b[0-9]{4}[\s-]?[0-9]{4}[\s-]?[0-9]{4}[\s-]?[0-9]{4}\b" }
            };

            txtResults.AppendText(string.Format("\n\n=== {0} ===\n", fileInfo));

            foreach (var pattern in patterns)
            {
                var matches = Regex.Matches(text, pattern.Value);
                if (matches.Count > 0)
                {
                    txtResults.AppendText(string.Format("\n{0} bulundu ({1} adet):\n", pattern.Key, matches.Count));
                    foreach (Match match in matches)
                    {
                        txtResults.AppendText(string.Format("- {0}\n", match.Value));
                    }
                }
            }
        }

        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dataGridFiles.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen indirilecek dosyaları seçin.");
                return;
            }

            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    progressBar.Visible = true;
                    progressBar.Maximum = dataGridFiles.SelectedRows.Count;
                    progressBar.Value = 0;

                    var files = new List<FileInfo>();
                    foreach (DataGridViewRow row in dataGridFiles.SelectedRows)
                    {
                        files.Add(new FileInfo(
                            Convert.ToInt32(row.Cells["LIBRARYID"].Value),
                            Convert.ToInt32(row.Cells["ID"].Value),
                            row.Cells["EXTENSION"].Value.ToString()
                        ));
                    }

                    Task.Run(() =>
                    {
                        fileExtractor.BatchExtract(fbd.SelectedPath, files);
                        this.Invoke((MethodInvoker)delegate
                        {
                            progressBar.Visible = false;
                            MessageBox.Show("Dosyalar başarıyla indirildi.");
                        });
                    });
                }
            }
        }

        private void BtnExportResults_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Excel Dosyası|*.xlsx";
                sfd.FileName = "KVKK_Analiz_Sonuclari.xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    using (var workbook = new XSSFWorkbook())
                    {
                        var sheet = workbook.CreateSheet("Analiz Sonuçları");
                        
                        // Başlık satırı
                        var headerRow = sheet.CreateRow(0);
                        headerRow.CreateCell(0).SetCellValue("Dosya Bilgisi");
                        headerRow.CreateCell(1).SetCellValue("Bulunan Veri Tipi");
                        headerRow.CreateCell(2).SetCellValue("Değer");

                        // Sonuçları parse et ve Excel'e aktar
                        string[] results = txtResults.Text.Split(new[] { "\n\n===" }, StringSplitOptions.RemoveEmptyEntries);
                        int rowIndex = 1;

                        foreach (string result in results)
                        {
                            string[] parts = result.Split('\n');
                            string fileInfo = parts[0].Replace("===", "").Trim();

                            for (int i = 1; i < parts.Length; i++)
                            {
                                string line = parts[i].Trim();
                                if (line.Contains("bulundu"))
                                {
                                    string dataType = line.Split('(')[0].Trim();
                                    i++; // Bir sonraki satıra geç (değerler)
                                    while (i < parts.Length && parts[i].StartsWith("-"))
                                    {
                                        var row = sheet.CreateRow(rowIndex++);
                                        row.CreateCell(0).SetCellValue(fileInfo);
                                        row.CreateCell(1).SetCellValue(dataType);
                                        row.CreateCell(2).SetCellValue(parts[i].Replace("-", "").Trim());
                                        i++;
                                    }
                                    i--; // Döngüde tekrar artacağı için azalt
                                }
                            }
                        }

                        // Sütun genişliklerini ayarla
                        sheet.AutoSizeColumn(0);
                        sheet.AutoSizeColumn(1);
                        sheet.AutoSizeColumn(2);

                        // Dosyayı kaydet
                        using (var fs = new FileStream(sfd.FileName, FileMode.Create, FileAccess.Write))
                        {
                            workbook.Write(fs);
                        }

                        MessageBox.Show("Sonuçlar Excel dosyasına aktarıldı.");
                    }
                }
            }
        }

        private DataGridView dataGridFiles;
        private TextBox txtSearch;
        private ComboBox cmbFileType;
        private Button btnAnalyze;
        private Button btnExport;
        private Button btnExportResults;
        private RichTextBox txtResults;
        private ProgressBar progressBar;
    }
} 