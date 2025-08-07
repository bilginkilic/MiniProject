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
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            padding-bottom: 10px;
            border-bottom: 2px solid #eee;
        }
        .header h2 {
            margin: 0;
            color: #333;
        }
        .button {
            padding: 8px 16px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
        }
        .button:hover {
            background-color: #0056b3;
        }
        .button.secondary {
            background-color: #6c757d;
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
            border-bottom: 2px solid #dee2e6;
            color: #495057;
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
            color: #28a745;
            font-weight: bold;
        }
        .status-inactive {
            color: #dc3545;
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
            background-color: #ffc107;
            color: #000;
        }
        .delete-button {
            background-color: #dc3545;
            color: white;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        
        <div class="container">
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
        </div>
    </form>
</body>
</html>