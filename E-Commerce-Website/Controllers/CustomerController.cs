using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Website.Controllers
{
    public class CustomerController : Controller
    {
        private myContext _context;
        private IWebHostEnvironment _env;
        public CustomerController(myContext context,IWebHostEnvironment env) {
            _context = context;
            _env = env;
        }
        public IActionResult Index()
        {
            List<Category> category=_context.tbl_category.ToList();
            ViewData["category"] = category;

            List<Product> products = _context.tbl_product.ToList();
            ViewData["product"] = products;

            ViewBag.checkSession = HttpContext.Session.GetString("customerSession");
            return View();
        }

        public IActionResult customerLogin() {
            return View();

        }

        [HttpPost]

        public IActionResult customerLogin(String customerEmail,String customerPassword)
        {
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_email == customerEmail);
            if (customer != null && customer.customer_password == customerPassword)
            {
                HttpContext.Session.SetString("customerSession", customer.customer_id.ToString());
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = "Incorrect Username or Password";
                return View();
            }

        }

        public IActionResult CustomerRegistration()
        {
            return View();

        }

        [HttpPost]

        public IActionResult CustomerRegistration(Customer customer)
        {
            _context.tbl_customer.Add(customer);
            _context.SaveChanges();
            return RedirectToAction("customerLogin");
        }

        public IActionResult customerLogout()
        {
            HttpContext.Session.Remove("customerSession");
            return RedirectToAction("Index");
        }

        public IActionResult customerProfile()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("customerSession")))
            {
                return RedirectToAction("customerLogin");
            }

            else
            {
                List<Category> category = _context.tbl_category.ToList();
                ViewData["category"] = category;
                var customerId = HttpContext.Session.GetString("customerSession");
                var row = _context.tbl_customer.Where(c => c.customer_id == int.Parse(customerId)).ToList();
                return View(row);

            }
        }
        public IActionResult updateCustomerProfile(Customer customer)
        {
            _context.tbl_customer.Update(customer);
            _context.SaveChanges();
            return RedirectToAction("customerProfile");
        }

        public IActionResult changeProfileImage(Customer customer,IFormFile customer_image)
        {
            if (customer_image == null || string.IsNullOrEmpty(customer_image.FileName))
            {
                return RedirectToAction("Profile");
            }

            // Save image to wwwroot/admin_image
            string ImagePath = Path.Combine(_env.WebRootPath, "customer_images", customer_image.FileName);
            using (FileStream fs = new FileStream(ImagePath, FileMode.Create))
            {
                customer_image.CopyTo(fs);
            }

            // Get existing admin from DB
            var existingCustomer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == customer.customer_id);
            if (existingCustomer != null)
            {
                existingCustomer.customer_image = customer_image.FileName;
                _context.SaveChanges();
            }
            return RedirectToAction("customerProfile");
        }

        public IActionResult feedback()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            return View();
        }

        [HttpPost]

        public IActionResult feedback(Feedback feedback)
        {
            TempData["message"] = "Thank You For Your Feedback";
            _context.tbl_feedback.Add(feedback);
            _context.SaveChanges();
            return RedirectToAction("feedback");
        }

        public IActionResult fetchAllProducts()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            List<Product> products = _context.tbl_product.ToList();
            ViewData["product"] = products;
            return View();
        }


        public IActionResult productDetails(int id)
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            var products = _context.tbl_product.Where(p=>p.product_id == id).ToList();

            return View(products);
        }

        public IActionResult AddToCart(int prod_id,Cart cart)
        {
            string isLogin=HttpContext.Session.GetString("customerSession");
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

        public IActionResult fetchCart()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            string customerId = HttpContext.Session.GetString("customerSession");
            if (customerId != null)
            {
                var cart = _context.tbl_cart.Where(c => c.cust_id == int.Parse(customerId)).Include(c => c.products).ToList();
                return View(cart);
            }
            else
            {
                return RedirectToAction("customerLogin");
            }
        }

        public IActionResult removeProduct(int id)
        {
           var product =  _context.tbl_cart.Find(id);
            _context.tbl_cart.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("fetchcart");
        }


        public IActionResult CheckOutAll()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            string customerId = HttpContext.Session.GetString("customerSession");
            if (customerId == null)
                return RedirectToAction("customerLogin");

            int custId = int.Parse(customerId);

            var cartItems = _context.tbl_cart
                            .Include(c => c.products)
                            .Where(c => c.cust_id == custId)
                            .ToList();

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


        public IActionResult OrderSuccess()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            return View();
        }

    }
}

