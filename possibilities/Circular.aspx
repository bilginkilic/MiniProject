<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Circular.aspx.cs" Inherits="AspxExamples.Circular" %>
<%-- Created: 2024.01.17 14:30 - v1ata --%>

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
            background-color: #f8f9fa;
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
            background: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }

        .list-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 25px;
            border-bottom: 2px solid #dee2e6;
            padding-bottom: 15px;
        }

        .toolbar {
            display: flex;
            gap: 10px;
        }

        .filter-panel {
            background: #fff;
            box-shadow: 0 1px 3px rgba(0,0,0,0.05);
            padding: 20px;
            border-radius: 6px;
            margin-bottom: 20px;
            border: 1px solid #ddd;
        }

        .filter-row {
            display: flex;
            gap: 20px;
            margin-bottom: 15px;
        }

        .filter-row:last-child {
            margin-bottom: 0;
        }

        .filter-group {
            flex: 1;
            display: flex;
            flex-direction: column;
            gap: 5px;
        }

        .filter-group label {
            color: #495057;
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .filter-group .form-control {
            padding: 8px 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
            width: 100%;
        }

        .date-range {
            flex: 2;
        }

        .date-inputs {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .date-inputs .form-control {
            flex: 1;
        }

        .date-inputs span {
            color: #666;
            font-weight: 500;
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
            background: #e9ecef;
            font-weight: 600;
            color: #495057;
            text-transform: uppercase;
            font-size: 12px;
            letter-spacing: 0.5px;
            border-bottom: 2px solid #dee2e6;
        }

        .circular-grid tr:hover {
            background: #f8f9fa;
            cursor: pointer;
        }

        /* Detail View Styles */
        .detail-view {
            display: none;
            background: #f8f9fa;
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
            background: #fff;
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
            min-width: 300px;
            max-width: 500px;
            transform: translateX(120%);
            transition: transform 0.3s ease;
        }

        .notification.show {
            transform: translateX(0);
            display: block;
        }

        .notification.success {
            border-left: 4px solid #28a745;
            background-color: #f0fff4;
        }

        .notification.error {
            border-left: 4px solid #dc3545;
            background-color: #fff5f5;
        }

        .notification.info {
            border-left: 4px solid #17a2b8;
            background-color: #f0f9ff;
        }

        .notification.warning {
            border-left: 4px solid #ffc107;
            background-color: #fffbeb;
        }

        .notification-content {
            display: flex;
            align-items: center;
            gap: 10px;
        }

        .notification-icon {
            font-size: 20px;
            line-height: 1;
        }

        .notification-message {
            flex: 1;
            font-size: 14px;
            line-height: 1.4;
        }

        .notification-close {
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

        .notification-close:hover {
            opacity: 1;
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
            background: #fff;
            border: 1px solid #ddd;
            border-radius: 4px;
            cursor: pointer;
        }

        .tab.active {
            background: #dc3545;
            color: white;
            border-color: #dc3545;
        }

        /* Modal Styles */
        .modal {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            width: 100vw;
            height: 100vh;
            background: rgba(0,0,0,0.5);
            z-index: 1000;
        }

        .modal.show {
            display: flex;
        }

        .modal-content {
            background: white;
            border-radius: 8px;
            width: 90%;
            max-width: 1200px;
            max-height: 90vh;
            display: flex;
            flex-direction: column;
            position: relative;
        }

        .modal-header {
            padding: 15px 20px;
            border-bottom: 1px solid #dee2e6;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: #f8f9fa;
        }

        .modal-header h2 {
            margin: 0;
            font-size: 18px;
            color: #333;
            font-weight: 600;
        }

        .modal-close {
            background: none;
            border: none;
            font-size: 28px;
            cursor: pointer;
            color: #666;
            padding: 0;
            width: 32px;
            height: 32px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            transition: all 0.2s ease;
        }

        .modal-close:hover {
            background: rgba(0,0,0,0.1);
            color: #333;
        }

        .modal-body {
            padding: 20px;
            overflow-y: auto;
            flex: 1;
        }

        .modal-footer {
            padding: 20px;
            border-top: 1px solid #dee2e6;
            display: flex;
            justify-content: flex-end;
            gap: 10px;
        }

        .modal-tabs {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
            border-bottom: 1px solid #dee2e6;
            padding-bottom: 10px;
        }

        .modal-tab {
            padding: 8px 16px;
            border: none;
            background: none;
            cursor: pointer;
            color: #666;
            font-weight: 500;
            position: relative;
        }

        .modal-tab.active {
            color: #dc3545;
        }

        .modal-tab.active::after {
            content: '';
            position: absolute;
            bottom: -11px;
            left: 0;
            width: 100%;
            height: 2px;
            background: #dc3545;
        }

        .modal-tab-content {
            display: none;
        }

        .modal-tab-content.active {
            display: block;
        }

        /* Preview Styles */
        .signature-auth-buttons {
            margin-bottom: 20px;
        }

        .selected-signatures-preview {
            background: #fff;
            padding: 20px;
            border-radius: 8px;
            border: 1px solid #ddd;
        }

        .preview-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
            gap: 20px;
            margin-top: 15px;
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
                    <h2>Sirküler Durumu</h2>
                    <div class="toolbar">
                        <asp:Button ID="btnExcel" runat="server" Text="Excel" CssClass="button secondary" OnClick="BtnExcel_Click" />
                        <asp:Button ID="btnNewCircular" runat="server" Text="Yeni Sirküler" CssClass="button primary" OnClick="BtnNewCircular_Click" />
                    </div>
                </div>

                <div class="filter-panel">
                    <div class="filter-row">
                        <div class="filter-group">
                            <label>Sirküler Durumu:</label>
                            <asp:DropDownList ID="ddlFilterDurum" runat="server" CssClass="form-control">
                                <asp:ListItem Text="Seçiniz" Value="" />
                                <asp:ListItem Text="Aktif" Value="Aktif" />
                                <asp:ListItem Text="Pasif" Value="Pasif" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-group">
                            <label>Müşteri No:</label>
                            <asp:TextBox ID="txtFilterMusteriNo" runat="server" CssClass="form-control" />
                        </div>
                        <div class="filter-group">
                            <label>Sirküler Ref:</label>
                            <asp:TextBox ID="txtFilterSirkulerRef" runat="server" CssClass="form-control" />
                        </div>
                    </div>
                    <div class="filter-row">
                        <div class="filter-group">
                            <label>Yetki Türleri:</label>
                            <asp:DropDownList ID="ddlFilterYetkiTurleri" runat="server" CssClass="form-control">
                                <asp:ListItem Text="Kredi İşlemleri" Value="Kredi İşlemleri" />
                                <asp:ListItem Text="Hazine İşlemleri" Value="Hazine İşlemleri" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-group">
                            <label>Yetki Şekli:</label>
                            <asp:DropDownList ID="ddlFilterYetkiSekli" runat="server" CssClass="form-control">
                                <asp:ListItem Text="Müştereken" Value="Müştereken" />
                                <asp:ListItem Text="Münferiden" Value="Münferiden" />
                            </asp:DropDownList>
                        </div>
                        <div class="filter-group date-range">
                            <label>Düzenleme Tarihi:</label>
                            <div class="date-inputs">
                                <asp:TextBox ID="txtFilterDuzenlemeTarihiBaslangic" runat="server" TextMode="Date" CssClass="form-control" />
                                <span>-</span>
                                <asp:TextBox ID="txtFilterDuzenlemeTarihiBitis" runat="server" TextMode="Date" CssClass="form-control" />
                            </div>
                        </div>
                    </div>
                    <div class="filter-row">
                        <div class="filter-group date-range">
                            <label>Geçerlilik Tarihi:</label>
                            <div class="date-inputs">
                                <asp:TextBox ID="txtFilterGecerlilikTarihiBaslangic" runat="server" TextMode="Date" CssClass="form-control" />
                                <span>-</span>
                                <asp:TextBox ID="txtFilterGecerlilikTarihiBitis" runat="server" TextMode="Date" CssClass="form-control" />
                            </div>
                        </div>
                        <div class="filter-group">
                            <label>Sirküler Tipi:</label>
                            <asp:DropDownList ID="ddlFilterSirkulerTipi" runat="server" CssClass="form-control">
                                <asp:ListItem Text="Ana Sirküler" Value="Ana Sirküler" />
                                <asp:ListItem Text="Ek Sirküler" Value="Ek Sirküler" />
                            </asp:DropDownList>
                        </div>
                    </div>
                </div>

                <asp:GridView ID="gvCirculars" runat="server" CssClass="circular-grid" AutoGenerateColumns="false" 
                    OnRowCommand="GvCirculars_RowCommand" OnRowDataBound="GvCirculars_RowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="Detail" ItemStyle-Width="50px">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkDetail" runat="server" CommandName="Detail" 
                                    CommandArgument='<%# Container.DataItemIndex %>' Text="Detail" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="SirkulerRef" HeaderText="Sirküler Ref." />
                        <asp:BoundField DataField="MusteriNo" HeaderText="Müşteri No" />
                        <asp:BoundField DataField="FirmaUnvani" HeaderText="Firma Ünvanı" />
                        <asp:BoundField DataField="DuzenlemeTarihi" HeaderText="Düzenleme Tarihi" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="GecerlilikTarihi" HeaderText="Geçerlilik Tarihi" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="SirkulerTipi" HeaderText="Sirküler Tipi" />
                        <asp:BoundField DataField="SirkulerNoterNo" HeaderText="Sirküler Noter No" />
                        <asp:BoundField DataField="OzelDurumlar" HeaderText="Özel Durumlar" />
                        <asp:BoundField DataField="SirkulerDurumu" HeaderText="Sirküler Durumu" />
                        <asp:BoundField DataField="SirkulerBelge" HeaderText="Sirküler Belge" />
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

                <!-- Signature and Auth Button -->
                <div class="signature-auth-buttons">
                    <button type="button" class="button primary" onclick="openSignatureModal()">
                        <i class="fas fa-signature"></i> İmza Seçimi ve Yetkili Listesi
                    </button>
                </div>

                <!-- Selected Signatures Preview -->
                <div class="selected-signatures-preview">
                    <h3>Seçili İmzalar ve Yetkililer</h3>
                    <div class="preview-grid">
                        <!-- Dinamik olarak doldurulacak -->
                    </div>
                </div>
            </div>
        </div>

        <!-- Signature and Auth Modal -->
        <div id="signatureAuthModal" class="modal">
            <div class="modal-content" style="width: 100%; height: 100%; max-width: none; max-height: none; border-radius: 0;">
                <div class="modal-header">
                    <h2>İmza Seçimi ve Yetkili Listesi</h2>
                    <button type="button" class="modal-close" onclick="closeSignatureModal()">&times;</button>
                </div>
                <div class="modal-body" style="flex: 1; padding: 0;">
                    <iframe id="signatureFrame" style="width: 100%; height: 100%; border: none;"></iframe>
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
            <button type="button" class="notification-close" onclick="hideNotification()">&times;</button>
            <div class="notification-content">
                <div class="notification-icon"></div>
                <div id="notificationMessage" class="notification-message"></div>
            </div>
        </div>

        <asp:HiddenField ID="hdnCurrentView" runat="server" Value="list" />
        <asp:HiddenField ID="hdnSelectedSignatures" runat="server" />
    </form>

    <script type="text/javascript">
        // Global variables and functions
        var currentView = 'list';
        var notificationTimeout;
        var switchView;

        // Modal Management
        function openSignatureModal() {
            const modal = document.getElementById('signatureAuthModal');
            const iframe = document.getElementById('signatureFrame');
            if (modal && iframe) {
                // Get current sirkuler ref from session
                const currentSirkulerRef = '<%= Session["CurrentSirkulerRef"] %>';
                
                // Set iframe src with ref parameter
                iframe.src = 'PdfSignatureForm.aspx' + (currentSirkulerRef ? '?ref=' + currentSirkulerRef : '');
                
                modal.classList.add('show');
                
                // Listen for messages from iframe
                window.addEventListener('message', handleSignatureMessage);
            }
        }

        function closeSignatureModal() {
            const modal = document.getElementById('signatureAuthModal');
            const iframe = document.getElementById('signatureFrame');
            if (modal) {
                modal.classList.remove('show');
                if (iframe) {
                    iframe.src = 'about:blank';
                }
                // Remove message listener
                window.removeEventListener('message', handleSignatureMessage);
            }
        }

        function handleSignatureMessage(event) {
            try {
                // Verify message origin
                if (event.origin !== window.location.origin) return;
                
                const data = event.data;
                if (data.type === 'SIGNATURE_SAVED') {
                    if (data.success) {
                        // Update preview with received data
                        updateSignaturePreview(data.data);
                        
                        // Close modal
                        closeSignatureModal();
                        
                        // Show success message
                        showNotification('İmza ve yetkili bilgileri kaydedildi', 'success');
                    } else {
                        showNotification('Kayıt sırasında hata oluştu: ' + (data.error || 'Bilinmeyen hata'), 'error');
                    }
                }
            } catch (err) {
                console.error('Error handling signature message:', err);
                showNotification('İşlem sırasında hata oluştu', 'error');
            }
        }

        function updateSignaturePreview(data) {
            try {
                const previewGrid = document.querySelector('.preview-grid');
                if (!previewGrid) return;

                previewGrid.innerHTML = '';
                
                if (data && data.gridState && data.gridState.current) {
                    data.gridState.current.forEach(record => {
                        if (record.Imzalar && record.Imzalar.length > 0) {
                            record.Imzalar.forEach(imza => {
                                const preview = document.createElement('div');
                                preview.className = 'signature-preview';
                                preview.style.backgroundImage = `url(${imza.ImageData})`;
                                preview.title = `${record.YetkiliAdi} - İmza ${imza.SiraNo}`;
                                previewGrid.appendChild(preview);
                            });
                        }
                    });
                }
            } catch (err) {
                console.error('Error updating signature preview:', err);
            }
        }

        // Tab Management
        document.addEventListener('DOMContentLoaded', function() {
            const modalTabs = document.querySelectorAll('.modal-tab');
            modalTabs.forEach(tab => {
                tab.addEventListener('click', function() {
                    const targetTab = this.getAttribute('data-tab');
                    
                    // Update tab states
                    modalTabs.forEach(t => t.classList.remove('active'));
                    this.classList.add('active');
                    
                    // Update content visibility
                    document.querySelectorAll('.modal-tab-content').forEach(content => {
                        content.classList.remove('active');
                    });
                    document.getElementById(targetTab + 'Tab').classList.add('active');
                });
            });
        });

        // Initialize all global functions
        (function initializeGlobalFunctions() {
            switchView = function(view) {
                try {
                    const listView = document.getElementById('listView');
                    const detailView = document.getElementById('detailView');
                    const hdnCurrentView = document.getElementById('<%= hdnCurrentView.ClientID %>');
                    
                    if (!listView || !detailView || !hdnCurrentView) {
                        console.error('Required elements not found');
                        return;
                    }

                    if (view === 'list') {
                        listView.style.display = 'block';
                        detailView.style.display = 'none';
                    } else if (view === 'detail') {
                        listView.style.display = 'none';
                        detailView.style.display = 'block';
                    }
                    
                    hdnCurrentView.value = view;
                    currentView = view;
                } catch (err) {
                    console.error('Error switching view:', err);
                    showNotification('Görünüm değiştirme sırasında hata oluştu', 'error');
                }
            };
        })();

        // View Management
        function switchView(view) {
            const listView = document.getElementById('listView');
            const detailView = document.getElementById('detailView');
            const hdnCurrentView = document.getElementById('<%= hdnCurrentView.ClientID %>');
            
            if (!listView || !detailView || !hdnCurrentView) {
                console.error('Required elements not found');
                return;
            }

            try {
                if (view === 'list') {
                    listView.style.display = 'block';
                    detailView.style.display = 'none';
                } else if (view === 'detail') {
                    listView.style.display = 'none';
                    detailView.style.display = 'block';
                }
                
                hdnCurrentView.value = view;
                currentView = view;
            } catch (err) {
                console.error('Error switching view:', err);
                showNotification('Görünüm değiştirme sırasında hata oluştu', 'error');
            }
        }

        // Notification System
        var notificationSystem = {
            icons: {
                success: '✓',
                error: '✕',
                warning: '⚠',
                info: 'ℹ'
            },
            show: function(message, type = 'info', duration = 5000) {
                try {
                    const notification = document.getElementById('notification');
                    const notificationMessage = document.getElementById('notificationMessage');
                    const notificationIcon = notification.querySelector('.notification-icon');
                    
                    if (!notification || !notificationMessage || !notificationIcon) {
                        console.error('Notification elements not found');
                        return;
                    }

                    // Clear any existing timeout
                    if (window.notificationTimeout) {
                        clearTimeout(window.notificationTimeout);
                    }

                    // Reset classes and add new ones
                    notification.className = 'notification';
                    notification.classList.add(type);

                    // Set icon and message
                    notificationIcon.textContent = this.icons[type] || this.icons.info;
                    notificationMessage.textContent = message;

                    // Show notification with animation
                    requestAnimationFrame(() => {
                        notification.classList.add('show');
                    });

                    // Auto hide after duration
                    if (duration > 0) {
                        window.notificationTimeout = setTimeout(() => {
                            this.hide();
                        }, duration);
                    }
                } catch (err) {
                    console.error('Error showing notification:', err);
                }
            },
            hide: function() {
                try {
                    const notification = document.getElementById('notification');
                    if (notification) {
                        notification.classList.remove('show');
                    }
                    if (window.notificationTimeout) {
                        clearTimeout(window.notificationTimeout);
                    }
                } catch (err) {
                    console.error('Error hiding notification:', err);
                }
            }
        };

        // Global notification functions
        function showNotification(message, type = 'info', duration = 5000) {
            notificationSystem.show(message, type, duration);
        }

        function hideNotification() {
            notificationSystem.hide();
        }

        function hideNotification() {
            try {
                const notification = document.getElementById('notification');
                if (notification) {
                    notification.classList.remove('show');
                }
                if (notificationTimeout) {
                    clearTimeout(notificationTimeout);
                }
            } catch (err) {
                console.error('Error hiding notification:', err);
            }
        }

        // Loading Management
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
            try {
                // Initialize view
                const viewValue = document.getElementById('<%= hdnCurrentView.ClientID %>').value;
                switchView(viewValue || 'list');

                // Initialize notification system
                const notification = document.getElementById('notification');
                const notificationMessage = document.getElementById('notificationMessage');
                if (!notification || !notificationMessage) {
                    console.error('Notification elements not found during initialization');
                }

                // Initialize loading overlay
                const loadingOverlay = document.getElementById('loadingOverlay');
                const loadingMessage = document.getElementById('loadingMessage');
                if (!loadingOverlay || !loadingMessage) {
                    console.error('Loading overlay elements not found during initialization');
                }

                // Initialize signature selection
                initializeSignatureSelection();

                console.log('Page initialized successfully');
            } catch (err) {
                console.error('Error during page initialization:', err);
                showNotification('Sayfa başlatılırken hata oluştu', 'error');
            }
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
