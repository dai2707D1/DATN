using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using ElectroShop.Models;

namespace ElectroShop.Areas.Admin.Controllers
{
    public class ProductController : BaseController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();

        // GET: Admin/Product
        public ActionResult Index()
        {
            ViewBag.countTrash = db.Products.Where(m => m.Status == 0).Count();
            var list = from p in db.Products
                       join c in db.Categorys
                       on p.CateID equals c.Id
                       where p.Status != 0
                       where p.CateID == c.Id
                       orderby p.Created_at descending
                       select new ProductCategory()
                       {
                           ProductId = p.ID,
                           ProductImg = p.Image,
                           ProductName = p.Name,
                           ProductStatus = p.Status,
                           ProductDiscount = p.Discount,
                           ProductPrice = p.Price,
                           ProductPriceSale = p.ProPrice,
                           ProductCreated_At = p.Created_at,
                           CategoryName = c.Name
                       };
            return View(list.ToList());
        }
        public ActionResult Trash()
        {
            var list = from p in db.Products
                       join c in db.Categorys
                       on p.CateID equals c.Id
                       where p.Status == 0
                       where p.CateID == c.Id
                       orderby p.Created_at descending
                       select new ProductCategory()
                       {
                           ProductId = p.ID,
                           ProductImg = p.Image,
                           ProductName = p.Name,
                           ProductStatus = p.Status,
                           ProductDiscount = p.Discount,
                           ProductPrice = p.Price,
                           ProductPriceSale = p.ProPrice,
                           ProductCreated_At = p.Created_at,
                           CategoryName = c.Name
                       };
            return View(list.ToList());
        }

        public ActionResult Details(int id)
        {
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/product/get?id="+id;
                var _url = _host + _api;
                var postTask = client.GetAsync(_url);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var r = result.Content.ReadAsAsync<MProduct>();
                    r.Wait();
                    var en = r.Result;
                    return View(en);
                }
                else
                {
                    Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n "+result.ReasonPhrase, "warning");
                    
                }
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Create()
        {
            ViewBag.countTrash = db.Products.Where(m => m.Status == 0).Count();
            MCategory mCategory = new MCategory();
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);
            //ViewBag.ListCat = new SelectList(db.Category.ToList(), "ID", "Name", 0);
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MProduct e)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.Where(m => m.Status != 0), "ID", "Name", 0);
            if (ModelState.IsValid)
            {
                String strSlug = MyString.ToAscii(e.Name);
                e.Slug = strSlug;
                var file = Request.Files["Image"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    String Strpath = Path.Combine(Server.MapPath("~/Public/library/product/"), filename);
                    file.SaveAs(Strpath);
                    e.Image = filename;
                }
                e.Created_by = int.Parse(Session["Admin_ID"].ToString());
                e.Updated_by = int.Parse(Session["Admin_ID"].ToString());
                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/product/add";
                    var _url = _host + _api;
                    var postTask = client.PostAsJsonAsync<MProduct>(_url, e);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {                        
                        Notification.set_flash("Thêm mới sản phẩm thành công!", "success");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Notification.set_flash("Lỗi !"+result.ReasonPhrase, "warning");
                    }
                }
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(x => x.ErrorMessage));
                Notification.set_flash("Lỗi ! " + message, "warning");
            }
            return View(e);
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.countTrash = db.Products.Where(m => m.Status == 0).Count();
            ViewBag.ListCat = new SelectList(db.Categorys.ToList(), "ID", "Name", 0);
            MProduct e = db.Products.Find(id);
            if (e == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "Product");
            }
            return View(e);
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MProduct e)
        {
            ViewBag.ListCat = new SelectList(db.Categorys.ToList(), "ID", "Name", 0);
            if (ModelState.IsValid)
            {
                e.Updated_by = int.Parse(Session["Admin_ID"].ToString());
                using (var client = new HttpClient())
                {
                    String strSlug = MyString.ToAscii(e.Name);
                    e.Slug = strSlug;
                    var file = Request.Files["Image"];
                    if (file != null && file.ContentLength > 0)
                    {
                        String filename = strSlug + file.FileName.Substring(file.FileName.LastIndexOf("."));
                        String Strpath = Path.Combine(Server.MapPath("~/Public/library/product/"), filename);
                        file.SaveAs(Strpath);
                        e.Image = filename;
                    }
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/product/edit";
                    var _url = _host + _api;
                    var postTask = client.PutAsJsonAsync<MProduct>(_url, e);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Đã cập nhật lại thông tin sản phẩm!", "success");
                    }
                    else
                    {

                        var Code = (int)result.StatusCode;
                        Notification.set_flash("Lỗi !", "warning");
                    }
                }
                
                return RedirectToAction("Index");
            }
            return View(e);
        }

        public ActionResult DelTrash(int? id)
        {
            MProduct e = db.Products.Find(id);        
            e.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/product/delete";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MProduct>(_url, e);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Ném thành công vào thùng rác!" + " ID = " + id, "success");
            }
            else
            {
                var Code = (int)result.StatusCode;
                Notification.set_flash("Lỗi !", "warning");
            }
            
            return RedirectToAction("Index");
        }
        public ActionResult Undo(int? id)
        {
            MProduct e = db.Products.Find(id);
            e.Status = 2;

            e.Updated_at = DateTime.Now;
            e.Updated_by = int.Parse(Session["Admin_ID"].ToString()); ;
            db.Entry(e).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash");
        }
        public ActionResult Delete(int id)
        {
            MProduct e = db.Products.Find(id);
            if (e == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash");
            }            
            return View(e);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MProduct e = db.Products.Find(id);
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/product/delete?real_mode=true";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MProduct>(_url, e);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Đã xóa vĩnh viễn sản phẩm!", "danger");
            }
            else
            {
                var Code = (int)result.StatusCode;
                Notification.set_flash("Lỗi !", "warning");
            }       
            return RedirectToAction("Trash");
        }

        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MProduct e = db.Products.Find(id);
            e.Status = (e.Status == 1) ? 2 : 1;
            e.Updated_at = DateTime.Now;
            e.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(e).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = e.Status });
        }
        [HttpPost]
        public JsonResult changeDiscount(int id)
        {
            MProduct e = db.Products.Find(id);
            e.Discount = (e.Discount == 1) ? 2 : 1;

            e.Updated_at = DateTime.Now;
            e.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(e).State = EntityState.Modified;
            db.SaveChanges();

            return Json(new { Discount = e.Discount });
        }
    }
}
