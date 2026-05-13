namespace Etereo.Shared.Cupones;

public record CrearCuponRequest(
    string   Codigo,
    string?  Descripcion,
    string   TipoDescuento,   // "Porcentaje" | "MontoFijo"
    decimal  Valor,
    int[]?   ServiciosIds,
    DateOnly FechaDesde,
    DateOnly FechaHasta,
    int?     UsosMaximos,
    bool     UnUsoPorCliente
);

public record ActualizarCuponRequest(
    string?  Descripcion,
    decimal? Valor,
    int[]?   ServiciosIds,
    DateOnly? FechaDesde,
    DateOnly? FechaHasta,
    int?     UsosMaximos,
    bool?    UnUsoPorCliente
);

public record EstadoCuponRequest(bool Activo);

public class CuponDto
{
    public int      Id              { get; set; }
    public string   Codigo          { get; set; } = string.Empty;
    public string?  Descripcion     { get; set; }
    public string   TipoDescuento   { get; set; } = string.Empty;
    public decimal  Valor           { get; set; }
    public int[]?   ServiciosIds    { get; set; }
    public DateOnly FechaDesde      { get; set; }
    public DateOnly FechaHasta      { get; set; }
    public int?     UsosMaximos     { get; set; }
    public int      UsosActuales    { get; set; }
    public bool     UnUsoPorCliente { get; set; }
    public bool     Activo          { get; set; }
    public DateTime CreadoEn        { get; set; }
}
