using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projectfiets.Data;
using projectfiets.Migrations;
using projectfiets.Models;
using System.Diagnostics;

namespace projectfiets.Controllers
{
    public class HomeController : Controller
    {
        private readonly FietsContext _context;

        public HomeController(FietsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ViewResult Index()
        {
            HttpContext.Session.SetString("Login", "false");
            return View();
        }

        [HttpPost]
        public ViewResult Index(Gebruiker customerView)
        {
            var email = _context.Gebruikers.Select(c => c.Email).ToList();
            var passwoordHashes = _context.Gebruikers.Select(p => p.Password).ToList();

            for (int i = 0; i < email.Count(); i++)
            {
                if (customerView.Email == email[i])
                {
                    if (PasswordHasher.VerifyPassword(customerView.Password, passwoordHashes[i]))
                    {
                        var isConfirmed = _context.Gebruikers.Select(ic => ic.Confirmed).ToList();
                        var isAdmin = _context.Gebruikers.Select(ia => ia.Admin).ToList();

                        if (isConfirmed[i] && !isAdmin[i])
                        {
                            var fietsen = _context.Fietsen.ToList();

                            var model = new OrderViewModel
                            {
                                Fietsen = fietsen,
                                FietsQuantities = fietsen.ToDictionary(f => f.ID, f => 0),
                                Admin = "false"
                            };

                            HttpContext.Session.SetString("Email", customerView.Email);
                            HttpContext.Session.SetInt32("ID", _context.Gebruikers.Where(c => c.Email == customerView.Email).First().ID);
                            HttpContext.Session.SetString("Admin", "false");
                            HttpContext.Session.SetString("Login", "true");

                            var InvalidFilepaths = new List<string>();

                            foreach (var fiets in fietsen)
                            {
                                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fiets.ImageUrl);
                                if (!System.IO.File.Exists(filePath))
                                {
                                    InvalidFilepaths.Add(fiets.Naam);
                                }
                            }

                            ViewBag.InvalidFilepaths = InvalidFilepaths;
                            return View("Order", model);
                        }
                        else if (isConfirmed[i] && isAdmin[i])
                        {
                            var emailView = _context.Gebruikers.ToList();
                            HttpContext.Session.SetString("Email", customerView.Email);
                            HttpContext.Session.SetInt32("ID", _context.Gebruikers.Where(c => c.Email == customerView.Email).First().ID);
                            HttpContext.Session.SetString("Admin", "true");
                            HttpContext.Session.SetString("Login", "true");
                            return View("Admin", emailView);
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("Login", "false");
                    }
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public ViewResult Registratie()
        {
            return View();
        }

        [HttpPost]
        public ViewResult Registratie(Gebruiker customerView)
        {
            if (!(customerView.Email == null) && !(customerView.Password == null))
            {
                var gebruikersnamen = _context.Gebruikers.Select(n => n.Email).ToList();
                if (!gebruikersnamen.Contains(customerView.Email))
                {
                    var customer = new Gebruiker
                    {
                        Email = customerView.Email,
                        Password = PasswordHasher.HashPassword(customerView.Password),
                        Confirmed = false,
                        Admin = false
                    };
                    _context.Gebruikers.Add(customer);
                    _context.SaveChanges();

                    var admin = HttpContext.Session.GetString("Admin");
                    return View("Index");
                }
                else
                {
                    ViewBag.Registratie = "Gebruikersnaam gebruikt!";
                    return View();
                }
            }
            return View();
        }

        public ViewResult Order()
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            if (LOGIN == "true")
            {
                var fietsen = _context.Fietsen.ToList();

                var admin = HttpContext.Session.GetString("Admin");
                var model = new OrderViewModel { Fietsen = fietsen, Admin = admin };

                model.FietsQuantities = fietsen.ToDictionary(f => f.ID, f => 0);

                var InvalidFilepaths = new List<string>();
                foreach (var fiets in fietsen)
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fiets.ImageUrl);
                    if (!System.IO.File.Exists(filePath))
                    {
                        InvalidFilepaths.Add(fiets.Naam);
                    }
                }
                ViewBag.InvalidFilepaths = InvalidFilepaths;

                return View(model);
            }
            else
            {
                return View("Index");
            }
        }

        public ViewResult PlaceOrder(OrderViewModel orderViewModel)
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var admin = HttpContext.Session.GetString("Admin");

            if ((LOGIN == "true") && (orderViewModel.FietsQuantities != null))
            {
                var ID = Convert.ToInt16(HttpContext.Session.GetInt32("ID"));
                var order = new Order
                {
                    CustomerID = ID,
                    CreateOrder = DateTime.Now,
                    price = 0,
                    OrderItems = new List<OrderItem>()
                };

                foreach (var itemfiets in _context.Fietsen)
                {
                    if (orderViewModel.FietsQuantities[itemfiets.ID] > 0)
                    {
                        var OrderItem = new OrderItem
                        {
                            Fiets = itemfiets,
                            Quantity = orderViewModel.FietsQuantities[itemfiets.ID],
                            Order = order
                        };
                        order.OrderItems.Add(OrderItem);
                    }
                }

                _context.SaveChanges();

                order.price = order.OrderItems.Sum(oi => oi.Fiets.Price * oi.Quantity);
                _context.Orders.Add(order);
                _context.SaveChanges();

                var orderFromDb = _context.Orders
                    .Where(o => o.CustomerID == ID)
                    .Include(o => o.OrderItems)
                    .ThenInclude(o => o.Fiets)
                    .OrderByDescending(o => o.CreateOrder)
                    .First();

                var naam = HttpContext.Session.GetString("Email");
                var model = new PlaceOrderViewModel { Order = orderFromDb, Naam = naam, Admin = admin };
                return View(model);
            }
            else
            {
                return View("Index");
            }
        }

