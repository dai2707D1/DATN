using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ElectroShop.Models;

namespace ElectroShop.APIs
{
    [RoutePrefix("api/order")]
    public class Order_ApiController : ApiController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MOrder>> GetAll(int? status = null, int? customerID = null)
        {
            List<MOrder> list = new List<MOrder>();
            if (status == null && customerID == null)
            {
                list = db.Orders.ToList();
            }
            else
            if (status != null && customerID != null)
            {
                list = db.Orders.Where(x => x.CustemerId == customerID && x.Status == status).ToList();
            }
            else
            if ( status == null && customerID != null)
            {
                list = db.Orders.Where(x => x.CustemerId == customerID).ToList();
            }
            else
            if ( status != null && customerID == null)
            {
                list = db.Orders.Where(x => x.Status == status).ToList();
            }
            return Json(list);
        }

        [HttpGet,Route("Get")]
        public JsonResult<MOrder> GetByCode(string code)
        {
            var result = db.Orders.FirstOrDefault(x=>x.Code==code);
            return Json(result);
        }
        [HttpGet, Route("Get")]
        public JsonResult<MOrder> Get(int Id)
        {
            var u = db.Orders.FirstOrDefault(x => x.Id == Id);
            return Json(u);
        }

        [HttpGet,Route("GetDetails")]
        public JsonResult<List<MOrderdetail>> GetChild(int pid)
        {
            return Json(db.Orderdetails.Where(x => x.OrderId == pid).ToList());
        }

        [HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MOrder e)
        {
            e.Updated_at = DateTime.Now;    
            db.Orders.Add(e);
            var stt = db.SaveChanges() > 0;
            if (!stt)
            {
                jsonMessage.Status_Code = 204;
                jsonMessage.Message = "No content";
            }
            else
            {
                jsonMessage.Status_Code = 200;
                jsonMessage.Message = "Created successfully!";
                jsonMessage.Return_ID = e.Id;
            }
            return Json(jsonMessage);
        }

        [HttpPut, Route("Edit")]
        public JsonResult<JsonMessageModel> Edit(MOrder e)
        {
            var o = db.Orders.Find(e.Id);
            if (o != null)
            {
                PropertyInfo[] list = o.GetType().GetProperties();
                foreach (var item in list)
                {
                    var name = item.Name;
                    var _new = e.GetType().GetProperty(name).GetValue(e, null);
                    var _old = o.GetType().GetProperty(name).GetValue(o, null);
                    if (_new != null && !string.IsNullOrEmpty(_new.ToString()) && _old.ToString() != _new.ToString())
                    {
                        o.GetType().GetProperty(name).SetValue(o, _new, null);
                    }
                }
                o.Updated_at = DateTime.Now;
                jsonMessage.Status_Code = 200;
                jsonMessage.Return_ID = (int)o.Id;
            }
            else
            {
                jsonMessage.Status_Code = 404;
            }
            return Json(jsonMessage);
        }

        [HttpPost, Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(MOrder e, bool real_mode)
        {
            var o = db.Orders.FirstOrDefault(x => x.Id == e.Id);
            if (o != null)
            {
                if (real_mode)
                    db.Orders.Remove(o);
                else
                {
                    o.Updated_at = DateTime.Now;
                    o.Trash = 1;
                    if (e.Updated_at != o.Updated_at)
                    {
                        o.Updated_at = e.Updated_at;
                    }
                }
                db.SaveChanges();
                jsonMessage.Status_Code = 200;
                jsonMessage.Return_ID = o.Id;
            }
            else
            {
                jsonMessage.Status_Code = 404;
            }
            return Json(jsonMessage);
        }
    }
}