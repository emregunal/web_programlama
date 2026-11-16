using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinik.Models;

public class Randevu
{
    public int Id { get; set; }

    [Display(Name = "Hayvan")]
    public int HayvanId { get; set; }

    [Display(Name = "Veteriner")]
    public int VeterinerId { get; set; }

    [Display(Name = "Başlangıç")]
    [DataType(DataType.DateTime)]
    public DateTime Baslangic { get; set; }

    [Range(5, 480)]
    [Display(Name = "Süre (dk)")]
    public int SureDakika { get; set; } = 30;

    [StringLength(250)]
    [Display(Name = "Neden")]
    public string? Neden { get; set; }

    [Display(Name = "Durum")]
    public RandevuDurumu Durum { get; set; } = RandevuDurumu.Planlandi;

    public Hayvan? Hayvan { get; set; }
    public Veteriner? Veteriner { get; set; }
}
