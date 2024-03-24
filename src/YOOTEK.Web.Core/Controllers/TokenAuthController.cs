using Abp;
using Abp.Auditing;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.Configuration;
using Abp.Extensions;
using Abp.MultiTenancy;
using Abp.Runtime.Caching;
using Abp.Runtime.Security;
using Abp.Runtime.Session;
using Abp.UI;
using Yootek.Authentication.External;
using Yootek.Authentication.JwtBearer;
using Yootek.Authorization;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.Configuration;
using Yootek.Identity;
using Yootek.Models.TokenAuth;
using Yootek.MultiTenancy;
using Yootek.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using NUglify.Helpers;

namespace Yootek.Controllers
{

    [Route("api/[controller]/[action]")]
    [Audited]
    public class TokenAuthController : YootekControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly UserManager _userManager;
        private readonly SignInManager _signInManager;
        private readonly TenantManager _tenantManager;
        private readonly ITenantCache _tenantCache;
        private readonly IWebUrlService _webUrlService;
        private readonly ICacheManager _cacheManager;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;
        private readonly IdentityOptions _identityOptions;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly ISettingManager _settingManager;
        private readonly IOptions<JwtBearerOptions> _jwtOptions;
        private readonly IJwtSecurityStampHandler _securityStampHandler;
        private readonly AbpUserClaimsPrincipalFactory<User, Role> _claimsPrincipalFactory;

        public TokenAuthController(
            LogInManager logInManager,
            UserManager userManager,
            TenantManager tenantManager,
            ITenantCache tenantCache,
            IWebUrlService webUrlService,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            IOptions<IdentityOptions> identityOptions,
            SignInManager signInManager,
            IAppConfigurationAccessor configurationAccessor,
            ISettingManager settingManager,
            UserRegistrationManager userRegistrationManager,
            IJwtSecurityStampHandler securityStampHandler,
            IOptions<JwtBearerOptions> jwtOptions,
            AbpUserClaimsPrincipalFactory<User, Role> claimsPrincipalFactory,
            ICacheManager cacheManager
            )
        {
            _logInManager = logInManager;
            _userManager = userManager;
            _tenantManager = tenantManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _signInManager = signInManager;
            _webUrlService = webUrlService;
            _identityOptions = identityOptions.Value;
            _appConfiguration = configurationAccessor.Configuration;
            _settingManager = settingManager;
            _jwtOptions = jwtOptions;
            _claimsPrincipalFactory = claimsPrincipalFactory;
            _cacheManager = cacheManager;
            _securityStampHandler = securityStampHandler;
        }

