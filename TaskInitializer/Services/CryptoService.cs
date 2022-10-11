using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace TaskInitializer.Services
{
    public class CryptoService
    {
        public static async Task<string> GeneratePlayStoreToken(string CredentialPath)
        {
            RSACryptoServiceProvider RSACryptoServiceProvider = new RSACryptoServiceProvider();

            JObject CredentialObject = JObject.Parse(await new StreamReader(CredentialPath).ReadToEndAsync());

            PemReader PemReader = new PemReader(new StringReader(CredentialObject["private_key"].ToString()));

            RsaPrivateCrtKeyParameters RsaPrivateCrtKeyParameters = (RsaPrivateCrtKeyParameters)PemReader.ReadObject();

            RSAParameters RSAParameters = DotNetUtilities.ToRSAParameters(RsaPrivateCrtKeyParameters);

            RSACryptoServiceProvider.ImportParameters(RSAParameters);

            Claim[] ClaimArray = new Claim[] { new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(DateTime.UtcNow.AddMinutes(60)).ToUnixTimeSeconds().ToString()), new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), new Claim(JwtRegisteredClaimNames.Iss, CredentialObject["client_email"].ToString()), new Claim(JwtRegisteredClaimNames.Aud, CredentialObject["token_uri"].ToString()), new Claim("scope", "https://www.googleapis.com/auth/androidpublisher") };

            RsaSecurityKey RsaSecurityKey = new RsaSecurityKey(RSACryptoServiceProvider);

            SigningCredentials SigningCredentials = new SigningCredentials(RsaSecurityKey, SecurityAlgorithms.RsaSha256);

            JwtSecurityToken JwtSecurityToken = new JwtSecurityToken(claims: ClaimArray, signingCredentials: SigningCredentials);

            return new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken);
        }
    }
}