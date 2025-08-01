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
            max-width: 100%;
            height: 100%;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            padding: 10px 0;
            margin-bottom: 20px;
        }
        .upload-panel {
            display: flex;
            gap: 10px;
            align-items: center;
            margin-bottom: 20px;
            flex-wrap: wrap;
        }
        .image-container {
            position: relative;
            flex: 1;
            min-height: 0;
            border: 1px solid #ccc;
            background: white;
            overflow: hidden;
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
        #selection {
            position: absolute;
            border: 2px solid red;
            background-color: rgba(255,0,0,0.1);
            pointer-events: none;
            display: none;
            z-index: 1000;
        }
        .button {
            padding: 8px 15px;
            margin-right: 10px;
            cursor: pointer;
            border: none;
            background-color: #007bff;
            color: white;
            border-radius: 4px;
            transition: background-color 0.3s;
        }
        .button:hover {
            background-color: #0056b3;
        }
        .button:disabled {
            background-color: #cccccc;
            cursor: not-allowed;
        }
        .footer {
            padding: 10px 0;
            display: flex;
            justify-content: space-between;
            align-items: center;
        }
        .message {
            padding: 10px;
            margin: 10px 0;
            border-radius: 4px;
        }
        .message.error {
            background-color: #ffe6e6;
            border: 1px solid #ff9999;
        }
        .message.success {
            background-color: #e6ffe6;
            border: 1px solid #99ff99;
        }
        /* Tam ekran butonu stilleri */
        .fullscreen-btn {
            position: absolute;
            top: 10px;
            right: 10px;
            z-index: 1001;
            background: rgba(0,0,0,0.5);
            color: white;
            border: none;
            padding: 5px 10px;
            cursor: pointer;
            border-radius: 4px;
        }
        /* Tam ekran modu stilleri */
        .fullscreen {
            position: fixed !important;
            top: 0 !important;
            left: 0 !important;
            width: 100vw !important;
            height: 100vh !important;
            z-index: 9999 !important;
            background: white !important;
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
                <asp:FileUpload ID="fileUpload" runat="server" />
                <asp:Button ID="btnUpload" runat="server" Text="Yükle" CssClass="button" OnClick="BtnUpload_Click" />
                <asp:Button ID="btnShowPdf" runat="server" Text="İmza Sirküleri Göster" CssClass="button" 
                    OnClick="BtnShowPdf_Click" Enabled="false" />
            </div>

            <div id="imageContainer" runat="server" class="image-container">
                <button type="button" class="fullscreen-btn" onclick="toggleFullscreen()">
                    <span class="fullscreen-icon">⛶</span>
                </button>
                <div class="image-wrapper">
                    <asp:Image ID="imgSignature" runat="server" style="max-width: 100%; max-height: 100%;" />
                    <div id="selection"></div>
                </div>
            </div>

            <div class="footer">
                <asp:HiddenField ID="hdnSelection" runat="server" />
                <asp:Button ID="btnSaveSignature" runat="server" Text="Seçilen İmzayı Kaydet" 
                    CssClass="button" OnClick="BtnSaveSignature_Click" Enabled="false" />
                
                <asp:Label ID="lblMessage" runat="server" CssClass="message"></asp:Label>
            </div>
        </div>

        <script type="text/javascript">
            var isSelecting = false;
            var startX, startY;
            var selectionBox = document.getElementById('selection');
            var hiddenField = document.getElementById('<%= hdnSelection.ClientID %>');
            var imageContainer = document.getElementById('<%= imageContainer.ClientID %>');
            var btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
            var isFullscreen = false;

            function toggleFullscreen() {
                var container = document.getElementById('<%= imageContainer.ClientID %>');
                if (!isFullscreen) {
                    container.classList.add('fullscreen');
                    document.querySelector('.fullscreen-icon').textContent = '⛶';
                } else {
                    container.classList.remove('fullscreen');
                    document.querySelector('.fullscreen-icon').textContent = '⛶';
                }
                isFullscreen = !isFullscreen;
                
                // Seçim kutusunu sıfırla
                if (selectionBox) {
                    selectionBox.style.display = 'none';
                }
                
                // Görüntüyü yeniden boyutlandır
                setTimeout(function() {
                    if (window.dispatchEvent) {
                        window.dispatchEvent(new Event('resize'));
                    }
                }, 100);
            }

            function startSelection(e) {
                isSelecting = true;
                var imageWrapper = document.querySelector('.image-wrapper');
                var rect = e.target.getBoundingClientRect();
                
                // Scroll pozisyonlarını hesaba kat
                startX = e.clientX - rect.left + imageWrapper.scrollLeft;
                startY = e.clientY - rect.top + imageWrapper.scrollTop;

                selectionBox.style.left = (e.clientX - rect.left) + 'px';
                selectionBox.style.top = (e.clientY - rect.top) + 'px';
                selectionBox.style.width = '0px';
                selectionBox.style.height = '0px';
                selectionBox.style.display = 'block';
            }

            function updateSelection(e) {
                if (!isSelecting) return;

                var imageWrapper = document.querySelector('.image-wrapper');
                var rect = e.target.getBoundingClientRect();
                var currentX = e.clientX - rect.left + imageWrapper.scrollLeft;
                var currentY = e.clientY - rect.top + imageWrapper.scrollTop;

                var x = Math.min(startX, currentX) - imageWrapper.scrollLeft;
                var y = Math.min(startY, currentY) - imageWrapper.scrollTop;
                var w = Math.abs(currentX - startX);
                var h = Math.abs(currentY - startY);

                selectionBox.style.left = x + 'px';
                selectionBox.style.top = y + 'px';
                selectionBox.style.width = w + 'px';
                selectionBox.style.height = h + 'px';
            }

            function endSelection(e) {
                if (!isSelecting) return;
                isSelecting = false;

                var imageWrapper = document.querySelector('.image-wrapper');
                var rect = e.target.getBoundingClientRect();
                var currentX = e.clientX - rect.left + imageWrapper.scrollLeft;
                var currentY = e.clientY - rect.top + imageWrapper.scrollTop;

                var x = Math.min(startX, currentX) - imageWrapper.scrollLeft;
                var y = Math.min(startY, currentY) - imageWrapper.scrollTop;
                var w = Math.abs(currentX - startX);
                var h = Math.abs(currentY - startY);

                if (w < 10 || h < 10) {
                    selectionBox.style.display = 'none';
                    return;
                }

                var selectionData = [
                    Math.round(x),
                    Math.round(y),
                    Math.round(w),
                    Math.round(h)
                ].join(',');

                hiddenField.value = selectionData;
                btnSave.disabled = false;
                
                if (typeof __doPostBack === 'function') {
                    __doPostBack('<%= btnSaveSignature.UniqueID %>', '');
                } else {
                    btnSave.click();
                }
            }

            function initializeImageEvents() {
                var img = document.querySelector('#<%= imgSignature.ClientID %>');
                if (img) {
                    img.addEventListener('mousedown', startSelection);
                    img.addEventListener('mousemove', updateSelection);
                    img.addEventListener('mouseup', endSelection);
                    img.addEventListener('mouseleave', function(e) {
                        if (isSelecting) {
                            endSelection(e);
                        }
                    });
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

            // Sayfa yüklendiğinde ve görüntü değiştiğinde olayları başlat
            if (window.addEventListener) {
                window.addEventListener('load', function() {
                    initializeImageEvents();
                    restoreSelection();
                });
            }

            // Pencere boyutu değiştiğinde seçimi yeniden konumlandır
            window.addEventListener('resize', function() {
                if (selectionBox.style.display !== 'none') {
                    restoreSelection();
                }
            });
        </script>
    </form>
</body>
</html>