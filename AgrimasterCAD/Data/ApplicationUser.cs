using Microsoft.AspNetCore.Identity;

namespace AgrimasterCAD.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = "";
        public string NumeroCedula { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string? NumeroCodia { get; set; }
    }

}
