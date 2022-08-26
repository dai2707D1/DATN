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
    public class CategoryController : BaseController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();

        // GET: Admin/Category
        public ActionResult Index()
        {
            ViewBag.count_trash = db.Categorys.Where(m => m.Status == 0).Count();
            var list = db.Categorys.Where(m => m.Status != 0).ToList();
            ViewBag.GetAllCategory = list;
            foreach (var row in list)
            {
                var temp_link = db.Links.Where(m => m.Type == "category" && m.TableId == row.Id);
                if (temp_link.Count() > 0)
                {
                    var row_link = temp_link.First();
                    row_link.Name = row.Name;
                    row_link.Slug = row.Slug;
                    db.Entry(row_link).State = EntityState.Modified;
                }
                else
                {
                    var row_link = new MLink();
                    row_link.Name = row.Name;
                    row_link.Slug = row.Slug;
                    row_link.Type = "category";
                    row_link.TableId = row.Id;
                    db.Links.Add(row_link);
                }
            }
            db.SaveChanges();
            return View(list);
        }
        public ActionResult Trash()
        {
            ViewBag.Title = "Danh sách các loại sản phẩm";
            ///////Select * from
            var model = db.Categorys
                .Where(m => m.Status == 0)
                .OrderByDescending(m => m.Created_at)
                .ToList();

            return View("Trash", model);
        }

        // GET: Admin/Category/Details/5
        public ActionResult Details(int id)
        {
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/category/get?id=" + id;
                var _url = _host + _api;
                var postTask = client.GetAsync(_url);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var r = result.Content.ReadAsAsync<MCategory>();
                    r.Wait();
                    var en = r.Result;
                    return View(en);
                }
                else
                {
                    Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n " + result.ReasonPhrase, "warning");

                }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Category/Create
        public ActionResult Create()
        {
            ViewBag.ListCast = new SelectList(db.Categorys, "Id", "Name", 0);
            ViewBag.ListOrder = new SelectList(db.Categorys, "Orders", "Name", 0);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MCategory en)
        {
            ViewBag.ListCast = new SelectList(db.Categorys.Where(m => m.Status != 0).ToList(), "Id", "Name", 0);
            ViewBag.ListOrder = new SelectList(db.Categorys.Where(m => m.Status != 0).ToList(), "Orders", "Name", 0);
            if (ModelState.IsValid)
            {
                string slug = MyString.ToAscii(en.Name);
                en.Slug = slug;
                CheckSlug check = new CheckSlug();

                if (!check.KiemTraSlug("Category", slug, null))
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Category");
                }

                en.Created_by = int.Parse(Session["Admin_ID"].ToString());
                en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/category/add";
                    var _url = _host + _api;
                    var postTask = client.PostAsJsonAsync<MCategory>(_url, en);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Danh mục đã được thêm!", "success");
                    }
                    else
                    {

                        var Code = (int)result.StatusCode;
                        Notification.set_flash("Lỗi !", "fail");
                    }
                }
                return RedirectToAction("Index");
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(x => x.ErrorMessage));
                Notification.set_flash("Lỗi ! " + message, "warning");
            }
            return View(en);
        }

        // GET: Admin/Category/Edit/5
        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MCategory en = db.Categorys.Find(id);
            if (en == null)
            {
                return HttpNotFound();
            }
            ViewBag.ListCast = new SelectList(db.Categorys.Where(m => m.Status != 0).ToList(), "Id", "Name", 0);
            ViewBag.ListOrder = new SelectList(db.Categorys.Where(m => m.Status != 0).ToList(), "Orders", "Name", 0);
            return View(en);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MCategory en)
        {
            if (ModelState.IsValid)
            {
                if (en.ParentId == null)
                {
                    en.ParentId = 0;
                }

                String slug = MyString.ToAscii(en.Name);
                en.Slug = slug;
                int ID = en.Id;

                if (db.Categorys.Where(m => m.Slug == slug && m.Id != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }
                if (db.Topics.Where(m => m.Slug == slug && m.Id != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong TOPIC, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }
                if (db.Posts.Where(m => m.Slug == slug && m.Id != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong POST, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }
                if (db.Products.Where(m => m.Slug == slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong PRODUCT, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Category");
                }

                en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
                en.Created_by = int.Parse(Session["Admin_ID"].ToString());
                /* en.Created_at = DateTime.Now;
                 en.Updated_at = DateTime.Now;
                 db.Categorys.Add(en);
                 // db.SaveChanges();

                 db.Entry(en).State = EntityState.Modified;
                 db.SaveChanges();*/
                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/category/edit";
                    var _url = _host + _api;
                    var postTask = client.PutAsJsonAsync<MCategory>(_url, en);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Cập nhật thành công!", "success");
                    }
                    else
                    {
                        var Code = (int)result.StatusCode;
                        Notification.set_flash("Lỗi !", "fail");
                    }
                }           
                return RedirectToAction("Index");
            }
            else
            {
                var message = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(x => x.ErrorMessage));
                Notification.set_flash("Lỗi ! " + message, "warning");
            }
            ViewBag.ListCast = new SelectList(db.Categorys.Where(m => m.Status != 0).ToList(), "Id", "Name", 0);
            ViewBag.ListOrder = new SelectList(db.Categorys.Where(m => m.Status != 0).ToList(), "Orders", "Name", 0);
            return View(en);
        }


        public ActionResult DelTrash(int id)
        {
            MCategory MCategory = db.Categorys.Find(id);
            if (MCategory == null)
            {
                Notification.set_flash("Không tồn tại danh mục cần xóa vĩnh viễn!", "warning");
                return RedirectToAction("Index");
            }
            int count_child = db.Categorys.Where(m => m.ParentId == id).Count();
            if (count_child != 0)
            {
                Notification.set_flash("Không thể xóa, danh mục có chứa danh mục con!", "warning");
                return RedirectToAction("Index");
            }
            MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/category/delete";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MCategory>(_url, MCategory);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Ném vào thùng rác!" + " ID = " + id, "success");
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
            MCategory en = db.Categorys.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại danh mục!", "warning");
                return RedirectToAction("Trash");
            }
            en.Status = 2;

            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            en.Created_by = int.Parse(Session["Admin_ID"].ToString());
            en.Created_at = DateTime.Now;
            en.Updated_at = DateTime.Now;

            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "Category");

        }
        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MCategory MCategory = db.Categorys.Find(id);
            MCategory.Status = (MCategory.Status == 1) ? 2 : 1;

            MCategory.Updated_at = DateTime.Now;
            MCategory.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(MCategory).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new
            {
                Status = MCategory.Status
            });
        }
        // GET: Admin/Category/Delete/5
        public ActionResult Delete(int id)
        {
            MCategory en = db.Categorys.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại danh mục cần xóa!", "warning");
                return RedirectToAction("Trash", "Category");
            }
            return View(en);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MCategory en = db.Categorys.Find(id);
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/category/delete?real_mode=true";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MCategory>(_url, en);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Đã xóa hoàn toàn danh mục!", "success");
            }
            else
            {
                var Code = (int)result.StatusCode;
                Notification.set_flash("Lỗi !", "warning");
            }
            
            return RedirectToAction("Index");
        }
    }
}
