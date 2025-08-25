<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FileUploadViewer.aspx.cs" Inherits="AspxExamples.FileUploadViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="PDF Dosya Yükleme ve Görüntüleme">
    <title>PDF Dosya Yükleme ve Görüntüleme</title>
    
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    
    <style type="text/css">
        /* ... Önceki stiller aynen kalacak ... */

        .upload-panel {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .upload-container {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        .file-upload-wrapper {
            width: 100%;
        }

        .file-upload-wrapper input[type="file"] {
            width: 100%;
            padding: 5px;
        }

        /* Diğer stiller aynen kalacak */
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
            <!-- ... Header kısmı aynen kalacak ... -->

            <div class="sidebar">
                <div class="instructions">
                    <h3>Nasıl Kullanılır?</h3>
                    <ul>
                        <li>PDF dosyası seçmek için "Dosya Seç" butonunu kullanın</li>
                        <li>Seçilen dosyayı yüklemek için "Yükle" butonuna tıklayın</li>
                        <li>Yüklenen dosyayı görüntüleyebilirsiniz</li>
                        <li>İşlemi tamamlamak için "Kaydet ve Kapat" butonuna tıklayın</li>
                    </ul>
                </div>

                <div class="upload-panel">
                    <div class="upload-container">
                        <div class="file-upload-wrapper">
                            <asp:FileUpload ID="fuPdfUpload" runat="server" accept=".pdf" />
                        </div>
                        <asp:Button ID="btnUpload" runat="server" 
                            Text="Yükle" 
                            CssClass="button primary" 
                            OnClick="BtnUpload_Click"
                            OnClientClick="return validateFileUpload();" />
                    </div>
                </div>

                <div class="file-list" id="fileList">
                    <!-- Dosya listesi buraya dinamik olarak eklenecek -->
                </div>
            </div>

            <!-- ... Diğer içerik aynen kalacak ... -->
        </div>

        <!-- ... Loading ve Notification divleri aynen kalacak ... -->

        <script type="text/javascript">
            let currentFile = null;
            const fileList = [];

            // ... Diğer fonksiyonlar aynen kalacak ...

            function updateFileList() {
                const fileListElement = document.getElementById('fileList');
                fileListElement.innerHTML = '';

                // Duplicate kontrolü için Set kullan
                const uniqueFiles = new Set(fileList);

                Array.from(uniqueFiles).forEach((file, index) => {
                    const fileItem = document.createElement('div');
                    fileItem.className = 'file-item' + (file === currentFile ? ' active' : '');
                    
                    fileItem.innerHTML = `
                        <span class="file-name">
                            <i class="fas fa-file-pdf"></i>
                            ${file.split('/').pop()}
                        </span>
                        <div class="file-actions">
                            <button type="button" class="button secondary" onclick="viewFile('${file}')" title="Görüntüle">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="button danger" onclick="deleteFile('${file}', ${index})" title="Sil">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    `;
                    
                    fileListElement.appendChild(fileItem);
                });

                document.getElementById('<%= hdnFileList.ClientID %>').value = JSON.stringify(Array.from(uniqueFiles));
            }

            // ... Diğer fonksiyonlar aynen kalacak ...

            // Sayfa yüklendiğinde
            window.addEventListener('load', function() {
                document.getElementById('<%= btnSave.ClientID %>').onclick = saveAndReturn;

                const urlParams = new URLSearchParams(window.location.search);
                const filePath = urlParams.get('file');
                
                if (filePath) {
                    const decodedPath = decodeURIComponent(filePath);
                    if (!fileList.includes(decodedPath)) {
                        fileList.push(decodedPath);
                    }
                    viewFile(decodedPath);
                }
                
                const savedList = document.getElementById('<%= hdnFileList.ClientID %>').value;
                if (savedList) {
                    try {
                        const parsed = JSON.parse(savedList);
                        // Duplicate kontrolü yaparak ekle
                        parsed.forEach(file => {
                            if (!fileList.includes(file)) {
                                fileList.push(file);
                            }
                        });
                        updateFileList();
                    } catch (e) {
                        console.error('File list parse error:', e);
                    }
                }
            });
        </script>
    </form>
</body>
</html>