using Microsoft.AspNetCore.Mvc;

namespace lapshop.Areas.admin.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
