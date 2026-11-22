using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace VeterinerKlinik.Helpers;

public class OndalikModelBinder : IModelBinder
{
    private static readonly Regex Bicim = new(@"^-?\d+([.,]\d{1,2})?$");

    public Task BindModelAsync(ModelBindingContext context)
    {
        var deger = context.ValueProvider.GetValue(context.ModelName);
        if (deger == ValueProviderResult.None) return Task.CompletedTask;

        context.ModelState.SetModelValue(context.ModelName, deger);
        var metin = deger.FirstValue?.Trim();
        var mesajlar = context.ModelMetadata.ModelBindingMessageProvider;

        if (string.IsNullOrEmpty(metin))
        {
            if (context.ModelMetadata.IsReferenceOrNullableType)
                context.Result = ModelBindingResult.Success(null);
            else
                context.ModelState.TryAddModelError(context.ModelName, mesajlar.ValueMustNotBeNullAccessor(string.Empty));
            return Task.CompletedTask;
        }

        if (Bicim.IsMatch(metin) && decimal.TryParse(metin.Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var sayi))
            context.Result = ModelBindingResult.Success(sayi);
        else
            context.ModelState.TryAddModelError(context.ModelName,
                mesajlar.ValueMustBeANumberAccessor(context.ModelMetadata.GetDisplayName()));

        return Task.CompletedTask;
    }
}

public class OndalikModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.UnderlyingOrModelType == typeof(decimal) ? new OndalikModelBinder() : null;
}
