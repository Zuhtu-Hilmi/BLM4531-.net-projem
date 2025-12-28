# \## 📱 Kullanıcı Arayüzü Özellikleri

# 

# \### Ana Sayfa (index.html)

# \- ⭐ Günün kelimesi kutusu (varsa otomatik gösterilir)

# \- 💬 Kelime arandığında örnek cümleler bölümü açılır

# \- ➕ Giriş yapan kullanıcılar örnek cümle ekleyebilir

# \- 📝 Cümleler anonim olarak gönderilir

# 

# \### Admin Paneli (admin.html)

# \- 📝 \*\*Kelime İşlemleri\*\*: Ekleme ve güncelleme

# \- 📩 \*\*Öneriler\*\*: Kullanıcı önerilerini onaylama/reddetme

# \- 💬 \*\*Örnek Cümleler\*\*: Bekleyen cümleleri onaylama/reddetme

# \- 📚 \*\*Tüm Kelimeler\*\*: Alfabetik listeleme ve filtreleme

# \- 📦 \*\*Arşiv\*\*: Tüm kelime değişikliklerinin geçmişi

# \- ⭐ \*\*Günün Kelimesi\*\*: Günlük kelime belirleme ve arşiv

# 

# \## 🔒 Güvenlik Notları

# 

# 1\. \*\*Anonim Gönderim\*\*: Örnek cümleler kullanıcılar tarafından anonim gönderilir, ancak admin panelinde göndericinin adı görünür.

# 

# 2\. \*\*Transaction Kullanımı\*\*: Kritik işlemler için SQL transaction kullanılır (öneri onaylama, kelime ekleme/güncelleme).

# 

# 3\. \*\*Veri Doğrulama\*\*: Tüm input'lar server-side'da kontrol edilir.

# 

# 4\. \*\*SQL Injection Koruması\*\*: Parametreli sorgular kullanılır.

# 

# \## 🎨 UI/UX İyileştirmeleri

# 

# \- Modern tab sistemi ile düzenli admin paneli

# \- Gradient renkli günün kelimesi kutusu

# \- Hover efektleri ve animasyonlar

# \- Responsive tasarım (mobil uyumlu)

# \- Kullanıcı dostu hata mesajları

# 

# \## 📊 Veritabanı İlişkileri

# 

# ```

# Kelimeler (Ana Tablo)

# &nbsp;   ├── OrnekCumleler (Foreign Key: KelimeId)

# &nbsp;   └── KelimeArsiv (Referans: Kelime adı)

# 

# Kullanicilar

# &nbsp;   ├── Favoriler (Foreign Key: KullaniciId)

# &nbsp;   ├── KelimeOnerileri (Foreign Key: KullaniciId)

# &nbsp;   └── OrnekCumleler (Foreign Key: KullaniciId)

# 

# GununKelimesi (Bağımsız Tablo)

# ```

# 

# \## 🔄 İş Akışları

# 

# \### Öneri Onaylama Akışı

# 1\. Kullanıcı kelime önerir → `KelimeOnerileri` tablosuna eklenir (Durum: Beklemede)

# 2\. Admin önerileri görür → `/api/sozluk/oneriler/bekleyen`

# 3\. Admin onayla butonuna basar → `/api/admin/oneri/onayla/{id}`

# 4\. Kelime `Kelimeler` tablosuna eklenir

# 5\. `KelimeOnerileri` durum alanı 'Onaylandi' olur

# 6\. İşlem `KelimeArsiv` tablosuna kaydedilir

# 

# \### Örnek Cümle Akışı

# 1\. Kullanıcı kelime arar ve örnek cümle ekler

# 2\. Cümle `OrnekCumleler` tablosuna eklenir (OnayDurumu: Beklemede)

# 3\. Admin bekleyen cümleleri görür

# 4\. Onaylanırsa cümle herkese görünür hale gelir

# 5\. Reddedilirse durum 'Reddedildi' olur (tabloda kalır ama gösterilmez)

# 

# \## 🎯 Gelecek Özellik Önerileri

# 

# \- \[ ] Kelime favorileme sistemi (mevcut)

# \- \[ ] Kullanıcı puanlama sistemi (örnek cümle ve öneri için)

# \- \[ ] Kelime arama geçmişi

# \- \[ ] Popüler kelimeler listesi

# \- \[ ] REST API dokümantasyonu (Swagger)

# \- \[ ] Kelime kategorileri/etiketleri

# \- \[ ] Sesli okuma özelliği (Text-to-Speech)

