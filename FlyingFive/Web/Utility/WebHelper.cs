using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace FlyingFive.Web.Utility
{
    /// <summary>
    /// Web应用程序助手
    /// </summary>
    public partial class WebHelper
    {
        private readonly string[] _staticFileExtensions = null;

        public WebHelper()
        {
            this._staticFileExtensions = new string[] { ".axd", ".bmp", ".css", ".gif", ".htm", ".html", ".ico", ".jpeg", ".jpg", ".js", ".png", ".rar", ".zip" };
            var staticResources = ConfigurationManager.AppSettings["StaticResources"];
            if (!string.IsNullOrEmpty(staticResources))
            {
                this._staticFileExtensions = staticResources.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// 当前是否请求为静态资源
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool IsStaticResource(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            string path = request.Path;
            string extension = VirtualPathUtility.GetExtension(path);

            if (extension == null) return false;

            return _staticFileExtensions.Contains(extension);
        }

        /// <summary>
        /// 重启Web站点，重新进入Application_Start而不重新创建w3wp进程
        /// </summary>
        /// <param name="makeRedirect"></param>
        /// <param name="redirectUrl"></param>
        public virtual void RestartAppDomain(bool makeRedirect = false, string redirectUrl = "")
        {
            if (GetTrustLevel() > AspNetHostingPermissionLevel.Medium)
            {
                HttpRuntime.UnloadAppDomain();
                TryWriteGlobalAsax();
            }
            else
            {
                bool success = TryWriteWebConfig();
                if (!success)
                {
                }
                success = TryWriteGlobalAsax();

                if (!success)
                {
                }
            }

            if (HttpContext.Current != null && makeRedirect && false)
            {
                if (String.IsNullOrEmpty(redirectUrl))
                    redirectUrl = GetThisPageUrl(true);
                HttpContext.Current.Response.Redirect(redirectUrl, true);
            }
        }

        protected virtual bool TryWriteWebConfig()
        {
            try
            {
                File.SetLastWriteTimeUtc(this.MapPath("~/web.config"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected virtual bool TryWriteGlobalAsax()
        {
            try
            {
                File.SetLastWriteTimeUtc(this.MapPath("~/global.asax"), DateTime.UtcNow);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取当前请求的Url地址
        /// </summary>
        /// <param name="includeQueryString">是否包含参数</param>
        /// <returns></returns>
        public virtual string GetThisPageUrl(bool includeQueryString)
        {
            bool useSsl = false;
            return GetThisPageUrl(includeQueryString, useSsl);
        }

        /// <summary>
        /// 获取当前请求的Url地址
        /// </summary>
        /// <param name="includeQueryString">是否包含参数</param>
        /// <param name="useSsl"></param>
        /// <returns></returns>
        public virtual string GetThisPageUrl(bool includeQueryString, bool useSsl)
        {
            string url = string.Empty;
            var _httpContext = HttpContext.Current;
            if (!IsRequestAvailable(_httpContext))
                return url;

            if (includeQueryString)
            {
                string storeHost = _httpContext.Request.Url.Host;
                if (storeHost.EndsWith("/"))
                    storeHost = storeHost.Substring(0, storeHost.Length - 1);
                url = storeHost + _httpContext.Request.RawUrl;
            }
            else
            {
                if (_httpContext.Request.Url != null)
                {
                    url = _httpContext.Request.Url.GetLeftPart(UriPartial.Path);
                }
            }
            url = url.ToLowerInvariant();
            return url;
        }

        /// <summary>
        /// Http上下文是否存在有效的请求
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        protected virtual Boolean IsRequestAvailable(HttpContext httpContext)
        {
            if (httpContext == null)
                return false;

            try
            {
                if (httpContext.Request == null)
                    return false;
            }
            catch (HttpException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 虚拟路径映射
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string MapPath(string path)
        {
            if (HostingEnvironment.IsHosted)
            {
                return HostingEnvironment.MapPath(path);
            }

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            path = path.Replace("~/", "").TrimStart('/').Replace('/', '\\');
            return Path.Combine(baseDirectory, path);
        }

        private static AspNetHostingPermissionLevel? _trustLevel = null;

        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!_trustLevel.HasValue)
            {
                //初始化为没有权限
                _trustLevel = AspNetHostingPermissionLevel.None;
                var levels = new[] {
                    AspNetHostingPermissionLevel.Unrestricted,
                    AspNetHostingPermissionLevel.High,
                    AspNetHostingPermissionLevel.Medium,
                    AspNetHostingPermissionLevel.Low,
                    AspNetHostingPermissionLevel.Minimal
                };
                //从高到低，依次判断
                foreach (AspNetHostingPermissionLevel trustLevel in levels)
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        _trustLevel = trustLevel;
                        break;
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return _trustLevel.Value;
        }
    }
}
