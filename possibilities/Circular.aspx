<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Circular.aspx.cs" Inherits="AspxExamples.Circular" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="Sirküler Yönetimi ve İmza Seçimi">
    <meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:;">
    <title>Sirküler Yönetimi</title>
    <style type="text/css">
        html, body { 
            margin: 0; 
            padding: 0;
            width: 100%;
            height: 100%;
            font-family: Arial, sans-serif;
            background-color: #f5f5f5;
        }

        .main-container {
            display: flex;
            flex-direction: column;
            min-height: 100vh;
            padding: 20px;
            box-sizing: border-box;
        }

        /* List View Styles */
        .list-view {
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }

        .list-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
        }

        .circular-grid {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .circular-grid th,
        .circular-grid td {
            padding: 12px;
            border: 1px solid #ddd;
            text-align: left;
        }

        .circular-grid th {
            background: #f8f9fa;
            font-weight: 500;
        }

        .circular-grid tr:hover {
            background: #f8f9fa;
            cursor: pointer;
        }

        /* Detail View Styles */
        .detail-view {
            display: none;
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }

        .detail-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 15px;
            border-bottom: 2px solid #eee;
        }

        .form-grid {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 20px;
            margin-bottom: 20px;
        }

        .form-row {
            display: grid;
            grid-template-columns: 150px 1fr;
            align-items: center;
            gap: 10px;
        }

        .form-row label {
            color: #666;
            font-size: 14px;
            font-weight: 500;
        }

        .form-row input,
        .form-row select {
            padding: 8px 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
            width: 100%;
        }

        /* Button Styles */
        .button {
            padding: 10px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            transition: all 0.3s ease;
            display: inline-flex;
            align-items: center;
            gap: 8px;
        }

        .button.primary {
            background: #dc3545;
            color: white;
        }

        .button.secondary {
            background: #6c757d;
            color: white;
        }

        .button:hover {
            opacity: 0.9;
            transform: translateY(-1px);
        }

        /* Signature Selection Styles */
        .signature-container {
            margin-top: 20px;
            padding: 20px;
            background: #f8f9fa;
            border-radius: 8px;
            border: 1px solid #ddd;
        }

        .signature-preview {
            width: 150px;
            height: 80px;
            border: 1px solid #ddd;
            margin: 10px;
            background-size: contain;
            background-repeat: no-repeat;
            background-position: center;
            background-color: white;
        }

        /* Notification Styles */
        .notification {
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 15px 25px;
            border-radius: 4px;
            background: white;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            display: none;
            z-index: 1000;
        }

        .notification.show {
            display: block;
        }

        /* Loading Overlay */
        .loading-overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background: rgba(0,0,0,0.5);
            display: none;
            justify-content: center;
            align-items: center;
            z-index: 9999;
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
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        /* Image Selection Styles */
        .image-container {
            position: relative;
            margin-top: 20px;
            border: 2px solid #eee;
            border-radius: 8px;
            overflow: hidden;
        }

        .image-wrapper {
            width: 100%;
            height: 500px;
            overflow: auto;
            background: #f8f9fa;
        }

        .image-wrapper img {
            max-width: 100%;
            height: auto;
        }

        .selection-box {
            position: absolute;
            border: 2px solid #dc3545;
            background-color: rgba(220,53,69,0.1);
            pointer-events: none;
            display: none;
        }

        /* Tab Styles */
        .tabs {
            display: flex;
            gap: 5px;
            margin-bottom: 20px;
        }

        .tab {
            padding: 10px 20px;
            background: #f8f9fa;
            border: 1px solid #ddd;
            border-radius: 4px;
            cursor: pointer;
        }

        .tab.active {
            background: #dc3545;
            color: white;
            border-color: #dc3545;
        }

        /* Responsive Design */
        @media (max-width: 1200px) {
            .form-grid {
                grid-template-columns: 1fr;
            }
        }

        @media (max-width: 768px) {
            .form-row {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="main-container">
            <!-- List View -->
            <div id="listView" class="list-view">
                <div class="list-header">
                    <h2>Sirküler Listesi</h2>
                    <asp:Button ID="btnNewCircular" runat="server" Text="Yeni Sirküler" CssClass="button primary" OnClick="BtnNewCircular_Click" />
                </div>

                <asp:GridView ID="gvCirculars" runat="server" CssClass="circular-grid" AutoGenerateColumns="false" 
                    OnRowCommand="GvCirculars_RowCommand" OnRowDataBound="GvCirculars_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="MusteriNo" HeaderText="Müşteri No" />
                        <asp:BoundField DataField="FirmaUnvani" HeaderText="Firma Ünvanı" />
                        <asp:BoundField DataField="DuzenlemeTarihi" HeaderText="Düzenleme Tarihi" />
                        <asp:BoundField DataField="GecerlilikTarihi" HeaderText="Geçerlilik Tarihi" />
                        <asp:BoundField DataField="SirkulerTipi" HeaderText="Sirküler Tipi" />
                        <asp:BoundField DataField="SirkulerNoter" HeaderText="Sirküler Noter No" />
                        <asp:BoundField DataField="Durum" HeaderText="Durum" />
                        <asp:TemplateField HeaderText="İşlemler">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditCircular" 
                                    CommandArgument='<%# Container.DataItemIndex %>' Text="Düzenle" />
                                |
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="DeleteCircular" 
                                    CommandArgument='<%# Container.DataItemIndex %>' Text="Sil" 
                                    OnClientClick="return confirm('Bu kaydı silmek istediğinizden emin misiniz?');" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <!-- Detail View -->
            <div id="detailView" class="detail-view">
                <div class="detail-header">
                    <h2>Sirküler Detayı</h2>
                    <div>
                        <asp:Button ID="btnSave" runat="server" Text="Kaydet" CssClass="button primary" OnClick="BtnSave_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="İptal" CssClass="button secondary" OnClick="BtnCancel_Click" />
                    </div>
                </div>

                <div class="form-grid">
                    <div class="form-row">
                        <label>Müşteri No:</label>
                        <asp:TextBox ID="txtMusteriNo" runat="server" />
                    </div>
                    <div class="form-row">
                        <label>Firma Ünvanı:</label>
                        <asp:TextBox ID="txtFirmaUnvani" runat="server" />
                    </div>
                    <div class="form-row">
                        <label>Düzenleme Tarihi:</label>
                        <asp:TextBox ID="txtDuzenlemeTarihi" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-row">
                        <label>Geçerlilik Tarihi:</label>
                        <asp:TextBox ID="txtGecerlilikTarihi" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-row">
                        <label>İç Yönerge PG Tarihi:</label>
                        <asp:TextBox ID="txtIcYonergePGTarihi" runat="server" TextMode="Date" />
                    </div>
                    <div class="form-row">
                        <label>TSG No:</label>
                        <asp:TextBox ID="txtTSGNo" runat="server" />
                    </div>
                    <div class="form-row">
                        <label>Özel Durumlar:</label>
                        <asp:TextBox ID="txtOzelDurumlar" runat="server" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="form-row">
                        <label>Sirküler Tipi:</label>
                        <asp:DropDownList ID="ddlSirkulerTipi" runat="server">
                            <asp:ListItem Text="Ana Sirküler" Value="Ana Sirküler" />
                            <asp:ListItem Text="Ek Sirküler" Value="Ek Sirküler" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label>Sirküler Noter No:</label>
                        <asp:TextBox ID="txtSirkulerNoterNo" runat="server" />
                    </div>
                    <div class="form-row">
                        <label>Açıklama:</label>
                        <asp:TextBox ID="txtAciklama" runat="server" TextMode="MultiLine" Rows="3" />
                    </div>
                    <div class="form-row">
                        <label>Sirküler Durumu:</label>
                        <asp:DropDownList ID="ddlSirkulerDurumu" runat="server">
                            <asp:ListItem Text="Aktif" Value="Aktif" />
                            <asp:ListItem Text="Pasif" Value="Pasif" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label>Y.K.K. Gerekli mi?:</label>
                        <asp:CheckBox ID="chkYKKGerekli" runat="server" />
                    </div>
                </div>

                <!-- Signature Selection Area -->
                <div class="signature-container">
                    <h3>İmza Seçimi</h3>
                    <div class="image-container">
                        <div class="tabs">
                            <div class="tab active" data-page="1">Sayfa 1</div>
                            <div class="tab" data-page="2">Sayfa 2</div>
                        </div>
                        <div class="image-wrapper">
                            <asp:Image ID="imgSignature" runat="server" />
                            <div class="selection-box"></div>
                        </div>
                    </div>
                    <div style="margin-top: 20px;">
                        <asp:FileUpload ID="fuSignature" runat="server" />
                        <asp:Button ID="btnUploadSignature" runat="server" Text="Yükle" CssClass="button secondary" OnClick="BtnUploadSignature_Click" />
                    </div>
                </div>
            </div>
        </div>

        <!-- Loading Overlay -->
        <div id="loadingOverlay" class="loading-overlay">
            <div class="loading-content">
                <div class="loading-spinner"></div>
                <p id="loadingMessage">İşlem yapılıyor...</p>
            </div>
        </div>

        <!-- Notification -->
        <div id="notification" class="notification">
            <span id="notificationMessage"></span>
        </div>

        <asp:HiddenField ID="hdnCurrentView" runat="server" Value="list" />
        <asp:HiddenField ID="hdnSelectedSignatures" runat="server" />
    </form>

    <script type="text/javascript">
        // JavaScript functions will go here
        function showLoading(message) {
            document.getElementById('loadingOverlay').style.display = 'flex';
            document.getElementById('loadingMessage').textContent = message || 'İşlem yapılıyor...';
        }

        function hideLoading() {
            document.getElementById('loadingOverlay').style.display = 'none';
        }

        function showNotification(message, type) {
            const notification = document.getElementById('notification');
            const notificationMessage = document.getElementById('notificationMessage');
            
            notification.className = 'notification show ' + (type || 'info');
            notificationMessage.textContent = message;
            
            setTimeout(() => {
                notification.classList.remove('show');
            }, 3000);
        }

        function switchView(view) {
            const listView = document.getElementById('listView');
            const detailView = document.getElementById('detailView');
            const hdnCurrentView = document.getElementById('<%= hdnCurrentView.ClientID %>');
            
            if (view === 'list') {
                listView.style.display = 'block';
                detailView.style.display = 'none';
                hdnCurrentView.value = 'list';
            } else {
                listView.style.display = 'none';
                detailView.style.display = 'block';
                hdnCurrentView.value = 'detail';
            }
        }

        // Initialize page
        document.addEventListener('DOMContentLoaded', function() {
            const currentView = document.getElementById('<%= hdnCurrentView.ClientID %>').value;
            switchView(currentView);

            // Initialize signature selection
            initializeSignatureSelection();
        });

        // Signature selection functionality
        let isSelecting = false;
        let startX, startY;
        let selectionBox = document.querySelector('.selection-box');

        function initializeSignatureSelection() {
            const imageWrapper = document.querySelector('.image-wrapper');
            if (!imageWrapper) return;

            imageWrapper.addEventListener('mousedown', startSelection);
            document.addEventListener('mousemove', updateSelection);
            document.addEventListener('mouseup', endSelection);
        }

        function startSelection(e) {
            isSelecting = true;
            const rect = e.target.getBoundingClientRect();
            startX = e.clientX - rect.left;
            startY = e.clientY - rect.top;

            selectionBox.style.left = startX + 'px';
            selectionBox.style.top = startY + 'px';
            selectionBox.style.width = '0';
            selectionBox.style.height = '0';
            selectionBox.style.display = 'block';
        }

        function updateSelection(e) {
            if (!isSelecting) return;

            const rect = document.querySelector('.image-wrapper').getBoundingClientRect();
            const currentX = e.clientX - rect.left;
            const currentY = e.clientY - rect.top;

            const width = Math.abs(currentX - startX);
            const height = Math.abs(currentY - startY);
            const left = Math.min(currentX, startX);
            const top = Math.min(currentY, startY);

            selectionBox.style.left = left + 'px';
            selectionBox.style.top = top + 'px';
            selectionBox.style.width = width + 'px';
            selectionBox.style.height = height + 'px';
        }

        function endSelection(e) {
            if (!isSelecting) return;
            isSelecting = false;

            // Save selection coordinates
            const selection = {
                left: parseInt(selectionBox.style.left),
                top: parseInt(selectionBox.style.top),
                width: parseInt(selectionBox.style.width),
                height: parseInt(selectionBox.style.height)
            };

            document.getElementById('<%= hdnSelectedSignatures.ClientID %>').value = JSON.stringify(selection);
        }

        // Tab switching
        document.querySelectorAll('.tab').forEach(tab => {
            tab.addEventListener('click', function() {
                document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
                this.classList.add('active');
                // TODO: Switch signature page
            });
        });
    </script>
</body>
</html>
