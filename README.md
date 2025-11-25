# WebApplication3 için PROJE DURUM RAPORU

1. TASARIM (FRONTEND) DURUMU:
   - Ana sayfa (index.html) tasarımı tamamlandı. Sade ve odaklanmayı sağlayan bir arayüz kodlandı.
   - Giriş ve Kayıt sayfaları (giris.html) hazırlandı.
   - JavaScript kullanılarak sayfa yenilenmeden (AJAX/Fetch) veri çekme işlemleri yapıldı.

2. VERİTABANI DURUMU:
   - Veritabanı yönetim sistemi olarak MSSQL (LocalDB) kullanıldı. (Firebase kullanılmadı).
   - 'Kelimeler', 'Kullanicilar' ve 'Favoriler' tabloları oluşturuldu.
   - ADO.NET (SqlClient) kullanılarak veritabanı bağlantısı sağlandı.

3. API (BACKEND) DURUMU:
   - .NET 6/7 Web API kullanıldı.
   - SozlukController: Kelime arama ve yeni kelime ekleme uçları (endpoints) çalışıyor.
   - KullaniciController: Giriş yapma ve kayıt olma işlemleri çalışıyor.
   - FavoriController: Kullanıcıların kelimeleri favorilere eklemesi ve listelemesi sağlanıyor.

PROJE ÖZETİ:
Proje şu an çalışır durumdadır. Kullanıcılar hesap oluşturabilir, giriş yapabilir, kelime arayabilir ve beğendikleri kelimeleri favorilerine ekleyebilirler.
