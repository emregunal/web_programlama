using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class KullaniciListeOgesi
{
    public int Id { get; set; }
    public string KullaniciAdi { get; set; } = string.Empty;
    public string AdSoyad { get; set; } = string.Empty;
    public KullaniciRolu Rol { get; set; }
    public string? VeterinerAdSoyad { get; set; }
    public bool Aktif { get; set; }
}
