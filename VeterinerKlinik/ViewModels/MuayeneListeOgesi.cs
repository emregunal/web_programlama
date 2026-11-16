using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class MuayeneListeOgesi
{
    public int Id { get; set; }
    public DateTime Tarih { get; set; }
    public string HayvanAdi { get; set; } = string.Empty;
    public HayvanTuru HayvanTuru { get; set; }
    public int SahipId { get; set; }
    public string SahipAdSoyad { get; set; } = string.Empty;
    public string VeterinerAdSoyad { get; set; } = string.Empty;
    public string Teshis { get; set; } = string.Empty;
    public decimal Ucret { get; set; }
    public decimal IlacToplami { get; set; }
    public decimal GenelToplam => Ucret + IlacToplami;
}
