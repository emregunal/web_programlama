using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinik.Models;

public class Asi
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Aşı adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Aşı Adı")]
    public string Ad { get; set; } = string.Empty;

    [Range(1, 3650)]
    [Display(Name = "Tekrar Aralığı (gün)")]
    public int TekrarAraligiGun { get; set; }
}
