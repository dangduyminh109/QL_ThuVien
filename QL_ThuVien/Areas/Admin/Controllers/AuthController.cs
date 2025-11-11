using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
//using QL_ThuVien.Areas.Admin.Services;
using QL_ThuVien.Models;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        private readonly DatabaseDataContext dataContext;
        public AuthController()
        {
            dataContext = new DatabaseDataContext();
        }
        // GET: Admin/Auth
        public ActionResult DangNhap()
        {
            return View();
        }
        public ActionResult DangNhapPost()
        {
            //string MK = Request.Form["MatKhau"];
            //string Email = Request.Form["Email"];
            //var TaiKhoan = dataContext.TaiKhoans.FirstOrDefault(item => item.MatKhau == MK && item.Email == Email);
            //if (TaiKhoan!= null && TaiKhoan.TrangThai == true)
            //{
            //    Session["TaiKhoan"] = TaiKhoan.MaTaiKhoan;
            //    Session["ChoPhep"] = TaiKhoan.PhanQuyen.ChoPhep;
            //    Session["HoTen"] = TaiKhoan.HoTen;
            //    Session["AnhDaiDien"] = TaiKhoan.AnhDaiDien;
            //    return Redirect("/Admin/SanPham");
            //}
            //ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
            //return RedirectToAction("DangNhap");
            return Redirect("/Admin/SanPham");
        }
        public ActionResult DangXuat()
        {
            Session.Clear(); 
            return RedirectToAction("DangNhap");
        }

        //public ActionResult HoSo(int id)
        //{
        //    return View(dataContext.TaiKhoans.FirstOrDefault(item=>item.MaTaiKhoan == id));
        //}
    }
}