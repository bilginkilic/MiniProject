# MessageQue – E-posta Kuyruğu (MESAGEINQUE)

Mesajlar **sadece sabah 9** ve **akşam 5** saatlerinde alıcıya ulaşır. Uygulama `sendMailInQue` ile e-postayı hemen göndermez, `MESAGEINQUE` tablosuna ekler; cron job 09:00 ve 17:00'da tablodaki bekleyen kayıtları alıp `SendMail` ile gönderir.

## Kullanım

### sendMailInQue (çağıranlar)

```csharp
var service = new MailQueueService();
// E-posta kuyruğa eklenir; 9 veya 17'de gönderilir
int queueId = service.SendMailInQue(
    toEmail: "alici@example.com",
    subject: "Konu",
    body: "<p>HTML içerik</p>",
    isBodyHtml: true,
    fromEmail: null  // null ise config'deki FromEmail kullanılır
);
```

### Cron job (09:00 ve 17:00)

- **Windows Görev Zamanlayıcı** ile `MessageQue.exe`'yi günde iki kez çalıştırın: **09:00** ve **17:00**.
- Exe çalışınca `SP_MESAGEINQUE_GET_PENDING` ile bekleyen kayıtlar alınır, her biri için `SendMail` çağrılır, sonuç `SP_MESAGEINQUE_UPD` ile güncellenir.

## Veritabanı

1. `sql/CREATE_MESAGEINQUE.sql` – Tablo oluşturma
2. `sql/SP_MESAGEINQUE_GET_ALL.sql` – Select All
3. `sql/SP_MESAGEINQUE_GET_BY_ID.sql` – Select (Id ile)
4. `sql/SP_MESAGEINQUE_GET_PENDING.sql` – Cron için bekleyen kayıtlar
5. `sql/SP_MESAGEINQUE_INS.sql` – Insert
6. `sql/SP_MESAGEINQUE_UPD.sql` – Update

`App.config` içinde `MessageQueDb` connection string ve SMTP ayarlarını kendi ortamınıza göre düzenleyin.
