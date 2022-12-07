using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SFAA.Entities;
using SFAA.DataOperation;

namespace SFAA.DBAdapter
{
    using System;
    public class DBAdapterOperationPriem
    {
        /// <summary>
        /// Данный метод проверяет что существует запись в fdata
        /// </summary>
        /// <param name="teacherFnpp"></param>
        /// <returns></returns>
        public object CheckExistPersonByFnpp(string teacherFnpp)
        {
            using (var context = new studbaseEntities())
            {
                var fdata = context.fdata;
                var fnpp = 0;
                int.TryParse(teacherFnpp, out fnpp);

                var result = fdata.Include("keylinks").FirstOrDefault(t => t.npp == fnpp);
                return result;
            }
        }

        /// <summary>
        /// Данный метод проверяет что сотрудник зав каф или исполняющий обязанности
        /// </summary>
        /// <param name="keylinks"></param>
        /// <returns></returns>
        public List<byte[]> CheckChiefPersonByGalUnid(ICollection<keylinks> keylinks)
        {
            using (var context = new studbaseEntities())
            {
                var gal_chief = context.gal_chief;
                var galunid = (from one in keylinks where one.gal_unid != null select one.gal_unid).ToList();

                var result = gal_chief.Where(r => galunid.Contains(r.cperson) && r.isChief == 1).Select(r => r.cdepartment).ToList();
                
                return result;
            }
        }

        /// <summary>
        /// Данный метод получает все договора по общежитиям
        /// </summary>
        /// <returns></returns>
        public List<ListHostelContract> GetAllHostelContractFromDb()
        {
            var result = new List<ListHostelContract>();
            try
            {
                using (var context = new studbaseEntities())
                {
                    var query = from h in context.hostel_contract
                                from hh in context.hostel_housing.Where(r => h.housingId == r.id).DefaultIfEmpty()
                                from hs in context.hostel_settings.Where(r => r.name == String.Concat("h",hh.hostel,"FullAdd")).DefaultIfEmpty()
                                where (h.contType == 1 || h.contType == 2)
                                orderby h.id descending 
                                select new ListHostelContract
                                {
                                    NrecStudentString = h.student,
                                    Id = h.id,
                                    Fnpp = h.fnpp,
                                    ContNumber = h.contNumber,
                                    ContDate = h.contDate,
                                    Order = h.order,
                                    OrderDate = h.orderDate,
                                    ContBegin = h.contBegin,
                                    ContEnd = h.contEnd,
                                    Hostel = hh.hostel,
                                    Block = hh.block,
                                    Flat = hh.flat,
                                    Status = h.status,
                                    Reason = h.reason,
                                    HostelAddress = hs.value
                                };

                    foreach (var one in query.ToList())
                    {
                        one.NrecStudent = DataOperation.DataOperation.Instance.StringHexToByteArray(one.NrecStudentString);
                        
                        using (var galcontext = new GalDbContext())
                        {
                            var queryGal = $"SELECT dbo.toInt64({one.NrecStudentString}) as valueNrec";

                            var reader = galcontext.ExecuteQuery(queryGal);
                            if (reader.Read())
                            {
                                one.NrecStudentStringInt64 = reader.GetInt64(reader.GetOrdinal("valueNrec")).ToString();
                            }
                            else
                            {
                                one.NrecStudentStringInt64 = String.Empty;
                            }
                        }

                        result.Add(one);

                    }
                    context.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска договоров. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод получает все gal_srec по его fnpp
        /// </summary>
        /// <param name="studentFnpp"></param>
        /// <returns></returns>
        public List<byte[]> GetAllStudentNrecByFnpp(string studentFnpp)
        {
            using (var context = new studbaseEntities())
            {
                var skard = context.skard;

                var fnpp = 0;
                int.TryParse(studentFnpp, out fnpp);

                var result = skard.Where(t => t.fnpp == fnpp && t.gal_srec != null).Select(r => r.gal_srec).ToList(); 
                return result;
            }
        }

        /// <summary>
        /// Данный метод получает все gal_srec по его fnpp
        /// </summary>
        /// <param name="studentFnpp"></param>
        /// <returns></returns>
        public static List<JsonGroupStudentsList> GetGroupStudentsList(Dictionary<string, string> Student)
        {
            using (var context = new studbaseEntities())
            {
                var keylinks = context.keylinks;
                List<byte[]> keys = new List<byte[]>();
                foreach (var item in Student.Keys)
                {
                    keys.Add(DataOperation.DataOperation.Instance.StringHexToByteArray(item));
                }
                var result = (from k in keylinks
                              where keys.Contains(k.gal_unid)
                              select new
                             {FNPP = k.fnpp ?? 0, Nrec = k.gal_unid}).ToList();

                return result.Select(r => new JsonGroupStudentsList
                {
                    FNPP = r.FNPP,
                    Fio = Student.ContainsKey(DataOperation.DataOperation.Instance.ByteToString(r.Nrec)) ? Student[DataOperation.DataOperation.Instance.ByteToString(r.Nrec)] : "ФИО не найдено"
                }).ToList();
            }
        }

        /// <summary>
        /// Данный метод получает информацию о преподавателе по его fnpp
        /// </summary>
        public List<JsonLecture> GetLectureInfo(string fnpp)
        {
            using (var context = new studbaseEntities())
            {
                var fdata = context.fdata;
                var wkard = context.wkardc_rp;
                var fnpp_temp = 0;
                int.TryParse(fnpp, out fnpp_temp);

                var result = (from f in fdata
                              join w in wkard on f.npp equals w.fnpp
                              where w.fnpp == fnpp_temp && w.prudal == "0" && w.vpo1cat.Equals("ППС")
                              select new
                              {fam = f.fam, nam = f.nam, otc = f.otc, pos = w.dolgnost}).ToList();

                return result.Select(r => new JsonLecture
                {
                    Fnpp = fnpp,
                    Fio = r.fam + " " + r.nam + " " + r.otc,
                    Position = r.pos
                }).ToList();
            }
        }
    }
}
