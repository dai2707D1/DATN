using ElectroShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace ElectroShop.APIs
{
    [RoutePrefix("api/account")]
    public class API_AccountController : ApiController
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        private JsonMessageModel jsonMessage = new JsonMessageModel();
        [HttpPost, Route("ChangePassword")]
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

    }
}
