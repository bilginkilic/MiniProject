# Stored Procedure İsimlendirme Standardı

## AuthDetail SP'leri
1. `[dbo].[SP_SGN_AUTHDETAIL_INSERT]`
2. `[dbo].[SP_SGN_AUTHDETAIL_UPDATE]`
3. `[dbo].[SP_SGN_AUTHDETAIL_DELETE]`
4. `[dbo].[SP_SGN_AUTHDETAIL_SELECT_BY_ID]`
5. `[dbo].[SP_SGN_AUTHDETAIL_SELECT_BY_CIRCULAR]`
6. `[dbo].[SP_SGN_AUTHDETAIL_SEARCH]`
7. `[dbo].[SP_SGN_AUTHDETAIL_GET_ALL_ACTIVE]`
8. `[dbo].[SP_SGN_AUTHDETAIL_GET_WITH_SIGNATURES]`

## AuthDetail Signatures SP'leri
1. `[dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_INSERT]`
2. `[dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_UPDATE]`
3. `[dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_DELETE]`
4. `[dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_ID]`
5. `[dbo].[SP_SGN_AUTHDETAIL_SIGNATURES_SELECT_BY_AUTHDETAIL]`

## Circular SP'leri
1. `[dbo].[SP_SGN_CIRCULAR_INS_SP]`
2. `[dbo].[SP_SGN_CIRCULAR_UPD_SP]`
3. `[dbo].[SP_SGN_CIRCULAR_SEL_SP]`
4. `[dbo].[SP_SGN_CIRCULAR_GET_ALL_ACTIVE]`

## İsimlendirme Kuralları
1. Schema: `[dbo]`
2. Prefix: `SP_SGN_`
3. Tablo Adı: `AUTHDETAIL` veya `CIRCULAR`
4. İşlem Tipi: 
   - INSERT/INS
   - UPDATE/UPD
   - SELECT/SEL
   - DELETE
   - SEARCH
   - GET_ALL_ACTIVE
   - GET_WITH_SIGNATURES
5. Detay: `_BY_ID`, `_BY_CIRCULAR`, `_BY_AUTHDETAIL` gibi

## Kullanım
```csharp
private const string SCHEMA = "dbo.SP_SGN_";
db.SetSpCommand(string.Format("{0}AUTHDETAIL_INSERT", SCHEMA))
```
