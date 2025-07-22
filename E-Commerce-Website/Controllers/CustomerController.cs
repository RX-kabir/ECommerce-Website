using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Website.Controllers
{
    public class CustomerController : Controller
    {
        private myContext _context;
        private IWebHostEnvironment _env;

        public CustomerController(myContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                ViewData["product"] = _context.tbl_product.ToList();
                ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.error = "An error occurred while loading homepage: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult customerLogin() => View();

        [HttpPost]
        public IActionResult customerLogin(string customerEmail, string customerPassword)
        {
            try
            {
                var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_email == customerEmail);
                if (customer != null && customer.customer_password == customerPassword)
                {
                    HttpContext.Session.SetString("customerSession", customer.customer_id.ToString());
                    return RedirectToAction("Index");
                }

                ViewBag.message = "Incorrect Username or Password";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.message = "Login failed: " + ex.Message;
                return View();
            }
        }

        public IActionResult CustomerRegistration() => View();

        [HttpPost]
        public IActionResult CustomerRegistration(Customer customer)
        {
            try
            {
                _context.tbl_customer.Add(customer);
                _context.SaveChanges();
                return RedirectToAction("customerLogin");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Registration failed: " + ex.Message;
                return View();
            }
        }

        public IActionResult customerLogout()
        {
            try
            {
                HttpContext.Session.Remove("customerSession");
            }
            catch { }
            return RedirectToAction("Index");
        }

        public IActionResult customerProfile()
        {
            try
            {
                if (string.IsNullOrEmpty(HttpContext.Session.GetString("customerSession")))
                    return RedirectToAction("customerLogin");

                ViewData["category"] = _context.tbl_category.ToList();
                int customerId = int.Parse(HttpContext.Session.GetString("customerSession"));
                var row = _context.tbl_customer.Where(c => c.customer_id == customerId).ToList();
                return View(row);
            }
            catch (Exception ex)
            {
                ViewBag.message = "Profile loading error: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult updateCustomerProfile(Customer customer)
        {
            try
            {
                _context.tbl_customer.Update(customer);
                _context.SaveChanges();
                return RedirectToAction("customerProfile");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Update failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult changeProfileImage(Customer customer, IFormFile customer_image)
        {
            try
            {
                if (customer_image == null || string.IsNullOrEmpty(customer_image.FileName))
                    return RedirectToAction("customerProfile");

                string ImagePath = Path.Combine(_env.WebRootPath, "customer_images", customer_image.FileName);
                using (FileStream fs = new FileStream(ImagePath, FileMode.Create))
                {
                    customer_image.CopyTo(fs);
                }

                var existingCustomer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == customer.customer_id);
                if (existingCustomer != null)
                {
                    existingCustomer.customer_image = customer_image.FileName;
                    _context.SaveChanges();
                }

                return RedirectToAction("customerProfile");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Image update failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult feedback()
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.message = "Error loading feedback page: " + ex.Message;
                return View("Error");
            }
        }

        [HttpPost]
        public IActionResult feedback(Feedback feedback)
        {
            try
            {
                _context.tbl_feedback.Add(feedback);
                _context.SaveChanges();
                TempData["message"] = "Thank You For Your Feedback";
                return RedirectToAction("feedback");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Feedback submission failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult fetchAllProducts()
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                ViewData["product"] = _context.tbl_product.ToList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.message = "Could not fetch products: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult productDetails(int id)
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                var products = _context.tbl_product.Where(p => p.product_id == id).ToList();
                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.message = "Product details load failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult AddToCart(int prod_id, Cart cart)
        {
            try
            {
                string isLogin = HttpContext.Session.GetString("customerSession");
                if (isLogin != null)
                {
                    cart.product_id = prod_id;
                    cart.cust_id = int.Parse(isLogin);
                    cart.product_quantity = 1;
                    cart.cart_status = 0;

                    _context.tbl_cart.Add(cart);
                    _context.SaveChanges();

                    TempData["message"] = "Product Successfully added in Cart";
                    return RedirectToAction("fetchAllProducts");
                }
                else
                {
                    return RedirectToAction("customerLogin");
                }
            }
            catch (Exception ex)
            {
                ViewBag.message = "Add to cart failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult fetchCart()
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                string customerId = HttpContext.Session.GetString("customerSession");

                if (customerId != null)
                {
                    var cart = _context.tbl_cart
                        .Where(c => c.cust_id == int.Parse(customerId))
                        .Include(c => c.products)
                        .ToList();
                    return View(cart);
                }
                return RedirectToAction("customerLogin");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Cart loading failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult removeProduct(int id)
        {
            try
            {
                var product = _context.tbl_cart.Find(id);
                if (product != null)
                {
                    _context.tbl_cart.Remove(product);
                    _context.SaveChanges();
                }
                return RedirectToAction("fetchCart");
            }
            catch (Exception ex)
            {
                ViewBag.message = "Product removal failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult CheckOutAll()
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                string customerId = HttpContext.Session.GetString("customerSession");

                if (customerId == null)
                    return RedirectToAction("customerLogin");

                int custId = int.Parse(customerId);
                var cartItems = _context.tbl_cart.Include(c => c.products).Where(c => c.cust_id == custId).ToList();

                if (!cartItems.Any())
                    return RedirectToAction("fetchCart");

                var totalPrice = cartItems.Sum(c => decimal.Parse(c.products.product_price) * c.product_quantity);

                var order = new Order
                {
                    cust_id = custId,
                    total_price = totalPrice,
                    order_date = DateTime.Now
                };

                _context.Orders.Add(order);
                _context.tbl_cart.RemoveRange(cartItems);
                _context.SaveChanges();

                return View(order);
            }
            catch (Exception ex)
            {
                ViewBag.message = "Checkout failed: " + ex.Message;
                return View("Error");
            }
        }

        public IActionResult OrderSuccess()
        {
            try
            {
                ViewData["category"] = _context.tbl_category.ToList();
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.message = "Error loading order success page: " + ex.Message;
                return View("Error");
            }
        }
    }
}
