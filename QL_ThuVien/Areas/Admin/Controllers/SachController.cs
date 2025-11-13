using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QL_ThuVien.Models;
using QL_ThuVien.Admin.Models;
namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class SachController : XacThucController
    {
        public SachController() { }

        // GET: Admin/Sach
        public ActionResult Index(string searchName = "")
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.SACH", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View(new List<dynamic>());
            }

            var list = new List<SachSearchResult>();

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ViewBag.Error = "Kết nối DB không đúng loại SqlConnection.";
                    return View(list);
                }

                var mustClose = conn.State == ConnectionState.Closed;
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT * FROM dbo.fn_TimKiemSach(@tuKhoa)";
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@tuKhoa", searchName ?? "");

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new SachSearchResult
                            {
                                maSach = (int)reader["maSach"],
                                tenSach = reader["tenSach"].ToString(),
                                hoTenTG = reader["hoTenTG"].ToString(),
                                tenNXB = reader["tenNXB"].ToString(),
                                namXuatBan = reader["namXuatBan"] != DBNull.Value ? (int?)reader["namXuatBan"] : null,
                                SlConLai = reader["SlConLai"] != DBNull.Value ? (int?)reader["SlConLai"] : null,
                                anhBia = reader["anhBia"] != DBNull.Value ? reader["anhBia"].ToString() : null
                            });
                        }
                    }
                }

                if (mustClose && conn.State != ConnectionState.Closed)
                    conn.Close();

                ViewBag.searchName = searchName;
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải dữ liệu sách: " + ex.Message;
            }

            return View(list);
        }

        // GET: Admin/Sach/Details
        public ActionResult Details(int maSach)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            var s = Db.SACHes.FirstOrDefault(t => t.maSach == maSach);
            if (s == null) return HttpNotFound();

            return View(s);
        }

        // GET: Admin/Sach/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.SACH", "INSERT"))
            {
                ViewBag.NoAccess = true;
                // vẫn load dropdown để hiển thị thông tin quyền (nhưng user không thể submit)
            }

            try
            {
                LoadDropdowns();
                // trả model rỗng để view dùng (nếu muốn có giá trị mặc định)
                return View(new SACH());
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi chuẩn bị form tạo sách: " + ex.Message;
                LoadDropdowns();
                return View(new SACH());
            }
        }

        // POST: Admin/Sach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SACH model, HttpPostedFileBase AnhBia)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.SACH", "INSERT"))
            {
                ViewBag.NoAccess = true;
                LoadDropdowns();
                return View(model);
            }

            try
            {
                // Validation cơ bản
                if (string.IsNullOrWhiteSpace(model.tenSach))
                {
                    ModelState.AddModelError("tenSach", "Tên sách là bắt buộc.");
                }
                if (!model.maTG.HasValue)
                {
                    ModelState.AddModelError("maTG", "Bạn phải chọn tác giả.");
                }
                if (!model.maNXB.HasValue)
                {
                    ModelState.AddModelError("maNXB", "Bạn phải chọn nhà xuất bản.");
                }
                if (!model.maTL.HasValue)
                {
                    ModelState.AddModelError("maTL", "Bạn phải chọn thể loại.");
                }
                if (!model.slSach.HasValue)
                {
                    ModelState.AddModelError("SlSach", "Số lượng sách là bắt buộc.");
                }
                else if (model.slSach < 0)
                {
                    ModelState.AddModelError("SlSach", "Số lượng sách không được âm.");
                }
                if (model.namXuatBan.HasValue && model.namXuatBan > DateTime.Now.Year)
                {
                    ModelState.AddModelError("namXuatBan", "Năm xuất bản không thể lớn hơn năm hiện tại.");
                }

                if (!ModelState.IsValid)
                {
                    LoadDropdowns();
                    return View(model);
                }

                // Xử lý upload ảnh (nếu có)
                string anhBiaRelative = null;
                if (AnhBia != null && AnhBia.ContentLength > 0)
                {
                    // Optional: kiểm tra định dạng file (png/jpg/jpeg/gif)
                    var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var ext = Path.GetExtension(AnhBia.FileName).ToLowerInvariant();
                    if (!allowedExt.Contains(ext))
                    {
                        ModelState.AddModelError("AnhBia", "Chỉ chấp nhận ảnh (jpg, jpeg, png, gif, bmp).");
                        LoadDropdowns();
                        return View(model);
                    }

                    var uploadsDir = Server.MapPath("~/Content/images/sach/");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var savePath = Path.Combine(uploadsDir, fileName);
                    AnhBia.SaveAs(savePath);

                    // lưu đường dẫn tương đối để ghi vào DB (ví dụ: /Content/uploads/books/xxxx.jpg)
                    anhBiaRelative = Url.Content("~/Content/images/sach/" + fileName);
                }

                // Gọi stored procedure sp_TaoSachMoi
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    TempData["ErrorMessage"] = "Kết nối DB không đúng loại SqlConnection.";
                    LoadDropdowns();
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_TaoSachMoi";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@tenSach", SqlDbType.NVarChar, 200) { Value = model.tenSach });
                    cmd.Parameters.Add(new SqlParameter("@maTG", SqlDbType.Int) { Value = model.maTG.Value });
                    cmd.Parameters.Add(new SqlParameter("@maNXB", SqlDbType.Int) { Value = model.maNXB.Value });
                    cmd.Parameters.Add(new SqlParameter("@maTL", SqlDbType.Int) { Value = model.maTL.Value });
                    cmd.Parameters.Add(new SqlParameter("@namXuatBan", SqlDbType.Int) { Value = (object)model.namXuatBan ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@SlSach", SqlDbType.Int) { Value = model.slSach.Value });
                    cmd.Parameters.Add(new SqlParameter("@moTa", SqlDbType.NVarChar, -1) { Value = (object)model.mota ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@anhBia", SqlDbType.NVarChar, 200) { Value = (object)anhBiaRelative ?? DBNull.Value });

                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Tạo sách mới thành công.";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                // Nếu proc THROW với error number custom sẽ đến đây
                ViewBag.Error = "Lỗi khi tạo sách: " + sqlEx.Message;
                LoadDropdowns();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tạo sách: " + ex.Message;
                LoadDropdowns();
                return View(model);
            }
        }

        // GET: Admin/Sach/Edit/5
        public ActionResult Edit(int maSach)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.SACH", "UPDATE"))
            {
                ViewBag.NoAccess = true;
                // vẫn load dropdown để hiển thị
            }

            var s = Db.SACHes.FirstOrDefault(x => x.maSach == maSach);
            if (s == null) return HttpNotFound();

            LoadDropdowns();
            return View(s);
        }

        // POST: Admin/Sach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(SACH model, HttpPostedFileBase AnhBia)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.SACH", "UPDATE"))
            {
                ViewBag.NoAccess = true;
                LoadDropdowns();
                return View(model);
            }

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(model.tenSach))
                {
                    ModelState.AddModelError("tenSach", "Tên sách là bắt buộc.");
                }
                if (!model.maTG.HasValue)
                {
                    ModelState.AddModelError("maTG", "Bạn phải chọn tác giả.");
                }
                if (!model.maNXB.HasValue)
                {
                    ModelState.AddModelError("maNXB", "Bạn phải chọn nhà xuất bản.");
                }
                if (!model.maTL.HasValue)
                {
                    ModelState.AddModelError("maTL", "Bạn phải chọn thể loại.");
                }
                if (!model.slSach.HasValue)
                {
                    ModelState.AddModelError("SlSach", "Số lượng sách là bắt buộc.");
                }
                else if (model.slSach < 0)
                {
                    ModelState.AddModelError("SlSach", "Số lượng sách không được âm.");
                }
                if (model.namXuatBan.HasValue && model.namXuatBan > DateTime.Now.Year)
                {
                    ModelState.AddModelError("namXuatBan", "Năm xuất bản không thể lớn hơn năm hiện tại.");
                }

                if (!ModelState.IsValid)
                {
                    LoadDropdowns();
                    return View(model);
                }

                // thực hiện lưu ảnh nếu có
                string anhBiaRelative = null;
                if (AnhBia != null && AnhBia.ContentLength > 0)
                {
                    var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var ext = Path.GetExtension(AnhBia.FileName).ToLowerInvariant();
                    if (!allowedExt.Contains(ext))
                    {
                        ModelState.AddModelError("AnhBia", "Chỉ chấp nhận ảnh (jpg, jpeg, png, gif, bmp).");
                        LoadDropdowns();
                        return View(model);
                    }

                    var uploadsDir = Server.MapPath("~/Content/images/sach/");
                    if (!Directory.Exists(uploadsDir)) Directory.CreateDirectory(uploadsDir);

                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var savePath = Path.Combine(uploadsDir, fileName);
                    AnhBia.SaveAs(savePath);

                    anhBiaRelative = Url.Content("~/Content/images/sach/" + fileName);
                }
                else
                {
                    // nếu không upload mới, giữ đường dẫn ảnh hiện tại (từ DB)
                    var exist = Db.SACHes.FirstOrDefault(x => x.maSach == model.maSach);
                    if (exist != null) anhBiaRelative = exist.anhBia;
                }

                // gọi stored procedure sp_CapNhatSach
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    TempData["ErrorMessage"] = "Kết nối DB không đúng loại SqlConnection.";
                    LoadDropdowns();
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_CapNhatSach";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@maSach", SqlDbType.Int) { Value = model.maSach });
                    cmd.Parameters.Add(new SqlParameter("@tenSach", SqlDbType.NVarChar, 200) { Value = model.tenSach });
                    cmd.Parameters.Add(new SqlParameter("@maTG", SqlDbType.Int) { Value = model.maTG.Value });
                    cmd.Parameters.Add(new SqlParameter("@maNXB", SqlDbType.Int) { Value = model.maNXB.Value });
                    cmd.Parameters.Add(new SqlParameter("@maTL", SqlDbType.Int) { Value = model.maTL.Value });
                    cmd.Parameters.Add(new SqlParameter("@namXuatBan", SqlDbType.Int) { Value = (object)model.namXuatBan ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@SlSach", SqlDbType.Int) { Value = model.slSach.Value });
                    cmd.Parameters.Add(new SqlParameter("@moTa", SqlDbType.NVarChar, -1) { Value = (object)model.mota ?? DBNull.Value });
                    cmd.Parameters.Add(new SqlParameter("@anhBia", SqlDbType.NVarChar, 200) { Value = (object)anhBiaRelative ?? DBNull.Value });

                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật sách thành công.";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlex)
            {
                ViewBag.Error = "Lỗi khi cập nhật sách: " + sqlex.Message;
                LoadDropdowns();
                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi cập nhật sách: " + ex.Message;
                LoadDropdowns();
                return View(model);
            }
        }

        // POST: Admin/Sach/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.SACH", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa sách.";
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
                    cmd.CommandText = "DELETE FROM SACH WHERE maSach = @maSach";
                    cmd.Parameters.AddWithValue("@maSach", id);
                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed)
                    conn.Close();

                TempData["SuccessMessage"] = "Xóa sách thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa sách: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SyncQuantities()
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.SACH", "UPDATE")) // Hoặc quyền tương ứng
            {
                TempData["ErrorMessage"] = "Bạn không có quyền đồng bộ số lượng sách.";
                return RedirectToAction("Index");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    TempData["ErrorMessage"] = "Kết nối CSDL không hợp lệ.";
                    return RedirectToAction("Index");
                }

                bool mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sp_DongBoSoLuongSach";

                    // Nếu thủ tục có output hoặc return value thì xử lý thêm ở đây

                    cmd.ExecuteNonQuery();
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Đồng bộ số lượng sách hoàn tất.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi đồng bộ số lượng sách: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper: load dropdowns TACGIA, NXB, THELOAI
        private void LoadDropdowns()
        {
            try
            {
                var tacgiaList = Db.TACGIAs.OrderBy(t => t.hoTenTG).ToList();
                var nxbList = Db.NXBs.OrderBy(n => n.tenNXB).ToList();
                var theloaiList = Db.THELOAIs.OrderBy(t => t.tenTL).ToList();

                ViewBag.TacGiaList = new SelectList(tacgiaList, "maTG", "hoTenTG");
                ViewBag.NXBList = new SelectList(nxbList, "maNXB", "tenNXB");
                ViewBag.TheLoaiList = new SelectList(theloaiList, "maTL", "tenTL");
            }
            catch
            {
                // nếu lỗi load, để rỗng dropdown (view sẽ xử lý)
                ViewBag.TacGiaList = new SelectList(new List<object>());
                ViewBag.NXBList = new SelectList(new List<object>());
                ViewBag.TheLoaiList = new SelectList(new List<object>());
            }
        }
    }
}
