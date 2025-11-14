/**
 * PivotJS Excel Export Helper Functions
 * Bu dosya PivotJS (PivotTable.js) pivot tablolarını Excel formatına aktarmak için yardımcı fonksiyonlar içerir.
 * 
 * PivotTable.js Resmi Örnekler: https://pivottable.js.org/examples/
 * 
 * Bu helper fonksiyonları PivotTable.js'in resmi API'sine uygun olarak geliştirilmiştir.
 * 
 * IE UYUMLULUK: Sunucu tabanlı export kullanılır (Aspose.Cells ile)
 */

// IE uyumlu tarih formatı (slice yerine substring kullanır)
function formatDateForFileName(date) {
    if (!date) {
        date = new Date();
    }
    var isoString = date.toISOString();
    // IE uyumlu: substring kullan (slice yerine)
    return isoString.substring(0, 10);
}

// IE uyumlu XMLHttpRequest oluştur
function createXHR() {
    if (window.XMLHttpRequest) {
        return new XMLHttpRequest();
    } else if (window.ActiveXObject) {
        // IE 6-7 için
        try {
            return new ActiveXObject("Msxml2.XMLHTTP");
        } catch (e) {
            try {
                return new ActiveXObject("Microsoft.XMLHTTP");
            } catch (e) {
                throw new Error("XMLHttpRequest desteklenmiyor!");
            }
        }
    } else {
        throw new Error("XMLHttpRequest desteklenmiyor!");
    }
}

// Sunucu tabanlı Excel export (IE uyumlu)
function exportToExcelServer(data, fileName, sheetName, exportType, serverUrl) {
    serverUrl = serverUrl || "ExcelExport.ashx";
    
    return new Promise(function(resolve, reject) {
        try {
            // Export tipine göre doğru property adını kullan
            var exportData = {
                exportType: exportType || "pivotTable",
                fileName: fileName || "Export_" + formatDateForFileName() + ".xlsx",
                sheetName: sheetName || "Sheet1"
            };
            
            // Server-side handler'ın beklediği property adlarını kullan
            if (exportType === "pivotTable") {
                exportData.tableData = data;
            } else if (exportType === "pivotData") {
                exportData.pivotData = data;
            } else {
                // jsonData veya diğer tipler için
                exportData.data = data;
            }
            
            var jsonData = JSON.stringify(exportData);
            
            // Query string uzunluk limiti sorunlarını önlemek için her zaman POST kullan
            // GET yöntemi query string uzunluk limitlerini aşabilir (genellikle 2048 karakter)
            var xhr = createXHR();
            
            // POST ile gönder (query string limiti sorununu önler)
            xhr.open("POST", serverUrl, true);
            xhr.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        // IE uyumlu dosya indirme
                        if (window.navigator && window.navigator.msSaveOrOpenBlob) {
                            // IE 10+ için
                            var blob = new Blob([xhr.response], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                            window.navigator.msSaveOrOpenBlob(blob, fileName || "Export.xlsx");
                            resolve({ success: true, fileName: fileName });
                        } else if (window.Blob && window.URL && window.URL.createObjectURL) {
                            // Modern tarayıcılar için
                            var blob = new Blob([xhr.response], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
                            var url = window.URL.createObjectURL(blob);
                            var a = document.createElement("a");
                            a.href = url;
                            a.download = fileName || "Export.xlsx";
                            document.body.appendChild(a);
                            a.click();
                            document.body.removeChild(a);
                            window.URL.revokeObjectURL(url);
                            resolve({ success: true, fileName: fileName });
                        } else {
                            // Eski IE için form submit ile POST gönderimi
                            // Query string kullanmak yerine form ile POST yapıyoruz
                            var form = document.createElement("form");
                            form.method = "POST";
                            form.action = serverUrl;
                            form.style.display = "none";
                            
                            var input = document.createElement("input");
                            input.type = "hidden";
                            input.name = "data";
                            input.value = jsonData;
                            form.appendChild(input);
                            
                            document.body.appendChild(form);
                            form.submit();
                            
                            setTimeout(function() {
                                document.body.removeChild(form);
                                resolve({ success: true, fileName: fileName });
                            }, 1000);
                        }
                    } else {
                        try {
                            var errorResponse = JSON.parse(xhr.responseText);
                            reject(new Error(errorResponse.error || "Sunucu hatası"));
                        } catch (e) {
                            reject(new Error("Excel export hatası: " + xhr.status));
                        }
                    }
                }
            };
            
            xhr.responseType = "blob";
            xhr.send(jsonData);
        } catch (error) {
            reject(error);
        }
    });
}

