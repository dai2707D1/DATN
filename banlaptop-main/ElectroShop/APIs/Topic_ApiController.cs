using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ElectroShop.Models;

namespace ElectroShop.APIs
{
    [RoutePrefix("api/topic")]
    public class Topic_ApiController : ApiController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MTopic>> GetAll(int? status = null, int? parrentID = null)
        {
            List<MTopic> list = new List<MTopic>();
            if (status == null && parrentID == null)
            {
                list = db.Topics.ToList();
            }
            else
            if (status != null && parrentID != null)
            {
                list = db.Topics.Where(x => x.Status == status && x.ParentId == parrentID).ToList();
            }
            else
            if ( status == null && parrentID != null)
            {
                list = db.Topics.Where(x => x.ParentId == parrentID).ToList();
            }          
            return Json(list);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MTopic> Get(int Id)
        {
            var u = db.Topics.FirstOrDefault(x => x.Id == Id);
            return Json(u);
        }


        [HttpGet, Route("Get")]
        public JsonResult<MProduct> GetBySlug(string slug)
        {
            var u = db.Products.FirstOrDefault(x => x.Slug == slug);
            return Json(u);
        }

        [HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MTopic e)
        {
            e.Slug = MyString.ToAscii(e.Name);
            db.Topics.Add(e);
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
        public JsonResult<JsonMessageModel> Edit(MTopic e)
        {
            var o = db.Topics.Find(e.Id);
            if (o != null)
            {
                if (o.Name != e.Name)
                    o.Name = e.Name;
                o.Slug = MyString.ToAscii(o.Name);
                if (o.Status != e.Status)
                    o.Status = e.Status;
                if (o.ParentId != e.ParentId)
                    o.ParentId = e.ParentId;
                if(o.Orders!=e.Orders)
                    o.Orders = e.Orders;
                if(o.Metakey!=e.Metakey)
                    o.Metakey = e.Metakey;
                if(o.Metadesc!=e.Metadesc)
                    o.Metadesc = e.Metadesc;
                if(o.Created_by!=e.Created_by)
                    o.Created_by = e.Created_by;
                if(o.Updated_by!=e.Updated_by)
                    o.Updated_by = e.Updated_by;
                o.Updated_at = DateTime.Now;

                db.SaveChanges();

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
        public JsonResult<JsonMessageModel> Delete(MTopic e, bool real_mode=false)
        {
            var o = db.Topics.FirstOrDefault(x => x.Id == e.Id);
            if (o != null)
            {
                if (real_mode)
                    db.Topics.Remove(o);
                else
                {
                    o.Updated_at = DateTime.Now;
                    o.Status = 0;
                    if (e.Updated_by != o.Updated_by)
                    {
                        o.Updated_by = e.Updated_by;
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