using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinik.Models;

public class ReceteKalemi
{
    public int Id { get; set; }

    [Display(Name = "Muayene")]
    public int MuayeneId { get; set; }

    [Display(Name = "İlaç")]
    public int IlacId { get; set; }

    [Required(ErrorMessage = "Doz zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Doz")]
    public string Doz { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Kullanım Şekli")]
    public string? KullanimSekli { get; set; }

    [Range(1, 365)]
    [Display(Name = "Süre (gün)")]
    public int SureGun { get; set; }

    [Range(1, 1000)]
    [Display(Name = "Adet")]
    public int Adet { get; set; }

    public Muayene? Muayene { get; set; }
    public Ilac? Ilac { get; set; }
}