/**
 * PivotTable.js pivot tablosunu Excel formatına aktarır (SUNUCU TABANLI - IE UYUMLU)
 * PivotTable.js'in oluşturduğu HTML tablosunu Excel'e dönüştürür
 * 
 * @param {string} tableSelector - Pivot tablonun CSS selector'ı (örn: "#output table.pvtTable")
 * @param {string} fileName - İndirilecek dosya adı (varsayılan: "Pivot_Tablo.xlsx")
 * @param {string} sheetName - Excel çalışma sayfası adı (varsayılan: "Pivot Tablo")
 * @param {string} serverUrl - Sunucu URL'i (varsayılan: "ExcelExport.ashx")
 * @param {boolean} useServer - Sunucu tabanlı export kullan (varsayılan: true - IE uyumluluğu için)
 * @returns {Object} {success: boolean, fileName: string}
 */
function exportPivotTableToExcel(tableSelector, fileName, sheetName, serverUrl, useServer) {
    // Varsayılan olarak sunucu tabanlı export kullan (IE uyumluluğu)
    if (useServer === undefined) {
        useServer = true;
    }
    
    // Sunucu tabanlı export
    if (useServer) {
        var pivotTable = document.querySelector(tableSelector);
        
        if (!pivotTable) {
            throw new Error('Pivot tablo bulunamadı! Lütfen geçerli bir CSS selector kullanın (örn: "#output table.pvtTable")');
        }
        
        // HTML tablosunu 2D array'e dönüştür
        var tableData = [];
        var rows = pivotTable.getElementsByTagName("tr");
        
        for (var i = 0; i < rows.length; i++) {
            var row = [];
            var cells = rows[i].getElementsByTagName("td");
            var thCells = rows[i].getElementsByTagName("th");
            
            // TH hücreleri (başlık)
            for (var j = 0; j < thCells.length; j++) {
                row.push(thCells[j].textContent || thCells[j].innerText);
            }
            
            // TD hücreleri (veri)
            for (var j = 0; j < cells.length; j++) {
                row.push(cells[j].textContent || cells[j].innerText);
            }
            
            if (row.length > 0) {
                tableData.push(row);
            }
        }
        
        return exportToExcelServer(tableData, fileName, sheetName || "Pivot Tablo", "pivotTable", serverUrl);
    }
    
    // Eski yöntem (client-side, IE uyumlu değil)
    return exportPivotTableToExcelClient(tableSelector, fileName, sheetName);
}

/**
 * PivotTable.js pivot tablosunu Excel formatına aktarır (CLIENT-SIDE - ESKİ YÖNTEM)
 * @private
 */
function exportPivotTableToExcelClient(tableSelector, fileName, sheetName) {
    // SheetJS kontrolü
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    const pivotTable = document.querySelector(tableSelector);
    
    if (!pivotTable) {
        throw new Error('Pivot tablo bulunamadı! Lütfen geçerli bir CSS selector kullanın (örn: "#output table.pvtTable")');
    }

    try {
        // HTML tablosunu worksheet'e dönüştür (PivotTable.js'in oluşturduğu tablo)
        const ws = XLSX.utils.table_to_sheet(pivotTable);
        
        // Sütun genişliklerini otomatik ayarla
        const colWidths = calculateColumnWidths(ws);
        ws['!cols'] = colWidths;
        
        // Workbook oluştur
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, sheetName || "Pivot Tablo");
        
        // Dosya adını ayarla (IE uyumlu)
        const finalFileName = fileName || "Pivot_Tablo_" + formatDateForFileName() + ".xlsx";
        
        // Excel dosyasını indir
        XLSX.writeFile(wb, finalFileName);
        
        return {
            success: true,
            fileName: finalFileName
        };
    } catch (error) {
        console.error("Excel export hatası:", error);
        throw error;
    }
}

