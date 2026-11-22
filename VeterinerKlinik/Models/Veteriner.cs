using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinerKlinik.Models;

public class Veteriner
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Soyad")]
    public string Soyad { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Uzmanlık")]
    public string? Uzmanlik { get; set; }

    [StringLength(20)]
    [Phone]
    [Display(Name = "Telefon")]
    public string? Telefon { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;

    [NotMapped]
    [Display(Name = "Veteriner")]
    public string AdSoyad => $"{Ad} {Soyad}";
}
