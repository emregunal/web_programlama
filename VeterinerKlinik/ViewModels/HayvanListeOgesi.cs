using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class HayvanListeOgesi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public HayvanTuru Tur { get; set; }
    public string? Irk { get; set; }
    public Cinsiyet Cinsiyet { get; set; }
    public DateTime? DogumTarihi { get; set; }
    public int SahipId { get; set; }
    public string SahipAdSoyad { get; set; } = string.Empty;
    public int MuayeneSayisi { get; set; }
}
