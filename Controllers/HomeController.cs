using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;

namespace Tassc.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        // Action method to generate the QR code
        public ActionResult GenerateQRCode(string url)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                using (Bitmap bitmap = qrCode.GetGraphic(20))
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    return File(memoryStream.ToArray(), "image/png");
                }
            }
        }

        // Action method to display the view with the QR code
        public ActionResult ShowQRCode()
        {
            string urlToEncode = Url.Action("Login", "Account", null, Request.Url.Scheme);
            ViewBag.QRCodeUrl = Url.Action("GenerateQRCode", "Home", new { url = urlToEncode });
            return View();
        }
    }
}
