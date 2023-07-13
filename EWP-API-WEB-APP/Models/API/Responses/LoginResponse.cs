using EWP_API_WEB_APP.Models.Data;

namespace EWP_API_WEB_APP.Models.API.Responses
{
    public class LoginResponse
    {

        public LoginResponse()
        {
            this.LoginSuccess = false;
        }

        public bool LoginSuccess { get; set; }
        public Users User { get; set; }

    }
}
