using Etereo.Api.Attributes;
using Etereo.Shared.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
[RequiereRol(Roles.Admin, Roles.Operario)]
public class UsuariosController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() =>
        Ok(new { data = "Módulo Usuarios — pendiente de implementación (Módulo 2)" });
}
