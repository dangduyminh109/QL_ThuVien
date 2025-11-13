using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class TheLoaiController : XacThucController
    {
        // GET: Admin/TheLoai
        public ActionResult Index()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.THELOAI", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View(new List<THELOAI>());
            }

            try
            {
                var ds = Db.THELOAIs.OrderBy(t => t.tenTL).ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách thể loại: " + ex.Message;
                return View(new List<THELOAI>());
            }
        }

        // GET: Admin/TheLoai/Details/5
        public ActionResult Details(int maTL)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var tl = Db.THELOAIs.FirstOrDefault(t => t.maTL == maTL);
            if (tl == null) return HttpNotFound();

            return View(tl);
        }

        // GET: Admin/TheLoai/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.THELOAI", "INSERT"))
            {
                ViewBag.NoAccess = true;
                return View();
            }

            return View();
        }

        // POST: Admin/TheLoai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(THELOAI model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.tenTL))
            {
                ModelState.AddModelError("tenTL", "Tên thể loại bắt buộc.");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO THELOAI (tenTL) VALUES (@tenTL)";
                    cmd.Parameters.AddWithValue("@tenTL", model.tenTL ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Tạo thể loại thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo thể loại: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/TheLoai/Edit/5
        public ActionResult Edit(int maTL)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var tl = Db.THELOAIs.FirstOrDefault(t => t.maTL == maTL);
            if (tl == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thể loại!";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.THELOAI", "UPDATE"))
            {
                ViewBag.NoAccess = true;
            }

            return View(tl);
        }

        // POST: Admin/TheLoai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(THELOAI model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.tenTL))
                ModelState.AddModelError("tenTL", "Tên thể loại bắt buộc.");

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE THELOAI SET tenTL=@tenTL WHERE maTL=@maTL";
                    cmd.Parameters.AddWithValue("@tenTL", model.tenTL ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@maTL", model.maTL);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật thể loại thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật thể loại: " + ex.Message);
                return View(model);
            }
        }

        // POST: Admin/TheLoai/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.THELOAI", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa thể loại.";
                return RedirectToAction("Index");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                    return Json(new { success = false, message = "Kết nối DB không đúng loại SqlConnection." });

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM THELOAI WHERE maTL=@maTL";
                    cmd.Parameters.AddWithValue("@maTL", id);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Xóa thể loại thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa thể loại: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
