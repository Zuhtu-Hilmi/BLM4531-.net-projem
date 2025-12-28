using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WebApplication3.Controllers
{
    [Route("api/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ÖNERİ ONAYLAMA
        [HttpPost("oneri/onayla/{id}")]
        public IActionResult OneriOnayla(int id)
        {
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();
                SqlTransaction transaction = baglanti.BeginTransaction();

                try
                {
                    // 1. Öneriyi getir
                    string getirSql = "SELECT Kelime, OnerilenAnlam FROM KelimeOnerileri WHERE Id = @id";
                    string? kelime = null;
                    string? anlam = null;

                    using (SqlCommand getirKomut = new SqlCommand(getirSql, baglanti, transaction))
                    {
                        getirKomut.Parameters.AddWithValue("@id", id);
                        using (SqlDataReader okuyucu = getirKomut.ExecuteReader())
                        {
                            if (okuyucu.Read())
                            {
                                kelime = okuyucu.GetString(0);
                                anlam = okuyucu.GetString(1);
                            }
                        }
                    }

                    if (kelime == null || anlam == null)
                    {
                        transaction.Rollback();
                        return NotFound("Öneri bulunamadı.");
                    }

                    // 2. Kelime zaten var mı kontrol et
                    string kontrolSql = "SELECT COUNT(1) FROM Kelimeler WHERE Kelime = @kelime";
                    using (SqlCommand kontrolKomut = new SqlCommand(kontrolSql, baglanti, transaction))
                    {
                        kontrolKomut.Parameters.AddWithValue("@kelime", kelime);
                        int varMi = (int)kontrolKomut.ExecuteScalar();

                        if (varMi > 0)
                        {
                            transaction.Rollback();
                            return Conflict("Bu kelime zaten sözlükte var.");
                        }
                    }

                    // 3. Kelimeyi sözlüğe ekle
                    string ekleSql = "INSERT INTO Kelimeler (Kelime, Anlam) VALUES (@kelime, @anlam)";
                    using (SqlCommand ekleKomut = new SqlCommand(ekleSql, baglanti, transaction))
                    {
                        ekleKomut.Parameters.AddWithValue("@kelime", kelime);
                        ekleKomut.Parameters.AddWithValue("@anlam", anlam);
                        ekleKomut.ExecuteNonQuery();
                    }

                    // 4. Öneri durumunu güncelle
                    string guncellemeSql = "UPDATE KelimeOnerileri SET Durum = 'Onaylandi' WHERE Id = @id";
                    using (SqlCommand guncellemeKomut = new SqlCommand(guncellemeSql, baglanti, transaction))
                    {
                        guncellemeKomut.Parameters.AddWithValue("@id", id);
                        guncellemeKomut.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return Ok("Öneri onaylandı ve sözlüğe eklendi.");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        // ÖNERİ REDDETME
        [HttpDelete("oneri/reddet/{id}")]
        public IActionResult OneriReddet(int id)
        {
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();

                try
                {
                    string sql = "UPDATE KelimeOnerileri SET Durum = 'Reddedildi' WHERE Id = @id";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@id", id);
                        int etkilenen = komut.ExecuteNonQuery();

                        if (etkilenen == 0)
                            return NotFound("Öneri bulunamadı.");

                        return Ok("Öneri reddedildi.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        // ARŞİV: Eklenen ve değiştirilen kelimeleri getir
        [HttpGet("arsiv")]
        public IActionResult GetArsiv()
        {
            var arsiv = new List<object>();
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();
                string sql = "SELECT Id, Kelime, EskiAnlam, YeniAnlam, Islem, Tarih FROM KelimeArsiv ORDER BY Tarih DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            arsiv.Add(new
                            {
                                Id = okuyucu.GetInt32(0),
                                Kelime = okuyucu.GetString(1),
                                EskiAnlam = okuyucu.IsDBNull(2) ? null : okuyucu.GetString(2),
                                YeniAnlam = okuyucu.GetString(3),
                                Islem = okuyucu.GetString(4),
                                Tarih = okuyucu.GetDateTime(5).ToString("dd.MM.yyyy HH:mm")
                            });
                        }
                    }
                }
            }
            return Ok(arsiv);
        }

        // TÜM KELİMELERİ GETIR (Harf filtresi ile)
        [HttpGet("kelimeler")]
        public IActionResult GetTumKelimeler([FromQuery] string? harf = null)
        {
            var kelimeler = new List<object>();
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();
                string sql;

                if (string.IsNullOrEmpty(harf))
                {
                    sql = "SELECT Id, Kelime, Anlam FROM Kelimeler ORDER BY Kelime";
                }
                else
                {
                    sql = "SELECT Id, Kelime, Anlam FROM Kelimeler WHERE Kelime LIKE @harf + '%' ORDER BY Kelime";
                }

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    if (!string.IsNullOrEmpty(harf))
                        komut.Parameters.AddWithValue("@harf", harf);

                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            kelimeler.Add(new
                            {
                                Id = okuyucu.GetInt32(0),
                                Kelime = okuyucu.GetString(1),
                                Anlam = okuyucu.GetString(2)
                            });
                        }
                    }
                }
            }
            return Ok(kelimeler);
        }

        // GÜNÜN KELİMESİ AYARLAMA
        [HttpPost("gunun-kelimesi")]
        public IActionResult GununKelimesiAyarla([FromBody] GununKelimesiModel model)
        {
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();

                try
                {
                    // Bugün için zaten var mı kontrol et
                    string kontrolSql = "SELECT COUNT(1) FROM GununKelimesi WHERE CAST(Tarih AS DATE) = CAST(GETDATE() AS DATE)";
                    using (SqlCommand kontrolKomut = new SqlCommand(kontrolSql, baglanti))
                    {
                        int varMi = (int)kontrolKomut.ExecuteScalar();
                        if (varMi > 0)
                            return Conflict("Bugün için günün kelimesi zaten ayarlanmış.");
                    }

                    // Ekle
                    string ekleSql = "INSERT INTO GununKelimesi (Kelime, Anlam, Tarih) VALUES (@kelime, @anlam, GETDATE())";
                    using (SqlCommand ekleKomut = new SqlCommand(ekleSql, baglanti))
                    {
                        ekleKomut.Parameters.AddWithValue("@kelime", model.Kelime);
                        ekleKomut.Parameters.AddWithValue("@anlam", model.Anlam);
                        ekleKomut.ExecuteNonQuery();
                    }

                    return Ok("Günün kelimesi ayarlandı.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        // BEKLEYEN ÖRNEK CÜMLELERİ GETİR
        [HttpGet("ornek-cumleler/bekleyen")]
        public IActionResult GetBekleyenOrnekCumleler()
        {
            var cumleler = new List<object>();
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();
                string sql = @"
                    SELECT o.Id, k.Kelime, o.Cumle, u.KullaniciAdi
                    FROM OrnekCumleler o
                    INNER JOIN Kelimeler k ON o.KelimeId = k.Id
                    INNER JOIN SozlukKullanici.dbo.Kullanicilar u ON o.KullaniciId = u.Id
                    WHERE o.OnayDurumu = 'Beklemede'
                    ORDER BY o.Tarih DESC";

                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            cumleler.Add(new
                            {
                                Id = okuyucu.GetInt32(0),
                                Kelime = okuyucu.GetString(1),
                                Cumle = okuyucu.GetString(2),
                                Gonderen = okuyucu.GetString(3)
                            });
                        }
                    }
                }
            }
            return Ok(cumleler);
        }

        // ÖRNEK CÜMLE ONAYLAMA
        [HttpPost("ornek-cumle/onayla/{id}")]
        public IActionResult OrnekCumleOnayla(int id)
        {
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();

                try
                {
                    string sql = "UPDATE OrnekCumleler SET OnayDurumu = 'Onaylandi' WHERE Id = @id";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@id", id);
                        int etkilenen = komut.ExecuteNonQuery();

                        if (etkilenen == 0)
                            return NotFound("Örnek cümle bulunamadı.");

                        return Ok("Örnek cümle onaylandı.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        // ÖRNEK CÜMLE REDDETME
        [HttpDelete("ornek-cumle/reddet/{id}")]
        public IActionResult OrnekCumleReddet(int id)
        {
            string? sozlukBaglanti = _configuration.GetConnectionString("SozlukBaglanti");

            using (SqlConnection baglanti = new SqlConnection(sozlukBaglanti))
            {
                baglanti.Open();

                try
                {
                    string sql = "UPDATE OrnekCumleler SET OnayDurumu = 'Reddedildi' WHERE Id = @id";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@id", id);
                        int etkilenen = komut.ExecuteNonQuery();

                        if (etkilenen == 0)
                            return NotFound("Örnek cümle bulunamadı.");

                        return Ok("Örnek cümle reddedildi.");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }
    }

    public class GununKelimesiModel
    {
        public string? Kelime { get; set; }
        public string? Anlam { get; set; }
    }
}