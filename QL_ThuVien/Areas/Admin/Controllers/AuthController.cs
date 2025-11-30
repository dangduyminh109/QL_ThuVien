using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class AuthController : Controller
    {
        // Sửa nếu tên server / database khác
        private readonly string _server = "DDM-LEGION\\SQLEXPRESS";
        private readonly string _database = "QL_THUVIEN";

        // Hiển thị form đăng nhập
        public ActionResult DangNhap()
        {
            return View();
        }

        // Xử lý đăng nhập (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhapPost(string dbUsername, string dbPassword)
        {
            // Trim input để tránh whitespace vô tình
            dbUsername = (dbUsername ?? "").Trim();
            dbPassword = (dbPassword ?? "").Trim();

            if (string.IsNullOrWhiteSpace(dbUsername) || string.IsNullOrWhiteSpace(dbPassword))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu.";
                return View("DangNhap");
            }

            // Build connection string dùng SQL Authentication với credentials người dùng nhập
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = _server,
                InitialCatalog = _database,
                IntegratedSecurity = false,
                UserID = dbUsername,
                Password = dbPassword,
                ConnectTimeout = 5
            };

            try
            {
                // Thử mở connection và gọi proc sp_GetMyInfo để kiểm tra mapping nhân viên
                using (var conn = new SqlConnection(builder.ConnectionString))
                {
                    conn.Open();

                    using (var cmd = new SqlCommand("dbo.sp_GetMyInfo", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@username", SqlDbType.NVarChar, 50) { Value = dbUsername });

                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleRow))
                        {
                            if (reader.Read())
                            {
                                // Lấy thông tin nhân viên trả về bởi proc
                                int maNV = reader["maNV"] != DBNull.Value ? Convert.ToInt32(reader["maNV"]) : 0;
                                string hoTen = reader["hoTenNV"] != DBNull.Value ? reader["hoTenNV"].ToString() : "";
                                string chucVu = reader["chucVu"] != DBNull.Value ? reader["chucVu"].ToString() : "";

                                // Lưu thông tin vào session (chỉ khi proc trả row -> mapping tồn tại)
                                Session["DBUser"] = dbUsername;
                                Session["UserConnString"] = builder.ConnectionString; // lưu connection string (plain) theo yêu cầu
                                Session["HoTen"] = hoTen;
                                Session["ChucVu"] = chucVu;
                                Session["MaNV"] = maNV;

                                // Redirect tới Dashboard area Admin
                                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                            }
                            else
                            {
                                // Kết nối DB thành công nhưng user không được ánh xạ với NHANVIEN
                                ViewBag.Error = "Đăng nhập thành công nhưng tài khoản chưa được ánh xạ với nhân viên.";
                                return View("DangNhap");
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                // Lỗi xác thực / permission / kết nối SQL
                // Nếu muốn debug thêm, có thể log sqlEx.Number / sqlEx.Message
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng, hoặc bạn không có quyền truy cập.";
                return View("DangNhap");
            }
            catch (Exception ex)
            {
                // Lỗi khác (ví dụ mạng, cấu hình)
                ViewBag.Error = "Lỗi khi kết nối cơ sở dữ liệu: " + ex.Message;
                return View("DangNhap");
            }
        }

        // Đăng xuất: xóa session liên quan và chuyển về trang đăng nhập
        public ActionResult DangXuat()
        {
            // Xoá các key liên quan; có thể dùng Session.Clear() để xoá toàn bộ
            try
            {
                Session.Remove("DBUser");
                Session.Remove("UserConnString");
                Session.Remove("HoTen");
                Session.Remove("ChucVu");
                Session.Remove("MaNV");
                Session.Clear();
            }
            catch
            {
                // ignore nếu session store có vấn đề
            }

            return RedirectToAction("DangNhap");
        }
    }
}
