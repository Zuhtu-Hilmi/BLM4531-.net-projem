# 📚 En Sade Sözlük - Modern Dictionary Application

> Türkçe kelime aramaları için minimalist, hızlı ve kullanıcı dostu bir sözlük uygulaması

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![HTML5](https://img.shields.io/badge/HTML5-E34F26?logo=html5&logoColor=white)](https://html.spec.whatwg.org/)
[![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?logo=javascript&logoColor=black)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

## 🌟 Özellikler

### Kullanıcı Özellikleri
- 🔍 **Hızlı Kelime Arama** - Gerçek zamanlı öneri sistemi ile
- ⭐ **Favoriler** - Beğendiğiniz kelimeleri kaydedin
- 🕐 **Arama Geçmişi** - Geçmişte aradığınız kelimeleri takip edin
- 🔥 **Popüler Kelimeler** - En çok arananan kelimeleri keşfedin
- 💬 **Örnek Cümleler** - Kullanıcılar örnek cümleler ekleyebilir
- 📖 **Fihrist Modu** - Alfabetik kelime listesi (çalışma modu)
- 💡 **Kelime Önerileri** - Sözlüğe katkıda bulunun
- 🌓 **Günün Kelimesi** - Her gün özel bir kelime

### Admin Özellikleri
- 📝 **Kelime Yönetimi** - Ekleme, güncelleme, silme
- 📩 **Öneri Yönetimi** - Kullanıcı önerilerini onaylama/reddetme
- 💬 **Örnek Cümle Yönetimi** - Cümleleri onaylama/reddetme
- 📊 **Detaylı İstatistikler** - Arama verileri ve trendler
- 📦 **Arşiv Sistemi** - Tüm değişikliklerin kaydı
- 📚 **Kelime Listeleme** - Harf bazlı filtreleme

## 🎬 Demo

### Ana Sayfa
```
┌─────────────────────────────────────────┐
│ ⭐ Günün Kelimesi: Algorithm            │
│ Bir problemi çözmek için...             │
└─────────────────────────────────────────┘

┌─────────────────────┐
│ [Kelime Ara...   🔍]│
└─────────────────────┘

🔥 Popüler Kelimeler      🕐 Arama Geçmişim
🥇 server (145)           • database 3x
🥈 algorithm (132)        • API 2x
🥉 database (98)          • server 5x
```

### Admin Paneli
```
┌─────────────────────────────────────────┐
│ 📝 Kelime İşlemleri | 📩 Öneriler       │
│ 💬 Örnek Cümleler  | 📚 Tüm Kelimeler   │
│ 📦 Arşiv           | ⭐ Günün Kelimesi  │
│ 📊 İstatistikler                        │
└─────────────────────────────────────────┘
```

## 🚀 Kurulum

### Gereksinimler
- .NET 8.0 SDK veya üzeri
- SQL Server LocalDB (veya tam SQL Server)
- Modern web tarayıcı (Chrome, Firefox, Edge)

### 1. Projeyi Klonlayın
```bash
git clone https://github.com/yourusername/en-sade-sozluk.git
cd en-sade-sozluk
```

### 2. Veritabanını Oluşturun

#### SQL Server Management Studio kullanarak:
```sql
-- 1. sozluk veritabanını oluştur
CREATE DATABASE sozluk;
GO

-- 2. SozlukKullanici veritabanını oluştur
CREATE DATABASE SozlukKullanici;
GO

-- 3. Tabloları oluştur
-- SQL/database_setup.sql dosyasını çalıştır
-- SQL/arama_gecmisi_tables.sql dosyasını çalıştır
```

#### Komut satırından:
```bash
sqlcmd -S (localdb)\MSSQLLocalDB -i SQL/database_setup.sql
sqlcmd -S (localdb)\MSSQLLocalDB -i SQL/arama_gecmisi_tables.sql
```

### 3. Bağlantı Ayarları

`appsettings.json` dosyasını kontrol edin:
```json
{
  "ConnectionStrings": {
    "SozlukBaglanti": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=sozluk;Integrated Security=True",
    "KullaniciBaglanti": "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=SozlukKullanici;Integrated Security=True"
  }
}
```

### 4. Projeyi Çalıştırın
```bash
dotnet restore
dotnet build
dotnet run
```

Tarayıcınızda açın: `https://localhost:7123`

### 5. Admin Hesabı Oluşturun
```sql
-- Varsayılan admin hesabı
INSERT INTO Kullanicilar (KullaniciAdi, Sifre) VALUES ('admin', '123456');
```

⚠️ **Güvenlik Uyarısı**: Production'da mutlaka güçlü şifre kullanın!

## 📁 Proje Yapısı

```
en-sade-sozluk/
├── Controllers/
│   ├── SozlukController.cs      # Kelime işlemleri
│   ├── KullaniciController.cs   # Kullanıcı yönetimi
│   ├── FavoriController.cs      # Favori işlemleri
│   ├── AdminController.cs       # Admin işlemleri
│   └── AramaController.cs       # Arama ve istatistikler
├── Models/
│   └── YeniKelimeRequest.cs     # DTO modelleri
├── wwwroot/
│   ├── index.html               # Ana sayfa
│   ├── admin.html               # Admin paneli
│   └── giris.html               # Giriş sayfası
├── SQL/
│   ├── database_setup.sql       # Veritabanı kurulum
│   └── arama_gecmisi_tables.sql # Arama geçmişi tabloları
├── appsettings.json             # Yapılandırma
├── Program.cs                   # Uygulama başlangıcı
└── README.md                    # Bu dosya
```

## 🗄️ Veritabanı Şeması

### sozluk Veritabanı
```
Kelimeler
├── Id (PK)
├── Kelime (UNIQUE)
└── Anlam

KelimeOnerileri
├── Id (PK)
├── KullaniciId (FK)
├── Kelime
├── OnerilenAnlam
└── Durum

OrnekCumleler
├── Id (PK)
├── KelimeId (FK)
├── KullaniciId (FK)
├── Cumle
├── OnayDurumu
└── Tarih

AramaGecmisi
├── Id (PK)
├── KullaniciId (FK, nullable)
├── Kelime
├── AramaTarihi
└── IpAdresi

KelimeArsiv
├── Id (PK)
├── Kelime
├── EskiAnlam
├── YeniAnlam
├── Islem
└── Tarih

GununKelimesi
├── Id (PK)
├── Kelime
├── Anlam
└── Tarih (UNIQUE)
```

### SozlukKullanici Veritabanı
```
Kullanicilar
├── Id (PK)
├── KullaniciAdi (UNIQUE)
└── Sifre

Favoriler
├── Id (PK)
├── KullaniciId (FK)
├── Kelime
└── Anlam
```

## 🔌 API Endpoints

### Kelime İşlemleri
```http
GET    /api/sozluk/{kelime}                  # Kelime anlamını getir
POST   /api/sozluk                            # Yeni kelime ekle
PUT    /api/sozluk/guncelle                   # Kelime güncelle
GET    /api/sozluk/ara?sorgu={text}           # Kelime ara (autocomplete)
GET    /api/sozluk/harf/{harf}                # Harfe göre kelimeler
GET    /api/sozluk/harf-detayli/{harf}        # Harfe göre kelimeler (detaylı)
POST   /api/sozluk/oner                       # Kelime öner
GET    /api/sozluk/oneriler/bekleyen          # Bekleyen öneriler
```

### Kullanıcı İşlemleri
```http
POST   /api/kullanici/kayit                   # Kayıt ol
POST   /api/kullanici/giris                   # Giriş yap
DELETE /api/kullanici/sil/{id}                # Hesabı sil
PUT    /api/kullanici/sifre-degistir          # Şifre değiştir
```

### Favori İşlemleri
```http
POST   /api/favori/ekle                       # Favoriye ekle
GET    /api/favori/listele/{kullaniciId}      # Favorileri listele
DELETE /api/favori/sil/{id}                   # Favoriden sil
```

### Arama ve İstatistikler
```http
POST   /api/arama/kaydet                      # Aramayı kaydet
GET    /api/arama/gecmis/{kullaniciId}        # Arama geçmişi
DELETE /api/arama/gecmis/temizle/{kullaniciId}# Geçmişi temizle
GET    /api/arama/populer                     # Popüler kelimeler
GET    /api/arama/populer/bugun               # Bugünün popülerleri
GET    /api/arama/istatistik                  # İstatistikler
```

### Admin İşlemleri
```http
POST   /api/admin/oneri/onayla/{id}           # Öneriyi onayla
DELETE /api/admin/oneri/reddet/{id}           # Öneriyi reddet
GET    /api/admin/arsiv                       # Arşivi getir
GET    /api/admin/kelimeler                   # Tüm kelimeleri listele
POST   /api/admin/gunun-kelimesi              # Günün kelimesini ayarla
POST   /api/admin/ornek-cumle/onayla/{id}     # Örnek cümleyi onayla
DELETE /api/admin/ornek-cumle/reddet/{id}     # Örnek cümleyi reddet
```

## 🎨 Teknolojiler

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **ADO.NET** - Veritabanı erişimi
- **SQL Server LocalDB** - Veritabanı

### Frontend
- **Vanilla JavaScript** - Framework kullanılmadan
- **HTML5 & CSS3** - Modern web standartları
- **Fetch API** - Backend iletişimi
- **LocalStorage** - Kullanıcı oturum yönetimi

### Özellikler
- ✅ RESTful API tasarımı
- ✅ Responsive (mobil uyumlu) tasarım
- ✅ Real-time search (anlık arama)
- ✅ Transaction kullanımı (veri tutarlılığı)
- ✅ SQL Injection koruması (parametreli sorgular)
- ✅ Index optimizasyonu (performans)

## 📊 Performans Optimizasyonları

### Veritabanı
```sql
-- Hızlı arama için index'ler
CREATE INDEX IX_Kelime ON Kelimeler(Kelime);
CREATE INDEX IX_AramaTarihi ON AramaGecmisi(AramaTarihi);
CREATE INDEX IX_KullaniciId ON AramaGecmisi(KullaniciId);
```

### Cache Stratejisi
- Popüler kelimeler: 5 dakika cache
- Günün kelimesi: Gün boyunca cache
- Statik içerik: Browser cache

### Veri Temizliği
```sql
-- 1 yıldan eski arama kayıtlarını sil
DELETE FROM AramaGecmisi 
WHERE AramaTarihi < DATEADD(YEAR, -1, GETDATE());
```

## 🔒 Güvenlik

### Mevcut Güvenlik Önlemleri
✅ SQL Injection koruması (parametreli sorgular)
✅ XSS koruması (HTML encoding)
✅ Transaction kullanımı (veri bütünlüğü)
✅ Input validation (veri doğrulama)

### Önerilen İyileştirmeler (Production için)
⚠️ Şifre hashleme (bcrypt/PBKDF2)
⚠️ JWT Token tabanlı authentication
⚠️ HTTPS zorunlu (HSTS)
⚠️ Rate limiting (spam koruması)
⚠️ CORS politikaları
⚠️ API key authentication (admin paneli)

## 🧪 Test Etme

### Manuel Test
```bash
# Kelime arama testi
curl https://localhost:7123/api/sozluk/server

# Popüler kelimeler testi
curl https://localhost:7123/api/arama/populer?limit=10

# Kullanıcı kaydı testi
curl -X POST https://localhost:7123/api/kullanici/kayit \
  -H "Content-Type: application/json" \
  -d '{"kullaniciAdi":"test","sifre":"123456"}'
```

### Swagger Kullanarak Test
1. Uygulamayı çalıştırın
2. API endpoint'lerini interaktif olarak test edin

## 📈 Geliştirme Roadmap

### v1.0 (Mevcut)
- ✅ Temel kelime arama
- ✅ Kullanıcı sistemi
- ✅ Favoriler
- ✅ Admin paneli
- ✅ Arama geçmişi
- ✅ Popüler kelimeler
- ✅ Örnek cümleler
- ✅ Günün kelimesi

### v1.1 (Planlanan)
- [ ] Şifre hashleme
- [ ] JWT authentication
- [ ] Email doğrulama
- [ ] Şifremi unuttum özelliği

### v1.2 (Planlanan)
- [ ] Kelime kategorileri
- [ ] Eş anlamlı kelimeler
- [ ] Zıt anlamlı kelimeler
- [ ] Telaffuz bilgisi

### v2.0 (Gelecek)
- [ ] Sesli okuma (Text-to-Speech)
- [ ] Görsel içerik (resimler)
- [ ] Oyunlaştırma (kelime oyunları)
- [ ] Sosyal özellikler (paylaşım)
- [ ] Mobil uygulama (React Native)
- [ ] AI destekli tanımlar

## 🤝 Katkıda Bulunma

Katkılarınızı bekliyoruz! İşte nasıl katkıda bulunabilirsiniz:

1. Bu projeyi fork edin
2. Feature branch oluşturun (`git checkout -b feature/YeniOzellik`)
3. Değişikliklerinizi commit edin (`git commit -m 'Yeni özellik eklendi'`)
4. Branch'inizi push edin (`git push origin feature/YeniOzellik`)
5. Pull Request oluşturun

### Katkı Kuralları
- Clean code prensiplerini takip edin
- Yorum satırları ekleyin (özellikle karmaşık mantıklar için)
- SQL injection'a karşı parametreli sorgu kullanın
- Responsive tasarıma uygun kod yazın

## 📝 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.
