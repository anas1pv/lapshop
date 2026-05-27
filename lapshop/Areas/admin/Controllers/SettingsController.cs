using lapshop.Bl;
using lapshop.Domains;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly LapShopContext _context;

        public SettingsController(LapShopContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var settings = await _context.TbSettings.FirstOrDefaultAsync();
            if (settings == null) settings = new TbSettings(); // حماية في حال عدم وجود سجل
            return View(settings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TbSettings model)
        {
            if (!ModelState.IsValid) return View(model);

            _context.TbSettings.Update(model);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Settings updated successfully!";
            return RedirectToAction("Edit");
        }
    }
}