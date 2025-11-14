/**
 * PivotJS Excel Export Helper Functions
 * Bu dosya PivotJS pivot tablolarını Excel formatına aktarmak için yardımcı fonksiyonlar içerir.
 */

/**
 * Pivot tabloyu Excel formatına aktarır
 * @param {string} tableSelector - Pivot tablonun CSS selector'ı (örn: "#output table.pvtTable")
 * @param {string} fileName - İndirilecek dosya adı (varsayılan: "Pivot_Tablo.xlsx")
 * @param {string} sheetName - Excel çalışma sayfası adı (varsayılan: "Pivot Tablo")
 */
function exportPivotTableToExcel(tableSelector, fileName, sheetName) {
    // SheetJS kontrolü
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    const pivotTable = document.querySelector(tableSelector);
    
    if (!pivotTable) {
        throw new Error('Pivot tablo bulunamadı!');
    }

    try {
        // HTML tablosunu worksheet'e dönüştür
        const ws = XLSX.utils.table_to_sheet(pivotTable);
        
        // Sütun genişliklerini otomatik ayarla
        const colWidths = calculateColumnWidths(ws);
        ws['!cols'] = colWidths;
        
        // Workbook oluştur
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, sheetName || "Pivot Tablo");
        
        // Dosya adını ayarla
        const finalFileName = fileName || "Pivot_Tablo_" + new Date().toISOString().slice(0,10) + ".xlsx";
        
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
 * JSON verisini Excel formatına aktarır
 * @param {Array} data - Excel'e aktarılacak veri array'i
 * @param {string} fileName - İndirilecek dosya adı
 * @param {string} sheetName - Excel çalışma sayfası adı
 * @param {Array} colWidths - Sütun genişlikleri (opsiyonel)
 */
function exportJsonToExcel(data, fileName, sheetName, colWidths) {
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    if (!Array.isArray(data) || data.length === 0) {
        throw new Error('Geçerli bir veri array\'i gerekli!');
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
        
        // Dosya adını ayarla
        const finalFileName = fileName || "Veri_" + new Date().toISOString().slice(0,10) + ".xlsx";
        
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
        
        const finalFileName = fileName || "Pivot_Tablolar_" + new Date().toISOString().slice(0,10) + ".xlsx";
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
 * PivotJS pivot verisini (pivotData) Excel formatına aktarır
 * @param {Object} pivotData - PivotJS pivotData objesi
 * @param {string} fileName - İndirilecek dosya adı
 * @param {string} sheetName - Excel çalışma sayfası adı
 */
function exportPivotDataToExcel(pivotData, fileName, sheetName) {
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
    }

    try {
        // PivotData'yı 2D array'e dönüştür
        const rows = [];
        
        // Başlık satırı
        const headerRow = [];
        pivotData.getColKeys().forEach(colKey => {
            headerRow.push(colKey.join('-'));
        });
        rows.push(headerRow);
        
        // Veri satırları
        pivotData.getRowKeys().forEach(rowKey => {
            const row = [];
            row.push(rowKey.join('-'));
            
            pivotData.getColKeys().forEach(colKey => {
                const value = pivotData.getAggregator(rowKey, colKey).value();
                row.push(value || 0);
            });
            
            rows.push(row);
        });
        
        // 2D array'i worksheet'e dönüştür
        const ws = XLSX.utils.aoa_to_sheet(rows);
        
        // Sütun genişliklerini ayarla
        const colWidths = calculateColumnWidths(ws);
        ws['!cols'] = colWidths;
        
        // Workbook oluştur
        const wb = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, sheetName || "Pivot Veri");
        
        // Dosya adını ayarla
        const finalFileName = fileName || "Pivot_Veri_" + new Date().toISOString().slice(0,10) + ".xlsx";
        
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

// Export fonksiyonları (ES6 module desteği için)
if (typeof module !== 'undefined' && module.exports) {
    module.exports = {
        exportPivotTableToExcel,
        exportJsonToExcel,
        exportMultiplePivotTablesToExcel,
        exportPivotDataToExcel,
        calculateColumnWidths,
        calculateAutoColumnWidths
    };
}

