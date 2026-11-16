using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class Muayene
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Hayvan seçin.")]
    [Display(Name = "Hayvan")]
    public int HayvanId { get; set; }

    [Required(ErrorMessage = "Veteriner seçin.")]
    [Display(Name = "Veteriner")]
    public int VeterinerId { get; set; }

    [Display(Name = "Randevu")]
    public int? RandevuId { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [Display(Name = "Tarih")]
    [DataType(DataType.DateTime)]
    public DateTime Tarih { get; set; }

    [StringLength(500, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Şikayet")]
    public string? Sikayet { get; set; }

    [Required(ErrorMessage = "Teşhis zorunludur.")]
    [StringLength(500, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Teşhis")]
    public string Teshis { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Tedavi")]
    public string? Tedavi { get; set; }

    [StringLength(1000, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Notlar")]
    public string? Notlar { get; set; }

    [Required(ErrorMessage = "Ücret zorunludur.")]
    [Range(typeof(decimal), "0", "100000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "Ücret 0 ile 100.000 ₺ arasında olmalıdır.")]
    [Precision(10, 2)]
    [Display(Name = "Ücret")]
    public decimal Ucret { get; set; }

    public Hayvan? Hayvan { get; set; }
    public Veteriner? Veteriner { get; set; }
    public Randevu? Randevu { get; set; }
    public ICollection<ReceteKalemi> ReceteKalemleri { get; set; } = new List<ReceteKalemi>();
}
