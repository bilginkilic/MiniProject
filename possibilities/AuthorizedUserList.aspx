<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AuthorizedUserList.aspx.cs" Inherits="AspxExamples.AuthorizedUserList" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Yetkili Kullanıcı Listesi</title>
    <style type="text/css">
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f0f2f5;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.05);
            border: 1px solid #e0e0e0;
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding: 15px;
            border-bottom: 2px solid #e0e0e0;
            background-color: white;
            border-radius: 6px;
        }
        .header h2 {
            margin: 0;
            color: #333;
            font-weight: 500;
        }
        .button {
            padding: 8px 16px;
            background-color: #fff;
            color: #dc3545;
            border: 1px solid #dc3545;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            transition: all 0.3s ease;
            font-weight: 500;
        }
        .button:hover {
            background-color: #dc3545;
            color: white;
            transform: translateY(-1px);
        }
        .button.secondary {
            background-color: #fff;
            color: #6c757d;
            border: 1px solid #6c757d;
        }
        .button.secondary:hover {
            background-color: #6c757d;
            color: white;
        }
        .button:disabled {
            opacity: 0.6;
            cursor: not-allowed;
            transform: none;
        }
        .grid-container {
            margin-top: 20px;
            overflow-x: auto;
        }
        .grid {
            width: 100%;
            border-collapse: collapse;
        }
        .grid th {
            background-color: #f8f9fa;
            padding: 12px;
            text-align: left;
            border-bottom: 2px solid #e0e0e0;
            color: #333;
            font-weight: 600;
            font-size: 13px;
        }
        .grid td {
            padding: 8px;
            border-bottom: 1px solid #e0e0e0;
            vertical-align: middle;
            color: #333;
            font-size: 14px;
        }
        .grid tr:hover {
            background-color: #f8f9fa;
        }
        .grid tr:nth-child(even) {
            background-color: #ffffff;
        }
        .grid tr:nth-child(even):hover {
            background-color: #f8f9fa;
        }
        .grid-input {
            width: 100%;
            padding: 6px 8px;
            border: 1px solid #e0e0e0;
            border-radius: 4px;
            font-size: 14px;
            color: #333;
            background: white;
            transition: all 0.2s ease;
        }
        .grid-input:focus {
            border-color: #dc3545;
            box-shadow: 0 0 0 2px rgba(220,53,69,0.1);
            outline: none;
        }
        .grid-input:disabled {
            background-color: #f8f9fa;
            cursor: not-allowed;
        }
        .grid-select {
            width: 100%;
            padding: 6px 24px 6px 8px;
            border: 1px solid #e0e0e0;
            border-radius: 4px;
            font-size: 14px;
            color: #333;
            background: white;
            appearance: none;
            background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' fill='%23333' viewBox='0 0 16 16'%3E%3Cpath d='M7.247 11.14L2.451 5.658C1.885 5.013 2.345 4 3.204 4h9.592a1 1 0 0 1 .753 1.659l-4.796 5.48a1 1 0 0 1-1.506 0z'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 8px center;
        }
        .grid-select:focus {
            border-color: #dc3545;
            box-shadow: 0 0 0 2px rgba(220,53,69,0.1);
            outline: none;
        }
        .grid-action-cell {
            white-space: nowrap;
            width: 1%;
        }
        .grid-button {
            padding: 4px 8px;
            font-size: 13px;
            border-radius: 4px;
            border: none;
            cursor: pointer;
            margin-right: 4px;
            transition: all 0.2s ease;
        }
        .grid-button.edit {
            background-color: #f8f9fa;
            color: #333;
            border: 1px solid #e0e0e0;
        }
        .grid-button.edit:hover {
            background-color: #e9ecef;
        }
        .grid-button.delete {
            background-color: #fff;
            color: #dc3545;
            border: 1px solid #dc3545;
        }
        .grid-button.delete:hover {
            background-color: #dc3545;
            color: #fff;
        }
        .grid-button.save {
            background-color: #fff;
            color: #28a745;
            border: 1px solid #28a745;
        }
        .grid-button.save:hover {
            background-color: #28a745;
            color: #fff;
        }
        .grid-button.cancel {
            background-color: #fff;
            color: #6c757d;
            border: 1px solid #6c757d;
        }
        .grid-button.cancel:hover {
            background-color: #6c757d;
            color: #fff;
        }
        .signature-preview {
            max-width: 100px;
            max-height: 40px;
            cursor: pointer;
        }
        .status-active {
            color: #dc3545; /* Kırmızı */
            font-weight: bold;
        }
        .status-inactive {
            color: #6c757d; /* Gri */
            font-weight: bold;
        }
        .action-buttons {
            display: flex;
            gap: 8px;
        }
        .action-button {
            padding: 4px 8px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 12px;
        }
        .edit-button {
            background-color: #6c757d; /* Gri */
            color: white;
            transition: all 0.3s ease;
        }
        .edit-button:hover {
            background-color: #5a6268; /* Koyu gri */
            transform: translateY(-1px);
        }
        .delete-button {
            background-color: #dc3545; /* Kırmızı */
            color: white;
            transition: all 0.3s ease;
        }
        .delete-button:hover {
            background-color: #c82333; /* Koyu kırmızı */
            transform: translateY(-1px);
        }
        /* Modal Styles */
        .modal-overlay {
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
        .modal-content {
            background: white;
            padding: 0;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(220, 53, 69, 0.15);
            border: 1px solid rgba(220, 53, 69, 0.1);
            width: 90%;
            max-width: 1200px;
            height: 90vh;
            position: relative;
            display: flex;
            flex-direction: column;
        }
        .modal-header {
            padding: 15px 20px;
            border-bottom: 2px solid #dc3545;
            display: flex;
            justify-content: space-between;
            align-items: center;
            background: #f8f9fa;
            border-radius: 8px 8px 0 0;
        }
        .modal-header h2 {
            margin: 0;
            color: #dc3545;
            font-size: 20px;
            font-weight: 500;
        }
        .modal-close {
            background: none;
            border: none;
            color: #6c757d;
            font-size: 24px;
            cursor: pointer;
            padding: 0;
            width: 32px;
            height: 32px;
            display: flex;
            align-items: center;
            justify-content: center;
            border-radius: 50%;
            transition: all 0.3s ease;
        }
        .modal-close:hover {
            background: rgba(220, 53, 69, 0.1);
            color: #dc3545;
        }
        .modal-body {
            flex: 1;
            overflow: hidden;
            position: relative;
        }
        .modal-iframe {
            width: 100%;
            height: 100%;
            border: none;
            background: white;
        }
        
        /* Form Styles */
        .form-container {
            padding: 20px;
            background: #fff;
            border-radius: 8px;
        }
        
        .form-row {
            display: flex;
            gap: 20px;
            margin-bottom: 15px;
        }
        
        .form-group {
            flex: 1;
            display: flex;
            flex-direction: column;
        }
        
        .form-group.full-width {
            flex: 0 0 100%;
        }
        
        .form-control {
            padding: 8px 12px;
            border: 1px solid #e9ecef;
            border-radius: 4px;
            font-size: 14px;
            transition: all 0.3s ease;
            background-color: #fff;
        }
        
        .form-control:focus {
            border-color: #dc3545;
            box-shadow: 0 0 0 2px rgba(220,53,69,0.1);
            outline: none;
            background-color: #fff5e6;
        }
        
        label {
            margin-bottom: 5px;
            color: #495057;
            font-weight: 500;
            font-size: 13px;
        }
        
        .form-section {
            background-color: #fff5e6;
            padding: 15px;
            border-radius: 6px;
            margin-bottom: 20px;
            border: 1px solid #e9ecef;
        }
        
        .form-section-title {
            color: #495057;
            font-size: 16px;
            font-weight: 500;
            margin-bottom: 15px;
            padding-bottom: 10px;
            border-bottom: 1px solid #e9ecef;
        }
        
        .form-control:disabled {
            background: #f8f9fa;
            cursor: not-allowed;
        }
        
        textarea.form-control {
            resize: vertical;
            min-height: 60px;
        }
        
        .button-group {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            margin-top: 20px;
        }
        
        .form-message {
            margin-top: 10px;
            padding: 10px;
            border-radius: 4px;
            font-size: 14px;
        }
        
        .form-message.error {
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            color: #721c24;
        }
        
        .form-message.success {
            background: #d4edda;
            border: 1px solid #c3e6cb;
            color: #155724;
        }
        
        /* Validation Styles */
        .validation-error {
            color: #dc3545;
            font-size: 12px;
            margin-top: 4px;
            display: block;
        }
        
        .validation-summary {
            color: #dc3545;
            background: #f8d7da;
            border: 1px solid #f5c6cb;
            padding: 10px;
            margin: 10px 0;
            border-radius: 4px;
            font-size: 14px;
        }
        
        .validation-summary ul {
            margin: 0;
            padding-left: 20px;
        }
        
        .form-control.input-validation-error {
            border-color: #dc3545;
        }
        
        .form-control.input-validation-error:focus {
            box-shadow: 0 0 0 2px rgba(220,53,69,0.25);
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        <asp:HiddenField ID="hdnSignaturePath" runat="server" />
        
        <script type="text/javascript">
            function openUserFormModal() {
                var modal = document.getElementById('userFormModal');
                modal.style.display = 'flex';
            }
            
            function closeUserFormModal() {
                var modal = document.getElementById('userFormModal');
                modal.style.display = 'none';
            }
            
            function openSignatureModal(yetkiliKontNo) {
                var modal = document.getElementById('signatureModal');
                var iframe = document.getElementById('signatureFrame');
                var url = 'PdfSignatureForm.aspx';
                
                if (yetkiliKontNo) {
                    url += '?yetkiliKontNo=' + yetkiliKontNo;
                }
                
                iframe.src = url;
                modal.style.display = 'flex';
                
                // Iframe yüklendiğinde loading göstergesini gizle
                iframe.onload = function() {
                    var loadingOverlay = iframe.contentWindow.document.getElementById('loadingOverlay');
                    if (loadingOverlay) {
                        loadingOverlay.style.display = 'none';
                    }
                };
            }
            
            function closeSignatureModal() {
                var modal = document.getElementById('signatureModal');
                var iframe = document.getElementById('signatureFrame');
                modal.style.display = 'none';
                iframe.src = 'about:blank';
            }

            function handleSignatureReturn(signaturePath) {
                // İmza yolunu hidden field'a kaydet
                var hdnSignaturePath = document.getElementById('<%= hdnSignaturePath.ClientID %>');
                if (hdnSignaturePath) {
                    hdnSignaturePath.value = signaturePath;
                    
                    // UpdatePanel'i güncelle
                    if (typeof(Sys) !== 'undefined' && Sys.WebForms) {
                        var prm = Sys.WebForms.PageRequestManager.getInstance();
                        if (prm) {
                            prm._doPostBack('UpdatePanel1', '');
                        }
                    }
                    
                    // Modal'ı kapat
                    closeSignatureModal();
                }
            }
        </script>
        
        <div class="container">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <div class="header">
                        <h2>Yetkili Kullanıcı Listesi</h2>
                        <asp:Button ID="btnAddNew" runat="server" Text="Yeni Yetkili Ekle" CssClass="button" OnClick="BtnAddNew_Click" />
                    </div>

                    <div class="grid-container">
                        <asp:GridView ID="gvAuthorizedUsers" runat="server" CssClass="grid" AutoGenerateColumns="false"
                            OnRowCommand="GvAuthorizedUsers_RowCommand" DataKeyNames="YetkiliKontNo">
                            <Columns>
                                <asp:TemplateField HeaderText="Yetkili Kont. No">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtYetkiliKontNo" runat="server" CssClass="grid-input" 
                                            Text='<%# Eval("YetkiliKontNo") %>' Enabled="false" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Şekli">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlYetkiSekli" runat="server" CssClass="grid-select">
                                            <asp:ListItem Text="Müştereken" Value="Müştereken" />
                                            <asp:ListItem Text="Münferiden" Value="Münferiden" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Bitiş Tarihi">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtYetkiBitisTarihi" runat="server" CssClass="grid-input" 
                                            Text='<%# Bind("YetkiBitisTarihi", "{0:yyyy-MM-dd}") %>' TextMode="Date" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Grubu">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlYetkiGrubu" runat="server" CssClass="grid-select">
                                            <asp:ListItem Text="A Grubu" Value="A Grubu" />
                                            <asp:ListItem Text="B Grubu" Value="B Grubu" />
                                            <asp:ListItem Text="C Grubu" Value="C Grubu" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Sınırlı Yetki Detayları">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtSinirliYetkiDetaylari" runat="server" CssClass="grid-input" 
                                            Text='<%# Eval("SinirliYetkiDetaylari") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Türleri">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlYetkiTurleri" runat="server" CssClass="grid-select">
                                            <asp:ListItem Text="Kredi İşlemleri, Hazine İşlemleri" Value="Kredi İşlemleri, Hazine İşlemleri" />
                                            <asp:ListItem Text="Kredi Sözleşmeleri / Transfer İşlemleri" Value="Kredi Sözleşmeleri / Transfer İşlemleri" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="İmza Örnekleri">
                                    <ItemTemplate>
                                        <div style="display: flex; gap: 10px;">
                                            <asp:Image runat="server" ID="imgSignature1" CssClass="signature-preview" 
                                                ImageUrl='<%# Eval("ImzaOrnegi1") %>' 
                                                Visible='<%# !string.IsNullOrEmpty(Eval("ImzaOrnegi1") as string) %>' />
                                            <asp:Image runat="server" ID="imgSignature2" CssClass="signature-preview" 
                                                ImageUrl='<%# Eval("ImzaOrnegi2") %>' 
                                                Visible='<%# !string.IsNullOrEmpty(Eval("ImzaOrnegi2") as string) %>' />
                                            <asp:Image runat="server" ID="imgSignature3" CssClass="signature-preview" 
                                                ImageUrl='<%# Eval("ImzaOrnegi3") %>' 
                                                Visible='<%# !string.IsNullOrEmpty(Eval("ImzaOrnegi3") as string) %>' />
                                        </div>
                                        <asp:Button ID="btnSelectSignature" runat="server" Text="İmza Seç" 
                                            CssClass="grid-button edit" CommandName="SelectSignature" 
                                            CommandArgument='<%# Eval("YetkiliKontNo") %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Tutarı">
                                    <ItemTemplate>
                                        <asp:TextBox ID="txtYetkiTutari" runat="server" CssClass="grid-input" 
                                            Text='<%# Bind("YetkiTutari", "{0:N2}") %>' TextMode="Number" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Döviz Cinsi">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlYetkiDovizCinsi" runat="server" CssClass="grid-select">
                                            <asp:ListItem Text="USD" Value="USD" />
                                            <asp:ListItem Text="EUR" Value="EUR" />
                                            <asp:ListItem Text="TRY" Value="TRY" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="Yetki Durumu">
                                    <ItemTemplate>
                                        <asp:DropDownList ID="ddlYetkiDurumu" runat="server" CssClass="grid-select">
                                            <asp:ListItem Text="Aktif" Value="Aktif" />
                                            <asp:ListItem Text="Pasif" Value="Pasif" />
                                        </asp:DropDownList>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderText="İşlemler" ItemStyle-CssClass="grid-action-cell">
                                    <ItemTemplate>
                                        <asp:Button ID="btnSave" runat="server" Text="Kaydet" 
                                            CssClass="grid-button save" CommandName="SaveUser" 
                                            CommandArgument='<%# Eval("YetkiliKontNo") %>' />
                                        <asp:Button ID="btnDelete" runat="server" Text="Sil" 
                                            CssClass="grid-button delete" CommandName="DeleteUser" 
                                            CommandArgument='<%# Eval("YetkiliKontNo") %>'
                                            OnClientClick="return confirm('Bu yetkiliyi silmek istediğinizden emin misiniz?');" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                </asp:GridView>
            </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <!-- Modal -->
        <div id="signatureModal" class="modal-overlay">
            <div class="modal-content">
                <div class="modal-header">
                    <h2>İmza Sirkülerinden İmza Seçimi</h2>
                    <button type="button" class="modal-close" onclick="closeSignatureModal()">&times;</button>
                </div>
                <div class="modal-body">
                    <iframe id="signatureFrame" class="modal-iframe" src="about:blank"></iframe>
                </div>
            </div>
        </div>
    </form>
</body>
</html>