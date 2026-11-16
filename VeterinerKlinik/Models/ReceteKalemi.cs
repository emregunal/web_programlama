using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class ReceteKalemi
{
    public int Id { get; set; }

    [Display(Name = "Muayene")]
    public int MuayeneId { get; set; }

    [Required(ErrorMessage = "İlaç seçin.")]
    [Display(Name = "İlaç")]
    public int IlacId { get; set; }

    [Required(ErrorMessage = "Doz zorunludur.")]
    [StringLength(100, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Doz")]
    public string Doz { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Kullanım Şekli")]
    public string? KullanimSekli { get; set; }

    [Required(ErrorMessage = "Süre zorunludur.")]
    [Range(1, 365, ErrorMessage = "Süre 1 ile 365 gün arasında olmalıdır.")]
    [Display(Name = "Süre (gün)")]
    public int SureGun { get; set; }

    [Required(ErrorMessage = "Miktar zorunludur.")]
    [Range(1, 1000, ErrorMessage = "Miktar 1 ile 1000 arasında olmalıdır.")]
    [Display(Name = "Miktar")]
    public int Adet { get; set; }

    [Precision(10, 2)]
    [Display(Name = "Birim Fiyat")]
    public decimal BirimFiyat { get; set; }

    public Muayene? Muayene { get; set; }
    public Ilac? Ilac { get; set; }

    [NotMapped]
    public decimal Tutar => Adet * BirimFiyat;
}
