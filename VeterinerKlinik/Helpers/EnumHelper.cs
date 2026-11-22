using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace VeterinerKlinik.Helpers;

public static class EnumHelper
{
    public static string GetDisplayName(this Enum deger)
    {
        var alan = deger.GetType().GetField(deger.ToString());
        return alan?.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? deger.ToString();
    }
}
