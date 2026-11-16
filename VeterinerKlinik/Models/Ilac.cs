using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class Ilac
{
    public int Id { get; set; }

    [Required(ErrorMessage = "İlaç adı zorunludur.")]
    [StringLength(100, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "İlaç Adı")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Form")]
    public string? Form { get; set; }

    [StringLength(30, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Birim")]
    public string? Birim { get; set; }

    [Required(ErrorMessage = "Birim fiyat zorunludur.")]
    [Range(typeof(decimal), "0.01", "100000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "Birim fiyat 0,01 ile 100.000 ₺ arasında olmalıdır.")]
    [Precision(10, 2)]
    [Display(Name = "Birim Fiyat")]
    public decimal BirimFiyat { get; set; }

    [NotMapped]
    public string AdVeForm => string.IsNullOrWhiteSpace(Form) ? Ad : $"{Ad} ({Form})";
}
