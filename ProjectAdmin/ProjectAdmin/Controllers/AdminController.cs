using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ProjectAdmin.Models;
using System.Data.Entity;
using System.Net;
using System.Net.Mail;

namespace ProjectAdmin.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {

        PharmacyEntities _db = new PharmacyEntities();
     

        [Authorize(Roles = "Admin,Accountant,Customer")]
        public ActionResult Index()
        {
            if (Session["ID"] != null)
            {
                Session["CuurentAdminCount"] = _db.SignUps.Where(model => model.Roles_Fk == 1).Count();
                Session["CuurentCustomerCount"] = _db.SignUps.Where(model => model.Roles_Fk == 3).Count();
                Session["CuurentAccountantCount"] = _db.SignUps.Where(model => model.Roles_Fk == 2).Count();
                return View(_db.SignUps.ToList());
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        [AllowAnonymous]
        public ActionResult Signup()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Signup(SignUp sp, HttpPostedFileBase file)
        {

            var SeachData = _db.SignUps.Where(x => x.Username == sp.Username).FirstOrDefault();
            if (SeachData != null)
            {
                TempData["Message"] = "Username Already Exisist";
                return View();
            }
            else
            {
                if (file != null)
                {
                    string path = Path.Combine(Server.MapPath("~/ProjectImages"), Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    sp.ImagePath = file.FileName;
                    sp.Roles_Fk = 4;
                    sp.Status = "Active";
                    sp.CreatedAt = DateTime.Now.ToString("ddd/MMM/yyyy");
                    var passencrypt = System.Text.Encoding.UTF8.GetBytes(sp.Password);
                    sp.Password = System.Convert.ToBase64String(passencrypt);
                    var confirmpassss = System.Text.Encoding.UTF8.GetBytes(sp.ConfirmPassword);
                    sp.ConfirmPassword = System.Convert.ToBase64String(confirmpassss);
                    _db.SignUps.Add(sp);
                    int result = _db.SaveChanges();
                    if (result > 0)
                    {
                        TempData["Message"] = "Account Has Been Created...";
                        TempData.Keep();
                        return RedirectToAction("Signup");
                    }
                    else
                    {
                        TempData["Message"] = "Please Provide Correct Details...";
                        TempData.Keep();
                        return RedirectToAction("Signup");
                    }
                }
                else
                {
                    sp.CreatedAt = DateTime.Now.ToString();
                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(sp.Password);
                    sp.Password = System.Convert.ToBase64String(plainTextBytes);
                    var confirmpassss = System.Text.Encoding.UTF8.GetBytes(sp.ConfirmPassword);
                    sp.ConfirmPassword = System.Convert.ToBase64String(confirmpassss);
                    sp.Roles_Fk = 4;
                    sp.Status = "Active";
                    _db.SignUps.Add(sp);
                    int result = _db.SaveChanges();
                    if (result > 0)
                    {
                        TempData["Message"] = "Account Has Been Created...";
                        TempData.Keep();
                        return RedirectToAction("Signup");
                    }
                    else
                    {
                        TempData["Message"] = "Please Provide Correct Details...";
                        TempData.Keep();
                        return RedirectToAction("Signup");
                    }
                }
            

            }
                

            

        } 
        [AllowAnonymous]
        public ActionResult Login()
        {

            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(SignUp sp)
        {
            var CheckUsernameExisit = _db.SignUps.Where(x => x.Username == sp.Username).FirstOrDefault();
            if (CheckUsernameExisit == null)
            {
                    TempData["Message"] = "Username Is Incorrect..";
                    TempData.Keep();
            }

            else
            {
                if (ModelState.IsValid == true)
                {
                   //Ye query 1code mene is liye use kra ha  ke ye ye username uthata ha phr database se lejake
                   //match krta ha aur phr uska password ye query mjhe bdle me lake dti ha
                   
                   //1.1 Query
                    var CheckPass = (from pd in _db.SignUps
                                where pd.Username == sp.Username
                                select new 
                                {
                                    pass = pd.Password,
                                }).FirstOrDefault();

                    //ye hash code ko use kra ha mne ye kya krta ha ye password uthata ha database se
                    //joke apko upar wali query se milta ha checkpass wali se phr ye pass ko decrypt 
                    //krta ha aur phr row 1.2 query se match username password match krke login krta ha

                    var plainTextBytes = System.Convert.FromBase64String(CheckPass.pass);
                    var convertpass = System.Text.Encoding.UTF8.GetString(plainTextBytes);
                    
                    //1.2 query for matching username and password
                    var row = _db.SignUps.Where(model => model.Username == sp.Username && convertpass == sp.Password).FirstOrDefault();

                    if (row != null)
                    {
                        if (row.UserRole.Roles == "Admin" || row.UserRole.Roles == "Accountant" || row.UserRole.Roles == "Customer")
                        {
                            var data = (from pd in _db.SignUps
                                        join od in _db.UserRoles on pd.Roles_Fk equals od.UserID
                                        where pd.Username == sp.Username
                                        select new  // Projection to Anonymous type
                                        {
                                            ID = pd.Id,
                                            username = pd.Username,
                                            email = pd.Email,
                                            pass = pd.Password,
                                            cpass = pd.ConfirmPassword,
                                            images = pd.ImagePath,
                                            fname = pd.Fname,
                                            lname = pd.Lname,
                                            term = pd.TermsConditions,
                                            news = pd.TermsConditions,
                                            CheckRole = od.Roles,
                                        }).FirstOrDefault();

                            if (data != null)
                            {

                                Session["ID"] = data.ID;
                                Session["username"] = data.username;
                                Session["fname"] = data.fname;
                                Session["lanme"] = data.lname;
                                Session["Email"] = data.email;
                                Session["Password"] = data.pass;
                                Session["confirmpass"] = data.cpass;
                                Session["Image"] = data.images;
                                Session["Terms"] = data.term;
                                Session["News"] = data.news;
                                Session["roles"] = data.CheckRole;
                                FormsAuthentication.SetAuthCookie(sp.Username, false);
                                return RedirectToAction("Index", "Admin");
                            }
                            else
                            {
                                TempData["Message"] = "Please Be Patient while The Admin Gives You Role";
                                TempData.Keep();
                                return RedirectToAction("Login");
                            }
                        }

                        else if (row.UserRole.Roles == "User")
                        {
                            var data = (from pd in _db.SignUps
                                        join od in _db.UserRoles on pd.Roles_Fk equals od.UserID
                                        where pd.Username == sp.Username
                                        select new  // Projection to Anonymous type
                                        {
                                            ID = pd.Id,
                                            username = pd.Username,
                                            email = pd.Email,
                                            pass = pd.Password,
                                            cpass = pd.ConfirmPassword,
                                            images = pd.ImagePath,
                                            fname = pd.Fname,
                                            lname = pd.Lname,
                                            term = pd.TermsConditions,
                                            news = pd.TermsConditions,
                                            CheckRole = od.Roles,
                                        }).FirstOrDefault();

                            if (data != null)
                            {

                                Session["ID"] = data.ID;
                                Session["username"] = data.username;
                                Session["fname"] = data.fname;
                                Session["lanme"] = data.lname;
                                Session["Email"] = data.email;
                                Session["Password"] = data.pass;
                                Session["confirmpass"] = data.cpass;
                                Session["Image"] = data.images;
                                Session["Terms"] = data.term;
                                Session["News"] = data.news;
                                Session["roles"] = data.CheckRole;
                                FormsAuthentication.SetAuthCookie(sp.Username, false);
                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                TempData["Message"] = "Please Be Patient while The Admin Gives You Role";
                                TempData.Keep();
                                return RedirectToAction("Login");
                            }
                        }

                        else
                        {
                            return RedirectToAction("Login");
                        }
                    }
                    else
                    {
                        return RedirectToAction("Login");
                    }
                }
            }
            return View();
        }
        [Authorize(Roles = "Admin,Accountant,Customer")]
        public ActionResult Profilee()
        {
            if (Session["ID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                if (User.Identity.IsAuthenticated)
                {
                    int id = Convert.ToInt32(Session["ID"]);
                    var query = _db.SignUps.Where(x => x.Id == id).FirstOrDefault();
                    ViewBag.Data = query;
                    return View();
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }
              
        }
        [Authorize(Roles = "Admin,Accountant,Customer")]
        public ActionResult EditProfile()
        {
           
            if (Session["ID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                if (User.Identity.IsAuthenticated)
                {
                    ViewBag.AllCountries = _db.Countries.ToList();
                    int id = Convert.ToInt32(Session["ID"]);
                    var query = _db.SignUps.Where(x => x.Id == id).FirstOrDefault();
                    ViewBag.Data = query;
                    return View();
                }
                else
                {
                    return RedirectToAction("Login");
                }
            }

        }
        [Authorize(Roles="Admin,Accountant,Customer")]
        [HttpPost]
        public ActionResult EditProfile(SignUp sp,HttpPostedFileBase file)
        {
            //for Fetching Countries to Database
            

            if (file != null)
            {
                if(User.Identity.IsAuthenticated)
                {
                   
                    string path = Path.Combine(Server.MapPath("~/ProjectImages"), Path.GetFileName(file.FileName));
                    file.SaveAs(path);
                    sp.Id = Convert.ToInt32(Session["ID"]);
                    sp.ImagePath = file.FileName;
                    sp.UpdatedBy = DateTime.Now.ToString("ddd/MMM/yyyy");
                    var passencrypt = System.Text.Encoding.UTF8.GetBytes(sp.Password);
                    sp.Password = System.Convert.ToBase64String(passencrypt);
                    var confirmpassss = System.Text.Encoding.UTF8.GetBytes(sp.ConfirmPassword);
                    sp.ConfirmPassword = System.Convert.ToBase64String(confirmpassss);

                    _db.Entry(sp).State = EntityState.Modified;
                    _db.Entry(sp).Property("CreatedAt").IsModified = false;
                    int a = _db.SaveChanges();
                    if (a > 0)
                    {
                        Session["Image"] = sp.ImagePath;
                        ViewBag.MessageEdit = "Data Has Been Successfully Updated";
                        return RedirectToAction("Profilee");
                    }
                    else
                    {
                        ViewBag.MessageEdit = "Data Has Not Been Updated";
                    }
                }
            }
            else
            {
                if (User.Identity.IsAuthenticated)
                {

                    sp.Id = Convert.ToInt32(Session["ID"]);
                    sp.ImagePath = Convert.ToString(Session["Image"]);
                    var passencrypt = System.Text.Encoding.UTF8.GetBytes(sp.Password);
                    sp.Password = System.Convert.ToBase64String(passencrypt);
                    var confirmpassss = System.Text.Encoding.UTF8.GetBytes(sp.ConfirmPassword);
                    sp.ConfirmPassword = System.Convert.ToBase64String(confirmpassss);
                    sp.UpdatedBy = DateTime.Now.ToString("ddd/MMM/yyyy");
                    _db.Entry(sp).State = EntityState.Modified;
                    _db.Entry(sp).Property("CreatedAt").IsModified = false;
                    int a = _db.SaveChanges();
                    if (a > 0)
                    {
                        Session["Image"] = sp.ImagePath;
                        ViewBag.MessageEdit = "Data Has Been Successfully Updated";
                        ModelState.Clear();
                        return RedirectToAction("Profilee");
                    }
                    else
                    {
                        ViewBag.MessageEdit = "Data Has Not Been Updated";
                    }
                }

          
            }
       
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AssignName()
        {
            var Assignname= _db.SignUps.ToList();
            return View(Assignname);

        }
        [Authorize(Roles = "Admin")]
        public ActionResult EditUSersRole(int Id)
        {

            ViewBag.EditFetchData = _db.SignUps.Where(x => x.Id == Id).FirstOrDefault();
            ViewBag.FetchRole = _db.UserRoles.ToList(); 
            return View();

        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditUSersRole(SignUp sp)
        {

            if (ModelState.IsValid)
            {
                _db.Entry(sp).State = EntityState.Modified;
                _db.Entry(sp).Property("Username").IsModified = false;
                _db.Entry(sp).Property("Email").IsModified = false;
                _db.Entry(sp).Property("Fname").IsModified = false;
                _db.Entry(sp).Property("Lname").IsModified = false;
                _db.Entry(sp).Property("Gender").IsModified = false;
                _db.Entry(sp).Property("Password").IsModified = false;
                _db.Entry(sp).Property("ConfirmPassword").IsModified = false;
                _db.Entry(sp).Property("TermsConditions").IsModified = false;
                _db.Entry(sp).Property("Newsletter").IsModified = false;
                _db.Entry(sp).Property("ImagePath").IsModified = false;
                _db.Entry(sp).Property("CreatedAt").IsModified = false;
                _db.Entry(sp).Property("UpdatedBy").IsModified = false;
                _db.Entry(sp).Property("Status").IsModified = false;
                _db.Entry(sp).Property("Country_Fk").IsModified = false;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("DeleteRoles");
            }
            

        }
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRoles(int Id)
        {
            ViewBag.DeleteRoles = _db.SignUps.Where(x => x.Id == Id).FirstOrDefault();
            return View();  
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult DeleteRoles(SignUp sp)
        {
            if (ModelState.IsValid)
            {
                _db.Entry(sp).State = EntityState.Modified;
                _db.Entry(sp).Property("Username").IsModified = false;
                _db.Entry(sp).Property("Email").IsModified = false;
                _db.Entry(sp).Property("Fname").IsModified = false;
                _db.Entry(sp).Property("Lname").IsModified = false;
                _db.Entry(sp).Property("Gender").IsModified = false;
                _db.Entry(sp).Property("Password").IsModified = false;
                _db.Entry(sp).Property("ConfirmPassword").IsModified = false;
                _db.Entry(sp).Property("TermsConditions").IsModified = false;
                _db.Entry(sp).Property("Newsletter").IsModified = false;
                _db.Entry(sp).Property("ImagePath").IsModified = false;
                _db.Entry(sp).Property("CreatedAt").IsModified = false;
                _db.Entry(sp).Property("UpdatedBy").IsModified = false;
                _db.Entry(sp).Property("Status").IsModified = false;
                _db.Entry(sp).Property("Country_Fk").IsModified = false;
                int result =_db.SaveChanges();
                if (result > 0)
                {
                    TempData["DeleteRole"] = "<div class='alert alert-success alert - dismissable'><button type='button' class='close' data-dismiss='alert' aria-hidden='true'>&times;</button>Role Has Been Deleted</div>";
                    TempData.Keep();
                    return RedirectToAction("AssignName");
                }
                else
                {
                    return RedirectToAction("DeleteRoles");

                }
                
            }
            else
            {
                return RedirectToAction("AssignName");
            }
        }
        [Authorize(Roles = "Admin,Accountant,Customer")]
        public ActionResult SendEmail()
        {
            return View();
        }
        [Authorize(Roles = "Admin,Accountant,Customer")]
        [HttpPost]
        public ActionResult SendEmail(string receiver, string subject, string message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var senderEmail = new MailAddress("Enter Your Email", "Asad Ali");
                    var receiverEmail = new MailAddress(receiver, "Receiver");
                    var password = "Enter Your Password Here";
                    var sub = subject;
                    var body = message;
                    var smtp = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body
                    })
                    {
                        smtp.Send(mess);
                    }
                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Some Error";
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult AddRoles()
        {
            ViewBag.UserRoles=_db.UserRoles.ToList();
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddRoles(UserRole ur)
        {
            _db.UserRoles.Add(ur);
            int result = _db.SaveChanges();
            if (result > 0)
            {
                TempData["AddRoleMessage"] = "<b>Note:</b> Role Added Successfully..";
                TempData.Keep();
                return RedirectToAction("AddRoles");
            }
            else
            {
                TempData["AddRoleMessage"] = "<b>Note:</b> Role Not Added..";
                TempData.Keep();
                return RedirectToAction("AddRoles");
            }
        }
            public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Login");
        }
        [Authorize(Roles ="Admin")]
        public ActionResult AddProduct()
        {
            ViewBag.Category = _db.Categories.ToList();
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddProduct(Product p, HttpPostedFileBase file)
        {
            string path = Path.Combine(Server.MapPath("~/ProjectImages"), Path.GetFileName(file.FileName));
            file.SaveAs(path);
            p.ProductImage = file.FileName;
            p.SignInfk = Convert.ToInt32(Session["ID"]);
            _db.Products.Add(p);
            int result = _db.SaveChanges();
            if (result > 0)
            {
                TempData["AddProductMessage"] = "<b>Note:</b> Product Added Successfully..";
                TempData.Keep();
                return RedirectToAction("AddProduct");
            }
            else
            {
                TempData["AddProductMessage"] = "<b>Note:</b> Product Not Added..";
                TempData.Keep();
                return RedirectToAction("AddProduct");
            }
        }
        public ActionResult ShowResume()
        {
            ViewBag.addiytheKing = _db.Resumes.ToList();
            return View();
        }

        public ActionResult ContactUs()
        {
            ViewBag.Contact = _db.ContectUs.ToList();
            return View();
        }
    }
}
