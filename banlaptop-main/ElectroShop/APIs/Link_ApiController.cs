using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ElectroShop.Models;

namespace ElectroShop.APIs
{
    [RoutePrefix("api/link")]
    public class Link_ApiController : ApiController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MLink>> GetAll(string type = "", int? tableID=null)
        {
            List<MLink> list = new List<MLink>();
            if (type=="" && tableID==null) 
            {
                list = db.Links.ToList();
            }
            else
            if (type != "" && tableID!=null)
            {
                list = db.Links.Where(x => x.Type == type && x.TableId==tableID).ToList();
            }
            else
            if (type == "" && tableID!=null)
            {
                list = db.Links.Where(x => x.TableId == tableID).ToList();
            }
            else
            if (type != "" && tableID==null)
            {
                list = db.Links.Where(x => x.Type == type).ToList();
            }

            return Json(list);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MLink> Get(int Id)
        {
            var u = db.Links.FirstOrDefault(x => x.Id == Id);
            return Json(u);
        }

        [HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MLink e)
        {
            db.Links.Add(e);
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
        public JsonResult<JsonMessageModel> Edit(MLink e)
        {
            var o = db.Links.Find(e.Id);
            if (o != null)
            {
                if (o.Name != e.Name)
                    o.Name = e.Name;
                if (o.Slug != e.Slug)
                    o.Slug = e.Slug;
                if (o.Type != e.Type)
                    o.Type = e.Type;
                if (o.TableId != e.TableId)
                    o.TableId = e.TableId;
                
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

        [HttpDelete, Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(int Id)
        {
            var o = db.Links.FirstOrDefault(x => x.Id == Id);
            if (o != null)
            {
                db.Links.Remove(o);
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