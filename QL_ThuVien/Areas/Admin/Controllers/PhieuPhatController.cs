using QL_ThuVien.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVien.Areas.Admin.Controllers
{
    public class PhieuPhatController : XacThucController
    {
        // GET: Admin/Phieu
        public ActionResult Index()
        {
            if (Db == null)
                return RedirectToAction("DangNhap", "Auth", new { area = "Admin" });

            try
            {
                CapNhatPhiPhatTuDong();
                var ds = Db.PHIPHATs.ToList();

                return View(ds);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorPermission = "Lỗi khi load danh sách phiếu mượn: " + ex.Message;
                return View(new List<PHIPHAT>());
            }
        }

        private void CapNhatPhiPhatTuDong()
        {
            var homNay = DateTime.Now.Date;

            var phieuQuaHan = Db.PHIEUMUONs
                .Where(pm => pm.daTra == false && pm.ngayTra < homNay)
                .ToList();

            foreach (var pm in phieuQuaHan)
            {
                bool daCoPhat = Db.PHIPHATs.Any(p => p.maPMUON == pm.maPMUON);

                if (!daCoPhat)
                {
                    int soNgayQua = (homNay - pm.ngayTra.Value).Days;
                    int tienPhat = soNgayQua * 5000;

                    var phiPhat = new PHIPHAT
                    {
                        maPMUON = pm.maPMUON,
                        SoNgayQuaHan = soNgayQua,
                        tienPhat = tienPhat
                    };

                    Db.PHIPHATs.InsertOnSubmit(phiPhat);
                }
                else
                {
                    var phiPhat = Db.PHIPHATs.First(p => p.maPMUON == pm.maPMUON);
                    int soNgayQua = (homNay - pm.ngayTra.Value).Days;

                    phiPhat.SoNgayQuaHan = soNgayQua;
                    phiPhat.tienPhat = soNgayQua * 5000;
                }
            }

            Db.SubmitChanges();
        }

    }
}