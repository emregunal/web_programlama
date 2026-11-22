using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class MuayeneDetayModel
{
    public Muayene Muayene { get; set; } = null!;
    public ReceteKalemi YeniKalem { get; set; } = new() { SureGun = 7, Adet = 1 };
    public decimal IlacToplami { get; set; }
    public decimal GenelToplam => Muayene.Ucret + IlacToplami;
}
