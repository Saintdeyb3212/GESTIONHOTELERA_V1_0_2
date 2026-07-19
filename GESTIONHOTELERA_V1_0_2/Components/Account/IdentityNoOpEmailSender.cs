using GESTIONHOTELERA_V1_0_2.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace GESTIONHOTELERA_V1_0_2.Components.Account
{
    // Remove the "else if (EmailSender is IdentityNoOpEmailSender)" block from RegisterConfirmation.razor after updating with a real implementation.
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
    {
        private readonly IEmailSender emailSender = new NoOpEmailSender();

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            emailSender.SendEmailAsync(email, "Confirmar su correo electrónico", $"Confirme su cuenta haciendo <a href='{confirmationLink}'>clic aquí</a>.");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            emailSender.SendEmailAsync(email, "Restablecer su contraseña", $"Restablezca su contraseña haciendo <a href='{resetLink}'>clic aquí</a>.");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            emailSender.SendEmailAsync(email, "Restablecer su contraseña", $"Restablezca su contraseña usando el siguiente código: {resetCode}");
    }
}
