using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class Price_VMController : Controller
    {

        private QLVMDataContext _db = new QLVMDataContext();

        public ActionResult Index()
        {

            List<PRICE_VM> list_price = _db.PRICE_VMs.OrderByDescending(x  => x.ID).ToList();
            PRICE_VM cur_price = list_price.FirstOrDefault();
            List<PRICE_VM> list_price_his = list_price.Where(x => x.ID != cur_price.ID).ToList();

            ViewBag.Cur_Price = cur_price;
            ViewBag.List_Price = list_price_his;
            
            return View();
        }

        [HttpPost]
        public ActionResult Index(FormCollection fc)
        {
            PRICE_VM price = new PRICE_VM();
            price.CPU = fc["CPUpri"];
            price.RAM = fc["RAMpri"];
            price.DISK_FAST = fc["Fastpri"];
            price.DISK_SLOW = fc["Slowpri"];
            _db.PRICE_VMs.InsertOnSubmit(price);
            _db.SubmitChanges();

            return RedirectToAction("Index");
        }

    }
}