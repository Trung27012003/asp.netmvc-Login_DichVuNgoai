using Login_DichVuNgoai.Data;
using Login_DichVuNgoai.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Login_DichVuNgoai.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MyDbContext _context;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = new MyDbContext();
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