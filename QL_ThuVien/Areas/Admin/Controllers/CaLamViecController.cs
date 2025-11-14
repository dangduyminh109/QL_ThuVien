using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class CaLamViecController : XacThucController
    {
        // GET: Admin/CaLamViec
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.CALAMVIEC", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View(new List<CALAMVIEC>());
            }

            try
            {
                var ds = Db.CALAMVIECs.OrderBy(c => c.maCA).ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách ca làm việc: " + ex.Message;
                return View(new List<CALAMVIEC>());
            }
        }

        // GET: Admin/CaLamViec/Details/5
        public ActionResult Details(int maCA)
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var ca = Db.CALAMVIECs.FirstOrDefault(c => c.maCA == maCA);
            if (ca == null) return HttpNotFound();

            return View(ca);
        }

        // GET: Admin/CaLamViec/Create
        public ActionResult Create()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.CALAMVIEC", "INSERT"))
            {
                ViewBag.NoAccess = true;
                return View();
            }

            return View();
        }

        // POST: Admin/CaLamViec/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CALAMVIEC model)
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.CALAMVIEC", "INSERT"))
            {
                ViewBag.NoAccess = true;
                return View(model);
            }

            // Validate
            if (string.IsNullOrWhiteSpace(model.buoi))
                ModelState.AddModelError("buoi", "Tên ca (buổi) là bắt buộc.");

            if (!model.thoiGianBD.HasValue)
                ModelState.AddModelError("thoiGianBD", "Thời gian bắt đầu là bắt buộc.");

            if (!model.thoiGianKT.HasValue)
                ModelState.AddModelError("thoiGianKT", "Thời gian kết thúc là bắt buộc.");

            if (model.thoiGianBD.HasValue && model.thoiGianKT.HasValue && model.thoiGianBD.Value >= model.thoiGianKT.Value)
                ModelState.AddModelError("", "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            if (!ModelState.IsValid)
                return View(model);

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
                    cmd.CommandText = "INSERT INTO CALAMVIEC (buoi, thoiGianBD, thoiGianKT) VALUES (@buoi, @thoiGianBD, @thoiGianKT)";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@buoi", model.buoi ?? (object)DBNull.Value);

                    var pBD = cmd.CreateParameter();
                    pBD.ParameterName = "@thoiGianBD";
                    pBD.SqlDbType = SqlDbType.Time;
                    pBD.Value = model.thoiGianBD.HasValue ? (object)model.thoiGianBD.Value : DBNull.Value;
                    cmd.Parameters.Add(pBD);

                    var pKT = cmd.CreateParameter();
                    pKT.ParameterName = "@thoiGianKT";
                    pKT.SqlDbType = SqlDbType.Time;
                    pKT.Value = model.thoiGianKT.HasValue ? (object)model.thoiGianKT.Value : DBNull.Value;
                    cmd.Parameters.Add(pKT);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed)
                    conn.Close();

                TempData["SuccessMessage"] = "Thêm ca làm việc thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo ca làm việc: " + ex.Message);
                return View(model);
            }
        }

        // GET: Admin/CaLamViec/Edit/5
        public ActionResult Edit(int maCA)
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var ca = Db.CALAMVIECs.FirstOrDefault(c => c.maCA == maCA);
            if (ca == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy ca làm việc!";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.CALAMVIEC", "UPDATE"))
                ViewBag.NoAccess = true;

            return View(ca);
        }

        // POST: Admin/CaLamViec/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CALAMVIEC model)
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.CALAMVIEC", "UPDATE"))
            {
                ViewBag.NoAccess = true;
                return View(model);
            }

            // Validate
            if (string.IsNullOrWhiteSpace(model.buoi))
                ModelState.AddModelError("buoi", "Tên ca (buổi) là bắt buộc.");

            if (!model.thoiGianBD.HasValue)
                ModelState.AddModelError("thoiGianBD", "Thời gian bắt đầu là bắt buộc.");

            if (!model.thoiGianKT.HasValue)
                ModelState.AddModelError("thoiGianKT", "Thời gian kết thúc là bắt buộc.");

            if (model.thoiGianBD.HasValue && model.thoiGianKT.HasValue && model.thoiGianBD.Value >= model.thoiGianKT.Value)
                ModelState.AddModelError("", "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.");

            if (!ModelState.IsValid)
                return View(model);

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
                    cmd.CommandText = @"UPDATE CALAMVIEC
                                        SET buoi = @buoi,
                                            thoiGianBD = @thoiGianBD,
                                            thoiGianKT = @thoiGianKT
                                        WHERE maCA = @maCA";
                    cmd.CommandType = CommandType.Text;

                    cmd.Parameters.AddWithValue("@buoi", model.buoi ?? (object)DBNull.Value);

                    var pBD = cmd.CreateParameter();
                    pBD.ParameterName = "@thoiGianBD";
                    pBD.SqlDbType = SqlDbType.Time;
                    pBD.Value = model.thoiGianBD.HasValue ? (object)model.thoiGianBD.Value : DBNull.Value;
                    cmd.Parameters.Add(pBD);

                    var pKT = cmd.CreateParameter();
                    pKT.ParameterName = "@thoiGianKT";
                    pKT.SqlDbType = SqlDbType.Time;
                    pKT.Value = model.thoiGianKT.HasValue ? (object)model.thoiGianKT.Value : DBNull.Value;
                    cmd.Parameters.Add(pKT);

                    cmd.Parameters.AddWithValue("@maCA", model.maCA);

                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed)
                    conn.Close();

                TempData["SuccessMessage"] = "Cập nhật ca làm việc thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật ca làm việc: " + ex.Message);
                return View(model);
            }
        }

        // POST: Admin/CaLamViec/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.CALAMVIEC", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa ca làm việc.";
                return RedirectToAction("Index");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                    return Json(new { success = false, message = "Kết nối DB không đúng loại SqlConnection." });

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                // Kiểm tra ràng buộc phân công
                using (var chk = conn.CreateCommand())
                {
                    chk.CommandText = "SELECT COUNT(*) FROM PHANCONG WHERE maCA = @maCA";
                    chk.CommandType = CommandType.Text;
                    chk.Parameters.AddWithValue("@maCA", id);
                    var cnt = Convert.ToInt32(chk.ExecuteScalar());

                    if (cnt > 0)
                    {
                        if (mustClose && conn.State != ConnectionState.Closed)
                            conn.Close();

                        TempData["ErrorMessage"] = "Không thể xóa ca này vì đang có phân công liên quan.";
                        return RedirectToAction("Index");
                    }
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "DELETE FROM CALAMVIEC WHERE maCA = @maCA";
                    cmd.Parameters.AddWithValue("@maCA", id);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed)
                    conn.Close();

                TempData["SuccessMessage"] = "Xóa ca làm việc thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa ca làm việc: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
