using System.Threading.Tasks;

namespace SingleOneAPI.Services
{
    public interface ISmtpConfigService
    {
        Task LoadSmtpSettingsFromDatabase(EnvironmentApiSettings environmentApiSettings, int clienteId);
    }
}
