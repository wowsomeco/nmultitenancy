using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MultiTenancy {
  public class JwtService {
    public int? ExpiredDays {
      get {
        string expired = _appContext.Config[$"{_jwtPrefix}:ExpiredDays"];
        return expired.IsEmpty() ? null : int.Parse(expired);
      }
    }
    public string SecretKey => _appContext.Config[$"{_jwtPrefix}:Secret"];
    public string AuthToken => _appContext.AuthToken;

    private readonly string _jwtPrefix = "Jwt";
    private readonly ApplicationContext _appContext;

    public JwtService(ApplicationContext appContext) {
      _appContext = appContext;
    }

    public bool ValidateToken(out JwtSecurityToken jwtToken) {
      SecurityToken validatedToken = null;
      var securityKey = new SymmetricSecurityKey(SecretKey.ToBytes());
      var tokenHandler = new JwtSecurityTokenHandler();
      var authToken = _appContext.AuthToken ?? throw HttpException.BadRequest("Can not find Auth token in the request header");

      try {
        tokenHandler.ValidateToken(
          authToken.Replace("Bearer ", ""),
          new TokenValidationParameters {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = securityKey
          },
          out validatedToken
        );

        jwtToken = validatedToken as JwtSecurityToken;
      } catch {
        throw HttpException.Unauthorized("Invalid Auth token");
      }

      return true;
    }

    public string Generate(params (string key, string value)[] claims) {
      var securityKey = new SymmetricSecurityKey(SecretKey.ToBytes());
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var tokenDescriptor = new SecurityTokenDescriptor {
        Subject = new ClaimsIdentity(ToClaims(claims)),
        Expires = ExpiredDays != null ? DateTime.UtcNow.AddDays(ExpiredDays.Value) : null,
        SigningCredentials = credentials
      };

      var tokenHandler = new JwtSecurityTokenHandler();
      var token = tokenHandler.CreateToken(tokenDescriptor);

      return tokenHandler.WriteToken(token);
    }

    // TODO: might want to create an extension for this so it's reusable
    private Claim[] ToClaims(params (string key, string value)[] claims) {
      List<Claim> c = new List<Claim>();
      foreach (var tuple in claims) {
        c.Add(new Claim(tuple.key, tuple.value));
      }
      return c.ToArray();
    }
  }
}