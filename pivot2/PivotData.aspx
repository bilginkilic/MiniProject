<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PivotData.aspx.cs" Inherits="YourNamespace.PivotData" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Pivot Data Report</title>
    
    <!-- Pivot.js CSS -->
    <link rel="stylesheet" type="text/css" href="css/pivot.min.css" />
    
    <!-- jQuery (pivot.js dependency) -->
    <script src="js/jquery-3.6.0.min.js"></script>
    
    <!-- Pivot.js -->
    <script src="js/pivot.min.js"></script>
    
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 10px;
            background-color: #f5f5f5;
        }
        
        .header-panel {
            margin-bottom: 20px;
            padding: 15px;
            background-color: #f8f9fa;
            border-radius: 5px;
            display: flex;
            align-items: center;
            gap: 15px;
            flex-wrap: wrap;
        }
        
        .date-group {
            display: flex;
            align-items: center;
            gap: 10px;
        }
        
        .date-group label {
            font-weight: bold;
            color: #333;
        }
        
        .date-group input {
            padding: 5px 10px;
            border: 1px solid #ddd;
            border-radius: 3px;
        }
        
        .button-group {
            display: flex;
            gap: 10px;
        }
        
        .btn {
            padding: 8px 20px;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            font-weight: bold;
            transition: background-color 0.3s;
        }
        
        .btn-primary {
            background-color: #007bff;
            color: white;
        }
        
        .btn-primary:hover {
            background-color: #0056b3;
        }
        
        .btn-success {
            background-color: #28a745;
            color: white;
        }
        
        .btn-success:hover {
            background-color: #218838;
        }
        
        .pivot-container {
            display: flex;
            gap: 10px;
            margin-top: 20px;
        }
        
        .pivot-output {
            flex: 1;
            overflow: auto;
        }
        
        .pvtTable {
            margin-top: 20px;
            width: 100%;
        }
        
        .footer-text {
            margin-top: 20px;
            padding: 10px;
            font-size: 12px;
            color: #666;
            text-align: center;
            border-top: 1px solid #ddd;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePageMethods="true" />
        <div class="header-panel">
            <div class="date-group">
                <label>Pivot Data From:</label>
                <input type="date" id="dateFrom" runat="server" />
            </div>
            <div class="date-group">
                <label>Pivot Data to:</label>
                <input type="date" id="dateTo" runat="server" />
            </div>
            <div class="button-group">
                <button type="button" class="btn btn-primary" onclick="loadData()">Load Data</button>
                <button type="button" class="btn btn-success" onclick="exportPivotToExcel()">Excel Data</button>
            </div>
        </div>
        
        <div class="pivot-container">
            <div id="output" class="pivot-output"></div>
        </div>
        
        <div class="footer-text">
            Kayıtlar listelendi. Total time süreleri dakika türünden sunulmaktadır.
        </div>
    </form>

    <script type="text/javascript">
        var pivotData = [];
        var pivotInstance = null;

        // Veri yükleme fonksiyonu
        function loadData() {
            var dateFrom = document.getElementById('<%= dateFrom.ClientID %>').value;
            var dateTo = document.getElementById('<%= dateTo.ClientID %>').value;
            
            // Server-side'dan veri al (PageMethod veya AJAX ile)
            // Bu kısmı kendi veri kaynağınıza göre düzenleyin
            PageMethods.GetPivotData(dateFrom, dateTo, onDataReceived, onError);
        }

        function onDataReceived(result) {
            try {
                pivotData = JSON.parse(result);
                
                // Pivot tablosunu oluştur/güncelle
                if (pivotInstance) {
                    $("#output").empty();
                }
                
                pivotInstance = $("#output").pivotUI(
                    pivotData,
                    {
                        rows: ["Currency"],
                        cols: [],
                        vals: ["Amount"],
                        aggregatorName: "Sum",
                        rendererName: "Table"
                    }
                );
            } catch (ex) {
                alert('Veri yüklenirken hata oluştu: ' + ex.message);
            }
        }

        function onError(error) {
            alert('Veri yüklenirken hata oluştu: ' + error.get_message());
        }

        // Excel'e export fonksiyonu
        function exportPivotToExcel() {
            try {
                // Pivot tablosunu bul
                var table = document.querySelector('table.pvtTable');
                
                if (!table) {
                    alert('Pivot tablosu bulunamadı. Lütfen önce veri yükleyin.');
                    return;
                }
                
                // Tablo HTML'ini al
                var tableHtml = table.outerHTML;
                
                // Windows Forms uygulaması için: window.external kullanarak C# tarafına sinyal gönder
                if (window.external && typeof window.external.ExportToExcel === 'function') {
                    // C# tarafında ExportToExcel metodu varsa onu çağır
                    window.external.ExportToExcel(tableHtml);
                } else {
                    // Server-side metod çağır - base64 string dönecek
                    PageMethods.ExportToExcel(tableHtml, onExportSuccess, onExportError);
                }
            } catch (ex) {
                alert('Excel export sırasında hata oluştu: ' + ex.message);
            }
        }

        function onExportSuccess(base64String) {
            try {
                if (!base64String || base64String === '') {
                    alert('Excel dosyası oluşturulamadı. Lütfen tekrar deneyin.');
                    return;
                }

                // Base64 string'i binary data'ya çevir
                var binaryString = atob(base64String);
                var bytes = new Uint8Array(binaryString.length);
                for (var i = 0; i < binaryString.length; i++) {
                    bytes[i] = binaryString.charCodeAt(i);
                }

                // Dosya adını oluştur
                var fileName = 'PivotTable_' + formatDateForFileName() + '.xlsx';

                // Blob oluştur ve indir
                if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                    // IE 10+ için
                    var blob = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    window.navigator.msSaveOrOpenBlob(blob, fileName);
                } else if (window.Blob && window.URL && window.URL.createObjectURL) {
                    // Modern tarayıcılar için
                    var blob = new Blob([bytes], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
                    var url = window.URL.createObjectURL(blob);
                    var a = document.createElement('a');
                    a.href = url;
                    a.download = fileName;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                    window.URL.revokeObjectURL(url);
                } else {
                    // Eski tarayıcılar için data URI kullan
                    var dataUri = 'data:application/vnd.openxmlformats-officedocument.spreadsheetml.sheet;base64,' + base64String;
                    var link = document.createElement('a');
                    link.href = dataUri;
                    link.download = fileName;
                    document.body.appendChild(link);
                    link.click();
                    document.body.removeChild(link);
                }
            } catch (ex) {
                alert('Excel dosyası indirilirken hata oluştu: ' + ex.message);
            }
        }

        function onExportError(error) {
            var errorMessage = error.get_message ? error.get_message() : (error.message || 'Bilinmeyen hata');
            alert('Excel export sırasında hata oluştu: ' + errorMessage);
        }

        // Tarih formatı için yardımcı fonksiyon
        function formatDateForFileName() {
            var now = new Date();
            var year = now.getFullYear();
            var month = String(now.getMonth() + 1).padStart(2, '0');
            var day = String(now.getDate()).padStart(2, '0');
            var hours = String(now.getHours()).padStart(2, '0');
            var minutes = String(now.getMinutes()).padStart(2, '0');
            return year + month + day + '_' + hours + minutes;
        }

        // Sayfa yüklendiğinde
        $(document).ready(function() {
            // Varsayılan tarihleri ayarla
            var today = new Date();
            var firstDay = new Date(today.getFullYear(), today.getMonth(), 1);
            var lastDay = new Date(today.getFullYear(), today.getMonth() + 1, 0);
            
            document.getElementById('<%= dateFrom.ClientID %>').valueAsDate = firstDay;
            document.getElementById('<%= dateTo.ClientID %>').valueAsDate = lastDay;
        });
    </script>
</body>
</html>

