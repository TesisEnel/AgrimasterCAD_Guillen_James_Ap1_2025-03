using AgrimasterCAD.Data;
using AgrimasterCAD.Models;
using Microsoft.EntityFrameworkCore;

namespace AgrimasterCAD.Services;

public class ComprobantesPagoService(IDbContextFactory<ApplicationDbContext> DbFactory)
{
    private async Task<bool> Existe(int comprobantePagoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.ComprobantesPagos.AnyAsync(c => c.ComprobantePagoId == comprobantePagoId);
    }

    private async Task<bool> Insertar(ComprobantesPago comprobantesPago)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.ComprobantesPagos.Add(comprobantesPago);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(ComprobantesPago comprobantesPago)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.ComprobantesPagos.Update(comprobantesPago);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(ComprobantesPago comprobantesPago)
    {
        if (!await Existe(comprobantesPago.ComprobantePagoId))
        {
            return await Insertar(comprobantesPago);
        }
        else
        {
            return await Modificar(comprobantesPago);
        }
    }
}
