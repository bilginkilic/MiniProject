using System;
using System.IO;
using System.Data.SqlClient;

namespace KvkkTools
{
    public class FileExtractor
    {
        private readonly string connectionString;

        public FileExtractor(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public byte[] GetFileData(int libraryId, int id)
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

        public void SaveToFile(int libraryId, int id, string extension, string outputPath)
        {
            byte[] fileData = GetFileData(libraryId, id);
            if (fileData != null && fileData.Length > 0)
            {
                string fullPath = Path.Combine(outputPath, $"file_{libraryId}_{id}{extension}");
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllBytes(fullPath, fileData);
            }
        }

        public void BatchExtract(string outputPath, params (int LibraryId, int Id, string Extension)[] files)
        {
            foreach (var file in files)
            {
                try
                {
                    SaveToFile(file.LibraryId, file.Id, file.Extension, outputPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Dosya çıkarma hatası (LibraryId={file.LibraryId}, Id={file.Id}): {ex.Message}");
                }
            }
        }
    }
} 