/**
 * JSON verisini Excel formatına aktarır (SUNUCU TABANLI - IE UYUMLU)
 * @param {Array} data - Excel'e aktarılacak veri array'i
 * @param {string} fileName - İndirilecek dosya adı
 * @param {string} sheetName - Excel çalışma sayfası adı
 * @param {Array} colWidths - Sütun genişlikleri (opsiyonel - sunucu tabanlı export'ta kullanılmaz)
 * @param {string} serverUrl - Sunucu URL'i (varsayılan: "ExcelExport.ashx")
 * @param {boolean} useServer - Sunucu tabanlı export kullan (varsayılan: true - IE uyumluluğu için)
 */
function exportJsonToExcel(data, fileName, sheetName, colWidths, serverUrl, useServer) {
    // Varsayılan olarak sunucu tabanlı export kullan (IE uyumluluğu)
    if (useServer === undefined) {
        useServer = true;
    }
    
    if (!Array.isArray(data) || data.length === 0) {
        throw new Error('Geçerli bir veri array\'i gerekli!');
    }
    
    // Sunucu tabanlı export
    if (useServer) {
        return exportToExcelServer(data, fileName, sheetName || "Veri", "jsonData", serverUrl);
    }
    
    // Eski yöntem (client-side, IE uyumlu değil)
    return exportJsonToExcelClient(data, fileName, sheetName, colWidths);
}

/**
 * JSON verisini Excel formatına aktarır (CLIENT-SIDE - ESKİ YÖNTEM)
 * @private
 */
function exportJsonToExcelClient(data, fileName, sheetName, colWidths) {
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    try {
        // JSON verisini worksheet'e dönüştür
        const ws = XLSX.utils.json_to_sheet(data);
        
        // Sütun genişliklerini ayarla
        if (colWidths && Array.isArray(colWidths)) {
            ws['!cols'] = colWidths.map(w => ({ wch: w }));
        } else {
            // Otomatik sütun genişliği hesapla
            const autoWidths = calculateAutoColumnWidths(data);
            ws['!cols'] = autoWidths;
        }
        
        // Workbook oluştur
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, sheetName || "Veri");
        
        // Dosya adını ayarla (IE uyumlu)
        const finalFileName = fileName || "Veri_" + formatDateForFileName() + ".xlsx";
        
        // Excel dosyasını indir
        XLSX.writeFile(wb, finalFileName);
        
        return {
            success: true,
            fileName: finalFileName
        };
    } catch (error) {
        console.error("Excel export hatası:", error);
        throw error;
    }
}

/**
 * Birden fazla pivot tabloyu tek bir Excel dosyasına aktarır
 * @param {Array} tables - Tablo bilgileri array'i [{selector, sheetName}, ...]
 * @param {string} fileName - İndirilecek dosya adı
 */
function exportMultiplePivotTablesToExcel(tables, fileName) {
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    if (!Array.isArray(tables) || tables.length === 0) {
        throw new Error('Geçerli bir tablo array\'i gerekli!');
    }

    try {
        const wb = XLSX.utils.book_new();
        
        tables.forEach((table, index) => {
            const pivotTable = document.querySelector(table.selector);
            
            if (!pivotTable) {
                console.warn(`Tablo bulunamadı: ${table.selector}`);
                return;
            }
            
            const ws = XLSX.utils.table_to_sheet(pivotTable);
            const colWidths = calculateColumnWidths(ws);
            ws['!cols'] = colWidths;
            
            const sheetName = table.sheetName || `Sayfa${index + 1}`;
            XLSX.utils.book_append_sheet(wb, ws, sheetName);
        });
        
        const finalFileName = fileName || "Pivot_Tablolar_" + formatDateForFileName() + ".xlsx";
        XLSX.writeFile(wb, finalFileName);
        
        return {
            success: true,
            fileName: finalFileName,
            sheetCount: tables.length
        };
    } catch (error) {
        console.error("Excel export hatası:", error);
        throw error;
    }
}

/**
 * Worksheet'ten sütun genişliklerini hesaplar
 * @param {Object} ws - XLSX worksheet objesi
 * @returns {Array} Sütun genişlikleri array'i
 */
