using Microsoft.AspNetCore.Mvc;
using UrlShortenerApi.Services;

namespace UrlShortenerApi.Controllers
{
    [Route("api")]
    public class ApiController : Controller
    {
        private readonly UrlDatabase _database;

        public ApiController(UrlDatabase database)
        {
            _database = database;
        }

        [HttpGet]
        [Route("url/{url}")]
        public IActionResult Url([FromRoute] string url)
        {
            if (_database.Map.ContainsKey(url))
            {
                return Redirect(_database.Map[url]);
            }

            return BadRequest("URL не найден. \n Используйте бот для регистрации.");
        }
    }
}