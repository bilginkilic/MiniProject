<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfViewer.aspx.cs" Inherits="AspxExamples.PdfViewer" %>
<%-- Created: 2024.01.17 14:30 - v1 - PDF Görüntüleyici --%>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="PDF Görüntüleyici">
    <title>PDF Görüntüleyici</title>
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
            display: flex;
            flex-direction: column;
            gap: 20px;
            max-width: 1200px;
            margin: 0 auto;
            width: 100%;
            background-color: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            border-radius: 8px;
            padding: 20px;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 15px 0;
            border-bottom: 2px solid #eee;
        }
        .header h2 {
            margin: 0;
            color: #333;
            font-size: 24px;
        }
        .upload-panel {
            display: flex;
            gap: 15px;
            align-items: center;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 6px;
            flex-wrap: wrap;
        }
        .button {
            padding: 10px 20px;
            cursor: pointer;
            border: none;
            background-color: #dc3545;
            color: white;
            border-radius: 6px;
            transition: all 0.3s ease;
            font-size: 14px;
            font-weight: 500;
            display: flex;
            align-items: center;
            gap: 8px;
        }
        .button:hover {
            background-color: #c82333;
            transform: translateY(-1px);
        }
        .button:disabled {
            background-color: #ccc;
            cursor: not-allowed;
            transform: none;
        }
        .pdf-preview-container {
            margin-top: 20px;
            border: 2px solid #eee;
            border-radius: 8px;
            overflow: hidden;
            background: white;
        }
        .pdf-toolbar {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 10px 15px;
            background: #f8f9fa;
            border-bottom: 1px solid #eee;
        }
        .pdf-toolbar h3 {
            margin: 0;
            color: #333;
            font-size: 18px;
        }
        .button-group {
            display: flex;
            gap: 10px;
        }
        .button.secondary {
            background-color: #6c757d;
        }
        .button.secondary:hover {
            background-color: #5a6268;
        }
        .pdf-viewer {
            height: 600px;
            overflow: hidden;
        }
        .pdf-frame {
            width: 100%;
            height: 600px;
            border: none;
            background: white;
        }
        /* Loading Animation */
        .loading-overlay {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.5);
            z-index: 9999;
            justify-content: center;
            align-items: center;
        }
        .loading-content {
            background: white;
            padding: 20px;
            border-radius: 8px;
            text-align: center;
        }
        .loading-spinner {
            width: 40px;
            height: 40px;
            border: 4px solid #f3f3f3;
            border-top: 4px solid #dc3545;
            border-radius: 50%;
            animation: spin 1s linear infinite;
            margin: 0 auto 10px;
        }
        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
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
            max-width: 400px;
            min-width: 300px;
        }
        .notification.success {
            border-left: 4px solid #28a745;
            background-color: #f0fff4;
        }
        .notification.error {
            border-left: 4px solid #dc3545;
            background-color: #fff5f5;
        }
        .notification.show {
            display: flex;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
            <div class="header">
                <h2>PDF Görüntüleyici</h2>
            </div>

            <div class="upload-panel">
                <asp:FileUpload ID="fuPdf" runat="server" accept=".pdf" />
                <asp:Button ID="btnUpload" runat="server" Text="PDF Yükle" 
                    CssClass="button" OnClick="BtnUpload_Click" />
            </div>

            <div class="pdf-preview-container">
                <div class="pdf-toolbar">
                    <h3>PDF Önizleme</h3>
                    <div class="button-group">
                        <asp:Button ID="btnSave" runat="server" Text="Kaydet" 
                            CssClass="button" OnClick="BtnSave_Click" Visible="false" />
                        <asp:Button ID="btnCancel" runat="server" Text="İptal" 
                            CssClass="button secondary" OnClick="BtnCancel_Click" Visible="false" />
                    </div>
                </div>
                <div class="pdf-viewer">
                    <embed id="pdfViewer" runat="server" class="pdf-frame" 
                        type="application/pdf" />
                </div>
            </div>
        </div>
        
        <!-- Loading Overlay -->
        <div id="loadingOverlay" class="loading-overlay">
            <div class="loading-content">
                <div class="loading-spinner"></div>
                <div id="loadingMessage">Yükleniyor...</div>
            </div>
        </div>
        
        <!-- Notification -->
        <div id="notification" class="notification">
            <span id="notificationMessage"></span>
        </div>
    </form>

    <script type="text/javascript">
        var notificationTimeout;

        function showLoading(message) {
            document.getElementById('loadingOverlay').style.display = 'flex';
            document.getElementById('loadingMessage').textContent = message || 'Yükleniyor...';
        }

        function hideLoading() {
            document.getElementById('loadingOverlay').style.display = 'none';
        }

        window.showNotification = function(message, type) {
            clearTimeout(notificationTimeout);
            
            var notification = document.getElementById('notification');
            var notificationMessage = document.getElementById('notificationMessage');
            
            if (!notification || !notificationMessage) {
                console.error('Notification elements not found');
                return;
            }
            
            // Reset classes
            notification.className = 'notification';
            notification.classList.add(type || 'info');
            notificationMessage.textContent = message || '';
            
            // Show notification
            notification.classList.add('show');
            
            // Auto hide after delay
            notificationTimeout = setTimeout(function() {
                notification.classList.remove('show');
            }, type === 'error' ? 8000 : 5000);
        }

        function hideNotification() {
            var notification = document.getElementById('notification');
            if (notification) {
                notification.classList.remove('show');
            }
            clearTimeout(notificationTimeout);
        }

        // Dosya seçildiğinde yükleme butonunu aktifleştir
        document.getElementById('<%= fuPdf.ClientID %>').addEventListener('change', function() {
            document.getElementById('<%= btnUpload.ClientID %>').disabled = !this.files.length;
        });

        // Sayfa yüklendiğinde
        window.addEventListener('load', function() {
            // Yükleme butonu başlangıçta disabled
            document.getElementById('<%= btnUpload.ClientID %>').disabled = true;
        });
    </script>
</body>
</html>
