using ASPNETCore.Application.Services;
using ASPNETCore.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly ILogger<CatalogController> _logger;
        private readonly CatalogService _catalogService;

        public CatalogController(CatalogService catalogService, ILogger<CatalogController> lo)
        {
            _catalogService = catalogService;
            _logger = lo;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<AttendanceStatus>>> GetAttendanceStatuses()
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает все статусы посещений");
            var dealTypes = await _catalogService.GetAttendanceStatusesAsync();
            return Ok(dealTypes);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<AttendanceStatus>> GetAttendanceStatus(int id)
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает тип посещения с id {id}");
            var dealType = await _catalogService.GetAttendanceStatusByIdAsync(id);
            if (dealType == null) return NotFound();
            return Ok(dealType);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает все города");
            var objectTypes = await _catalogService.GetAllCitiesAsync();
            return Ok(objectTypes);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<City>> GetCity(int id)
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает город с id {id}");
            var objectType = await _catalogService.GetCityByIdAsync(id);
            if (objectType == null) return NotFound();
            return Ok(objectType);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<EventCategory>>> GetEventCategories()
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает все категории мероприятий");
            var statuses = await _catalogService.GetAllEventCategoriesAsync();
            return Ok(statuses);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<EventCategory>> GetEventCategory(int id)
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает категорию мероприятия с id {id}");
            var status = await _catalogService.GetEventCategoryByIdAsync(id);
            if (status == null) return NotFound();
            return Ok(status);
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<EventStatus>>> GetEventStatuses()
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает статусы мероприятий");
            var resStatuses = await _catalogService.GetAllEventStatusesAsync();
            return Ok(resStatuses);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<EventStatus>> GetEventStatus(int id)
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает статус мероприятия с id {id}");
            var resStatus = await _catalogService.GetEventStatusByIdAsync(id);
            if (resStatus == null) return NotFound();
            return Ok(resStatus);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ReportStatus>>> GetReportStatuses()
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает статусы жалоб");
            var resStatuses = await _catalogService.GetAllReportStatusesAsync();
            return Ok(resStatuses);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ReportStatus>> GetReportStatus(int id)
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает статус жалобы с id {id}");
            var resStatus = await _catalogService.GetReportStatusByIdAsync(id);
            if (resStatus == null) return NotFound();
            return Ok(resStatus);
        }
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<VolunteerRank>>> GetVolunteerRanks()
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает ранг пользователя");
            var resStatuses = await _catalogService.GetAllVolunteerRanksAsync();
            return Ok(resStatuses);
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<VolunteerRank>> GetVolunteerRank(int id)
        {
            var currUser = User.Identity.IsAuthenticated ? User.Identity.Name : "Неавторизованный пользователь";
            _logger.LogInformation($"{currUser} получает ранг пользователя с id {id}");
            var resStatus = await _catalogService.GetVolunteerRankByIdAsync(id);
            if (resStatus == null) return NotFound();
            return Ok(resStatus);
        }
    }
}