namespace VeterinerKlinik.ViewModels;

public class SahipListeOgesi
{
    public int Id { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string? Eposta { get; set; }
    public int HayvanSayisi { get; set; }
    public DateTime KayitTarihi { get; set; }
}
