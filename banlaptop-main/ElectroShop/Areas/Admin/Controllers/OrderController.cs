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
    public class OrderController : BaseController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();

        public ActionResult Index()
        {
            ViewBag.countTrash = db.Orders.Where(m => m.Trash == 1).Count();
            var results = (from od in db.Orderdetails
                           join o in db.Orders on od.OrderId equals o.Id
                           where o.Trash != 1

                           group od by new { od.OrderId, o } into groupb
                           orderby groupb.Key.o.CreateDate descending
                           select new ListOrder
                           {
                               ID = groupb.Key.OrderId,
                               SAmount = groupb.Sum(m => m.Amount),
                               CustomerName = groupb.Key.o.DeliveryName,
                               Status = groupb.Key.o.Status,
                               CreateDate = groupb.Key.o.CreateDate,
                               ExportDate = groupb.Key.o.ExportDate,
                           });

            return View(results.ToList());
        }
        public ActionResult Trash()
        {
            ViewBag.countTrash = db.Orders.Where(m => m.Status == 0).Count();
            var results = (from od in db.Orderdetails
                           join o in db.Orders on od.OrderId equals o.Id
                           where o.Trash == 1

                           group od by new { od.OrderId, o } into groupb
                           orderby groupb.Key.o.CreateDate descending
                           select new ListOrder
                           {
                               ID = groupb.Key.OrderId,
                               SAmount = groupb.Sum(m => m.Amount),
                               CustomerName = groupb.Key.o.DeliveryName,
                               CustomerAddress = groupb.Key.o.DeliveryAddress,
                               CustomerEmail = groupb.Key.o.DeliveryEmail,
                               Status = groupb.Key.o.Status,
                               CreateDate = groupb.Key.o.CreateDate,
                               ExportDate = groupb.Key.o.ExportDate,
                           });

            return View(results.ToList());
        }

        public ActionResult DelTrash(int? id)
        {
            MOrder en = db.Orders.Find(id);
            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/order/delete";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MOrder>(_url, en);
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
            Notification.set_flash("Đã hủy đơn hàng!" + " ID = " + id, "success");
            return RedirectToAction("Index");
        }
        public ActionResult Undo(int? id)
        {
            MOrder en = db.Orders.Find(id);
            en.Trash = 0;

            en.Updated_at = DateTime.Now;
            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            Notification.set_flash("Khôi phục thành công!" + " ID = " + id, "success");
            return RedirectToAction("Trash");
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại đơn hàng!", "warning");
            }

            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/order/get?id=" + id;
            var _url = _host + _api;
            var postTask = client.GetAsync(_url);
            postTask.Wait();

            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var r = result.Content.ReadAsAsync<MOrder>();
                r.Wait();
                var en = r.Result;
                ViewBag.orderDetails = db.Orderdetails.Where(m => m.OrderId == id).ToList();
                ViewBag.productOrder = db.Products.ToList();
                return View(en);
            }
            else
            {
                Notification.set_flash("Lỗi truy xuẩt dữ liệu! \n " + result.ReasonPhrase, "warning");
               
            }
            return RedirectToAction("Index", "Post");
        }


        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                Notification.set_flash("Không tồn tại đơn hàng!", "warning");
                return RedirectToAction("Trash", "Order");
            }
            MOrder en = db.Orders.Find(id);
            if (en == null)
            {
                Notification.set_flash("Không tồn tại đơn hàng!", "warning");
                return RedirectToAction("Trash", "Order");
            }
            ViewBag.orderDetails = db.Orderdetails.Where(m => m.OrderId == id).ToList();
            ViewBag.productOrder = db.Products.ToList();
            return View(en);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            MOrder en = db.Orders.Find(id);
            var client = new HttpClient();
            var _host = Request.Url.Scheme + "://" + Request.Url.Authority;
            var _api = "/api/order/delete?real_mode=true";
            var _url = _host + _api;
            var postTask = client.PostAsJsonAsync<MOrder>(_url, en);
            postTask.Wait();
            var result = postTask.Result;
            if (result.IsSuccessStatusCode)
            {
                Notification.set_flash("Đã xóa đơn hàng!", "success");
            }
            else
            {
                var Code = (int)result.StatusCode;
                Notification.set_flash("Lỗi !", "warning");
            }
            return RedirectToAction("Trash");
        }
        [HttpPost]
        public JsonResult changeStatus(int id, int op)
        {
            MOrder en = db.Orders.Find(id);
            if (op == 1) { en.Status = 1; } else if (op == 2) { en.Status = 2; } else { en.Status = 3; }

            en.ExportDate = DateTime.Now;
            en.Updated_at = DateTime.Now;
            en.Updated_by = int.Parse(Session["Admin_ID"].ToString());
            db.Entry(en).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { s = en.Status, t = en.ExportDate.ToString() });
        }


    }
}
