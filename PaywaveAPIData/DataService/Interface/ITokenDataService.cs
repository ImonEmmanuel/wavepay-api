using PaywaveAPIData.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaywaveAPIData.DataService.Interface
{
    public interface ITokenDataService
    {
        RefreshToken GetTokenbyClientId(string clientId );
        RefreshToken GetToken(string refreshToken);
        bool AddToken(RefreshToken token);
        bool RemoveToken(string tokenId);
        bool UpdateToken(RefreshToken token, string tokenId);
    }
}
