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
    public class PageController : BaseController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();

        public ActionResult Index()
        {
            ViewBag.countTrash = db.Posts.Where(m => m.Status == 0 && m.Type == "page").Count();
            var list = db.Posts.Where(m => m.Status != 0 && m.Type == "page").ToList();
            foreach (var row in list)
            {
                var temp_link = db.Links.Where(m => m.Type == "page" && m.TableId == row.Id);
                if (temp_link.Count() > 0)
                {
                    var row_link = temp_link.First();
                    row_link.Name = row.Title;
                    row_link.Slug = row.Slug;
                    db.Entry(row_link).State = EntityState.Modified;
                }
                else
                {
                    var row_link = new MLink();
                    row_link.Name = row.Title;
                    row_link.Slug = row.Slug;
                    row_link.Type = "page";
                    row_link.TableId = row.Id;
                    db.Links.Add(row_link);
                }
            }
            db.SaveChanges();
            return View(list);
        }
        public ActionResult Trash()
        {
            ViewBag.countTrash = db.Posts.Where(m => m.Status == 0 && m.Type == "page").Count();
            return View(db.Posts.Where(m => m.Status == 0 && m.Type == "page").ToList());
        }

        public ActionResult Details(int? id)
        {
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            ViewBag.countTrash = db.Posts.Where(m => m.Status == 0 && m.Type == "page").Count();
            if (id == null)
            {
                Notification.set_flash("Không tồn tại trang đơn!", "warning");
                return RedirectToAction("Index", "Page");
            }
            else
            {
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/page/get?id=" + id;
                var _url = _host + _api;
                var postTask = client.GetAsync(_url);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    var r = result.Content.ReadAsAsync<MPost>();
                    r.Wait();
                    var en = r.Result;
                    return View(en);
                }
                else
                {
                    Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n " + result.ReasonPhrase, "warning");
                    return RedirectToAction("Index", "Post");
                }
            }
        }

        public ActionResult Create()
        {
            ViewBag.countTrash = db.Posts.Where(m => m.Status == 0 && m.Type == "page").Count();
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MPost en)
        {
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            if (ModelState.IsValid)
            {
                en.Created_By = int.Parse(Session["Admin_ID"].ToString());
                en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/page/add";
                var _url = _host + _api;
                var postTask = client.PostAsJsonAsync(_url,en);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    Notification.set_flash("Đã thêm trang đơn mới!", "success");
                    return RedirectToAction("Index");
                }
                else
                {
                    Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n " + result.ReasonPhrase, "warning");
                }
                             
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

        public ActionResult Edit(int? id)
        {
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            ViewBag.countTrash = db.Posts.Where(m => m.Status == 0 && m.Type == "page").Count();
            if (id == null)
            {
                Notification.set_flash("Không tồn tại trang đơn!", "warning");
                return RedirectToAction("Index", "Page");
            }
            MPost en = db.Posts.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại trang đơn!", "warning");
                return RedirectToAction("Index", "Page");
            }
            return View(en);
        }
        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(MPost en)
        {
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            if (ModelState.IsValid)
            {
                en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/page/edit";
                var _url = _host + _api;
                var postTask = client.PutAsJsonAsync(_url, en);
                postTask.Wait();

                var result = postTask.Result;
                if (result.IsSuccessStatusCode)
                {
                    Notification.set_flash("Đã cập nhật lại nội dung trang đơn!", "success");
                    return RedirectToAction("Index");
                }
                else
                {
                    Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n " + result.ReasonPhrase, "warning");
                }                
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
        public ActionResult DelTrash(int? id)
        {
            MPost en = db.Posts.Find(id);
            en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/page/delete";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MPost>(_url, en);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Đã chuyển vào thùng rác!" + " ID = " + id, "success");
            }
            else
            {
                Notification.set_flash("Lỗi !" + result.ReasonPhrase, "warning");
            }
            
            return RedirectToAction("Index");
        }
        public ActionResult Undo(int? id)
        {
            MPost en = db.Posts.Find(id);
            en.Status = 2;

            en.Updated_At = DateTime.Now;
            en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash");
        }
        public ActionResult Delete(int? id)
        {
            ViewBag.countTrash = db.Posts.Where(m => m.Status == 0 && m.Type == "page").Count();
            if (id == null)
            {
                Notification.set_flash("Không tồn tại trang đơn!", "warning");
                return RedirectToAction("Index", "Page");
            }
            MPost en = db.Posts.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại trang đơn!", "warning");
                return RedirectToAction("Index", "Page");
            }
            return View(en);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MPost en = db.Posts.Find(id);
            db.Posts.Remove(en);
            db.SaveChanges();
            Notification.set_flash("Đã xóa vĩnh viễn", "danger");
            return RedirectToAction("Trash");
        }

    }
}
