using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace WebApplication3.Controllers
{
    [Route("api/favori")]
    [ApiController]
    public class FavoriController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FavoriController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("ekle")]
        public IActionResult Ekle([FromBody] FavoriEkleModel model)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("KullaniciBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                try
                {
                    baglanti.Open();
                    // Zaten ekli mi kontrolü
                    string kontrolSql = "SELECT COUNT(1) FROM Favoriler WHERE KullaniciId = @uid AND Kelime = @kelime";
                    using (SqlCommand k_komut = new SqlCommand(kontrolSql, baglanti))
                    {
                        k_komut.Parameters.AddWithValue("@uid", model.KullaniciId);
                        k_komut.Parameters.AddWithValue("@kelime", model.Kelime);
                        int sayi = (int)k_komut.ExecuteScalar();
                        if (sayi > 0) return Conflict("Bu kelime zaten favorilerinizde.");
                    }

                    // Ekleme
                    string sql = "INSERT INTO Favoriler (KullaniciId, Kelime, Anlam) VALUES (@uid, @kelime, @anlam)";
                    using (SqlCommand komut = new SqlCommand(sql, baglanti))
                    {
                        komut.Parameters.AddWithValue("@uid", model.KullaniciId);
                        komut.Parameters.AddWithValue("@kelime", model.Kelime);
                        komut.Parameters.AddWithValue("@anlam", model.Anlam);
                        komut.ExecuteNonQuery();
                    }
                    return Ok("Favorilere eklendi.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Hata: " + ex.Message);
                }
            }
        }

        [HttpGet("listele/{kullaniciId}")]
        public IActionResult Listele(int kullaniciId)
        {
            var favoriler = new List<object>();
            string? baglantiDizesi = _configuration.GetConnectionString("KullaniciBaglanti");

            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "SELECT Id, Kelime, Anlam FROM Favoriler WHERE KullaniciId = @uid ORDER BY Id DESC";
                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@uid", kullaniciId);
                    using (SqlDataReader okuyucu = komut.ExecuteReader())
                    {
                        while (okuyucu.Read())
                        {
                            favoriler.Add(new
                            {
                                Id = okuyucu.GetInt32(0),
                                Kelime = okuyucu.GetString(1),
                                Anlam = okuyucu.GetString(2)
                            });
                        }
                    }
                }
            }
            return Ok(favoriler);
        }

        // Favoriden Silme (Opsiyonel ama hocaya göstermek için iyi olur)
        [HttpDelete("sil/{id}")]
        public IActionResult Sil(int id)
        {
            string? baglantiDizesi = _configuration.GetConnectionString("KullaniciBaglanti");
            using (SqlConnection baglanti = new SqlConnection(baglantiDizesi))
            {
                baglanti.Open();
                string sql = "DELETE FROM Favoriler WHERE Id = @id";
                using (SqlCommand komut = new SqlCommand(sql, baglanti))
                {
                    komut.Parameters.AddWithValue("@id", id);
                    komut.ExecuteNonQuery();
                }
            }
            return Ok("Silindi");
        }
    }

    public class FavoriEkleModel
    {
        public int KullaniciId { get; set; }
        public string? Kelime { get; set; }
        public string? Anlam { get; set; }
    }
}