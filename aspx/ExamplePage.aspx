<%@ Page Language="C#" AutoEventWireup="true" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Örnek ASPX Sayfası</title>
    <style type="text/css">
        body { font-family: Arial, sans-serif; margin: 20px; }
        .container { max-width: 800px; margin: 0 auto; }
        .header { background-color: #f0f0f0; padding: 10px; margin-bottom: 20px; }
        .content { padding: 20px; border: 1px solid #ddd; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <div class="header">
                <h2>Örnek ASPX İçerik</h2>
            </div>
            <div class="content">
                <p>Bu bir örnek ASPX sayfasıdır. Popup içinde görüntülenecektir.</p>
                <asp:Button ID="btnClose" runat="server" Text="Kapat" 
                    OnClientClick="window.close(); return false;" />
            </div>
        </div>
    </form>
</body>
</html>