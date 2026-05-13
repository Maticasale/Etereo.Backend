namespace Etereo.Shared.Imputaciones;

// ── Imputaciones ──────────────────────────────────────────────────────────────

public record CrearImputacionRequest(
    DateOnly  Fecha,
    string    Tipo,          // "Ingreso" | "Egreso"
    int       CategoriaId,
    string?   Descripcion,
    decimal   Monto,
    int?      TurnoId,
    int?      OperarioId
);

public record ActualizarImputacionRequest(
    DateOnly? Fecha,
    int?      CategoriaId,
    string?   Descripcion,
    decimal?  Monto,
    int?      OperarioId
);

public class ImputacionDto
{
    public int      Id               { get; set; }
    public DateOnly Fecha            { get; set; }
    public string   Tipo             { get; set; } = string.Empty;
    public int      CategoriaId      { get; set; }
    public string   NombreCategoria  { get; set; } = string.Empty;
    public string?  Descripcion      { get; set; }
    public decimal  Monto            { get; set; }
    public int?     TurnoId          { get; set; }
    public int?     OperarioId       { get; set; }
    public string?  NombreOperario   { get; set; }
    public int      CargadoPorId     { get; set; }
    public string   Origen           { get; set; } = string.Empty;
    public DateTime CreadoEn         { get; set; }
}

public class ResumenImputacionesDto
{
    public decimal TotalIngresos { get; set; }
    public decimal TotalEgresos  { get; set; }
    public decimal Balance       => TotalIngresos - TotalEgresos;
    public List<ResumenCategoriaDto> PorCategoria { get; set; } = [];
}

public class ResumenCategoriaDto
{
    public string  NombreCategoria { get; set; } = string.Empty;
    public string  Tipo            { get; set; } = string.Empty;
    public decimal Total           { get; set; }
}

// ── Catálogos ─────────────────────────────────────────────────────────────────

public record CrearCategoriaImputacionRequest(string Nombre, string Tipo, string? Descripcion);
public record ActualizarCategoriaImputacionRequest(string? Nombre, string? Descripcion);
public record EstadoImputacionRequest(bool Activo);

public class CategoriaImputacionDto
{
    public int     Id           { get; set; }
    public string  Nombre       { get; set; } = string.Empty;
    public string  Tipo         { get; set; } = string.Empty;
    public string? Descripcion  { get; set; }
    public bool    Activo       { get; set; }
    public int     OrdenDisplay { get; set; }
}

public record CrearMetodoPagoRequest(string Nombre);
public record ActualizarMetodoPagoRequest(string Nombre);

public class MetodoPagoDto
{
    public int    Id           { get; set; }
    public string Nombre       { get; set; } = string.Empty;
    public bool   Activo       { get; set; }
    public int    OrdenDisplay { get; set; }
}

public record CrearMotivoBloqueoRequest(string Nombre);
public record ActualizarMotivoBloqueoRequest(string Nombre);

public class MotivoBloqueoSalonDto
{
    public int    Id           { get; set; }
    public string Nombre       { get; set; } = string.Empty;
    public bool   Activo       { get; set; }
    public int    OrdenDisplay { get; set; }
}
