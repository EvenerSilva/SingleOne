using Microsoft.AspNetCore.Mvc;
using SingleOneAPI;

namespace SingleOne.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly EnvironmentApiSettings _environmentApiSettings;

        public ValuesController(EnvironmentApiSettings environmentApiSettings)
        {
            _environmentApiSettings = environmentApiSettings;
        }

        [HttpGet]
        public object Get()
        {
            //return "SingleOne - development \n by Fmb Tecnologia";
            return new
            {
                title = "SingleOne - by Fmb Tecnologia",
                url = _environmentApiSettings.SiteUrl,
                database = _environmentApiSettings.DatabaseConfiguration.Host,
                smtp = _environmentApiSettings.SMTPHost
            };
        }
    }
}
