using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WebApplication3.Controllers
{
    [Route("api/arama")]
    [ApiController]
    public class AramaController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AramaController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ARAMA KAYDET (Her arama sonrası otomatik çağrılacak)
        [HttpPost("kaydet")]
        public IActionResult AramaKaydet([FromBody] AramaKayitModel model)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();

                    // IP adresi alma (opsiyonel)
                    string? ipAdresi = HttpContext.Connection.RemoteIpAddress?.ToString();

                    string sql = "INSERT INTO AramaGecmisi (KullaniciId, Kelime, AramaTarihi, IpAdresi) " +
                                 "VALUES (@uid, @kelime, GETDATE(), @ip)";

                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@uid", model.KullaniciId.HasValue ? model.KullaniciId.Value : DBNull.Value);
                        komut.Parameters.AddWithValue("@kelime", model.Kelime);
                        komut.Parameters.AddWithValue("@ip", ipAdresi ?? (object)DBNull.Value);

                        komut.ExecuteNonQuery();
                    }

                    return Ok();
                }
                catch (Exception ex)
                {
                    // Arama kaydı hata verse bile kullanıcıyı etkilemesin
                    Console.WriteLine("Arama kaydedilemedi: " + ex.Message);
                    return Ok(); // Yine de OK dön
                }
            }
        }

        // KULLANICININ ARAMA GEÇMİŞİ
        [HttpGet("gecmis/{kullaniciId}")]
        public IActionResult AramaGecmisi(int kullaniciId, [FromQuery] int limit = 50)
        {
            var gecmis = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                // Son aramaları getir, tekrar edenleri grupla
                string sql = @"
                    SELECT TOP (@limit)
                        Kelime,
                        MAX(AramaTarihi) AS SonArama,
                        COUNT(*) AS AramaSayisi
                    FROM AramaGecmisi
                    WHERE KullaniciId = @uid
                    GROUP BY Kelime
                    ORDER BY MAX(AramaTarihi) DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@uid", kullaniciId);
                    komut.Parameters.AddWithValue("@limit", limit);

                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            gecmis.Add(new
                            {
                                Kelime = okuyucu.GetString(0),
                                SonArama = okuyucu.GetDateTime(1).ToString("dd.MM.yyyy HH:mm"),
                                AramaSayisi = okuyucu.GetInt32(2)
                            });
                        }
                    }
                }
            }
            return Ok(gecmis);
        }

        // KULLANICININ ARAMA GEÇMİŞİNİ TEMİZLE
        [HttpDelete("gecmis/temizle/{kullaniciId}")]
        public IActionResult GecmisiTemizle(int kullaniciId)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "DELETE FROM AramaGecmisi WHERE KullaniciId = @uid";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@uid", kullaniciId);
                    int silinenSayisi = komut.ExecuteNonQuery();
                    return Ok($"{silinenSayisi} adet kayıt silindi.");
                }
            }
        }

        // POPÜLER KELİMELER (Son 30 gün)
        [HttpGet("populer")]
        public IActionResult PopulerKelimeler([FromQuery] int limit = 20, [FromQuery] int gunSayisi = 30)
        {
            var populerler = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                string sql = @"
                    SELECT TOP (@limit)
                        Kelime,
                        COUNT(*) AS AramaSayisi,
                        COUNT(DISTINCT KullaniciId) AS FarkliKullaniciSayisi,
                        MAX(AramaTarihi) AS SonArama
                    FROM AramaGecmisi
                    WHERE AramaTarihi >= DATEADD(DAY, -@gunSayisi, GETDATE())
                    GROUP BY Kelime
                    ORDER BY COUNT(*) DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@limit", limit);
                    komut.Parameters.AddWithValue("@gunSayisi", gunSayisi);

                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            populerler.Add(new
                            {
                                Kelime = okuyucu.GetString(0),
                                AramaSayisi = okuyucu.GetInt32(1),
                                FarkliKullaniciSayisi = okuyucu.GetInt32(2),
                                SonArama = okuyucu.GetDateTime(3).ToString("dd.MM.yyyy")
                            });
                        }
                    }
                }
            }
            return Ok(populerler);
        }

        // BUGÜN EN ÇOK ARANANANLAR
        [HttpGet("populer/bugun")]
        public IActionResult BugunPopulerler([FromQuery] int limit = 10)
        {
            var populerler = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                string sql = @"
                    SELECT TOP (@limit)
                        Kelime,
                        COUNT(*) AS AramaSayisi
                    FROM AramaGecmisi
                    WHERE CAST(AramaTarihi AS DATE) = CAST(GETDATE() AS DATE)
                    GROUP BY Kelime
                    ORDER BY COUNT(*) DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@limit", limit);

                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            populerler.Add(new
                            {
                                Kelime = okuyucu.GetString(0),
                                AramaSayisi = okuyucu.GetInt32(1)
                            });
                        }
                    }
                }
            }
            return Ok(populerler);
        }

        // İSTATİSTİKLER (Admin için)
        [HttpGet("istatistik")]
        public IActionResult Istatistik()
        {
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                // Toplam arama sayısı
                int toplamArama = 0;
                using (SqlCommand komut = new SqlCommand("SELECT COUNT(*) FROM AramaGecmisi", baglanti))
                {
                    toplamArama = (int)komut.ExecuteScalar();
                }

                // Bugünkü aramalar
                int bugunArama = 0;
                using (SqlCommand komut = new SqlCommand(
                    "SELECT COUNT(*) FROM AramaGecmisi WHERE CAST(AramaTarihi AS DATE) = CAST(GETDATE() AS DATE)",
                    baglanti))
                {
                    bugunArama = (int)komut.ExecuteScalar();
                }

                // Farklı kelime sayısı
                int farkliKelime = 0;
                using (SqlCommand komut = new SqlCommand("SELECT COUNT(DISTINCT Kelime) FROM AramaGecmisi", baglanti))
                {
                    farkliKelime = (int)komut.ExecuteScalar();
                }

                // Aktif kullanıcı sayısı (son 24 saat)
                int aktifKullanici = 0;
                using (SqlCommand komut = new SqlCommand(
                    "SELECT COUNT(DISTINCT KullaniciId) FROM AramaGecmisi WHERE AramaTarihi >= DATEADD(HOUR, -24, GETDATE()) AND KullaniciId IS NOT NULL",
                    baglanti))
                {
                    aktifKullanici = (int)komut.ExecuteScalar();
                }

                return Ok(new
                {
                    ToplamAramaSayisi = toplamArama,
                    BugunkuAramalar = bugunArama,
                    FarkliKelimelSayisi = farkliKelime,
                    AktifKullaniciSayisi24Saat = aktifKullanici
                });
            }
        }

        // TEMİZLİK (Eski kayıtları sil - Admin)
        [HttpDelete("temizlik")]
        public IActionResult EskiKayitlariSil()
        {
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                // 1 yıldan eski kayıtları sil
                string sql = "DELETE FROM AramaGecmisi WHERE AramaTarihi < DATEADD(YEAR, -1, GETDATE())";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    int silinenSayisi = komut.ExecuteNonQuery();
                    return Ok($"{silinenSayisi} adet eski kayıt silindi.");
                }
            }
        }
    }

    // Model sınıfları
    public class AramaKayitModel
    {
        public int? KullaniciId { get; set; } // Nullable - giriş yapmamış kullanıcılar için
        public string? Kelime { get; set; }
    }
}