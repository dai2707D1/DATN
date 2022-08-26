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
    [RoutePrefix("api/product")]
    
    public class Product_ApiController : ApiController
    {

        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MProduct>> GetAll(int? status=null, int? cateID=null)
        {
            List<MProduct> list = new List<MProduct>();
            if(status!=null && cateID != null)
            {
                list = db.Products.Where(x=>x.Status==status && x.CateID==cateID).ToList();
            }
            else
            if (status==null && cateID!=null)
            {
                list = db.Products.Where(x => x.CateID == cateID).ToList();
            }
            else
            if(cateID==null && status!=null)
            {
                list = db.Products.Where(x=>x.Status==status).ToList();
            }    
            else
            {
                list = db.Products.ToList();
            }    
            return Json(list);
        }

        [HttpGet,Route("Get")]
        public JsonResult<MProduct> GetById(int id)
        {
            var u = db.Products.FirstOrDefault(x => x.ID == id);
            return Json(u);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MProduct> GetBySlug(string slug)
        {
            var u = db.Products.FirstOrDefault(x => x.Slug == slug);
            return Json(u);
        }

        [HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MProduct e)
        {
            String strSlug = MyString.ToAscii(e.Name);
            e.Slug = strSlug;
            e.Created_at = DateTime.Now;
            e.Updated_at = DateTime.Now;
            db.Products.Add(e);
            var stt = db.SaveChanges() > 0;
            if (!stt)
            {
                jsonMessage.Status_Code = 204;
            }
            else
            {
                jsonMessage.Status_Code = 200;
                jsonMessage.Return_ID = e.ID;
            }
            return Json(jsonMessage);
        }

        [HttpPut, Route("Edit")]
        public JsonResult<JsonMessageModel> Edit(MProduct e)
        {
            var o = db.Products.Find(e.ID);
            if (o != null)
            {
                PropertyInfo[] list = o.GetType().GetProperties();
                foreach (var item in list)
                {
                    var name = item.Name;
                    var _new = e.GetType().GetProperty(name).GetValue(e, null);
                    var _old = o.GetType().GetProperty(name).GetValue(o, null);
                    if (_new != null && _old.ToString() != _new.ToString())
                    {
                        o.GetType().GetProperty(name).SetValue(o, _new, null);
                    }
                }
                o.Slug = MyString.ToAscii(o.Name);
                o.Updated_at = DateTime.Now;

                    db.SaveChanges();

                    jsonMessage.Status_Code = 200;
                    jsonMessage.Return_ID = o.ID;
            }
            else
            {
                jsonMessage.Status_Code = 404;

            }
            return Json(jsonMessage);
        }

        [HttpPost, Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(MProduct e, bool real_mode=false)
        {
            var o = db.Products.FirstOrDefault(x => x.ID == e.ID);
            if (o != null)
            {
                if (real_mode)
                    db.Products.Remove(o);
                else
                {
                    o.Updated_at = DateTime.Now;
                    o.Status = 0;
                    if (e.Updated_by!=o.Updated_by){
                        o.Updated_by = e.Updated_by;
                    }                    
                }    
                    
                db.SaveChanges();

                jsonMessage.Status_Code = 200;
                jsonMessage.Return_ID = o.ID;

            }
            else
            {
                jsonMessage.Status_Code = 404;
            }
            return Json(jsonMessage);
        }
    }
}