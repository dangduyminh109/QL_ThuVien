using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using QL_ThuVien.Models;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public abstract class XacThucController : Controller
    {
        protected DatabaseDataContext Db;
        protected NHANVIEN CurrentUser;
        protected string CurrentDbUser;
        protected List<string> RolesList = new List<string>();
        protected string CurrentRoles;
        protected string RoleLoadError;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 1) Kiểm tra login
            var dbUserObj = Session["DBUser"];
            var connObj = Session["UserConnString"];
            if (dbUserObj == null || connObj == null)
            {
                filterContext.Result = RedirectToLogin();
                return;
            }

            CurrentDbUser = dbUserObj.ToString();
            var connStr = connObj.ToString();

            // 2) Khởi tạo DataContext
            try { Db = new DatabaseDataContext(connStr); }
            catch (Exception ex)
            {
                filterContext.Result = new ContentResult { Content = "Không thể kết nối DB: " + ex.Message };
                return;
            }

            RoleLoadError = null;
            RolesList.Clear();
            CurrentRoles = string.Empty;
            CurrentUser = null;

            // 3) Nạp CurrentUser với check quyền SELECT
            LoadCurrentUser();

            // Nếu không tìm thấy user -> redirect login
            if (CurrentUser == null)
            {
                filterContext.Result = RedirectToLogin();
                return;
            }

            // 4) Nạp roles
            LoadRoles();

            // 5) Đổ thông tin vào ViewBag
            ViewBag.HoTen = CurrentUser?.hoTenNV ?? "";
            ViewBag.ChucVu = CurrentUser?.chucVu ?? "";
            ViewBag.DBUser = CurrentDbUser ?? "";
            ViewBag.Roles = CurrentRoles ?? "";
            ViewBag.RoleLoadError = RoleLoadError;

            base.OnActionExecuting(filterContext);
        }

        #region --- Helper Methods ---

        private ActionResult RedirectToLogin()
        {
            return new RedirectToRouteResult(new RouteValueDictionary {
                { "area", "Admin" }, { "controller", "Auth" }, { "action", "DangNhap" }
            });
        }

        private void LoadCurrentUser()
        {
            var sqlConn = Db.Connection as SqlConnection;
            if (sqlConn == null)
            {
                // fallback LINQ nếu không phải SqlConnection
                try { CurrentUser = Db.NHANVIENs.FirstOrDefault(n => n.DBUserName == CurrentDbUser); }
                catch { CurrentUser = null; }
                return;
            }

            bool openedHere = false;
            try
            {
                if (sqlConn.State == ConnectionState.Closed) { sqlConn.Open(); openedHere = true; }

                // Kiểm tra quyền SELECT
                bool hasSelect = false;
                using (var cmd = sqlConn.CreateCommand())
                {
                    cmd.CommandText = "SELECT permission_name FROM fn_my_permissions('dbo.NHANVIEN','OBJECT')";
                    cmd.CommandType = CommandType.Text;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader["permission_name"]?.ToString().Equals("SELECT", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                hasSelect = true;
                                break;
                            }
                        }
                    }
                }

                if (hasSelect)
                {
                    // Dùng LINQ
                    try { CurrentUser = Db.NHANVIENs.FirstOrDefault(n => n.DBUserName == CurrentDbUser); }
                    catch (Exception ex)
                    {
                        RoleLoadError = AppendRoleLoadError(RoleLoadError, "Lỗi load user bằng LINQ: " + ex.Message);
                        CurrentUser = null;
                    }
                }
                else
                {
                    // fallback: gọi stored procedure sp_GetMyInfo
                    using (var cmd = sqlConn.CreateCommand())
                    {
                        cmd.CommandText = "dbo.sp_GetMyInfo";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@username", SqlDbType.NVarChar, 50) { Value = CurrentDbUser });

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentUser = new NHANVIEN
                                {
                                    maNV = reader["maNV"] != DBNull.Value ? Convert.ToInt32(reader["maNV"]) : 0,
                                    hoTenNV = reader["hoTenNV"]?.ToString(),
                                    gioiTinh = reader["gioiTinh"] != DBNull.Value ? (bool?)Convert.ToBoolean(reader["gioiTinh"]) : null,
                                    ngaySinh = reader["ngaySinh"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(reader["ngaySinh"]) : null,
                                    chucVu = reader["chucVu"]?.ToString(),
                                    sdt = reader["sdt"]?.ToString(),
                                    email = reader["email"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                RoleLoadError = AppendRoleLoadError(RoleLoadError, "Lỗi khi load user: " + sqlEx.Message);
                CurrentUser = null;
            }
            finally
            {
                if (openedHere) try { sqlConn.Close(); } catch { }
            }
        }

        private void LoadRoles()
        {
            var sqlConn = Db.Connection as SqlConnection;
            if (sqlConn == null) return;

            bool openedHere = false;
            try
            {
                if (sqlConn.State == ConnectionState.Closed) { sqlConn.Open(); openedHere = true; }

                const string roleQuery = @"
                    SELECT r.name AS RoleName
                    FROM sys.database_role_members rm
                    JOIN sys.database_principals r ON rm.role_principal_id = r.principal_id
                    JOIN sys.database_principals u ON rm.member_principal_id = u.principal_id
                    WHERE u.name = @dbUser
                    ORDER BY r.name;
                ";

                using (var cmd = new SqlCommand(roleQuery, sqlConn))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@dbUser", CurrentDbUser);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var rn = reader["RoleName"]?.ToString();
                            if (!string.IsNullOrEmpty(rn)) RolesList.Add(rn);
                        }
                    }
                }

                if (RolesList.Any()) CurrentRoles = string.Join(", ", RolesList);

                // fallback: nếu role không load được, dùng chucVu
                if (!RolesList.Any() && !string.IsNullOrEmpty(CurrentUser?.chucVu))
                {
                    RolesList = CurrentUser.chucVu.Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => r.Trim()).Where(r => r.Length > 0).ToList();
                    CurrentRoles = string.Join(", ", RolesList);
                }
            }
            catch (SqlException sqlEx)
            {
                RoleLoadError = AppendRoleLoadError(RoleLoadError, "Lỗi khi đọc roles: " + sqlEx.Message);
            }
            finally
            {
                if (openedHere) try { sqlConn.Close(); } catch { }
            }
        }

        private string AppendRoleLoadError(string existing, string newMsg)
        {
            if (string.IsNullOrWhiteSpace(existing)) return newMsg;
            return existing + " | " + newMsg;
        }

        protected bool HasAnyRole(params string[] roles)
        {
            if (roles == null || roles.Length == 0) return false;
            if (RolesList == null || RolesList.Count == 0) return false;

            var normalized = new HashSet<string>(RolesList.Select(r => r.Trim()), StringComparer.OrdinalIgnoreCase);
            return roles.Any(r => normalized.Contains(r?.Trim()));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Db != null) { try { Db.Dispose(); } catch { } Db = null; }
            }
            base.Dispose(disposing);
        }

        protected bool HasPermission(string objectName, string permissionName)
        {
            try
            {
                var conn = Db.Connection as SqlConnection;
                if (conn == null) return false;

                var mustClose = (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                try
                {
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT permission_name FROM fn_my_permissions(@objectName,'OBJECT')";
                        cmd.Parameters.AddWithValue("@objectName", objectName);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (string.Equals(reader["permission_name"].ToString(), permissionName, StringComparison.OrdinalIgnoreCase))
                                    return true;
                            }
                        }
                    }
                }
                finally
                {
                    if (mustClose && conn.State != ConnectionState.Closed) conn.Close();
                }
            }
            catch
            {
                // Nếu có lỗi khi kiểm tra quyền -> mặc định deny
            }

            return false;
        }

        /// <summary>
        /// Lấy danh sách role có thể gán bằng stored procedure dbo.sp_GetAssignableRoles
        /// </summary>
        protected List<string> GetAssignableRoles(SqlConnection externalConn = null)
        {
            var roles = new List<string>();
            try
            {
                var conn = externalConn ?? Db.Connection as SqlConnection;
                if (conn == null) return roles;

                var mustClose = (externalConn == null) && (conn.State == ConnectionState.Closed);
                if (mustClose) conn.Open();

                using (var cmd = new SqlCommand("dbo.sp_GetAssignableRoles", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            roles.Add(reader["RoleName"].ToString());
                        }
                    }
                }

                if (mustClose && conn.State != ConnectionState.Closed) conn.Close();
            }
            catch
            {
                // trả về list rỗng nếu có lỗi
                roles = new List<string>();
            }

            return roles;
        }

        /// <summary>
        /// Đảm bảo ViewBag.DsQuyen luôn có giá trị (được gọi trước mọi return View(...)).
        /// </summary>
        protected void EnsureViewBagRoles(SqlConnection conn = null)
        {
            ViewBag.DsQuyen = GetAssignableRoles(conn);
        }


        #endregion
    }
}
