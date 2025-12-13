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
                    // Hata ayıklama için gerçek hatayı döndür
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
                // SQL Injection'a karşı parametre kullanımı ve 'LIKE' sorgusu
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
                    // Önce kelime var mı diye kontrol et
                    string sqlKontrol = "SELECT COUNT(1) FROM Kelimeler WHERE Kelime = @Kelime";
                    using (SqlCommand kontrolKomut = new SqlCommand(sqlKontrol, baglanti))
                    {
                        kontrolKomut.Parameters.AddWithValue("@Kelime", yeniKelime.Kelime);
                        int sayi = (int)kontrolKomut.ExecuteScalar();
                        if (sayi > 0)
                        {
                            return Conflict("Bu kelime zaten sözlükte mevcut.");
                        }
                    }

                    // Kelime yoksa ekle
                    string sqlEkle = "INSERT INTO Kelimeler (Kelime, Anlam) VALUES (@Kelime, @Anlam)";
                    using (SqlCommand komut = new SqlCommand(sqlEkle, baglanti))
                    {
                        komut.Parameters.AddWithValue("@Kelime", yeniKelime.Kelime);
                        komut.Parameters.AddWithValue("@Anlam", yeniKelime.Anlam);
                        komut.ExecuteNonQuery();
                    }
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
            // Veri boş mu kontrolü
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

                // Önce kelime var mı diye bakalım
                string kontrolQuery = "SELECT COUNT(*) FROM Kelimeler WHERE Kelime = @Kelime";
                using (SqlCommand kontrolCmd = new SqlCommand(kontrolQuery, connection))
                {
                    kontrolCmd.Parameters.AddWithValue("@Kelime", model.Kelime);
                    int varMi = (int)kontrolCmd.ExecuteScalar();

                    if (varMi == 0)
                    {
                        return NotFound("Bu kelime sistemde yok, önce eklemelisiniz.");
                    }
                }

                // Kelime varsa anlamını güncelle (UPDATE)
                string updateQuery = "UPDATE Kelimeler SET Anlam = @Anlam WHERE Kelime = @Kelime";
                using (SqlCommand updateCmd = new SqlCommand(updateQuery, connection))
                {
                    updateCmd.Parameters.AddWithValue("@Kelime", model.Kelime);
                    updateCmd.Parameters.AddWithValue("@Anlam", model.Anlam);
                    updateCmd.ExecuteNonQuery();
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
                    // Aynı kelime zaten önerilmiş mi veya sözlükte var mı kontrolü yapılabilir (İsteğe bağlı)

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
            // Listeyi "dynamic" veya özel bir sınıf olarak tutabiliriz.
            var kelimeler = new List<object>();

            string? baglantiDizesi = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                // Hem kelimeyi hem anlamı çekiyoruz
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

        // Dosyanın en altına (namespace içine) bu modeli ekleyin:
        public class OneriModel
        {
            public int KullaniciId { get; set; }
            public string? Kelime { get; set; }
            public string? Anlam { get; set; }
        }
    }
}