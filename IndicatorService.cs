using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SibMed.Journal.Clients;
using SibMed.Journal.Data;
using SibMed.Journal.Entities;
using SibMed.Journal.Models;
using SibMed.Journal.PermissionParts;
using SibMed.Journal.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;


namespace SibMed.Journal.Services
{
    public class IndicatorService : IIndicatorService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApiService _apiService;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public IndicatorService(ApplicationDbContext dbContext, IApiService apiService, IMapper mapper, IUserService userService)
        {
            _dbContext = dbContext;
            _apiService = apiService;
            _mapper = mapper;
            _userService = userService;
        }

        public async Task<IList<IndicatorModel>> GetUniversity(DateTime date)
        {
            var indicators = new List<IndicatorModel>();
            var headDivision = await _dbContext.DictionaryHeadDivisions.AsNoTracking().OrderBy(d => d.Id).FirstOrDefaultAsync();

            if (headDivision != null)
            {
                var result = await _apiService.GetIndicatorsBySubdivision(headDivision.UniversityUUID, date);

                if (result?.ЗначениеПоказателей?.Any() ?? false)
                {
                    foreach (var indicator in result.ЗначениеПоказателей)
                    {
                        indicators.Add(new IndicatorModel()
                        {
                            Id = indicator.GUIDПоказателя,
                            Group = indicator.ГруппаПоказателя,
                            Title = indicator.Показатель,
                            ActualValue = indicator.ФактическоеЗначение,
                            PlannedValue = indicator.ПлановоеЗначение,
                            PercentageCompletion = indicator.ПроцентВыполнения
                        });
                    }
                }
            }

            return indicators;
        }

        public async Task<IList<IndicatorModel>> GetIndicatorFaculties(string indicatorId, DateTime date, List<DictionaryFaculty> faculties)
        {
            var result = new List<IndicatorModel>();

            foreach (var faculty in faculties ?? Enumerable.Empty<DictionaryFaculty>())
            {
                var response = await _apiService.GetSubdivisionIndicator(faculty.UniversityUUID, indicatorId, date);

                if (response?.ЗначениеПоказателей?.Any() ?? false)
                {
                    var indicator = response.ЗначениеПоказателей.FirstOrDefault();

                    result.Add(new IndicatorModel
                    {
                        Id = faculty.Id.ToString(),
                        Title = faculty.Title,
                        ActualValue = indicator.ФактическоеЗначение,
                        PlannedValue = indicator.ПлановоеЗначение,
                        PercentageCompletion = indicator.ПроцентВыполнения
                    });
                }
            }

            return result;
        }

        public async Task<IList<IndicatorModel>> GetIndicatorDepartments(string indicatorId, DateTime date, List<DictionaryDepartment> deparments)
        {
            var faculties = new List<IndicatorModel>();

            foreach (var department in deparments ?? Enumerable.Empty<DictionaryDepartment>())
            {
                var result = await _apiService.GetSubdivisionIndicator(department.UniversityUUID, indicatorId, date);

                if (result?.ЗначениеПоказателей?.Any() ?? false)
                {
                    var indicator = result.ЗначениеПоказателей.FirstOrDefault();

                    faculties.Add(new IndicatorModel
                    {
                        Id = department.Id.ToString(),
                        Title = department.Title,
                        ActualValue = indicator.ФактическоеЗначение,
                        PlannedValue = indicator.ПлановоеЗначение,
                        PercentageCompletion = indicator.ПроцентВыполнения
                    });
                }
            }

            return faculties;
        }

        public async Task<IList<IndicatorModel>> GetIndicatorEmployees(string indicatorId, string departmentUUID, DateTime date, List<string> logins)
        {
            var employees = new List<IndicatorModel>();
            IndicatorValueDataModel result;

            if (string.IsNullOrEmpty(departmentUUID))
            {
                result = await _apiService.GetEmployeesSubdivisionIndicator(departmentUUID, indicatorId, logins, date);
            }
            else
            {
                result = await _apiService.GetEmployeesSubdivisionIndicator(null, indicatorId, logins, date);
            }

            if (result?.ЗначениеПоказателей?.Any() ?? false)
            {
                foreach (var indicator in result.ЗначениеПоказателей)
                {
                    employees.Add(new IndicatorModel
                    {
                        Id = indicator.GUIDФизическоеЛицо,
                        Title = indicator.ФизическоеЛицо,
                        ActualValue = indicator.ФактическоеЗначение,
                        PlannedValue = indicator.ПлановоеЗначение,
                        PercentageCompletion = indicator.ПроцентВыполнения
                    });
                }
            }

            return employees;
        }

