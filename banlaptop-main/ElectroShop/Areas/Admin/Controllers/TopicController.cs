using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using ElectroShop.Models;

namespace ElectroShop.Areas.Admin.Controllers
{
    public class TopicController : BaseController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();

        // GET: Admin/Topic
        public ActionResult Index()
        {
            ViewBag.demrac = db.Topics.Where(m => m.Status == 0).Count();
            var list = db.Topics.Where(m => m.Status != 0).ToList();

            foreach (var row in list)
            {
                var temp_link = db.Links.Where(m => m.Type == "topic" && m.TableId == row.Id);
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
                    row_link.Type = "topic";
                    row_link.TableId = row.Id;
                    db.Links.Add(row_link);
                }
            }
            db.SaveChanges();
            return View(list);
        }

        // GET: Admin/Topic/Details/5
        public ActionResult Details(int id)
        {
            ViewBag.demrac = db.Topics.Where(m => m.Status == 0).Count();
            {
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/topic/get?id=" + id;
                var _url = _host + _api;
                var postTask = client.GetAsync(_url);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var r = result.Content.ReadAsAsync<MTopic>();
                    r.Wait();
                    var en = r.Result;
                    return View(en);
                }
                else
                {
                    Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n " + result.ReasonPhrase, "warning");                  
                }
            }
            return RedirectToAction(nameof(Index));
        }
        public ActionResult Status(int? id)
        {
            MTopic en = db.Topics.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại danh mục!", "warning");
                return RedirectToAction("Index");
            }
            en.Status = (en.Status == 1) ? 2 : 1;

            en.Updated_at = DateTime.Now;
            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Thay đổi trạng thái thành công!" + " id = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult Create()
        {
            ViewBag.demrac = db.Topics.Where(m => m.Status == 0).Count();
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Orders", "Name", 0);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MTopic en)
        {
            ViewBag.demrac = db.Topics.Where(m => m.Status == 0).Count();
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Orders", "Name", 0);
            if (ModelState.IsValid)
            {
                if (en.ParentId == null)
                {
                    en.ParentId = 0;
                }
                String Slug = MyString.ToAscii(en.Name);
                if (db.Categorys.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }
                if (db.Topics.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong TOPIC, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }
                if (db.Posts.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong POST, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }
                if (db.Products.Where(m => m.Slug == Slug).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong PRODUCT, vui lòng thử lại!", "warning");
                    return RedirectToAction("Create", "Topic");
                }


                en.Slug = Slug;
                en.Created_by = int.Parse(Session["Admin_ID"].ToString());
                en.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/topic/add";
                    var _url = _host + _api;
                    var postTask = client.PostAsJsonAsync<MTopic>(_url, en);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Danh mục đã được thêm!", "success");
                        return RedirectToAction("Index", "Topic");
                    }
                    else
                    {
                        var Code = (int)result.StatusCode;
                        Notification.set_flash("Lỗi !", "fail");
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
            ViewBag.list = db.Categorys.Where(m => m.Status == 1).ToList();
            return View(en);
        }
        public ActionResult DelTrash(int id)
        {
            MTopic en = db.Topics.Find(id);
            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/topic/delete";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MTopic>(_url, en);
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
        public ActionResult ReTrash(int? id)
        {
            MTopic cate = db.Topics.Find(id);
            if (cate == null)
            {
                Notification.set_flash("Không tồn tại chủ đề!", "danger");
                return RedirectToAction("Trash", "Topic");
            }
            cate.Status = 2;

            cate.Updated_at = DateTime.Now;
            cate.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(cate).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash", "Topic");
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại !", "warning");
                return RedirectToAction("Trash", "Topic");
            }
            MTopic en = db.Topics.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại!", "warning");
                return RedirectToAction("Trash", "Topic");
            }
            return View(en);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MTopic en = db.Topics.Find(id);
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/topic/delete?real_mode=true";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MTopic>(_url, en);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Đã xóa hoàn toàn chủ đề!", "success");
            }
            else
            {
                var Code = (int)result.StatusCode;
                Notification.set_flash("Lỗi !", "warning");
            }           
            return RedirectToAction("Trash", "Topic");
        }

        public ActionResult Trash()
        {
            return View(db.Topics.Where(m => m.Status == 0).ToList());
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.demrac = db.Topics.Where(m => m.Status == 0).Count();
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Orders", "Name", 0);
            MTopic en = db.Topics.Find(id);
            if (en == null)
            {
                Notification.set_flash("404!", "warning");
                return RedirectToAction("Index", "Topic");
            }
            return View(en);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MTopic en)
        {
            ViewBag.listTopic = new SelectList(db.Topics.Where(m => m.Status == 1), "ID", "Name", 0);
            ViewBag.listOrder = new SelectList(db.Topics.Where(m => m.Status == 1), "Orders", "Name", 0);
            if (ModelState.IsValid)
            {
                if (en.ParentId == null)
                {
                    en.ParentId = 0;
                }
                String Slug = MyString.ToAscii(en.Name);
                int ID = en.Id;
                if (db.Categorys.Where(m => m.Slug == Slug && m.Id != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                if (db.Topics.Where(m => m.Slug == Slug && m.Id != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong TOPIC, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                if (db.Posts.Where(m => m.Slug == Slug && m.Id != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong POST, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                if (db.Products.Where(m => m.Slug == Slug && m.ID != ID).Count() > 0)
                {
                    Notification.set_flash("Tên danh mục đã tồn tại trong PRODUCT, vui lòng thử lại!", "warning");
                    return RedirectToAction("Edit", "Topic");
                }
                en.Updated_by = int.Parse(Session["Admin_ID"].ToString());

                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/topic/edit";
                    var _url = _host + _api;
                    var postTask = client.PutAsJsonAsync<MTopic>(_url, en);
                    postTask.Wait();
                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Cập nhật thành công chủ đề!", "success");
                    }
                    else
                    {
                        var Code = (int)result.StatusCode;
                        Notification.set_flash("Lỗi !", "warning");
                    }
                }                
                return RedirectToAction("Index");
            }
            ViewBag.list = db.Categorys.Where(m => m.Status == 1).ToList();
            return View(en);
        }

        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MTopic en = db.Topics.Find(id);
            en.Status = (en.Status == 1) ? 2 : 1;

            en.Updated_at = DateTime.Now;
            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = en.Status });
        }
    }
}
