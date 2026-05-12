namespace Etereo.Domain.Enums;

public enum Rol              { Admin, Operario, Cliente }
public enum AuthProvider     { Local, Google }
public enum EstadoUsuario    { Activo, Inactivo, Bloqueado }
public enum Sexo             { Masculino, Femenino, NoEspecifica }
public enum SexoSubservicio  { Masculino, Femenino, Ambos }
public enum Salon            { Salon1, Salon2, Ambos }
public enum EstadoTurno
{
    PendienteConfirmacion, Confirmado, Rechazado,
    Cancelado, Multa, Ausente, Realizado, Impago, Publicidad
}
public enum TipoDescuento    { Porcentaje, MontoFijo }
public enum TipoImputacion   { Ingreso, Egreso }
public enum TipoCategoriaImp { Ingreso, Egreso, Ambos }
public enum OrigenImputacion { Manual, Automatico }
public enum TipoEmail
{
    ConfirmacionRegistro, ConfirmacionTurno, RechazoTurno,
    RecordatorioTurno, PostTurnoCalificacion,
    RecuperacionPassword, CambioPassword, Campana
}
public enum EstadoEmail      { Enviado, Fallido }
