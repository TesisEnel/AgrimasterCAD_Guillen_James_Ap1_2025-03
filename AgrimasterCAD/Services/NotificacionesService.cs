using AgrimasterCAD.Data;
using AgrimasterCAD.Models;
using Microsoft.EntityFrameworkCore;

namespace AgrimasterCAD.Services;

public class NotificacionesService(IDbContextFactory<ApplicationDbContext> DbFactory)
{
    public async Task Crear(string usuarioId, string titulo, string mensaje, string tipo = "info")
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var notificacion = new Notificaciones
        {
            UsuarioId = usuarioId,
            Titulo = titulo,
            Mensaje = mensaje,
            Tipo = tipo,
            Fecha = DateTime.Now
        };

        contexto.Notificaciones.Add(notificacion);
        await contexto.SaveChangesAsync();
    }

    public async Task<List<Notificaciones>> Listar(string usuarioId)
    {
        await using var db = await DbFactory.CreateDbContextAsync();
        return await db.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.Fecha)
            .ToListAsync();
    }

    public async Task MarcarLeida(int solicitudId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();

        var notificacion = await contexto.Notificaciones.FindAsync(solicitudId);

        if (notificacion != null)
        {
            notificacion.Leida = true;
            await contexto.SaveChangesAsync();
        }
    }
}
