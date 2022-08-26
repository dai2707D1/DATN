
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ElectroShop.Models;

namespace ElectroShop.APIs
{
    [RoutePrefix("api/category")]
    public class Category_ApiController : ApiController
    {

        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MCategory>> GetAll(int? status=null, int? parrentId=null)
        {
            List<MCategory> list = new List<MCategory>();

            if (status != null && parrentId != null)
            {
                list = db.Categorys.Where(x => x.Status == status && x.ParentId == parrentId).ToList();
            }
            else
            if (status != null && parrentId == null)
            {
                list = db.Categorys.Where(x => x.Status == status).ToList();
            }
            else
            if (status == null && parrentId != null)
            {
                list = db.Categorys.Where(x => x.ParentId == parrentId).ToList();
            }
            else
            {
                list = db.Categorys.ToList();
            }    

            return Json(list);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MCategory> Get(int Id)
        {
            var u = db.Categorys.FirstOrDefault(x => x.Id == Id);
            return Json(u);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MCategory> GetBySlug(string slug)
        {
            var u = db.Categorys.FirstOrDefault(x => x.Slug == slug);
            return Json(u);
        }

        [HttpGet, Route("Get")]
        public JsonResult<List<MCategory>> GetChild(int ParentID)
        {
            var u = db.Categorys.Where(x => x.ParentId == ParentID).ToList();
            return Json(u);
        }

        [HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MCategory e)
        {
            String strSlug = MyString.ToAscii(e.Name);
            e.Slug = strSlug;
            e.Created_at = DateTime.Now;
            e.Updated_at = DateTime.Now;
            db.Categorys.Add(e);
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
        public JsonResult<JsonMessageModel> Edit(MCategory e)
        {
            var o = db.Categorys.Find(e.Id);
            if (o != null)
            {

                if (o.Name != e.Name)
                    o.Name = e.Name;
                o.Slug = MyString.ToAscii(o.Name);
                if (o.Status != e.Status)
                    o.Status = e.Status;
                if (o.Created_by != e.Created_by)
                    o.Created_by = e.Created_by;
                if (o.Updated_by != e.Updated_by)
                    o.Updated_by = e.Updated_by;
                o.Updated_at = DateTime.Now;
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

        [HttpPost, Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(MCategory e, bool real_mode = false)
        {
            var o = db.Categorys.FirstOrDefault(x => x.Id == e.Id);
            if (o != null)
            {
                if (real_mode)
                    db.Categorys.Remove(o);
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