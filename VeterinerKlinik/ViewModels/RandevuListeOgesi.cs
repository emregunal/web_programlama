using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class RandevuListeOgesi
{
    public int Id { get; set; }
    public DateTime Baslangic { get; set; }
    public DateTime Bitis { get; set; }
    public int HayvanId { get; set; }
    public string HayvanAdi { get; set; } = string.Empty;
    public HayvanTuru HayvanTuru { get; set; }
    public int SahipId { get; set; }
    public string SahipAdSoyad { get; set; } = string.Empty;
    public string VeterinerAdSoyad { get; set; } = string.Empty;
    public string? Neden { get; set; }
    public RandevuDurumu Durum { get; set; }
    public int? MuayeneId { get; set; }
}
