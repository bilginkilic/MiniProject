<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FileUploadViewer.aspx.cs" Inherits="AspxExamples.FileUploadViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <!-- ... Head içeriği aynen kalacak ... -->
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

            <div class="main-content">
                <div class="pdf-viewer" id="pdfViewer">
                    <div class="empty-state">
                        <i class="fas fa-file-pdf"></i>
                        <p>Görüntülenecek PDF dosyası seçilmedi</p>
                    </div>
                </div>
            </div>

            <div class="footer">
                <asp:Button ID="btnCancel" runat="server" 
                    Text="İptal" 
                    CssClass="button cancel" 
                    OnClientClick="window.close(); return false;" />
                <asp:Button ID="btnSaveAndClose" runat="server" 
                    Text="Kaydet ve Kapat" 
                    CssClass="button save" 
                    Enabled="false" />
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

        <!-- Hidden Fields -->
        <asp:HiddenField ID="hdnSelectedFile" runat="server" />
        <asp:HiddenField ID="hdnFileList" runat="server" />
        <asp:HiddenField ID="hdnIsReturnRequested" runat="server" Value="false" />

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

            function viewFile(filePath) {
                showLoading();
                currentFile = filePath;
                
                const viewer = document.getElementById('pdfViewer');
                viewer.innerHTML = `<iframe src="${filePath}" onload="hideLoading()"></iframe>`;
                
                document.getElementById('<%= hdnSelectedFile.ClientID %>').value = filePath;
                updateFileList();
            }

            function enableSaveButton() {
                document.getElementById('<%= btnSaveAndClose.ClientID %>').disabled = false;
            }

            // Sayfa yüklendiğinde
            window.addEventListener('load', function() {
                document.getElementById('<%= btnSaveAndClose.ClientID %>').onclick = saveAndReturn;

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

            // ... Diğer fonksiyonlar aynen kalacak ...
        </script>
    </form>
</body>
</html>