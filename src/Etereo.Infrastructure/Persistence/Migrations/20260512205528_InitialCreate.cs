using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Etereo.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calificaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    turno_id = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    operario_id = table.Column<int>(type: "integer", nullable: false),
                    puntuacion = table.Column<int>(type: "integer", nullable: false),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calificaciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "categorias_imputacion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    orden_display = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias_imputacion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "configuracion_email",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    recordatorio_dias_antes = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    postturno_horas_despues = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    emails_activos = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_email", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cupon_usos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cupon_id = table.Column<int>(type: "integer", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    turno_id = table.Column<int>(type: "integer", nullable: false),
                    usado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupon_usos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cupones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    tipo_descuento = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    valor = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    servicios_ids = table.Column<int[]>(type: "integer[]", nullable: true),
                    fecha_desde = table.Column<DateOnly>(type: "date", nullable: false),
                    fecha_hasta = table.Column<DateOnly>(type: "date", nullable: false),
                    usos_maximos = table.Column<int>(type: "integer", nullable: true),
                    usos_actuales = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    un_uso_por_cliente = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "disponibilidad_operario",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operario_id = table.Column<int>(type: "integer", nullable: false),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    trabaja = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    motivo_ausencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disponibilidad_operario", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "disponibilidad_salon",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    salon = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    motivo_id = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    creado_por_id = table.Column<int>(type: "integer", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_disponibilidad_salon", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "emails_enviados",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    destinatario = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    turno_id = table.Column<int>(type: "integer", nullable: true),
                    usuario_id = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    error_detalle = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    enviado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emails_enviados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "imputaciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    categoria_id = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    monto = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    turno_id = table.Column<int>(type: "integer", nullable: true),
                    operario_id = table.Column<int>(type: "integer", nullable: true),
                    cargado_por_id = table.Column<int>(type: "integer", nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Manual"),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imputaciones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "metodos_pago",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    orden_display = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metodos_pago", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "motivos_bloqueo_salon",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    orden_display = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_motivos_bloqueo_salon", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operario_subservicios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operario_id = table.Column<int>(type: "integer", nullable: false),
                    subservicio_id = table.Column<int>(type: "integer", nullable: false),
                    porcentaje_comision = table.Column<decimal>(type: "numeric(4,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operario_subservicios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "operario_vistas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operario_id = table.Column<int>(type: "integer", nullable: false),
                    ver_mis_turnos = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ver_mis_comisiones = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ver_mi_calificacion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ver_mis_estadisticas = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operario_vistas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expira_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    usado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    usuario_id = table.Column<int>(type: "integer", nullable: false),
                    token_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    expira_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revocado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "reglas_descuento_sesion",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    servicio_id = table.Column<int>(type: "integer", nullable: false),
                    zonas_minimas = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    porcentaje_descuento = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reglas_descuento_sesion", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    salon = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    categoria_imputacion_id = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    orden_display = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sesiones",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cliente_id = table.Column<int>(type: "integer", nullable: true),
                    nombre_anonimo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    telefono_anonimo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    operario_id = table.Column<int>(type: "integer", nullable: false),
                    salon = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    fecha_hora_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PendienteConfirmacion"),
                    descuento_auto_pct = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sesiones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subservicios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    servicio_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    precio = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    duracion_min = table.Column<int>(type: "integer", nullable: true),
                    requiere_silencio = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    es_pack = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    detalle_pack = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sexo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    orden_display = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subservicios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "turnos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    salon = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: true),
                    nombre_anonimo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    telefono_anonimo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    operario_id = table.Column<int>(type: "integer", nullable: false),
                    subservicio_id = table.Column<int>(type: "integer", nullable: false),
                    variante_id = table.Column<int>(type: "integer", nullable: true),
                    sesion_id = table.Column<int>(type: "integer", nullable: true),
                    fecha_hora_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duracion_min = table.Column<int>(type: "integer", nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PendienteConfirmacion"),
                    motivo_rechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    precio_base = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    porcentaje_descuento = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    cupon_id = table.Column<int>(type: "integer", nullable: true),
                    precio_final = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    metodo_pago_id = table.Column<int>(type: "integer", nullable: true),
                    comision_calculada = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ip_origen = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    creado_por_id = table.Column<int>(type: "integer", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_turnos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellido = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    sexo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    auth_provider = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    google_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    motivo_bloqueo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    debe_cambiar_password = table.Column<bool>(type: "boolean", nullable: false),
                    avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "variantes_subservicio",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subservicio_id = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    duracion_min = table.Column<int>(type: "integer", nullable: false),
                    sexo = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    orden_display = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_variantes_subservicio", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calificaciones_turno_id",
                table: "calificaciones",
                column: "turno_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_categorias_imputacion_nombre",
                table: "categorias_imputacion",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cupon_usos_cupon_id_cliente_id",
                table: "cupon_usos",
                columns: new[] { "cupon_id", "cliente_id" });

            migrationBuilder.CreateIndex(
                name: "IX_cupones_codigo",
                table: "cupones",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_disponibilidad_operario_operario_id_fecha",
                table: "disponibilidad_operario",
                columns: new[] { "operario_id", "fecha" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_disponibilidad_salon_fecha",
                table: "disponibilidad_salon",
                column: "fecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_emails_enviados_tipo_turno_id",
                table: "emails_enviados",
                columns: new[] { "tipo", "turno_id" });

            migrationBuilder.CreateIndex(
                name: "IX_emails_enviados_tipo_usuario_id",
                table: "emails_enviados",
                columns: new[] { "tipo", "usuario_id" });

            migrationBuilder.CreateIndex(
                name: "IX_imputaciones_fecha",
                table: "imputaciones",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_imputaciones_operario_id",
                table: "imputaciones",
                column: "operario_id");

            migrationBuilder.CreateIndex(
                name: "IX_imputaciones_tipo_categoria_id",
                table: "imputaciones",
                columns: new[] { "tipo", "categoria_id" });

            migrationBuilder.CreateIndex(
                name: "IX_imputaciones_turno_id",
                table: "imputaciones",
                column: "turno_id");

            migrationBuilder.CreateIndex(
                name: "IX_metodos_pago_nombre",
                table: "metodos_pago",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_motivos_bloqueo_salon_nombre",
                table: "motivos_bloqueo_salon",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operario_subservicios_operario_id_subservicio_id",
                table: "operario_subservicios",
                columns: new[] { "operario_id", "subservicio_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_operario_vistas_operario_id",
                table: "operario_vistas",
                column: "operario_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_token_hash",
                table: "password_reset_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_reglas_descuento_sesion_servicio_id",
                table: "reglas_descuento_sesion",
                column: "servicio_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_servicios_nombre",
                table: "servicios",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_subservicios_servicio_id_nombre",
                table: "subservicios",
                columns: new[] { "servicio_id", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_turnos_cliente_id",
                table: "turnos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_estado",
                table: "turnos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_fecha_hora_inicio",
                table: "turnos",
                column: "fecha_hora_inicio");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_operario_id",
                table: "turnos",
                column: "operario_id");

            migrationBuilder.CreateIndex(
                name: "IX_turnos_sesion_id",
                table: "turnos",
                column: "sesion_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_google_id",
                table: "usuarios",
                column: "google_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_variantes_subservicio_subservicio_id_nombre",
                table: "variantes_subservicio",
                columns: new[] { "subservicio_id", "nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calificaciones");

            migrationBuilder.DropTable(
                name: "categorias_imputacion");

            migrationBuilder.DropTable(
                name: "configuracion_email");

            migrationBuilder.DropTable(
                name: "cupon_usos");

            migrationBuilder.DropTable(
                name: "cupones");

            migrationBuilder.DropTable(
                name: "disponibilidad_operario");

            migrationBuilder.DropTable(
                name: "disponibilidad_salon");

            migrationBuilder.DropTable(
                name: "emails_enviados");

            migrationBuilder.DropTable(
                name: "imputaciones");

            migrationBuilder.DropTable(
                name: "metodos_pago");

            migrationBuilder.DropTable(
                name: "motivos_bloqueo_salon");

            migrationBuilder.DropTable(
                name: "operario_subservicios");

            migrationBuilder.DropTable(
                name: "operario_vistas");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "reglas_descuento_sesion");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "sesiones");

            migrationBuilder.DropTable(
                name: "subservicios");

            migrationBuilder.DropTable(
                name: "turnos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "variantes_subservicio");
        }
    }
}
