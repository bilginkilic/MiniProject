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
            border: 2px solid #007bff;
            background-color: rgba(0,123,255,0.1);
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
            background-color: #007bff;
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
            background-color: #0056b3;
            transform: translateY(-1px);
        }
        .button:disabled {
            background-color: #ccc;
            cursor: not-allowed;
            transform: none;
        }
        .button.secondary {
            background-color: #6c757d;
        }
        .button.secondary:hover {
            background-color: #5a6268;
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
                <div class="image-wrapper">
                    <asp:Image ID="imgSignature" runat="server" />
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

            function getMousePosition(e, element) {
                var rect = element.getBoundingClientRect();
                return {
                    x: e.clientX - rect.left + element.scrollLeft,
                    y: e.clientY - rect.top + element.scrollTop
                };
            }

            function startSelection(e) {
                isSelecting = true;
                var imageWrapper = document.querySelector('.image-wrapper');
                var pos = getMousePosition(e, imageWrapper);
                
                startX = pos.x;
                startY = pos.y;

                selectionBox.style.left = startX + 'px';
                selectionBox.style.top = startY + 'px';
                selectionBox.style.width = '0px';
                selectionBox.style.height = '0px';
                selectionBox.style.display = 'block';
            }

            function updateSelection(e) {
                if (!isSelecting) return;

                var imageWrapper = document.querySelector('.image-wrapper');
                var pos = getMousePosition(e, imageWrapper);
                
                var x = Math.min(startX, pos.x);
                var y = Math.min(startY, pos.y);
                var w = Math.abs(pos.x - startX);
                var h = Math.abs(pos.y - startY);

                selectionBox.style.left = x + 'px';
                selectionBox.style.top = y + 'px';
                selectionBox.style.width = w + 'px';
                selectionBox.style.height = h + 'px';
            }

            function endSelection(e) {
                if (!isSelecting) return;
                isSelecting = false;

                var imageWrapper = document.querySelector('.image-wrapper');
                var pos = getMousePosition(e, imageWrapper);
                
                var x = Math.min(startX, pos.x);
                var y = Math.min(startY, pos.y);
                var w = Math.abs(pos.x - startX);
                var h = Math.abs(pos.y - startY);

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