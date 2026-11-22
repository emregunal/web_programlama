using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class Ilac
{
    public int Id { get; set; }

    [Required(ErrorMessage = "İlaç adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "İlaç Adı")]
    public string Ad { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Form")]
    public string? Form { get; set; }

    [StringLength(30)]
    [Display(Name = "Birim")]
    public string? Birim { get; set; }

    [Precision(10, 2)]
    [Display(Name = "Birim Fiyat")]
    public decimal BirimFiyat { get; set; }
}
