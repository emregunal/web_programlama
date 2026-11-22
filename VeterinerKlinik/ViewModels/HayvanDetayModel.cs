using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class HayvanDetayModel
{
    public Hayvan Hayvan { get; set; } = null!;
    public List<AsiKaydiListeOgesi> AsiKayitlari { get; set; } = new();
}
