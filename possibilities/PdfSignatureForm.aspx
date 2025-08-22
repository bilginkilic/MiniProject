<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfSignatureForm.aspx.cs" Inherits="AspxExamples.PdfSignatureForm" %>
<%-- Created: yutkus kulaklık --%>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml" lang="tr">
<head runat="server">
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <meta name="description" content="İmza Sirkülerinden İmza Seçimi ve Yönetimi">
    <meta http-equiv="Content-Security-Policy" content="default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:;">
    <title>İmza Sirkülerinden İmza Seçimi</title>
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
            grid-template-columns: 1fr 350px;
            grid-template-rows: auto auto 1fr auto;
            grid-template-areas: 
                "header header"
                "main sidebar"
                "auth-details auth-details"
                "footer footer";
            box-sizing: border-box;
            min-width: 1200px;
            max-width: 1800px;
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
        .main-content {
            grid-area: main;
            display: flex;
            flex-direction: column;
            min-width: 0;
            border-right: 2px solid #eee;
            padding-right: 20px;
        }
        .sidebar {
            grid-area: sidebar;
            display: flex;
            flex-direction: column;
            gap: 20px;
            padding-left: 10px;
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
            display: flex;
            flex-direction: column;
            border: 2px solid #eee;
            border-radius: 8px;
            background: #fff;
            overflow: hidden;
            flex: 1;
            min-height: 0;
            width: 100%;
        }
        #pageContents {
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
            width: 100%;
            height: 100%;
        }
        .tab-content.active {
            display: flex;
            flex-direction: column;
            flex: 1;
        }
        .image-wrapper {
            width: 100%;
            height: 100%;
            overflow-y: auto;
            overflow-x: hidden;
            padding: 5px;
            box-sizing: border-box;
            display: flex;
            justify-content: center;
            align-items: flex-start;
            position: relative;
            background: #f8f9fa;
            flex: 1;
            min-width: 900px;
        }
        .image-wrapper img {
            width: 98%;
            min-width: 850px;
            height: auto;
            display: block;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            background: white;
            margin: 10px auto;
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
        /* Date Input Styles */
        .date-input {
            display: flex;
            align-items: center;
        }
        .date-input input[type="date"] {
            padding: 6px 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
            width: 200px;
        }
        .date-input input[type="date"]:disabled {
            background-color: #f5f5f5;
            cursor: not-allowed;
        }
        .auth-details {
            grid-area: auth-details;
            padding: 20px;
            background: #f8f9fa;
            border: 1px solid #eee;
            border-radius: 8px;
            margin-top: 0;
            display: block;
            width: 100%;
            height: auto;
        }
        .auth-details .form-grid {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 15px;
            margin-bottom: 20px;
        }
        .auth-details .form-row {
            display: grid;
            grid-template-columns: 150px 1fr;
            align-items: center;
            gap: 10px;
        }
        .auth-details label {
            color: #666;
            font-size: 13px;
            font-weight: 500;
        }
        .auth-details input,
        .auth-details select {
            padding: 6px 10px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
            width: 100%;
        }
        .auth-details .date-input {
            display: flex;
            gap: 5px;
            align-items: center;
        }
        .auth-details .date-input select {
            width: auto;
        }
        .auth-details-table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
            background: white;
        }
        .auth-details-table th,
        .auth-details-table td {
            padding: 10px;
            border: 1px solid #ddd;
            font-size: 13px;
            text-align: left;
        }
        .auth-details-table th {
            background: #f8f9fa;
            font-weight: 500;
            color: #333;
        }
        .auth-details-table tr:hover {
            background: #f8f9fa;
            cursor: pointer;
        }
        .auth-details-table tr.selected {
            background: #fff0f0;
            border-left: 3px solid #dc3545;
        }
        .auth-details-table tr.clicked {
            animation: rowClick 0.3s;
        }
        @keyframes rowClick {
            0% { background-color: #e3f2fd; }
            50% { background-color: #bbdefb; }
            100% { background-color: #e3f2fd; }
        }
        .signature-preview {
            width: 120px;
            height: 60px;
            border: 1px solid #ddd;
            border-radius: 4px;
            background-size: contain;
            background-repeat: no-repeat;
            background-position: center;
            background-color: white;
            margin: 0 auto;
        }
        .footer {
            grid-area: footer;
            padding: 15px 0;
            display: flex;
            justify-content: space-between;
            align-items: center;
            border-top: 2px solid #eee;
            margin-top: 0;
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

        /* Customer Search Modal Styles */
        .modal {
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
        .modal.show {
            display: flex;
        }
        .modal-content {
            background: white;
            padding: 20px;
            border-radius: 8px;
            width: 600px;
            max-width: 90%;
            max-height: 90vh;
            display: flex;
            flex-direction: column;
            position: relative;
        }
        .modal-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 1px solid #eee;
        }
        .modal-header h3 {
            margin: 0;
            color: #333;
        }
        .modal-close {
            background: none;
            border: none;
            font-size: 24px;
            cursor: pointer;
            color: #666;
            padding: 0;
        }
        .modal-search {
            display: flex;
            gap: 10px;
            margin-bottom: 20px;
        }
        .modal-search input {
            flex: 1;
            padding: 8px 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 14px;
        }
        .modal-table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
            max-height: 400px;
            overflow-y: auto;
            display: block;
        }
        .modal-table th,
        .modal-table td {
            padding: 10px;
            border: 1px solid #ddd;
            text-align: left;
        }
        .modal-table th {
            background: #f8f9fa;
            position: sticky;
            top: 0;
        }
        .modal-table tbody tr {
            cursor: pointer;
            transition: background 0.2s;
        }
        .modal-table tbody tr:hover {
            background: #f0f0f0;
        }
        .modal-table tbody tr.selected {
            background: #e3f2fd;
        }

        /* PDF List Panel Styles */
        .pdf-list-panel {
            padding: 15px;
            background: #f8f9fa;
            border-radius: 6px;
            border: 1px solid #eee;
        }

        .pdf-list-panel h3 {
            margin: 0 0 15px 0;
            color: #333;
            font-size: 16px;
        }

        .pdf-list {
            display: flex;
            flex-direction: column;
            gap: 8px;
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
            padding: 15px;
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            border: 1px solid #eee;
            margin-top: auto;
        }

        .selected-signatures h3 {
            margin: 0 0 15px 0;
            color: #333;
            font-size: 18px;
        }

        .signature-slots {
            display: flex;
            flex-direction: column;
            gap: 10px;
        }

        .signature-slot {
            width: 100%;
            height: 80px;
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

            <div class="main-content">
                <div id="imageContainer" runat="server" class="image-container">
                    <div class="tabs" id="pageTabs">
                        <!-- Tabs will be added here dynamically -->
                    </div>
                    <div id="pageContents">
                        <!-- Tab contents will be added here dynamically -->
                    </div>
                </div>
            </div>

            <div class="sidebar">
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
                        <asp:FileUpload ID="fuSignature" runat="server" accept=".pdf" />
                        <asp:Button ID="btnUpload" runat="server" Text="İmza Sirkülerini Yükle ve Göster" CssClass="button" OnClick="BtnUpload_Click" />
                        <asp:Button ID="btnShowPdf" runat="server" Text="" CssClass="button" style="display: none;" />
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
            </div>

            <!-- Yetki Detayları -->
            <div class="auth-details">
                <div class="form-grid">
                    <div class="form-row">
                        <label>YETKİLİ KONTAKT:</label>
                        <div style="display: flex; gap: 10px; align-items: center;">
                            <asp:TextBox runat="server" ID="txtYetkiliKontakt" Text="5000711" style="flex: 1;" />
                            <asp:TextBox runat="server" ID="txtYetkiliAdi" placeholder="Yetkili Adı" style="flex: 2;" />
                            <button type="button" id="btnYetkiliAra" class="button secondary" style="margin: 0;">
                                <i class="fas fa-search"></i> Ara
                            </button>
                        </div>
                    </div>
                    <div class="form-row">
                        <label>YETKİ GRUBU:</label>
                        <asp:DropDownList runat="server" ID="selYetkiGrubu">
                            <asp:ListItem Text="A Grubu" Value="A Grubu" />
                            <asp:ListItem Text="B Grubu" Value="B Grubu" />
                            <asp:ListItem Text="C Grubu" Value="C Grubu" />
                            <asp:ListItem Text="D Grubu" Value="D Grubu" />
                            <asp:ListItem Text="E Grubu" Value="E Grubu" />
                            <asp:ListItem Text="F Grubu" Value="F Grubu" />
                            <asp:ListItem Text="G Grubu" Value="G Grubu" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label for="txtYetkiTutari">YETKİ TUTARI:</label>
                        <asp:TextBox runat="server" ID="txtYetkiTutari" Text="1000000" 
                               TextMode="Number" step="0.01" min="0"
                               onkeypress="return isNumberKey(event)"
                               onpaste="return validatePaste(event)"
                               oninput="validateDecimalPlaces(this, 2)"
                               placeholder="Yetki tutarını giriniz" />
                    </div>
                    <div class="form-row">
                        <label>YETKİ BİTİŞ TARİHİ:</label>
                        <div class="date-input">
                            <asp:TextBox runat="server" ID="yetkiBitisTarihi" 
                                   TextMode="Date"
                                   CssClass="form-control"
                                   min="2024-01-01" 
                                   max="2030-12-31" />
                            <div style="display: flex; align-items: center; margin-left: 10px;">
                                <asp:CheckBox runat="server" ID="chkAksiKarar" 
                                       style="margin-right: 5px;" 
                                       onchange="handleAksiKararChange(this)" />
                                <label for="chkAksiKarar" style="font-weight: normal;">Aksi Karara Kadar</label>
                            </div>
                        </div>
                    </div>
                    <div class="form-row">
                        <label>SINIRLI YETKİ DETAYLARI:</label>
                        <asp:TextBox runat="server" ID="txtSinirliYetkiDetaylari" TextMode="MultiLine" Rows="3" style="resize: vertical; min-height: 60px;" />
                    </div>
                    <div class="form-row">
                        <label>YETKİ DÖVİZ CİNSİ:</label>
                        <asp:DropDownList runat="server" ID="selYetkiDovizCinsi">
                            <asp:ListItem Text="USD" Value="USD" />
                            <asp:ListItem Text="EUR" Value="EUR" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label>YETKİ ŞEKLİ:</label>
                        <asp:DropDownList runat="server" ID="selYetkiSekli">
                            <asp:ListItem Text="Müştereken" Value="Müştereken" />
                            <asp:ListItem Text="Müştereken1" Value="Müştereken1" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label>YETKİ TÜRLERİ:</label>
                        <asp:DropDownList runat="server" ID="selYetkiTurleri">
                            <asp:ListItem Text="Kredi İşlemleri, Hazine İşlemleri" Value="Kredi İşlemleri, Hazine İşlemleri" />
                            <asp:ListItem Text="Kredi İşlemleri2, Hazine İşlemleri1" Value="Kredi İşlemleri2, Hazine İşlemleri1" />
                        </asp:DropDownList>
                    </div>
                    <div class="form-row">
                        <label>YETKİ DURUMU:</label>
                        <asp:DropDownList runat="server" ID="selYetkiDurumu">
                            <asp:ListItem Text="Aktif" Value="Aktif" />
                            <asp:ListItem Text="Pasif" Value="Pasif" />
                        </asp:DropDownList>
                    </div>
                </div>

                <div style="display: flex; justify-content: flex-end; gap: 10px; margin-bottom: 10px;">
                    <button type="button" id="btnEkle" class="button">
                        <i class="fas fa-plus"></i> Ekle
                    </button>
                    <button type="button" id="btnSil" class="button secondary">
                        <i class="fas fa-trash"></i> Sil
                    </button>
                </div>

                <table class="auth-details-table">
                    <thead>
                        <tr>
                            <th>Yetkili Kont. No</th>
                            <th>Yetkili Adı Soyadı</th>
                            <th>Yetki Şekli</th>
                            <th>Yetki Süresi</th>
                            <th>Yetki Bitiş Tarihi</th>
                            <th>İmza Yetki Grubu</th>
                            <th>Sınırlı Yetki Detayları</th>
                            <th>Yetki Türleri</th>
                            <th>İmza Örneği 1</th>
                            <th>İmza Örneği 2</th>
                            <th>İmza Örneği 3</th>
                            <th>Yetki Tutarı</th>
                            <th>Yetki Döv.</th>
                            <th>Durum</th>
                        </tr>
                    </thead>
                    <tbody>
                       
                    </tbody>
                </table>
            </div>

            <div class="footer">
                <asp:HiddenField ID="hdnSelection" runat="server" />
                <asp:HiddenField ID="hdnPageCount" runat="server" />
                <asp:HiddenField ID="hdnSignatures" runat="server" />
                <asp:HiddenField ID="hdnYetkiliKayitlar" runat="server" />
                <asp:HiddenField ID="hdnIsReturnRequested" runat="server" Value="false" />
                <asp:HiddenField ID="hdnYetkiliImzaEslesmesi" runat="server" />
                <asp:Button ID="btnSaveSignature" runat="server" Text="Kaydet ve Geri Dön" 
                    CssClass="button" OnClientClick="return saveAndReturn();" />
                
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

        <!-- Customer Search Modal -->
        <div id="customerSearchModal" class="modal">
            <div class="modal-content">
                <div class="modal-header">
                    <h3>Müşteri Arama</h3>
                    <button type="button" class="modal-close" onclick="closeCustomerModal()">&times;</button>
                </div>
                <div class="modal-search">
                    <input type="text" id="customerSearchInput" placeholder="Müşteri no ile arama yapın..." />
                    <button type="button" class="button" onclick="searchCustomers()">
                        <i class="fas fa-search"></i> Ara
                    </button>
                </div>
                <div class="modal-table">
                    <table>
                        <thead>
                            <tr>
                                <th>Müşteri No</th>
                                <th>Müşteri Adı</th>
                                <th>Durum</th>
                            </tr>
                        </thead>
                        <tbody id="customerTableBody">
                            <!-- Müşteri listesi buraya dinamik olarak eklenecek -->
                        </tbody>
                    </table>
                </div>
            </div>
        </div>

        <script type="text/javascript">
            function saveAndReturn() {
                try {
                    console.group('Kaydet ve Geri Dön İşlemi');
                    console.log('Fonksiyon başladı');

                    // Grid state'i güncelle
                    updateGridState();
                    
                    // Kayıtları gridState'den al
                    var kayitlar = gridState.current;
                    console.log('Grid kayıtları:', kayitlar);

                    // İmza verilerini al
                    var signatures = [];
                    document.querySelectorAll('.signature-area').forEach(function(area) {
                        if (area.dataset.image) {
                            signatures.push({
                                Page: parseInt(area.dataset.page),
                                X: parseInt(area.dataset.x),
                                Y: parseInt(area.dataset.y),
                                Width: parseInt(area.dataset.width),
                                Height: parseInt(area.dataset.height),
                                Image: area.dataset.image
                            });
                        }
                    });
                    console.log('İmza verileri:', signatures);

                    // Debug için verileri konsola yazdır
                    console.log('Gönderilecek yetkili kayıtları:', kayitlar);
                    console.log('Gönderilecek imza verileri:', signatures);

                    // Veriyi hazırla
                    var requestData = {
                        yetkiliKayitlarJson: JSON.stringify(kayitlar),
                        signatureDataJson: JSON.stringify(signatures.map(function(sig) {
                            return {
                                Page: sig.Page,
                                X: sig.X,
                                Y: sig.Y,
                                Width: sig.Width,
                                Height: sig.Height,
                                Image: sig.Image
                            };
                        }))
                    };
                    console.log('Gönderilecek veri:', requestData);

                    // Fetch API ile çağrı
                    fetch('PdfSignatureForm.aspx/SaveSignatureWithAjax', {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json; charset=utf-8'
                        },
                        body: JSON.stringify(requestData)
                    })
                    .then(response => response.json())
                    .then(data => {
                        console.log('Başarılı yanıt:', data);
                        if (data.d && data.d.success) {
                            showNotification('Veriler başarıyla kaydedildi', 'success');
                            setTimeout(() => {
                                window.location.href = document.referrer || '/';
                            }, 1000);
                        } else {
                            showNotification(data.d.error || 'Bilinmeyen bir hata oluştu', 'error');
                        }
                    })
                    .catch(error => {
                        console.error('Fetch hatası:', error);
                        showNotification('İşlem sırasında bir hata oluştu: ' + error.message, 'error');
                    });

                    return false; // Form submit'i engelle
                } catch (err) {
                    console.error('Kaydet ve geri dön hatası:', err);
                    showNotification('İşlem sırasında bir hata oluştu: ' + err.message, 'error');
                    return false;
                }
            }

            // Global notification functions
            window.notificationTimer = null;

            window.showNotification = function(message, type) {
                try {
                    var notification = document.getElementById('notification');
                    var notificationMessage = document.getElementById('notificationMessage');
                    
                    if (!notification || !notificationMessage) {
                        console.error('Notification elements not found');
                        return;
                    }
                    
                    clearTimeout(window.notificationTimer);
                    notification.className = 'notification';
                    if (type) notification.classList.add(type);
                    notificationMessage.textContent = message || '';
                    notification.classList.add('show');
                    
                    window.notificationTimer = setTimeout(function() {
                        window.hideNotification();
                    }, type === 'error' ? 8000 : 5000);
                } catch (err) {
                    console.error('Show notification error:', err);
                }
            };
            
            window.hideNotification = function() {
                try {
                    var notification = document.getElementById('notification');
                    if (notification) {
                        notification.classList.remove('show');
                    }
                    clearTimeout(window.notificationTimer);
                } catch (err) {
                    console.error('Hide notification error:', err);
                }
            };
            
            // Initialize application
            document.addEventListener('DOMContentLoaded', function() {
                // Basic initialization checks
                var notification = document.getElementById('notification');
                var notificationMessage = document.getElementById('notificationMessage');
                
                if (!notification || !notificationMessage) {
                    console.error('Notification elements not found during initialization');
                }
            });

            const ErrorHandler = {
                handle: function(error, context) {
                    console.error(`[${context}] Error:`, error);
                    const message = this.getUserFriendlyMessage(error);
                    showNotification(message, 'error');
                },
                getUserFriendlyMessage: function(error) {
                    const errorMessages = {
                        'NETWORK_ERROR': 'Bağlantı hatası oluştu. Lütfen internet bağlantınızı kontrol edin.',
                        'VALIDATION_ERROR': 'Lütfen tüm zorunlu alanları doldurun.',
                        'FILE_ERROR': 'Dosya işlemi sırasında hata oluştu.',
                        'SAVE_ERROR': 'Kaydetme işlemi sırasında hata oluştu.',
                        'AUTH_ERROR': 'Yetkilendirme hatası oluştu.',
                        'PARSE_ERROR': 'Sunucu yanıtı işlenirken hata oluştu.'
                    };
                    return errorMessages[error.code] || error.message || 'Beklenmeyen bir hata oluştu.';
                }
            };

            const SecurityManager = {
                csrfToken: null,
                init: function() {
                    const tokenElement = document.querySelector('input[name="__RequestVerificationToken"]');
                    this.csrfToken = tokenElement ? tokenElement.value : null;
                },
                addCsrfToFormData: function(formData) {
                    if (this.csrfToken) {
                        formData.append('__RequestVerificationToken', this.csrfToken);
                    }
                    return formData;
                },
                sanitizeInput: function(input) {
                    return input.replace(/[<>]/g, '');
                }
            };

            const Validator = {
                isValidSignature: function(signature) {
                    return signature 
                        && typeof signature.x === 'number'
                        && typeof signature.y === 'number'
                        && typeof signature.width === 'number'
                        && typeof signature.height === 'number'
                        && signature.width > 0
                        && signature.height > 0;
                },
                
                validateFormData: function(data) {
                    const errors = [];
                    if (!data.yetkiliKontakt?.trim()) {
                        errors.push('Yetkili kontakt zorunludur');
                    }
                    if (!data.yetkiliAdi?.trim()) {
                        errors.push('Yetkili adı zorunludur');
                    }
                    const yetkiTutari = document.getElementById('txtYetkiTutari')?.value;
                    if (!yetkiTutari || isNaN(parseFloat(yetkiTutari))) {
                        errors.push('Geçerli bir yetki tutarı giriniz');
                    }
                    return errors;
                }
            };

            const Utils = {
                debounce: function(func, wait) {
                    let timeout;
                    return function executedFunction(...args) {
                        const later = () => {
                            clearTimeout(timeout);
                            func(...args);
                        };
                        clearTimeout(timeout);
                        timeout = setTimeout(later, wait);
                    };
                },
                
                lazyLoadImages: function() {
                    const imageObserver = new IntersectionObserver((entries, observer) => {
                        entries.forEach(entry => {
                            if (entry.isIntersecting) {
                                const img = entry.target;
                                img.src = img.dataset.src;
                                observer.unobserve(img);
                            }
                        });
                    });

                    document.querySelectorAll('img[data-src]').forEach(img => {
                        imageObserver.observe(img);
                    });
                }
            };

            /* Created: 2024.01.17 14:30 */

            // Initialize application
            document.addEventListener('DOMContentLoaded', function() {
                try {
                    // Basic initialization checks
                    const notification = document.getElementById('notification');
                    const notificationMessage = document.getElementById('notificationMessage');
                    
                    if (!notification || !notificationMessage) {
                        console.error('Notification elements not found during initialization');
                        return;
                    }

                    console.log('Application initialized successfully');
                } catch (error) {
                    console.error('Application initialization failed:', error);
                }
            });

            // Application functions
            function showNotification(message, type, persistent) {
                try {
                    console.log('Notification:', type, message);
                    const notification = document.getElementById('notification');
                    const notificationMessage = document.getElementById('notificationMessage');
                    
                    if (!notification || !notificationMessage) {
                        console.error('Notification elements not found');
                        return;
                    }
                    
                    // Reset classes
                    notification.className = 'notification';
                    notification.classList.add(type || 'info');
                    if (persistent) {
                        notification.classList.add('persistent');
                    }
                    notificationMessage.textContent = message ? message.toString() : '';
                    
                    notification.classList.add('show');
                    
                    // For non-persistent notifications, auto-hide after delay
                    if (!persistent) {
                        setTimeout(function() {
                            hideNotification();
                        }, type === 'error' ? 8000 : 5000); // Show errors longer
                    }
                } catch (err) {
                    console.error('Notification error:', err);
                }
            }

            function hideNotification() {
                try {
                    const notification = document.getElementById('notification');
                    if (notification) {
                        notification.classList.remove('show');
                        notification.classList.remove('persistent');
                    }
                } catch (err) {
                    console.error('Hide notification error:', err);
                }
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
                console.log('startSelection başladı');
                
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
                        deleteBtn.onclick = () => deleteSignature(index);
                    } else {
                        slot.classList.remove('filled');
                        slotImage.style.backgroundImage = '';
                        deleteBtn.style.display = 'none';
                    }
                });
                
                // Enable save button - grid'de data varsa her zaman aktif olmalı
                const tbody = document.querySelector('.auth-details-table tbody');
                const hasGridData = tbody && tbody.children.length > 0;
                btnSave.disabled = false; // Grid'de data varsa veya imza seçilmişse aktif
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

            let selectedRow = null;
            let isEditing = false;

            function initializeImageEvents() {
                var img = document.querySelector('.tab-content.active img');
                if (img) {
                    var imageWrapper = document.querySelector('.tab-content.active .image-wrapper');
                    imageWrapper.addEventListener('mousedown', startSelection);
                }

                // Grid satır çift tıklama olayı
                document.querySelectorAll('.auth-details-table tbody tr').forEach(row => {
                    row.addEventListener('dblclick', () => handleRowDoubleClick(row));
                });

                // Ekle/Güncelle butonu olayı
                document.getElementById('btnEkle').addEventListener('click', handleAddUpdate);
                
                // Sil butonu olayı
                document.getElementById('btnSil').addEventListener('click', handleDelete);

                // Yetkili arama butonu olayı
                document.getElementById('btnYetkiliAra').addEventListener('click', handleYetkiliArama);
            }

            function selectRow(row) {
                // Önceki seçili satırın seçimini kaldır
                document.querySelectorAll('.auth-details-table tbody tr').forEach(r => {
                    r.classList.remove('selected');
                });
                
                // Yeni satırı seç
                row.classList.add('selected');
                selectedRow = row;
            }

            function handleRowDoubleClick(row) {
                // Çift tıklama animasyonu
                row.classList.add('clicked');
                setTimeout(() => row.classList.remove('clicked'), 300);

                selectRow(row);
                isEditing = true;
                
                // Form alanlarını doldur
                                    // Form alanlarını doldur
                    document.getElementById('txtYetkiliKontakt').value = row.cells[0].textContent;
                    document.getElementById('txtYetkiliAdi').value = row.cells[1].textContent;
                    document.getElementById('selYetkiSekli').value = row.cells[2].textContent;
                    document.getElementById('selYetkiGrubu').value = row.cells[5].textContent;
                    document.getElementById('txtSinirliYetkiDetaylari').value = row.cells[6].textContent;
                    document.getElementById('selYetkiTurleri').value = row.cells[7].textContent;
                    document.getElementById('txtYetkiTutari').value = row.cells[11].textContent;
                    document.getElementById('selYetkiDovizCinsi').value = row.cells[12].textContent;
                    document.getElementById('selYetkiDurumu').value = row.cells[13].textContent;
                
                // Tarih değerini ayarla
                const tarihText = row.cells[3].textContent;
                const dateInput = document.getElementById('yetkiBitisTarihi');
                
                if (tarihText === 'Aksi Karara Kadar') {
                    document.getElementById('chkAksiKarar').checked = true;
                    handleAksiKararChange(document.getElementById('chkAksiKarar'));
                } else {
                    // "dd.mm.yyyy" formatından "yyyy-mm-dd" formatına çevir
                    const [day, month, year] = tarihText.split('.');
                    dateInput.value = `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
                    document.getElementById('chkAksiKarar').checked = false;
                }
                
                // Diğer alanları doldur
                document.querySelector('textarea[name="sinirliYetkiDetaylari"]').value = row.cells[6].textContent;
                document.querySelector('select[name="yetkiTurleri"]').value = row.cells[7].textContent;
                
                // İmzaları yüklemeye gerek yok, sadece form verilerini doldur

                                    // Ekle butonunu Güncelle olarak değiştir
                    const btnEkle = document.getElementById('btnEkle');
                    if (btnEkle) {
                        btnEkle.innerHTML = '<i class="fas fa-save"></i> Güncelle';
                        btnEkle.classList.add('update-mode');
                        btnEkle.setAttribute('data-original-text', '<i class="fas fa-plus"></i> Ekle');
                    }
            }

            function handleAddUpdate() {
                try {
                    console.log('handleAddUpdate başladı');
                    const btnEkle = document.getElementById('btnEkle');
                    if (!btnEkle) {
                        throw new Error('btnEkle elementi bulunamadı');
                    }
                    console.log('btnEkle bulundu:', btnEkle);
                    let isUpdate = false;
                    if (btnEkle) {
                        isUpdate = btnEkle.classList.contains('update-mode');
                    }
                
                    // Form verilerini kontrol et
                    const yetkiliKontakt = document.getElementById('txtYetkiliKontakt')?.value?.trim();
                    const yetkiliAdi = document.getElementById('txtYetkiliAdi')?.value?.trim();
                    const yetkiTutari = document.getElementById('txtYetkiTutari')?.value;
                    const yetkiTutariNum = parseFloat(yetkiTutari);
                    const imzalar = [];
                    
                    document.querySelectorAll('.signature-slot').forEach(slot => {
                        if (slot.classList.contains('filled')) {
                            const slotImage = slot.querySelector('.slot-image');
                            if (slotImage && slotImage.style.backgroundImage) {
                                imzalar.push(slotImage.style.backgroundImage);
                            }
                        }
                    });

                    // Zorunlu alan kontrolü
                    if (!yetkiliKontakt || !yetkiliAdi) {
                        showNotification('Lütfen yetkili kontakt ve adı alanlarını doldurun', 'warning');
                        return;
                    }

                    if (!yetkiTutari || isNaN(yetkiTutariNum) || yetkiTutariNum <= 0) {
                        showNotification('Lütfen geçerli bir yetki tutarı girin', 'warning');
                        return;
                    }
                    
                    // Ondalık basamak kontrolü
                    if (yetkiTutari.includes('.')) {
                        const decimalPlaces = yetkiTutari.split('.')[1].length;
                        if (decimalPlaces > 2) {
                            showNotification('Yetki tutarı en fazla 2 ondalık basamak içerebilir', 'warning');
                            return;
                        }
                    }

                    if (imzalar.length === 0) {
                        showNotification('Lütfen en az bir imza seçin', 'warning');
                        return;
                    }

                    // Tarih kontrolü
                    const isAksiKarar = document.getElementById('chkAksiKarar').checked;
                    const today = new Date();
                    const yetkiTarihi = today.getDate() + '.' + (today.getMonth() + 1) + '.' + today.getFullYear();
                    const yetkiBitisTarihi = isAksiKarar ? 
                        '31.12.2050' : 
                        document.querySelector('select[name="gun"]').value + '.' + 
                        document.querySelector('select[name="ay"]').value + '.' + 
                        document.querySelector('select[name="yil"]').value;
                
                    // Yeni kayıt veya güncelleme için veri hazırla
                    const formData = {
                        YetkiliKontakt: yetkiliKontakt,
                        YetkiliAdi: yetkiliAdi,
                        YetkiSekli: document.querySelector('select[name="yetkiSekli"]')?.value || 'Müştereken',
                        YetkiTarihi: yetkiTarihi,
                        YetkiBitisTarihi: yetkiBitisTarihi,
                        SinirliYetkiDetaylari: document.querySelector('textarea[name="sinirliYetkiDetaylari"]')?.value || '',
                        YetkiTurleri: document.querySelector('select[name="yetkiTurleri"]')?.value || '',
                        YetkiTutari: yetkiTutariNum.toFixed(2),
                        YetkiDovizCinsi: document.querySelector('select[name="yetkiDovizCinsi"]')?.value || 'USD',
                        YetkiDurumu: 'Aktif',
                        Imzalar: imzalar
                    };

                    if(isUpdate) {
                        if(!selectedRow) {
                            throw new Error('Güncellenecek satır seçilmedi');
                        }

                        // Satırı güncelle
                        updateTableRow(selectedRow, formData);
                        btnEkle.innerHTML = '<i class="fas fa-plus"></i> Ekle';
                        btnEkle.classList.remove('update-mode');
                        selectedRow = null;
                        showNotification('Kayıt başarıyla güncellendi', 'success');
                    } else {
                        // Yeni satır ekle
                        addTableRow(formData);
                        showNotification('Yeni kayıt eklendi', 'success');
                    }

                    // Formu temizle
                    clearForm();
                    
                } catch (err) {
                    console.error('handleAddUpdate hatası:', err);
                    showNotification(err.message || 'İşlem sırasında bir hata oluştu', 'error');
                }

            // Silinen handleFormSubmit fonksiyonu ve tekrar eden kod
            }

            // Grid state tracking
            let gridState = {
                added: [],
                updated: [],
                deleted: [],
                current: []
            };

            function getRowData(row) {
                const signaturePreviews = Array.from(row.querySelectorAll('.signature-preview'))
                    .map(preview => preview.style.backgroundImage)
                    .map(bgImage => bgImage.replace(/^url\(['"](.+)['"]\)$/, '$1') || '');

                return {
                    yetkiliKontakt: row.cells[0].textContent,
                    yetkiliAdi: row.cells[1].textContent,
                    yetkiSekli: row.cells[2].textContent,
                    yetkiTarihi: row.cells[3].textContent,
                    yetkiBitisTarihi: row.cells[4].textContent,
                    yetkiGrubu: row.cells[5].textContent,
                    sinirliYetkiDetaylari: row.cells[6].textContent,
                    yetkiTurleri: row.cells[7].textContent,
                    imzalar: signaturePreviews,
                    yetkiTutari: row.cells[11].textContent,
                    yetkiDovizCinsi: row.cells[12].textContent,
                    yetkiDurumu: row.cells[13].textContent
                };
            }

            function trackDeletedRow(rowData) {
                gridState.deleted.push(rowData);
            }

            function updateGridState() {
                const tbody = document.querySelector('.auth-details-table tbody');
                if (!tbody) return;

                gridState.current = Array.from(tbody.rows).map(row => getRowData(row));
            }

            function handleDelete() {
                try {
                    if(!selectedRow) {
                        throw new Error('Silinecek kayıt seçilmedi');
                    }

                    if(confirm('Seçili kaydı silmek istediğinize emin misiniz?')) {
                        // Store deleted row data before removing
                        const deletedRowData = getRowData(selectedRow);
                        trackDeletedRow(deletedRowData);

                        selectedRow.remove();
                        clearForm();
                        showNotification('Kayıt silindi', 'success');

                        // Enable save button after grid operation
                        const btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
                        if (btnSave) {
                            btnSave.disabled = false;
                        }

                        // Update grid state tracking
                        updateGridState();
                    }
                } catch (err) {
                    console.error('Silme işlemi hatası:', err);
                    showNotification(err.message || 'Silme işlemi sırasında bir hata oluştu', 'error');
                } finally {
                    const btnEkle = document.getElementById('btnEkle');
                    if (btnEkle) {
                        btnEkle.classList.remove('update-mode');
                        btnEkle.classList.remove('adding-mode');
                        btnEkle.innerHTML = '<i class="fas fa-plus"></i> Ekle';
                    }
                }
            }

            function handleYetkiliArama() {
                try {
                    // Müşteri arama modalını aç
                    document.getElementById('customerSearchModal').classList.add('show');
                    document.getElementById('customerSearchInput').focus();
                } catch (err) {
                    console.error('Yetkili arama hatası:', err);
                    showNotification(err.message || 'Arama sırasında bir hata oluştu', 'error');
                }
            }

            function closeCustomerModal() {
                document.getElementById('customerSearchModal').classList.remove('show');
            }

            function searchCustomers() {
                try {
                    const searchInput = document.getElementById('customerSearchInput');
                    if (!searchInput) {
                        throw new Error('Arama alanı bulunamadı');
                    }

                    const searchTerm = searchInput.value.trim();
                    if (!searchTerm) {
                        showNotification('Lütfen bir arama terimi girin', 'warning');
                        return;
                    }

                    showLoading('Müşteriler aranıyor...');

                    // TODO: Web servis çağrısı burada yapılacak
                    // Şimdilik dummy data
                    setTimeout(() => {
                        const dummyData = [
                            { no: '1001', name: 'Test Müşteri 1', status: 'Aktif' },
                            { no: '1002', name: 'Test Müşteri 2', status: 'Aktif' },
                            { no: '1003', name: 'Test Müşteri 3', status: 'Pasif' }
                        ];

                        updateCustomerTable(dummyData);
                        hideLoading();
                    }, 500);

                } catch (err) {
                    console.error('Müşteri arama hatası:', err);
                    showNotification(err.message || 'Arama sırasında bir hata oluştu', 'error');
                    hideLoading();
                }
            }

            function updateCustomerTable(customers) {
                try {
                    const tbody = document.getElementById('customerTableBody');
                    if (!tbody) {
                        throw new Error('Müşteri tablosu bulunamadı');
                    }

                    tbody.innerHTML = '';
                    customers.forEach(customer => {
                        const row = document.createElement('tr');
                        row.innerHTML = `
                            <td>${customer.no}</td>
                            <td>${customer.name}</td>
                            <td>${customer.status}</td>
                        `;
                        row.onclick = () => selectCustomer(customer);
                        tbody.appendChild(row);
                    });

                } catch (err) {
                    console.error('Müşteri tablosu güncelleme hatası:', err);
                    showNotification(err.message || 'Tablo güncellenirken bir hata oluştu', 'error');
                }
            }

            function selectCustomer(customer) {
                try {
                    // Yetkili alanlarını doldur
                    document.getElementById('txtYetkiliKontakt').value = customer.no;
                    document.getElementById('txtYetkiliAdi').value = customer.name;

                    // Modalı kapat
                    closeCustomerModal();
                    showNotification('Müşteri seçildi', 'success');

                } catch (err) {
                    console.error('Müşteri seçme hatası:', err);
                    showNotification(err.message || 'Müşteri seçilirken bir hata oluştu', 'error');
                }
            }

            function updateTableRow(row, data) {
                if (!row || !row.cells) {
                    window.showNotification('Geçersiz tablo satırı', 'error');
                    return;
                }

                if (!data) {
                    window.showNotification('Güncellenecek veri bulunamadı', 'error');
                    return;
                }

                try {
                    // Temel alanları güncelle
                    const updates = [
                        { index: 0, value: data.yetkiliKontakt },
                        { index: 1, value: data.yetkiliAdi },
                        { index: 2, value: data.yetkiSekli },
                        { index: 3, value: data.yetkiTarihi },
                        { index: 6, value: data.sinirliYetkiDetaylari },
                        { index: 7, value: data.yetkiTurleri }
                    ];

                    updates.forEach(update => {
                        if (row.cells[update.index]) {
                            row.cells[update.index].textContent = update.value || '';
                        }
                    });

                    // İmzaları güncelle
                    if (data.imzalar && Array.isArray(data.imzalar)) {
                        data.imzalar.forEach((imza, index) => {
                            const cell = row.cells[index + 8];
                            if (cell) {
                                const signaturePreview = cell.querySelector('.signature-preview');
                                if (signaturePreview) {
                                    signaturePreview.style.backgroundImage = imza || '';
                                }
                            }
                        });
                    }

                    // Enable save button after grid operation
                    const btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
                    if (btnSave) {
                        btnSave.disabled = false;
                    }

                    // Update grid state tracking
                    updateGridState();

                    // Yetkili kayıtlarını güncelle
                    updateYetkiliKayitlar();
                } catch (err) {
                    console.error('Satır güncelleme hatası:', err);
                    showNotification(err.message || 'Satır güncellenirken bir hata oluştu', 'error');
                    throw err;
                }
            }

            function updateYetkiliKayitlar() {
                try {
                    const tbody = document.querySelector('.auth-details-table tbody');
                    if (!tbody) return;

                    const kayitlar = [];
                    const rows = tbody.querySelectorAll('tr');
                    
                    rows.forEach((row, index) => {
                        const imzalar = [];
                        // İmza hücrelerini kontrol et (8, 9, 10 indeksli hücreler)
                        for(let i = 8; i <= 10; i++) {
                            const signaturePreview = row.cells[i]?.querySelector('.signature-preview');
                            if (signaturePreview && signaturePreview.style.backgroundImage) {
                                imzalar.push({
                                    Base64Image: signaturePreview.style.backgroundImage.replace(/^url\(['"](.+)['"]\)$/, '$1'),
                                    SlotIndex: i - 8
                                });
                            }
                        }

                        const kayit = {
                            YetkiliKontakt: row.cells[0].textContent,
                            YetkiliAdi: row.cells[1].textContent,
                            YetkiSekli: row.cells[2].textContent,
                            YetkiTarihi: row.cells[3].textContent,
                            AksiKararaKadar: row.cells[4].textContent === 'Aksi Karara Kadar',
                            YetkiGrubu: row.cells[5].textContent,
                            SinirliYetkiDetaylari: row.cells[6].textContent,
                            YetkiTurleri: row.cells[7].textContent,
                            Imzalar: imzalar,
                            YetkiTutari: row.cells[11].textContent,
                            YetkiDovizCinsi: row.cells[12].textContent,
                            YetkiDurumu: row.cells[13].textContent
                        };
                        kayitlar.push(kayit);
                    });

                    // Hidden field'ları güncelle
                    var hdnYetkiliKayitlar = document.getElementById('<%= hdnYetkiliKayitlar.ClientID %>');
                    if (hdnYetkiliKayitlar) {
                        hdnYetkiliKayitlar.value = JSON.stringify(kayitlar);
                        console.log('Yetkili kayıtları güncellendi:', kayitlar);
                    } else {
                        console.error('hdnYetkiliKayitlar elementi bulunamadı');
                    }
                    
                    // İmza eşleşmelerini güncelle
                    updateImzaEslesmesi();
                } catch (err) {
                    console.error('Yetkili kayıtları güncelleme hatası:', err);
                    showNotification('Yetkili kayıtları güncellenirken bir hata oluştu', 'error');
                }
            }

            function updateImzaEslesmesi() {
                try {
                    const tbody = document.querySelector('.auth-details-table tbody');
                    if (!tbody) return;

                    const eslesmeler = {};
                    const rows = tbody.querySelectorAll('tr');
                    
                    rows.forEach((row, rowIndex) => {
                        // İmza hücrelerini kontrol et (8, 9, 10 indeksli hücreler)
                        for(let i = 8; i <= 10; i++) {
                            const signaturePreview = row.cells[i]?.querySelector('.signature-preview');
                            if (signaturePreview && signaturePreview.style.backgroundImage) {
                                // İmza indeksini bul
                                const imzaIndex = selectedSignatures.findIndex(sig => 
                                    sig.image === signaturePreview.style.backgroundImage.replace(/^url\(['"](.+)['"]\)$/, '$1')
                                );
                                if (imzaIndex !== -1) {
                                    eslesmeler[rowIndex] = imzaIndex;
                                }
                            }
                        }
                    });

                    document.getElementById('<%= hdnYetkiliImzaEslesmesi.ClientID %>').value = JSON.stringify(eslesmeler);
                } catch (err) {
                    console.error('İmza eşleşmeleri güncelleme hatası:', err);
                    showNotification('İmza eşleşmeleri güncellenirken bir hata oluştu', 'error');
                }
            }

            function addTableRow(data) {
                try {
                    const tbody = document.querySelector('.auth-details-table tbody');
                    if (!tbody) return null;
                    const row = tbody.insertRow(0);
                    const allCells = [
                        data.yetkiliKontakt || '',
                        data.yetkiliAdi || '',
                        document.getElementById('selYetkiSekli').value || 'Müştereken',
                        data.yetkiTarihi || '',
                        data.yetkiTarihi || '',
                        document.getElementById('selYetkiGrubu').value || 'A Grubu',
                        document.getElementById('txtSinirliYetkiDetaylari').value || '',
                        document.getElementById('selYetkiTurleri').value || ''
                    ];

                    // Temel hücreleri ekle
                    allCells.forEach(cellData => {
                        const cell = row.insertCell();
                        cell.textContent = cellData;
                    });

                    // İmza hücreleri
                    for(let i = 0; i < 3; i++) {
                        const cell = row.insertCell();
                        const signaturePreview = document.createElement('div');
                        signaturePreview.className = 'signature-preview';
                        if(data.imzalar && data.imzalar[i]) {
                            signaturePreview.style.backgroundImage = data.imzalar[i];
                        }
                        cell.appendChild(signaturePreview);
                    }

                    // Son hücreleri ekle
                    [
                        document.getElementById('txtYetkiTutari').value || '100.000',
                        document.getElementById('selYetkiDovizCinsi').value || 'USD',
                        document.getElementById('selYetkiDurumu').value || 'Aktif'
                    ].forEach(text => {
                        const cell = row.insertCell();
                        cell.textContent = text;
                    });

                    // Event listener'ları ekle
                    row.addEventListener('dblclick', () => handleRowDoubleClick(row));
                    row.addEventListener('click', () => selectRow(row));

                    // Enable save button after grid operation
                    const btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
                    if (btnSave) {
                        btnSave.disabled = false;
                    }

                    // Update grid state tracking
                    updateGridState();

                    // Yetkili kayıtlarını güncelle
                    updateYetkiliKayitlar();

                    return row;
                } catch (err) {
                    console.error('Satır ekleme hatası:', err);
                    showNotification(err.message || 'Satır eklenirken bir hata oluştu', 'error');
                    return null;
                }
            }

            function handleAddUpdate() {
                try {
                    console.log('handleAddUpdate başladı');
                    const btnEkle = document.getElementById('btnEkle');
                    if (!btnEkle) {
                        throw new Error('btnEkle elementi bulunamadı');
                    }
                    console.log('btnEkle bulundu:', btnEkle);
                    let isUpdate = false;
                    if (btnEkle) {
                        isUpdate = btnEkle.classList.contains('update-mode');
                    }
                
                    // Form verilerini kontrol et
                    const yetkiliKontakt = document.getElementById('txtYetkiliKontakt')?.value?.trim();
                    const yetkiliAdi = document.getElementById('txtYetkiliAdi')?.value?.trim();
                    const yetkiTutari = document.getElementById('txtYetkiTutari')?.value;
                    const yetkiTutariNum = parseFloat(yetkiTutari);
                    const imzalar = [];
                    
                    document.querySelectorAll('.signature-slot').forEach(slot => {
                        if (slot.classList.contains('filled')) {
                            const slotImage = slot.querySelector('.slot-image');
                            if (slotImage && slotImage.style.backgroundImage) {
                                imzalar.push(slotImage.style.backgroundImage);
                            }
                        }
                    });

                    // Zorunlu alan kontrolü
                    if (!yetkiliKontakt || !yetkiliAdi) {
                        showNotification('Lütfen yetkili kontakt ve adı alanlarını doldurun', 'warning');
                        return;
                    }

                    if (!yetkiTutari || isNaN(yetkiTutariNum) || yetkiTutariNum <= 0) {
                        showNotification('Lütfen geçerli bir yetki tutarı girin', 'warning');
                        return;
                    }
                    
                    // Ondalık basamak kontrolü
                    if (yetkiTutari.includes('.')) {
                        const decimalPlaces = yetkiTutari.split('.')[1].length;
                        if (decimalPlaces > 2) {
                            showNotification('Yetki tutarı en fazla 2 ondalık basamak içerebilir', 'warning');
                            return;
                        }
                    }

                    if (imzalar.length === 0) {
                        showNotification('Lütfen en az bir imza seçin', 'warning');
                        return;
                    }

                    // Tarih kontrolü
                    const isAksiKarar = document.getElementById('chkAksiKarar').checked;
                    const today = new Date();
                    const yetkiTarihi = today.getDate() + '.' + (today.getMonth() + 1) + '.' + today.getFullYear();
                    const yetkiBitisTarihi = isAksiKarar ? 
                        'Aksi Karara Kadar' : 
                        document.getElementById('yetkiBitisTarihi').value;
                
                    // Yeni kayıt veya güncelleme için veri hazırla
                    const formData = {
                        yetkiliKontakt: yetkiliKontakt,
                        yetkiliAdi: yetkiliAdi,
                        yetkiSekli: document.getElementById('selYetkiSekli').value || 'Müştereken',
                        yetkiTarihi: yetkiTarihi,
                        yetkiBitisTarihi: yetkiBitisTarihi,
                        yetkiGrubu: document.getElementById('selYetkiGrubu').value || 'A Grubu',
                        sinirliYetkiDetaylari: document.getElementById('txtSinirliYetkiDetaylari').value || '',
                        yetkiTurleri: document.getElementById('selYetkiTurleri').value || '',
                        yetkiTutari: yetkiTutariNum.toFixed(2),
                        yetkiDovizCinsi: document.getElementById('selYetkiDovizCinsi').value || 'USD',
                        yetkiDurumu: document.getElementById('selYetkiDurumu').value || 'Aktif',
                        imzalar: imzalar
                    };

                    if(isUpdate) {
                        if(!selectedRow) {
                            throw new Error('Güncellenecek satır seçilmedi');
                        }

                        // Satırı güncelle
                        updateTableRow(selectedRow, formData);
                        btnEkle.innerHTML = '<i class="fas fa-plus"></i> Ekle';
                        btnEkle.classList.remove('update-mode');
                        selectedRow = null;
                        showNotification('Kayıt başarıyla güncellendi', 'success');
                    } else {
                        // Yeni satır ekle
                        addTableRow(formData);
                        showNotification('Yeni kayıt eklendi', 'success');
                    }

                    // Formu temizle
                    clearForm();
                    
                } catch (err) {
                    console.error('handleAddUpdate hatası:', err);
                    showNotification(err.message || 'İşlem sırasında bir hata oluştu', 'error');
                }
            }
            

            function clearForm() {
                try {
                    // Form alanlarını temizle
                    const elements = {
                        txtYetkiliKontakt: document.getElementById('txtYetkiliKontakt'),
                        txtYetkiliAdi: document.getElementById('txtYetkiliAdi'),
                        txtSinirliYetkiDetaylari: document.getElementById('txtSinirliYetkiDetaylari'),
                        txtYetkiTutari: document.getElementById('txtYetkiTutari')
                    };

                    // Güvenli bir şekilde değerleri temizle
                    Object.keys(elements).forEach(key => {
                        if (elements[key]) {
                            elements[key].value = '';
                        }
                    });
                    
                    // Tarihleri bugüne ayarla
                    const today = new Date();
                    const selGun = document.getElementById('selGun');
                    const selAy = document.getElementById('selAy');
                    const selYil = document.getElementById('selYil');
                    
                    if (selGun) selGun.value = today.getDate().toString().padStart(2, '0');
                    if (selAy) selAy.value = (today.getMonth() + 1).toString().padStart(2, '0');
                    if (selYil) selYil.value = today.getFullYear().toString();
                    
                    // Aksi karara kadar checkbox'ını temizle
                    document.getElementById('chkAksiKarar').checked = false;
                    
                    // İmzaları temizle
                    document.querySelectorAll('.signature-slot').forEach(slot => {
                        slot.classList.remove('filled');
                        slot.querySelector('.slot-image').style.backgroundImage = '';
                        const deleteBtn = slot.querySelector('.delete-signature');
                        if (deleteBtn) {
                            deleteBtn.style.display = 'none';
                        }
                    });

                    // Yetki detayları tablosundaki imzaları temizle
                    for(let i = 1; i <= 3; i++) {
                        const authSignature = document.getElementById(`authSignature${i}`);
                        if(authSignature) {
                            authSignature.style.backgroundImage = '';
                        }
                    }

                    // Seçili satırı temizle
                    if(selectedRow) {
                        selectedRow.classList.remove('selected');
                    }
                    selectedRow = null;
                    isEditing = false;
                    
                    // Seçili imzaları temizle
                    selectedSignatures = [];
                    hiddenSignatures.value = '';
                    
                    // Ekle/Güncelle butonunu sıfırla
                    const btnEkle = document.getElementById('btnEkle');
                    if (btnEkle) {
                        btnEkle.innerHTML = '<i class="fas fa-plus"></i> Ekle';
                        btnEkle.classList.remove('update-mode');
                    }
                } catch (err) {
                    console.error('Form temizleme hatası:', err);
                    showNotification('Form temizlenirken bir hata oluştu', 'error');
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
                try {
                    showLoading('Veriler kaydediliyor...');

                    // Get current grid state
                    updateGridState();
                    
                    // Prepare data for transfer
                    const transferData = {
                        gridState: {
                            current: gridState.current,
                            deleted: gridState.deleted
                        },
                        timestamp: new Date().getTime()
                    };

                    // Create FormData and add grid state
                    const formData = new FormData();
                    
                    // Add ASP.NET form data
                    formData.append('__VIEWSTATE', document.getElementById('__VIEWSTATE').value);
                    formData.append('__VIEWSTATEGENERATOR', document.getElementById('__VIEWSTATEGENERATOR').value);
                    formData.append('__EVENTVALIDATION', document.getElementById('__EVENTVALIDATION').value);
                    formData.append('__EVENTTARGET', '<%= btnSaveSignature.UniqueID %>');
                    
                    // Add grid state data
                    formData.append('gridState', JSON.stringify({
                        current: gridState.current,
                        deleted: gridState.deleted
                    }));
                    
                    // Add CSRF token
                    SecurityManager.addCsrfToFormData(formData);
                
                    // Send data to server
                    fetch(window.location.href, {
                        method: 'POST',
                        body: formData,
                        headers: {
                            'X-Requested-With': 'XMLHttpRequest'
                        }
                    })
                    .then(response => {
                        if (!response.ok) {
                            throw { 
                                code: 'NETWORK_ERROR', 
                                message: `HTTP error! status: ${response.status}`
                            };
                        }
                        return response.text();
                    })
                    .then(text => {
                        try {
                            return JSON.parse(text);
                        } catch (e) {
                            throw { 
                                code: 'PARSE_ERROR', 
                                message: 'Server yanıtı JSON formatında değil'
                            };
                        }
                    })
                    .then(data => {
                        if (!data.success) {
                            throw { 
                                code: 'SAVE_ERROR', 
                                message: data.error || 'Kayıt işlemi başarısız'
                            };
                        }
                        
                        showNotification('Veriler başarıyla kaydedildi', 'success');
                        
                        // Send data to opener window using multiple methods
                        if (window.opener && !window.opener.closed) {
                            // 1. PostMessage for immediate update
                            window.opener.postMessage({
                                type: 'SIGNATURE_SAVED',
                                success: true,
                                data: transferData
                            }, '*');
                            
                            // 2. Store in sessionStorage as backup
                            try {
                                sessionStorage.setItem('SIGNATURE_DATA', JSON.stringify(transferData));
                            } catch (err) {
                                console.warn('SessionStorage save failed:', err);
                            }
                            
                            // 3. Set hidden field value
                            const hdnTransferData = document.getElementById('<%= hdnSignatures.ClientID %>');
                            if (hdnTransferData) {
                                hdnTransferData.value = JSON.stringify(transferData);
                            }
                        }
                        
                        // Close window after delay
                        setTimeout(() => {
                            try {
                                window.close();
                            } catch (err) {
                                console.log('Pencere kapatılamadı:', err);
                            }
                        }, 1500);
                    })
                    .catch(error => {
                        ErrorHandler.handle(error, 'saveSignature');
                    })
                    .finally(() => {
                        hideLoading();
                    });
                    
                    return false;
                } catch (error) {
                    ErrorHandler.handle(error, 'saveSignature');
                    hideLoading();
                    return false;
                }
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

            // Save button click handler artık saveAndReturn kullanıyor

            // Ajax timeout kontrolü
            const AJAX_TIMEOUT = 30000; // 30 saniye
            let currentAjaxRequest = null;

            function handleAjaxTimeout() {
                if (currentAjaxRequest) {
                    currentAjaxRequest.abort();
                }
                hideLoading();
                showNotification('İmza kaydetme işlemi zaman aşımına uğradı. Lütfen tekrar deneyiniz.', 'error');
                const btnSave = document.getElementById('<%= btnSaveSignature.ClientID %>');
                if (btnSave) {
                    btnSave.disabled = false;
                }
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
            const hdnCurrentPdfList = document.getElementById('<%= hdnCurrentPdfList.ClientID %>').value;

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
                    initializeDatePicker();
                    
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

            // Numeric validation functions
            function isNumberKey(evt) {
                var charCode = (evt.which) ? evt.which : evt.keyCode;
                if (charCode === 46) { // Allow decimal point (.)
                    var inputValue = evt.target.value;
                    if (inputValue.indexOf('.') > -1) {
                        return false;
                    }
                }
                // Allow only numbers and decimal point
                if (charCode !== 46 && charCode > 31 && (charCode < 48 || charCode > 57)) {
                    return false;
                }
                return true;
            }

            function validatePaste(evt) {
                var clipboardData = evt.clipboardData || window.clipboardData;
                var pastedData = clipboardData.getData('Text');
                if (!/^\d*\.?\d*$/.test(pastedData)) {
                    return false;
                }
                return true;
            }

            function validateDecimalPlaces(input, decimals) {
                var value = input.value;
                if (value.includes('.')) {
                    var parts = value.split('.');
                    if (parts[1].length > decimals) {
                        input.value = parseFloat(value).toFixed(decimals);
                    }
                }
                // Remove leading zeros
                if (value.length > 1 && value[0] === '0' && value[1] !== '.') {
                    input.value = parseFloat(value);
                }
            }

            // Tarih seçici için JavaScript fonksiyonları
            function initializeDatePicker() {
                const dateInput = document.getElementById('yetkiBitisTarihi');
                const today = new Date();
                
                // Varsayılan değeri bugün olarak ayarla
                dateInput.valueAsDate = today;
                
                // Minimum tarihi bugün olarak ayarla
                dateInput.min = today.toISOString().split('T')[0];
            }

            function handleAksiKararChange(checkbox) {
                const dateInput = document.getElementById('yetkiBitisTarihi');
                
                if (checkbox.checked) {
                    // Aksi karara kadar seçiliyse
                    dateInput.disabled = true;
                    dateInput.value = '2030-12-31'; // Maksimum tarih
                } else {
                    // Aksi karar seçili değilse
                    dateInput.disabled = false;
                    // Bugünün tarihini set et
                    const today = new Date();
                    dateInput.valueAsDate = today;
                }
            }

            function getYetkiBitisTarihi() {
                const aksiKarar = document.getElementById('chkAksiKarar').checked;
                if (aksiKarar) {
                    return 'Aksi Karara Kadar';
                }
                
                const dateInput = document.getElementById('yetkiBitisTarihi');
                const date = new Date(dateInput.value);
                return date.toLocaleDateString('tr-TR'); // "dd.mm.yyyy" formatında
            }
        </script>
        <!-- Notification Container -->
        <div id="notification" class="notification">
            <button type="button" class="close-btn" onclick="hideNotification()">&times;</button>
            <span id="notificationMessage"></span>
        </div>
    </form>
</body>
</html>