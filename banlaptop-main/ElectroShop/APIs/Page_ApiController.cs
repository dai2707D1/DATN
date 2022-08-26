using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.UI;
using ElectroShop.Models;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using RouteAttribute = System.Web.Http.RouteAttribute;

namespace ElectroShop.APIs
{

    [RoutePrefix("api/page")]
    public class Page_ApiController : ApiController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MPost>> GetAll(int? status = null, int? Topid = null, string type = "page")
        {
            List<MPost> list = new List<MPost>();
            if (status == null && Topid == null && type == "")
            {
                list = db.Posts.ToList();
            }
            else
            if (status != null && Topid != null && type != "")
            {
                list = db.Posts.Where(x => x.Status == status && x.Topid == Topid && x.Type == "page").ToList();
            }
            else
            if (status == null && Topid != null && type == "")
            {
                list = db.Posts.Where(x => x.Topid == Topid).ToList();
            }
            else
            if (status == null && Topid == null && type != "")
            {
                list = db.Posts.Where(x => x.Type == "page").ToList();
            }
            else
            {
                list = db.Posts.Where(x => x.Status == status).ToList();
            }
            return Json(list);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MPost> Get(int Id)
        {
            var u = db.Posts.FirstOrDefault(x => x.Id == Id && x.Type.ToLower()=="page");
            return Json(u);
        }

        [System.Web.Http.HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MPost e)
        {
            e.Slug = MyString.ToAscii(e.Title);
            e.Type = "page";
            e.Updated_At = DateTime.Now;
            e.Created_At = DateTime.Now;
            db.Posts.Add(e);
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

        [System.Web.Http.HttpPut, Route("Edit")]
        public JsonResult<JsonMessageModel> Edit(MPost e)
        {
            var o = db.Posts.FirstOrDefault(x => x.Id == e.Id && x.Type.ToLower() == "page");
            if (o != null)
            {             
                PropertyInfo[] list = o.GetType().GetProperties();
                foreach(var item in list)
                {
                    var name = item.Name;
                    var _new = e.GetType().GetProperty(name).GetValue(e,null);
                    var _old = o.GetType().GetProperty(name).GetValue(o, null);
                    if (_new!=null && !string.IsNullOrEmpty(_new.ToString()) && _old.ToString() != _new.ToString())
                    {
                        o.GetType().GetProperty(name).SetValue(o, _new, null);
                    }                   
                }
                o.Slug = MyString.ToAscii(o.Title);
                o.Updated_At = DateTime.Now;
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


        [System.Web.Http.HttpPost, Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(MPost e, bool real_mode = false)
        {
            var o = db.Posts.FirstOrDefault(x => x.Id == e.Id && x.Type.ToLower() == "page");
            if (o != null)
            {
                if (real_mode)
                    db.Posts.Remove(o);
                else
                {
                    o.Updated_At = DateTime.Now;
                    o.Status = 0;
                    if (e.Updated_By != o.Updated_By)
                    {
                        o.Updated_By = e.Updated_By;
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