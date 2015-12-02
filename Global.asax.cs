using Elmah;
using Elmah.Contrib.WebApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ElmahTests
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            GlobalConfiguration.Configuration.Filters.Add(new ElmahHandleErrorApiAttribute());
        }

        #region Elmah Error Filtering
        // full description of error handling: http://code.google.com/p/elmah/wiki/ErrorFiltering
        // customizing error email (HttpContext is only available if email errorLog tag async attribute is set to false): http://scottonwriting.net/sowblog/archive/2011/01/06/customizing-elmah-s-error-emails.aspx
        // to fully customize sql error, we can override the Log method in Elmah.SqlErrorLog (e.g. http://www.tigraine.at/2009/11/22/writing-elmah-exceptions-to-log4net/)

        void ErrorLog_Filtering(object sender, ExceptionFilterEventArgs args)
        {
            Filter(args);
        }

        void ErrorMail_Filtering(object sender, ExceptionFilterEventArgs args)
        {
            if (args.Exception is ArgumentException)
            {
                args.Dismiss();
                return;
            }

            var context = (HttpContext)args.Context;
            if (context != null && context.Request.UserAgent != null)
            {

                if (context.Request.UserAgent == "Test Certificate Info" && args.Exception is System.Web.HttpException)
                {
                    args.Dismiss();
                    return;
                }

                if (context.Request.UserAgent.Contains("ZmEu"))
                {
                    args.Dismiss();
                    return;
                }

                if (context.Request.UserAgent.Contains("Googlebot"))
                {
                    args.Dismiss();
                    return;
                }
            }

            Filter(args);
        }

        void Filter(ExceptionFilterEventArgs args)
        {
            HttpContext context = (HttpContext)args.Context;

            // exclude crawlers
            if (context.Request.UserAgent != null)
            {

                if (Regex.IsMatch(context.Request.UserAgent, @"bingbot"))
                {
                    args.Dismiss();
                    return;
                }
            }

            if (args.Exception.GetBaseException() is HttpException)
            {
                HttpException ex = (HttpException)args.Exception.GetBaseException();
                if (ex.GetHttpCode() == 404)
                {
                    args.Dismiss();
                    return;
                }
            }
        }

        void ErrorMail_Mailing(object sender, ErrorMailEventArgs e)
        {

            // if async is set to true, this.Context will be null
            if (this.Context == null)
                return;

            if (this.Context.Request == null)
                return;

            // change email subject in certain conditions
            if (String.Equals(this.Context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase))
                e.Mail.Subject = "Ajax Error";

            if (this.Context.Request.InputStream == null)
                return;

            if (!this.Context.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return;

            this.Context.Request.InputStream.Seek(0, SeekOrigin.Begin);
            var sr = new StreamReader(this.Context.Request.InputStream);
            var json = sr.ReadToEnd(); // will be empty string if InputStream was disposed
            var html = String.Format("<p>JSON:</p><pre style=\"background-color:#eee;\">{0}</pre>", json);

            e.Mail.Body = e.Mail.Body.Insert(e.Mail.Body.IndexOf("<pre id=\"errorDetail\">"), html);

        }

        #endregion
    }
}
