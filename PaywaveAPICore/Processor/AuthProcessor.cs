using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaywaveAPICore;
using PaywaveAPICore.Authentication;
using PaywaveAPICore.Extension;
using PaywaveAPICore.Utilities;
using PaywaveAPIData.DataService.Interface;
using PaywaveAPIData.Enum;
using PaywaveAPIData.Model;
using PaywaveAPIData.Response;
using PaywaveAPIData.Response.Auth;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaywaveAPICore.Processor
{
    public class AuthProcessor 
    {
        private readonly AppSettings _appSettings;
        private readonly JwtTokenConfig _jwtTokenConfig;
        private readonly IAutheticationDataService _authenticaDataService;
        private readonly ITokenDataService _tokenDataService;

        public AuthProcessor(IOptions<AppSettings> appSettings, IAutheticationDataService autheticationDataService,
            IOptions<JwtTokenConfig> jwtTokenConfig, ITokenDataService tokenDataService)
        {
            _appSettings = appSettings.Value;
            _authenticaDataService = autheticationDataService;
            _jwtTokenConfig = jwtTokenConfig.Value;
            _tokenDataService = tokenDataService;
        }

        public bool ForgetPassWord(ForgetPasswordModel forgetPasswordModel)
        {
            throw new NotImplementedException();
        }
        public ServiceResponse<Client> GetClient(string clientId)
        {
            ServiceResponse<Client> resp = new();
            Client data = _authenticaDataService.GetClientById(clientId);
            if (data == null)
            {
                resp.statusCode = ResponseStatus.NOT_FOUND;
                resp.message = "Email is incorrect";
                resp.data = null;
                return resp;
            }
            resp.data = data;
            resp.message = "The process was completed successully";
            return resp;
        }

        public ServiceResponse<AutheticationResponse> Login(LoginModel loginModel)
        {
            ServiceResponse<AutheticationResponse> resp = new();
            if (string.IsNullOrEmpty(loginModel.Email) || string.IsNullOrEmpty(loginModel.Password))
            {
                resp.message = "Email and Password is required";
                resp.statusCode = ResponseStatus.BAD_REQUEST;
                return resp;
            }
            var userData = _authenticaDataService.GetClientByEmail(loginModel.Email);
            if(userData is null)
            {
                resp.statusCode = ResponseStatus.NOT_FOUND;
                resp.message = "Email is incorrect";
                resp.data = null;
                return resp;
            }
            if (PasswordHasher.Verify(loginModel.Password, userData.password) == false)
            {
                resp.statusCode = ResponseStatus.UNAUTHORIZED;
                resp.message = "Email and Password Mismatch, Try again";
                resp.data = null;
                return resp;
            }
            var data = generateJwtToken(userData);
            //Genereate A Token Here and send to the user log the token on the db and use the refresh token to constantly refresh the jwt token
            resp.statusCode = ResponseStatus.OK;
            resp.message = "Login Successful";
            resp.data = data;
            return resp;
        }


        public ServiceResponse<SignupResp> SignUp(SignupModel signupModel)
        {
            ServiceResponse<SignupResp> resp = new ();
            if (string.IsNullOrEmpty(signupModel.UserName) || string.IsNullOrEmpty(signupModel.Password) || string.IsNullOrEmpty(signupModel.ConfirmPassword) || string.IsNullOrEmpty(signupModel.Email))
            {
                resp.message = "Email, Username and Password is required";
                resp.statusCode = ResponseStatus.BAD_REQUEST;
                return resp;
            }
            if (!PasswordValidatorExtension.IsPasswordValid(signupModel.Password))
            {
                resp.message = "Password Must Contain a Capital, Lower Alphabet, Number ,Special Character and Lenght must be greater than 8";
                resp.statusCode = ResponseStatus.BAD_REQUEST;
                return resp;
            }
            if (signupModel.Password != signupModel.ConfirmPassword)
            {
                resp.message = "Password Mismatch";
                resp.statusCode = ResponseStatus.BAD_REQUEST;
                return resp;
            }
            Client client = createClient(signupModel);
            SignupResp signupResp = new SignupResp
            {
                Id = client.ID,
                email = client.email,
                userName = client.userName,
            };

            if (_authenticaDataService.GetClientByUserName(client.userName) != null)
            {
                resp.message = "The chosen username is unavailable";
                resp.statusCode = ResponseStatus.CONFLICT;
                return resp;
            }

            client.password = PasswordHasher.Hash(client.password);
            var data = _authenticaDataService.InsertClient(client);
            if(data == true)
            {
                resp.message = "User registered Successfully";
                resp.statusCode = ResponseStatus.CREATED;
                resp.data = signupResp;
                return resp;
            }
            
            resp.message = "Sign-Up Unsuccessful";
            resp.statusCode = ResponseStatus.REQUEST_TIMEOUT;

            return resp;

        }

        public ServiceResponse<string> UpdateDetails(UpdateDataModel model, string email)
        {
            ///update this to use the information gotten from the token 
            ///To authenticate a user and get the needed information from it to find the user
            ServiceResponse<string> resp = new();
            Client data = _authenticaDataService.GetClientByEmail(email);
            if(data is null)
            {
                resp.statusCode = ResponseStatus.NOT_FOUND;
                resp.message = "Email is incorrect";
                resp.data = null;
                return resp;
            }
            data.bvn = model.bvn;
            data.firstName = model.firstName;
            data.lastName = model.lastName;
            data.middleName = model.middleName;
            data.phoneNumber = model.phoneNumber;
            data.transactionPin = model.transactionPin;

            data.address = model.address;
            data.next_of_kin = model.next_of_kin;
            _authenticaDataService.UpdateClient(data);

            resp.statusCode = ResponseStatus.OK;
            resp.message = "Details Updated Successfully";
            resp.data = data.ID;
            return resp;
        }

        private static Client createClient(SignupModel model)
        {
            return new Client
            {
                ID = Guid.NewGuid().ToString(),
                email = model.Email.ToLower(),
                userName = model.UserName.ToLower(),
                password = model.Password
            };
        }


        private AutheticationResponse generateJwtToken(Client client)
        {
            DateTime now = DateTime.Now;
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtTokenConfig.Secret);
            var claims = new List<Claim>
                {
                    new Claim("ID", client.ID),
                    new Claim(ClaimTypes.UserData, client.userName),
                    new Claim(ClaimTypes.Email, client.email),
                };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtTokenConfig.Issuer,
                Audience = _jwtTokenConfig.Audience,
                IssuedAt = now,
                Subject = new ClaimsIdentity(claims),
                Expires = now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            AutheticationResponse resp = new AutheticationResponse();
            resp.AccessToken = tokenHandler.WriteToken(token);
            resp.clientId = client.ID;
            resp.ExpiresIn = now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration);

            //Save token to Db
            var oldRefreshToken = _tokenDataService.GetToken(client.ID);

            var newRefreshToken = new RefreshToken
            {
                ClientEmail = client.email,
                Token = GenerateStringExtension.GenerateRandomBase64(),
                ExpireAt = now.AddMinutes(_jwtTokenConfig.RefreshTokenExpiration)
            };

            if (oldRefreshToken is null)
            {
                _tokenDataService.AddToken(newRefreshToken);
            }
            else
            {
                _tokenDataService.RemoveToken(oldRefreshToken.ID);
                _tokenDataService.AddToken(newRefreshToken);
            }
            resp.RefreshToken = newRefreshToken.Token;
            return resp;
        }
    }
}
