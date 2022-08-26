using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ElectroShop.Models;

namespace ElectroShop.APIs
{
    [RoutePrefix("api/post")]
    public class Post_ApiController : ApiController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();

        [HttpGet, Route("GetAll"), Route("")]
        public JsonResult<List<MPost>> GetAll(int? status = null, int? Topid = null, string type="post")
        {
            List<MPost> list = new List<MPost>();
            if (status == null && Topid == null && type=="")
            {
                list = db.Posts.ToList();
            }
            else
            if (status != null && Topid != null && type!="")
            {
                list = db.Posts.Where(x => x.Status == status && x.Topid == Topid && x.Type==type).ToList();
            }
            else
            if (status == null && Topid != null && type=="")
            {
                list = db.Posts.Where(x => x.Topid == Topid).ToList();
            }
            else
            if(status==null && Topid==null && type!="")
            {
                list = db.Posts.Where(x => x.Type == type).ToList();
            }   
            else
            {
                list = db.Posts.Where(x=>x.Status==status).ToList();
            }
            return Json(list);
        }

        [HttpGet, Route("Get")]
        public JsonResult<MPost> Get(int Id)
        {
            var u = db.Posts.FirstOrDefault(x => x.Id == Id && x.Type.ToLower() == "post");
            return Json(u);
        }

        [HttpPost, Route("Add")]
        public JsonResult<JsonMessageModel> Add(MPost e)
        {
            e.Slug = MyString.ToAscii(e.Title);
            e.Updated_At = DateTime.Now;
            e.Type = "post";
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

        [HttpPut, Route("Edit")]
        public JsonResult<JsonMessageModel> Edit(MPost e)
        {
            var o = db.Posts.FirstOrDefault(x => x.Id == e.Id && x.Type.ToLower() == "post");
            if (o != null)
            {
                e.Type = "post";
                if (o.Title != e.Title)
                o.Title = e.Title;
                o.Slug = MyString.ToAscii(e.Title);
            if (o.Status != e.Status)
                o.Status = e.Status;
            if (o.Topid != e.Topid)
                o.Topid = e.Topid;
            if (o.Detail != e.Detail)
                o.Detail = e.Detail;
            if (o.Img != e.Img)
                o.Img = e.Img;
            if (o.MetaKey != e.MetaKey)
                o.MetaKey = e.MetaKey;
            if (o.MetaDesc != e.MetaDesc)
                o.MetaDesc = e.MetaDesc;
            if (o.Created_By != e.Created_By)
                o.Created_By = e.Created_By;
            if (o.Updated_By != e.Updated_By)
                o.Updated_By = e.Updated_By;
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

        [HttpPost, Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(MPost e, bool real_mode=false)
        {
            var o = db.Posts.FirstOrDefault(x => x.Id == e.Id && x.Type.ToLower() == "post");
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