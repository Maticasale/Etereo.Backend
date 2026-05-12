using Etereo.Shared.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Etereo.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequiereRolAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _roles;

    public RequiereRolAttribute(params string[] roles)
    {
        _roles = roles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                error = new { codigo = "TOKEN_INVALIDO_O_EXPIRADO", mensaje = "Token inválido o ausente." }
            });
            return;
        }

        var rolClaim = user.FindFirstValue("rol");
        if (rolClaim is null || !_roles.Contains(rolClaim))
        {
            context.Result = new ObjectResult(new
            {
                error = new { codigo = "ACCESO_DENEGADO", mensaje = "No tenés permiso para esta acción." }
            })
            { StatusCode = 403 };
        }
    }
}