        public ViewResult ListOrders()
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var ADMIN = HttpContext.Session.GetString("Admin");

            if ((LOGIN == "true") && (ADMIN == "true"))
            {
                if (_context.OrderItems.Any())
                {
                    var orderItems = _context.OrderItems
                        .Include(oi => oi.Order)
                        .Include(oi => oi.Fiets)
                        .Include(o => o.Order.OrderItems)
                        .ToList();

                    return View(orderItems);
                }
                else
                {
                    return View();
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet]
        public ViewResult Admin(Gebruiker customer, int id, bool goedkeuring)
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var ADMIN = HttpContext.Session.GetString("Admin");
            if ((LOGIN == "true") && (ADMIN == "true"))
            {
                var gebruikerID = _context.Gebruikers.Select(u => u.ID).ToList();

                for (int i = 0; i < gebruikerID.Count; i++)
                {
                    if (gebruikerID[i] == id)
                    {
                        var gebruiker = _context.Gebruikers.Find(id);

                        if (gebruiker != null)
                        {
                            if (!gebruiker.Admin && gebruiker.Confirmed)
                            {
                                gebruiker.Admin = true;
                            }
                            else if (gebruiker.Confirmed && gebruiker.Admin)
                            {
                                gebruiker.Admin = false;
                            }
                            else if (!gebruiker.Confirmed && !gebruiker.Admin && goedkeuring)
                            {
                                gebruiker.Confirmed = true;
                            }
                            else if (!gebruiker.Confirmed && !gebruiker.Admin && !goedkeuring)
                            {
                                _context.Gebruikers.Remove(gebruiker);
                            }
                        }
                        _context.SaveChanges();
                    }
                }
                var emails = _context.Gebruikers.ToList();
                return View(emails);
            }
            else
            {
                return View("Index");
            }
        }

        public ViewResult PersoonlijkeOrders()
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            if (LOGIN == "true")
            {
                var ID = Convert.ToInt16(HttpContext.Session.GetInt32("ID"));
                var orders = _context.Orders
                    .Where(o => o.CustomerID == ID)
                    .Include(o => o.OrderItems)
                    .ThenInclude(o => o.Fiets)
                    .ToList();
                return View(orders);
            }
            else 
            { 
                return View("Index"); 
            }
        }

