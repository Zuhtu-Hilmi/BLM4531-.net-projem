namespace WebApplication3
{
    // Controller'ın POST isteğinden veri alması için basit bir model
    public class YeniKelimeRequest
    {
        public string? Kelime { get; set; }
        public string? Anlam { get; set; }
    }
}
