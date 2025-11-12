using System.Linq;
using System.Web.Mvc;
using QL_ThuVien.Models;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class DashboardController : XacThucController
    {
        public ActionResult Index()
        {
            try
            {
                ViewBag.HoTen = CurrentUser.hoTenNV;
                ViewBag.ChucVu = CurrentUser.chucVu;
                ViewBag.DBUser = CurrentDbUser;

                //// Lấy danh sách sách trong DB
                //var sachList = Db.SACHes.ToList();
                //ViewBag.SachList = sachList;

                //// Ví dụ: nếu muốn hiển thị thêm danh sách nhân viên
                //var nhanVienList = Db.NHANVIENs.ToList();
                //ViewBag.NhanVienList = nhanVienList;

                return View();
            }
            catch (System.Exception ex)
            {
                ViewBag.Error = "Lỗi: " + ex.Message;
                return View();
            }
        }
    }
}
