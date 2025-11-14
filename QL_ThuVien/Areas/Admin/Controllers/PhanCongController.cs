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
    public class PhanCongController : XacThucController
    {
        // GET: Admin/PhanCong
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            // Kiểm tra quyền SELECT trên bảng PHANCONG
            bool hasSelect = HasPermission("dbo.PHANCONG", "SELECT");
            if (!hasSelect)
            {
                ViewBag.NoAccess = true;
                return View(new List<PHANCONG>());
            }

            try
            {
                // Lấy danh sách phân công, sắp xếp theo ngày giảm dần rồi theo maNV
                var ds = Db.PHANCONGs
                           .OrderByDescending(p => p.ngayPhanCong)
                           .ThenBy(p => p.maNV)
                           .ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách phân công: " + ex.Message;
                return View(new List<PHANCONG>());
            }
        }

        // GET: Admin/PhanCong/Details?maCA=1&maNV=2&ngayPhanCong=2025-11-14
        public ActionResult Details(int maCA, int maNV, DateTime ngayPhanCong)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var pc = Db.PHANCONGs.FirstOrDefault(p => p.maCA == maCA && p.maNV == maNV && p.ngayPhanCong == ngayPhanCong);
            if (pc == null) return HttpNotFound();

            return View(pc);
        }

        // GET: Admin/PhanCong/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.PHANCONG", "INSERT"))
            {
                ViewBag.NoAccess = true;
                // Populate dropdowns để view không null
                ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                return View();
            }

            ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
            ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
            return View();
        }

        // POST: Admin/PhanCong/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PHANCONG model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            // Basic validation
            if (model == null)
            {
                ModelState.AddModelError("", "Dữ liệu phân công không hợp lệ.");
            }
            else
            {
                if (model.maNV <= 0) ModelState.AddModelError("maNV", "Vui lòng chọn nhân viên.");
                if (model.maCA <= 0) ModelState.AddModelError("maCA", "Vui lòng chọn ca làm việc.");
                if (string.IsNullOrEmpty(Request["ngayPhanCong"]))
                {
                    ModelState.AddModelError("ngayPhanCong", "Vui lòng chọn ngày phân công.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                return View(model);
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                    ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    // Gọi stored procedure SP_PhanCong để thêm phân công (proc sẽ kiểm tra trùng lịch và số nhân viên)
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "SP_PhanCong";

                    cmd.Parameters.AddWithValue("@maNV", model.maNV);
                    cmd.Parameters.AddWithValue("@maCA", model.maCA);
                    cmd.Parameters.AddWithValue("@ngayPhanCong", model.ngayPhanCong);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Phân công nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                ModelState.AddModelError("", "Lỗi SQL khi phân công: " + sqlEx.Message);
                ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi phân công: " + ex.Message);
                ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                return View(model);
            }
        }

        // GET: Admin/PhanCong/Edit?maCA=1&maNV=2&ngayPhanCong=2025-11-14
        public ActionResult Edit(int maCA, int maNV, DateTime ngayPhanCong)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var pc = Db.PHANCONGs.FirstOrDefault(p => p.maCA == maCA && p.maNV == maNV && p.ngayPhanCong == ngayPhanCong);
            if (pc == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phân công!";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.PHANCONG", "UPDATE"))
            {
                ViewBag.NoAccess = true;
            }

            ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
            ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
            ViewBag.CanUpdate = HasPermission("dbo.PHANCONG", "UPDATE");

            return View(pc);
        }

        // POST: Admin/PhanCong/Edit
        // Lưu ý: vì PK gồm (maCA, maNV, ngayPhanCong) nên ta nhận thêm original keys để xóa bản ghi cũ rồi gọi SP_PhanCong để insert bản mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PHANCONG model, int originalMaCA, int originalMaNV, DateTime originalNgayPhanCong)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (model == null)
            {
                ModelState.AddModelError("", "Dữ liệu phân công không hợp lệ.");
            }
            else
            {
                if (model.maNV <= 0) ModelState.AddModelError("maNV", "Vui lòng chọn nhân viên.");
                if (model.maCA <= 0) ModelState.AddModelError("maCA", "Vui lòng chọn ca làm việc.");
                if (string.IsNullOrEmpty(Request["ngayPhanCong"]))
                {
                    ModelState.AddModelError("ngayPhanCong", "Vui lòng chọn ngày phân công.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                return View(model);
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                    ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Xóa phân công cũ (dùng transaction)
                        using (var cmdDel = conn.CreateCommand())
                        {
                            cmdDel.Transaction = trans;
                            cmdDel.CommandText = "DELETE FROM PHANCONG WHERE maCA=@maCA AND maNV=@maNV AND ngayPhanCong=@ngayPhanCong";
                            cmdDel.Parameters.AddWithValue("@maCA", originalMaCA);
                            cmdDel.Parameters.AddWithValue("@maNV", originalMaNV);
                            cmdDel.Parameters.AddWithValue("@ngayPhanCong", originalNgayPhanCong);
                            cmdDel.ExecuteNonQuery();
                        }

                        // Thêm phân công mới bằng SP_PhanCong (proc sẽ kiểm tra trùng lịch / số NV)
                        using (var cmdIns = conn.CreateCommand())
                        {
                            cmdIns.Transaction = trans;
                            cmdIns.CommandType = CommandType.StoredProcedure;
                            cmdIns.CommandText = "SP_PhanCong";
                            cmdIns.Parameters.AddWithValue("@maNV", model.maNV);
                            cmdIns.Parameters.AddWithValue("@maCA", model.maCA);
                            cmdIns.Parameters.AddWithValue("@ngayPhanCong", model.ngayPhanCong);
                            cmdIns.ExecuteNonQuery();
                        }

                        trans.Commit();
                        TempData["SuccessMessage"] = "Cập nhật phân công thành công!";
                        if (mustClose && conn.State != ConnectionState.Closed) conn.Close();
                        return RedirectToAction("Index");
                    }
                    catch (SqlException sqlExTrans)
                    {
                        trans.Rollback();
                        ModelState.AddModelError("", "Lỗi SQL khi cập nhật phân công: " + sqlExTrans.Message);
                    }
                    catch (Exception exTrans)
                    {
                        trans.Rollback();
                        ModelState.AddModelError("", "Lỗi khi cập nhật phân công: " + exTrans.Message);
                    }
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật phân công: " + ex.Message);
            }

            ViewBag.NhanViens = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
            ViewBag.CaLamViecs = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["ErrorMessage"] = "Tham số không hợp lệ.";
                return RedirectToAction("Index");
            }

            var parts = id.Split('_');
            if (parts.Length != 3)
            {
                TempData["ErrorMessage"] = "Tham số không đúng định dạng.";
                return RedirectToAction("Index");
            }

            if (!int.TryParse(parts[0], out int maCA) ||
                !int.TryParse(parts[1], out int maNV) ||
                !DateTime.TryParse(parts[2], out DateTime ngayPhanCong))
            {
                TempData["ErrorMessage"] = "Tham số không đúng kiểu dữ liệu.";
                return RedirectToAction("Index");
            }

            // Các phần xử lý xóa như bạn có
            // Ví dụ:
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.PHANCONG", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa phân công.";
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
                    cmd.CommandText = "DELETE FROM PHANCONG WHERE maCA=@maCA AND maNV=@maNV AND ngayPhanCong=@ngayPhanCong";
                    cmd.Parameters.AddWithValue("@maCA", maCA);
                    cmd.Parameters.AddWithValue("@maNV", maNV);
                    cmd.Parameters.AddWithValue("@ngayPhanCong", ngayPhanCong);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Xóa phân công thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                TempData["ErrorMessage"] = "Lỗi SQL khi xóa phân công: " + sqlEx.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa phân công: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}
