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
    public class DocGiaController : XacThucController
    {
        // GET: Admin/DocGia
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            // Kiểm tra quyền SELECT trên bảng DOCGIA
            bool hasSelectOnDocGia = HasPermission("dbo.DOCGIA", "SELECT");
            if (!hasSelectOnDocGia)
            {
                ViewBag.NoAccess = true;
                return View(new List<DOCGIA>());
            }

            try
            {
                // Lấy danh sách độc giả, sắp xếp theo họ tên
                var ds = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách độc giả: " + ex.Message;
                return View(new List<DOCGIA>());
            }
        }


        // GET: Admin/DocGia/Details/5
        public ActionResult Details(int maDocGia)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var dg = Db.DOCGIAs.FirstOrDefault(d => d.maDocGia == maDocGia);
            if (dg == null) return HttpNotFound();

            return View(dg);
        }

        // GET: Admin/DocGia/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.DOCGIA", "INSERT"))
            {
                ViewBag.NoAccess = true;
                return View();
            }

            return View();
        }

        // POST: Admin/DocGia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DOCGIA model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.hoTenDG))
            {
                ModelState.AddModelError("hoTenDG", "Họ tên độc giả bắt buộc.");
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
                    cmd.CommandText = "INSERT INTO DOCGIA (hoTenDG, gioiTinh, ngaySinh, diaChi, sdt, email) VALUES (@hoTenDG, @gioiTinh, @ngaySinh, @diaChi, @sdt, @email)";
                    cmd.Parameters.AddWithValue("@hoTenDG", model.hoTenDG ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gioiTinh", model.gioiTinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ngaySinh", model.ngaySinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@diaChi", model.diaChi ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@sdt", model.sdt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", model.email ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Tạo độc giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo độc giả: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/DocGia/Edit/5
        public ActionResult Edit(int maDocGia)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var dg = Db.DOCGIAs.FirstOrDefault(d => d.maDocGia == maDocGia);
            if (dg == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy độc giả!";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.DOCGIA", "UPDATE"))
            {
                ViewBag.NoAccess = true;
            }

            return View(dg);
        }

        // POST: Admin/DocGia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DOCGIA model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.hoTenDG))
                ModelState.AddModelError("hoTenDG", "Họ tên độc giả bắt buộc.");

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
                    cmd.CommandText = @"UPDATE DOCGIA SET hoTenDG=@hoTenDG, gioiTinh=@gioiTinh, ngaySinh=@ngaySinh, diaChi=@diaChi, sdt=@sdt, email=@email
                                        WHERE maDocGia=@maDocGia";
                    cmd.Parameters.AddWithValue("@hoTenDG", model.hoTenDG ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gioiTinh", model.gioiTinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ngaySinh", model.ngaySinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@diaChi", model.diaChi ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@sdt", model.sdt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", model.email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@maDocGia", model.maDocGia);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật độc giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật độc giả: " + ex.Message);
                return View(model);
            }
        }

        // POST: Admin/DocGia/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.DOCGIA", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa độc giả.";
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
                    cmd.CommandText = "DELETE FROM DOCGIA WHERE maDocGia=@maDocGia";
                    cmd.Parameters.AddWithValue("@maDocGia", id);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Xóa độc giả thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa độc giả: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

    }
}