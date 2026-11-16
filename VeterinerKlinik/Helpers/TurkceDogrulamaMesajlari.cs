using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace VeterinerKlinik.Helpers;

public static class TurkceDogrulamaMesajlari
{
    public static void Uygula(DefaultModelBindingMessageProvider m)
    {
        m.SetValueMustNotBeNullAccessor(_ => "Bu alan zorunludur.");
        m.SetMissingBindRequiredValueAccessor(ad => $"{ad} alanı zorunludur.");
        m.SetMissingKeyOrValueAccessor(() => "Değer zorunludur.");
        m.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi boş olamaz.");
        m.SetAttemptedValueIsInvalidAccessor((deger, ad) => $"'{deger}' değeri {ad} için geçersiz.");
        m.SetNonPropertyAttemptedValueIsInvalidAccessor(deger => $"'{deger}' değeri geçersiz.");
        m.SetUnknownValueIsInvalidAccessor(ad => $"{ad} için girilen değer geçersiz.");
        m.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Girilen değer geçersiz.");
        m.SetValueIsInvalidAccessor(deger => $"'{deger}' değeri geçersiz.");
        m.SetValueMustBeANumberAccessor(ad => $"{ad} için en fazla 2 ondalık haneli bir sayı girin (ör. 12,50).");
        m.SetNonPropertyValueMustBeANumberAccessor(() => "Bu alan sayı olmalıdır.");
    }
}