        public async Task<IList<IndicatorModel>> GetFaculty(int facultyId, DateTime date)
        {
            var indicators = new List<IndicatorModel>();
            var faculty = await _dbContext.DictionaryFaculties.AsNoTracking().OrderBy(d => d.Id).FirstOrDefaultAsync(f => f.Id == facultyId);

            if (faculty != null)
            {
                var result = await _apiService.GetIndicatorsBySubdivision(faculty.UniversityUUID, date);

                if (result?.ЗначениеПоказателей?.Any() ?? false)
                {
                    foreach (var indicator in result.ЗначениеПоказателей)
                    {
                        indicators.Add(new IndicatorModel()
                        {
                            Id = indicator.GUIDПоказателя,
                            Group = indicator.ГруппаПоказателя,
                            Title = indicator.Показатель,
                            ActualValue = indicator.ФактическоеЗначение,
                            PlannedValue = indicator.ПлановоеЗначение,
                            PercentageCompletion = indicator.ПроцентВыполнения
                        });
                    }
                }
            }

            return indicators;
        }

        public async Task<IList<IndicatorModel>> GetDepartment(int departmentId, DateTime date)
        {
            var indicators = new List<IndicatorModel>();
            var department = await _dbContext.DictionaryDepartments.AsNoTracking().OrderBy(d => d.Id).FirstOrDefaultAsync(f => f.Id == departmentId);

            if (department != null)
            {
                var result = await _apiService.GetIndicatorsBySubdivision(department.UniversityUUID, date);

                if (result?.ЗначениеПоказателей?.Any() ?? false)
                {
                    foreach (var indicator in result.ЗначениеПоказателей)
                    {
                        indicators.Add(new IndicatorModel()
                        {
                            Id = indicator.GUIDПоказателя,
                            Group = indicator.ГруппаПоказателя,
                            Title = indicator.Показатель,
                            ActualValue = indicator.ФактическоеЗначение,
                            PlannedValue = indicator.ПлановоеЗначение,
                            PercentageCompletion = indicator.ПроцентВыполнения
                        });
                    }
                }
            }

            return indicators;
        }

        public async Task<bool> UpdateMedical(IndicatorDataModel model, ImportResult importResult)
        {
            if (string.IsNullOrWhiteSpace(model.ИдентификаторПоказателя) || model.ПериодРасчета == null)
            {
                return false;
            }

            var currentIndicator = await _dbContext.MedicalIndicators.FirstOrDefaultAsync(m => m.Code == model.ИдентификаторПоказателя && m.Date == model.ПериодРасчета);

            if (currentIndicator == null)
            {
                currentIndicator = new MedicalIndicator();
                await _dbContext.MedicalIndicators.AddAsync(currentIndicator);
                importResult.NewElementsCount++;
            }
            else
            {
                importResult.EditedElementsCount++;
            }
            _mapper.Map(model, currentIndicator);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<List<DictionaryFaculty>> GetDeansFaculties(AuthorizationUserModel userModel)
        {
            List<string> positions = new() { "Директор", "Заместитель директора", "Декан", "Заместитель декана" };
            List<DictionaryFaculty> facultyList = new();

            foreach (var employee in userModel.ВспомогательныеРеквизитыСотрудника?.Where(a => positions.Contains(a.Должность.Представление)) ?? Enumerable.Empty<EmployeeDataModel>())
            {
                var department = await _dbContext.DictionaryFaculties.FirstOrDefaultAsync(d => d.Title == employee.Подразделение.Представление);

                if (department != null)
                {
                    facultyList.Add(department);
                }
            }

            return facultyList;
        }

        public async Task<List<DictionaryDepartment>> GetHeadsDepatmetns(AuthorizationUserModel userModel)
        {
            List<DictionaryDepartment> departmentList = new();

            foreach (var employee in userModel.ВспомогательныеРеквизитыСотрудника?.Where(a => a.Должность.Представление.Equals("Заведующий кафедрой")) ?? Enumerable.Empty<EmployeeDataModel>())
            {
                var department = await _dbContext.DictionaryDepartments.AsNoTracking().FirstOrDefaultAsync(d => d.Title == employee.Подразделение.Представление);

                if (department != null)
                {
                    departmentList.Add(department);
                }
            }

            return departmentList;
        }
    }
}
