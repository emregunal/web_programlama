namespace VeterinerKlinik.ViewModels;

public class IlacListeOgesi
{
    public int Id { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string? Form { get; set; }
    public string? Birim { get; set; }
    public decimal BirimFiyat { get; set; }
    public int KullanimSayisi { get; set; }
}
