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

    [Precision(10, 2)]
    [Display(Name = "Tutar")]
    public decimal Tutar { get; set; }

    [Display(Name = "Tarih")]
    [DataType(DataType.DateTime)]
    public DateTime Tarih { get; set; }

    [Display(Name = "Ödeme Yöntemi")]
    public OdemeYontemi Yontem { get; set; }

    [StringLength(250)]
    [Display(Name = "Açıklama")]
    public string? Aciklama { get; set; }

    public Sahip? Sahip { get; set; }
    public Muayene? Muayene { get; set; }
}
