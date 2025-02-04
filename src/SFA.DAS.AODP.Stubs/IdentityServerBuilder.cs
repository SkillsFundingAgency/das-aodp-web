using System;
using System.Net;
using System.Security.Claims;
using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using static System.Net.WebRequestMethods;

namespace SFA.DAS.AODP.Stubs
{
    public class IdentityServerBuilder
    {
        private readonly WireMockServer _server;
        private readonly Fixture _fixture;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly SigningKeyInfo _signingKeyInfo;
        private readonly string _authority;
        private const string _audience = "TestAudience";
        private const string _secretKey = "abcdefghijklmnopqrstuv123456789==";

        public IdentityServerBuilder(int port)
        {
            _fixture = new Fixture();
            _authority = $"https://localhost:{port}/Auth";
            _server = WireMockServer.StartWithAdminInterface(port, useSSL: true);
            _jwtTokenGenerator = new JwtTokenGenerator(_authority, _audience, _secretKey);
            _signingKeyInfo = _jwtTokenGenerator.GetSigningKeyInfo();
        }

        public static IdentityServerBuilder Create(int port)
        {
            return new IdentityServerBuilder(port);
        }

        public WireMockServer Build()
        {
            Console.WriteLine($"IdentityServer Running ({_server.Urls[0]})");
            return _server;
        }

        public IdentityServerBuilder WithPing()
        {
            _server
                .Given(
                    Request.Create()
                        .WithPath($"/api/ping")
                        .UsingGet()
                )
                .RespondWith(
                    Response.Create()
                        .WithStatusCode((int)HttpStatusCode.OK)
                );

            return this;
        }

        public IdentityServerBuilder WithWellKnownOpenIdEndpoint()
        {            
            _server
                .Given(Request.Create()
                .UsingMethod("GET")
                .WithPath("/Auth/.well-known/openid-configuration"))
                .AtPriority(1)
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithBodyAsJson(new
                    {
                        issuer = $"{_jwtTokenGenerator.Authority}",
                        authorization_endpoint = $"{_jwtTokenGenerator.Authority}/authorize",
                        token_endpoint = $"{_jwtTokenGenerator.Authority}/oauth/token",
                        device_authorization_endpoint = $"{_jwtTokenGenerator.Authority}/oauth/device/code",
                        userinfo_endpoint = $"{_jwtTokenGenerator.Authority}/userinfo",
                        mfa_challenge_endpoint = $"{_jwtTokenGenerator.Authority}/mfa/challenge",
                        jwks_uri = $"{_jwtTokenGenerator.Authority}/.well-known/jwks.json",
                        registration_endpoint = $"{_jwtTokenGenerator.Authority}/oidc/register",
                        revocation_endpoint = $"{_jwtTokenGenerator.Authority}/oauth/revoke",
                        scopes_supported = new[] { "openid", "profile", "offline_access", "name", "given_name", "family_name", "nickname", "email", "email_verified", "picture", "created_at", "identities", "phone", "address" },
                        response_types_supported = new[] { "code", "token", "id_token", "code token", "code id_token" },
                        code_challenge_methods_supported = new[] { "S256", "plain" },
                        response_modes_supported = new[] { "query", "fragment", "form_post" },
                        subject_types_supported = new[] { "public" },
                        id_token_signing_alg_values_supported = new[] { "HS256", "RS256", "PS256" },
                        token_endpoint_auth_methods_supported = new[] { "client_secret_basic", "client_secret_post", "private_key_jwt" },
                        claims_supported = new[] { "aud", "auth_time", "created_at", "email", "email_verified", "exp", "family_name", "given_name", "iat", "identities", "iss", "name", "nickname", "phone_number", "picture", "sub" },
                        request_uri_parameter_supported = false,
                        request_parameter_supported = false,
                        token_endpoint_auth_signing_alg_values_supported = new[] { "RS256", "RS384", "PS256" },
                        backchannel_logout_supported = true,
                        backchannel_logout_session_supported = true,
                        end_session_endpoint = $"{_jwtTokenGenerator.Authority}/oidc/logout"
                    })
                );

            return this;
        }

        public IdentityServerBuilder WithSigningKeyInfoEndpoint()
        {
            _server
                .Given(Request.Create()
                .UsingMethod("GET")
                .WithPath("/Auth/.well-known/jwks.json"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithBodyAsJson(new
                    {
                        keys = new[]
                        {
                            new
                            {
                                kty = "RSA",
                                use = "sig",
                                n =  _signingKeyInfo.Modulus,
                                e = _signingKeyInfo.Exponent,
                                kid =  _signingKeyInfo.Kid,
                                alg = _signingKeyInfo.Algorithm
                            }
                        }
                    })
                );

            return this;
        }

        public IdentityServerBuilder WithAuthorizeEndpoint()
        {
            //request:
            //https://authorization-server.com/auth?response_type=code
            //&client_id = 29352735982374239857
            //& redirect_uri = https://example-app.com/callback
            //&scope = create + delete
            //& state = xcoivjuywkdkhvusuye3kch

            //GET /authorize?
            //response_type=code%20id_token
            //&client_id=s6BhdRkqt3
            //&redirect_uri=https%3A%2F%2Fclient.example.org%2Fcb
            //&scope=openid%20profile%20email
            //&nonce=n-0S6_WzA2Mj
            //&state=af0ifjsldkj HTTP/1.1

            //response
            //HTTP / 1.1 302 Found
            //Location: https://client.example.org/cb#
            //code = SplxlOBeZQQYbYS6WxSbIA
            //& id_token = eyJ0... NiJ9.eyJ1c... I6IjIifX0.DeWt4Qu... ZXso
            //& state = af0ifjsldkj

            _server
                .Given(Request.Create()
                .UsingMethod("GET")
                .WithPath("/authorize"))
                .RespondWith(Response.Create()
                    .WithStatusCode(302)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithBodyAsJson("TBC")
                );

            return this;
        }

        public IdentityServerBuilder WithTokenEndpoint()
        {
            // Create JWT with desired claims
            var authorizationToken = _jwtTokenGenerator.CreateToken
            (
                customClaims: new Claim[]
                {
                    new("email", "sample@email.com"),
                    new("permissions", "SamplePermission1"),
                    new("permissions", "SamplePermission2"),
                },
                expiresAt: DateTime.UtcNow.AddHours(1)
            );

            _server
                .Given(Request.Create()
                .UsingMethod("GET")
                .WithPath("/oauth/token"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithBodyAsJson(authorizationToken)
                );

            return this;
        }

        public IdentityServerBuilder WithUserInfoEndpoint()
        {
            _server
                .Given(Request.Create()
                .UsingMethod("GET")
                .WithPath("/userinfo"))
                .RespondWith(Response.Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json; charset=utf-8")
                    .WithBodyAsJson("TBC")
                );

            return this;
        }
    }
}