using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Models;
using System.IO;
using System.Net.NetworkInformation;
using System.Net;
using static WebApplication1.Controllers.Unit_VMController;
using System.Data;
using PagedList;
using System.Web.UI.WebControls;

namespace WebApplication1.Controllers
{
    public class Unit_VMController : Controller
    {

        private QLVMDataContext _db = new QLVMDataContext();
        //private string encryptionKey = "FPT IS HCM";
        public class Data
        {
            public int ID_UNIT { get; set; }
            public string UNIT_NAME { get; set; }

            public int ID_VM { get; set; }
            public int? ID_STATUS { get; set; }
            public string VM_NAME { get; set; }
            public string TYPE { get; set; }
            public string OWNER { get; set; }
            public string LOCATION { get; set; }
            public DateTime? START_DATE { get; set; }
            public DateTime? END_DATE { get; set; }

            public int ID_IP { get; set; }
            public string IP_ADDRESS { get; set; }

            public int ID_CREN { get; set; }
            public string CRENDENTIAL { get; set; }

            public int COUNT { get; set; }
        }

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, string SearchDateStart, string SearchDateEnd, int? page)
        {
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


            List<UNIT_VM> UNIT_VM = _db.UNIT_VMs.ToList();
            List<VM_STAT> VM_STATS = _db.VM_STATs.ToList();
            List<CRENDENTIAL_VM> CRENDENTIAL_VM = _db.CRENDENTIAL_VMs.ToList();
            List<IP_VM> IP_VM = _db.IP_VMs.ToList();            

            UpdateStat();
            var query = from unit in UNIT_VM
                        join stats in VM_STATS on unit.ID_UNIT equals stats.ID_UNIT
                        select new Data
                        {
                            ID_UNIT = unit.ID_UNIT,
                            UNIT_NAME = unit.UNIT_NAME,

                            ID_VM = stats.ID_VM,
                            ID_STATUS = stats.ID_STATUS,
                            VM_NAME = stats.VM_NAME,
                            TYPE = stats.TYPE,
                            OWNER = stats.OWNER,
                            LOCATION = stats.LOCATION,
                            START_DATE = stats.START_DATE,
                            END_DATE = stats.END_DATE,

                            COUNT = CountStat(unit.ID_UNIT),
                        };

            if (!String.IsNullOrEmpty(searchString))
            {
                List<int> ID_UNIT_list = new List<int>();
                var x = query.Where(s => s.UNIT_NAME.ToLower().Contains(searchString.Trim().ToLower())
                                       || s.VM_NAME.ToLower().Contains(searchString.Trim().ToLower())
                                       || s.OWNER.ToLower().Contains(searchString.Trim().ToLower())
                                       ).ToList();
                foreach (var item in x)
                {
                    ID_UNIT_list.Add(item.ID_UNIT);
                }
                query = query.Where(s => ID_UNIT_list.Contains(s.ID_UNIT)).ToList();
            }
            if (!String.IsNullOrEmpty(SearchDateStart))
            {
                List<int> ID_UNIT_list = new List<int>();
                var y = query.Where(s => s.START_DATE >= DateTime.Parse(SearchDateStart)).ToList();
                foreach (var item in y)
                {
                    ID_UNIT_list.Add(item.ID_UNIT);
                }
                query = query.Where(s => ID_UNIT_list.Contains(s.ID_UNIT)).ToList();
            }
            if (!String.IsNullOrEmpty(SearchDateEnd))
            {
                List<int> ID_UNIT_list = new List<int>();
                var y = query.Where(s => s.END_DATE <= DateTime.Parse(SearchDateEnd)).ToList();
                foreach (var item in y)
                {
                    ID_UNIT_list.Add(item.ID_UNIT);
                }
                query = query.Where(s => ID_UNIT_list.Contains(s.ID_UNIT)).ToList();
            }

            ViewBag.CurrentSort = sortOrder;

            ViewBag.PHYSort = String.IsNullOrEmpty(sortOrder) ? "PHY" : "PHY";
            ViewBag.VMSort = String.IsNullOrEmpty(sortOrder) ? "VM" : "VM";
            ViewBag.CHSort = String.IsNullOrEmpty(sortOrder) ? "CH" : "CH";
            ViewBag.SHHSort = String.IsNullOrEmpty(sortOrder) ? "SHH" : "SHH";
            ViewBag.HHSort = String.IsNullOrEmpty(sortOrder) ? "HH" : "HH";

            switch (sortOrder)
            {
                //case "name_desc":
                //    bor = bor.OrderByDescending(s => s.Borrower).ToList();
                //    break;                

                case "PHY":
                    List<int> ID_UNIT_lista = new List<int>();
                    var a = query.Where(x => x.TYPE == "PHY").ToList();
                    foreach (var item in a)
                    {
                        ID_UNIT_lista.Add(item.ID_UNIT);
                    }
                    query = query.Where(s => ID_UNIT_lista.Contains(s.ID_UNIT)).ToList();
                    break;
                case "VM":
                    List<int> ID_UNIT_listb = new List<int>();
                    var b = query.Where(x => x.TYPE == "VM").ToList();
                    foreach (var item in b)
                    {
                        ID_UNIT_listb.Add(item.ID_UNIT);
                    }
                    query = query.Where(s => ID_UNIT_listb.Contains(s.ID_UNIT)).ToList();
                    break;
                case "CH":
                    List<int> ID_UNIT_listc = new List<int>();
                    var c = query.Where(x => x.ID_STATUS == 1).ToList();
                    foreach (var item in c)
                    {
                        ID_UNIT_listc.Add(item.ID_UNIT);
                    }
                    query = query.Where(s => ID_UNIT_listc.Contains(s.ID_UNIT)).ToList();
                    break;
                case "SHH":
                    List<int> ID_UNIT_listd = new List<int>();
                    var d = query.Where(x => x.ID_STATUS == 2).ToList();
                    foreach (var item in d)
                    {
                        ID_UNIT_listd.Add(item.ID_UNIT);
                    }
                    query = query.Where(s => ID_UNIT_listd.Contains(s.ID_UNIT)).ToList();
                    break;
                case "HH":
                    List<int> ID_UNIT_liste = new List<int>();
                    var e = query.Where(x => x.ID_STATUS == 3).ToList();
                    foreach (var item in e)
                    {
                        ID_UNIT_liste.Add(item.ID_UNIT);
                    }
                    query = query.Where(s => ID_UNIT_liste.Contains(s.ID_UNIT)).ToList();
                    break;
            }



            int pageSize = First10Unit();
            int pageNumber = (page ?? 1);
            ViewBag.Data = query.OrderByDescending(x => x.ID_UNIT).ToList().ToPagedList(pageNumber, pageSize);         

            ViewBag.CRENDENTIAL_VM = CRENDENTIAL_VM;
            ViewBag.IP_VM = IP_VM;          

            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(FormCollection fc)
        {
            List<string> VM_Name = fc["NameVM"].Split(',').ToList();
            List<string> TypeVM = fc["TypeVM"].Split(',').ToList();
            List<string> ownerVM = fc["ownerVM"].Split(',').ToList();
            List<string> startDate = fc["startDate"].Split(',').ToList();
            List<string> endDate = fc["endDate"].Split(',').ToList();
            List<string> Location = fc["Location"].Split(',').ToList();

            List<string> countIP = fc["IP_count"].Split(',').ToList();
            List<string> countLI = fc["LI_count"].Split(',').ToList();

            List<string> IP = fc["IP"].Split(',').ToList();
            List<string> CREN = fc["CREN"].Split(',').ToList();

            int x_IP = 0;
            int y_LI = 0;

   

            UNIT_VM unit = new UNIT_VM();
            unit.UNIT_NAME = fc["Unit"];
            _db.UNIT_VMs.InsertOnSubmit(unit);
            _db.SubmitChanges();

            for (int i = 0; i < TypeVM.Count(); i++)
            {
                VM_STAT stat = new VM_STAT();
                stat.ID_UNIT = unit.ID_UNIT;
                stat.ID_STATUS = 1;
                stat.VM_NAME = VM_Name[i];
                stat.TYPE = TypeVM[i];
                stat.OWNER = ownerVM[i];
                stat.LOCATION = Location[i];
                stat.START_DATE = DateTime.Parse(startDate[i]);
                stat.END_DATE = DateTime.Parse(endDate[i]);
                _db.VM_STATs.InsertOnSubmit(stat);
                _db.SubmitChanges();

                for (int j = 0; j < int.Parse(countIP[i]); j++)
                {
                    IP_VM ip = new IP_VM();
                    ip.ID_VM = stat.ID_VM;
                    ip.IP_ADDRESS = IP[x_IP];
                    x_IP++;
                    _db.IP_VMs.InsertOnSubmit(ip);
                    _db.SubmitChanges();
                }

                for (int j = 0; j < int.Parse(countLI[i]); j++)
                {
                    CRENDENTIAL_VM cre = new CRENDENTIAL_VM();
                    cre.ID_VM = stat.ID_VM;
                    cre.CRENDENTIAL = CREN[y_LI];
                    y_LI++;
                    _db.CRENDENTIAL_VMs.InsertOnSubmit(cre);
                    _db.SubmitChanges();
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Detail(int ID_UNIT)
        {
            UNIT_VM unit_vm = _db.UNIT_VMs.FirstOrDefault(x => x.ID_UNIT == ID_UNIT);
            List<VM_STAT> vm_stat = _db.VM_STATs.Where(x => x.ID_UNIT == ID_UNIT).ToList();
            List<IP_VM>  ip_vm = new List<IP_VM>();
            List<CRENDENTIAL_VM> cren_vm = new List<CRENDENTIAL_VM>();

            foreach(var item in vm_stat)
            {
                List<IP_VM> ip = _db.IP_VMs.Where(x => x.ID_VM == item.ID_VM).ToList();
                List<CRENDENTIAL_VM> cren = _db.CRENDENTIAL_VMs.Where(x => x.ID_VM == item.ID_VM).ToList();
            
                ip_vm.AddRange(ip);
                cren_vm.AddRange(cren);
            }

            ViewBag.UNIT_VM = unit_vm;
            ViewBag.VM_STAT = vm_stat;
            ViewBag.IP_VM = ip_vm;
            ViewBag.CRENDENTIAL_VM = cren_vm;

            return View();
        }

        [HttpPost]
        public ActionResult Detail(FormCollection fc) 
        {
            List<string> VM_Name = fc["NameVM"].Split(',').ToList();
            List<string> TypeVM = fc["TypeVM"].Split(',').ToList();
            List<string> ownerVM = fc["ownerVM"].Split(',').ToList();
            List<string> startDate = fc["startDate"].Split(',').ToList();
            List<string> endDate = fc["endDate"].Split(',').ToList();
            List<string> Location = fc["Location"].Split(',').ToList();

            List<string> countIP = fc["IP_count"].Split(',').ToList();
            List<string> countLI = fc["LI_count"].Split(',').ToList();

            List<string> IP = fc["IP"].Split(',').ToList();
            List<string> CREN = fc["CREN"].Split(',').ToList();
            
            int UnitID = int.Parse(fc["UnitID"].Trim());

            int x_IP = 0;
            int y_LI = 0;

            UNIT_VM unit = _db.UNIT_VMs.FirstOrDefault(x => x.ID_UNIT.Equals(UnitID));
            unit.UNIT_NAME = fc["Unit"].Trim();
            _db.SubmitChanges();

            List<VM_STAT> vm_list = _db.VM_STATs.Where(x => x.ID_UNIT == UnitID).ToList();
            foreach(var vm in vm_list)
            {
                List<IP_VM> ip = _db.IP_VMs.Where(x => x.ID_VM == vm.ID_VM).ToList();
                List<CRENDENTIAL_VM> cre = _db.CRENDENTIAL_VMs.Where(x => x.ID_VM == vm.ID_VM).ToList();
                _db.IP_VMs.DeleteAllOnSubmit(ip);
                _db.CRENDENTIAL_VMs.DeleteAllOnSubmit(cre);
                _db.SubmitChanges();
            }
            _db.VM_STATs.DeleteAllOnSubmit(vm_list);
            _db.SubmitChanges();

            for (int i = 0; i < TypeVM.Count(); i++)
            {
                VM_STAT stat = new VM_STAT();
                stat.ID_UNIT = unit.ID_UNIT;
                stat.ID_STATUS = 1;
                stat.VM_NAME = VM_Name[i];
                stat.TYPE = TypeVM[i];
                stat.OWNER = ownerVM[i];
                stat.LOCATION = Location[i];
                stat.START_DATE = DateTime.Parse(startDate[i]);
                stat.END_DATE = DateTime.Parse(endDate[i]);
                _db.VM_STATs.InsertOnSubmit(stat);
                _db.SubmitChanges();

                for (int j = 0; j < int.Parse(countIP[i]); j++)
                {
                    IP_VM ip = new IP_VM();
                    ip.ID_VM = stat.ID_VM;
                    ip.IP_ADDRESS = IP[x_IP];
                    x_IP++;
                    _db.IP_VMs.InsertOnSubmit(ip);
                    _db.SubmitChanges();
                }

                for (int j = 0; j < int.Parse(countLI[i]); j++)
                {
                    CRENDENTIAL_VM cre = new CRENDENTIAL_VM();
                    cre.ID_VM = stat.ID_VM;
                    cre.CRENDENTIAL = CREN[y_LI];
                    y_LI++;
                    _db.CRENDENTIAL_VMs.InsertOnSubmit(cre);
                    _db.SubmitChanges();
                }
            }

            UpdateStat();
            return RedirectToAction("Detail", new { ID_UNIT = UnitID });
        }

        public ActionResult Delete(int ID_UNIT)
        {
            UNIT_VM unit = _db.UNIT_VMs.FirstOrDefault(x => x.ID_UNIT.Equals(ID_UNIT));
            if (unit != null)
            {
                List<VM_STAT> vm = _db.VM_STATs.Where(x => x.ID_UNIT.Equals(ID_UNIT)).ToList();
                foreach (var item in vm)
                {
                    List<IP_VM> ip = _db.IP_VMs.Where(x => x.ID_VM.Equals(item.ID_VM)).ToList();
                    List<CRENDENTIAL_VM> cre = _db.CRENDENTIAL_VMs.Where(x => x.ID_VM.Equals(item.ID_VM)).ToList();

                    _db.IP_VMs.DeleteAllOnSubmit(ip);
                    _db.CRENDENTIAL_VMs.DeleteAllOnSubmit(cre);
                    _db.SubmitChanges();
                }
                _db.VM_STATs.DeleteAllOnSubmit(vm);
                _db.SubmitChanges();
                _db.UNIT_VMs.DeleteOnSubmit(unit);
                _db.SubmitChanges();
            }
            return RedirectToAction("Index");
        }

        public static string Encrypt(string clearText, string encryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x69, 0x63, 0x20, 0x4B, 0x65, 0x79 }, 1000);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string encryptionKey)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6E, 0x20, 0x4D, 0x65, 0x64, 0x76, 0x65, 0x64, 0x69, 0x63, 0x20, 0x4B, 0x65, 0x79 }, 1000);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static int CountStat (int id)
        {
            QLVMDataContext _db = new QLVMDataContext();            
            List<VM_STAT> VM_STATS = _db.VM_STATs.Where(x => x.ID_UNIT == id).ToList();         
            return VM_STATS.Count();
        }

        public static void UpdateStat()
        {
            QLVMDataContext _db = new QLVMDataContext();
            List<VM_STAT> stat = _db.VM_STATs.Where(x => x.ID_STATUS != 3).ToList();
            var date = DateTime.Now;

            foreach (var item in stat)
            {
                if (DateTime.Compare(date.AddDays(1), (DateTime)item.END_DATE) >= 0)
                {
                    item.ID_STATUS = 2;
                    _db.SubmitChanges();
                }

                if (DateTime.Compare(date, (DateTime)item.END_DATE) > 0)
                {
                    item.ID_STATUS = 3;
                    _db.SubmitChanges();
                }
            }
        }

        public static int First10Unit()
        {
            QLVMDataContext _db = new QLVMDataContext();
            List<UNIT_VM> unit = _db.UNIT_VMs.OrderByDescending(x => x.ID_UNIT).Take(10).ToList();
            int count = 0;
            foreach (var item in unit)
            {
                List<VM_STAT> stat = _db.VM_STATs.Where(x => x.ID_UNIT == item.ID_UNIT).ToList();
                count = count + stat.Count();
            }
            return count;
        }

    }
}