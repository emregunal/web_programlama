using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinik.Models;

public class Kullanici
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

    [Required]
    [StringLength(500)]
    public string ParolaOzeti { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public KullaniciRolu Rol { get; set; }

    [Display(Name = "Veteriner kaydı")]
    public int? VeterinerId { get; set; }

    [Display(Name = "Aktif")]
    public bool Aktif { get; set; } = true;

    [Display(Name = "Kayıt tarihi")]
    public DateTime KayitTarihi { get; set; }

    public Veteriner? Veteriner { get; set; }
}
