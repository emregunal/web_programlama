using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class Odeme
{
    public int Id { get; set; }

    [Display(Name = "Sahip")]
    public int SahipId { get; set; }

    [Display(Name = "Muayene")]
    public int? MuayeneId { get; set; }

    [Required(ErrorMessage = "Tutar zorunludur.")]
    [Range(typeof(decimal), "0.01", "1000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "Tutar 0,01 ile 1.000.000 ₺ arasında olmalıdır.")]
    [Precision(10, 2)]
    [Display(Name = "Tutar")]
    public decimal Tutar { get; set; }

    [Required(ErrorMessage = "Tarih zorunludur.")]
    [Display(Name = "Tarih")]
    [DataType(DataType.DateTime)]
    public DateTime Tarih { get; set; }

    [Required(ErrorMessage = "Ödeme yöntemi seçin.")]
    [Display(Name = "Ödeme Yöntemi")]
    public OdemeYontemi Yontem { get; set; }

    [StringLength(250, ErrorMessage = "{0} en fazla {1} karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public Sahip? Sahip { get; set; }
    public Muayene? Muayene { get; set; }
}
