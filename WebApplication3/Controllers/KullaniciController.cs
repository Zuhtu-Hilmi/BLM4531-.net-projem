using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient; // MSSQL kütüphanesi

namespace WebApplication3.Controllers
{
    [Route("api/kullanici")]
    [ApiController]
    public class KullaniciController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public KullaniciController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("kayit")]
        public IActionResult KayitOl([FromBody] KullaniciLoginModel model)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("KullaniciBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();
                    // Kullanıcı adı var mı kontrolü
                    string kontrolSql = "SELECT COUNT(1) FROM Kullanicilar WHERE KullaniciAdi = @kadi";
                    using (SqlCommand kontrolKomut = new SqlCommand(kontrolSql, baglanti))
                    {
                        kontrolKomut.Parameters.AddWithValue("@kadi", model.KullaniciAdi);
                        int varMi = (int)kontrolKomut.ExecuteScalar();
                        if (varMi > 0) return Conflict("Bu kullanıcı adı zaten alınmış.");
                    }

                    // Kayıt işlemi
                    string sql = "INSERT INTO Kullanicilar (KullaniciAdi, Sifre) VALUES (@kadi, @sifre)";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@kadi", model.KullaniciAdi);
                        komut.Parameters.AddWithValue("@sifre", model.Sifre); // Gerçek projede şifre hashlenir!
                        komut.ExecuteNonQuery();
                    }
                    return Ok("Kayıt başarılı!");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        [HttpPost("giris")]
        public IActionResult GirisYap([FromBody] KullaniciLoginModel model)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("KullaniciBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT Id FROM Kullanicilar WHERE KullaniciAdi = @kadi AND Sifre = @sifre";
                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@kadi", model.KullaniciAdi);
                    komut.Parameters.AddWithValue("@sifre", model.Sifre);

                    object? sonuc = komut.ExecuteScalar();
                    if (sonuc != null)
                    {
                        // Giriş başarılı, ID'yi döndür
                        return Ok(new { Id = Convert.ToInt32(sonuc), Mesaj = "Giriş başarılı" });
                    }
                }
            }
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");
        }

        [HttpDelete("sil/{id}")]
        public IActionResult HesapSil(int id)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("KullaniciBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();

                // Transaction başlatıyoruz (Ya hepsi silinir ya hiçbiri)
                SqlTransaction transaction = baglanti.BeginTransaction();

                try
                {
                    // 1. ADIM: Önce kullanıcının favorilerini temizle
                    string favSilSql = "DELETE FROM Favoriler WHERE KullaniciId = @uid";
                    using (SqlCommand favKomut = new SqlCommand(favSilSql, baglanti, transaction))
                    {
                        favKomut.Parameters.AddWithValue("@uid", id);
                        favKomut.ExecuteNonQuery();
                    }

                    // 2. ADIM: Şimdi kullanıcının kendisini sil
                    string kulSilSql = "DELETE FROM Kullanicilar WHERE Id = @uid";
                    using (SqlCommand kulKomut = new SqlCommand(kulSilSql, baglanti, transaction))
                    {
                        kulKomut.Parameters.AddWithValue("@uid", id);
                        int etkilenen = kulKomut.ExecuteNonQuery();

                        // Eğer kullanıcı zaten yoksa?
                        if (etkilenen == 0)
                        {
                            transaction.Rollback(); // İşlemleri geri al
                            return NotFound("Kullanıcı bulunamadı.");
                        }
                    }

                    // Her şey yolunda gittiyse onayla
                    transaction.Commit();
                    return Ok("Hesabınız ve tüm verileriniz başarıyla silindi.");
                }
                catch (Exception ex)
                {
                    // Hata olursa hiçbir şeyi silme, eski haline getir
                    transaction.Rollback();
                    return StatusCode(500, "Silme işlemi sırasında hata oluştu: " + ex.Message);
                }
            }
        }
    }

    public class KullaniciLoginModel
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
    }
}