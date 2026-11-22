using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace VeterinerKlinik.Models;

public class Hayvan
{
    public int Id { get; set; }

    [Display(Name = "Sahip")]
    public int SahipId { get; set; }

    [Required(ErrorMessage = "Ad zorunludur.")]
    [StringLength(50)]
    [Display(Name = "Ad")]
    public string Ad { get; set; } = string.Empty;

    [Display(Name = "Tür")]
    public HayvanTuru Tur { get; set; }

    [StringLength(50)]
    [Display(Name = "Irk")]
    public string? Irk { get; set; }

    [Display(Name = "Cinsiyet")]
    public Cinsiyet Cinsiyet { get; set; }

    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? DogumTarihi { get; set; }

    [Precision(10, 2)]
    [Display(Name = "Kilo (kg)")]
    public decimal? Kilo { get; set; }

    [StringLength(500)]
    [Display(Name = "Notlar")]
    public string? Notlar { get; set; }

    public Sahip? Sahip { get; set; }
    public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    public ICollection<Muayene> Muayeneler { get; set; } = new List<Muayene>();
    public ICollection<AsiKaydi> AsiKayitlari { get; set; } = new List<AsiKaydi>();
}
