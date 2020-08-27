﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using API.Data;
using API.Models;
using API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions
{
    public class BasicAuthenticationOptions : AuthenticationSchemeOptions
    {
    }

    public class CustomAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        private readonly DataContext _context;
        private readonly IAuthService _authService;
        public CustomAuthenticationHandler(IOptionsMonitor<BasicAuthenticationOptions> options, ILoggerFactory logger,
            UrlEncoder encoder, ISystemClock clock, DataContext context, IAuthService authService) : base(options, logger, encoder, clock)
        {
            _context = context;
            _authService = authService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();
            string authorizationHeader = Request.Headers["Authorization"].ToString().Split(" ").Last();
            if (string.IsNullOrEmpty(authorizationHeader))
                return AuthenticateResult.NoResult();
            try
            {
                return AuthenticateUser(authorizationHeader);
            }
            catch (SecurityTokenExpiredException)
            {
                try
                {
                    if (!Request.Headers.ContainsKey("refresh-token"))
                        return AuthenticateResult.Fail("Unauthorized");
                    var token = Request.Headers["refresh-token"].ToString().Split(" ").Last();
                    
                    var jwt = await _authService.CreateJwtFromRefreshToken(token);
                    var result = AuthenticateUser(jwt);
                    Request.HttpContext.Response.Cookies.Append("jwt", jwt);
                    Request.HttpContext.Response.Headers.Add("New-Jwt", jwt);
                    return result;
                }
                catch
                {
                    return AuthenticateResult.Fail("Unauthorized");
                }
            }
            catch
            {
                Console.WriteLine("Why here?");
                return AuthenticateResult.Fail("Unauthorized");
            }
        }

        private JwtSecurityToken ValidateToken(string token, byte[] key)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            return (JwtSecurityToken) validatedToken;

        }

        private AuthenticateResult AuthenticateUser(string token)
        {
            var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("TOKEN"));
            var jwtToken = ValidateToken(token, key);
            var identity = new ClaimsIdentity(jwtToken.Claims, Scheme.Name);
            var role = jwtToken.Claims.Where(x => x.Type == "role").Select(x => x.Value).ToArray();
            var principal = new GenericPrincipal(identity,role);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

    }
}