using Login_DichVuNgoai.Data;
using Login_DichVuNgoai.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Login_DichVuNgoai.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger, MyDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task LoginWithGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("GoogleResponseLogin")
            });
        }
        public async Task<IActionResult> GoogleResponseLogin()// trang đăng nhập google
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.ToList();
            // 1. phải check xem có tồn tại trong externallogin không
            // 2. Nếu có thì lấy userid và login (lưu userid vào session)
            // 2.1 Nếu không có thì:
            // redirect sang trang bắt nó nhập thông tin, tạo user mới và externallogin và login (lưu userid vào session)


            var providerKey = claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            var externalLogin = await _context.ExternalLogins.FirstOrDefaultAsync(c=>c.ProviderKey == providerKey);
            if (externalLogin != null)// nếu account tồn tại
            {
                HttpContext.Session.SetString("UserId", externalLogin.UserId.ToString()); // gán userid vào session
                return Content("login thành công tài khoản đã kết nối sẵn" + externalLogin.UserId);
            }
            else
            {
                // đáng ra là redirect sang form bắt user nhập thông tin
                var user = new User();// tạo user mới
                user.Username = email;
                user.Email = email;
                user.EmailConfirmed = false;
                user.Password = "";
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                var googleId = _context.ProviderLogins.FirstOrDefault(c => c.ProviderName.ToLower() == "google").ProviderId; // lấy id provider google
                var external = new ExternalLogin(); //tạo bản ghi external login google
                external.ProviderKey = providerKey;
                external.ProviderId = googleId;
                external.UserId = user.UserId;
                await _context.ExternalLogins.AddAsync(external);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("UserId", user.UserId.ToString()); // gán userid vào session
                return Content("login thành công tài khoản vừa tạo" + user.UserId);
            }
        }
        public async Task ConnectWithGoogle(int id) // kết nối google
        {
            HttpContext.Session.SetString("Id", id.ToString()); // mỗi tài khoản khi đăng nhập mới cho liên kết nên có thể sửa id thành UserId
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
            {
                RedirectUri = Url.Action("ConnectGoogle")
            });
        }
        public async Task<IActionResult> ConnectGoogle()// trang đăng nhập google
        {
            var id = HttpContext.Session.GetString("Id")??"";
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            var claims = result.Principal.Identities.FirstOrDefault().Claims.ToList();
            // phần connect
            // kiểm tra xem có đã tồn tại trong externallogin không
            // nếu có thì hiện hủy kết nối
            // nếu không thì cho sang trang kết nối
            // chọn account:
            // -nếu chọn account đã có trong externallogin thì hiện thông báo đã có
            // -nếu chọn account chưa có trong externallogin thì thêm vào externallogin


            var providerKey = claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            var email = claims?.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;

            var externalLogin = await _context.ExternalLogins.FirstOrDefaultAsync(c => c.ProviderKey == providerKey);
            if (externalLogin != null)
            {
                return Content("Connect false");
            }
            else
            {
                var googleId = _context.ProviderLogins.FirstOrDefault(c => c.ProviderName.ToLower() == "google").ProviderId; // lấy id provider google
                var external = new ExternalLogin(); //tạo bản ghi external login google
                external.ProviderKey = providerKey;
                external.ProviderId = googleId;
                external.UserId = int.Parse(id);
                await _context.ExternalLogins.AddAsync(external);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetString("UserId", id.ToString()); // gán userid vào session
                return Content("Connect true");
                //return RedirectToAction("Index", "Home");
            }
        }
        public async Task<IActionResult> DisConnectGoogle(int id) //Disconnect
        {
            var external = await _context.ExternalLogins.ToListAsync();
            var googleId = _context.ProviderLogins.FirstOrDefault(c => c.ProviderName.ToLower() == "google").ProviderId; // lấy id provider google
            var externalLogin = await _context.ExternalLogins.FirstOrDefaultAsync(c => c.UserId == id && c.ProviderId == googleId);
             _context.ExternalLogins.Remove(externalLogin);
            await _context.SaveChangesAsync();
            return Content("Disconnect thành công cho  "+ id);
        }
        public async Task<IActionResult> LogOut() // đăng xuất
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);// xóa dữ liệu khỏi cookies
            return RedirectToAction("Index", "Home");
        }
        
        public async Task add500()
        {
            
            for (int i = 0; i < 500; i++)
            {
                var user = new User()
                {
                    Username =  i.ToString(),
                    Password = "1",
                    Email = i + "@gmail.com",
                    EmailConfirmed = true
                };
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
        }
        public async Task delete500()
        {
            var user = await _context.Users.Take(500).ToListAsync();
            foreach (var item in user)
            {
                _context.Users.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.ToListAsync();
            return View(user);
        }
        public async Task<IActionResult> Detail(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(c => c.UserId == id);
            var external = await _context.ExternalLogins.Where(c => c.UserId == id).ToListAsync();
            ViewBag.External = external;
            var provider = await _context.ProviderLogins.ToListAsync();
            ViewBag.Provider = provider;
            var exists = external.Where(el => provider.Any(pl => pl.ProviderId == el.ProviderId)).ToList();// kiểm tra và lấy ra những provider đã được liên kết với user
            ViewBag.Exists = exists;
            return View(user);
        }
        public IActionResult SignIn()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}