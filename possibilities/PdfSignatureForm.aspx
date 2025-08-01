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
        }
        .container {
            flex: 1;
            display: flex;
            flex-direction: column;
            padding: 20px;
            box-sizing: border-box;
            max-width: 1200px;
            margin: 0 auto;
            width: 100%;
            height: 100%;
            background-color: white;
            box-shadow: 0 0 10px rgba(0,0,0,0.1);
            border-radius: 8px;
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
            border: 2px solid #eee;
            border-radius: 8px;
            background: #fff;
            overflow: hidden;
            margin: 10px 0;
            display: flex;
            flex-direction: column;
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
            display: none;
            flex: 1;
            position: relative;
        }
        .tab-content.active {
            display: flex;
        }
        .image-wrapper {
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            display: flex;
            justify-content: center;
            align-items: center;
            overflow: auto;
        }
        .image-wrapper img {
            max-width: none;
            max-height: none;
        }
        #selection {
            position: absolute;
            border: 2px solid #dc3545;
            background-color: rgba(220,53,69,0.1);
            pointer-events: none;
            display: none;
            z-index: 1000;
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
            background: rgba(0, 0, 0, 0.5);
            z-index: 9999;
            justify-content: center;
            align-items: center;
        }
        .loading-content {
            display: flex;
            flex-direction: column;
            align-items: center;
            gap: 20px;
            background: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
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
        }
        .notification.show {
            transform: translateX(0);
        }
        .notification.success {
            border-left: 4px solid #28a745;
        }
        .notification.error {
            border-left: 4px solid #dc3545;
        }
        .notification.info {
            border-left: 4px solid #17a2b8;
        }
        .footer {
            padding: 15px 0;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-top: 2px solid #eee;
            margin-top: 20px;
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
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
            <div class="header">
                <h2>İmza Sirkülerinden İmza Seçimi</h2>
            </div>
            
            <div class="upload-panel">
                <div class="instructions">
                    <strong>Nasıl Kullanılır:</strong>
                    <ol>
                        <li>PDF formatındaki imza sirkülerinizi seçin ve "Yükle" butonuna tıklayın</li>
                        <li>"İmza Sirküleri Göster" butonuna tıklayarak dökümanı görüntüleyin</li>
                        <li>Mouse ile imza alanını seçin - seçim tamamlandığında otomatik kaydedilecektir</li>
                    </ol>
                </div>
                <asp:FileUpload ID="fileUpload" runat="server" />
                <asp:Button ID="btnUpload" runat="server" Text="Yükle" CssClass="button" OnClick="BtnUpload_Click" />
                <asp:Button ID="btnShowPdf" runat="server" Text="İmza Sirküleri Göster" CssClass="button secondary" 
                    OnClick="BtnShowPdf_Click" Enabled="false" />
            </div>

            <div id="imageContainer" runat="server" class="image-container">
                <div class="tabs" id="pageTabs">
                    <!-- Tabs will be added here dynamically -->
                </div>
                <div id="pageContents">
                    <!-- Tab contents will be added here dynamically -->
                </div>
            </div>

            <div class="footer">
                <asp:HiddenField ID="hdnSelection" runat="server" />
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
            <span id="notificationMessage"></span>
        </div>

        <script type="text/javascript">
            function getMousePosition(e, element) {
                var rect = element.getBoundingClientRect();
                return {
                    x: e.clientX - rect.left + element.scrollLeft,
                    y: e.clientY - rect.top + element.scrollTop
                };
            }

            function showLoading(message) {
                document.getElementById('loadingOverlay').style.display = 'flex';
                document.getElementById('loadingMessage').textContent = message || 'İşlem yapılıyor...';
            }

            function updateLoadingMessage(message) {
                document.getElementById('loadingMessage').textContent = message;
            }

            var isSelecting = false;
            var startX, startY;
            var selectionBox = document.getElementById('selection');
            var hiddenField = document.getElementById('<%= hdnSelection.ClientID %>');
            var imageContainer = document.getElementById('<%= imageContainer.ClientID %>');
            var btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
            var currentSelection = null;

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
                
                // Her sayfa için tab ve içerik oluştur
                for (let i = 1; i <= pageCount; i++) {
                    // Tab oluştur
                    const tab = document.createElement('div');
                    tab.className = 'tab' + (i === 1 ? ' active' : '');
                    tab.setAttribute('data-page', i);
                    tab.textContent = `Sayfa ${i}`;
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
                    img.id = `imgSignature_${i}`;
                    img.src = `cdn/page_${i}.png?t=${new Date().getTime()}`;
                    
                    wrapper.appendChild(img);
                    content.appendChild(wrapper);
                    contentsContainer.appendChild(content);
                }

                // Selection box'ı her tab content'e ekle
                document.querySelectorAll('.tab-content').forEach(content => {
                    const selection = document.createElement('div');
                    selection.id = `selection_${content.getAttribute('data-page')}`;
                    selection.className = 'selection';
                    selection.style.cssText = 'position: absolute; border: 2px solid #dc3545; background-color: rgba(220,53,69,0.1); pointer-events: none; display: none; z-index: 1000; border-radius: 4px;';
                    content.appendChild(selection);
                });

                // Event listener'ları ekle
                document.querySelectorAll('.tab-content').forEach(content => {
                    content.addEventListener('mousedown', startSelection);
                });
            }

            // Seçim işlemleri için aktif sayfayı takip et
            let currentPage = 1;

            function getCurrentSelectionBox() {
                return document.querySelector(`.tab-content[data-page="${currentPage}"] .selection`);
            }

            function startSelection(e) {
                e.preventDefault();
                
                // Tıklanan sayfayı belirle
                const tabContent = e.target.closest('.tab-content');
                if (!tabContent) return;
                
                currentPage = parseInt(tabContent.getAttribute('data-page'));
                selectionBox = getCurrentSelectionBox();
                
                if (currentSelection) {
                    clearSelection();
                    return;
                }

                isSelecting = true;
                const pos = getMousePosition(e, tabContent);
                
                startX = pos.x;
                startY = pos.y;

                selectionBox.style.left = startX + 'px';
                selectionBox.style.top = startY + 'px';
                selectionBox.style.width = '0px';
                selectionBox.style.height = '0px';
                selectionBox.style.display = 'block';

                tabContent.addEventListener('mousemove', updateSelection);
                tabContent.addEventListener('mouseup', endSelection);
                tabContent.addEventListener('mouseleave', endSelection);
            }

            function updateSelection(e) {
                e.preventDefault();
                if (!isSelecting) return;

                const tabContent = e.target.closest('.tab-content');
                if (!tabContent) return;

                const pos = getMousePosition(e, tabContent);
                
                const x = Math.min(startX, pos.x);
                const y = Math.min(startY, pos.y);
                const w = Math.abs(pos.x - startX);
                const h = Math.abs(pos.y - startY);

                selectionBox.style.left = x + 'px';
                selectionBox.style.top = y + 'px';
                selectionBox.style.width = w + 'px';
                selectionBox.style.height = h + 'px';
            }

            function endSelection(e) {
                e.preventDefault();
                if (!isSelecting) return;
                
                const tabContent = e.target.closest('.tab-content');
                if (!tabContent) return;

                isSelecting = false;
                const pos = getMousePosition(e, tabContent);
                
                const x = Math.min(startX, pos.x);
                const y = Math.min(startY, pos.y);
                const w = Math.abs(pos.x - startX);
                const h = Math.abs(pos.y - startY);

                if (w < 10 || h < 10) {
                    selectionBox.style.display = 'none';
                    return;
                }

                currentSelection = {
                    page: currentPage,
                    x: Math.round(x),
                    y: Math.round(y),
                    width: Math.round(w),
                    height: Math.round(h)
                };

                var selectionData = [
                    currentSelection.page,
                    currentSelection.x,
                    currentSelection.y,
                    currentSelection.width,
                    currentSelection.height
                ].join(',');

                hiddenField.value = selectionData;
                btnSave.disabled = false;

                tabContent.removeEventListener('mousemove', updateSelection);
                tabContent.removeEventListener('mouseup', endSelection);
                tabContent.removeEventListener('mouseleave', endSelection);
            }

            function clearSelection() {
                if (selectionBox) {
                    selectionBox.style.display = 'none';
                }
                currentSelection = null;
                btnSave.disabled = true;
                hiddenField.value = '';
            }

            function initializeImageEvents() {
                var img = document.querySelector('#<%= imgSignature.ClientID %>');
                if (img) {
                    var imageWrapper = document.querySelector('.image-wrapper');
                    imageWrapper.addEventListener('mousedown', startSelection);
                }
            }

            function restoreSelection() {
                var savedData = hiddenField.value;
                if (savedData) {
                    var parts = savedData.split(',');
                    if (parts.length >= 4) {
                        var x = parseInt(parts[0]);
                        var y = parseInt(parts[1]);
                        var w = parseInt(parts[2]);
                        var h = parseInt(parts[3]);

                        selectionBox.style.left = x + 'px';
                        selectionBox.style.top = y + 'px';
                        selectionBox.style.width = w + 'px';
                        selectionBox.style.height = h + 'px';
                        selectionBox.style.display = 'block';
                        btnSave.disabled = false;
                    }
                }
            }

            function showNotification(message, type) {
                const notification = document.getElementById('notification');
                const notificationMessage = document.getElementById('notificationMessage');
                
                notification.className = 'notification ' + type;
                notificationMessage.textContent = message;
                
                notification.classList.add('show');
                
                setTimeout(() => {
                    notification.classList.remove('show');
                }, 3000);
            }

            // Modify file upload event
            var uploadButton = document.getElementById('<%= btnUpload.ClientID %>');
            if (uploadButton) {
                uploadButton.addEventListener('click', function() {
                    showLoading('Dosya yükleniyor...');
                });
            }

            // Modify PDF show event
            var showPdfButton = document.getElementById('<%= btnShowPdf.ClientID %>');
            if (showPdfButton) {
                showPdfButton.addEventListener('click', function() {
                    showLoading('PDF dosyası yükleniyor...');
                    setTimeout(() => updateLoadingMessage('PDF sayfaları görüntüye dönüştürülüyor...'), 1000);
                    setTimeout(() => updateLoadingMessage('Görüntü hazırlanıyor...'), 2000);
                });
            }

            // Add event listener for signature save button
            var saveSignatureButton = document.getElementById('<%= btnSaveSignature.ClientID %>');
            if (saveSignatureButton) {
                saveSignatureButton.addEventListener('click', function() {
                    if (currentSelection) {
                        showLoading('İmza alanı kesiliyor...');
                        setTimeout(() => updateLoadingMessage('İmza kaydediliyor...'), 1000);
                        clearSelection();
                    }
                });
            }

            // Add event listener for form submission
            var form = document.getElementById('form1');
            if (form) {
                form.addEventListener('submit', function() {
                    showLoading('Form gönderiliyor...');
                });
            }

            // Initialize Sys.WebForms.PageRequestManager for AJAX handling
            if (typeof(Sys) !== 'undefined') {
                var prm = Sys.WebForms.PageRequestManager.getInstance();
                
                prm.add_endRequest(function(sender, args) {
                    hideLoading();
                    // Sayfa sayısını al (hidden field'dan veya başka bir kaynaktan)
                    var pageCount = parseInt(document.getElementById('<%= hdnPageCount.ClientID %>').value) || 1;
                    initializeTabs(pageCount);
                });
            }

            if (window.addEventListener) {
                window.addEventListener('load', function() {
                    initializeImageEvents();
                    restoreSelection();
                });
            }

            window.addEventListener('resize', function() {
                if (selectionBox.style.display !== 'none') {
                    restoreSelection();
                }
            });
        </script>
    </form>
</body>
</html>