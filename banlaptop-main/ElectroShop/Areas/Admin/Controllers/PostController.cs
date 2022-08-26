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
    public class PostController : BaseController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();

        public ActionResult Index()
        {
            ViewBag.demrac = db.Posts.Where(m => m.Status == 0 && m.Type == "post").Count();
            var list = from p in db.Posts
                       join t in db.Topics
                       on p.Topid equals t.Id
                       where p.Status != 0
                       orderby p.Created_At descending
                       select new PostTopic()
                       {
                           PostId = p.Id,
                           PostImg = p.Img,
                           PostName = p.Title,
                           PostStatus = p.Status,
                           TopicName = t.Name,
                           PostCreated_At = p.Created_At,
                       };
            return View(list.ToList());
        }
        public ActionResult Trash()
        {
            var list = from p in db.Posts
                       join t in db.Topics
                       on p.Topid equals t.Id
                       where p.Status == 0
                       orderby p.Created_At descending
                       select new PostTopic()
                       {
                           PostId = p.Id,
                           PostImg = p.Img,
                           PostName = p.Title,
                           PostStatus = p.Status,
                           TopicName = t.Name
                       };
            return View(list.ToList());
        }
        // Create
        public ActionResult Create()
        {
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            return View();
        }

        [HttpPost, ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Create(MPost en)
        {
            if (ModelState.IsValid)
            {
                en.Created_By = int.Parse(Session["Admin_ID"].ToString());
                en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
                var file = Request.Files["Img"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = MyString.ToAscii(en.Title) + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    en.Img = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/library/post/"), filename);
                    file.SaveAs(Strpath);
                    en.Img = filename;
                }
                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/post/add";
                    var _url = _host + _api;
                    var postTask = client.PostAsJsonAsync<MPost>(_url, en);
                    postTask.Wait();
                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Đã thêm bài viết mới!", "success");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Notification.set_flash("Lỗi !" + result.ReasonPhrase, "warning");
                    }
                }
            }
            return View(en);
        }
        // Edit
        public ActionResult Edit(int? id)
        {
            MTopic mTopic = new MTopic();
            ViewBag.ListTopic = new SelectList(db.Topics.ToList(), "ID", "Name", 0);
            MPost en = db.Posts.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại bài viết!", "warning");
                return RedirectToAction("Index", "Post");
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
                String strSlug = MyString.ToAscii(en.Title);
                en.Slug = strSlug;
                en.Type = "post";
                en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
                var file = Request.Files["Img"];
                if (file != null && file.ContentLength > 0)
                {
                    String filename = MyString.ToAscii(en.Title) + file.FileName.Substring(file.FileName.LastIndexOf("."));
                    en.Img = filename;
                    String Strpath = Path.Combine(Server.MapPath("~/Public/library/post/"), filename);
                    file.SaveAs(Strpath);
                    en.Img = filename;
                }
                using (var client = new HttpClient())
                {
                    var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                    var _api = "/api/post/edit";
                    var _url = _host + _api;
                    var postTask = client.PutAsJsonAsync<MPost>(_url, en);
                    postTask.Wait();
                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        Notification.set_flash("Đã cập nhật lại bài viết!", "success");
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        Notification.set_flash("Lỗi !" + result.ReasonPhrase, "warning");
                    }
                }            
            }
            return View(en);
        }
        public ActionResult DelTrash(int id)
        {
            MPost en = db.Posts.Find(id);
            en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/post/delete";
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
                Notification.set_flash("Lỗi !"+result.ReasonPhrase, "warning");
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
        [HttpPost]
        public JsonResult changeStatus(int id)
        {
            MPost en = db.Posts.Find(id);
            en.Status = (en.Status == 1) ? 2 : 1;

            en.Updated_At = DateTime.Now;
            en.Updated_By = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { Status = en.Status });
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại bài viết!", "warning");
                return RedirectToAction("Index", "Post");
            }
            else
            {
                var client = new HttpClient();
                var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
                var _api = "/api/post/get?id=" + id;
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

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại bài viết!", "warning");
                return RedirectToAction("Index", "Post");
            }
            MPost en = db.Posts.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại bài viết!", "warning");
                return RedirectToAction("Index", "Post");
            }
            return View(en);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MPost en = db.Posts.Find(id);
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/post/delete?real_mode=true";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MPost>(_url, en);
            postTask.Wait();
            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Đã xóa vĩnh viễn", "danger");
            }
            else
            {
                var Code = (int)result.StatusCode;
                Notification.set_flash("Lỗi !", "warning");
            }            
            return RedirectToAction("Trash");
        }

    }
}
