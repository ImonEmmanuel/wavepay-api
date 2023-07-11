using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaywaveAPIData.Response.Auth
{
    public class AutheticationResponse
    {
        public string AccessToken { get; set; }
        public bool Status { get; set; } = true;
        public DateTime ExpiresIn { get; set; }
        public string clientId { get; set; }
        public string RefreshToken { get; set; }

    }
}
