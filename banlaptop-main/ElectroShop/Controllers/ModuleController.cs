using ElectroShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace ElectroShop.Controllers
{
    public class ModuleController : Controller
    {
        private ElectroShopDbContext db = new ElectroShopDbContext();
        // GET: Module
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Header()
        {
            return View("_Header");
        }
        public ActionResult CategorySearch()
        {
            var list = db.Categorys.ToList(); 
            return View("_CategorySearch",list);
        }
        public ActionResult Cart()
        {
            return View("_Cart");
        }
        public ActionResult Navbar()
        {
            return View("_Navbar");
        }
        public ActionResult ProductView()
        {
            return View("_ProductView");
        }
        public ActionResult LatestNew()
        {
            var list = new List<MPost>();

            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Post", new { httproute = "DefaultApi", status = 1, type = "post" });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MPost>>();
                readTask.Wait();
                list = readTask.Result;
            }
            return View("_LatestNew", list.Take(3));
        }
        public ActionResult Brand()
        {
            return View("_Brand");
        }
        public ActionResult CompanyFacality()
        {
            return View("_CompanyFacality");
        }
        public ActionResult Footer()
        {
            return View("_Footer");
        }
        public ActionResult Copyright()
        {
            return View("_Copyright");
        }
        public ActionResult LinkHeader()
        {
            return View("_LinkHeader");
        }
        public ActionResult LinkFooter()
        {
            return View("_LinkFooter");
        }
        public ActionResult Category()
        {
            var list = new List<MCategory>();

            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Category", new { httproute = "DefaultApi", status=1, parrentid=0 });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<IList<MCategory>>();
                readTask.Wait();
                list = (List<MCategory>)readTask.Result;
            }
            return View("_Category", list);
        }
        public ActionResult CategoryFooter()
        {
            var list = new List<MCategory>();
            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Category", new { httproute = "DefaultApi", status = 1, parrentid = 0 });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<IList<MCategory>>();
                readTask.Wait();

                list = (List<MCategory>)readTask.Result;
            }

            return View("_CategoryFooter", list);
        }
        public ActionResult ProductNew()
        {
            var list = new List<MProduct>();

            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Product", new { httproute = "DefaultApi", status = 1 });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MProduct>>();
                readTask.Wait();
                list = readTask.Result;
            }
            return View("_ProductNew", list.Where(x => x.Discount != 0).Take(4));
        }

        public ActionResult Sale()
        {
            var list = new List<MProduct>();

            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Product", new { httproute = "DefaultApi", status = 1 });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MProduct>>();
                readTask.Wait();
                list = readTask.Result;
            }
            return View("_Sale", list.OrderByDescending(x => x.Created_at).Take(3));
        }
        public ActionResult SlideShow()
        {

            return View("_SlideShow",db.Sliders.Where(m=>m.Status!=0).ToList());
        }
        public ActionResult ListPage()
        {

            var list = new List<MPost>();

            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Post", new { httproute = "DefaultApi", status = 1,type="page" });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MPost>>();
                readTask.Wait();
                list = readTask.Result;
            }
            return View("_ListPage",list.OrderByDescending(m => m.Created_At));
        }
        public ActionResult MainMenu()
        {
            var list = db.Menus
              .Where(m => m.Status == 1 && m.Position=="mainmenu")
              .ToList();
            return View("_MainMenu", list);
        }
        public ActionResult MainMenuMobile()
        {
            var list = db.Menus
              .Where(m => m.Status == 1 && m.Type == "custom")
              .ToList();
            return View("_MainMenuMobile", list);
        }
        public ActionResult ListCate()
        {
            var list = new List<MCategory>(); 
            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Category", new { httproute = "DefaultApi", parrentId=0});
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MCategory>>();
                readTask.Wait();

                list = (List<MCategory>)readTask.Result;

            }
            return View("_ListCate", list.Where(x => x.Status != 0));
        }
        public ActionResult Posts()
        {
            var list = new List<MTopic>();
            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Topic", new { httproute = "DefaultApi", status=1, parrentId = 0 });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MTopic>>();
                readTask.Wait();
                list = readTask.Result;
            }
            return View("Posts", list);
        }
        public ActionResult PostHome(int topid)
        {
            List<int> listtopid = new List<int>();
            listtopid.Add(topid);

            var list2 = db.Topics
                .Where(m => m.ParentId == topid).Select(m => m.Id)
                .ToList();
            foreach (var id2 in list2)
            {
                listtopid.Add(id2);
                var list3 = db.Topics
                    .Where(m => m.ParentId == id2)
                    .Select(m => m.Id).ToList();
                foreach (var id3 in list3)
                {
                    listtopid.Add(id3);
                }
            }

            var list = db.Posts
                .Where(m => m.Status == 1 && listtopid
                .Contains(m.Topid))
                .Take(12)
                .OrderByDescending(m => m.Created_At);

            return View("PostHome", list);
        }
        public ActionResult ListTopic()
        {
            var list = new List<MTopic>();
            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Topic", new { httproute = "DefaultApi",  parrentId = 0 });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MTopic>>();
                readTask.Wait();
                list = (List<MTopic>)readTask.Result;
            }
            return View("ListTopic",list.Where(x => x.Status != 0));
        }
        public ActionResult PListPage()
        {
            var list = new List<MPost>();

            var client = new HttpClient();
            var _api = Url.Action("GetAll", "Post", new { httproute = "DefaultApi", status = 1, type = "page" });
            var _url = Request.Url.Scheme + "://" + Request.Url.Authority + _api;

            var responseTask = client.GetAsync(_url);
            responseTask.Wait();
            var result = responseTask.Result;
            if (result.IsSuccessStatusCode)
            {
                var readTask = result.Content.ReadAsAsync<List<MPost>>();
                readTask.Wait();
                list = readTask.Result;
            }
            return View("PListPage", list);
        }
    }
}