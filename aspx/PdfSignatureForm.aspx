<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfSignatureForm.aspx.cs" Inherits="AspxExamples.PdfSignatureForm" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>İmza Sirkülerinden İmza Seçimi</title>
    <style type="text/css">
        body { 
            margin: 20px; 
            font-family: Arial, sans-serif;
        }
        .container {
            width: 900px;
            margin: 0 auto;
        }
        .upload-panel {
            margin-bottom: 20px;
        }
        .image-container {
            position: relative;
            width: 800px;
            height: 500px;
            border: 1px solid #ccc;
            margin: 20px 0;
            background: white;
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
        }
        .hidden {
            display: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>İmza Sirkülerinden İmza Seçimi</h2>
            
            <div class="upload-panel">
                <asp:FileUpload ID="fileUpload" runat="server" />
                <asp:Button ID="btnUpload" runat="server" Text="Yükle" CssClass="button" OnClick="BtnUpload_Click" />
                <asp:Button ID="btnShowPdf" runat="server" Text="İmza Sirküleri Göster" CssClass="button" 
                    OnClick="BtnShowPdf_Click" Enabled="false" />
            </div>

            <div id="imageContainer" runat="server" class="image-container">
                <asp:Image ID="imgSignature" runat="server" Width="100%" Height="100%" />
                <div id="selection"></div>
            </div>

            <asp:HiddenField ID="hdnSelection" runat="server" />
            <asp:Button ID="btnSaveSignature" runat="server" Text="Seçilen İmzayı Kaydet" 
                CssClass="button" OnClick="BtnSaveSignature_Click" Enabled="false" />
            
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
        </div>

        <script type="text/javascript">
            var isSelecting = false;
            var startX, startY;
            var selectionBox = document.getElementById('selection');
            var hiddenField = document.getElementById('<%= hdnSelection.ClientID %>');
            var imageContainer = document.getElementById('<%= imageContainer.ClientID %>');
            var btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');

            function startSelection(e) {
                isSelecting = true;
                var rect = imageContainer.getBoundingClientRect();
                startX = e.clientX - rect.left;
                startY = e.clientY - rect.top;

                selectionBox.style.left = startX + 'px';
                selectionBox.style.top = startY + 'px';
                selectionBox.style.width = '0px';
                selectionBox.style.height = '0px';
                selectionBox.style.display = 'block';
            }

            function updateSelection(e) {
                if (!isSelecting) return;

                var rect = imageContainer.getBoundingClientRect();
                var currentX = e.clientX - rect.left;
                var currentY = e.clientY - rect.top;

                var x = Math.min(startX, currentX);
                var y = Math.min(startY, currentY);
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

                var rect = imageContainer.getBoundingClientRect();
                var currentX = e.clientX - rect.left;
                var currentY = e.clientY - rect.top;

                var x = Math.min(startX, currentX);
                var y = Math.min(startY, currentY);
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
                __doPostBack('<%= btnSaveSignature.UniqueID %>', '');
            }

            if (imageContainer) {
                imageContainer.addEventListener('mousedown', startSelection);
                imageContainer.addEventListener('mousemove', updateSelection);
                imageContainer.addEventListener('mouseup', endSelection);
                imageContainer.addEventListener('mouseleave', function(e) {
                    if (isSelecting) {
                        endSelection(e);
                    }
                });
            }

            // Son seçimi geri yükle
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

            window.onload = restoreSelection;
        </script>
    </form>
</body>
</html>