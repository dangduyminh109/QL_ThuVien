using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class PhieuMuonController : XacThucController
    {
        // --- ViewModel nội bộ (nếu bạn đã có ở project khác, có thể bỏ phần này) ---
        public class SachMuonItem
        {
            public int maSach { get; set; }
            public int soLuong { get; set; }
        }

        public class TaoPhieuMuonRequest
        {
            public int maDocGia { get; set; }
            public int maNV { get; set; }
            public bool daTra { get; set; }
            public List<SachMuonItem> SachMuonList { get; set; }
        }
        public class SachMuonDetailViewModel
        {
            public int maSach { get; set; }
            public string tenSach { get; set; }
            public int soLuong { get; set; }
        }
        public class PhieuMuonDetailsViewModel
        {
            public PHIEUMUON PhieuMuon { get; set; }
            public List<SachMuonDetailViewModel> SachMuonDetails { get; set; }
        }

        // -------------------------------------------------------------------------

        // GET: Admin/PhieuMuon
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.PHIEUMUON", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View(new List<PHIEUMUON>());
            }

            try
            {
                var ds = Db.PHIEUMUONs
                           .OrderByDescending(p => p.ngayMuon)
                           .ToList();
                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách phiếu mượn: " + ex.Message;
                return View(new List<PHIEUMUON>());
            }
        }
        // GET: Admin/PhieuMuon/Details/5
        public ActionResult Details(int maPMUON)
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.PHIEUMUON", "SELECT"))
            {
                ViewBag.NoAccess = true;
                return View("Index");
            }

            var phieuMuon = Db.PHIEUMUONs.FirstOrDefault(p => p.maPMUON == maPMUON);
            if (phieuMuon == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu mượn!";
                return RedirectToAction("Index");
            }

            var chiTiet = (from c in Db.CTPHIEUMUONs
                           join s in Db.SACHes on c.maSach equals s.maSach
                           where c.maPMUON == maPMUON
                           select new SachMuonDetailViewModel
                           {
                               maSach = c.maSach,
                               tenSach = s.tenSach,
                               soLuong = c.soLuong
                           }).ToList();

            var model = new PhieuMuonDetailsViewModel
            {
                PhieuMuon = phieuMuon,
                SachMuonDetails = chiTiet
            };

            return View(model);
        }

        // GET: Admin/PhieuMuon/Create
        public ActionResult Create()
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.PHIEUMUON", "INSERT"))
            {
                ViewBag.NoAccess = true;
                // Vẫn load dropdown để view không null (nếu muốn hiển thị disabled)
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View();
            }

            ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
            ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
            ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
            return View();
        }

        // POST: Admin/PhieuMuon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(TaoPhieuMuonRequest model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (model == null || model.SachMuonList == null || !model.SachMuonList.Any())
            {
                ModelState.AddModelError("", "Danh sách sách mượn không được để trống.");
            }

            // Basic validation
            if (model != null)
            {
                if (model.maDocGia <= 0) ModelState.AddModelError("maDocGia", "Vui lòng chọn độc giả.");
                if (model.maNV <= 0) ModelState.AddModelError("maNV", "Vui lòng chọn nhân viên thực hiện.");
                foreach (var item in model.SachMuonList ?? new List<SachMuonItem>())
                {
                    if (item.maSach <= 0) ModelState.AddModelError("", "Vui lòng chọn sách hợp lệ.");
                    if (item.soLuong <= 0) ModelState.AddModelError("", "Số lượng mượn phải lớn hơn 0.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View(model);
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                    ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                    ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                    return View(model);
                }

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_TaoPhieuMuon";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@maDocGia", model.maDocGia);
                    cmd.Parameters.AddWithValue("@maNV", model.maNV);

                    // Tạo DataTable tương ứng SachMuonType (maSach, soLuong)
                    var tvp = new DataTable();
                    tvp.Columns.Add("maSach", typeof(int));
                    tvp.Columns.Add("soLuong", typeof(int));

                    foreach (var item in model.SachMuonList)
                    {
                        tvp.Rows.Add(item.maSach, item.soLuong);
                    }

                    var p = cmd.Parameters.AddWithValue("@SachMuonList", tvp);
                    p.SqlDbType = SqlDbType.Structured;
                    p.TypeName = "SachMuonType";

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Tạo phiếu mượn thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                // Lấy lỗi THROW từ SQL (ví dụ: quá hạn hoặc không đủ số lượng)
                ModelState.AddModelError("", sqlEx.Message);
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tạo phiếu mượn: " + ex.Message);
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View(model);
            }
        }
        // GET: Admin/PhieuMuon/Edit/5
        public ActionResult Edit(int maPMUON)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.PHIEUMUON", "UPDATE"))
            {
                ViewBag.NoAccess = true;
                // Load dropdowns để view không null (nếu muốn hiện disabled)
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View();
            }

            var phieuMuon = Db.PHIEUMUONs.FirstOrDefault(p => p.maPMUON == maPMUON);
            if (phieuMuon == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu mượn!";
                return RedirectToAction("Index");
            }

            // Lấy chi tiết phiếu mượn
            var chiTiet = Db.CTPHIEUMUONs.Where(c => c.maPMUON == maPMUON).ToList();

            // Map sang view model (chú ý: maDocGia, maNV, daTra có thể là nullable trong EF)
            var model = new TaoPhieuMuonRequest
            {
                maDocGia = phieuMuon.maDocGia.GetValueOrDefault(0),
                maNV = phieuMuon.maNV.GetValueOrDefault(0),
                daTra = phieuMuon.daTra.GetValueOrDefault(false),
                SachMuonList = chiTiet.Select(c => new SachMuonItem
                {
                    maSach = c.maSach,
                    soLuong = c.soLuong
                }).ToList()
            };

            ViewBag.maPMUON = maPMUON;
            ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
            ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
            ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();

            return View(model);
        }

        // POST: Admin/PhieuMuon/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int maPMUON, TaoPhieuMuonRequest model)
        {
            if (Db == null) return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            if (!HasPermission("dbo.PHIEUMUON", "UPDATE"))
            {
                ViewBag.NoAccess = true;
                ViewBag.maPMUON = maPMUON;
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View(model);
            }

            if (model == null || model.SachMuonList == null || !model.SachMuonList.Any())
            {
                ModelState.AddModelError("", "Danh sách sách mượn không được để trống.");
            }

            if (model != null)
            {
                if (model.maDocGia <= 0) ModelState.AddModelError("maDocGia", "Vui lòng chọn độc giả.");
                if (model.maNV <= 0) ModelState.AddModelError("maNV", "Vui lòng chọn nhân viên thực hiện.");
                foreach (var item in model.SachMuonList ?? new List<SachMuonItem>())
                {
                    if (item.maSach <= 0) ModelState.AddModelError("", "Vui lòng chọn sách hợp lệ.");
                    if (item.soLuong <= 0) ModelState.AddModelError("", "Số lượng mượn phải lớn hơn 0.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.maPMUON = maPMUON;
                ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                return View(model);
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                {
                    ModelState.AddModelError("", "Kết nối DB không đúng loại SqlConnection.");
                    ViewBag.maPMUON = maPMUON;
                    ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
                    ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
                    ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
                    return View(model);
                }

                var mustClose = conn.State == ConnectionState.Closed;
                if (mustClose) conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_SuaPhieuMuon";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@maPMUON", maPMUON);
                    cmd.Parameters.AddWithValue("@maDocGia", model.maDocGia);
                    cmd.Parameters.AddWithValue("@maNV", model.maNV);
                    cmd.Parameters.AddWithValue("@daTra", model.daTra ? 1 : 0);

                    // tạo TVP tương ứng SachMuonType
                    var tvp = new DataTable();
                    tvp.Columns.Add("maSach", typeof(int));
                    tvp.Columns.Add("soLuong", typeof(int));

                    foreach (var item in model.SachMuonList)
                    {
                        tvp.Rows.Add(item.maSach, item.soLuong);
                    }

                    var p = cmd.Parameters.AddWithValue("@SachMuonList", tvp);
                    p.SqlDbType = SqlDbType.Structured;
                    p.TypeName = "SachMuonType";

                    cmd.ExecuteNonQuery();
                }

                if (mustClose) conn.Close();

                TempData["SuccessMessage"] = "Cập nhật phiếu mượn thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                ModelState.AddModelError("", sqlEx.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi cập nhật phiếu mượn: " + ex.Message);
            }

            ViewBag.maPMUON = maPMUON;
            ViewBag.DocGia = Db.DOCGIAs.OrderBy(d => d.hoTenDG).ToList();
            ViewBag.NhanVien = Db.NHANVIENs.OrderBy(n => n.hoTenNV).ToList();
            ViewBag.Sach = Db.SACHes.OrderBy(s => s.tenSach).ToList();
            return View(model);
        }

        // POST: Admin/PhieuMuon/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            if (Db == null)
            {
                TempData["ErrorMessage"] = "Không kết nối tới CSDL. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index");
            }

            if (!HasPermission("dbo.PHIEUMUON", "DELETE"))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa phiếu mượn.";
                return RedirectToAction("Index");
            }

            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null)
                    return Json(new { success = false, message = "Kết nối DB không đúng loại SqlConnection." });

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        // Xóa chi tiết trước (FK)
                        using (var cmdDelCt = conn.CreateCommand())
                        {
                            cmdDelCt.Transaction = trans;
                            cmdDelCt.CommandText = "DELETE FROM CTPHIEUMUON WHERE maPMUON = @maPMUON";
                            cmdDelCt.Parameters.AddWithValue("@maPMUON", id);
                            cmdDelCt.ExecuteNonQuery();
                        }

                        // Xóa phiếu mượn
                        using (var cmdDelPm = conn.CreateCommand())
                        {
                            cmdDelPm.Transaction = trans;
                            cmdDelPm.CommandText = "DELETE FROM PHIEUMUON WHERE maPMUON = @maPMUON";
                            cmdDelPm.Parameters.AddWithValue("@maPMUON", id);
                            cmdDelPm.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();

                TempData["SuccessMessage"] = "Xóa phiếu mượn thành công!";
                return RedirectToAction("Index");
            }
            catch (SqlException sqlEx)
            {
                TempData["ErrorMessage"] = "Lỗi SQL khi xóa phiếu mượn: " + sqlEx.Message;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi xóa phiếu mượn: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public ActionResult ReturnBooks()
        {
            
            return View();
        }

        public ActionResult SearchPhieuMuon(int? maPMUON)
        {

            ViewBag.MaPMUON = maPMUON;
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });


            var phieuMuon = Db.PHIEUMUONs.FirstOrDefault(p => p.maPMUON == maPMUON);
            if (phieuMuon == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy phiếu mượn!";
                return RedirectToAction("ReturnBooks");
            }
            if (phieuMuon.daTra == true)
            {
                TempData["ErrorMessage"] = "Mã phiếu này đã được trả trước đó";
                return RedirectToAction("ReturnBooks");
            }

            // 
            var conn = Db.Connection as SqlConnection;

            var mustClose = (conn.State == ConnectionState.Closed);
            if (mustClose) conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT dbo.FNC_kiemTraHopLe(@maPMUON)";
            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@maPMUON", maPMUON);
            var check = cmd.ExecuteScalar();
            if (!Convert.ToBoolean(check))
            {
                TempData["err"] = "Phiếu mươn này đã trễ hạn";
            }


            var chiTiet = (from c in Db.CTPHIEUMUONs
                           join s in Db.SACHes on c.maSach equals s.maSach
                           where c.maPMUON == maPMUON
                           select new SachMuonDetailViewModel
                           {
                               maSach = c.maSach,
                               tenSach = s.tenSach,
                               soLuong = c.soLuong
                           }).ToList();

            var model = new PhieuMuonDetailsViewModel
            {
                PhieuMuon = phieuMuon,
                SachMuonDetails = chiTiet
            };


            return View("ReturnBooks", model);
        }

        [HttpPost]
        public ActionResult ReturnBooks(int? maPMUON)
        {
            if(maPMUON == null)
            {
                TempData["err"] = "Mã phiếu trống";
                return View();
            }
            try
            {
                var conn = Db.Connection as SqlConnection;

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();
                var cmd = conn.CreateCommand();

                cmd.CommandText = "sp_traSach";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@maPM", maPMUON);
                cmd.ExecuteNonQuery();

                if (mustClose) conn.Close();
                TempData["SuccessMessage"] = "Trả sách thành công";
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["err"] = "Đã có lỗi xảy ra";
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }


    }
}
