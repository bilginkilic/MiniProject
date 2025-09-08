<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfViewer.aspx.cs" Inherits="AspxExamples.PdfViewer" %>

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
        .pdf-container {
            flex: 1;
            min-height: 500px;
            border: 2px solid #eee;
            border-radius: 8px;
            overflow: hidden;
        }
        .pdf-viewer {
            width: 100%;
            height: 100%;
            border: none;
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

            <div class="pdf-container">
                <iframe id="pdfViewer" runat="server" class="pdf-viewer"></iframe>
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
        function showLoading(message) {
            document.getElementById('loadingOverlay').style.display = 'flex';
            document.getElementById('loadingMessage').textContent = message || 'Yükleniyor...';
        }

        function hideLoading() {
            document.getElementById('loadingOverlay').style.display = 'none';
        }

        function showNotification(message, type) {
            var notification = document.getElementById('notification');
            var notificationMessage = document.getElementById('notificationMessage');
            
            notification.className = 'notification ' + (type || 'info');
            notificationMessage.textContent = message;
            notification.classList.add('show');
            
            setTimeout(function() {
                notification.classList.remove('show');
            }, type === 'error' ? 5000 : 3000);
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