        [HttpPost]     
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModelTenant model)
        {
            try
            {
                Logger.Fatal(JsonConvert.SerializeObject(model));
                long t1 = TimeUtils.GetNanoseconds();


                var mobileSetting = _appConfiguration["MobileVersion"] ?? "0.0.2";
                var loginResult = await GetLoginResultAsync(
                    model.UserNameOrEmailAddress,
                    model.Password,
                    !string.IsNullOrEmpty(model.TenancyName) ? model.TenancyName : GetTenancyNameOrNull()
                );

                var vrs = new
                {
                    appVersion = new
                    {
                        version = mobileSetting,
                        typeVersion = 1
                    }

                };
                var mobileConfig = JsonConvert.SerializeObject(vrs);
                var claimToken = await CreateJwtClaims(loginResult.Identity, loginResult.User,
                    tokenType: TokenType.RefreshToken);

                var refreshToken = CreateRefreshToken(claimToken);
                var claimRefresh = await CreateJwtClaims(loginResult.Identity, loginResult.User,
                  refreshTokenKey: refreshToken.key);
                var accessToken = CreateAccessToken(claimRefresh);
                mb.statisticMetris(t1, 0, "authenticate");
                var result = new AuthenticateResultModel
                {
                    AccessToken = accessToken,
                    EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                    ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds,
                    UserId = loginResult.User.Id,
                    EmailAddress = loginResult.User.EmailAddress,
                    TenantId = loginResult.Tenant != null ? loginResult.Tenant.Id : 0,
                    ThirdAccounts = loginResult.User.ThirdAccounts,
                    MobileConfig = loginResult.Tenant != null ? loginResult.Tenant.MobileConfig : mobileConfig,
                    RefreshToken = refreshToken.token,
                    RefreshTokenExpireInSeconds = (int)_configuration.RefreshTokenExpiration.TotalSeconds,
                };

                Logger.Fatal(JsonConvert.SerializeObject(result));
                return result;
            }
            catch(UserFriendlyException e)
            {
                Logger.Fatal(e.Message, e);
                throw;
            }
        }

        [HttpPost]
        public async Task<RefreshTokenResult> RefreshToken(string refreshToken)
        {
            Logger.Fatal(refreshToken);
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentNullException(nameof(refreshToken));
            }

            if (!IsRefreshTokenValid(refreshToken, out var principal))
            {
                throw new UserFriendlyException("Refresh token is not valid!");
            }

            try
            {
                var user = await _userManager.GetUserAsync(
                    UserIdentifier.Parse(principal.Claims.First(x => x.Type == AppConsts.UserIdentifier).Value)
                );

                if (user == null)
                {
                    throw new UserFriendlyException("Unknown user or user identifier");
                }

                principal = await _claimsPrincipalFactory.CreateAsync(user);

                var accessToken = CreateAccessToken(await CreateJwtClaims(principal.Identity as ClaimsIdentity, user));

                var result = new RefreshTokenResult(
                    accessToken,
                    GetEncryptedAccessToken(accessToken),
                    (int)_configuration.AccessTokenExpiration.TotalSeconds);
                Logger.Fatal(JsonConvert.SerializeObject(result));

                return await Task.FromResult(result);
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ValidationException("Refresh token is not valid!", e);
            }
        }


        [HttpPost]
        public async Task<RefreshTokenResult> RefreshErpToken([FromBody] RefreshTokenModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Token))
            {
                throw new ArgumentNullException(nameof(model.Token));
            }

            if (!IsRefreshTokenValid(model.Token, out var principal))
            {
                throw new UserFriendlyException("Refresh token is not valid!");
            }

            try
            {
                var user = await _userManager.GetUserAsync(
                    UserIdentifier.Parse(principal.Claims.First(x => x.Type == AppConsts.UserIdentifier).Value)
                );

                if (user == null)
                {
                    throw new UserFriendlyException("Unknown user or user identifier");
                }

                principal = await _claimsPrincipalFactory.CreateAsync(user);

                var claims = (await CreateJwtClaims(principal.Identity as ClaimsIdentity, user)).ToList();

                if (model.StoreId.HasValue)
                {
                    claims.Add(new Claim(ErpClaimTypes.StoreId, model.StoreId.ToString()));
                }

                if (model.BranchId.HasValue)
                {
                    claims.Add(new Claim(ErpClaimTypes.BranchId, model.BranchId.ToString()));
                }

                var accessToken = CreateAccessToken(claims);

                var result = new RefreshTokenResult(
                    accessToken,
                    GetEncryptedAccessToken(accessToken),
                    (int)_configuration.AccessTokenExpiration.TotalSeconds);
                Logger.Fatal(JsonConvert.SerializeObject(result));

                return await Task.FromResult(result);
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ValidationException("Refresh token is not valid!", e);
            }
        }


        [HttpGet]
        [AbpAuthorize]
        public async Task LogOut()
        {
            if (AbpSession.UserId != null)
            {

                var tokenValidityKeyInClaims = User.Claims.ToList().First(c => c.Type == AppConsts.TokenValidityKey);
                await RemoveTokenAsync(tokenValidityKeyInClaims.Value);

                var refreshTokenValidityKeyInClaims =
                    User.Claims.FirstOrDefault(c => c.Type == AppConsts.RefreshTokenValidityKey);
                if (refreshTokenValidityKeyInClaims != null)
                {
                    await RemoveTokenAsync(refreshTokenValidityKeyInClaims.Value);
                }

                //if (AllowOneConcurrentLoginPerUser())
                //{
                //    await _securityStampHandler.RemoveSecurityStampCacheItem(
                //        AbpSession.TenantId,
                //        AbpSession.GetUserId()
                //    );
                //}
            }
        }

        private bool AllowOneConcurrentLoginPerUser()
        {
            return _settingManager.GetSettingValue<bool>(AppSettings.UserManagement.AllowOneConcurrentLoginPerUser);
        }

        private async Task RemoveTokenAsync(string tokenKey)
        {
            await _userManager.RemoveTokenValidityKeyAsync(
                await _userManager.GetUserAsync(AbpSession.ToUserIdentifier()), tokenKey
            );

            await _cacheManager.GetCache(AppConsts.TokenValidityKey).RemoveAsync(tokenKey);
        }

        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            var a = new Dictionary<string, string>();
            a.Add("UserInfoEndpoint", "https://www.googleapis.com/oauth2/v2/userinfo");
            var data = new ExternalLoginProviderInfoModel()
            {
                Name = "Google",
                ClientId = "1013268474414-h9c95rfojtfeai5thtcn90l45jonsga3.apps.googleusercontent.com",
                AdditionalParams = a
            };
            var result = new List<ExternalLoginProviderInfoModel>();
            result.Add(data);
            return result;
        }

        private async Task<IEnumerable<Claim>> CreateJwtClaims(
         ClaimsIdentity identity, User user,
         TimeSpan? expiration = null,
         TokenType tokenType = TokenType.AccessToken,
         string refreshTokenKey = null)
        {
            var tokenValidityKey = Guid.NewGuid().ToString();
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == _identityOptions.ClaimsIdentity.UserIdClaimType);

            if (_identityOptions.ClaimsIdentity.UserIdClaimType != JwtRegisteredClaimNames.Sub)
            {
                claims.Add(new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value));
            }

            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
                    ClaimValueTypes.Integer64),
                new Claim(AppConsts.TokenValidityKey, tokenValidityKey),
                new Claim(AppConsts.UserIdentifier, user.ToUserIdentifier().ToUserIdentifierString()),
                new Claim(AppConsts.TokenType, tokenType.To<int>().ToString())
            });

            if (!string.IsNullOrEmpty(refreshTokenKey))
            {
                claims.Add(new Claim(AppConsts.RefreshTokenValidityKey, refreshTokenKey));
            }

            if (!expiration.HasValue)
            {
                expiration = tokenType == TokenType.AccessToken
                    ? _configuration.AccessTokenExpiration
                    : _configuration.RefreshTokenExpiration;
            }

            var expirationDate = DateTime.UtcNow.Add(expiration.Value);

            await _cacheManager
                .GetCache(AppConsts.TokenValidityKey)
                .SetAsync(tokenValidityKey, "", absoluteExpireTime: new DateTimeOffset(expirationDate));

            using (CurrentUnitOfWork.SetTenantId(user.TenantId))
            {
                await _userManager.AddTokenValidityKeyAsync(
                user,
                tokenValidityKey,
                expirationDate
                );
            }

            return claims;
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            try
            {
                //var externalUser = await GetExternalUserInfo(model);

                var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                //var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
                var externalUser = new ExternalAuthUserInfo()
                {
                    Provider = model.AuthProvider,
                    ProviderKey = model.ProviderKey,
                    Name = model.Name,
                    EmailAddress = model.EmailAddress,
                    Surname = model.SurName
                };

                switch (loginResult.Result)
                {
                    case AbpLoginResultType.Success:
                        {
                            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));
                            return new ExternalAuthenticateResultModel
                            {
                                AccessToken = accessToken,
                                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                                ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds
                            };
                        }
                    case AbpLoginResultType.UnknownExternalLogin:
                        {
                            var newUser = await RegisterExternalUserAsync(externalUser);
                            if (!newUser.IsActive)
                            {
                                return new ExternalAuthenticateResultModel
                                {
                                    WaitingForActivation = true
                                };
                            }

                            // Try to login again with newly registered user!
                            loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                            if (loginResult.Result != AbpLoginResultType.Success)
                            {
                                throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                    loginResult.Result,
                                    model.ProviderKey,
                                    GetTenancyNameOrNull()
                                );
                            }

                            return new ExternalAuthenticateResultModel
                            {
                                AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity)),
                                ExpireInSeconds = (int)_configuration.AccessTokenExpiration.TotalSeconds
                            };
                        }
                    default:
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        [HttpPost]
        public ActionResult ExternalLogin(string provider, string returnUrl, string ss = "")
        {
            var redirectUrl = Url.Action(
                "ExternalLoginCallback",
                "TokenAuth",
                new
                {
                    ReturnUrl = returnUrl,
                    authSchema = provider,
                    ss = ss
                });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        //[HttpGet("ExternalLoginCallback")]
        //public async Task<ActionResult> ExternalLoginCallback(string returnUrl, string remoteError = null,
        //    string ss = "")
        //{
        //    returnUrl = NormalizeReturnUrl(returnUrl);

        //    if (remoteError != null)
        //    {
        //        Logger.Error("Remote Error in ExternalLoginCallback: " + remoteError);
        //        throw new UserFriendlyException(L("CouldNotCompleteLoginOperation"));
        //    }

        //    var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
        //    if (externalLoginInfo == null)
        //    {
        //        Logger.Warn("Could not get information from external login.");
        //        return new RedirectResult($"{returnUrl}?error=externalsigninerror");
        //    }

        //    var tenancyName = GetTenancyNameOrNull();

        //    var loginResult = await _logInManager.LoginAsync(externalLoginInfo, tenancyName);

        //    switch (loginResult.Result)
        //    {
        //        case AbpLoginResultType.Success:
        //            {
        //                await _signInManager.SignInAsync(loginResult.Identity, false);

        //                if (!string.IsNullOrEmpty(ss) && ss.Equals("true", StringComparison.OrdinalIgnoreCase) &&
        //                    loginResult.Result == AbpLoginResultType.Success)
        //                {
        //                    loginResult.User.SetSignInToken();
        //                    returnUrl = AddSingleSignInParametersToReturnUrl(returnUrl, loginResult.User.SignInToken,
        //                        loginResult.User.Id, loginResult.User.TenantId);
        //                }
        //                return new RedirectResult(returnUrl);
        //            }
        //        case AbpLoginResultType.UnknownExternalLogin:
        //            {
        //                var email = externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email);

        //                (string name, string surname) nameInfo;
        //                nameInfo = GetNameAndSurnameFromClaims(
        //                       externalLoginInfo.Principal.Claims.ToList(), _identityOptions);

        //                var viewModel = new ExternalAuthUserInfo
        //                {
        //                    EmailAddress = email,
        //                    Name = nameInfo.name,
        //                    Surname = nameInfo.surname,
        //                };

        //                var newUser = await RegisterExternalUserAsync(viewModel);

        //                return new RedirectResult(returnUrl);
        //            }
        //        default:
        //            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
        //                loginResult.Result,
        //                externalLoginInfo.Principal.FindFirstValue(ClaimTypes.Email) ?? externalLoginInfo.ProviderKey,
        //                tenancyName
        //            );
        //    }
        //}



        //private

        //private (string name, string surname) GetNameAndSurnameFromClaims(List<Claim> claims, IdentityOptions identityOptions)
        //{
        //    string name = null;
        //    string surname = null;

        //    var givenNameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
        //    if (givenNameClaim != null && !givenNameClaim.Value.IsNullOrEmpty())
        //    {
        //        name = givenNameClaim.Value;
        //    }

        //    var surnameClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname);
        //    if (surnameClaim != null && !surnameClaim.Value.IsNullOrEmpty())
        //    {
        //        surname = surnameClaim.Value;
        //    }

        //    if (name == null || surname == null)
        //    {
        //        var nameClaim = claims.FirstOrDefault(c => c.Type == identityOptions.ClaimsIdentity.UserNameClaimType);
        //        if (nameClaim != null)
        //        {
        //            var nameSurName = nameClaim.Value;
        //            if (!nameSurName.IsNullOrEmpty())
        //            {
        //                var lastSpaceIndex = nameSurName.LastIndexOf(' ');
        //                if (lastSpaceIndex < 1 || lastSpaceIndex > (nameSurName.Length - 2))
        //                {
        //                    name = surname = nameSurName;
        //                }
        //                else
        //                {
        //                    name = nameSurName.Substring(0, lastSpaceIndex);
        //                    surname = nameSurName.Substring(lastSpaceIndex);
        //                }
        //            }
        //        }
        //    }

        //    return (name, surname);
        //}

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Name,
                externalUser.Surname,
                externalUser.EmailAddress,
                externalUser.EmailAddress,
                Authorization.Users.User.CreateRandomPassword(),
                true,
                false
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.AccessTokenExpiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private async Task<string> GetThirdAccountMultiTenantAsync(string email)
        {
            try
            {
                string result = null;
                var tenants = await _tenantManager.GetAllTenantAsync();
                if (tenants == null)
                {
                    return result;
                }

                foreach (var tenant in tenants)
                {

                    using (CurrentUnitOfWork.SetTenantId(tenant.Id))
                    {
                        var userth = await _userManager.FindByNameOrEmailAsync(email);
                        if (userth != null && userth.ThirdAccounts != null)
                        {
                            result = userth.ThirdAccounts;
                            break;
                        }
                    }
                }
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }

        //private string NormalizeReturnUrl(string returnUrl, Func<string> defaultValueBuilder = null)
        //{
        //    if (defaultValueBuilder == null)
        //    {
        //        defaultValueBuilder = GetAppHomeUrl;
        //    }

        //    if (returnUrl.IsNullOrEmpty())
        //    {
        //        return defaultValueBuilder();
        //    }

        //    if (AbpSession.UserId.HasValue)
        //    {
        //        return defaultValueBuilder();
        //    }

        //    if (Url.IsLocalUrl(returnUrl) ||
        //        _webUrlService.GetRedirectAllowedExternalWebSites().Any(returnUrl.Contains))
        //    {
        //        return returnUrl;
        //    }

        //    return defaultValueBuilder();
        //}

        //public string GetAppHomeUrl()
        //{
        //    return Url.Action("Index", "Home", new { area = "AppAreaName" });
        //}

        //private string AddSingleSignInParametersToReturnUrl(string returnUrl, string signInToken, long userId,
        //    int? tenantId)
        //{
        //    returnUrl += (returnUrl.Contains("?") ? "&" : "?") +
        //                 "accessToken=" + signInToken +
        //                 "&userId=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(userId.ToString()));
        //    if (tenantId.HasValue)
        //    {
        //        returnUrl += "&tenantId=" + Convert.ToBase64String(Encoding.UTF8.GetBytes(tenantId.Value.ToString()));
        //    }

        //    return returnUrl;
        //}

        private bool IsSchemeEnabledOnTenant(ExternalLoginProviderInfo scheme)
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return true;
            }

            switch (scheme.Name)
            {
                case "OpenIdConnect":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.OpenIdConnect_IsDeactivated, AbpSession.GetTenantId());
                case "Microsoft":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Microsoft_IsDeactivated, AbpSession.GetTenantId());
                case "Google":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Google_IsDeactivated, AbpSession.GetTenantId());
                case "Twitter":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Twitter_IsDeactivated, AbpSession.GetTenantId());
                case "Facebook":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.Facebook_IsDeactivated, AbpSession.GetTenantId());
                case "WsFederation":
                    return !_settingManager.GetSettingValueForTenant<bool>(
                        AppSettings.ExternalLoginProvider.Tenant.WsFederation_IsDeactivated, AbpSession.GetTenantId());
                default: return true;
            }
        }

        private (string token, string key) CreateRefreshToken(IEnumerable<Claim> claims)
        {
            var claimsList = claims.ToList();
            return (CreateToken(claimsList, AppConsts.RefreshTokenExpiration),
                claimsList.First(c => c.Type == AppConsts.TokenValidityKey).Value);
        }

        private string CreateToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                signingCredentials: _configuration.SigningCredentials,
                expires: expiration == null ? (DateTime?)null : now.Add(expiration.Value)
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }


        private bool IsRefreshTokenValid(string refreshToken, out ClaimsPrincipal principal)
        {
            principal = null;

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = _configuration.Audience,
                    ValidIssuer = _configuration.Issuer,
                    IssuerSigningKey = _configuration.SecurityKey
                };

                foreach (var validator in _jwtOptions.Value.SecurityTokenValidators)
                {
                    if (!validator.CanReadToken(refreshToken))
                    {
                        continue;
                    }

                    try
                    {
                        principal = validator.ValidateToken(refreshToken, validationParameters, out _);

                        if (principal.Claims.FirstOrDefault(x => x.Type == AppConsts.TokenType)?.Value ==
                            TokenType.RefreshToken.To<int>().ToString())
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex.ToString(), ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug(ex.ToString(), ex);
            }

            return false;
        }

    }
}
