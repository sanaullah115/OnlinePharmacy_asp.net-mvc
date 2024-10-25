using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ProjectAdmin.Models;
namespace ProjectAdmin.Controllers
{
    public class HomeController : Controller
    {
        PharmacyEntities _db = new PharmacyEntities();
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.Product = _db.Products.ToList();
            return View();
        }
        [Authorize(Roles = "User")]
        public ActionResult AddToCart(int productId)
        {
            if (Session["cart"] == null)
            {
                List<Item> cart = new List<Item>();
                var product = _db.Products.Find(productId);
                cart.Add(new Item()
                {
                    product = product,
                    Quantity = 1
                });
                Session["cart"] = cart;
            }
            else
            {
                List<Item> cart = (List<Item>)Session["cart"];
                var count = cart.Count();
                var product = _db.Products.Find(productId);
                for (int i = 0; i < count; i++)
                {
                    if (cart[i].product.ProductId == productId)
                    {
                        int prevQty = cart[i].Quantity;
                        cart.Remove(cart[i]);
                        cart.Add(new Item()
                        {
                            product = product,
                            Quantity = prevQty + 1
                        });
                        break;
                    }
                    else
                    {
                        var prd = cart.Where(x => x.product.ProductId == productId).SingleOrDefault();
                        if (prd == null)
                        {
                            cart.Add(new Item()
                            {
                                product = product,
                                Quantity = 1
                            });
                        }
                    }
                }
                Session["cart"] = cart;
            }
            return Redirect("Index");
        }

        public ActionResult Checkout()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Checkout(string PhoneNumber, string CreditCardNumber, string PinCode)
        {
            Session["Number"] = PhoneNumber;
            Session["CdNumber"] = CreditCardNumber;
            Session["Pincode"] = PinCode;
            return RedirectToAction("CheckoutDetails", "Home");
        }
        public ActionResult DecreaseQty(int productId)
        {
            if (Session["cart"] != null)
            {
                List<Item> cart = (List<Item>)Session["cart"];
                var product = _db.Products.Find(productId);
                foreach (var item in cart)
                {
                    if (item.product.ProductId == productId)
                    {
                        int prevQty = item.Quantity;
                        if (prevQty > 0)
                        {
                            cart.Remove(item);
                            cart.Add(new Item()
                            {
                                product = product,
                                Quantity = prevQty - 1
                            });
                        }
                        break;
                    }
                }
                Session["cart"] = cart;
            }
            return Redirect("Checkout");
        }

        public ActionResult IncreaseQty(int productId)
        {
            if (Session["cart"] != null)
            {
                List<Item> cart = (List<Item>)Session["cart"];
                var product = _db.Products.Find(productId);
                foreach (var item in cart)
                {
                    if (item.product.ProductId == productId)
                    {
                        int prevQty = item.Quantity;
                        if (prevQty > 0)
                        {
                            cart.Remove(item);
                            cart.Add(new Item()
                            {
                                product = product,
                                Quantity = prevQty + 1
                            });
                        }
                        break;
                    }
                }
                Session["cart"] = cart;
            }
            return Redirect("Checkout");
        }

        public ActionResult CheckoutDetails()
        {
            return View();
        }
        public ActionResult SaveCart()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SaveCart(Product product, HttpPostedFileBase file)
        {
            var pd = (List<Item>)Session["cart"];
            foreach (var item in pd)
            {
                UserProductData sale = new UserProductData();
                var data = (from pro in _db.Products
                            where pro.ProductId == item.product.ProductId
                            select new
                            {
                                pass = pro.Quantity
                            }).FirstOrDefault();
                string path = Path.Combine(Server.MapPath("~/ProjectImages"), Path.GetFileName(file.FileName));
                file.SaveAs(path);
                sale.PhoneNumber = Convert.ToString(Session["Number"]);
                sale.CreditCardNumber = Convert.ToString(Session["CdNumber"]);
                sale.PinCode = Convert.ToString(Session["Pincode"]);
                sale.Docmuent = file.FileName;
                sale.ProductName = item.product.ProductName;
                sale.Quantity = item.Quantity;
                sale.Price = Convert.ToInt32(item.product.Price);
                sale.ProID = item.product.ProductId;
                sale.SignInfk = Convert.ToInt32(Session["ID"]);
                _db.UserProductDatas.Add(sale);
                Product newproduct = _db.Products.Find(item.product.ProductId);
                int collectstock = Convert.ToInt32(data.pass) - Convert.ToInt32(item.Quantity);
                newproduct.Quantity = collectstock;

            }

            int result = _db.SaveChanges();
            if (result > 0)
            {

                Session.Remove("cart");
                TempData["LineMessage"] = "<script>swal('Digital Printing', 'Thank You For Shopping','success');</script>";
                TempData.Keep();
                return RedirectToAction("Index");


            }
            else
            {

                return RedirectToAction("CheckoutDetails", "Home");
            }

        }
        public ActionResult RemoveFromCart(int productId)
        {
            List<Item> cart = (List<Item>)Session["cart"];
            foreach (var item in cart)
            {
                if (item.product.ProductId == productId)
                {
                    cart.Remove(item);
                    break;
                }
            }
            Session["cart"] = cart;
            return Redirect("Index");
        }

        public ActionResult Capsule()
        {
            ViewBag.Data = _db.Products.Where(model => model.Category_FK == 1).ToList();
            return View();
        }
        public ActionResult Tablet()
        {
            ViewBag.Data1 = _db.Products.Where(model => model.Category_FK == 2).ToList();
            return View();
        }
        public ActionResult Syrup()
        {
            ViewBag.Data2 = _db.Products.Where(model => model.Category_FK == 3).ToList();
            return View();
        }
   

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Contact(ContectU contect)
        {
          
             
                _db.ContectUs.Add(contect);
            int result = _db.SaveChanges();
            if (result > 0)
            {
                TempData["ContactMessage"] = "<div class='alert alert-success' role='alert'> Thank you For Contacting Us.We Will Contact You With In Working 3 Days.</div>";
                TempData.Keep();
                return RedirectToAction("Contact", "Home");
            }
            else
            {
                TempData["ContactMessage"] = "<div class='alert alert-danger' role='alert'> Please Provide Correct Details.</div>";
                TempData.Keep();
                return RedirectToAction("Contact", "Home");
            }
        } 
        
        [Authorize(Roles = "User")]
        public ActionResult Career()
        {
            return View();
        }
        [Authorize(Roles = "User")]
        [HttpPost]
        public ActionResult Career(Resume rs, HttpPostedFileBase file)
        {
            string path = Path.Combine(Server.MapPath("~/ProjectImages"), Path.GetFileName(file.FileName));
            file.SaveAs(path);
            rs.ResumeDocumentPath = file.FileName;
            rs.UploadedData = DateTime.Now.ToString("ddd/MMM/yyyy");
            rs.SignIn_Fk=Convert.ToInt32(Session["ID"]);
            _db.Resumes.Add(rs);
            int result = _db.SaveChanges();
            if (result > 0)
            {
                TempData["Career"] = "<div class='alert alert-success' role='alert'> Thank you For Uploading Your Reusume.</div>";
                TempData.Keep();
                return RedirectToAction("Career", "Home");
            }
            else
            {
                TempData["Career"] = "<div class='alert alert-danger' role='alert'> Please Provide Correct Details.</div>";
                TempData.Keep();
                return RedirectToAction("Career", "Home");
            }
        }
    }
}