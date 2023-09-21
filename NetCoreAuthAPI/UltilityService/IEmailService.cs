using NetCoreAuthAPI.Models;

namespace NetCoreAuthAPI.UltilityService
{
    public interface IEmailService
    {
        void SendEmail(EmailModel emailModel);
    }
}
