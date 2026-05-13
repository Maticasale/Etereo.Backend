namespace Etereo.Shared.Servicios;

// ── Servicio ──────────────────────────────────────────────────────────────────

public record CrearServicioRequest(
    string  Nombre,
    string  Salon,
    int?    CategoriaImputacionId,
    int     OrdenDisplay = 0
);

public record ActualizarServicioRequest(
    string? Nombre,
    string? Salon,
    int?    CategoriaImputacionId,
    int?    OrdenDisplay
);

public record EstadoRequest(bool Activo);

// ── Subservicio ───────────────────────────────────────────────────────────────

public record CrearSubservicioRequest(
    int      ServicioId,
    string   Nombre,
    string?  Descripcion,
    decimal? Precio,
    int?     DuracionMin,
    bool     RequiereSilencio = false,
    bool     EsPack           = false,
    string?  DetallePack      = null,
    string   Sexo             = "Ambos",
    int      OrdenDisplay     = 0
);

public record ActualizarSubservicioRequest(
    string?  Nombre,
    string?  Descripcion,
    decimal? Precio,
    int?     DuracionMin,
    bool?    RequiereSilencio,
    bool?    EsPack,
    string?  DetallePack,
    string?  Sexo,
    int?     OrdenDisplay
);

// ── Variante ──────────────────────────────────────────────────────────────────

public record CrearVarianteRequest(
    string  Nombre,
    decimal Precio,
    int     DuracionMin,
    string  Sexo         = "Ambos",
    int     OrdenDisplay = 0
);

public record ActualizarVarianteRequest(
    string?  Nombre,
    decimal? Precio,
    int?     DuracionMin,
    string?  Sexo,
    int?     OrdenDisplay
);

// ── Regla descuento ───────────────────────────────────────────────────────────

public record ActualizarReglaDescuentoRequest(
    int     ZonasMinimas,
    decimal PorcentajeDescuento,
    bool    Activo
);

// ── Response DTOs ─────────────────────────────────────────────────────────────

public class VarianteDto
{
    public int     Id           { get; set; }
    public string  Nombre       { get; set; } = string.Empty;
    public decimal Precio       { get; set; }
    public int     DuracionMin  { get; set; }
    public string  Sexo         { get; set; } = string.Empty;
    public bool    Activo       { get; set; }
    public int     OrdenDisplay { get; set; }
}

public class SubservicioDto
{
    public int            Id               { get; set; }
    public int            ServicioId       { get; set; }
    public string         Nombre           { get; set; } = string.Empty;
    public string?        Descripcion      { get; set; }
    public decimal?       Precio           { get; set; }
    public int?           DuracionMin      { get; set; }
    public bool           RequiereSilencio { get; set; }
    public bool           EsPack           { get; set; }
    public string?        DetallePack      { get; set; }
    public string         Sexo             { get; set; } = string.Empty;
    public bool           Activo           { get; set; }
    public int            OrdenDisplay     { get; set; }
    public List<VarianteDto> Variantes     { get; set; } = [];
}

public class ServicioDto
{
    public int                  Id                    { get; set; }
    public string               Nombre                { get; set; } = string.Empty;
    public string               Salon                 { get; set; } = string.Empty;
    public int?                 CategoriaImputacionId { get; set; }
    public bool                 Activo                { get; set; }
    public int                  OrdenDisplay          { get; set; }
    public List<SubservicioDto> Subservicios          { get; set; } = [];
}

public class EstadoConfiguracionDto
{
    public bool   Configurado { get; set; }
    public string Mensaje     { get; set; } = string.Empty;
}

public class ReglaDescuentoDto
{
    public int     Id                   { get; set; }
    public int     ServicioId           { get; set; }
    public string  NombreServicio       { get; set; } = string.Empty;
    public int     ZonasMinimas         { get; set; }
    public decimal PorcentajeDescuento  { get; set; }
    public bool    Activo               { get; set; }
}
