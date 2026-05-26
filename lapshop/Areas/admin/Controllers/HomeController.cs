using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using lapshop.Bl;
using lapshop.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lapshop.Areas.admin.Controllers
{
    [Area("admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly LapShopContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(LapShopContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var sevenDaysAgo = DateTime.Now.AddDays(-7);

            // 1. حساب عدد طلبات آخر 7 أيام
            var weeklyOrdersCount = await _context.TbSalesInvoices
                .Where(i => i.InvoiceDate >= sevenDaysAgo)
                .CountAsync();

            // 2. حساب إجمالي أرباح آخر 7 أيام من جدول الـ Items المربوط بالفاتورة
            var weeklySalesRevenue = await _context.TbSalesInvoiceItems
                .Where(ii => _context.TbSalesInvoices.Any(i => i.InvoiceId == ii.InvoiceId && i.InvoiceDate >= sevenDaysAgo))
                .SumAsync(ii => (decimal)ii.Qty * ii.InvoicePrice);

            // 3. إجمالي عدد المستخدمين المسجلين
            var totalUsersCount = await _userManager.Users.CountAsync();

            // 4. جلب آخر 5 طلبات (Recent Orders) لشاشه الـ Dashboard الرئيسية
            var recentOrders = await _context.TbSalesInvoices
                .OrderByDescending(i => i.InvoiceDate)
                .Take(5)
                .Select(i => new RecentOrderViewModel
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceDate = i.InvoiceDate,
                    CustomerName = _context.Users.Where(u => u.Id == i.CustomerId)
                                                 .Select(u => u.FirstName + " " + u.LastName)
                                                 .FirstOrDefault() ?? "Guest Customer",
                    Total = _context.TbSalesInvoiceItems.Where(ii => ii.InvoiceId == i.InvoiceId)
                                                        .Sum(ii => (decimal)ii.Qty * ii.InvoicePrice),
                    Status = "Completed"
                })
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                WeeklyOrders = weeklyOrdersCount,
                WeeklySales = weeklySalesRevenue,
                TotalUsers = totalUsersCount,
                RecentOrders = recentOrders
            };

            return View(viewModel);
        }

        // الـ API الخاص برسم الـ Charts بالبيانات الحقيقية
        [HttpGet]
        public async Task<IActionResult> GetSalesStats()
        {
            // جلب أرباح كل فئة (Category) للـ Pie Chart
            var categoryStats = await _context.TbSalesInvoiceItems
                .GroupBy(ii => ii.Item.CategoryId)
                .Select(g => new
                {
                    CategoryName = _context.TbCategories.Where(c => c.CategoryId == g.Key).Select(c => c.CategoryName).FirstOrDefault() ?? "Other",
                    Revenue = g.Sum(ii => (decimal)ii.Qty * ii.InvoicePrice)
                })
                .ToListAsync();

            // جلب المبيعات الشهرية لآخر 6 أشهر للـ Line Chart
            var monthlyStats = await _context.TbSalesInvoices
                .Where(i => i.InvoiceDate >= DateTime.Now.AddMonths(-6))
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                .Select(g => new
                {
                    Month = g.Key.Year + "-" + g.Key.Month.ToString("D2"),
                    Revenue = _context.TbSalesInvoiceItems
                        .Where(ii => _context.TbSalesInvoices.Any(si => si.InvoiceId == ii.InvoiceId && si.InvoiceDate.Year == g.Key.Year && si.InvoiceDate.Month == g.Key.Month))
                        .Sum(ii => (decimal)ii.Qty * ii.InvoicePrice)
                })
                .ToListAsync();

            return Json(new { categories = categoryStats, monthly = monthlyStats });
        }
    }

    public class AdminDashboardViewModel
    {
        public int WeeklyOrders { get; set; }
        public decimal WeeklySales { get; set; }
        public int TotalUsers { get; set; }
        public List<RecentOrderViewModel> RecentOrders { get; set; } = new List<RecentOrderViewModel>();
    }

    public class RecentOrderViewModel
    {
        public int InvoiceId { get; set; }
        public string CustomerName { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
    }
}