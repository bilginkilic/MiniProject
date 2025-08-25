<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FileUploadViewer.aspx.cs" Inherits="AspxExamples.FileUploadViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="PDF Dosya Yükleme ve Görüntüleme">
    <meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:;">
    <title>PDF Dosya Yükleme ve Görüntüleme</title>
    <style type="text/css">
        html, body { 
            margin: 0; 
            padding: 0;
            width: 100%;
            height: 100%;
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
        }
        
        form {
            width: 100%;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            padding: 20px;
            box-sizing: border-box;
        }

        .container {
            flex: 1;
            display: grid;
            grid-template-columns: 300px 1fr;
            grid-template-rows: auto 1fr;
            grid-template-areas: 
                "header header"
                "sidebar main";
            gap: 20px;
            max-width: 1600px;
            margin: 0 auto;
            width: 100%;
            background-color: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            border-radius: 8px;
            padding: 20px;
            box-sizing: border-box;
        }

        .header {
            grid-area: header;
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding-bottom: 20px;
            border-bottom: 2px solid #eee;
        }

        .header h1 {
            margin: 0;
            color: #333;
            font-size: 24px;
        }

        .sidebar {
            grid-area: sidebar;
            background: #fff;
            border-right: 2px solid #eee;
            padding-right: 20px;
        }

        .main-content {
            grid-area: main;
            display: flex;
            flex-direction: column;
            min-height: 0;
        }

        .upload-panel {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
        }

        .file-list {
            margin-top: 20px;
        }

        .file-item {
            display: flex;
            align-items: center;
            padding: 10px;
            background: white;
            border: 1px solid #ddd;
            border-radius: 4px;
            margin-bottom: 10px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .file-item:hover {
            background: #f0f0f0;
        }

        .file-item.active {
            background: #e3f2fd;
            border-color: #2196f3;
        }

        .file-name {
            flex: 1;
            margin-right: 10px;
            overflow: hidden;
            text-overflow: ellipsis;
            white-space: nowrap;
        }

        .file-actions {
            display: flex;
            gap: 5px;
        }

        .button {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.2s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .button.primary {
            background: #2196f3;
            color: white;
        }

        .button.primary:hover {
            background: #1976d2;
        }

        .button.secondary {
            background: #6c757d;
            color: white;
        }

        .button.secondary:hover {
            background: #5a6268;
        }

        .button.danger {
            background: #dc3545;
            color: white;
        }

        .button.danger:hover {
            background: #c82333;
        }

        .button:disabled {
            background: #ccc;
            cursor: not-allowed;
        }

        .pdf-viewer {
            flex: 1;
            min-height: 0;
            border: 2px solid #eee;
            border-radius: 8px;
            overflow: hidden;
            position: relative;
            background: #f8f9fa;
        }

        .pdf-viewer iframe {
            width: 100%;
            height: 100%;
            border: none;
        }

        .loading-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(255, 255, 255, 0.8);
            justify-content: center;
            align-items: center;
            z-index: 1000;
        }

        .loading-spinner {
            width: 50px;
            height: 50px;
            border: 5px solid #f3f3f3;
            border-top: 5px solid #2196f3;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .notification {
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 25px;
            border-radius: 4px;
            background: white;
            box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            display: none;
            z-index: 1000;
        }

        .notification.success {
            border-left: 4px solid #28a745;
        }

        .notification.error {
            border-left: 4px solid #dc3545;
        }

        .notification.warning {
            border-left: 4px solid #ffc107;
        }

        .empty-state {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            height: 100%;
            color: #666;
            text-align: center;
            padding: 20px;
        }

        .empty-state i {
            font-size: 48px;
            margin-bottom: 10px;
            color: #ccc;
        }

        .instructions {
            background: #fff;
            padding: 15px;
            border-radius: 4px;
            margin-bottom: 20px;
            border: 1px solid #ddd;
        }

        .instructions h3 {
            margin: 0 0 10px 0;
            color: #333;
        }

        .instructions ul {
            margin: 0;
            padding-left: 20px;
        }

        .instructions li {
            margin-bottom: 5px;
            color: #666;
        }

    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
            <div class="header">
                <h1>PDF Dosya Yükleme ve Görüntüleme</h1>
            </div>

            <div class="sidebar">
                <div class="instructions">
                    <h3>Nasıl Kullanılır?</h3>
                    <ul>
                        <li>PDF dosyası seçmek için "Dosya Seç" butonunu kullanın</li>
                        <li>Seçilen dosyayı yüklemek için "Yükle" butonuna tıklayın</li>
                        <li>Yüklenen dosyaları listeden seçerek görüntüleyebilirsiniz</li>
                        <li>Dosyaları silmek için çöp kutusu ikonuna tıklayın</li>
                    </ul>
                </div>

                <div class="upload-panel">
                    <asp:FileUpload ID="fuPdfUpload" runat="server" accept=".pdf" />
                    <asp:Button ID="btnUpload" runat="server" Text="Yükle" CssClass="button primary" OnClick="BtnUpload_Click" />
                </div>

                <div class="file-list" id="fileList">
                    <!-- Dosya listesi buraya dinamik olarak eklenecek -->
                </div>
            </div>

            <div class="main-content">
                <div class="pdf-viewer" id="pdfViewer">
                    <div class="empty-state">
                        <i class="fas fa-file-pdf"></i>
                        <p>Görüntülenecek PDF dosyası seçilmedi</p>
                    </div>
                </div>
            </div>
        </div>

        <!-- Loading Overlay -->
        <div id="loadingOverlay" class="loading-overlay">
            <div class="loading-spinner"></div>
        </div>

        <!-- Notification -->
        <div id="notification" class="notification">
            <span id="notificationMessage"></span>
        </div>

        <asp:HiddenField ID="hdnCurrentFile" runat="server" />
        <asp:HiddenField ID="hdnFileList" runat="server" />

        <script type="text/javascript">
            let currentFile = null;
            const fileList = [];

            function showLoading() {
                document.getElementById('loadingOverlay').style.display = 'flex';
            }

            function hideLoading() {
                document.getElementById('loadingOverlay').style.display = 'none';
            }

            function showNotification(message, type) {
                const notification = document.getElementById('notification');
                const messageElement = document.getElementById('notificationMessage');
                
                notification.className = 'notification ' + (type || 'info');
                messageElement.textContent = message;
                notification.style.display = 'block';
                
                setTimeout(() => {
                    notification.style.display = 'none';
                }, 3000);
            }

            function updateFileList() {
                const fileListElement = document.getElementById('fileList');
                fileListElement.innerHTML = '';

                fileList.forEach((file, index) => {
                    const fileItem = document.createElement('div');
                    fileItem.className = 'file-item' + (file === currentFile ? ' active' : '');
                    
                    fileItem.innerHTML = `
                        <span class="file-name">${file.split('/').pop()}</span>
                        <div class="file-actions">
                            <button type="button" class="button secondary" onclick="viewFile('${file}')">
                                <i class="fas fa-eye"></i>
                            </button>
                            <button type="button" class="button danger" onclick="deleteFile('${file}', ${index})">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    `;
                    
                    fileListElement.appendChild(fileItem);
                });

                // Hidden field güncelle
                document.getElementById('<%= hdnFileList.ClientID %>').value = JSON.stringify(fileList);
            }

            function viewFile(filePath) {
                showLoading();
                currentFile = filePath;
                
                const viewer = document.getElementById('pdfViewer');
                viewer.innerHTML = `<iframe src="${filePath}" onload="hideLoading()"></iframe>`;
                
                document.getElementById('<%= hdnCurrentFile.ClientID %>').value = filePath;
                updateFileList();
            }

            function deleteFile(filePath, index) {
                if (confirm('Bu dosyayı silmek istediğinizden emin misiniz?')) {
                    showLoading();
                    
                    // AJAX ile silme işlemi
                    fetch('FileUploadViewer.aspx/DeleteFile', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify({ filePath: filePath })
                    })
                    .then(response => response.json())
                    .then(data => {
                        if (data.d.success) {
                            fileList.splice(index, 1);
                            if (currentFile === filePath) {
                                currentFile = null;
                                document.getElementById('pdfViewer').innerHTML = `
                                    <div class="empty-state">
                                        <i class="fas fa-file-pdf"></i>
                                        <p>Görüntülenecek PDF dosyası seçilmedi</p>
                                    </div>
                                `;
                            }
                            updateFileList();
                            showNotification('Dosya başarıyla silindi', 'success');
                        } else {
                            showNotification(data.d.message || 'Dosya silinirken bir hata oluştu', 'error');
                        }
                    })
                    .catch(error => {
                        console.error('Delete error:', error);
                        showNotification('Dosya silinirken bir hata oluştu', 'error');
                    })
                    .finally(() => {
                        hideLoading();
                    });
                }
            }

            // Sayfa yüklendiğinde
            window.addEventListener('load', function() {
                // URL'den dosya yolu parametresini kontrol et
                const urlParams = new URLSearchParams(window.location.search);
                const filePath = urlParams.get('file');
                
                if (filePath) {
                    viewFile(decodeURIComponent(filePath));
                }
                
                // Kaydedilmiş dosya listesini yükle
                const savedList = document.getElementById('<%= hdnFileList.ClientID %>').value;
                if (savedList) {
                    try {
                        const parsed = JSON.parse(savedList);
                        fileList.push(...parsed);
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
