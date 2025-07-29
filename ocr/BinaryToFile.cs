using System;
using System.IO;
using System.Data.SqlClient;
using System.Text;

namespace OcrTools
{
    public class BinaryToFile
    {
        public static void ConvertBinaryToFile(string connectionString, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                
                // Önce dosya metadatalarını alalım
                string metadataQuery = @"SELECT [LIBRARYID], [ID], [CONTENTTYPE], [PARTCOUNT], 
                                              [FILESIZE], [HASH], [EXTENSION], [MIMETYPE]
                                       FROM [EBA].[dbo].[EFSFILEDATA]";

                using (SqlCommand command = new SqlCommand(metadataQuery, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int libraryId = reader.GetInt32(reader.GetOrdinal("LIBRARYID"));
                        int id = reader.GetInt32(reader.GetOrdinal("ID"));
                        string extension = reader.GetString(reader.GetOrdinal("EXTENSION"));
                        int partCount = reader.GetInt32(reader.GetOrdinal("PARTCOUNT"));
                        
                        Console.WriteLine($"Dosya işleniyor: LibraryID={libraryId}, ID={id}, Extension={extension}");
                        
                        // Her dosya için binary parçaları birleştirip kaydedelim
                        SaveFileWithParts(connection, libraryId, id, extension, partCount, outputDirectory);
                    }
                }
            }
        }

        private static void SaveFileWithParts(SqlConnection connection, int libraryId, int id, 
                                            string extension, int partCount, string outputDirectory)
        {
            string outputPath = Path.Combine(outputDirectory, $"file_{libraryId}_{id}{extension}");
            
            using (FileStream fs = new FileStream(outputPath, FileMode.Create))
            {
                // Dosya parçalarını sırayla alalım
                string partsQuery = @"SELECT [DATA] 
                                    FROM [EBA].[dbo].[EFSFILEDATAPARTS]
                                    WHERE [LIBRARYID] = @LibraryId AND [FILEDATAID] = @Id
                                    ORDER BY [PARTINDEX]";

                using (SqlCommand command = new SqlCommand(partsQuery, connection))
                {
                    command.Parameters.AddWithValue("@LibraryId", libraryId);
                    command.Parameters.AddWithValue("@Id", id);
                    
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            byte[] fileData = (byte[])reader["DATA"];
                            fs.Write(fileData, 0, fileData.Length);
                        }
                    }
                }
            }
            
            Console.WriteLine($"Dosya kaydedildi: {outputPath}");
        }

        static void Main(string[] args)
        {
            string connectionString = "Server=TRLVDB1012\\Turkish;Database=EBA;Integrated Security=True;";
            string outputDirectory = "output_files";
            
            try
            {
                ConvertBinaryToFile(connectionString, outputDirectory);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
            }
        }
    }
} 