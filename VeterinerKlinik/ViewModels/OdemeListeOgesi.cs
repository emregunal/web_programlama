using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class OdemeListeOgesi
{
    public int Id { get; set; }
    public DateTime Tarih { get; set; }
    public int SahipId { get; set; }
    public string SahipAdSoyad { get; set; } = string.Empty;
    public int? MuayeneId { get; set; }
    public string? HayvanAdi { get; set; }
    public OdemeYontemi Yontem { get; set; }
    public decimal Tutar { get; set; }
    public string? Aciklama { get; set; }
}
