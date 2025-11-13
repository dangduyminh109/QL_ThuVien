using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class NXBController : XacThucController
    {
        // GET: Admin/NXB
        public ActionResult Index()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.NXB", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View(new List<NXB>());
            }

            try
            {
                var ds = Db.NXBs.OrderBy(n => n.tenNXB).ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách NXB: " + ex.Message;
                return View(new List<NXB>());
            }
        }

        // GET: Admin/NXB/Details/5
        public ActionResult Details(int maNXB)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });
            var nxb = Db.NXBs.FirstOrDefault(n => n.maNXB == maNXB);
            if (nxb == null) return HttpNotFound();
            return View(nxb);
        }

        // GET: Admin/NXB/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });
            if (!HasPermission("dbo.NXB", "INSERT")) ViewBag.NoAccess = true;
            return View();
        }

        // POST: Admin/NXB/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NXB model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.tenNXB))
                ModelState.AddModelError("tenNXB", "Tên nhà xuất bản bắt buộc.");

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null) return View(model);

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO NXB (tenNXB, diaChi) VALUES (@tenNXB, @diaChi)";
                    cmd.Parameters.AddWithValue("@tenNXB", model.tenNXB ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@diaChi", model.diaChi ?? (object)DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Tạo NXB thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo NXB: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/NXB/Edit/5
        public ActionResult Edit(int maNXB)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });
            var nxb = Db.NXBs.FirstOrDefault(n => n.maNXB == maNXB);
            if (nxb == null) { TempData["ErrorMessage"] = "Không tìm thấy NXB!"; return RedirectToAction("Index"); }
            if (!HasPermission("dbo.NXB", "UPDATE")) ViewBag.NoAccess = true;
            return View(nxb);
        }

        // POST: Admin/NXB/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NXB model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (string.IsNullOrWhiteSpace(model.tenNXB))
                ModelState.AddModelError("tenNXB", "Tên nhà xuất bản bắt buộc.");

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null) return View(model);

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "UPDATE NXB SET tenNXB=@tenNXB, diaChi=@diaChi WHERE maNXB=@maNXB";
                    cmd.Parameters.AddWithValue("@tenNXB", model.tenNXB ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@diaChi", model.diaChi ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@maNXB", model.maNXB);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật NXB thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật NXB: " + ex.Message);
                return View(model);
            }
        }

        // POST: Admin/NXB/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null) { TempData["ErrorMessage"] = "Không kết nối tới CSDL."; return RedirectToAction("Index"); }
            if (!HasPermission("dbo.NXB", "DELETE")) { TempData["ErrorMessage"] = "Bạn không có quyền xóa NXB."; return RedirectToAction("Index"); }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null) return Json(new { success = false, message = "Kết nối DB không đúng loại SqlConnection." });

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM NXB WHERE maNXB=@maNXB";
                    cmd.Parameters.AddWithValue("@maNXB", id);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Xóa NXB thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa NXB: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
