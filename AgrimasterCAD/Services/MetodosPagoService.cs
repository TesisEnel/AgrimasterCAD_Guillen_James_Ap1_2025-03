using AgrimasterCAD.Data;
using AgrimasterCAD.Models;
using Microsoft.EntityFrameworkCore;

namespace AgrimasterCAD.Services;

public class MetodosPagoService(IDbContextFactory<ApplicationDbContext> DbFactory)
{
    private async Task<bool> Existe(int metodoPagoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.MetodosPago.AnyAsync(m => m.MetodoPagoId == metodoPagoId);
    }

    private async Task<bool> Insertar(MetodosPago metodoPago)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.MetodosPago.Add(metodoPago);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(MetodosPago metodoPago)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.MetodosPago.Update(metodoPago);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(MetodosPago metodoPago)
    {
        if (!await Existe(metodoPago.MetodoPagoId))
        {
            return await Insertar(metodoPago);
        }
        else
        {
            return await Modificar(metodoPago);
        }
    }

    public async Task<List<MetodosPago>> ListarPorUsuario(string usuarioId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.MetodosPago.Where(m => m.UsuarioId == usuarioId).ToListAsync();
    }
}
