using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinik.Models;

public class AsiKaydi
{
    public int Id { get; set; }

    [Display(Name = "Hayvan")]
    public int HayvanId { get; set; }

    [Display(Name = "Aşı")]
    public int AsiId { get; set; }

    [Display(Name = "Veteriner")]
    public int? VeterinerId { get; set; }

    [Display(Name = "Uygulama Tarihi")]
    [DataType(DataType.Date)]
    public DateTime UygulamaTarihi { get; set; }

    [Display(Name = "Sonraki Tarih")]
    [DataType(DataType.Date)]
    public DateTime? SonrakiTarih { get; set; }

    public Hayvan? Hayvan { get; set; }
    public Asi? Asi { get; set; }
    public Veteriner? Veteriner { get; set; }
}
