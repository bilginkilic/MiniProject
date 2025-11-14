/**
 * PivotJS Excel Export Helper Functions
 * Bu dosya PivotJS (PivotTable.js) pivot tablolarını Excel formatına aktarmak için yardımcı fonksiyonlar içerir.
 * 
 * PivotTable.js Resmi Örnekler: https://pivottable.js.org/examples/
 * 
 * Bu helper fonksiyonları PivotTable.js'in resmi API'sine uygun olarak geliştirilmiştir.
 */

/**
 * PivotTable.js pivot tablosunu Excel formatına aktarır
 * PivotTable.js'in oluşturduğu HTML tablosunu Excel'e dönüştürür
 * 
 * @param {string} tableSelector - Pivot tablonun CSS selector'ı (örn: "#output table.pvtTable")
 * @param {string} fileName - İndirilecek dosya adı (varsayılan: "Pivot_Tablo.xlsx")
 * @param {string} sheetName - Excel çalışma sayfası adı (varsayılan: "Pivot Tablo")
 * @returns {Object} {success: boolean, fileName: string}
 */
function exportPivotTableToExcel(tableSelector, fileName, sheetName) {
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
 * PivotTable.js pivot verisini (pivotData) Excel formatına aktarır
 * PivotTable.js resmi API'sine uygun olarak geliştirilmiştir
 * 
 * @param {Object} pivotData - PivotTable.js pivotData objesi (pivot() veya pivotUI() onRefresh callback'inden)
 * @param {string} fileName - İndirilecek dosya adı
 * @param {string} sheetName - Excel çalışma sayfası adı
 * @param {Object} options - Ek seçenekler {includeRowLabels: true, includeColLabels: true}
 */
function exportPivotDataToExcel(pivotData, fileName, sheetName, options) {
    if (typeof XLSX === 'undefined') {
        throw new Error('SheetJS (XLSX) kütüphanesi yüklenmemiş!');
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
        calculateColumnWidths,
        calculateAutoColumnWidths
    };
}

