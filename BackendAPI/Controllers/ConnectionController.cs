using Backend;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackendAPI.Controllers
{
    [ApiController]
    public class ConnectionController : ControllerBase
    {
        private ILogger<ConnectionController> Logger { get; }

        public ConnectionController(ILogger<ConnectionController> logger)
        {
            Logger = logger;
        }

        [HttpGet("connection/logout")]
        public void Logout()
        {
            using var timer = new RequestTimer<ConnectionController>($"Connection/{nameof(Logout)}", Logger);
            ConnectionManager.Instance.Logout();
        }
        [HttpGet("connection/login")]
        public async Task Login()
        {
            using var timer = new RequestTimer<ConnectionController>($"Connection/{nameof(Login)}", Logger);
            await ConnectionManager.Instance.Login(true);
        }
        [HttpGet("connection/userid")]
        public string UserId()
        {
            using var timer = new RequestTimer<ConnectionController>($"Connection/{nameof(UserId)}", Logger);

            var user = DataContainer.Instance.User;
            if (user == null)
            {
                timer.ErrorMessage = "no logged in user";
                return null;
            }

            timer.DetailMessage = $"userId={user.Id}";
            return user.Id;
        }

        [HttpGet("json/connection/userid")]
        public Dictionary<string, string> JsonUserId() => new() { { Constants.JSON_RESULT, UserId() } };
    }
}
