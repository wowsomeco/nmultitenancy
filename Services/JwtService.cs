using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace MultiTenancy {
  public class JwtService {
    public int ExpiredDays { get; set; } = 7;

    private readonly string _secretKey;

    public JwtService(string secretKey) {
      _secretKey = secretKey;
    }

    public string Generate(params (string key, string value)[] claims) {
      var securityKey = new SymmetricSecurityKey(_secretKey.ToBytes());
      var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

      var tokenDescriptor = new SecurityTokenDescriptor {
        Subject = new ClaimsIdentity(ToClaims(claims)),
        Expires = DateTime.UtcNow.AddDays(ExpiredDays),
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