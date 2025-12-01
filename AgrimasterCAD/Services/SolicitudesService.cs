using System.Linq.Expressions;
using AgrimasterCAD.Data;
using AgrimasterCAD.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace AgrimasterCAD.Services;

public class SolicitudesService(IDbContextFactory<ApplicationDbContext> DbFactory, IWebHostEnvironment env)
{
    private async Task<bool> Existe(int solicitudId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Solicitudes.AnyAsync(s => s.SolicitudId == solicitudId);
    }

    private async Task<bool> Insertar(Solicitudes solicitud)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Solicitudes.Add(solicitud);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Solicitudes solicitud)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Solicitudes.Update(solicitud);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Solicitudes solicitud)
    {
        if (!await Existe(solicitud.SolicitudId))
        {
            return await Insertar(solicitud);
        }
        else
        {
            return await Modificar(solicitud);
        }
    }

    public async Task<List<Solicitudes>> Listar(Expression<Func<Solicitudes, bool>> criterio)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
        .Where(criterio)
        .Include(s => s.Cliente)
        .Include(s => s.Agrimensor)
        .Include(s => s.ComprobantePago)
        .Include(s => s.Documentos)
        .Include(s => s.Plano)
        .Include(s => s.Pagos)
        .Include(s => s.Seguimientos)
        .ToListAsync();
    }

    public async Task<Solicitudes?> Buscar(int solicitudId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Solicitudes
        .Include(s => s.Cliente)
        .Include(s => s.Agrimensor)
        .Include(s => s.ComprobantePago)
        .Include(s => s.Documentos)
        .Include(s => s.Plano)
        .Include(s => s.Pagos)
        .Include(s => s.Seguimientos)
        .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);
    }

    public async Task<string> GuardarArchivo(IBrowserFile archivo, string subcarpeta)
    {
        try
        {
            // Carpeta destino: wwwroot/uploads/{subcarpeta}
            var carpeta = Path.Combine(env.WebRootPath, "uploads", subcarpeta);

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            // Nombre único
            var archivoNombre = $"{Guid.NewGuid()}_{archivo.Name}";
            var rutaArchivo = Path.Combine(carpeta, archivoNombre);

            // Guardar archivo
            await using var stream = new FileStream(rutaArchivo, FileMode.Create);
            await archivo.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024).CopyToAsync(stream);

            // Devolver ruta relativa (para la DB)
            return $"/uploads/{subcarpeta}/{archivoNombre}";
        }
        catch (IOException)
        {
            throw new Exception("El archivo supera el límite de 5 MB.");
        }
    }

    public async Task<string> GuardarPlano(int solicitudId, IBrowserFile archivo)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var solicitud = await contexto.Solicitudes
            .Include(s => s.Plano)
            .FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);

        if (solicitud == null)
            throw new Exception("La solicitud no existe.");

        var ruta = await GuardarArchivo(archivo, "planos");

        if (solicitud.Plano == null)
        {
            solicitud.Plano = new Planos
            {
                SolicitudId = solicitudId,
                RutaArchivo = ruta
            };

            contexto.Planos.Add(solicitud.Plano);
        }
        else
        {
            solicitud.Plano.RutaArchivo = ruta;
            contexto.Planos.Update(solicitud.Plano);
        }

        await contexto.SaveChangesAsync();

        return ruta;
    }

    public async Task AgregarSeguimiento(int solicitudId, string comentario)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var solicitud = await contexto.Solicitudes.FirstOrDefaultAsync(s => s.SolicitudId == solicitudId);

        if (solicitud == null)
            throw new Exception("La solicitud no existe.");

        var seguimiento = new SolicitudSeguimientos
        {
            SolicitudId = solicitudId,
            Comentario = comentario,
        };

        contexto.SolicitudSeguimientos.Add(seguimiento);
        await contexto.SaveChangesAsync();
    }
}
