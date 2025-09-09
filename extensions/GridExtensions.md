# Grid Extensions

Bu dosya, DataGridView ile ilgili yaygın kullanılan extension metodlarını içerir.

## DataGridView'dan DataTable'a Dönüşüm

### Tüm Grid'i DataTable'a Dönüştürme

```csharp
public static DataTable ConvertGridToDataTable(this DataGridView dgv)
{
    DataTable dt = new DataTable();

    // Sütunları oluştur
    foreach (DataGridViewColumn column in dgv.Columns)
    {
        // Sütun görünür değilse atla (isteğe bağlı)
        if (!column.Visible) continue;

        // Sütun tipini belirle ve ekle
        Type columnType = column.ValueType ?? typeof(string);
        dt.Columns.Add(column.HeaderText, columnType);
    }

    // Satırları ekle
    foreach (DataGridViewRow row in dgv.Rows)
    {
        // Son boş satırı atla
        if (!row.IsNewRow)
        {
            DataRow dataRow = dt.NewRow();
            
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (!dgv.Columns[i].Visible) continue;

                // Null kontrolü yap
                dataRow[dgv.Columns[i].HeaderText] = row.Cells[i].Value ?? DBNull.Value;
            }

            dt.Rows.Add(dataRow);
        }
    }

    return dt;
}
```

### Sadece Seçili Satırları DataTable'a Dönüştürme

```csharp
public static DataTable ConvertSelectedGridRowsToDataTable(this DataGridView dgv)
{
    DataTable dt = new DataTable();

    // Sütunları oluştur
    foreach (DataGridViewColumn column in dgv.Columns)
    {
        if (!column.Visible) continue;
        dt.Columns.Add(column.HeaderText, column.ValueType ?? typeof(string));
    }

    // Sadece seçili satırları ekle
    foreach (DataGridViewRow row in dgv.SelectedRows)
    {
        DataRow dataRow = dt.NewRow();
        
        for (int i = 0; i < dgv.Columns.Count; i++)
        {
            if (!dgv.Columns[i].Visible) continue;
            dataRow[dgv.Columns[i].HeaderText] = row.Cells[i].Value ?? DBNull.Value;
        }

        dt.Rows.Add(dataRow);
    }

    return dt;
}
```

## Kullanım Örnekleri

```csharp
// Tüm grid'i dönüştürme
DataTable fullTable = dataGridView1.ConvertGridToDataTable();

// Sadece seçili satırları dönüştürme
DataTable selectedTable = dataGridView1.ConvertSelectedGridRowsToDataTable();
```

## Önemli Notlar

1. Bu extension metodları, grid verilerini olduğu gibi aktarır
2. Sütun başlıkları grid'deki HeaderText değerlerini kullanır
3. Veri tipleri korunur
4. DBNull değerler doğru şekilde işlenir
5. Görünmez sütunlar varsayılan olarak atlanır
