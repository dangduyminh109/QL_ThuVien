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

                // 1. Số lượng SÁCH
                ViewBag.SoLuongSach = Db.SACHes.Count();

                // 2. Số lượng NHÀ XUẤT BẢN (NXB)
                ViewBag.SoLuongNXB = Db.NXBs.Count();

                // 3. Số lượng THỂ LOẠI
                ViewBag.SoLuongTheLoai = Db.THELOAIs.Count();

                // 4. Số lượng TÁC GIẢ
                ViewBag.SoLuongTacGia = Db.TACGIAs.Count();

                // 5. Số lượng PHIẾU MƯỢN
                ViewBag.SoLuongPhieuMuon = Db.PHIEUMUONs.Count();

                // 6. Số lượng ĐỘC GIẢ
                ViewBag.SoLuongDocGia = Db.DOCGIAs.Count();

                // 7. Số lượng NHÂN VIÊN
                ViewBag.SoLuongNhanVien = Db.NHANVIENs.Count();

                // 8. Số phiếu mượn chưa trả
                ViewBag.SoPhieuMuonChuaTra = Db.PHIEUMUONs.Where(x => x.daTra == false).Count();

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
