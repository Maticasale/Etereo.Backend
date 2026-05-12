using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Emails;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Entities.Turnos;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Etereo.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public DatabaseSeeder(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task SeedAsync()
    {
        await SeedConfiguracionEmailAsync();
        await SeedCategoriasImputacionAsync();
        await SeedMetodosPagoAsync();
        await SeedMotivosBloqueoAsync();
        await SeedAdminAsync();
        await SeedServiciosAsync();
        await _db.SaveChangesAsync();
    }

    private async Task SeedConfiguracionEmailAsync()
    {
        if (!await _db.ConfiguracionesEmail.AnyAsync())
        {
            _db.ConfiguracionesEmail.Add(new ConfiguracionEmail
            {
                Id = 1,
                RecordatorioDiasAntes = 1,
                PostturnoHorasDespues = 2,
                EmailsActivos = true,
                ActualizadoEn = DateTime.UtcNow
            });
        }
    }

    private async Task SeedCategoriasImputacionAsync()
    {
        if (await _db.CategoriasImputacion.AnyAsync()) return;

        var ingresos = new[]
        {
            "Peluquería", "Depilación Descartable", "Depilación Láser", "Masajes",
            "Lash Lifting", "Cejas", "Facial", "Alquiler de Máquina", "Pestañas", "Otro Ingreso"
        };
        var egresos = new[]
        {
            "Electricidad", "Wi-Fi", "Seguro", "Alquiler Local", "Comisión Operaria",
            "Agua Destilada", "Gel", "Papel Depilación", "Espátulas Descartables",
            "Community Manager", "Publicidad", "Otro Egreso"
        };

        int orden = 0;
        foreach (var nombre in ingresos)
            _db.CategoriasImputacion.Add(new CategoriaImputacion { Nombre = nombre, Tipo = TipoCategoriaImp.Ingreso, Activo = true, OrdenDisplay = orden++ });
        foreach (var nombre in egresos)
            _db.CategoriasImputacion.Add(new CategoriaImputacion { Nombre = nombre, Tipo = TipoCategoriaImp.Egreso, Activo = true, OrdenDisplay = orden++ });
    }

    private async Task SeedMetodosPagoAsync()
    {
        if (await _db.MetodosPago.AnyAsync()) return;

        var metodos = new[] { "Efectivo", "Transferencia", "Tarjeta débito", "Tarjeta crédito" };
        int orden = 0;
        foreach (var nombre in metodos)
            _db.MetodosPago.Add(new MetodoPago { Nombre = nombre, Activo = true, OrdenDisplay = orden++ });
    }

    private async Task SeedMotivosBloqueoAsync()
    {
        if (await _db.MotivosBloqueoSalon.AnyAsync()) return;

        var motivos = new[] { "Feriado", "Fin de semana", "Vacaciones", "Cierre por evento", "Otro" };
        int orden = 0;
        foreach (var nombre in motivos)
            _db.MotivosBloqueoSalon.Add(new MotivoBloqueoSalon { Nombre = nombre, Activo = true, OrdenDisplay = orden++ });
    }

    private async Task SeedAdminAsync()
    {
        if (await _db.Usuarios.AnyAsync(u => u.Rol == Rol.Admin)) return;

        var password = _config["ADMIN_PASSWORD"] ?? "Admin1234!";
        _db.Usuarios.Add(new Usuario
        {
            Email = "admin@etereo.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12),
            Nombre = "Admin",
            Apellido = "Etereo",
            Rol = Rol.Admin,
            Estado = EstadoUsuario.Activo,
            AuthProvider = AuthProvider.Local,
            Sexo = Sexo.NoEspecifica,
            CreadoEn = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow
        });
    }

    private async Task SeedServiciosAsync()
    {
        if (await _db.Servicios.AnyAsync()) return;

        await _db.SaveChangesAsync();

        var catLaser    = await _db.CategoriasImputacion.FirstAsync(c => c.Nombre == "Depilación Láser");
        var catDesc     = await _db.CategoriasImputacion.FirstAsync(c => c.Nombre == "Depilación Descartable");
        var catPeluca   = await _db.CategoriasImputacion.FirstAsync(c => c.Nombre == "Peluquería");
        var catPestanas = await _db.CategoriasImputacion.FirstAsync(c => c.Nombre == "Pestañas");
        var catFacial   = await _db.CategoriasImputacion.FirstAsync(c => c.Nombre == "Facial");
        var catMasajes  = await _db.CategoriasImputacion.FirstAsync(c => c.Nombre == "Masajes");

        var laser = new Servicio { Nombre = "Depilación Láser", Salon = Salon.Salon1, CategoriaImputacionId = catLaser.Id, Activo = true, OrdenDisplay = 0 };
        var desc  = new Servicio { Nombre = "Depilación Descartable", Salon = Salon.Salon1, CategoriaImputacionId = catDesc.Id, Activo = true, OrdenDisplay = 1 };
        var pelo  = new Servicio { Nombre = "Peluquería", Salon = Salon.Salon2, CategoriaImputacionId = catPeluca.Id, Activo = true, OrdenDisplay = 2 };
        var cejas = new Servicio { Nombre = "Cejas y Pestañas", Salon = Salon.Salon1, CategoriaImputacionId = catPestanas.Id, Activo = true, OrdenDisplay = 3 };
        var facial = new Servicio { Nombre = "Cosmetología / Facial", Salon = Salon.Salon1, CategoriaImputacionId = catFacial.Id, Activo = true, OrdenDisplay = 4 };
        var masajes = new Servicio { Nombre = "Masajes", Salon = Salon.Salon1, CategoriaImputacionId = catMasajes.Id, Activo = true, OrdenDisplay = 5 };

        _db.Servicios.AddRange(laser, desc, pelo, cejas, facial, masajes);
        await _db.SaveChangesAsync();

        await SeedSubserviciosLaserAsync(laser.Id);
        await SeedSubserviciosDescartableAsync(desc.Id);
        await SeedSubserviciosPeluqueriaAsync(pelo.Id);
        await SeedSubserviciosCejasAsync(cejas.Id);
        await SeedSubserviciosFacialAsync(facial.Id);
        await SeedSubserviciosMasajesAsync(masajes.Id);
        await SeedReglasDescuentoAsync(laser.Id, desc.Id);
    }

    private async Task SeedSubserviciosLaserAsync(int servicioId)
    {
        var zonasMujeres = new (string n, decimal p, int d)[]
        {
            ("Bozo", 5000m, 20), ("Mentón", 6100m, 20), ("Rostro completo", 15400m, 30),
            ("Axila", 9300m, 20), ("Cavado bikini", 10800m, 25), ("Cavado completo", 12400m, 25),
            ("Media pierna", 13300m, 30), ("Pierna completa", 15800m, 40),
            ("Brazo completo", 13400m, 30), ("Antebrazo", 10300m, 20),
            ("Tira de cola", 5100m, 15), ("Glúteos", 11100m, 25),
            ("Línea alba", 6400m, 15), ("Empeine", 5100m, 15), ("Patilla", 6000m, 15)
        };
        int orden = 0;
        foreach (var (n, p, d) in zonasMujeres)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Femenino, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        var packsMujeres = new (string n, decimal p, string det)[]
        {
            ("Pack 1: Ax+CC+MP+(TC opc)", 28500m, "Axilas + Cavado completo + Media pierna + Tira de cola (opcional)"),
            ("Pack 3: Ax+CC+TC+PC",        29900m, "Axilas + Cavado completo + Tira de cola + Pierna completa"),
            ("Pack 4: RC+Ax+PC",           28500m, "Rostro completo + Axilas + Pierna completa"),
            ("Pack 5: Ax+CC+TC",           20100m, "Axilas + Cavado completo + Tira de cola"),
            ("Completo",                   36000m, "Rostro completo + Axilas + Cavado completo + Tira de cola + Pierna completa + Glúteos")
        };
        foreach (var (n, p, det) in packsMujeres)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = 60, Sexo = SexoSubservicio.Femenino, EsPack = true, DetallePack = det, Activo = true, OrdenDisplay = orden++ });

        var zonasHombres = new (string n, decimal p, int d)[]
        {
            ("Piernas completas", 16800m, 40), ("Media pierna", 14400m, 30), ("Rostro completo", 19200m, 30),
            ("Barba", 14400m, 25), ("Espalda", 16300m, 35), ("Pecho", 11500m, 25),
            ("Abdomen", 11500m, 25), ("Axilas", 10000m, 20), ("Brazo completo", 14400m, 30),
            ("Antebrazo", 10900m, 20), ("Hombros", 7600m, 20), ("Glúteos", 12500m, 25),
            ("Tira de cola", 6000m, 15), ("Pubis (ingle)", 10900m, 20), ("Pubis (ingle+test.)", 14400m, 25),
            ("Cuello", 9400m, 20), ("Manos", 6000m, 15), ("Patillas-pómulos", 7300m, 15), ("Empeine", 5100m, 15)
        };
        foreach (var (n, p, d) in zonasHombres)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n + " (H)", Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Masculino, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        var packsHombres = new (string n, decimal p, string det)[]
        {
            ("Pack 9: Ax+Pecho+Pubis+(LA opc)", 29100m, "Axilas + Pecho + Pubis (ingle+test.) + Línea alba (opcional)"),
            ("Pack 10: Ax+Pecho+Abd+PC",         36500m, "Axilas + Pecho + Abdomen + Piernas completas"),
            ("Pack 11: Abd+Pubis+PC",            28000m, "Abdomen + Pubis (ingle+test.) + Piernas completas"),
            ("Pack Completo (35%-)",             52000m, "Rostro completo + Axilas + Pecho + Abdomen + Brazos completos + Espalda + Pubis (ingle+test.) + Piernas completas")
        };
        foreach (var (n, p, det) in packsHombres)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = 90, Sexo = SexoSubservicio.Masculino, EsPack = true, DetallePack = det, Activo = true, OrdenDisplay = orden++ });

        await _db.SaveChangesAsync();
    }

    private async Task SeedSubserviciosDescartableAsync(int servicioId)
    {
        var zonasMujeres = new (string n, decimal p, int d)[]
        {
            ("Perfilado de cejas", 8700m, 15), ("Bozo", 5000m, 15), ("Rostro completo", 12000m, 30),
            ("Axilas", 5500m, 20), ("Brazos completos", 12000m, 30), ("Abdomen", 8000m, 25),
            ("Espalda", 15000m, 30), ("Cavado completo", 10900m, 25), ("Cavado bikini", 6900m, 20),
            ("Tira de cola", 5500m, 15), ("Línea alba", 5500m, 15),
            ("Piernas completas", 16000m, 40), ("Media pierna", 9000m, 30), ("Empeine", 3000m, 10)
        };
        int orden = 0;
        foreach (var (n, p, d) in zonasMujeres)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Femenino, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        var zonasHombres = new (string n, decimal p, int d)[]
        {
            ("Perfilado de cejas", 8700m, 15), ("Bozo", 7000m, 15), ("Barba (incl. bozo)", 10000m, 25),
            ("Fosas nasales", 4700m, 10), ("Axilas", 7000m, 20), ("Brazos completos", 12000m, 30),
            ("Abdomen", 8000m, 25), ("Espalda", 15000m, 35), ("Pubis", 10000m, 20),
            ("Glúteos", 10000m, 25), ("Tira de cola", 5500m, 15), ("Línea alba", 5500m, 15),
            ("Piernas completas", 16000m, 40), ("Media pierna", 9000m, 30),
            ("Empeine", 3000m, 10), ("Rostro completo", 12000m, 30)
        };
        foreach (var (n, p, d) in zonasHombres)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n + " (H)", Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Masculino, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        await _db.SaveChangesAsync();
    }

    private async Task SeedSubserviciosPeluqueriaAsync(int servicioId)
    {
        var tratamientos = new (string n, decimal p, int d)[]
        {
            ("Nutrición intensa", 22000m, 60), ("Tratamiento biotínico", 24000m, 60),
            ("Tratamiento RDF", 25000m, 60), ("Cauterización antifrizz", 25000m, 60),
            ("Lavado express+brushing+planchita", 20500m, 45),
            ("Hidronutritivo+rep.lípidos", 22500m, 60), ("Matizador hidronutritivo", 24000m, 60),
            ("Corte de puntas", 8000m, 20), ("Botox capilar", 25000m, 60),
            ("Peinado social", 25000m, 60),
            ("Maquillaje social", 35000m, 60), ("Maquillaje novia/quinceañera", 46000m, 90)
        };
        int orden = 0;
        foreach (var (n, p, d) in tratamientos)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        var alisados = new Subservicio { ServicioId = servicioId, Nombre = "Alisados", Precio = null, DuracionMin = null, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++ };
        _db.Subservicios.Add(alisados);

        var trenzas = new Subservicio { ServicioId = servicioId, Nombre = "Trenzas", Precio = null, DuracionMin = null, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++ };
        _db.Subservicios.Add(trenzas);

        await _db.SaveChangesAsync();

        var variantesAlisados = new (string n, decimal p, int d)[]
        {
            ("A los hombros", 26000m, 90), ("A antebrazo", 38000m, 120),
            ("A cintura", 51000m, 150), ("A la cola", 67000m, 180)
        };
        int vOrden = 0;
        foreach (var (n, p, d) in variantesAlisados)
            _db.VariantesSubservicio.Add(new VarianteSubservicio { SubservicioId = alisados.Id, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, Activo = true, OrdenDisplay = vOrden++ });

        var variantesTrenzas = new (string n, decimal p, int d)[]
        {
            ("Superiores parciales", 4400m, 45), ("Superiores completas", 6600m, 60),
            ("Vincha trenzada", 4000m, 40), ("Laterales parciales", 4400m, 45),
            ("Laterales completas", 5200m, 50), ("Trenzas caribeñas", 13000m, 120),
            ("Trenzas africanas", 12000m, 120), ("Africanas sup+caribeñas post", 11000m, 110),
            ("Boxeadoras", 7500m, 80), ("Trencitas", 8000m, 90)
        };
        vOrden = 0;
        foreach (var (n, p, d) in variantesTrenzas)
            _db.VariantesSubservicio.Add(new VarianteSubservicio { SubservicioId = trenzas.Id, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, Activo = true, OrdenDisplay = vOrden++ });

        var promos = new (string n, decimal p, string det)[]
        {
            ("Biotina x2 (c/15 días + corte)", 38400m, "2 sesiones biotínico cada 15 días + corte de puntas"),
            ("Biotina x4 (semanal)", 42000m, "4 sesiones de tratamiento biotínico, una por semana"),
            ("Hidronutrición x2 (c/15 días+corte)", 37600m, "2 sesiones hidronutritivo + rep lípidos cada 15 días + corte de puntas"),
            ("Matizador x2 (c/15 días)", 36800m, "2 sesiones de tratamiento matizador cada 15 días")
        };
        foreach (var (n, p, det) in promos)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = 120, Sexo = SexoSubservicio.Ambos, EsPack = true, DetallePack = det, Activo = true, OrdenDisplay = orden++ });

        await _db.SaveChangesAsync();
    }

    private async Task SeedSubserviciosCejasAsync(int servicioId)
    {
        var individuales = new (string n, decimal p, int d)[]
        {
            ("Lash lifting", 15500m, 60), ("Laminado de cejas", 15500m, 60),
            ("Perfilado de cejas", 8700m, 20), ("Tinte de pestañas", 8100m, 30),
            ("Pestañas PXP", 11000m, 90), ("Retoques pestañas", 9000m, 60)
        };
        int orden = 0;
        foreach (var (n, p, d) in individuales)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        var combos = new (string n, decimal p, int d, string det)[]
        {
            ("Lifting+Laminado cejas", 38300m, 110, "Lash lifting + Laminado de cejas"),
            ("Lifting+Laminado+Perfilado+Tinte", 38300m, 150, "Lash lifting + Laminado + Perfilado + Tinte"),
            ("Lifting+Tinte", 18900m, 80, "Lash lifting + Tinte de pestañas"),
            ("Perfilado+Laminado", 19300m, 70, "Perfilado de cejas + Laminado de cejas"),
            ("Lifting+Tinte+Perfilado", 25600m, 100, "Lash lifting + Tinte + Perfilado de cejas")
        };
        foreach (var (n, p, d, det) in combos)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, EsPack = true, DetallePack = det, Activo = true, OrdenDisplay = orden++ });

        await _db.SaveChangesAsync();
    }

    private async Task SeedSubserviciosFacialAsync(int servicioId)
    {
        var faciales = new (string n, decimal p, int d)[]
        {
            ("Dermaplaing", 17000m, 60),
            ("Limpieza facial profunda con extracciones", 15000m, 60),
            ("Renueva y reafirma", 19000m, 60),
            ("Limpieza profunda espalda", 13000m, 45)
        };
        int orden = 0;
        foreach (var (n, p, d) in faciales)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        await _db.SaveChangesAsync();
    }

    private async Task SeedSubserviciosMasajesAsync(int servicioId)
    {
        int orden = 0;
        var masajesFijos = new (string n, decimal p, int d)[]
        {
            ("Relajante cuerpo completo", 27000m, 60),
            ("Descontracturante espalda", 18000m, 40),
            ("Piernas cansadas", 15000m, 40)
        };
        foreach (var (n, p, d) in masajesFijos)
            _db.Subservicios.Add(new Subservicio { ServicioId = servicioId, Nombre = n, Precio = p, DuracionMin = d, RequiereSilencio = true, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++ });

        var drenaje = new Subservicio
        {
            ServicioId = servicioId, Nombre = "Drenaje linfático",
            Precio = null, DuracionMin = null,
            RequiereSilencio = true, Sexo = SexoSubservicio.Ambos, EsPack = false, Activo = true, OrdenDisplay = orden++
        };
        _db.Subservicios.Add(drenaje);
        await _db.SaveChangesAsync();

        var variantesDrenaje = new (string n, decimal p, int d)[]
        {
            ("Cuerpo completo (con rostro)", 30000m, 60),
            ("Rostro (mascarilla hidratante)", 10000m, 30),
            ("Cuerpo completo (sin rostro)", 22500m, 50),
            ("Piernas completas", 15000m, 40)
        };
        int vOrden = 0;
        foreach (var (n, p, d) in variantesDrenaje)
            _db.VariantesSubservicio.Add(new VarianteSubservicio { SubservicioId = drenaje.Id, Nombre = n, Precio = p, DuracionMin = d, Sexo = SexoSubservicio.Ambos, Activo = true, OrdenDisplay = vOrden++ });

        _db.Subservicios.Add(new Subservicio
        {
            ServicioId = servicioId, Nombre = "2 sesiones descontracturantes",
            Precio = 28000m, DuracionMin = 80,
            RequiereSilencio = true, Sexo = SexoSubservicio.Ambos, EsPack = true,
            DetallePack = "2 sesiones de masajes descontracturantes de espalda (40min c/u)",
            Activo = true, OrdenDisplay = orden++
        });

        await _db.SaveChangesAsync();
    }

    private async Task SeedReglasDescuentoAsync(int laserId, int descartableId)
    {
        if (await _db.ReglasDescuentoSesion.AnyAsync()) return;

        _db.ReglasDescuentoSesion.Add(new ReglaDescuentoSesion
        {
            ServicioId = laserId, ZonasMinimas = 3, PorcentajeDescuento = 15.00m, Activo = true,
            CreadoEn = DateTime.UtcNow, ActualizadoEn = DateTime.UtcNow
        });
        _db.ReglasDescuentoSesion.Add(new ReglaDescuentoSesion
        {
            ServicioId = descartableId, ZonasMinimas = 3, PorcentajeDescuento = 10.00m, Activo = true,
            CreadoEn = DateTime.UtcNow, ActualizadoEn = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }
}
