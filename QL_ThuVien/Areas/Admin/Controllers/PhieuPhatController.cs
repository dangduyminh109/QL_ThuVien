using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class PhieuPhatController : XacThucController
    {
        // GET: Admin/Phieu
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            try
            {
                var ds = Db.PHIPHATs.ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách phiếu mượn: " + ex.Message;
                return View(new List<PHIPHAT>());
            }
        }
    }
}