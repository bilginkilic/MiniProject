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
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1200px;
            margin: 0 auto;
            background: white;
            padding: 20px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(220,53,69,0.1);
            border: 1px solid rgba(220,53,69,0.1);
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding: 15px;
            border-bottom: 2px solid #dc3545;
            background-color: #f8f9fa;
            border-radius: 6px;
        }
        .header h2 {
            margin: 0;
            color: #dc3545;
            font-weight: 500;
        }
        .button {
            padding: 8px 16px;
            background-color: #dc3545; /* Kırmızı */
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            transition: all 0.3s ease;
        }
        .button:hover {
            background-color: #c82333; /* Koyu kırmızı */
            transform: translateY(-1px);
        }
        .button.secondary {
            background-color: #6c757d; /* Gri */
        }
        .button.secondary:hover {
            background-color: #5a6268; /* Koyu gri */
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
            background-color: #dc3545; /* Kırmızı */
            padding: 12px;
            text-align: left;
            border-bottom: 2px solid #c82333; /* Koyu kırmızı */
            color: white;
            font-weight: 500;
        }
        .grid td {
            padding: 12px;
            border-bottom: 1px solid #dee2e6;
            vertical-align: middle;
        }
        .grid tr:hover {
            background-color: #f8f9fa;
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
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        <asp:HiddenField ID="hdnSignaturePath" runat="server" />
        
        <script type="text/javascript">
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
                        <asp:BoundField DataField="YetkiliKontNo" HeaderText="Yetkili Kont. No" />
                        <asp:BoundField DataField="YetkiliAdiSoyadi" HeaderText="Yetkili Adı Soyadı" />
                        <asp:BoundField DataField="YetkiSekli" HeaderText="Yetki Şekli" />
                        <asp:BoundField DataField="YetkiSuresi" HeaderText="Yetki Süresi" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="YetkiBitisTarihi" HeaderText="Yetki Bitiş Tarihi" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="YetkiGrubu" HeaderText="Yetki Grubu" />
                        <asp:BoundField DataField="SinirliYetkiDetaylari" HeaderText="Sınırlı Yetki Detayları" />
                        <asp:BoundField DataField="YetkiTurleri" HeaderText="Yetki Türleri" />
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
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="YetkiTutari" HeaderText="Yetki Tutarı" DataFormatString="{0:N2}" />
                        <asp:BoundField DataField="YetkiDovizCinsi" HeaderText="Yetki Döviz Cinsi" />
                        <asp:TemplateField HeaderText="Yetki Durumu">
                            <ItemTemplate>
                                <asp:Label runat="server" Text='<%# Eval("YetkiDurumu") %>'
                                    CssClass='<%# (Eval("YetkiDurumu").ToString() == "Aktif" ? "status-active" : "status-inactive") %>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="İşlemler">
                            <ItemTemplate>
                                <div class="action-buttons">
                                    <asp:LinkButton ID="btnEdit" runat="server" CssClass="action-button edit-button"
                                        CommandName="EditUser" CommandArgument='<%# Eval("YetkiliKontNo") %>'
                                        Text="Düzenle" />
                                    <asp:LinkButton ID="btnDelete" runat="server" CssClass="action-button delete-button"
                                        CommandName="DeleteUser" CommandArgument='<%# Eval("YetkiliKontNo") %>'
                                        Text="Sil" OnClientClick="return confirm('Bu yetkiliyi silmek istediğinizden emin misiniz?');" />
                                </div>
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