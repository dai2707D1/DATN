using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using ElectroShop.Models;

namespace ElectroShop.APIs
{
    
    [RoutePrefix("api/user")]
    public class User_ApiController : ApiController
    {
        // GET: Test
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();
        
        [HttpGet,Route("GetAll"),Route("")]
        public JsonResult<List<MUser>> GetAll(int? status=null, int? access=null)
        {
            List<MUser> list = new List<MUser>();

            if (status!=null && access!=null)
            {
                list = db.Users.Where(x => x.Status == status && x.Access == access).ToList();
            }
            else
            if(status!=null && access==null)
            {
                list = db.Users.Where(x=>x.Status==status).ToList();
            }
            else
            if(status==null && access!=null)
            {
                list = db.Users.Where(x=>x.Access==access).ToList();
            }
            else
            {
                list = db.Users.ToList();
            }    

            return Json(list);
        }
        
        [HttpGet,Route("Get")]
        public JsonResult<MUser> Get(int id)
        {
            var u = db.Users.FirstOrDefault(x=>x.ID==id);
            return Json(u);
        }

        [HttpGet,Route("GetByEmail")]
        public JsonResult<MUser> GetByEmail(string email)
        {
            var u =  db.Users.FirstOrDefault(x=>x.Email==email);
            return Json(u);
        }

        [HttpPost,Route("Add")]
        public JsonResult<JsonMessageModel> Add(MUser e)
        {
            if (!string.IsNullOrEmpty(e.Email))
            {
                var exist = db.Users.FirstOrDefault(x=>x.Email==e.Email);
                if (exist != null)
                {
                    jsonMessage.Status_Code = 202;
                    jsonMessage.Message = "This email is already in use!";
                }
                else
                {
                    db.Users.Add(e);
                    var stt = db.SaveChanges()>0;
                    if (!stt)
                    {
                        jsonMessage.Status_Code = 204;
                        jsonMessage.Message = "No content";
                    }
                    else
                    {
                        jsonMessage.Status_Code = 200;
                        jsonMessage.Message = "Created successfully!";
                        jsonMessage.Return_ID = e.ID;
                    }    
                    
                }    
            }
            else
            {
                jsonMessage.Status_Code = 400;
                jsonMessage.Message = "Invalid email!";
            }

            return Json(jsonMessage);
        }

        [HttpPut,Route("Edit")]
        public JsonResult<JsonMessageModel> Edit(MUser e)
        {
            var o = db.Users.Find(e.ID);
            if (o != null)
            {
                if (o.FullName != e.FullName)
                    o.FullName = e.FullName;
                if(o.Name!=e.Name)
                    o.Name = e.Name;
                if(o.Gender!=e.Gender)
                    o.Gender = e.Gender;
                if(o.Phone!=e.Phone)
                    o.Phone = e.Phone;
                if(o.Address!=e.Address)
                    o.Address = e.Address; 
                if(o.Access!=e.Access)
                    o.Access = e.Access;
                if(o.Status!=e.Status)
                    o.Status = e.Status;
                if(o.Created_by!=e.Created_by)
                    o.Created_by = e.Created_by;
                if(o.Updated_by!=e.Updated_by)
                    o.Updated_by = e.Updated_by;
                if(o.Image!=e.Image)
                    o.Image = e.Image;
                if(e.Password!=o.Password)
                {
                    o.Password = e.Password;
                }    
                o.Updated_at = DateTime.Now;

                var exist = db.Users.FirstOrDefault(x=>x.Email==e.Email);
                if (exist != null)
                {
                    jsonMessage.Message = "This email is in use!";
                    jsonMessage.Status_Code = 202;
                }
                else
                {
                    db.SaveChanges();

                    jsonMessage.Status_Code = 200;
                    jsonMessage.Return_ID = o.ID;
                }
            }    
                
            else
            {
                jsonMessage.Status_Code = 404;

            }    
            return Json(jsonMessage);
        }

        [HttpDelete,Route("Delete")]
        public JsonResult<JsonMessageModel> Delete(int id)
        {
            var o = db.Users.FirstOrDefault(x=>x.ID==id);
            if (o != null)
            {
                db.Users.Remove(o);
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


        [HttpPost,Route("ChangePassword")]
        public JsonResult<JsonMessageModel> ChangePassword(int id, string password)
        {
            var u = db.Users.FirstOrDefault(x => x.ID == id);
            if (u != null)
            {
                if (!string.IsNullOrEmpty(password))
                {
                    u.Password = MyString.ToMD5(password);
                    db.SaveChanges();

                    jsonMessage.Status = "OK";
                    jsonMessage.Return_ID = u.ID;
                    jsonMessage.Status_Code = 200;
                    jsonMessage.Message = "Changed password successfully!";
                }
                else
                {
                    jsonMessage.Message = "Password is empty!";
                    jsonMessage.Status_Code = 400;
                }    
            }
            else
            {
                jsonMessage.Message = "Not found!";
                jsonMessage.Status_Code = 404;
            }    
           
            return Json(jsonMessage);

        }

        [HttpPost, Route("Login")]
        public JsonResult<JsonMessageModel> Login(string username,string password)
        {
            jsonMessage.Status_Code = 404;
            password = MyString.ToMD5(password);
            var o = db.Users.FirstOrDefault(x=>x.Email== username || x.Phone.ToString()== username);
            if (o != null)
            {
                if (o.Status == 0)
                {
                    jsonMessage.Message = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ quản trị viên!";
                }
                if (password == o.Password)
                {
                    jsonMessage.Status_Code = 200;
                    jsonMessage.Return_ID =o.ID;
                    jsonMessage.Message = "Đăng nhập thành công!";
                }
                else
                {
                    jsonMessage.Message = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }    
            }
            else
            {
                jsonMessage.Message = "Tài khoản không tồn tại!";
            }
            return Json(jsonMessage);
        }

        [HttpGet, Route("Login")]
        public JsonResult<JsonMessageModel> GEtLogin(string username, string password)
        {
            jsonMessage.Status_Code = 404;
            password = MyString.ToMD5(password);
            var o = db.Users.FirstOrDefault(x => x.Email == username || x.Name==username || x.Phone.ToString() == username);
            if (o != null)
            {             
                if (password == o.Password)
                {
                    if (o.Status == 0)
                    {
                        jsonMessage.Message = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ quản trị viên!";
                    }
                    else
                    {
                        jsonMessage.Status_Code = 200;
                        jsonMessage.Return_ID = o.ID;
                        jsonMessage.Message = "Đăng nhập thành công!";
                        
                    }              
                }
                else
                {
                    jsonMessage.Message = "Tên đăng nhập hoặc mật khẩu không đúng!";
                }
            }
            else
            {
                jsonMessage.Message = "Tài khoản không tồn tại!";
            }
            return Json(jsonMessage);
        }

    }
}