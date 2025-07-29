using System;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace KvkkTools
{
    public class FileInfo
    {
        public int LibraryId { get; set; }
        public int Id { get; set; }
        public string Extension { get; set; }

        public FileInfo(int libraryId, int id, string extension)
        {
            LibraryId = libraryId;
            Id = id;
            Extension = extension;
        }
    }

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
                string query = string.Format("SELECT [DATA] FROM [EBA].[dbo].[EFSFILEDATAPARTS] " +
                                          "WHERE [LIBRARYID] = @LibraryId AND [FILEDATAID] = @Id " +
                                          "ORDER BY [PARTINDEX]");

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

        public void SaveToFile(FileInfo fileInfo, string outputPath)
        {
            byte[] fileData = GetFileData(fileInfo.LibraryId, fileInfo.Id);
            if (fileData != null && fileData.Length > 0)
            {
                string fileName = string.Format("file_{0}_{1}{2}", 
                    fileInfo.LibraryId, fileInfo.Id, fileInfo.Extension);
                string fullPath = Path.Combine(outputPath, fileName);
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                File.WriteAllBytes(fullPath, fileData);
            }
        }

        public void BatchExtract(string outputPath, List<FileInfo> files)
        {
            foreach (var file in files)
            {
                try
                {
                    SaveToFile(file, outputPath);
                }
                catch (Exception ex)
                {
                    string errorMessage = string.Format("Dosya çıkarma hatası (LibraryId={0}, Id={1}): {2}", 
                        file.LibraryId, file.Id, ex.Message);
                    Console.WriteLine(errorMessage);
                }
            }
        }
    }
} 