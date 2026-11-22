using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class Muayene
{
    public int Id { get; set; }

    [Display(Name = "Hayvan")]
    public int HayvanId { get; set; }

    [Display(Name = "Veteriner")]
    public int VeterinerId { get; set; }

    [Display(Name = "Randevu")]
    public int? RandevuId { get; set; }

    [Display(Name = "Tarih")]
    [DataType(DataType.DateTime)]
    public DateTime Tarih { get; set; }

    [StringLength(500)]
    [Display(Name = "Şikayet")]
    public string? Sikayet { get; set; }

    [Required(ErrorMessage = "Teşhis zorunludur.")]
    [StringLength(500)]
    [Display(Name = "Teşhis")]
    public string Teshis { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Tedavi")]
    public string? Tedavi { get; set; }

    [StringLength(1000)]
    [Display(Name = "Notlar")]
    public string? Notlar { get; set; }

    [Precision(10, 2)]
    [Display(Name = "Ücret")]
    public decimal Ucret { get; set; }

    public Hayvan? Hayvan { get; set; }
    public Veteriner? Veteriner { get; set; }
    public Randevu? Randevu { get; set; }
    public ICollection<ReceteKalemi> ReceteKalemleri { get; set; } = new List<ReceteKalemi>();
}
