using sentTeamMessage.Models;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using sentTeamMessage.TokenStorage;
using System.Security.Claims;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OpenIdConnect;

namespace sentTeamMessage.Controllers
{

    public abstract class BaseController : Controller
    {

        protected void Flash(string message, string debug = null)
        {
            var alerts = TempData.ContainsKey(Alert.AlertKey) ?
                (List<Alert>)TempData[Alert.AlertKey] :
                new List<Alert>();

            alerts.Add(new Alert
            {
                Message = message,
                Debug = debug
            });

            TempData[Alert.AlertKey] = alerts;
        }

        
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var localHost = System.Configuration.ConfigurationManager.AppSettings["ida:RedirectUri"];
            var UrlParameters = RouteData.Values.GetEnumerator();
            UrlParameters.MoveNext();
            var sParam = UrlParameters.Current;

            while (sParam.Key != null && !sParam.Key.Equals("id"))
            {
                UrlParameters.MoveNext();
                sParam = UrlParameters.Current;
            }
            if (sParam.Key == "id")
            {
                Global.sValue = sParam.Value.ToString();
            }
            while (sParam.Key != null && !sParam.Key.Equals("MsgTxt"))
            {
                UrlParameters.MoveNext();
                sParam = UrlParameters.Current;
            }
            if (sParam.Key == "MsgTxt")
            {
                Global.sMsgValue = sParam.Value.ToString();
            }

            var UrlParamForRedirect = "";
            if (Global.sValue != null && !Global.sValue.Equals(""))
            {
                UrlParamForRedirect = "Home/Index/" + Global.sValue;
            }
            if (Global.sMsgValue != null && !Global.sMsgValue.Equals(""))
            {
                UrlParamForRedirect += "/" + Global.sMsgValue;
            }

            if (!Request.IsAuthenticated)
            {
                // Signal OWIN to send an authorization request to Azure
                Request.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = localHost + UrlParamForRedirect },
                    OpenIdConnectAuthenticationDefaults.AuthenticationType);

            }
            if (Request.IsAuthenticated)
            {

                // Get the user's token cache
                var tokenStore = new SessionTokenStore(null,
                    System.Web.HttpContext.Current, ClaimsPrincipal.Current);

                if (tokenStore.HasData())
                {
                    // Add the user to the view bag
                    ViewBag.User = tokenStore.GetUserDetails();
                }
                else
                {
                    // The session has lost data. This happens often
                    // when debugging. Log out so the user can log back in
                    Request.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                    filterContext.Result = RedirectToAction("Index", "Home");
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}