using lapshop.Bl;
using lapshop.Models;
using lapshop.Domains;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace lapshop.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        IItems oItem;
        LapShopContext _context;
        public ItemsController(IItems iitem, LapShopContext context)
        {
            oItem = iitem;
            _context = context;
        }
        //GET: api/<ItemsController>
        /// <summary>
        /// get all items from database
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ApiResponse Get()
        {
            ApiResponse oApiResponse = new ApiResponse();
            oApiResponse.Data = oItem.GetAll();
            oApiResponse.Errors = null;
            oApiResponse.StatusCode = "200";
            return oApiResponse;
        }

        [HttpGet("GetPaged")]
        public ApiResponse GetPaged(int page = 1, int pageSize = 24, int categoryId = 0, string search = "", string ramSizes = "", string brands = "")
        {
            ApiResponse oApiResponse = new ApiResponse();
            try
            {
                var query = oItem.GetAllItemsData(categoryId);

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(a => a.ItemName.ToLower().Contains(searchLower) || (a.Description != null && a.Description.ToLower().Contains(searchLower))).ToList();
                }

                if (!string.IsNullOrEmpty(ramSizes))
                {
                    var rams = ramSizes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    query = query.Where(a => rams.Contains(a.RamSize ?? 8)).ToList();
                }

                if (!string.IsNullOrEmpty(brands))
                {
                    var brandIds = brands.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                    query = query.Where(a => brandIds.Contains(a.CategoryId)).ToList();
                }

                var totalCount = query.Count();
                var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                oApiResponse.Data = new { items = items, totalCount = totalCount };
                oApiResponse.Errors = null;
                oApiResponse.StatusCode = "200";
            }
            catch (Exception ex)
            {
                oApiResponse.Data = null;
                oApiResponse.Errors = ex.Message;
                oApiResponse.StatusCode = "502";
            }
            return oApiResponse;
        }

        [HttpGet("PopulateProductsData")]
        public ApiResponse PopulateProductsData()
        {
            ApiResponse oApiResponse = new ApiResponse();
            try
            {
                int updatedCount = 0;
                var dbItems = _context.TbItems.ToList();
                
                foreach (var item in dbItems)
                {
                    bool modified = false;

                    // 1. Assign image based on brand/type
                    if (string.IsNullOrEmpty(item.ImageName) || item.ImageName == "silver_ultrabook.png" || item.ImageName.EndsWith(".jpg"))
                    {
                        if (item.CategoryId == 1) // Apple
                        {
                            item.ImageName = "apple_macbook.png";
                            modified = true;
                        }
                        else if (item.CategoryId == 4 || item.CategoryId == 8 || item.CategoryId == 14) // ASUS, MSI, Razer
                        {
                            item.ImageName = "gaming_laptop.png";
                            modified = true;
                        }
                        else if (item.CategoryId == 5) // Dell
                        {
                            item.ImageName = "dark_laptop.png";
                            modified = true;
                        }
                        else // HP, Lenovo, Acer, etc.
                        {
                            item.ImageName = (item.ItemId % 2 == 0) ? "silver_ultrabook.png" : "dark_laptop.png";
                            modified = true;
                        }
                    }

                    // 2. Generate professional description if empty
                    if (string.IsNullOrEmpty(item.Description) || item.Description.Length < 10)
                    {
                        var brand = _context.TbCategories.FirstOrDefault(c => c.CategoryId == item.CategoryId)?.CategoryName ?? "Premium";
                        var processor = string.IsNullOrEmpty(item.Processor) ? "Intel/AMD" : item.Processor;
                        var ram = item.RamSize.HasValue ? item.RamSize.Value.ToString() : "8";
                        var hdd = string.IsNullOrEmpty(item.HardDisk) ? "high-speed SSD" : item.HardDisk;
                        var gpu = string.IsNullOrEmpty(item.Gpu) ? "Intel Iris Xe Graphics" : item.Gpu;
                        var screen = string.IsNullOrEmpty(item.ScreenSize) ? "15.6-inch" : item.ScreenSize;

                        item.Description = $"This premium {brand} {item.ItemName} is a powerful laptop engineered for modern computing. It is powered by a high-performance {processor} processor, equipped with {ram}GB of RAM, and features {hdd} storage. Graphics are handled by {gpu}, displaying beautiful visuals on its {screen} screen. Perfect for developers, creators, and business professionals seeking reliability and speed on the go.";
                        modified = true;
                    }

                    if (modified)
                    {
                        _context.Entry(item).State = EntityState.Modified;
                        updatedCount++;
                    }
                }

                if (updatedCount > 0)
                {
                    _context.SaveChanges();
                }

                oApiResponse.Data = new { message = $"Successfully updated {updatedCount} products with real images and descriptions." };
                oApiResponse.Errors = null;
                oApiResponse.StatusCode = "200";
            }
            catch (Exception ex)
            {
                oApiResponse.Data = null;
                oApiResponse.Errors = ex.Message;
                oApiResponse.StatusCode = "502";
            }
            return oApiResponse;
        }

        // GET api/<ItemsController>/5
        /// <summary>
        /// get item by id
        /// </summary>
        /// <param name="id">item id</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public ApiResponse Get(int id)
        {
            ApiResponse oApiResponse = new ApiResponse();
            oApiResponse.Data = oItem.GetById(id);
            oApiResponse.Errors = null;
            oApiResponse.StatusCode = "200";
            return oApiResponse;
        }

        //[HttpGet("{id}")]
        //public IActionResult Get(int id)
        //{
        //    ApiResponse oApiResponse = new ApiResponse();
        //    oApiResponse.Data = oItem.GetById(id);
        //    oApiResponse.Errors = null;
        //    oApiResponse.StatusCode = "200";
        //    return Ok(new object() { });
        //}

        // GET api/<ItemsController>/5
        [HttpGet("GetByCategoryId/{categoryId}")]
        public ApiResponse GetByCategoryId(int categoryId)
        {
            ApiResponse oApiResponse = new ApiResponse();
            oApiResponse.Data = oItem.GetAllItemsData(categoryId);
            oApiResponse.Errors = null;
            oApiResponse.StatusCode = "200";
            return oApiResponse;
        }

        // POST api/<ItemsController>
        [HttpPost]
        public ApiResponse Post([FromBody] TbItem item)
        {
            try
            {
                oItem.Save(item);
                ApiResponse oApiResponse = new ApiResponse();
                oApiResponse.Data = "done";
                oApiResponse.Errors = null;
                oApiResponse.StatusCode = "200";
                return oApiResponse;
            }
            catch (Exception ex)
            {
                ApiResponse oApiResponse = new ApiResponse();
                oApiResponse.Data = null;
                oApiResponse.Errors = ex.Message;
                oApiResponse.StatusCode = "502";
                return oApiResponse;
            }
        }

        [HttpPost]
        [Route("Delete")]
        public void Delete([FromBody] int id)
        {
            oItem.Dekete(id);
        }
    }
}
