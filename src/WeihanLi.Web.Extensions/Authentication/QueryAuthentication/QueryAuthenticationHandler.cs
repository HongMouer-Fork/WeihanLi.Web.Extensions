﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WeihanLi.AspNetCore.Authentication.QueryAuthentication
{
    public class QueryAuthenticationHandler : AuthenticationHandler<QueryAuthenticationOptions>
    {
        public QueryAuthenticationHandler(IOptionsMonitor<QueryAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Query.Count > 0 && await Options.AuthenticationValidator(Context))
            {
                var claims = new List<Claim>();
                if (Request.Query.TryGetValue(Options.UserIdQueryKey, out var userIdValues))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdValues.ToString()));
                }
                if (Request.Query.TryGetValue(Options.UserNameQueryKey, out var userNameValues))
                {
                    claims.Add(new Claim(ClaimTypes.Name, userNameValues.ToString()));
                }
                if (Request.Query.TryGetValue(Options.UserRolesQueryKey, out var userRolesValues))
                {
                    var userRoles = userRolesValues.ToString()
                        .Split(new[] { Options.Delimiter }, StringSplitOptions.RemoveEmptyEntries);
                    claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r)));
                }
                if (Options.AdditionalQueryToClaims.Count > 0)
                {
                    foreach (var queryToClaim in Options.AdditionalQueryToClaims)
                    {
                        if (Request.Query.TryGetValue(queryToClaim.Key, out var queryValues))
                        {
                            foreach (var val in queryValues.ToString().Split(new[] { Options.Delimiter }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                claims.Add(new Claim(queryToClaim.Value, val));
                            }
                        }
                    }
                }
                // claims identity 's authentication type can not be null https://stackoverflow.com/questions/45261732/user-identity-isauthenticated-always-false-in-net-core-custom-authentication
                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }

            return AuthenticateResult.NoResult();
        }
    }
}