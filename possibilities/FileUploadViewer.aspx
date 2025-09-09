<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FileUploadViewer.aspx.cs" Inherits="AspxExamples.FileUploadViewer" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="PDF Dosya Yükleme ve Görüntüleme">
    <meta http-equiv="Content-Security-Policy" content="default-src 'self' http://trrgap3027; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:;">
    <title>PDF Dosya Yükleme ve Görüntüleme v1</title>
    <style type="text/css">
        html, body { 
            margin: 0; 
            padding: 0;
            width: 100%;
            height: 100%;
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
            overflow: auto;
        }
        form {
            width: 100%;
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            overflow: visible;
            padding: 20px;
            box-sizing: border-box;
        }
        .container {
            flex: 1;
            display: grid;
            grid-template-rows: auto 1fr auto;
            grid-template-areas: 
                "header"
                "main"
                "footer";
            box-sizing: border-box;
            min-width: 800px;
            max-width: 1200px;
            margin: 0 auto;
            width: 100%;
            background-color: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            border-radius: 8px;
            min-height: 100%;
            height: auto;
            position: relative;
            gap: 20px;
            padding: 20px;
            overflow: visible;
        }
        .header {
            grid-area: header;
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 15px 0;
            margin-bottom: 0;
            border-bottom: 2px solid #eee;
        }
        .header h2 {
            margin: 0;
            color: #333;
            font-size: 24px;
        }
        .main-content {
            grid-area: main;
            display: flex;
            flex-direction: column;
            min-width: 0;
            padding: 20px 0;
        }
        .upload-panel {
            background: #f8f9fa;
            border-radius: 8px;
            padding: 20px;
            margin-bottom: 20px;
            border: 1px solid #dee2e6;
            display: flex;
            gap: 10px;
            align-items: center;
        }
        .button {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.2s ease;
            background: #dc3545;
            color: white;
        }
        .button:hover {
            background: #c82333;
        }
        .button.save {
            background: #28a745;
        }
        .button.save:hover {
            background: #218838;
        }
        .image-container {
            flex: 1;
            min-height: 0;
            display: flex;
            flex-direction: column;
            border: 2px solid #eee;
            border-radius: 8px;
            background: #fff;
            overflow: hidden;
        }
        .page-contents {
            flex: 1;
            min-height: 0;
            position: relative;
            overflow: hidden;
            background: #f5f5f5;
            height: 100%;
            width: 100%;
            display: flex;
            flex-direction: column;
        }
        .page-contents iframe {
            width: 100%;
            height: 100%;
            border: none;
        }
        .footer {
            grid-area: footer;
            display: flex;
            justify-content: flex-end;
            padding: 15px 0;
            border-top: 2px solid #eee;
            gap: 10px;
        }
        /* Loading Overlay */
        .loading-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(220, 53, 69, 0.1);
            z-index: 9999;
            justify-content: center;
            align-items: center;
            backdrop-filter: blur(3px);
        }
        .loading-content {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 20px;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(220, 53, 69, 0.15);
            border: 1px solid rgba(220, 53, 69, 0.1);
        }
        .loading-spinner {
            width: 50px;
            height: 50px;
            border: 5px solid #f3f3f3;
            border-top: 5px solid #dc3545;
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }
        .loading-message {
            color: #333;
            font-size: 16px;
            text-align: center;
            max-width: 300px;
        }
        /* Notification */
        .notification {
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 25px;
            border-radius: 6px;
            background: #fff;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 9999;
            display: none;
            align-items: center;
            gap: 10px;
            transform: translateX(120%);
            transition: transform 0.3s ease;
            max-width: 400px;
            min-width: 300px;
        }
        .notification.show {
            transform: translateX(0);
        }
        .notification.success {
            border-left: 4px solid #28a745;
            background-color: #f0fff4;
        }
        .notification.error {
            border-left: 4px solid #dc3545;
            background-color: #fff5f5;
        }
        .notification.warning {
            border-left: 4px solid #ffc107;
            background-color: #fffbeb;
        }
        .notification .close-btn {
            position: absolute;
            top: 5px;
            right: 5px;
            cursor: pointer;
            padding: 5px;
            font-size: 18px;
            color: #666;
            background: none;
            border: none;
            opacity: 0.7;
        }
        .notification .close-btn:hover {
            opacity: 1;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server" enctype="multipart/form-data">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
            <div class="header">
                <h2>PDF Dosya Yükleme ve Görüntüleme</h2>
            </div>

            <div class="main-content">
                <div id="imageContainer" runat="server" class="image-container">
                    <div class="upload-panel">
                        <asp:FileUpload ID="fuPdfUpload" runat="server" accept=".pdf" />
                        <asp:Button ID="btnUpload" runat="server" Text="PDF Dosyasını Yükle ve Göster" 
                            CssClass="button" OnClick="BtnUpload_Click" />
                    </div>
                    <div id="pageContents" class="page-contents">
                        <!-- PDF içeriği buraya gelecek -->
                    </div>
                </div>
            </div>

            <div class="footer">
                <asp:HiddenField ID="hdnSelectedFile" runat="server" />
                <asp:HiddenField ID="hdnIsReturnRequested" runat="server" Value="false" />
                <asp:Button ID="btnSaveAndClose" runat="server" Text="Kaydet ve Kapat" 
                    CssClass="button save" OnClientClick="return saveAndReturn();" />
            </div>
        </div>
        
        <!-- Loading Overlay -->
        <div id="loadingOverlay" class="loading-overlay">
            <div class="loading-content">
                <div class="loading-spinner"></div>
                <div id="loadingMessage" class="loading-message">İşlem yapılıyor...</div>
            </div>
        </div>
        
        <!-- Notification -->
        <div id="notification" class="notification">
            <button type="button" class="close-btn" onclick="hideNotification()">&times;</button>
            <span id="notificationMessage"></span>
        </div>

        <script type="text/javascript">
            function showLoading(message) {
                document.getElementById('loadingOverlay').style.display = 'flex';
                document.getElementById('loadingMessage').textContent = message || 'İşlem yapılıyor...';
            }

            function hideLoading() {
                document.getElementById('loadingOverlay').style.display = 'none';
            }

            function showNotification(message, type) {
                const notification = document.getElementById('notification');
                const messageElement = document.getElementById('notificationMessage');
                
                notification.className = 'notification ' + (type || 'info');
                messageElement.textContent = message;
                notification.classList.add('show');
                
                setTimeout(() => {
                    notification.classList.remove('show');
                }, type === 'error' ? 8000 : 3000);
            }

            function hideNotification() {
                document.getElementById('notification').classList.remove('show');
            }

            function saveAndReturn() {
                try {
                    const selectedFile = document.getElementById('<%= hdnSelectedFile.ClientID %>').value;
                    if (!selectedFile) {
                        showNotification('Lütfen bir dosya seçin', 'warning');
                        return false;
                    }

                    showLoading('Dosya kaydediliyor...');
                    document.getElementById('<%= hdnIsReturnRequested.ClientID %>').value = 'true';
                    
                    PageMethods.SaveAndReturn(selectedFile, function(response) {
                        if (response.success) {
                            PageMethods.SetSelectedFile(selectedFile, function(response) {
                                if (response.success) {
                                    showNotification('Dosya başarıyla kaydedildi', 'success');
                                    setTimeout(() => {
                                        if (window.parent && window.parent !== window) {
                                            window.parent.postMessage({ type: 'FILE_SELECTED', success: true }, '*');
                                        }
                                        window.close();
                                    }, 1500);
                                } else {
                                    showNotification('Dosya yolu kaydedilemedi', 'error');
                                }
                                hideLoading();
                            });
                        } else {
                            showNotification(response.error || 'Kaydetme işlemi başarısız oldu', 'error');
                            hideLoading();
                        }
                    });

                    return false;
                } catch (error) {
                    console.error('saveAndReturn hatası:', error);
                    showNotification('İşlem sırasında bir hata oluştu', 'error');
                    hideLoading();
                    return false;
                }
            }
        </script>
    </form>
</body>
</html>