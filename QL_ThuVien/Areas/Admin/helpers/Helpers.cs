using System.Collections.Generic;
using System;
using System.Web;
namespace QL_ThuVien.Areas.Admin.Helpers
{
    public static class HelperUtils
    {
        public static string LocPageUrl(HttpRequestBase request)
        {
            // Lấy URL gốc và query string nếu có
            string UrlGoc = request.RawUrl;
            // VD: "/Admin/DanhMuc?name=abc&page=2&sort=desc"

            // Tách phần base URL và query string
            string[] thanhPhan = UrlGoc.Split('?');
            string Url = thanhPhan[0]; // "/Admin/DanhMuc"

            if (thanhPhan.Length == 1)
            {
                // Không có query string, return với dấu ?
                return Url + "?";
            }

            // Có query string
            string[] thamSo = thanhPhan[1].Split('&');
            List<string> locThamSo = new List<string>();

            foreach (var item in thamSo)
            {
                // Loại bỏ tham số có tên "page"
                if (!item.StartsWith("page="))
                {
                    locThamSo.Add(item);
                }
            }

            string chuoiThamSoNew = string.Join("&", locThamSo);
            return string.IsNullOrEmpty(chuoiThamSoNew) ? Url + "?" : Url + "?" + chuoiThamSoNew + "&";
            // KQ: "/Admin/DanhMuc?name=abc&2&sort=desc&"
        }

    }

}