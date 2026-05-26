using lapshop.Bl;
using lapshop.Domains;
using lapshop.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace lapshop.Controllers
{
    public class OrderController : Controller
    {
        private IItems itemService;
        private UserManager<ApplicationUser> _userManager;
        private ISalesInvoice salesInvoiceService;

        public OrderController(IItems itemservice, UserManager<ApplicationUser> userManager, ISalesInvoice ssalesInvoiceService)
        {
            itemService = itemservice;
            _userManager = userManager;
            salesInvoiceService = ssalesInvoiceService;
        }
        public IActionResult Cart()
        {
            //-----------------------------------------------------------------------------------
            //string sesstionCart = string.Empty;
            //if (HttpContext.Request.Cookies["Cart"] != null)
            //    sesstionCart = HttpContext.Request.Cookies["Cart"];
            //var cart = JsonConvert.DeserializeObject<ShoppingCart>(sesstionCart);
            //return View(cart);
            //-----------------------------------------------------------------------------------
            //string sessionCart = string.Empty;
            string cookies = HttpContext.Request.Cookies["Cart"];
            var cart = new ShoppingCart(); // بنبدأ بسلة فاضية دايماً

            if (!string.IsNullOrEmpty(cookies))
            {
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
            }
            //if (HttpContext.Session.GetString("Cart") != null && HttpContext.Session.GetString("Cart") != "")
            //    sessionCart = HttpContext.Session.GetString("Cart");
            //var cart = JsonConvert.DeserializeObject<ShoppingCart>(CookiesCart);
            return View(cart);
        }

        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Users");
            
            var invoices = salesInvoiceService.GetAll()
                            .Where(a => a.CustomerId == Guid.Parse(user.Id))
                            .OrderByDescending(a => a.InvoiceDate)
                            .ToList();
            return View(invoices);
        }

        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Users");

            var invoice = salesInvoiceService.GetAll()
                            .FirstOrDefault(a => a.InvoiceId == id && a.CustomerId == Guid.Parse(user.Id));
            
            if (invoice == null)
            {
                return RedirectToAction("MyOrders");
            }

            var itemsService = HttpContext.RequestServices.GetRequiredService<ISalesInvoiceItems>();
            var items = itemsService.GetSalesInvoiceId(id);

            ViewBag.Invoice = invoice;
            return View(items);
        }

        [Authorize]
        public async Task<IActionResult> OrderSuccess()
        {
            string sesstionCart = string.Empty;
            if (HttpContext.Request.Cookies["Cart"] != null)
            {
                sesstionCart = HttpContext.Request.Cookies["Cart"];
                var cart = JsonConvert.DeserializeObject<ShoppingCart>(sesstionCart);
                if (cart != null && cart.lstItems.Count > 0)
                {
                    await SaveOrder(cart);
                    // Clear the cart cookie after successful order placement
                    HttpContext.Response.Cookies.Delete("Cart");
                    return View();
                }
            }
            return RedirectToAction("Cart");
        }

        public IActionResult AddToCart(int itemId)
        {
            //    ShoppingCart cart;

            //    if (HttpContext.Request.Cookies["Cart"] != null)
            //        cart = JsonConvert.DeserializeObject<ShoppingCart>(HttpContext.Request.Cookies["Cart"]);
            //    else
            //        cart = new ShoppingCart();

            //    var item = itemService.GetById(itemId);

            //    var itemInList = cart.lstItems.Where(a => a.ItemId == itemId).FirstOrDefault();

            //    if (itemInList != null)
            //    {
            //        itemInList.Qty++;
            //        itemInList.Total = itemInList.Qty * itemInList.Price;
            //    }
            //    else
            //    {
            //        cart.lstItems.Add(new ShoppingCartItem
            //        {
            //            ItemId = item.ItemId,
            //            ItemName = item.ItemName,
            //            Price = item.SalesPrice,
            //            Qty = 1,
            //            Total = item.SalesPrice
            //        });
            //    }
            //    cart.Total = cart.lstItems.Sum(a => a.Total);

            //    HttpContext.Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart));

            //    return RedirectToAction("Cart");
            //-----------------------------------------------------------------------------------
            ShoppingCart cart;
            String Cookies = string.Empty;
            Cookies = HttpContext.Request.Cookies["Cart"];
            if (Cookies != null)
                cart = JsonConvert.DeserializeObject<ShoppingCart>(Cookies);
            else
                cart = new ShoppingCart();
            
            var item = itemService.GetById(itemId);
            var itemInList = cart.lstItems.Where(a => a.ItemId == itemId).FirstOrDefault();

            if (itemInList != null)
            {
                itemInList.Qty++;
                itemInList.Total = itemInList.Qty * itemInList.Price;
            }
            else
            {
                cart.lstItems.Add(new ShoppingCartItem
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Price = item.SalesPrice,
                    ImageName = item.ImageName, // Populated ImageName
                    Qty = 1,
                    Total = item.SalesPrice
                });
            }
            cart.Total = cart.lstItems.Sum(a => a.Total);

            var josonconvertCookies = JsonConvert.SerializeObject(cart);
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = true
            };
            HttpContext.Response.Cookies.Append("Cart", josonconvertCookies, options);
            return RedirectToAction("Cart");
        }

        [HttpGet]
        public IActionResult AddToCartAjax(int itemId)
        {
            ShoppingCart cart;
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (cookies != null)
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
            else
                cart = new ShoppingCart();
            
            var item = itemService.GetById(itemId);
            var itemInList = cart.lstItems.Where(a => a.ItemId == itemId).FirstOrDefault();

            if (itemInList != null)
            {
                itemInList.Qty++;
                itemInList.Total = itemInList.Qty * itemInList.Price;
            }
            else
            {
                cart.lstItems.Add(new ShoppingCartItem
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Price = item.SalesPrice,
                    ImageName = item.ImageName,
                    Qty = 1,
                    Total = item.SalesPrice
                });
            }
            cart.Total = cart.lstItems.Sum(a => a.Total);

            var jsonCookies = JsonConvert.SerializeObject(cart);
            CookieOptions options = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                Secure = true
            };
            HttpContext.Response.Cookies.Append("Cart", jsonCookies, options);

            int totalQty = cart.lstItems.Sum(x => (int)x.Qty);

            return Json(new { success = true, cartCount = totalQty });
        }

        public IActionResult RemoveFromCart(int itemId)
        {
            ShoppingCart cart;
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (cookies != null)
            {
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
                var itemToRemove = cart.lstItems.FirstOrDefault(a => a.ItemId == itemId);
                if (itemToRemove != null)
                {
                    cart.lstItems.Remove(itemToRemove);
                    cart.Total = cart.lstItems.Sum(a => a.Total);
                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true
                    };
                    HttpContext.Response.Cookies.Append("Cart", JsonConvert.SerializeObject(cart), options);
                }
            }
            return RedirectToAction("Cart");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int itemId, int qty)
        {
            ShoppingCart cart;
            string cookies = HttpContext.Request.Cookies["Cart"];
            if (cookies != null)
            {
                cart = JsonConvert.DeserializeObject<ShoppingCart>(cookies);
                var item = cart.lstItems.FirstOrDefault(a => a.ItemId == itemId);
                if (item != null)
                {
                    item.Qty = qty;
                    item.Total = qty * item.Price;
                    cart.Total = cart.lstItems.Sum(a => a.Total);

                    var jsonCookies = JsonConvert.SerializeObject(cart);
                    CookieOptions options = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(7),
                        HttpOnly = true,
                        Secure = true
                    };
                    HttpContext.Response.Cookies.Append("Cart", jsonCookies, options);
                }
            }
            return Ok();
        }

        private async Task SaveOrder(ShoppingCart oShopingCart)
        {
            try
            {
                List<TbSalesInvoiceItem> lstInvoiceItems = new List<TbSalesInvoiceItem>();
                foreach (var item in oShopingCart.lstItems)
                {
                    lstInvoiceItems.Add(new TbSalesInvoiceItem()
                    {
                        ItemId = item.ItemId,
                        Qty = item.Qty,
                        InvoicePrice = item.Price
                    });
                }
                var user = await _userManager.GetUserAsync(User);
                TbSalesInvoice oSalesInvoice = new TbSalesInvoice()
                {
                    InvoiceDate = DateTime.Now,
                    CustomerId = Guid.Parse(user.Id),
                    DelivryDate = DateTime.Now.AddDays(5),
                    CreatedBy = user.Id,
                    CreatedDate = DateTime.Now
                };

                salesInvoiceService.Save(oSalesInvoice, lstInvoiceItems, true);
            }
            catch (Exception)
            {

            }
        }
    }
}
