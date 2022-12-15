using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SibMed.Journal.Attributes;
using SibMed.Journal.Data;
using SibMed.Journal.Entities;
using SibMed.Journal.Models;
using SibMed.Journal.PermissionParts;
using SibMed.Journal.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SibMed.Journal.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class IndicatorsController : ControllerBase
    {
        private readonly IIndicatorService _indicatorService;
        private readonly IApiService _apiService;
        private readonly ApplicationDbContext _dbContext;
        private readonly IMapper _mapper;

        public IndicatorsController(IIndicatorService indicatorService, IApiService apiService, ApplicationDbContext dbContext, IMapper mapper)
        {
            _indicatorService = indicatorService;
            _apiService = apiService;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        /// <summary>
        /// Получение показателей по университету.
        /// </summary>
        /// <response code="200">Показатели.</response>
        /// <response code="403">Не достаточно прав доступа.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<IndicatorModel>), 200)]
        [HttpGet("University")]
        [HasPermission(Permissions.GetIndicators)]
        public async Task<IActionResult> GetUniversity(DateTime date)
        {
            var rector = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == User.GetId() && (e.Position.Equals("Ректор") || e.Position.StartsWith("Проректор")));
            if (rector == null)
            {
                return Forbid();
            }

            var indicators = await _indicatorService.GetUniversity(date);

            return Ok(indicators);
        }

        /// <summary>
        /// Получение факультетов по показателю университета.
        /// </summary>
        /// <response code="200">Факультеты-показатель.</response>
        /// <response code="403">Не достаточно прав доступа.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<IndicatorModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("University/Faculties")]
        public async Task<IActionResult> Get(string indicatorId, DateTime date)
        {
            List<DictionaryFaculty> dictionaryFaculties = new();
            IList<IndicatorModel> indicators;
            AuthorizationUserModel userModel = await _apiService.GetUser(User.Identity.Name, HostType.Zkgu);

            dictionaryFaculties = await _indicatorService.GetDeansFaculties(userModel);

            if (dictionaryFaculties.Any())
            {
                indicators = await _indicatorService.GetIndicatorFaculties(indicatorId, date, dictionaryFaculties);

                return Ok(indicators);
            }

            var rector = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == User.GetId() && (e.Position.Equals("Ректор") || e.Position.StartsWith("Проректор")));

            if (rector != null)
            {
                dictionaryFaculties = await _dbContext.DictionaryFaculties.AsNoTracking().Where(f => f.UniversityUUID != null).ToListAsync();
            }
            else
            {
                return Forbid();
            }

            indicators = await _indicatorService.GetIndicatorFaculties(indicatorId, date, dictionaryFaculties);

            return Ok(indicators);
        }

        /// <summary>
        /// Получение кафедр по показателю факультета университета.
        /// </summary>
        /// <response code="200">Кафедры-показатель.</response>
        /// <response code="403">Не достаточно прав доступа.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<IndicatorModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("University/Faculties/Departments")]
        public async Task<IActionResult> Get(string indicatorId, int facultyId, DateTime date)
        {
            List<DictionaryDepartment> departments = new();
            List<DictionaryFaculty> dictionaryFaculties;
            AuthorizationUserModel userModel = await _apiService.GetUser(User.Identity.Name, HostType.Zkgu);

            departments = await _indicatorService.GetHeadsDepatmetns(userModel);
            departments = departments.Where(d => d.FacultyId == facultyId).ToList();

            dictionaryFaculties = await _indicatorService.GetDeansFaculties(userModel);

            if (dictionaryFaculties.Any())
            {
                var faculty = dictionaryFaculties.FirstOrDefault(f => f.Id == facultyId);

                if (faculty != null)
                {
                    var temp_departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(d => d.FacultyId == faculty.Id).ToListAsync();
                    departments.AddRange(temp_departments);
                }
            }

            if (!departments.Any())
            {
                var rector = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == User.GetId() && (e.Position.Equals("Ректор") || e.Position.StartsWith("Проректор")));

                if (rector != null)
                {
                    departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(f => f.UniversityUUID != null && f.FacultyId == facultyId).ToListAsync();
                }
                else
                {
                    return Forbid();
                }
            }

            var indicators = await _indicatorService.GetIndicatorDepartments(indicatorId, date, departments);

            return Ok(indicators);
        }

        /// <summary>
        /// Получение сотрудников по показателю кафедры университета.
        /// </summary>
        /// <response code="200">Кафедры-показатель.</response>
        /// <response code="403">Не достаточно прав доступа.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<IndicatorModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("University/Faculties/Departments/Employees")]
        public async Task<IActionResult> Get(string indicatorId, int? facultyId, int? departmentId, DateTime date)
        {
            List<IndicatorGroupModel> result = new();
            List<DictionaryDepartment> departments = new();
            List<Teacher> teachers = new();
            List<ApplicationUser> users = new();
            List<string> logins = new();
            List<IndicatorModel> indicators = new();
            List<DictionaryFaculty> dictionaryFaculties = new();
            AuthorizationUserModel userModel = await _apiService.GetUser(User.Identity.Name, HostType.Zkgu);

            departments = await _indicatorService.GetHeadsDepatmetns(userModel);
            if (departments.Any() && !departmentId.HasValue && facultyId.HasValue)
            {
                return Forbid();
            }

            dictionaryFaculties = await _indicatorService.GetDeansFaculties(userModel);

            if (dictionaryFaculties.Any())
            {
                var facultieQuery = dictionaryFaculties.Select(f => f.Id);

                var facultiesId = facultyId.HasValue ? facultieQuery.Where(f => f == facultyId).ToList() : facultieQuery.ToList();

                var temp_departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(d => facultiesId.Contains(d.FacultyId ?? -1)).ToListAsync();
                departments.AddRange(temp_departments);
            }

            if (departmentId.HasValue)
            {
                departments = departments.Where(d => d.Id == departmentId).ToList();
            }

            if (!departments.Any())
            {
                var rector = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == User.GetId() && (e.Position.Equals("Ректор") || e.Position.StartsWith("Проректор")));

                if (rector != null)
                {
                    if (departmentId.HasValue)
                    {
                        departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(f => f.UniversityUUID != null && f.Id == departmentId).ToListAsync();
                    }
                    else if (facultyId.HasValue)
                    {
                        departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(f => f.UniversityUUID != null && f.FacultyId == facultyId).ToListAsync();
                    }
                    else
                    {
                        departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(f => f.UniversityUUID != null).ToListAsync();
                    }
                }
                else
                {
                    if (!departmentId.HasValue)
                    {
                        return Forbid();
                    }

                    teachers = await _dbContext.Teachers.AsNoTracking()
                        .Include(t => t.User).Where(u => u.UserId == User.GetId()
                        && u.DepartmentId == departmentId
                        && (!facultyId.HasValue || u.Department.FacultyId == facultyId)).ToListAsync();

                    if (!teachers.Any())
                    {
                        return Forbid();
                    }

                    users = teachers.GroupBy(t => t.User).Select(t => t.Key).ToList();
                    logins = users.Select(u => u.UserName).ToList();

                    var department = await _dbContext.DictionaryDepartments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == departmentId);
                    indicators = (List<IndicatorModel>)await _indicatorService.GetIndicatorEmployees(indicatorId, department?.UniversityUUID, date, logins);

                    return Ok(indicators);
                }
            }

            foreach (var department in departments ?? Enumerable.Empty<DictionaryDepartment>())
            {
                teachers = await _dbContext.Teachers.AsNoTracking()
                    .Include(t => t.User).Where(t => t.Department.Id == department.Id).ToListAsync();

                users = teachers.GroupBy(t => t.User).Select(t => t.Key).ToList();
                logins = users.Select(u => u.UserName).ToList();

                var indicator = await _indicatorService.GetIndicatorEmployees(indicatorId, department.UniversityUUID, date, logins);

                if (indicator.Any())
                {
                    indicators.AddRange(indicator);
                }
            }
            return Ok(indicators);
        }

        /// <summary>
        /// Получение показателей по факультету.
        /// </summary>
        /// <response code="200">Факультеты-показатель.</response>
        /// <response code="403">Не достаточно прав доступа.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<IndicatorGroupModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("Faculties")]
        public async Task<IActionResult> GetFaculties(DateTime date)
        {
            List<IndicatorGroupModel> result = new();
            List<DictionaryFaculty> dictionaryFaculties = new();
            AuthorizationUserModel userModel = await _apiService.GetUser(User.Identity.Name, HostType.Zkgu);

            dictionaryFaculties = await _indicatorService.GetDeansFaculties(userModel);

            if (!dictionaryFaculties.Any())
            {
                var rector = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == User.GetId() && (e.Position.Equals("Ректор") || e.Position.StartsWith("Проректор")));

                if (rector != null)
                {
                    dictionaryFaculties = await _dbContext.DictionaryFaculties.AsNoTracking().Where(f => f.UniversityUUID != null).ToListAsync();
                }
                else
                {
                    return Forbid();
                }
            }

            foreach (var faculty in dictionaryFaculties)
            {
                var indicators = await _indicatorService.GetFaculty(faculty.Id, date);

                if (indicators.Any())
                {
                    result.Add(new IndicatorGroupModel
                    {
                        Id = faculty.Id.ToString(),
                        Title = faculty.Title,
                        Indicators = indicators
                    });
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Получение показателей по кафедре.
        /// </summary>
        /// <response code="200">Кафедры-показатель.</response>
        /// <response code="403">Не достаточно прав доступа.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<IndicatorGroupModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("Departments")]
        public async Task<IActionResult> GetDepartments(DateTime date)
        {
            List<IndicatorGroupModel> result = new();
            List<DictionaryDepartment> departments = new();
            List<DictionaryFaculty> dictionaryFaculties = new();
            AuthorizationUserModel userModel = await _apiService.GetUser(User.Identity.Name, HostType.Zkgu);

            departments = await _indicatorService.GetHeadsDepatmetns(userModel);

            dictionaryFaculties = await _indicatorService.GetDeansFaculties(userModel);

            if (dictionaryFaculties.Any())
            {
                var facultiesId = dictionaryFaculties.Select(f => f.Id).ToList();

                var temp_departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(d => facultiesId.Contains(d.FacultyId ?? -1)).ToListAsync();
                departments.AddRange(temp_departments);
            }

            if (!departments.Any())
            {
                var rector = await _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == User.GetId() && (e.Position.Equals("Ректор") || e.Position.StartsWith("Проректор")));

                if (rector != null)
                {
                    departments = await _dbContext.DictionaryDepartments.AsNoTracking().Where(f => f.UniversityUUID != null).ToListAsync();
                }
                else
                {
                    return Forbid();
                }
            }

            foreach (var department in departments.Distinct().ToList())
            {
                var indicators = await _indicatorService.GetDepartment(department.Id, date);

                if (indicators.Any())
                {
                    result.Add(new IndicatorGroupModel
                    {
                        Id = department.Id.ToString(),
                        Title = department.Title,
                        Indicators = indicators
                    });
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Получение показателей МИС.
        /// </summary>
        /// <response code="200">Показатели МИС.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<MedicalIndicatorModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("Medicals")]
        public async Task<IActionResult> GetMedicals()
        {
            var result = new List<MedicalIndicatorModel>();
            var indicators = await _dbContext.MedicalIndicators.AsNoTracking()
                .GroupBy(i => i.Code)
                .Select(i => new { i.Key, i.First().Title })
                .ToListAsync();

            foreach (var indicator in indicators)
            {
                result.Add(new MedicalIndicatorModel
                {
                    Code = indicator.Key,
                    Title = indicator.Title,
                });
            }

            return Ok(result);
        }

        /// <summary>
        /// Получение значения показателя МИС.
        /// </summary>
        /// <response code="200">Значения показателя МИС.</response>
        /// <response code="404">Данные не найдены.</response>
        [ProducesResponseType(typeof(IList<MedicalIndicatorModel>), 200)]
        [HasPermission(Permissions.GetIndicators)]
        [HttpGet("Medicals/{code}")]
        public async Task<IActionResult> GetMedicals(string code, DateTime? dateStart = null, DateTime? dateEnd = null)
        {
            var result = new List<MedicalIndicatorModel>();
            IQueryable<MedicalIndicator> indicators = _dbContext.MedicalIndicators.AsNoTracking()
                .Where(i => i.Code == code);

            if (dateStart.HasValue)
            {
                indicators = indicators.Where(i => i.Date >= dateStart.Value);
            }
            if (dateEnd.HasValue)
            {
                indicators = indicators.Where(i => i.Date <= dateEnd.Value);
            }
            foreach (var indicator in await indicators.OrderByDescending(i => i.Date).ToListAsync())
            {
                result.Add(_mapper.Map<MedicalIndicatorModel>(indicator));
            }

            return Ok(result);
        }
    }
}
