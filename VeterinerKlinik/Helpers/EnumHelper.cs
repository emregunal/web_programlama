using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VeterinerKlinik.Helpers;

public static class EnumHelper
{
    public static string GetDisplayName(this Enum deger)
    {
        var alan = deger.GetType().GetField(deger.ToString());
        return alan?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? deger.ToString();
    }

    public static List<SelectListItem> SecimListesi<T>(T? secili) where T : struct, Enum =>
        Enum.GetValues<T>()
            .Select(d => new SelectListItem(d.GetDisplayName(), Convert.ToInt32(d).ToString(), secili.HasValue && secili.Value.Equals(d)))
            .ToList();
}
