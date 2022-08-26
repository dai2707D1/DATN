using ElectroShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace ElectroShop.Controllers
{
    public class AccountController : Controller
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        //public AccountController()
        //{
        //    if (System.Web.HttpContext.Current.Session["User_Name"] == null)
        //    {
        //        System.Web.HttpContext.Current.Response.Redirect("~/");
        //    }
        //}

        [HttpGet,Route("Login")]
        public JsonResult UserLogin(String Username, String Password)
        {
            var _json = new JsonMessageModel();
            _json.Message = "Tài khoản hoặc mật khẩu không đúng!";
            var client = new HttpClient();
            var _api = Url.Action("Login", "User", new { httproute = "DefaultApi" , username=Username, password=Password});
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();

            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<JsonMessageModel>();
                readTask.Wait();
                _json = readTask.Result;
                
                

                var o = db.Users.Find(_json.Return_ID);
                if (o != null)
                {
                    Session["User_ID"] = _json.Return_ID;
                    Session["User_Name"] = o.FullName;
                    Session["User_Images"] = o.Image;
                }
                
                
            }
            return Json(_json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UserLogout(String url)
        {
            Session["User_Name"] = null;
            Session["User_ID"] = null;
            return Redirect("~/" + url);
        }
        public ActionResult ProFile()
        {
            if (System.Web.HttpContext.Current.Session["User_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/");
            }
            return View();
        }
        public ActionResult Notification()
        {
            if (System.Web.HttpContext.Current.Session["User_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/");
            }
            return View();
        }
        public ActionResult Order()
        {
            if (System.Web.HttpContext.Current.Session["User_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/");
            }
            int userid = Convert.ToInt32(Session["User_ID"]);
            var list = db.Orders.Where(m => m.CustemerId == userid).OrderByDescending(m => m.CreateDate).ToList();
            ViewBag.itemOrder = db.Orderdetails.ToList();
            int user_id = Convert.ToInt32(Session["User_ID"]);
            ViewBag.Info = db.Users.Where(m => m.ID == user_id).First();
            ViewBag.productOrder = db.Products.ToList();
            return View(list);
        }
        public ActionResult ActionOrder()
        {
            if (System.Web.HttpContext.Current.Session["User_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/");
            }
            var list = db.Orders.ToList();
            ViewBag.Hoanthanh = db.Orders.Where(m => m.Status == 3).Count();
            ViewBag.ChoXuLy = db.Orders.Where(m => m.Status == 1).Count();
            ViewBag.DangXuLy = db.Orders.Where(m => m.Status == 2).Count();
            return View("_ActionOrder", list);
        }
        public ActionResult OrderDetails(int id)
        {
            if (System.Web.HttpContext.Current.Session["User_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/");
            }
            int userid = Convert.ToInt32(Session["User_ID"]);
            var checkO = db.Orders.Where(m => m.CustemerId == userid && m.Id == id);
            if (checkO.Count() == 0)
            {
                return this.NotFound();
            }

            var id_order = db.Orders.Where(m => m.CustemerId == userid && m.Id == id).FirstOrDefault();
            ViewBag.Order = id_order;
            var itemOrder = db.Orderdetails.Where(m => m.OrderId == id_order.Id).ToList();
            ViewBag.productOrder = db.Products.ToList();
            return View(itemOrder);
        }
        public ActionResult NotFound()
        {
            if (System.Web.HttpContext.Current.Session["User_Name"] == null)
            {
                System.Web.HttpContext.Current.Response.Redirect("~/");
            }
            return View();
        }
        [HttpPost]
        public JsonResult Register(MUser user)
        {
            try
            {
                var checkPM = db.Users.Any(m => m.Phone == user.Phone && m.Email.ToLower().Equals(user.Email.ToLower()));
                if (checkPM)
                {
                    return Json(new { Code = 1, Message = "Số điện thoại hoặc Email đã được sử dụng." });
                }
                user.Gender = 1;
                user.Image = "";
                user.Access = 0;
                user.Status = 1;
                user.Password = MyString.ToMD5(user.Password);
                user.Created_at = DateTime.Now;
                user.Created_by = 1;
                user.Updated_at = DateTime.Now;
                user.Updated_by = 1;

                db.Users.Add(user);
                db.SaveChanges();

                return Json(new { Code = 0, Message = "Đăng ký thành công!" });
            }
            catch (Exception e)
            {
                return Json(new { Code = 1, Message = "Đăng ký thất bại!" });
                throw e;
            }
        }
    }
}