function calculateColumnWidths(ws) {
    const colWidths = [];
    const range = XLSX.utils.decode_range(ws['!ref'] || 'A1');
    
    for (let col = range.s.c; col <= range.e.c; col++) {
        let maxWidth = 10; // Minimum genişlik
        
        for (let row = range.s.r; row <= range.e.r; row++) {
            const cellAddress = XLSX.utils.encode_cell({ r: row, c: col });
            const cell = ws[cellAddress];
            
            if (cell && cell.v) {
                const cellValue = String(cell.v);
                maxWidth = Math.max(maxWidth, cellValue.length);
            }
        }
        
        colWidths.push({ wch: Math.min(maxWidth + 2, 50) }); // Max 50 karakter
    }
    
    return colWidths;
}

/**
 * JSON verisinden otomatik sütun genişliklerini hesaplar
 * @param {Array} data - Veri array'i
 * @returns {Array} Sütun genişlikleri array'i
 */
function calculateAutoColumnWidths(data) {
    if (!data || data.length === 0) {
        return [];
    }
    
    const keys = Object.keys(data[0]);
    const colWidths = [];
    
    keys.forEach(key => {
        let maxWidth = key.length; // Başlık genişliği
        
        data.forEach(row => {
            if (row[key] !== null && row[key] !== undefined) {
                const value = String(row[key]);
                maxWidth = Math.max(maxWidth, value.length);
            }
        });
        
        colWidths.push({ wch: Math.min(maxWidth + 2, 50) });
    });
    
    return colWidths;
}

/**
 * PivotTable.js pivot verisini (pivotData) Excel formatına aktarır (SUNUCU TABANLI - IE UYUMLU)
 * PivotTable.js resmi API'sine uygun olarak geliştirilmiştir
 * 
 * @param {Object} pivotData - PivotTable.js pivotData objesi (pivot() veya pivotUI() onRefresh callback'inden)
 * @param {string} fileName - İndirilecek dosya adı
 * @param {string} sheetName - Excel çalışma sayfası adı
 * @param {Object} options - Ek seçenekler {includeRowLabels: true, includeColLabels: true}
 * @param {string} serverUrl - Sunucu URL'i (varsayılan: "ExcelExport.ashx")
 * @param {boolean} useServer - Sunucu tabanlı export kullan (varsayılan: true - IE uyumluluğu için)
 */
function exportPivotDataToExcel(pivotData, fileName, sheetName, options, serverUrl, useServer) {
    // Varsayılan olarak sunucu tabanlı export kullan (IE uyumluluğu)
    if (useServer === undefined) {
        useServer = true;
    }
    
    if (!pivotData || typeof pivotData.getRowKeys !== 'function' || typeof pivotData.getColKeys !== 'function') {
        throw new Error('Geçerli bir PivotTable.js pivotData objesi gerekli!');
    }

    try {
        const opts = options || {};
        const includeRowLabels = opts.includeRowLabels !== false; // Varsayılan: true
        const includeColLabels = opts.includeColLabels !== false; // Varsayılan: true
        
        // PivotData'yı 2D array'e dönüştür (PivotTable.js resmi API'sine uygun)
        const rows = [];
        
        // Sütun başlıkları
        const colKeys = pivotData.getColKeys();
        const rowKeys = pivotData.getRowKeys();
        
        if (colKeys.length === 0 && rowKeys.length === 0) {
            throw new Error('Pivot tablo boş!');
        }
        
        // Başlık satırı oluştur
        const headerRow = [];
        if (includeRowLabels && rowKeys.length > 0) {
            // İlk sütun boş (row label'lar için)
            headerRow.push('');
        }
        
        if (includeColLabels) {
            colKeys.forEach(colKey => {
                // PivotTable.js'de colKey bir array olabilir
                const label = Array.isArray(colKey) ? colKey.join(' - ') : String(colKey);
                headerRow.push(label || '');
            });
        }
        
        if (headerRow.length > 0) {
            rows.push(headerRow);
        }
        
        // Veri satırları (PivotTable.js resmi API pattern'ine uygun)
        rowKeys.forEach(rowKey => {
            const row = [];
            
            // Row label ekle
            if (includeRowLabels) {
                const rowLabel = Array.isArray(rowKey) ? rowKey.join(' - ') : String(rowKey);
                row.push(rowLabel || '');
            }
            
            // Her sütun için değer al
            colKeys.forEach(colKey => {
                try {
                    const aggregator = pivotData.getAggregator(rowKey, colKey);
                    const value = aggregator ? aggregator.value() : null;
                    // Null/undefined değerleri boş string veya 0 olarak göster
                    row.push(value !== null && value !== undefined ? value : '');
                } catch (e) {
                    console.warn('Aggregator hatası:', e);
                    row.push('');
                }
            });
            
            rows.push(row);
        });
        
        // Eğer hiç veri yoksa uyar
        if (rows.length === 0) {
            throw new Error('Dışa aktarılacak veri bulunamadı!');
        }
        
        // Sunucu tabanlı export
        if (useServer) {
            // PivotData'yı sunucuya gönderebileceğimiz formata dönüştür
            var pivotDataForServer = {
                rowKeys: rowKeys.map(function(rk) {
                    return Array.isArray(rk) ? rk : [rk];
                }),
                colKeys: colKeys.map(function(ck) {
                    return Array.isArray(ck) ? ck : [ck];
                }),
                dataMatrix: rows.slice(1) // Header hariç
            };
            
            return exportToExcelServer(pivotDataForServer, fileName, sheetName || "Pivot Veri", "pivotData", serverUrl)
                .then(function(result) {
                    result.rowCount = rows.length;
                    result.colCount = rows.length > 0 ? rows[0].length : 0;
                    return result;
                });
        }
        
        // Eski yöntem (client-side, IE uyumlu değil)
        return exportPivotDataToExcelClient(pivotData, fileName, sheetName, options);
    } catch (error) {
        console.error("Excel export hatası:", error);
        throw error;
    }
}

