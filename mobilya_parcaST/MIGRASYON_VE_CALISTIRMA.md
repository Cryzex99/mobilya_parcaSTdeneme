# Mobilya Stok Takip - Kurulum ve Çalıştırma

## 1. XAMPP MySQL Kurulumu

1. **XAMPP Control Panel**'i açın (`C:\xampp\xampp-control.exe`).
2. **MySQL** satırında **Start** butonuna tıklayın. Yeşil renk aldığında başlatılmıştır.
3. phpMyAdmin kullanacaksanız **Apache** servisini de başlatın.
4. Tarayıcıda `http://localhost/phpmyadmin` adresine gidin.

## 2. Veritabanı Oluşturma

phpMyAdmin'de:
1. Sol üst **"Yeni"** butonundan "Veritabanı" seçin.
2. Veritabanı adı: `MobilyaStokDb`
3. Karşılaştırma: `utf8mb4_general_ci` seçin.
4. **Oluştur** butonuna tıklayın.

## 3. Bağlantı Ayarları

Proje klasöründeki `appsettings.json` dosyası kontrol edin:

```json
{
  "Database": {
    "Server": "127.0.0.1",
    "Port": 3306,
    "Name": "MobilyaStokDb",
    "User": "root"
  }
}
```

**XAMPP varsayılanları:**
- Server: `127.0.0.1`
- Port: `3306`
- Kullanıcı: `root`
- Parola: **boş** (yazılmaz)

Farklı ayarlarınız varsa `appsettings.json` içinde değiştirin. Parola için Windows ortam değişkeni kullanın:

```powershell
$env:Database__Password = "parolanız"
```

Parola yoksa bu değişkeni tanımlamayın.

## 4. EF Core Migration

XAMPP MySQL servisi **çalışıyor** olmalıdır. Proje klasöründe PowerShell açıp şu komutları çalıştırın:

```powershell
# Migration oluştur
dotnet ef migrations add IlkKurulum

# Veritabanını güncelle
dotnet ef database update
```

Bu komutlar `Kategoriler` ve `Urunler` tablolarını oluşturur.

## 5. Uygulamayı Çalıştırma

Visual Studio'da:
1. Projeyi açın ve **Build > Build Solution** yapın.
2. **F5** tuşu veya Ctrl+F5 ile çalıştırın.
3. Uygulama açıldığında veritabanına bağlanır.

**Not:** MySQL servisi, uygulama açılırken çalışıyor olmalıdır. Hata alırsanız XAMPP kontrol panelinden MySQL'i yeniden başlatın.
