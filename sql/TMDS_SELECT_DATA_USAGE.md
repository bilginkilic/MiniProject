# TMDS Select Data Service Usage Guide

## Service Details
- **Endpoint**: `/BtmuAppsService/BtmuAppsWebService.asmx`
- **Operation**: TMDSSelectData
- **Protocol**: SOAP 1.1
- **Host**: trlvap1100

## Headers
```
Content-Type: text/xml; charset=utf-8
SOAPAction: "http://btmu.com/TMDSSelectData"
```

## SOAP Request Example
```xml
<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    <soap:Body>
        <TMDSSelectData xmlns="http://btmu.com/">
            <input>
                <tmBankaEftKod>string</tmBankaEftKod>
                <tmSubeKod>string</tmSubeKod>
                <tmSiraNo>string</tmSiraNo>
                <tmTutar>string</tmTutar>
                <tmTutarDoviz>string</tmTutarDoviz>
                <tmVadeBilgi>string</tmVadeBilgi>
            </input>
        </TMDSSelectData>
    </soap:Body>
</soap:Envelope>
```

## CURL Example
```bash
curl --location 'http://trlvap1100/BtmuAppsService/BtmuAppsWebService.asmx' \
--header 'Content-Type: text/xml; charset=utf-8' \
--header 'SOAPAction: "http://btmu.com/TMDSSelectData"' \
--data '<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    <soap:Body>
        <TMDSSelectData xmlns="http://btmu.com/">
            <input>
                <tmBankaEftKod>string</tmBankaEftKod>
                <tmSubeKod>string</tmSubeKod>
                <tmSiraNo>string</tmSiraNo>
                <tmTutar>string</tmTutar>
                <tmTutarDoviz>string</tmTutarDoviz>
                <tmVadeBilgi>string</tmVadeBilgi>
            </input>
        </TMDSSelectData>
    </soap:Body>
</soap:Envelope>'
```

## Postman Usage
1. Create a new POST request
2. Enter URL: `http://trlvap1100/BtmuAppsService/BtmuAppsWebService.asmx`
3. Add Headers:
   ```
   Content-Type: text/xml; charset=utf-8
   SOAPAction: "http://btmu.com/TMDSSelectData"
   ```
4. Body -> raw -> XML -> Paste the SOAP request XML

## Input Parameters
| Parameter | Type | Description |
|-----------|------|-------------|
| tmBankaEftKod | string | Banka EFT kodu |
| tmSubeKod | string | Şube kodu |
| tmSiraNo | string | Sıra numarası |
| tmTutar | string | Tutar |
| tmTutarDoviz | string | Tutar döviz cinsi |
| tmVadeBilgi | string | Vade bilgisi |

## Response
- Success: HTTP 200 OK with SOAP response
- Error: SOAP Fault message

## Notes
- Replace `string` placeholders with actual values
- Service is only available for requests from the local machine
- Use `https://` if SSL is required
- Add authentication headers if required
- Test environment: Local machine only
- Response format: XML
- Character encoding: UTF-8

## Error Handling
- Check response status code
- Parse SOAP Fault for error details
- Validate input parameters before sending
- Handle network timeouts
- Log all errors for troubleshooting

## Security Considerations
- Service is restricted to local machine access
- Use HTTPS in production
- Implement proper authentication if needed
- Validate input data
- Handle sensitive data appropriately
- Follow security best practices
