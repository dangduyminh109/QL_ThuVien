using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class XacThucController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["TaiKhoan"] == null)
            {
                filterContext.Result = new RedirectResult("/Admin/Auth/DangNhap");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}