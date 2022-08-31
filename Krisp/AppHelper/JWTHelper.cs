using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shared.Helpers;

namespace Krisp.AppHelper
{
	internal static class JWTHelper
	{
		public static string GenerateToken(string installationID, string sessionID, string secret, bool strong)
		{
			Logger logger = LogWrapper.GetLogger("JWTHelper");
			logger.LogDebug("JWTHelper: Generating jwt with installationID = {0}, sessionId = {1} and secret = {2}", new object[] { installationID, sessionID, secret });
			if (string.IsNullOrWhiteSpace(secret))
			{
				logger.LogError("JWTHelper: Cannot generate JWT without secret, terminating...");
				return "";
			}
			if (string.IsNullOrWhiteSpace(sessionID))
			{
				logger.LogError("JWTHelper: Cannot generate JWT without sessionID, terminating...");
				return "";
			}
			SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
			string text = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds().ToString();
			logger.LogDebug("JWTHelper: iat = {0}", new object[] { text });
			List<Claim> list = new List<Claim>
			{
				new Claim("installation_id", installationID),
				new Claim("session_id", sessionID),
				new Claim("iat", text)
			};
			if (DeviceLoginHelper.DeviceMode)
			{
				list.Add(new Claim("team_key", DeviceLoginHelper.TeamKey));
			}
			if (strong)
			{
				list.Add(new Claim("sign", "strong"));
			}
			SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(list),
				SigningCredentials = new SigningCredentials(symmetricSecurityKey, "HS256")
			};
			JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
			jwtSecurityTokenHandler.SetDefaultTimesOnTokenCreation = false;
			SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
			return jwtSecurityTokenHandler.WriteToken(securityToken);
		}
	}
}
