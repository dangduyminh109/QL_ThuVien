using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class TacGiaController : XacThucController
    {
        // GET: Admin/TacGia
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.TACGIA", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View(new List<TACGIA>());
            }

            try
            {
                var ds = Db.TACGIAs.OrderBy(t => t.hoTenTG).ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách tác giả: " + ex.Message;
                return View(new List<TACGIA>());
            }
        }

        // GET: Admin/TacGia/Details/5
        public ActionResult Details(int maTG)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var tg = Db.TACGIAs.FirstOrDefault(t => t.maTG == maTG);
            if (tg == null) return HttpNotFound();

            return View(tg);
        }

        // GET: Admin/TacGia/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.TACGIA", "INSERT"))
            {
                ViewBag.NoAccess = true;
                return View();
            }

            return View();
        }

        // POST: Admin/TacGia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TACGIA model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.hoTenTG))
            {
                ModelState.AddModelError("hoTenTG", "Họ tên tác giả bắt buộc.");
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
                    cmd.CommandText = "INSERT INTO TACGIA (hoTenTG, ngaySinh, gioiTinh) VALUES (@hoTenTG, @ngaySinh, @gioiTinh)";
                    cmd.Parameters.AddWithValue("@hoTenTG", model.hoTenTG ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ngaySinh", model.ngaySinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gioiTinh", model.gioiTinh ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Tạo tác giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo tác giả: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/TacGia/Edit/5
        public ActionResult Edit(int maTG)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var tg = Db.TACGIAs.FirstOrDefault(t => t.maTG == maTG);
            if (tg == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tác giả!";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.TACGIA", "UPDATE"))
            {
                ViewBag.NoAccess = true;
            }

            return View(tg);
        }

        // POST: Admin/TacGia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(TACGIA model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.hoTenTG))
            {
                ModelState.AddModelError("hoTenTG", "Họ tên tác giả bắt buộc.");
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
                    cmd.CommandText = @"UPDATE TACGIA SET hoTenTG=@hoTenTG, ngaySinh=@ngaySinh, gioiTinh=@gioiTinh
                                        WHERE maTG=@maTG";
                    cmd.Parameters.AddWithValue("@hoTenTG", model.hoTenTG ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ngaySinh", model.ngaySinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gioiTinh", model.gioiTinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@maTG", model.maTG);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật tác giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật tác giả: " + ex.Message);
                return View(model);
            }
        }

        // POST: Admin/TacGia/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.TACGIA", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa tác giả.";
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
                    cmd.CommandText = "DELETE FROM TACGIA WHERE maTG=@maTG";
                    cmd.Parameters.AddWithValue("@maTG", id);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Xóa tác giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa tác giả: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
