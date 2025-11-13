using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using QL_ThuVien.Models;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class NhanVienController : XacThucController
    {
        public NhanVienController() { }
        // ---------------------- Actions ----------------------

        // GET: Admin/NhanVien
        public ActionResult Index()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            bool hasSelectOnNhanVien = HasPermission("dbo.NHANVIEN", "SELECT");
            if (!hasSelectOnNhanVien)
            {
                ViewBag.NoAccess = true;
                return View(new List<NHANVIEN>());
            }

            try
            {
                var ds = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách: " + ex.Message;
                return View(new List<NHANVIEN>());
            }
        }

        // Details
        public ActionResult Details(int maNV)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var nv = Db.NHANVIENs.FirstOrDefault(n => n.maNV == maNV);
            if (nv == null) return HttpNotFound();
            return View(nv);
        }

        // GET: Admin/NhanVien/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            bool canInsert = HasPermission("dbo.NHANVIEN", "INSERT");
            if (!canInsert)
            {
                ViewBag.NoAccess = true;
                // vẫn populate roles để view không bị null (nếu bạn muốn hiển thị các role nhưng disabled)
                EnsureViewBagRoles();
                return View();
            }

            EnsureViewBagRoles();
            return View();
        }

        // POST: Admin/NhanVien/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NHANVIEN model, string username, string password, string roleToAdd)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            // Basic validation và thêm lỗi vào ModelState
            if (string.IsNullOrWhiteSpace(username)) ModelState.AddModelError("username", "Username bắt buộc.");
            if (string.IsNullOrWhiteSpace(password)) ModelState.AddModelError("password", "Password bắt buộc.");
            if (string.IsNullOrWhiteSpace(roleToAdd)) ModelState.AddModelError("roleToAdd", "Vui lòng chọn role.");

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    EnsureViewBagRoles();
                    ViewBag.SelectedRole = roleToAdd;
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "dbo.sp_CreateEmployeeWithUser";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@hoTenNV", model.hoTenNV ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gioiTinh", model.gioiTinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ngaySinh", model.ngaySinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@chucVu", string.IsNullOrWhiteSpace(roleToAdd) ? (object)DBNull.Value : roleToAdd);
                    cmd.Parameters.AddWithValue("@sdt", model.sdt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", model.email ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@roleToAdd", roleToAdd);
                    cmd.Parameters.AddWithValue("@createdBy", CurrentDbUser);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Tạo nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu nhân viên: " + sqlEx.Message;
                EnsureViewBagRoles();
                ViewBag.SelectedRole = roleToAdd;
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu nhân viên: " + ex.Message;
                EnsureViewBagRoles();
                ViewBag.SelectedRole = roleToAdd;
                return View(model);
            }
        }

        // GET: Admin/NhanVien/Edit/5
        public ActionResult Edit(int maNV)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });


            NHANVIEN nv = null;
            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    TempData["ErrorMessage"] = "Kết nối DB không đúng loại SqlConnection.";
                    return RedirectToAction("Index");
                }


                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();


                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM dbo.NHANVIEN WHERE maNV = @maNV";
                    cmd.Parameters.AddWithValue("@maNV", maNV);


                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nv = new NHANVIEN
                            {
                                maNV = (int)reader["maNV"],
                                hoTenNV = reader["hoTenNV"].ToString(),
                                gioiTinh = reader["gioiTinh"] as bool?,
                                ngaySinh = reader["ngaySinh"] as DateTime?,
                                chucVu = reader["chucVu"].ToString(),
                                sdt = reader["sdt"].ToString(),
                                email = reader["email"].ToString(),
                                DBUserName = reader["DBUserName"].ToString(),
                            };
                        }
                    }
                }

                // Lấy roles và set selected role
                EnsureViewBagRoles();
                string userRole = null;

                using (var cmdRole = conn.CreateCommand())
                {
                    cmdRole.CommandText = "dbo.sp_GetUserRole";
                    cmdRole.CommandType = CommandType.StoredProcedure;
                    cmdRole.Parameters.AddWithValue("@username", nv.DBUserName);

                    using (var reader = cmdRole.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            userRole = reader["RoleName"].ToString();
                        }
                    }
                }

                // Set role vào ViewBag
                ViewBag.SelectedRole = userRole;

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();


                if (nv == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhân viên!";
                    return RedirectToAction("Index");
                }


                // Kiểm tra quyền UPDATE (nếu cần hiển thị UI khác)
                ViewBag.CanUpdate = HasPermission("dbo.NHANVIEN", "UPDATE");


                return View(nv);
            }
            catch (Exception ex)
            {
                // Hiển thị lỗi và trả view rỗng (nếu muốn show lỗi tại view)
                ViewBag.Error = "Lỗi khi tải dữ liệu nhân viên: " + ex.Message;
                EnsureViewBagRoles();
                return View(nv);
            }
        }


        // POST: Admin/NhanVien/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NHANVIEN model, string username, string password, string roleToAdd)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            // Validation: chỉ require username (password & role không bắt buộc trên edit)
            if (string.IsNullOrWhiteSpace(username))
            {
                ModelState.AddModelError("username", "Username bắt buộc.");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    EnsureViewBagRoles();
                    ViewBag.SelectedRole = string.IsNullOrWhiteSpace(roleToAdd) ? model?.DBUserName : roleToAdd;
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "dbo.sp_UpdateEmployeeWithUser";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@maNV", model.maNV);
                    cmd.Parameters.AddWithValue("@hoTenNV", model.hoTenNV ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@gioiTinh", model.gioiTinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ngaySinh", model.ngaySinh ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@chucVu", string.IsNullOrWhiteSpace(roleToAdd) ? (object)DBNull.Value : roleToAdd);
                    cmd.Parameters.AddWithValue("@sdt", model.sdt ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@email", model.email ?? (object)DBNull.Value);

                    // password & role optional: nếu rỗng/NULL -> truyền DBNull để proc bỏ qua
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", string.IsNullOrWhiteSpace(password) ? (object)DBNull.Value : password);
                    cmd.Parameters.AddWithValue("@roleToAdd", string.IsNullOrWhiteSpace(roleToAdd) ? (object)DBNull.Value : roleToAdd);

                    cmd.Parameters.AddWithValue("@updatedBy", CurrentDbUser);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                // show sql error in view
                ModelState.AddModelError("", "Lỗi SQL khi cập nhật nhân viên: " + sqlEx.Message);
                EnsureViewBagRoles();
                ViewBag.SelectedRole = string.IsNullOrWhiteSpace(roleToAdd) ? model?.DBUserName : roleToAdd;
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật nhân viên: " + ex.Message);
                EnsureViewBagRoles();
                ViewBag.SelectedRole = string.IsNullOrWhiteSpace(roleToAdd) ? model?.DBUserName : roleToAdd;
                return View(model);
            }
        }

        // POST: Admin/NhanVien/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            // Kiểm tra kết nối
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            // Kiểm tra quyền DELETE trên bảng NHANVIEN
            if (!HasPermission("dbo.NHANVIEN", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa nhân viên.";
                return RedirectToAction("Index");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                    return Json(new { success = false, message = "Kết nối DB không đúng loại SqlConnection." });

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.sp_DeleteEmployeeWithUser";

                        // nếu maNV trong DB là int thì dùng int; nếu bạn dùng nvarchar, đổi kiểu tham số ở proc & đây thành string
                        cmd.Parameters.AddWithValue("@maNV", id);
                        cmd.Parameters.AddWithValue("@deletedBy", CurrentDbUser ?? (object)DBNull.Value);

                        cmd.ExecuteNonQuery();
                    }
                }
                finally
                {
                    if (mustClose && conn.State != ConnectionState.Closed) conn.Close();
                }
                TempData["SuccessMessage"] = "Xóa nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                TempData["ErrorMessage"] = "Lỗi SQL khi xóa nhân viên: " + sqlEx.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa nhân viên: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
