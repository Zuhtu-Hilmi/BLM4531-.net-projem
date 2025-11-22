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
    }

    public class KullaniciLoginModel
    {
        public string? KullaniciAdi { get; set; }
        public string? Sifre { get; set; }
    }
}