/**
 * PivotTable.js pivot verisini Excel formatına aktarır (CLIENT-SIDE - ESKİ YÖNTEM)
 * @private
 */
function exportPivotDataToExcelClient(pivotData, fileName, sheetName, options) {
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    try {
        const opts = options || {};
        const includeRowLabels = opts.includeRowLabels !== false;
        const includeColLabels = opts.includeColLabels !== false;
        
        const rows = [];
        const colKeys = pivotData.getColKeys();
        const rowKeys = pivotData.getRowKeys();
        
        if (colKeys.length === 0 && rowKeys.length === 0) {
            throw new Error('Pivot tablo boş!');
        }
        
        const headerRow = [];
        if (includeRowLabels && rowKeys.length > 0) {
            headerRow.push('');
        }
        
        if (includeColLabels) {
            colKeys.forEach(colKey => {
                const label = Array.isArray(colKey) ? colKey.join(' - ') : String(colKey);
                headerRow.push(label || '');
            });
        }
        
        if (headerRow.length > 0) {
            rows.push(headerRow);
        }
        
        rowKeys.forEach(rowKey => {
            const row = [];
            
            if (includeRowLabels) {
                const rowLabel = Array.isArray(rowKey) ? rowKey.join(' - ') : String(rowKey);
                row.push(rowLabel || '');
            }
            
            colKeys.forEach(colKey => {
                try {
                    const aggregator = pivotData.getAggregator(rowKey, colKey);
                    const value = aggregator ? aggregator.value() : null;
                    row.push(value !== null && value !== undefined ? value : '');
                } catch (e) {
                    console.warn('Aggregator hatası:', e);
                    row.push('');
                }
            });
            
            rows.push(row);
        });
        
        if (rows.length === 0) {
            throw new Error('Dışa aktarılacak veri bulunamadı!');
        }
        
        const ws = XLSX.utils.aoa_to_sheet(rows);
        const colWidths = calculateColumnWidths(ws);
        ws['!cols'] = colWidths;
        
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, sheetName || "Pivot Veri");
        
        const finalFileName = fileName || "Pivot_Veri_" + formatDateForFileName() + ".xlsx";
        XLSX.writeFile(wb, finalFileName);
        
        return {
            success: true,
            fileName: finalFileName,
            rowCount: rows.length,
            colCount: rows.length > 0 ? rows[0].length : 0
        };
    } catch (error) {
        console.error("Excel export hatası:", error);
        throw error;
    }
}

// Export fonksiyonları (ES6 module desteği için)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        exportPivotTableToExcel,
        exportJsonToExcel,
        exportMultiplePivotTablesToExcel,
        exportPivotDataToExcel,
        exportToExcelServer,
        formatDateForFileName,
        createXHR,
        calculateColumnWidths,
        calculateAutoColumnWidths
    };
}

