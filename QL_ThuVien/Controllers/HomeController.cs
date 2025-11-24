using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Linq;            // DataContext
using QL_ThuVien.Models;           // Các lớp SACH, TACGIA, NXB, THELOAI, ...

namespace QL_ThuVien.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _db;

        // Kết nối bằng user "reader" pass "123" theo yêu cầu
        public HomeController()
        {
            // CHÚ Ý: thay Server/Database nếu cần. Nếu bạn có typed DataContext (VD: QLThuVienDataContext),
            // thay new DataContext(...) bằng new QLThuVienDataContext(connStr)
            //var connStr = @"Server=.;Database=QL_ThuVien;User Id=reader;Password=123;Trusted_Connection=False;";
            //_db = new DataContext(connStr);
            var connStr = @"Server=LAPTOP-3F26GI9M;Database=QL_THUVIEN;User Id=reader;Password=dat23062005;Trusted_Connection=True;";
            _db = new DataContext(connStr);
        }

        // GET: /Home/Index
        // Trả về danh sách sách (card) — mỗi item chứa một số thông tin cơ bản
        public ActionResult Index()
        {
            try
            {
                var sachTable = _db.GetTable<SACH>();
                var tgTable = _db.GetTable<TACGIA>();
                var nxbTable = _db.GetTable<NXB>();
                var tlTable = _db.GetTable<THELOAI>();

                var list = (from s in sachTable
                            join tg in tgTable on s.maTG equals tg.maTG into tgJoin
                            from tg in tgJoin.DefaultIfEmpty()
                            join nxb in nxbTable on s.maNXB equals nxb.maNXB into nxbJoin
                            from nxb in nxbJoin.DefaultIfEmpty()
                            join tl in tlTable on s.maTL equals tl.maTL into tlJoin
                            from tl in tlJoin.DefaultIfEmpty()
                            orderby s.tenSach
                            select new BookCardViewModel
                            {
                                maSach = s.maSach,
                                TenSach = s.tenSach,
                                TenTacGia = tg != null ? tg.hoTenTG : "",
                                TenNXB = nxb != null ? nxb.tenNXB : "",
                                TenTheLoai = tl != null ? tl.tenTL : "",
                                SlConLai = s.SlConLai ?? 0,
                                AnhBia = s.anhBia
                            }
                            ).ToList();

                return View(list);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi kết nối / truy vấn, trả về View rỗng kèm message
                ViewBag.Error = "Lỗi khi tải danh sách: " + ex.Message;
                return View(new List<BookCardViewModel>());
            }
        }

        // GET: /Home/Detail/5
        public ActionResult Detail(int id)
        {
            try
            {
                var sachTable = _db.GetTable<SACH>();
                var tgTable = _db.GetTable<TACGIA>();
                var nxbTable = _db.GetTable<NXB>();
                var tlTable = _db.GetTable<THELOAI>();

                var item = (from s in sachTable
                            where s.maSach == id
                            join tg in tgTable on s.maTG equals tg.maTG into tgJoin
                            from tg in tgJoin.DefaultIfEmpty()
                            join nxb in nxbTable on s.maNXB equals nxb.maNXB into nxbJoin
                            from nxb in nxbJoin.DefaultIfEmpty()
                            join tl in tlTable on s.maTL equals tl.maTL into tlJoin
                            from tl in tlJoin.DefaultIfEmpty()
                            select new BookDetailViewModel
                            {
                                maSach = s.maSach,
                                TenSach = s.tenSach,
                                NamXuatBan = s.namXuatBan,
                                SoLuong = s.slSach,
                                SlConLai = s.SlConLai ?? 0,
                                TenTacGia = tg != null ? tg.hoTenTG : "",
                                TenNXB = nxb != null ? nxb.tenNXB : "",
                                TenTheLoai = tl != null ? tl.tenTL : "",
                                AnhBia = s.anhBia,
                                MoTa = s.mota
                            }).FirstOrDefault();

                if (item == null)
                    return HttpNotFound();

                return View(item);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải thông tin chi tiết: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        public ActionResult TimKiem(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return RedirectToAction("Index");

                var sachTable = _db.GetTable<SACH>();
                var tgTable = _db.GetTable<TACGIA>();
                var nxbTable = _db.GetTable<NXB>();
                var tlTable = _db.GetTable<THELOAI>();

                keyword = keyword.Trim().ToLower();

                var list = (from s in sachTable
                            join tg in tgTable on s.maTG equals tg.maTG into tgJoin
                            from tg in tgJoin.DefaultIfEmpty()
                            join nxb in nxbTable on s.maNXB equals nxb.maNXB into nxbJoin
                            from nxb in nxbJoin.DefaultIfEmpty()
                            join tl in tlTable on s.maTL equals tl.maTL into tlJoin
                            from tl in tlJoin.DefaultIfEmpty()
                            where
                                s.tenSach.ToLower().Contains(keyword) ||
                                (tg.hoTenTG ?? "").ToLower().Contains(keyword) ||
                                (tl.tenTL ?? "").ToLower().Contains(keyword) ||
                                (nxb.tenNXB ?? "").ToLower().Contains(keyword)
                            orderby s.tenSach
                            select new BookCardViewModel
                            {
                                maSach = s.maSach,
                                TenSach = s.tenSach,
                                TenTacGia = tg != null ? tg.hoTenTG : "",
                                TenNXB = nxb != null ? nxb.tenNXB : "",
                                TenTheLoai = tl != null ? tl.tenTL : "",
                                SlConLai = s.SlConLai ?? 0,
                                AnhBia = s.anhBia
                            }
                        ).ToList();

                ViewBag.Title = $"Kết quả tìm kiếm cho: {keyword}";
                return View("Index", list);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi tìm kiếm: " + ex.Message;
                return RedirectToAction("Index");
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class BookCardViewModel
    {
        public int maSach { get; set; }
        public string TenSach { get; set; }
        public string TenTacGia { get; set; }
        public string TenNXB { get; set; }
        public string TenTheLoai { get; set; }
        public int SlConLai { get; set; }

        public string AnhBia { get; set; }  
    }

    public class BookDetailViewModel
    {
        public int maSach { get; set; }
        public string TenSach { get; set; }
        public int? NamXuatBan { get; set; }
        public int? SoLuong { get; set; }
        public int SlConLai { get; set; }
        public string TenTacGia { get; set; }
        public string TenNXB { get; set; }
        public string TenTheLoai { get; set; }

        public string AnhBia { get; set; }  // Ảnh bìa
        public string MoTa { get; set; }    // Mô tả
    }

}
