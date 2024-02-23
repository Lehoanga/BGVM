using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using PagedList;
using System.Net.Sockets;
using System.Web.Configuration;

namespace WebApplication1.Controllers
{
    public class Ticket_VMController : Controller
    {
        // GET: Ticket_VM

        private QLVMDataContext _db = new QLVMDataContext();

        public ActionResult Index(string currentFilter, string searchString, string SearchDateStart, string SearchDateEnd, int? page)
        {
            List<TICKET_VM> list = _db.TICKET_VMs.OrderByDescending(x => x.ID).ToList();
            if (searchString != null && SearchDateStart != null && SearchDateEnd != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;
            ViewBag.CurrentDateStart = SearchDateStart;
            ViewBag.SearchDateEnd = SearchDateEnd;

            if (!String.IsNullOrEmpty(searchString))
            {
                list = list.Where(s => s.TICKETBA.ToString().ToLower().Contains(searchString.Trim().ToLower())
                                       || s.LOCATION.ToString().ToLower().Contains(searchString.Trim().ToLower())
                                       || s.OWNER.ToLower().Contains(searchString.Trim().ToLower())
                                       ).ToList();
            }
            if (!String.IsNullOrEmpty(SearchDateStart))
            {
                list = list.Where(s => s.STARTDATE >= DateTime.Parse(SearchDateStart)).ToList();
            }
            if (!String.IsNullOrEmpty(SearchDateEnd))
            {
                list = list.Where(s => s.STARTDATE <= DateTime.Parse(SearchDateEnd)).ToList();
            }


            int pageSize = 15;
            int pageNumber = (page ?? 1);
            
            return View(list.ToPagedList(pageNumber, pageSize));
        }


        public ActionResult Create()
        {
            var list_os = _db.OS_VMs.Select(x => new {x.ID, x.NAME_OS }).ToList();
            ViewBag.OS = list_os;
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection fc)
        {
            try
            {
                List<PRICE_VM> list_price = _db.PRICE_VMs.OrderByDescending(x => x.ID).ToList();
                PRICE_VM cur_price = list_price.FirstOrDefault();

                List<string> NAME = fc["VM-ID"].Split(',').ToList();
                List<string> CPU = fc["CPU"].Split(',').ToList();
                List<string> RAM = fc["RAM"].Split(',').ToList();
                List<string> OS = fc["OS"].Split(',').ToList();
                List<string> FAST = fc["FAST"].Split(',').ToList();
                List<string> SLOW = fc["SLOW"].Split(',').ToList();
                for (int i = 0; i < FAST.Count; i++)
                {
                    if (FAST[i]=="")
                    {
                        FAST[i] = "0";
                    }
                }
                for (int i = 0; i < SLOW.Count; i++)
                {
                    if (SLOW[i]=="")
                    {
                        SLOW[i] = "0";
                    }
                }

                TICKET_VM tic = new TICKET_VM();                
                tic.STARTDATE = DateTime.Parse(fc["startDate"]);
                tic.ENDDATE = DateTime.Parse(fc["endDate"]);
                tic.OWNER = fc["owner_VM"];
                tic.TICKETBA = fc["ticket"];
                tic.LOCATION = fc["loaction_VM"];
                tic.ID_PRICE = cur_price.ID;
                _db.TICKET_VMs.InsertOnSubmit(tic);
                _db.SubmitChanges();

                int daterange = (int)(tic.ENDDATE - tic.STARTDATE).Value.TotalDays;
                int Price = 0;

                for (int i = 0; i < CPU.Count(); i++)
                {
                    CONFIG_VM conf = new CONFIG_VM();
                    conf.ID_TICKET = tic.ID;
                    conf.VM_ID = NAME[i];
                    conf.CPU = int.Parse(CPU[i]);
                    conf.RAM = int.Parse(RAM[i]);
                    conf.DISK_FAST = int.Parse(FAST[i]);
                    conf.DISK_SLOW = int.Parse(SLOW[i]);
                    conf.ID_OS = int.Parse(OS[i]);
                    conf.PRICE = GET_TOTAL((int)conf.RAM, (int)conf.CPU, (int)conf.DISK_FAST, (int)conf.DISK_SLOW, (int)tic.ID_PRICE, daterange);
                    _db.CONFIG_VMs.InsertOnSubmit(conf);
                    _db.SubmitChanges();
                    Price = Price + int.Parse(conf.PRICE.Replace(".", ""));
                }

                tic.TOTAL_PRICE = GET_DOT(Price.ToString());
                _db.SubmitChanges();

            }
            catch
            {

            }

            return RedirectToAction("Index");
        }

        public ActionResult Detail(int ID)
        {
            var list_os = _db.OS_VMs.Select(x => new { x.ID, x.NAME_OS }).ToList();
            ViewBag.OS = list_os;

            TICKET_VM tic = _db.TICKET_VMs.FirstOrDefault(x => x.ID == ID);
            ViewBag.tic = tic;

            List<CONFIG_VM> list = _db.CONFIG_VMs.Where(x => x.ID_TICKET == tic.ID).ToList();
            ViewBag.list = list;

            List<PRICE_VM> list_price = _db.PRICE_VMs.OrderByDescending(x => x.ID).ToList();
            PRICE_VM cur_price = list_price.FirstOrDefault();
            List<PRICE_VM> list_price_his = list_price.Where(x => x.ID != cur_price.ID).ToList();
            PRICE_VM tic_price = _db.PRICE_VMs.FirstOrDefault(x => x.ID == tic.ID_PRICE);
            List<OS_VM> os = _db.OS_VMs.ToList();

            ViewBag.Cur_Price = cur_price;
            ViewBag.List_Price = list_price_his;
            ViewBag.Tic_Price = tic_price;
            ViewBag.os_vm = os;

            return View();
        }

        [HttpPost]
        public ActionResult Detail(FormCollection fc)
        {
            TICKET_VM tic = _db.TICKET_VMs.FirstOrDefault(x => x.ID == int.Parse(fc["ID"]));
            tic.TICKETBA = fc["TICKETBA"];
            tic.OWNER = fc["OWNER"];
            tic.LOCATION = fc["LOCATION"];
            _db.SubmitChanges();

            return RedirectToAction("Detail", new {ID = int.Parse(fc["ID"])});
        }

        public ActionResult History()
        {

            var z = Request.Form["Year"];
            string yearSearch = z;
            if (z == null)
            {
                yearSearch = DateTime.Now.Year.ToString();
            }
            ViewBag.yearTK = yearSearch;

            List<TICKET_VM> bor = _db.TICKET_VMs.ToList();
            List<string> year = new List<string>();
            List<string> loc = new List<string>();
            List<int> countloc = new List<int>();

            var date = DateTime.Now;
            var result = date.Year;
            year.Add(result.ToString());


            int countWin = 0;
            int countLin = 0;
            int countUni = 0;
            int countOth = 0;
            List<CONFIG_VM> conf = _db.CONFIG_VMs.ToList();
            foreach (var item in conf)
            {
                OS_VM os = _db.OS_VMs.FirstOrDefault(x => item.ID_OS == x.ID);
                TICKET_VM tic = _db.TICKET_VMs.FirstOrDefault(x => x.ID == item.ID_TICKET);
                DateTime b = (DateTime)tic.STARTDATE;
                if (yearSearch == "Tổng" || b.Year.ToString() == yearSearch)
                {
                    switch (os.TYPE_OS)
                    {
                        case "Windows":
                            countWin++;
                            break;
                        case "Linux":
                            countLin++;
                            break;
                        case "Unix":
                            countUni++;
                            break;
                        case "Khác":
                            countOth++;
                            break;
                    }
                }
            }
            ViewBag.countWin = countWin; 
            ViewBag.countLin = countLin;
            ViewBag.countUni = countUni;
            ViewBag.countOth = countOth;




            int[] month = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            foreach (var item in bor)
            {
                DateTime a = (DateTime)item.STARTDATE;
                year.Add(a.Year.ToString());                
                for (int i = 0; i < 12; i++)
                {

                    if (i + 1 == int.Parse(a.Month.ToString()))
                    {
                        if (yearSearch == "Tổng" || a.Year.ToString() == yearSearch)
                        {
                            month[i] = month[i] + 1;
                            loc.Add(item.LOCATION);
                        }
                    }
                }
            }

            loc = loc.Distinct().ToList();
            Dictionary<string, int> locationCount = new Dictionary<string, int>();

            foreach (var location in loc)
            {
                locationCount[location] = 0;
            }

            foreach (var item in bor)
            {
                DateTime a = (DateTime)item.STARTDATE;
                year.Add(a.Year.ToString());
                if (yearSearch == "Tổng" || a.Year.ToString() == yearSearch)
                {
                    if (locationCount.ContainsKey(item.LOCATION))
                    {
                        locationCount[item.LOCATION]++;
                    }
                }
                   
            }

            year.Add("Tổng");
            ViewBag.Year = year.Distinct().ToList();
            ViewBag.Month = month;
            ViewBag.Loc = locationCount;

            return View();  
        }

        public static string GET_TOTAL (int RAM, int CPU, int FAST, int LOW, int ID, int TOTAL)
        {
            QLVMDataContext context = new QLVMDataContext();
            PRICE_VM pr = context.PRICE_VMs.FirstOrDefault(x => x.ID == ID);

            int total_price = (RAM * int.Parse(pr.RAM.Replace(".", "")) + CPU * int.Parse(pr.CPU.Replace(".", "")) + FAST * int.Parse(pr.DISK_FAST.Replace(".", "")) + LOW * int.Parse(pr.DISK_SLOW.Replace(".", ""))) * TOTAL;
            string result = GET_DOT(total_price.ToString());

            return result;
        }

        public static string GET_DOT(string x)
        {
            decimal number = decimal.Parse(x);
            CultureInfo customCulture = new CultureInfo("en-US");
            customCulture.NumberFormat.NumberGroupSeparator = ".";

            string formattedString = number.ToString("N0", customCulture);
            
            return formattedString;
        }

    }
}