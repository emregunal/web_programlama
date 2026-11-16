using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VeterinerKlinik.Models;

public class Sahip
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

    [Required(ErrorMessage = "Telefon zorunludur.")]
    [StringLength(20)]
    [Phone]
    [Display(Name = "Telefon")]
    public string Telefon { get; set; } = string.Empty;

    [StringLength(100)]
    [EmailAddress]
    [Display(Name = "E-posta")]
    public string? Eposta { get; set; }

    [StringLength(250)]
    [Display(Name = "Adres")]
    public string? Adres { get; set; }

    [Display(Name = "Kayıt Tarihi")]
    [DataType(DataType.Date)]
    public DateTime KayitTarihi { get; set; }

    public ICollection<Hayvan> Hayvanlar { get; set; } = new List<Hayvan>();
    public ICollection<Odeme> Odemeler { get; set; } = new List<Odeme>();

    [NotMapped]
    [Display(Name = "Ad Soyad")]
    public string AdSoyad => $"{Ad} {Soyad}";
}
