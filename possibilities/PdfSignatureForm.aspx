<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfSignatureForm.aspx.cs" Inherits="AspxExamples.PdfSignatureForm" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>İmza Sirkülerinden İmza Seçimi</title>
    <style type="text/css">
        html, body { 
            margin: 0; 
            padding: 0;
            width: 100%;
            height: 100%;
            overflow: hidden;
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
        }
        form {
            width: 100%;
            height: 100%;
            display: flex;
            flex-direction: column;
            overflow: hidden;
        }
        .container {
            flex: 1;
            display: flex;
            flex-direction: column;
            padding: 20px;
            box-sizing: border-box;
            max-width: 1200px;
            margin: 20px auto;
            width: 100%;
            background-color: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            border-radius: 8px;
            overflow-x: hidden;
            overflow-y: auto;
            max-height: calc(100vh - 40px); /* Ekranın yüksekliğinden margin'leri çıkar */
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 15px 0;
            margin-bottom: 20px;
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
            margin-bottom: 20px;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 6px;
            flex-wrap: wrap;
        }
        .upload-panel .instructions {
            flex: 1 1 100%;
            color: #666;
            font-size: 14px;
            margin-bottom: 10px;
        }
        .image-container {
            position: relative;
            flex: 1;
            min-height: 0;
            display: flex;
            flex-direction: column;
            border: 2px solid #eee;
            border-radius: 8px;
            background: #fff;
            margin: 10px 0;
            overflow: hidden;
            height: calc(100vh - 450px); /* Seçilen imzalar kısmı için yer bırak */
            min-height: 400px; /* Minimum yükseklik */
        }
        #pageContents {
            flex: 1;
            min-height: 0;
            position: relative;
            overflow: hidden;
            background: #f5f5f5;
        }
        .tabs {
            display: flex;
            padding: 10px 10px 0 10px;
            background: #f8f9fa;
            border-bottom: 2px solid #eee;
            gap: 5px;
        }
        .tab {
            padding: 8px 16px;
            background: #fff;
            border: 1px solid #ddd;
            border-bottom: none;
            border-radius: 6px 6px 0 0;
            cursor: pointer;
            color: #666;
            transition: all 0.3s ease;
        }
        .tab:hover {
            background: #f8f9fa;
        }
        .tab.active {
            background: #dc3545;
            color: white;
            border-color: #dc3545;
        }
        .tab-content {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            display: none;
            overflow: hidden;
        }
        .tab-content.active {
            display: block;
        }
        .image-wrapper {
            width: 100%;
            height: 100%;
            overflow-y: auto;
            overflow-x: hidden;
            padding: 30px;
            box-sizing: border-box;
            display: flex;
            justify-content: center;
            align-items: flex-start;
            position: relative;
            background: #f8f9fa;
        }
        .image-wrapper img {
            width: 90%; /* Sayfanın genişliğini artırdık */
            height: auto;
            display: block;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            background: white;
            margin: 0 auto;
            margin-bottom: 30px;
            border-radius: 4px;
        }
        .button {
            padding: 10px 20px;
            margin-right: 10px;
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
        .button:not(:disabled) {
            background-color: #dc3545;
        }
        .button.secondary {
            background-color: #6c757d;
        }
        .button.secondary:hover {
            background-color: #5a6268;
        }
        
        /* Loading Animation Styles */
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
        
        /* Notification Styles */
        .notification {
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 25px;
            border-radius: 6px;
            background: #fff;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            z-index: 9999;
            display: flex;
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
        .notification.info {
            border-left: 4px solid #17a2b8;
            background-color: #f0f9ff;
        }
        .notification.persistent {
            animation: none;
            transform: translateX(0);
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
        .footer {
            padding: 15px 0;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-top: 2px solid #eee;
            margin-top: 20px;
            background: white;
            position: relative;
            z-index: 1;
        }
        .message {
            padding: 12px 15px;
            margin: 10px 0;
            border-radius: 6px;
            font-size: 14px;
            display: flex;
            align-items: center;
            gap: 10px;
        }
        .message.error {
            background-color: #fff3f3;
            border: 1px solid #ffcdd2;
            color: #d32f2f;
        }
        .message.success {
            background-color: #f1f8e9;
            border: 1px solid #c5e1a5;
            color: #33691e;
        }
        .help-text {
            color: #666;
            font-size: 13px;
            margin-top: 5px;
        }
        /* Selection box style */
        .selection {
            position: fixed;
            border: 2px solid #dc3545;
            background-color: rgba(220,53,69,0.1);
            pointer-events: none;
            display: none;
            z-index: 1000;
            border-radius: 4px;
        }

        /* PDF List Panel Styles */
        .pdf-list-panel {
            margin-bottom: 20px;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 6px;
        }

        .pdf-list-panel h3 {
            margin: 0 0 15px 0;
            color: #333;
            font-size: 18px;
        }

        .pdf-list {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
        }

        .pdf-item {
            display: flex;
            align-items: center;
            padding: 8px 12px;
            background: white;
            border: 1px solid #ddd;
            border-radius: 4px;
            cursor: pointer;
            transition: all 0.2s ease;
        }

        .pdf-item:hover {
            background: #f0f0f0;
        }

        .pdf-item.active {
            background: #e3f2fd;
            border-color: #2196f3;
        }

        .pdf-item-name {
            margin-right: 10px;
        }

        .pdf-item-remove {
            color: #dc3545;
            cursor: pointer;
            padding: 2px 6px;
            border-radius: 3px;
            font-size: 12px;
            margin-left: 8px;
        }

        .pdf-item-remove:hover {
            background: #dc3545;
            color: white;
        }

        /* Selected Signatures Styles */
        .selected-signatures {
            margin: 10px 0;
            padding: 15px;
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            border: 1px solid #eee;
            position: sticky;
            bottom: 0;
            background: white;
            z-index: 10;
        }

        .selected-signatures h3 {
            margin: 0 0 15px 0;
            color: #333;
            font-size: 18px;
        }

        .signature-slots {
            display: flex;
            gap: 20px;
            justify-content: center;
        }

        .signature-slot {
            width: 200px;
            height: 100px;
            border: 2px dashed #ccc;
            border-radius: 6px;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            position: relative;
            background: #f8f9fa;
            transition: all 0.3s ease;
        }

        .signature-slot.filled {
            border: 2px solid #28a745;
            background: #fff;
        }

        .slot-placeholder {
            color: #666;
            font-size: 14px;
        }

        .slot-image {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            display: none;
            background-size: contain;
            background-repeat: no-repeat;
            background-position: center;
        }

        .delete-signature {
            position: absolute;
            top: -10px;
            right: -10px;
            background: #dc3545;
            color: white;
            border: none;
            border-radius: 50%;
            width: 24px;
            height: 24px;
            font-size: 12px;
            cursor: pointer;
            display: none;
            align-items: center;
            justify-content: center;
            transition: all 0.3s ease;
        }

        .delete-signature:hover {
            background: #c82333;
            transform: scale(1.1);
        }

        .signature-slot.filled .slot-image {
            display: block;
        }

        .signature-slot.filled .slot-placeholder {
            display: none;
        }

        .signature-slot.filled .delete-signature {
            display: flex;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
            <div class="header">
                <h2>İmza Sirkülerinden İmza Seçimi</h2>
            </div>
            
            <div class="pdf-list-panel">
                <h3>Mevcut PDF Listesi</h3>
                <div class="pdf-list" id="pdfList">
                    <!-- PDF listesi buraya dinamik olarak eklenecek -->
                </div>
                <asp:HiddenField ID="hdnCurrentPdfList" runat="server" />
            </div>

            <div class="upload-panel">
                <div class="instructions">
                    <strong>Nasıl Kullanılır:</strong>
                    <ol>
                        <li>Listeden bir PDF seçin veya yeni bir PDF yükleyin</li>
                        <li>Mouse ile imza alanını seçin (en fazla 3 imza seçebilirsiniz)</li>
                        <li>Seçimleri tamamladığınızda "Seçilen İmzaları Kaydet" butonuna tıklayın</li>
                    </ol>
                </div>
                <asp:FileUpload ID="fileUpload" runat="server" />
                <asp:Button ID="btnUpload" runat="server" Text="İmza Sirkülerini Yükle ve Göster" CssClass="button" OnClick="BtnUpload_Click" />
                <asp:Button ID="btnShowPdf" runat="server" Text="" CssClass="button" style="display: none;" />
            </div>

            <div id="imageContainer" runat="server" class="image-container">
                <div class="tabs" id="pageTabs">
                    <!-- Tabs will be added here dynamically -->
                </div>
                <div id="pageContents">
                    <!-- Tab contents will be added here dynamically -->
                </div>
            </div>

            <!-- Selected Signatures Container -->
            <div class="selected-signatures">
                <h3>Seçilen İmzalar</h3>
                <div class="signature-slots">
                    <div class="signature-slot" data-slot="1">
                        <div class="slot-placeholder">İmza 1</div>
                        <div class="slot-image"></div>
                        <button type="button" class="delete-signature" style="display: none;">Sil</button>
                    </div>
                    <div class="signature-slot" data-slot="2">
                        <div class="slot-placeholder">İmza 2</div>
                        <div class="slot-image"></div>
                        <button type="button" class="delete-signature" style="display: none;">Sil</button>
                    </div>
                    <div class="signature-slot" data-slot="3">
                        <div class="slot-placeholder">İmza 3</div>
                        <div class="slot-image"></div>
                        <button type="button" class="delete-signature" style="display: none;">Sil</button>
                    </div>
                </div>
            </div>

            <div class="footer">
                <asp:HiddenField ID="hdnSelection" runat="server" />
                <asp:HiddenField ID="hdnPageCount" runat="server" />
                <asp:HiddenField ID="hdnSignatures" runat="server" />
                <asp:Button ID="btnSaveSignature" runat="server" Text="Seçilen İmzayı Kaydet" 
                    CssClass="button" OnClick="BtnSaveSignature_Click" Enabled="false" />
                
                <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
            </div>
        </div>
        
        <!-- Loading Overlay -->
        <div id="loadingOverlay" class="loading-overlay">
            <div class="loading-content">
                <div class="loading-spinner"></div>
                <div id="loadingMessage" class="loading-message">İşlem yapılıyor...</div>
            </div>
        </div>
        
        <!-- Notification Container -->
        <div id="notification" class="notification">
            <button type="button" class="close-btn" onclick="hideNotification()">&times;</button>
            <span id="notificationMessage"></span>
        </div>

        <script type="text/javascript">
            // Notification functions
            function showNotification(message, type, persistent = false) {
                console.log('Notification:', type, message);
                const notification = document.getElementById('notification');
                const notificationMessage = document.getElementById('notificationMessage');
                
                // Reset classes
                notification.className = 'notification ' + type;
                if (persistent) {
                    notification.classList.add('persistent');
                }
                notificationMessage.textContent = message;
                
                notification.classList.add('show');
                
                // For non-persistent notifications, auto-hide after delay
                if (!persistent) {
                    setTimeout(() => {
                        hideNotification();
                    }, type === 'error' ? 8000 : 5000); // Show errors longer
                }
            }

            function hideNotification() {
                const notification = document.getElementById('notification');
                notification.classList.remove('show');
                notification.classList.remove('persistent');
            }

            function getMousePosition(e, element) {
                var wrapper = element.closest('.image-wrapper');
                var image = wrapper.querySelector('img');
                var imageRect = image.getBoundingClientRect();
                var wrapperRect = wrapper.getBoundingClientRect();

                // Resmin gerçek boyutları ve görüntülenen boyutları arasındaki oranı hesapla
                var scaleX = image.naturalWidth / imageRect.width;
                var scaleY = image.naturalHeight / imageRect.height;

                // Fare pozisyonunu resmin orijinal koordinat sistemine göre hesapla
                var x = (e.clientX - imageRect.left) * scaleX;
                var y = (e.clientY - imageRect.top) * scaleY;

                return {
                    x: Math.max(0, Math.min(x, image.naturalWidth)),
                    y: Math.max(0, Math.min(y, image.naturalHeight))
                };
            }

            function showLoading(message) {
                document.getElementById('loadingOverlay').style.display = 'flex';
                document.getElementById('loadingMessage').textContent = message || 'İşlem yapılıyor...';
            }

            function hideLoading() {
                document.getElementById('loadingOverlay').style.display = 'none';
            }

            function updateLoadingMessage(message) {
                document.getElementById('loadingMessage').textContent = message;
            }

            function formatString(format) {
                var args = Array.prototype.slice.call(arguments, 1);
                return format.replace(/{(\d+)}/g, function(match, number) { 
                    return typeof args[number] != 'undefined'
                        ? args[number] 
                        : match;
                });
            }

            var isSelecting = false;
            var startX, startY;
            var selectionBox = null;
            var hiddenField = document.getElementById('<%= hdnSelection.ClientID %>');
            var hiddenSignatures = document.getElementById('<%= hdnSignatures.ClientID %>');
            var imageContainer = document.getElementById('<%= imageContainer.ClientID %>');
            var btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
            var currentSelection = null;
            var selectedSignatures = [];
            const MAX_SIGNATURES = 3;

            function showPage(pageNumber) {
                // Tüm tabları ve içerikleri gizle
                document.querySelectorAll('.tab').forEach(tab => tab.classList.remove('active'));
                document.querySelectorAll('.tab-content').forEach(content => content.classList.remove('active'));
                
                // Seçilen tabı ve içeriği göster
                document.querySelector(`.tab[data-page="${pageNumber}"]`).classList.add('active');
                document.querySelector(`.tab-content[data-page="${pageNumber}"]`).classList.add('active');
                
                // Seçimi temizle
                clearSelection();
            }

            function initializeTabs(pageCount) {
                const tabsContainer = document.getElementById('pageTabs');
                const contentsContainer = document.getElementById('pageContents');
                
                // Mevcut tabları ve içerikleri temizle
                tabsContainer.innerHTML = '';
                contentsContainer.innerHTML = '';
                
                console.log('Initializing tabs with page count:', pageCount);
                
                // Her sayfa için tab ve içerik oluştur
                for (let i = 1; i <= pageCount; i++) {
                    // Tab oluştur
                    const tab = document.createElement('div');
                    tab.className = 'tab' + (i === 1 ? ' active' : '');
                    tab.setAttribute('data-page', i);
                    tab.textContent = formatString('Sayfa {0}', i);
                    tab.onclick = () => showPage(i);
                    tabsContainer.appendChild(tab);
                    
                    // İçerik container'ı oluştur
                    const content = document.createElement('div');
                    content.className = 'tab-content' + (i === 1 ? ' active' : '');
                    content.setAttribute('data-page', i);
                    
                    // Image wrapper oluştur
                    const wrapper = document.createElement('div');
                    wrapper.className = 'image-wrapper';
                    
                    // Resmi oluştur
                    const img = document.createElement('img');
                    img.id = formatString('imgSignature_{0}', i);
                    
                    // Resim yükleme hatası kontrolü
                    img.onerror = function() {
                        console.error(formatString('Image load error for page {0}', i));
                        this.style.display = 'none';
                        wrapper.innerHTML += '<div class="error-message" style="color: #dc3545; padding: 20px; text-align: center;">Resim yüklenemedi. Lütfen sayfayı yenileyin.</div>';
                    };
                    
                    // Resim yükleme başarılı
                    img.onload = function() {
                        console.log(formatString('Image loaded successfully for page {0}', i));
                        console.log(formatString('Image dimensions: {0}x{1}', this.naturalWidth, this.naturalHeight));
                        this.style.display = 'block';
                    };

                    // Base64 verisini kullan
                    img.src = imageDataList[i - 1];
                    console.log(formatString('Setting image source for page {0}', i));
                    
                    wrapper.appendChild(img);
                    content.appendChild(wrapper);
                    
                    // Selection box ekle
                    const selection = document.createElement('div');
                    selection.id = formatString('selection_{0}', i);
                    selection.className = 'selection';
                    selection.style.cssText = 'position: absolute; border: 2px solid #dc3545; background-color: rgba(220,53,69,0.1); pointer-events: none; display: none; z-index: 1000; border-radius: 4px;';
                    content.appendChild(selection);
                    
                    contentsContainer.appendChild(content);
                }

                // Event listener'ları ekle
                document.querySelectorAll('.image-wrapper').forEach(wrapper => {
                    wrapper.addEventListener('mousedown', startSelection);
                });
                
                console.log('Tabs initialization completed');
            }

            function getCurrentSelectionBox() {
                return document.querySelector(`.tab-content.active .selection`);
            }

            function startSelection(e) {
                e.preventDefault();
                
                // Eğer mevcut seçim varsa ve yeni tıklama seçim dışındaysa, seçimi temizle
                if (currentSelection) {
                    clearSelection();
                    return;
                }

                isSelecting = true;
                const wrapper = e.currentTarget;
                const pos = getMousePosition(e, wrapper);
                
                startX = pos.x;
                startY = pos.y;

                selectionBox = getCurrentSelectionBox();
                if (selectionBox) {
                    selectionBox.style.left = (startX - wrapper.scrollLeft) + 'px';
                    selectionBox.style.top = (startY - wrapper.scrollTop) + 'px';
                    selectionBox.style.width = '0px';
                    selectionBox.style.height = '0px';
                    selectionBox.style.display = 'block';

                    document.addEventListener('mousemove', updateSelection);
                    document.addEventListener('mouseup', endSelection);
                }
            }

            function updateSelection(e) {
                e.preventDefault();
                if (!isSelecting || !selectionBox) return;

                const wrapper = document.querySelector('.tab-content.active .image-wrapper');
                const image = wrapper.querySelector('img');
                const imageRect = image.getBoundingClientRect();
                const pos = getMousePosition(e, wrapper);
                
                // Seçim koordinatlarını hesapla
                const x = Math.min(startX, pos.x);
                const y = Math.min(startY, pos.y);
                const w = Math.abs(pos.x - startX);
                const h = Math.abs(pos.y - startY);

                // Görüntülenen koordinatlara dönüştür
                const scaleX = imageRect.width / image.naturalWidth;
                const scaleY = imageRect.height / image.naturalHeight;
                const displayX = x * scaleX + imageRect.left - wrapper.getBoundingClientRect().left;
                const displayY = y * scaleY + imageRect.top - wrapper.getBoundingClientRect().top;
                const displayW = w * scaleX;
                const displayH = h * scaleY;

                // Seçim kutusunu resmin üzerine yerleştir
                selectionBox.style.position = 'absolute';
                selectionBox.style.left = displayX + 'px';
                selectionBox.style.top = displayY + 'px';
                selectionBox.style.width = displayW + 'px';
                selectionBox.style.height = displayH + 'px';
            }

            function endSelection(e) {
                e.preventDefault();
                if (!isSelecting || !selectionBox) return;
                
                isSelecting = false;
                const wrapper = document.querySelector('.tab-content.active .image-wrapper');
                const pos = getMousePosition(e, wrapper);
                
                const x = Math.min(startX, pos.x);
                const y = Math.min(startY, pos.y);
                const w = Math.abs(pos.x - startX);
                const h = Math.abs(pos.y - startY);

                if (w < 10 || h < 10) {
                    clearSelection();
                    return;
                }

                if (selectedSignatures.length >= MAX_SIGNATURES) {
                    showNotification('En fazla ' + MAX_SIGNATURES + ' imza seçebilirsiniz. Lütfen önce bir imzayı silin.', 'error');
                    clearSelection();
                    return;
                }

                const activeTab = document.querySelector('.tab.active');
                const currentPage = parseInt(activeTab.getAttribute('data-page'));

                currentSelection = {
                    page: currentPage,
                    x: Math.round(x),
                    y: Math.round(y),
                    width: Math.round(w),
                    height: Math.round(h)
                };

                // Capture the selected area as an image
                const img = wrapper.querySelector('img');
                const canvas = document.createElement('canvas');
                const ctx = canvas.getContext('2d');
                
                // Set canvas size to selection size
                canvas.width = w;
                canvas.height = h;
                
                // Draw the selected portion of the image
                ctx.drawImage(img, x, y, w, h, 0, 0, w, h);
                
                // Convert to base64
                const imageData = canvas.toDataURL('image/png');

                // Get current active PDF path
                const currentPdfPath = pdfList.length > 0 ? pdfList[0] : ''; // Default to first PDF in list

                // Add to selected signatures
                const signatureData = {
                    page: currentPage,
                    x: Math.round(x),
                    y: Math.round(y),
                    width: Math.round(w),
                    height: Math.round(h),
                    image: imageData,
                    sourcePdfPath: currentPdfPath
                };

                selectedSignatures.push(signatureData);
                updateSignatureSlots();

                // Update hidden field with all signatures
                hiddenSignatures.value = JSON.stringify(selectedSignatures);
                
                clearSelection();
                document.removeEventListener('mousemove', updateSelection);
                document.removeEventListener('mouseup', endSelection);
            }

            function updateSignatureSlots() {
                const slots = document.querySelectorAll('.signature-slot');
                
                slots.forEach((slot, index) => {
                    const signature = selectedSignatures[index];
                    const slotImage = slot.querySelector('.slot-image');
                    const deleteBtn = slot.querySelector('.delete-signature');
                    
                    if (signature) {
                        slot.classList.add('filled');
                        slotImage.style.backgroundImage = `url(${signature.image})`;
                        deleteBtn.style.display = 'flex';
                        
                        // Update delete button click handler
                        deleteBtn.onclick = () => deleteSignature(index);
                    } else {
                        slot.classList.remove('filled');
                        slotImage.style.backgroundImage = '';
                        deleteBtn.style.display = 'none';
                    }
                });
                
                // Enable/disable save button based on selections
                btnSave.disabled = selectedSignatures.length === 0;
            }

            function deleteSignature(index, event) {
                if (event) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                
                // Mevcut aktif sayfayı kaydet
                const currentActiveTab = document.querySelector('.tab.active');
                const currentPage = currentActiveTab ? parseInt(currentActiveTab.getAttribute('data-page')) : 1;
                
                selectedSignatures.splice(index, 1);
                updateSignatureSlots();
                hiddenSignatures.value = JSON.stringify(selectedSignatures);
                
                // Silme işleminden sonra aynı sayfayı göster
                if (currentPage) {
                    showPage(currentPage);
                }
                
                showNotification('İmza silindi', 'success');
                return false; // Prevent any form submission
            }

            function clearSelection() {
                if (selectionBox) {
                    selectionBox.style.display = 'none';
                }
                currentSelection = null;
                btnSave.disabled = true;
                hiddenField.value = '';
                
                document.removeEventListener('mousemove', updateSelection);
                document.removeEventListener('mouseup', endSelection);
            }

            function initializeImageEvents() {
                var img = document.querySelector('.tab-content.active img');
                if (img) {
                    var imageWrapper = document.querySelector('.tab-content.active .image-wrapper');
                    imageWrapper.addEventListener('mousedown', startSelection);
                }
            }

            function restoreSelection() {
                var savedData = hiddenField.value;
                if (savedData) {
                    var parts = savedData.split(',');
                    if (parts.length >= 5) {
                        var page = parseInt(parts[0]);
                        var x = parseInt(parts[1]);
                        var y = parseInt(parts[2]);
                        var w = parseInt(parts[3]);
                        var h = parseInt(parts[4]);

                        // Doğru sayfaya geç
                        showPage(page);

                        // Selection box'ı göster
                        selectionBox = getCurrentSelectionBox();
                        if (selectionBox) {
                            selectionBox.style.left = x + 'px';
                            selectionBox.style.top = y + 'px';
                            selectionBox.style.width = w + 'px';
                            selectionBox.style.height = h + 'px';
                            selectionBox.style.display = 'block';
                            btnSave.disabled = false;
                        }
                    }
                }
            }

            function saveSignature() {
                if (selectedSignatures.length === 0) {
                    showNotification('Lütfen en az bir imza seçin', 'error');
                    return false;
                }
                
                showLoading('İmzalar kaydediliyor...');
                
                // Butonu devre dışı bırak
                var btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
                if (btnSave) {
                    btnSave.disabled = true;
                }

                // Her bir imza için base64'ten dosya oluştur ve kaydet
                var formData = new FormData();
                
                // ASP.NET form verilerini ekle
                formData.append('__VIEWSTATE', document.getElementById('__VIEWSTATE').value);
                formData.append('__VIEWSTATEGENERATOR', document.getElementById('__VIEWSTATEGENERATOR').value);
                formData.append('__EVENTVALIDATION', document.getElementById('__EVENTVALIDATION').value);
                formData.append('__EVENTTARGET', '<%= btnSaveSignature.UniqueID %>');
                
                // İmza verilerini ekle
                formData.append('hdnSignatures', JSON.stringify(selectedSignatures));
                
                // AJAX çağrısı yap
                var xhr = new XMLHttpRequest();
                xhr.open('POST', window.location.href, true);
                xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
                
                xhr.onload = function() {
                    if (xhr.status === 200) {
                        try {
                            var response = JSON.parse(xhr.responseText);
                            if (response.success) {
                                showNotification('İmzalar başarıyla kaydedildi', 'success');
                                setTimeout(function() {
                                    window.close();
                                }, 1500);
                            } else {
                                throw new Error(response.error || 'Kayıt başarısız');
                            }
                        } catch (e) {
                            showNotification('Hata: ' + e.message, 'error');
                            if (btnSave) btnSave.disabled = false;
                        }
                    } else {
                        showNotification('Sunucu hatası', 'error');
                        if (btnSave) btnSave.disabled = false;
                    }
                    hideLoading();
                };
                
                xhr.onerror = function() {
                    hideLoading();
                    showNotification('Bağlantı hatası', 'error');
                    if (btnSave) btnSave.disabled = false;
                };
                
                xhr.send(formData);
                return false;
            }

            function base64ToBlob(base64, mimeType) {
                const byteCharacters = atob(base64);
                const byteArrays = [];

                for (let offset = 0; offset < byteCharacters.length; offset += 512) {
                    const slice = byteCharacters.slice(offset, offset + 512);
                    const byteNumbers = new Array(slice.length);
                    
                    for (let i = 0; i < slice.length; i++) {
                        byteNumbers[i] = slice.charCodeAt(i);
                    }
                    
                    const byteArray = new Uint8Array(byteNumbers);
                    byteArrays.push(byteArray);
                }

                return new Blob(byteArrays, { type: mimeType });
            }

            // Save button click handler
            var saveButton = document.getElementById('<%= btnSaveSignature.ClientID %>');
            if (saveButton) {
                saveButton.onclick = function(e) {
                    e.preventDefault();
                    return saveSignature();
                };
            }

            // Initialize Sys.WebForms.PageRequestManager for AJAX handling
            if (typeof(Sys) !== 'undefined') {
                var prm = Sys.WebForms.PageRequestManager.getInstance();
                
                prm.add_initializeRequest(function(sender, args) {
                    if (args.get_postBackElement().id === '<%= btnSaveSignature.ClientID %>') {
                        showLoading('İmza kaydediliyor...');
                    }
                });
                
                prm.add_endRequest(function(sender, args) {
                    hideLoading();
                    if (args.get_error() != undefined) {
                        var errorMessage = args.get_error().message;
                        console.error('Server error:', errorMessage);
                        showNotification('İmza kaydedilirken bir hata oluştu: ' + errorMessage, 'error');
                        args.set_errorHandled(true);
                        btnSave.disabled = false;
                    } else {
                        // Başarılı işlem sonrası sadece seçimi temizle, sayfayı yeniden yükleme
                        if (currentSelection) {
                            clearSelection();
                            // Seçim butonunu aktif bırak, böylece yeni seçim yapılabilir
                            btnSave.disabled = false;
                        }
                    }
                });

                // Timeout kontrolü
                var saveTimeout;
                prm.add_beginRequest(function() {
                    saveTimeout = setTimeout(function() {
                        hideLoading();
                        showNotification('İmza kaydetme işlemi zaman aşımına uğradı. Lütfen tekrar deneyiniz.', 'error');
                        btnSave.disabled = false;
                    }, 30000); // 30 saniye timeout
                });

                prm.add_endRequest(function() {
                    clearTimeout(saveTimeout);
                });
            }

            // Modify file upload event
            var uploadButton = document.getElementById('<%= btnUpload.ClientID %>');
            if (uploadButton) {
                uploadButton.addEventListener('click', function() {
                    showLoading('Dosya yükleniyor...');
                    setTimeout(() => updateLoadingMessage('PDF sayfaları görüntüye dönüştürülüyor...'), 1000);
                    setTimeout(() => updateLoadingMessage('Görüntü hazırlanıyor...'), 2000);
                });
            }

            // PDF listesi yönetimi
            let pdfList = [];
            const pdfListContainer = document.getElementById('pdfList');
            const hdnCurrentPdfList = document.getElementById('<%= hdnCurrentPdfList.ClientID %>');

            function initializePdfList() {
                // URL'den PDF listesini al
                const urlParams = new URLSearchParams(window.location.search);
                const pdfListParam = urlParams.get('pdfList');
                
                if (pdfListParam) {
                    pdfList = decodeURIComponent(pdfListParam).split(',');
                    hdnCurrentPdfList.value = pdfListParam;
                    updatePdfListUI();
                }
            }

            function updatePdfListUI() {
                pdfListContainer.innerHTML = '';
                pdfList.forEach((pdfPath, index) => {
                    const pdfItem = document.createElement('div');
                    pdfItem.className = 'pdf-item';
                    pdfItem.innerHTML = `
                        <span class="pdf-item-name">${pdfPath.split('/').pop()}</span>
                        <span class="pdf-item-remove" onclick="removePdf(${index}, event)">✕</span>
                    `;
                    pdfItem.onclick = () => loadPdf(pdfPath);
                    pdfListContainer.appendChild(pdfItem);
                });
            }

            function loadPdf(pdfPath) {
                // PDF yükleme işlemi için mevcut form mekanizmasını kullan
                showLoading('PDF yükleniyor...');
                // TODO: PDF yükleme işlemi için sunucu tarafında gerekli değişiklikleri yap
            }

            function removePdf(index, event) {
                event.stopPropagation();
                pdfList.splice(index, 1);
                hdnCurrentPdfList.value = pdfList.join(',');
                updatePdfListUI();
                showNotification('PDF listeden kaldırıldı', 'success');
            }

            function addPdfToList(pdfPath) {
                if (!pdfList.includes(pdfPath)) {
                    pdfList.push(pdfPath);
                    hdnCurrentPdfList.value = pdfList.join(',');
                    updatePdfListUI();
                }
            }

            if (window.addEventListener) {
                window.addEventListener('load', function() {
                    initializeImageEvents();
                    initializePdfList();
                    
                    // Initialize signature slots
                    const slots = document.querySelectorAll('.signature-slot');
                    slots.forEach((slot, index) => {
                        const deleteBtn = slot.querySelector('.delete-signature');
                        if (deleteBtn) {
                            deleteBtn.onclick = (e) => deleteSignature(index, e);
                        }
                    });

                    // Restore any saved signatures
                    if (hiddenSignatures.value) {
                        try {
                            selectedSignatures = JSON.parse(hiddenSignatures.value);
                            updateSignatureSlots();
                        } catch (e) {
                            console.error('Error restoring signatures:', e);
                        }
                    }
                });
            }

            window.addEventListener('resize', function() {
                if (selectionBox && selectionBox.style.display !== 'none') {
                    restoreSelection();
                }
            });
        </script>
    </form>
</body>
</html>