using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VeterinerKlinik.Helpers;

public class YetkiFiltresi : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor aciklama)
            return;

        var anonimMi = context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()
                       || context.Filters.OfType<IAllowAnonymousFilter>().Any();
        var controller = aciklama.ControllerName;
        var action = aciklama.ActionName;
        var homeHata = string.Equals(controller, "Home", StringComparison.OrdinalIgnoreCase)
                       && string.Equals(action, "Error", StringComparison.OrdinalIgnoreCase);
        if (anonimMi || homeHata)
            return;

        var kullanici = context.HttpContext.User;
        if (kullanici.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        if (string.Equals(controller, "Hesap", StringComparison.OrdinalIgnoreCase))
            return;

        if (!YetkiKurallari.IzinVarMi(kullanici, controller, action))
            context.Result = new ForbidResult();
    }
}
