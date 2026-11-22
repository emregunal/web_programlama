using System.ComponentModel.DataAnnotations;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.ViewModels;

public class KullaniciFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 30 karakter arasında olmalıdır.")]
    [RegularExpression("^[a-zA-Z0-9._]+$", ErrorMessage = "Kullanıcı adında yalnızca harf, rakam, nokta ve alt çizgi olabilir.")]
    [Display(Name = "Kullanıcı adı")]
    public string KullaniciAdi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ad soyad zorunludur.")]
    [StringLength(100, ErrorMessage = "Ad soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad soyad")]
    public string AdSoyad { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public KullaniciRolu Rol { get; set; }

    [Display(Name = "Veteriner kaydı")]
    public int? VeterinerId { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;

    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Parola en fazla 100 karakter olabilir.")]
    [Display(Name = "Parola")]
    public string? YeniParola { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Parola tekrar")]
    public string? YeniParolaTekrar { get; set; }
}
