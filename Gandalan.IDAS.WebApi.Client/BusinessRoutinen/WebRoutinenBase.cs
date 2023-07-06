// *****************************************************************************
// Gandalan GmbH & Co. KG - (c) 2023
// *****************************************************************************
// Middleware//Gandalan.IDAS.WebApi.Client//WebRoutinenBase.cs
// Created: 27.01.2016
// Edit: phil - 31.05.2023 11:57
// *****************************************************************************

using Gandalan.IDAS.Client.Contracts.Contracts;
using Gandalan.IDAS.Logging;
using Gandalan.IDAS.Web;
using Gandalan.IDAS.WebApi.Client.Settings;
using Gandalan.IDAS.WebApi.DTO;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gandalan.IDAS.WebApi.Client
{
    public class WebRoutinenBase 
    {
        #region  Felder

        public IWebApiConfig Settings;
        public bool IsJwt;
        private RESTRoutinen _restRoutinen;

        #endregion

        #region  Eigenschaften

        public UserAuthTokenDTO AuthToken { get; set; }
        public string Status { get; private set; }
        public bool IgnoreOnErrorOccured { get; set; }
        public string JwtToken { get; set; }

        #endregion

        public static event ErrorOccuredEventHandler ErrorOccured;

        public delegate void ErrorOccuredEventHandler(object sender, ApiErrorArgs e);

        public WebRoutinenBase(IWebApiConfig settings)
        {
            // Settings werden kopiert, damit die abgeleiteten Klassen ggf. eigene Settings ergänzen/
            // ändern können (vor allem z.B. Base-URL)
            if (settings != null)
            {
                Settings = new WebApiSettings();
                Settings.CopyToThis(settings);
                if (settings is IJwtWebApiConfig jc)
                {
                    IsJwt = true;
                    JwtToken = jc.JwtToken;
                }
                if (settings.AuthToken?.Token != Guid.Empty && settings.AuthToken?.Expires > DateTime.UtcNow)
                {
                    AuthToken = settings.AuthToken;
                }
            }
        }

        private async Task runPreRequestChecks(bool skipAuth = false)
        {
            if (_restRoutinen == null)
                initRestRoutinen();

            if (!skipAuth && !await LoginAsync())
                throw new ApiUnauthorizedException("You are not authorized.");
        }

        private void initRestRoutinen()
        {
            var config = new HttpClientConfig()
            {
                BaseUrl = Settings.Url,
                UseCompression = Settings.UseCompression,
                UserAgent = Settings.UserAgent
            };

            if (IsJwt && !string.IsNullOrEmpty(JwtToken))
            {
                config.AdditionalHeaders.Add("Authorization", "Bearer " + JwtToken);
            }
            else if (AuthToken != null)
            {
                config.AdditionalHeaders.Add("X-Gdl-AuthToken", AuthToken.Token.ToString());
            }

            if (Settings.InstallationId != Guid.Empty)
            {
                config.AdditionalHeaders.Add("X-Gdl-InstallationId", Settings.InstallationId.ToString());
            }

            _restRoutinen = new RESTRoutinen(config);
        }

        protected virtual void OnErrorOccured(ApiErrorArgs e)
        {
            if (e.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new ApiUnauthorizedException(Status = e.Message);
            }
            ErrorOccured?.Invoke(this, e);
        }

        /// <summary>
        /// Meldet mit den aktuellen Credentials am Endpunkt /api/Login/Authenticate an. Wenn ein AuthToken vorhanden ist, wird
        /// zunächst versucht, dieses zu verwenden und ggf. zu updaten. Wenn das fehlschlägt, erfolgt eine Anmeldung mit Username/
        /// Passwort aus den Settings.
        /// </summary>
        /// <returns>Status des Logins</returns>
        public async Task<bool> LoginAsync()
        {
            if (IsJwt)
            {
                return await checkJwtTokenAsync();
            }

            try
            {
                UserAuthTokenDTO result = null;
                if (AuthToken == null || AuthToken.Expires < DateTime.UtcNow)
                {
                    if (!string.IsNullOrEmpty(Settings.UserName) && !string.IsNullOrEmpty(Settings.Passwort))
                    {
                        var ldto = new LoginDTO()
                        {
                            Email = Settings.UserName,
                            Password = Settings.Passwort,
                            AppToken = Settings.AppToken
                        };
                        result = await PostAsync<UserAuthTokenDTO>("/api/Login/Authenticate", ldto, null, true);
                    }
                }
                else
                {
                    if (AuthToken.Expires < DateTime.UtcNow.AddHours(6))
                    {
                        result = await RefreshTokenAsync(AuthToken.Token);
                    }
                    else
                    {
                        Status = "OK (Cached)";
                        return true;
                    }
                }

                if (result != null)
                {
                    AuthToken = result;
                    initRestRoutinen();
                    Status = "OK";
                    return true;
                }

                AuthToken = null;
                Status = "Error";
                return false;
            }
            catch (ApiException apiex)
            {
                Status = apiex.Message;
                if (Status.ToLower().Contains("<title>"))
                {
                    Status = internalStripHtml(Status);
                }

                if (apiex.InnerException != null)
                    Status += " - " + apiex.InnerException.Message;
                AuthToken = null;
                return false;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                AuthToken = null;
                return false;
            }
        }

        public async Task<UserAuthTokenDTO> RefreshTokenAsync(Guid authTokenGuid)
        {
            try
            {
                return await PutAsync<UserAuthTokenDTO>("/api/Login/Update", new UserAuthTokenDTO() { Token = authTokenGuid }, null, true);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<T> PostAsync<T>(string uri, object data, JsonSerializerSettings settings = null, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.PostAsync<T>(uri, data, settings);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri, data);
            }
        }

        public async Task PostAsync(string uri, object data, JsonSerializerSettings settings = null, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                await _restRoutinen.PostAsync(uri, data, settings);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri, data);
            }
        }

        public async Task<byte[]> PostDataAsync(string uri, byte[] data, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.PostDataAsync(uri, data);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri, data);
            }
        }

        public async Task<byte[]> GetDataAsync(string uri, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth); 
                return await _restRoutinen.GetDataAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri);
            }
        }

        public async Task<string> GetAsync(string uri, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.GetAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri);
            }
        }

        public async Task<T> GetAsync<T>(string uri, JsonSerializerSettings settings = null, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.GetAsync<T>(uri, settings);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri);
            }
        }

        public async Task PutAsync(string uri, object data, JsonSerializerSettings settings = null, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                await _restRoutinen.PutAsync(uri, data, settings);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri, data);
            }
        }

        public async Task<T> PutAsync<T>(string uri, object data, JsonSerializerSettings settings = null, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.PutAsync<T>(uri, data, settings);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri, data);
            }
        }

        public async Task<byte[]> PutDataAsync(string uri, byte[] data, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.PutDataAsync(uri, data);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri, data);
            }
        }

        public async Task DeleteAsync(string uri, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                await _restRoutinen.DeleteAsync(uri);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri);
            }
        }

        public async Task DeleteAsync(string uri, object data, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                await _restRoutinen.DeleteAsync(uri, data);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri);
            }
        }

        public async Task<T> DeleteAsync<T>(string uri, object data, bool skipAuth = false)
        {
            try
            {
                await runPreRequestChecks(skipAuth);
                return await _restRoutinen.DeleteAsync<T>(uri, data);
            }
            catch (HttpRequestException ex)
            {
                throw await HandleWebException(ex, uri);
            }
        }

        private static string internalStripHtml(string htmlString)
        {
            var result = htmlString;
            if (result.ToLower().Contains("<title>") && result.ToLower().Contains("</title>"))
            {
                var start = result.IndexOf("<title>") + 7;
                var end = result.IndexOf("</title>");
                if (end > start)
                    result = $"Interner Serverfehler (\"{result.Substring(start, end - start)}\"). Bitte versuchen Sie es zu einem späteren Zeitpunkt erneut.";
            }
            return result;
        }

        private async Task<bool> checkJwtTokenAsync()
        {
            if (internalCheckJwtToken(out var refreshToken, out bool checkResult))
            {
                return checkResult;
            }

            try
            {
                // refresh JWT using refresh token
                var newJwt = await PutAsync<string>("/api/LoginJwt/Refresh", new { Token = refreshToken }, null, true);
                JwtToken = newJwt;
                return true;
            }
            catch (ApiException apiex)
            {
                Status = apiex.Message;
                if (Status.ToLower().Contains("<title>"))
                {
                    Status = internalStripHtml(Status);
                }

                if (apiex.InnerException != null)
                {
                    Status += " - " + apiex.InnerException.Message;
                }

                JwtToken = null;
                return false;
            }
            catch (Exception ex)
            {
                Status = ex.Message;
                JwtToken = null;
                return false;
            }
        }

        private bool internalCheckJwtToken(out string refreshToken, out bool checkResult)
        {
            refreshToken = null;
            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(JwtToken))
            {
                // unreadable
                checkResult = false;
                return true;
            }

            var jwtToken = tokenHandler.ReadJwtToken(JwtToken);
            if (jwtToken.ValidTo >= DateTime.UtcNow)
            {
                // token is not expired
                checkResult = true;
                return true;
            }

            var tokenType = "Normal";
            var tokenTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "type");
            if (tokenTypeClaim != null)
            {
                tokenType = tokenTypeClaim.Value;
            }

            var refreshTokenClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "refreshToken");
            refreshToken = refreshTokenClaim?.Value;
            Guid.TryParse(refreshToken, out Guid refreshTokenGuid);

            // Service tokens can have refreshTokenGuid empty
            if (tokenType != "Service" &&
                refreshTokenClaim == null ||
                refreshTokenGuid == Guid.Empty)
            {
                // JWT is expired and has no refreshToken
                checkResult = false;
                return true;
            }

            // token is good - return "false" for further processing
            checkResult = true;
            return false;
        }

        private async Task<ApiException> HandleWebException(HttpRequestException ex, string url)
        {
            ApiException exception = await TranslateException(ex);
            return InternalHandleWebException(exception, url);
        }

        private async Task<ApiException> HandleWebException(HttpRequestException ex, string url, object data)
        {
            ApiException exception = await TranslateException(ex, data);
            return InternalHandleWebException(exception, url);
        }

        private ApiException InternalHandleWebException(ApiException exception, string url)
        {
            if (!IgnoreOnErrorOccured)
            {
                OnErrorOccured(new ApiErrorArgs(exception.Message, exception.StatusCode));
            }

            var foundUrlInData = false;
            object dataBaseUrl = null;
            object dataUrl = null;
            object dataCallMethod = null;

            // Check if we already have data from RESTRoutinen.AddInfoToException()
            if (!exception.Data.Contains("URL"))
            {
                var innerException = exception.InnerException;
                while (innerException != null)
                {
                    if (innerException.Data.Contains("URL"))
                    {
                        dataBaseUrl = innerException.Data["BaseUrl"];
                        dataUrl = innerException.Data["URL"];
                        dataCallMethod = innerException.Data["CallMethod"];

                        foundUrlInData = true;
                    }

                    innerException = innerException.InnerException;
                }
            }
            else
            {
                dataBaseUrl = exception.Data["BaseUrl"];
                dataUrl = exception.Data["URL"];
                dataCallMethod = exception.Data["CallMethod"];

                foundUrlInData = true;
            }

            if (!foundUrlInData)
            {
                var callerMethodName = new StackTrace().GetFrame(2)?.GetMethod()?.Name;

                exception.Data.Add("BaseUrl", Settings.Url);
                exception.Data.Add("URL", url);
                exception.Data.Add("CallMethod", callerMethodName);

                dataBaseUrl = exception.Data["BaseUrl"];
                dataUrl = exception.Data["URL"];
                dataCallMethod = exception.Data["CallMethod"];
            }

            L.Fehler(exception, $"Exception data: BaseUrl: {dataBaseUrl} URL: {dataUrl} CallMethod: {dataCallMethod}{Environment.NewLine}");

            return exception;
        }

        protected static async Task<ApiException> TranslateException(HttpRequestException ex, object payload)
        {
            if (ex.Data.Contains("Response"))
            {
                HttpStatusCode code = (HttpStatusCode)ex.Data["StatusCode"];
                var responseString = (string)ex.Data["Response"];

                if (!string.IsNullOrWhiteSpace(responseString))
                {
                    // Newtonsoft TypeNameInfo - try to restore original exception from Backend
                    if (responseString.Contains("$type"))
                    {
                        try
                        {
                            Exception original = JsonConvert.DeserializeObject<Exception>(responseString, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                            return new ApiException(original.Message, code, original, payload);
                        }
                        catch { }
                    }

                    try
                    {
                        dynamic infoObject = JsonConvert.DeserializeObject<dynamic>(responseString);
                        string status = infoObject.status;
                        return new ApiException(status, code, payload) { ExceptionString = infoObject.exception.ToString() };
                    }
                    catch
                    {
                        return new ApiException(responseString, code, payload);
                    }
                }

                return new ApiException(ex.Message, code, ex, payload);
            }

            return new ApiException(ex.Message, ex, payload);
        }

        protected static async Task<ApiException> TranslateException(HttpRequestException ex)
        {
            if (ex.Data.Contains("Response"))
            {
                HttpStatusCode code = (HttpStatusCode)ex.Data["StatusCode"];
                var response = (string)ex.Data["Response"];

                if (!string.IsNullOrWhiteSpace(response))
                {
                    try
                    {
                        Exception original = JsonConvert.DeserializeObject<Exception>(response, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All });
                        return new ApiException(original.Message, code, original);
                    }
                    catch
                    {
                        try
                        {
                            dynamic infoObject = JsonConvert.DeserializeObject<dynamic>(response);
                            string status = infoObject.status;
                            return new ApiException(status, code) { ExceptionString = infoObject.exception.ToString() };
                        }
                        catch
                        {
                            return new ApiException(response, code);
                        }
                    }
                }

                return new ApiException(ex.Message, code, ex);
            }

            return new ApiException(ex.Message, ex);
        }
    }
}
