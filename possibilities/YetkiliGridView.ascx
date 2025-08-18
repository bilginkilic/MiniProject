<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="YetkiliGridView.ascx.cs" Inherits="AspxExamples.YetkiliGridView" %>

<asp:UpdatePanel ID="upGrid" runat="server">
    <ContentTemplate>
        <asp:GridView ID="grdYetkililer" runat="server" 
            AutoGenerateColumns="False"
            CssClass="auth-details-table"
            DataKeyNames="YetkiliKontakt"
            OnRowDataBound="grdYetkililer_RowDataBound"
            OnRowCommand="grdYetkililer_RowCommand"
            AllowPaging="True"
            AllowSorting="True"
            PageSize="10"
            OnPageIndexChanging="grdYetkililer_PageIndexChanging"
            OnSorting="grdYetkililer_Sorting">
            <Columns>
                <asp:BoundField DataField="YetkiliKontakt" HeaderText="Yetkili Kont. No" SortExpression="YetkiliKontakt" />
                <asp:BoundField DataField="YetkiliAdi" HeaderText="Yetkili Adı Soyadı" SortExpression="YetkiliAdi" />
                <asp:BoundField DataField="YetkiSekli" HeaderText="Yetki Şekli" SortExpression="YetkiSekli" />
                <asp:BoundField DataField="YetkiTarihi" HeaderText="Yetki Süresi" SortExpression="YetkiTarihi" />
                <asp:BoundField DataField="YetkiBitisTarihi" HeaderText="Yetki Bitiş Tarihi" SortExpression="YetkiBitisTarihi" />
                <asp:BoundField DataField="YetkiGrubu" HeaderText="İmza Yetki Grubu" SortExpression="YetkiGrubu" />
                <asp:BoundField DataField="SinirliYetkiDetaylari" HeaderText="Sınırlı Yetki Detayları" />
                <asp:BoundField DataField="YetkiTurleri" HeaderText="Yetki Türleri" />
                
                <asp:TemplateField HeaderText="İmza Örneği 1">
                    <ItemTemplate>
                        <div class="signature-preview" id='imgSignature1_<%# Eval("YetkiliKontakt") %>'>
                            <asp:HiddenField ID="hdnImza1" runat="server" Value='<%# GetImzaValue(Container.DataItem, 0) %>' />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="İmza Örneği 2">
                    <ItemTemplate>
                        <div class="signature-preview" id='imgSignature2_<%# Eval("YetkiliKontakt") %>'>
                            <asp:HiddenField ID="hdnImza2" runat="server" Value='<%# GetImzaValue(Container.DataItem, 1) %>' />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="İmza Örneği 3">
                    <ItemTemplate>
                        <div class="signature-preview" id='imgSignature3_<%# Eval("YetkiliKontakt") %>'>
                            <asp:HiddenField ID="hdnImza3" runat="server" Value='<%# GetImzaValue(Container.DataItem, 2) %>' />
                        </div>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="YetkiTutari" HeaderText="Yetki Tutarı" SortExpression="YetkiTutari" />
                <asp:BoundField DataField="YetkiDovizCinsi" HeaderText="Yetki Döv." SortExpression="YetkiDovizCinsi" />
                <asp:BoundField DataField="YetkiDurumu" HeaderText="Durum" SortExpression="YetkiDurumu" />
                
                <asp:TemplateField HeaderText="İşlemler">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CommandName="EditRow" 
                            CommandArgument='<%# Container.DataItemIndex %>'
                            CssClass="button secondary">
                            <i class="fas fa-edit"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnDelete" runat="server" CommandName="DeleteRow"
                            CommandArgument='<%# Container.DataItemIndex %>'
                            CssClass="button secondary"
                            OnClientClick="return confirm('Silmek istediğinize emin misiniz?');">
                            <i class="fas fa-trash"></i>
                        </asp:LinkButton>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerStyle CssClass="gridview-pager" />
            <HeaderStyle CssClass="gridview-header" />
            <RowStyle CssClass="gridview-row" />
            <AlternatingRowStyle CssClass="gridview-alternating-row" />
        </asp:GridView>
    </ContentTemplate>
</asp:UpdatePanel>
