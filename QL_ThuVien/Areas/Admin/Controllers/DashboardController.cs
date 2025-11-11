using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DatabaseDataContext dataContext;
        public DashboardController()
        {
            dataContext = new DatabaseDataContext();
        }
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            return View();
        }

    }
}