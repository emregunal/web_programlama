using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class AsiKaydiListeOgesi
{
    public int Id { get; set; }
    public int HayvanId { get; set; }
    public string HayvanAdi { get; set; } = string.Empty;
    public HayvanTuru HayvanTuru { get; set; }
    public int SahipId { get; set; }
    public string SahipAdSoyad { get; set; } = string.Empty;
    public string SahipTelefon { get; set; } = string.Empty;
    public string AsiAdi { get; set; } = string.Empty;
    public string? VeterinerAdSoyad { get; set; }
    public DateTime UygulamaTarihi { get; set; }
    public DateTime? SonrakiTarih { get; set; }
    public AsiDurumu Durum { get; set; }
}
