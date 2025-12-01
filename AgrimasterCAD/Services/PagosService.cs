using AgrimasterCAD.Data;
using AgrimasterCAD.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;

namespace AgrimasterCAD.Services;

public class PagosService(IDbContextFactory<ApplicationDbContext> DbFactory, IWebHostEnvironment env)
{
    private async Task<bool> Existe(int pagoId)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        return await contexto.Pagos.AnyAsync(p => p.PagoId == pagoId);
    }

    private async Task<bool> Insertar(Pagos pago)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Pagos.Add(pago);
        return await contexto.SaveChangesAsync() > 0;
    }

    private async Task<bool> Modificar(Pagos pago)
    {
        await using var contexto = await DbFactory.CreateDbContextAsync();
        contexto.Pagos.Update(pago);
        return await contexto.SaveChangesAsync() > 0;
    }

    public async Task<bool> Guardar(Pagos pago)
    {
        if (!await Existe(pago.PagoId))
        {
            return await Insertar(pago);
        }
        else
        {
            return await Modificar(pago);
        }
    }

    public async Task<string?> GuardarComprobante(IBrowserFile archivo, int solicitudId)
    {
        try
        {
            var carpeta = Path.Combine(env.WebRootPath, "uploads", "comprobantes");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var archivoNombre = $"{solicitudId}_{Guid.NewGuid()}.pdf";
            var ruta = Path.Combine(carpeta, archivoNombre);

            using var stream = new FileStream(ruta, FileMode.Create);
            await archivo.OpenReadStream(5 * 1024 * 1024).CopyToAsync(stream);

            return $"/uploads/comprobantes/{archivoNombre}";
        }
        catch
        {
            return null;
        }
    }
}
