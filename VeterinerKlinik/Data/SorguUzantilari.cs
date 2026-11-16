using Microsoft.EntityFrameworkCore;
using VeterinerKlinik.Models;

namespace VeterinerKlinik.Data;

public static class SorguUzantilari
{
    public static IQueryable<Muayene> HesapIle(this IQueryable<Muayene> sorgu) =>
        sorgu.Include(m => m.ReceteKalemleri).Include(m => m.Odemeler);
}
