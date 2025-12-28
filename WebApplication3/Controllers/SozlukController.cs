using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WebApplication3.Controllers
{
    [Route("api/sozluk")]
    [ApiController]
    public class SozlukController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public SozlukController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{kelime}")]
        public IActionResult GetAnlam(string kelime)
        {
            string? anlam = null;
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            if (string.IsNullOrEmpty(baglantiDizesi))
            {
                return StatusCode(500, "HATA: Veritabanı bağlantı dizesi 'appsettings.json' dosyasında bulunamadı.");
            }

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();
                    string sqlSorgu = "SELECT Anlam FROM Kelimeler WHERE Kelime = @ArananKelime";
                    using (SqlCommand komut = new SqlCommand(sqlSorgu, baglanti))
                    {
                        komut.Parameters.AddWithValue("@ArananKelime", kelime);
                        object? sonuc = komut.ExecuteScalar();
                        if (sonuc != null)
                        {
                            anlam = sonuc.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return StatusCode(500, "SUNUCU HATASI: " + ex.Message);
                }
            }

            if (anlam != null)
            {
                return Ok(anlam);
            }
            else
            {
                return NotFound("Kelime bulunamadı");
            }
        }

        [HttpGet("ara")]
        public IActionResult KelimeAra([FromQuery] string sorgu)
        {
            if (string.IsNullOrEmpty(sorgu) || sorgu.Length < 2) return Ok(new List<string>());

            var oneriler = new List<string>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT TOP 5 Kelime FROM Kelimeler WHERE Kelime LIKE @sorgu + '%'";
                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@sorgu", sorgu);
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            oneriler.Add(okuyucu.GetString(0));
                        }
                    }
                }
            }
            return Ok(oneriler);
        }

        [HttpPost]
        public IActionResult PostKelime([FromBody] YeniKelimeRequest yeniKelime)
        {
            if (yeniKelime == null || string.IsNullOrEmpty(yeniKelime.Kelime) || string.IsNullOrEmpty(yeniKelime.Anlam))
            {
                return BadRequest("Kelime ve Anlam alanları boş olamaz.");
            }

            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");
            if (string.IsNullOrEmpty(baglantiDizesi))
            {
                return StatusCode(500, "HATA: Veritabanı bağlantı dizesi bulunamadı.");
            }

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();
                    SqlTransaction transaction = baglanti.BeginTransaction();

                    // Önce kelime var mı diye kontrol et
                    string sqlKontrol = "SELECT COUNT(1) FROM Kelimeler WHERE Kelime = @Kelime";
                    using (SqlCommand kontrolKomut = new SqlCommand(sqlKontrol, baglanti, transaction))
                    {
                        kontrolKomut.Parameters.AddWithValue("@Kelime", yeniKelime.Kelime);
                        int sayi = (int)kontrolKomut.ExecuteScalar();
                        if (sayi > 0)
                        {
                            transaction.Rollback();
                            return Conflict("Bu kelime zaten sözlükte mevcut.");
                        }
                    }

                    // Kelime yoksa ekle
                    string sqlEkle = "INSERT INTO Kelimeler (Kelime, Anlam) VALUES (@Kelime, @Anlam)";
                    using (SqlCommand komut = new SqlCommand(sqlEkle, baglanti, transaction))
                    {
                        komut.Parameters.AddWithValue("@Kelime", yeniKelime.Kelime);
                        komut.Parameters.AddWithValue("@Anlam", yeniKelime.Anlam);
                        komut.ExecuteNonQuery();
                    }

                    // Arşive kaydet
                    string arsivSql = "INSERT INTO KelimeArsiv (Kelime, YeniAnlam, Islem, Tarih) VALUES (@Kelime, @Anlam, 'Eklendi', GETDATE())";
                    using (SqlCommand arsivKomut = new SqlCommand(arsivSql, baglanti, transaction))
                    {
                        arsivKomut.Parameters.AddWithValue("@Kelime", yeniKelime.Kelime);
                        arsivKomut.Parameters.AddWithValue("@Anlam", yeniKelime.Anlam);
                        arsivKomut.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return StatusCode(500, "Veritabanı yazma hatası: " + ex.Message);
                }
            }

            return Ok("Kelime başarıyla eklendi.");
        }

        [HttpPut("guncelle")]
        public IActionResult KelimeGuncelle(YeniKelimeRequest model)
        {
            if (string.IsNullOrEmpty(model.Kelime) || string.IsNullOrEmpty(model.Anlam))
            {
                return BadRequest("Kelime ve anlam boş olamaz.");
            }

            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");
            if (string.IsNullOrEmpty(baglantiDizesi))
            {
                return StatusCode(500, "HATA: Veritabanı bağlantı dizesi bulunamadı.");
            }

            using (SqlConnection connection = new SqlConnection(baglantiDizesi))
            {
                connection.Open();
                SqlTransaction transaction = connection.BeginTransaction();

                try
                {
                    // Önce eski anlamı al
                    string eskiAnlam = "";
                    string getEskiSql = "SELECT Anlam FROM Kelimeler WHERE Kelime = @Kelime";
                    using (SqlCommand getEskiKomut = new SqlCommand(getEskiSql, connection, transaction))
                    {
                        getEskiKomut.Parameters.AddWithValue("@Kelime", model.Kelime);
                        object? sonuc = getEskiKomut.ExecuteScalar();
                        if (sonuc == null)
                        {
                            transaction.Rollback();
                            return NotFound("Bu kelime sistemde yok, önce eklemelisiniz.");
                        }
                        eskiAnlam = sonuc.ToString() ?? "";
                    }

                    // Anlamı güncelle
                    string updateQuery = "UPDATE Kelimeler SET Anlam = @Anlam WHERE Kelime = @Kelime";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection, transaction))
                    {
                        updateCmd.Parameters.AddWithValue("@Kelime", model.Kelime);
                        updateCmd.Parameters.AddWithValue("@Anlam", model.Anlam);
                        updateCmd.ExecuteNonQuery();
                    }

                    // Arşive kaydet
                    string arsivSql = "INSERT INTO KelimeArsiv (Kelime, EskiAnlam, YeniAnlam, Islem, Tarih) VALUES (@Kelime, @EskiAnlam, @YeniAnlam, 'Güncellendi', GETDATE())";
                    using (SqlCommand arsivKomut = new SqlCommand(arsivSql, connection, transaction))
                    {
                        arsivKomut.Parameters.AddWithValue("@Kelime", model.Kelime);
                        arsivKomut.Parameters.AddWithValue("@EskiAnlam", eskiAnlam);
                        arsivKomut.Parameters.AddWithValue("@YeniAnlam", model.Anlam);
                        arsivKomut.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }

            return Ok("Kelime anlamı başarıyla güncellendi.");
        }

        [HttpGet("harf/{basHarf}")]
        public IActionResult GetKelimelerByHarf(string basHarf)
        {
            var kelimeler = new List<string>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT Kelime FROM Kelimeler WHERE Kelime LIKE @harf + '%' ORDER BY Kelime";
                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@harf", basHarf);
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            kelimeler.Add(okuyucu.GetString(0));
                        }
                    }
                }
            }
            return Ok(kelimeler);
        }

        [HttpPost("oner")]
        public IActionResult KelimeOner([FromBody] OneriModel model)
        {
            if (string.IsNullOrEmpty(model.Kelime) || string.IsNullOrEmpty(model.Anlam))
            {
                return BadRequest("Kelime ve anlam boş olamaz.");
            }

            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");
            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();
                    string sql = "INSERT INTO KelimeOnerileri (KullaniciId, Kelime, OnerilenAnlam) VALUES (@uid, @kelime, @anlam)";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@uid", model.KullaniciId);
                        komut.Parameters.AddWithValue("@kelime", model.Kelime);
                        komut.Parameters.AddWithValue("@anlam", model.Anlam);
                        komut.ExecuteNonQuery();
                    }
                    return Ok("Öneriniz alındı, admin onayından sonra yayınlanacaktır.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        [HttpGet("harf-detayli/{harf}")]
        public IActionResult GetDetayliByHarf(string harf)
        {
            var kelimeler = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT Kelime, Anlam FROM Kelimeler WHERE Kelime LIKE @harf + '%' ORDER BY Kelime";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@harf", harf);
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            kelimeler.Add(new
                            {
                                Kelime = okuyucu.GetString(0),
                                Anlam = okuyucu.GetString(1)
                            });
                        }
                    }
                }
            }
            return Ok(kelimeler);
        }

        [HttpGet("oneriler/bekleyen")]
        public IActionResult GetBekleyenOneriler()
        {
            var oneriler = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                string sql = @"
            SELECT o.Id, o.Kelime, o.OnerilenAnlam, k.KullaniciAdi 
            FROM KelimeOnerileri o
            INNER JOIN SozlukKullanici.dbo.Kullanicilar k ON o.KullaniciId = k.Id
            WHERE o.Durum = 'Beklemede'
            ORDER BY o.Id DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            oneriler.Add(new
                            {
                                Id = okuyucu.GetInt32(0),
                                Kelime = okuyucu.GetString(1),
                                Anlam = okuyucu.GetString(2),
                                Gonderen = okuyucu.GetString(3)
                            });
                        }
                    }
                }
            }
            return Ok(oneriler);
        }

        // ÖRNEK CÜMLE EKLEME
        [HttpPost("ornek-cumle")]
        public IActionResult OrnekCumleEkle([FromBody] OrnekCumleModel model)
        {
            if (string.IsNullOrEmpty(model.Kelime) || string.IsNullOrEmpty(model.Cumle))
            {
                return BadRequest("Kelime ve cümle boş olamaz.");
            }

            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");
            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();
                    string sql = "INSERT INTO OrnekCumleler (KelimeId, KullaniciId, Cumle, OnayDurumu, Tarih) " +
                                 "VALUES ((SELECT Id FROM Kelimeler WHERE Kelime = @kelime), @uid, @cumle, 'Beklemede', GETDATE())";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@kelime", model.Kelime);
                        komut.Parameters.AddWithValue("@uid", model.KullaniciId);
                        komut.Parameters.AddWithValue("@cumle", model.Cumle);
                        komut.ExecuteNonQuery();
                    }
                    return Ok("Örnek cümleniz alındı, onaydan sonra yayınlanacaktır.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        // KELİMENİN ÖRNEK CÜMLELERİNİ GETİR
        [HttpGet("ornek-cumleler/{kelime}")]
        public IActionResult GetOrnekCumleler(string kelime)
        {
            var cumleler = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = @"
                    SELECT o.Id, o.Cumle, o.Tarih 
                    FROM OrnekCumleler o
                    INNER JOIN Kelimeler k ON o.KelimeId = k.Id
                    WHERE k.Kelime = @kelime AND o.OnayDurumu = 'Onaylandi'
                    ORDER BY o.Tarih DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@kelime", kelime);
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            cumleler.Add(new
                            {
                                Id = okuyucu.GetInt32(0),
                                Cumle = okuyucu.GetString(1),
                                Tarih = okuyucu.GetDateTime(2).ToString("dd.MM.yyyy")
                            });
                        }
                    }
                }
            }
            return Ok(cumleler);
        }

        // GÜNÜN KELİMESİNİ GETİR
        [HttpGet("gunun-kelimesi")]
        public IActionResult GetGununKelimesi()
        {
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT TOP 1 Kelime, Anlam, Tarih FROM GununKelimesi WHERE CAST(Tarih AS DATE) = CAST(GETDATE() AS DATE)";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        if (okuyucu.Read())
                        {
                            return Ok(new
                            {
                                Kelime = okuyucu.GetString(0),
                                Anlam = okuyucu.GetString(1),
                                Tarih = okuyucu.GetDateTime(2).ToString("dd.MM.yyyy")
                            });
                        }
                    }
                }
            }
            return NotFound("Bugün için günün kelimesi ayarlanmamış.");
        }

        // GÜNÜN KELİMESİ ARŞİVİ
        [HttpGet("gunun-kelimesi/arsiv")]
        public IActionResult GetGununKelimesiArsiv()
        {
            var arsiv = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT Kelime, Anlam, Tarih FROM GununKelimesi ORDER BY Tarih DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            arsiv.Add(new
                            {
                                Kelime = okuyucu.GetString(0),
                                Anlam = okuyucu.GetString(1),
                                Tarih = okuyucu.GetDateTime(2).ToString("dd.MM.yyyy")
                            });
                        }
                    }
                }
            }
            return Ok(arsiv);
        }

        public class OneriModel
        {
            public int KullaniciId { get; set; }
            public string? Kelime { get; set; }
            public string? Anlam { get; set; }
        }

        public class OrnekCumleModel
        {
            public string? Kelime { get; set; }
            public int KullaniciId { get; set; }
            public string? Cumle { get; set; }
        }
    }
}