        [HttpGet]
        public ViewResult Aanpassen(int id)
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var ADMIN = HttpContext.Session.GetString("Admin");
            if ((LOGIN == "true") && (ADMIN == "true") && (id != 0))
            {
                var fiets = _context.Fietsen
                    .FirstOrDefault(f => f.ID == id);

                return View(fiets);
            }
            else { return View("Index"); }
        }

        [HttpPost]
        public IActionResult Aanpassen(fiets fiets)
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var ADMIN = HttpContext.Session.GetString("Admin");

            if ((LOGIN == "true") && (ADMIN == "true"))
            {
                var fietsToUpdate = _context.Fietsen.Find(fiets.ID);

                if (fietsToUpdate != null)
                {
                    fietsToUpdate.Price = fiets.Price;
                    _context.SaveChanges();
                }

                return RedirectToAction("Aanpassen", new { id = fiets.ID });
            }
            else
            {
                return View("Index");
            }
        }

        public IActionResult SamenvattingOrders()
        {
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Fiets)
                .ToList();

            var orderSummary = new List<OrderSummaryViewModel>();

            foreach (var order in orders)
            {
                foreach (var orderItem in order.OrderItems)
                {
                    var existingItem = orderSummary.FirstOrDefault(os => os.FietsModel == orderItem.Fiets.Naam);
                    if (existingItem != null)
                    {
                        existingItem.Aantal += orderItem.Quantity;
                        existingItem.TotalePrijs += orderItem.Quantity * orderItem.Fiets.Price;
                    }
                    else
                    {
                        orderSummary.Add(new OrderSummaryViewModel
                        {
                            FietsModel = orderItem.Fiets.Naam,
                            Aantal = orderItem.Quantity,
                            TotalePrijs = orderItem.Quantity * orderItem.Fiets.Price
                        });
                    }
                }
            }

            return View(orderSummary);
        }


        public IActionResult AanpassenOrder(int id)
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var ADMIN = HttpContext.Session.GetString("Admin");
            if ((LOGIN == "true") && (ADMIN == "true"))
            {
                var order = _context.Orders
                    .Include(o => o.OrderItems)
                        .ThenInclude(oi => oi.Fiets)
                    .FirstOrDefault(o => o.ID == id);

                if (order != null)
                {
                    return View(order);
                }
                else
                {
                    ViewBag.error = "Order niet gevonden.";
                    return View();
                }
            }
            else
            {
                return View("AanpassenOrder");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AanpassenOrder(int id, decimal price)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.ID == id);

            if (order == null)
            {
                ViewBag.error = "Order niet gevonden.";
                return View();
            }

            order.price = price;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(AanpassenOrder));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.ID == id);
        }

        public ViewResult Verwijderen(List<int> gebruikers)
        {
            var LOGIN = HttpContext.Session.GetString("Login");
            var ADMIN = HttpContext.Session.GetString("Admin");
            if ((LOGIN == "true") && (ADMIN == "true"))
            {
                int i = 0;
                foreach (var gebruikerId in gebruikers)
                {
                    var customer = _context.Gebruikers
                        .Include(c => c.Orders)
                        .FirstOrDefault(c => c.ID == gebruikerId);

                    if (customer != null)
                    {
                        _context.Orders.RemoveRange(customer.Orders);
                        _context.SaveChanges();
                    }
                    i++;
                }

                var namen = _context.Gebruikers
                    .Include(c => c.Orders)
                    .ThenInclude(o => o.OrderItems)
                    .ThenInclude(oi => oi.Fiets)
                    .ToList();
                return View("ListOrders", namen);
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult VerwijderenOrder(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.ID == orderId);

            if (order != null)
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
                _context.Orders.Remove(order);
                _context.SaveChanges();
            }

            return RedirectToAction("listOrders");
        }

        public ViewResult VerwijderenOrder(List<int> Orders, int gebruiker)
        {
            foreach (var orderId in Orders)
            {
                var bestelling = _context.Orders.Find(orderId);
                if (bestelling != null)
                {
                    _context.Orders.Remove(bestelling);
                }
            }
            _context.SaveChanges();

            var customer = _context.Gebruikers
                .Include(c => c.Orders)
                .ThenInclude(o => o.OrderItems)
                .ThenInclude(oi => oi.Fiets)
                .FirstOrDefault(c => c.ID == gebruiker);

            return View("ListOrders", customer);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
