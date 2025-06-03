using Microsoft.AspNetCore.Mvc;

namespace NZWalks.UI.Controllers
{
    public class BaseController : Controller
    {
        protected string GetJwtTokenFromCookies()
        {
            return Request.Cookies["jwtToken"]; // Replace "jwtToken" if have a different cookie name
        }
    }
}
