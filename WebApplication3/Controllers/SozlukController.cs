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


        [HttpPost]
        public IActionResult PostKelime([FromBody] YeniKelimeRequest yeniKelime)    //bu fonksiyon şu anda kullanım dışında
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
    }
}