using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinik.Models;

public enum HayvanTuru
{
    [Display(Name = "Kedi")] Kedi,
    [Display(Name = "Köpek")] Kopek,
    [Display(Name = "Kuş")] Kus,
    [Display(Name = "Kemirgen")] Kemirgen,
    [Display(Name = "Sürüngen")] Surungen,
    [Display(Name = "Diğer")] Diger
}

public enum Cinsiyet
{
    [Display(Name = "Erkek")] Erkek,
    [Display(Name = "Dişi")] Disi
}

public enum RandevuDurumu
{
    [Display(Name = "Planlandı")] Planlandi,
    [Display(Name = "Tamamlandı")] Tamamlandi,
    [Display(Name = "İptal Edildi")] IptalEdildi,
    [Display(Name = "Gelmedi")] Gelmedi
}

public enum OdemeYontemi
{
    [Display(Name = "Nakit")] Nakit,
    [Display(Name = "Kredi Kartı")] KrediKarti,
    [Display(Name = "Havale")] Havale
}
