using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class OS_VMController : Controller
    {
        private QLVMDataContext _db = new QLVMDataContext();
        // GET: OS_VM
        public ActionResult Index(string sortOrder)
        {
            ViewBag.WindowSortParm = String.IsNullOrEmpty(sortOrder) ? "Window" : "Window";
            ViewBag.LinuxSortParm = String.IsNullOrEmpty(sortOrder) ? "Linux" : "Linux";
            ViewBag.UNIXSortParm = String.IsNullOrEmpty(sortOrder) ? "Unix" : "Unix";
            ViewBag.OtherSortParm = String.IsNullOrEmpty(sortOrder) ? "Other" : "Other";
            ViewBag.AllSortParm = String.IsNullOrEmpty(sortOrder) ? "All" : "All";

            List<OS_VM> list_os_vm = _db.OS_VMs.ToList();

            switch (sortOrder)
            {
                case "Window":
                    list_os_vm = list_os_vm.Where(x => x.TYPE_OS.Equals("Windows")).OrderByDescending(s => s.ID).ToList();
                    break;
                case "Linux":
                    list_os_vm = list_os_vm.Where(x => x.TYPE_OS.Equals("Linux")).OrderByDescending(s => s.ID).ToList();
                    break;
                case "UNIX":
                    list_os_vm = list_os_vm.Where(x => x.TYPE_OS.Equals("Unix")).OrderByDescending(s => s.ID).ToList();
                    break;
                case "Other":
                    list_os_vm = list_os_vm.Where(x => x.TYPE_OS.Equals("Other")).OrderByDescending(s => s.ID).ToList();
                    break;
                case "All":
                    list_os_vm = list_os_vm.OrderByDescending(s => s.ID).ToList();
                    break;
                default:
                    list_os_vm = list_os_vm.OrderByDescending(s => s.ID).ToList();
                    break;
            }

            return View(list_os_vm);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection fc)
        {
            OS_VM os = new OS_VM();
            os.TYPE_OS = fc["type_os"];
            os.NAME_OS = fc["name_os"];
            _db.OS_VMs.InsertOnSubmit(os);
            _db.SubmitChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Detail(int ID)
        {
            OS_VM os = _db.OS_VMs.FirstOrDefault(x => x.ID == ID);
            string[] os_list = {"Windows", "Linux", "Unix", "Khác"};
            ViewBag.OS = os;
            ViewBag.OS_list = os_list;
            return View();
        }
    }
}