Projen için GitHub veya klasör içinde kullanabileceğin, sade ve net bir **README.md** dosyası hazırladım.

---

# En Sade Sözlük - Web API Projesi

Bu proje, **ASP.NET Core Web API** altyapısı ve **ADO.NET** kullanılarak geliştirilmiş, performans odaklı ve etkileşimli bir çevrimiçi sözlük uygulamasıdır.

## 🚀 Özellikler

### Kullanıcı Arayüzü

* **Hızlı Arama:** Anlık arama önerileri (Autocomplete) ve hızlı sonuç getirme.
* **Üyelik Sistemi:** Kayıt olma, giriş yapma ve şifre değiştirme.
* **Kişiselleştirme:** Aranan kelimeleri favorilere ekleme ve geçmiş aramaları görüntüleme.
* **Etkileşim:** Yeni kelime veya örnek cümle önerebilme.

### Yönetici (Admin) Paneli

* **İçerik Yönetimi:** Yeni kelime ekleme, silme ve anlam güncelleme.
* **Onay Mekanizması:** Kullanıcılardan gelen kelime ve cümle önerilerini onaylama veya reddetme.
* **İstatistikler:** Toplam arama, günlük aktif kullanıcı ve popüler kelime analizleri.
* **Arşiv:** Kelime değişiklik geçmişini ve günün kelimesi arşivini görüntüleme.

## 🛠️ Teknolojiler

* **Backend:** ASP.NET Core Web API (.NET 6/8)
* **Veritabanı Erişimi:** ADO.NET (Saf SQL sorguları ile yüksek performans)
* **Veritabanı:** Microsoft SQL Server
* **Frontend:** HTML5, CSS3, Vanilla JavaScript (Fetch API)

## ⚙️ Kurulum

1. **Veritabanını Hazırlayın:**
SQL Server üzerinde `sozluk` ve `SozlukKullanici` adında iki veritabanı oluşturun. İlgili tabloları (`Kelimeler`, `Kullanicilar`, `Favoriler`, `AramaGecmisi` vb.) oluşturun.
2. **Bağlantı Ayarları:**
`appsettings.json` dosyasındaki Connection String alanlarını kendi sunucunuza göre düzenleyin:
```json
"ConnectionStrings": {
  "SozlukBaglanti": "Data Source=YOUR_SERVER;Initial Catalog=sozluk;...",
  "KullaniciBaglanti": "Data Source=YOUR_SERVER;Initial Catalog=SozlukKullanici;..."
}

```


3. **Projeyi Çalıştırın:**
Terminal üzerinden proje dizininde şu komutu çalıştırın:
```bash
dotnet run

```


Tarayıcıda `https://localhost:7073` veya `http://localhost:5016` adresine gidin.

## 📂 Proje Yapısı

* **/Controllers:** API uç noktalarını (Endpoints) yöneten sınıflar (`SozlukController`, `AdminController`, `KullaniciController` vb.).
* **/wwwroot:** Statik dosyalar (`index.html`, `admin.html`, `giris.html`).
* **Program.cs:** Servis kayıtları ve Middleware yapılandırması.