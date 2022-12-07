using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.SqlServer;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using SFAA.Entities;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore.Internal;

namespace SFAA.DBAdapter
{
    using System;
    using System.Data;
    using System.Data.Entity;
    using System.Data.SqlClient;
    using System.Diagnostics;

    using SFAA.DataOperation;

    public class DBAdapterOperationGalaxy
    {
        /// <summary>
        ///Данный метод ищет в базе галактике все ведомости преподавателя по его keylinks из базы приема.
        /// </summary>
        /// <param name="personKeylinks">
        /// The person keylinks.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<JsonTeacherList> GetListOfTeacherByGalUnid(ICollection<keylinks> personKeylinks, List<byte[]> isChief)
        {
            var result = new List<JsonTeacherList>();
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var galunid = (from one in personKeylinks where one.gal_unid != null select one.gal_unid).ToList();

                    var listType = new List<int>
                        {(int) ListStatus.CLOSE, (int) ListStatus.OPEN};

                    var query = from l in context.T_U_LIST
                                from s in context.T_U_STUDGROUP.Where(r => l.F_CSTGR == r.F_NREC)
                                    .DefaultIfEmpty() // left join первый вариант
                                join chair in context.T_CATALOGS on l.F_CCHAIR equals chair.F_NREC into
                                    chairLeft //left join второй вариант
                                from chair in chairLeft.DefaultIfEmpty()
                                from facl in context.T_CATALOGS.Where(r => l.F_CFAC == r.F_NREC).DefaultIfEmpty()
                                from d in context.T_U_DISCIPLINE.Where(r => l.F_CDIS == r.F_NREC).DefaultIfEmpty()
                                from exam in context.T_PERSONS.Where(r => l.F_CEXAMINER == r.F_NREC).DefaultIfEmpty()
                                from typework in context.T_U_TYPEWORK.Where(r => l.F_CTYPEWORK == r.F_NREC).DefaultIfEmpty()
                                where ((galunid.Contains(l.F_CEXAMINER)
                                        || context.T_U_LIST_EXAMINER.Any(
                                            t => (t.F_CLIST == l.F_NREC && galunid.Contains(t.F_CPERSONS)))
                                        || (isChief.Contains(l.F_CCHAIR) || isChief.Contains(l.F_CFAC)))
                                       && listType.Contains(l.F_WSTATUS)
                                       && (s.F_WSTATUSGR == 0)
                                       && (l.F_CPARENT == DataOperation.Instance.GetNrecNull)
                                       && !context.T_U_CURR_DIS_STUDTRANS.Any(r => r.F_CLIST == l.F_NREC)
                                    )
                                // && (l.F_WTYPE == (int)UlistTypeEnum.Exam || l.F_WTYPE == (int)UlistTypeEnum.Ladder)
                                //  && (l.F_WFORMED != (int)FormEduEnum.Extramural && l.F_WHOURSAUD != 0)
                                select new JsonTeacherList
                                {
                                    Nrec = l.F_NREC,
                                    NumDoc = l.F_NUMDOC ?? string.Empty,
                                    Year = l.F_WYEARED,
                                    TypeListString = typework.F_NAME,
                                    Semester = (l.F_WSEMESTR % 2 == 0) ? "весенний" : "осенний",
                                    Status = (ListStatus)l.F_WSTATUS,
                                    FormEdu = l.F_WFORMED,
                                    StudGroup = s.F_NAME ?? string.Empty,
                                    ListChair = chair.F_LONGNAME ?? string.Empty,
                                    ListFacult = facl.F_LONGNAME ?? string.Empty,
                                    Discipline = d.F_NAME ?? string.Empty,
                                    Examiner = exam.F_FIO ?? string.Empty,
                                    DopStatusList = l.F_WADDFLD_10_,
                                    TypeListInt = l.F_WTYPE,
                                    ExaminerNrec = l.F_CEXAMINER,
                                    StudentCount = context.T_U_MARKS.Count(r => r.F_CLIST == l.F_NREC),
                                    MarkCount = (from l_sub in context.T_U_LIST
                                                 from m_sub in context.T_U_MARKS.Where(r => r.F_CLIST == l_sub.F_NREC)
                                                 where (l_sub.F_NREC == l.F_NREC || l_sub.F_CPARENT == l.F_NREC)
                                                       && m_sub.F_CMARK != DataOperation.Instance.GetNrecNull
                                                       && (m_sub.F_WENDRES == (int)MarkTypeEnum.Final ||
                                                           m_sub.F_WENDRES == (int)MarkTypeEnum.Recertification ||
                                                           m_sub.F_WENDRES == (int)MarkTypeEnum.Transfer)
                                                 select m_sub.F_NREC
                                        ).Count(),
                                };
                    foreach (var one in query.ToList())
                    {
                        one.NrecString = DataOperation.Instance.ByteToString(one.Nrec);

                        using (var galcontext = new GalDbContext())
                        {
                            var queryGal =
                                "SELECT TOP 1 dbo.toInt64(F$NREC) as valueNrec" + " FROM T$U_LIST " +
                                $"WHERE F$NREC = {one.NrecString} ";

                            var reader = galcontext.ExecuteQuery(queryGal);
                            if (reader.Read())
                            {
                                one.NrecInt64 = reader.GetInt64(reader.GetOrdinal("valueNrec")).ToString();
                            }
                            else
                            {
                                one.NrecInt64 = String.Empty;
                            }
                        }

                        try
                        {
                            one.ExaminerNrecString = DataOperation.Instance.ByteToString(one.ExaminerNrec);
                        }
                        catch (Exception)
                        {
                            //ignore
                        }

                        result.Add(one);

                    }
                    context.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска в базе ведомостей преподавателя. Ошибка {e}");
            }

            return result;

        }

        /// <summary>
        /// Данный метод возвращает информацию о ведомости по ее nrec
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public JsonStructList GetListByNrec(JsonStructList structList)
        {
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var query = from l in context.T_U_LIST
                                from s in context.T_U_STUDGROUP.Where(r => l.F_CSTGR == r.F_NREC).DefaultIfEmpty()
                                from exam in context.T_PERSONS.Where(r => l.F_CEXAMINER == r.F_NREC).DefaultIfEmpty()
                                from facl in context.T_CATALOGS.Where(r => l.F_CFAC == r.F_NREC).DefaultIfEmpty()
                                join chair in context.T_CATALOGS on l.F_CCHAIR equals chair.F_NREC into
                                    chairLeft //left join второй вариант
                                from chair in chairLeft.DefaultIfEmpty()
                                from d in context.T_U_DISCIPLINE.Where(r => l.F_CDIS == r.F_NREC).DefaultIfEmpty()
                                from w in context.T_UP_WRATING_HOURS.Where(r => l.F_NREC == r.F_CLIST).DefaultIfEmpty()
                                from typework in context.T_U_TYPEWORK.Where(r => l.F_CTYPEWORK == r.F_NREC).DefaultIfEmpty()
                                where l.F_NREC == structList.Nrec
                                orderby l.F_WYEARED descending
                                select new JsonStructList()
                                {
                                    NumDoc = l.F_NUMDOC ?? string.Empty,
                                    ListFacult = facl.F_NAME,
                                    StudGroup = s.F_NAME,
                                    Semester = l.F_WSEMESTR,
                                    Discipline = d.F_NAME,
                                    AudHoursTotalList = l.F_WHOURS,
                                    AudHoursList = l.F_WHOURSAUD,
                                    AudHoursCurr = (w.F_AUDHOURS != null) ? w.F_AUDHOURS : 0,
                                    DateList = l.F_DATEDOC,
                                    FormAttestationList = typework.F_NAME,
                                    TypeDiffer = l.F_WTYPEDIFFER,
                                    DisciplineAbbr = d.F_ABBR,
                                    ExaminerFio = exam.F_FIO,
                                    ListChair = chair.F_NAME,
                                    Status = (ListStatus)l.F_WSTATUS,
                                    DisciplineNrec = d.F_NREC,
                                    TypeList = l.F_WTYPE,
                                    DateOfCurHours = (w.F_CWDATE != null) ? w.F_CWDATE : 0,
                                    NrecKursList = (from kursList in context.T_U_LIST
                                                    where l.F_NREC != kursList.F_NREC
                                                          && l.F_CDIS == kursList.F_CDIS
                                                          && l.F_CCUR == kursList.F_CCUR
                                                          && l.F_CSTGR == kursList.F_CSTGR
                                                          && l.F_WYEARED == kursList.F_WYEARED
                                                          && l.F_WSEMESTR == kursList.F_WSEMESTR
                                                          && (kursList.F_WTYPE == (int)UlistTypeEnum.KursProject || kursList.F_WTYPE == (int)UlistTypeEnum.KursWork)
                                                          && kursList.F_CPARENT == DataOperation.Instance.GetNrecNull
                                                    select kursList.F_NREC).FirstOrDefault(),
                                    DopStatusList = l.F_WADDFLD_10_,
                                    ExaminerNrec = exam.F_NREC,
                                };

                    var result = query.FirstOrDefault();
                    if (result != null)
                    {
                        structList.NumDoc = result.NumDoc;
                        structList.ListFacult = result.ListFacult;
                        structList.StudGroup = result.StudGroup;
                        structList.Semester = result.Semester;
                        structList.Discipline = result.Discipline;
                        structList.AudHoursTotalList = result.AudHoursTotalList;
                        structList.AudHoursList = result.AudHoursList;
                        structList.AudHoursCurr = result.AudHoursCurr;
                        structList.DateList = result.DateList;
                        structList.FormAttestationList = result.FormAttestationList;
                        structList.TypeDiffer = result.TypeDiffer;
                        structList.DisciplineAbbr = result.DisciplineAbbr;
                        structList.ExaminerFio = result.ExaminerFio;
                        structList.ListChair = result.ListChair;
                        structList.Status = result.Status;
                        structList.TypeList = result.TypeList;
                        structList.DateOfCurHours = result.DateOfCurHours;

                        structList.DisciplineNrecString = DataOperation.Instance.ByteToString(result.DisciplineNrec);
                        structList.NrecKursList = result.NrecKursList;

                        structList.NrecKursListString = (result.NrecKursList != null) ? DataOperation.Instance.ByteToString(result.NrecKursList) : string.Empty;

                        structList.ExaminerNrec = result.ExaminerNrec;
                        structList.ExaminerNrecString = (result.ExaminerNrec != null) ? DataOperation.Instance.ByteToString(result.ExaminerNrec) : string.Empty;

                        structList.DopStatusList = result.DopStatusList;

                        using (var galcontext = new GalDbContext())
                        {
                            var queryGal =
                                "SELECT TOP 1 dbo.toInt64(F$NREC) as valueNrec" + " FROM T$U_LIST " +
                                $"WHERE F$NREC = {structList.NrecString} ";

                            var reader = galcontext.ExecuteQuery(queryGal);
                            if (reader.Read())
                            {
                                structList.NrecInt64 = reader.GetInt64(reader.GetOrdinal("valueNrec")).ToString();
                            }
                            else
                            {
                                structList.NrecInt64 = string.Empty;
                            }
                        }

                        var list = context.T_U_LIST.FirstOrDefault(r => r.F_NREC == structList.Nrec);
                        var temp_ListByte = new List<byte[]>();

                        var query2 = from l in context.T_U_LIST
                                     from c in context.T_U_CURRICULUM.Where(r => r.F_NREC == list.F_CCUR).DefaultIfEmpty()
                                     where l.F_NREC != list.F_NREC && l.F_NREC != structList.NrecKursList
                                           && l.F_CDIS == list.F_CDIS
                                           && (l.F_CCUR == list.F_CCUR || l.F_CCUR == c.F_CPARENT)
                                           //&& l.F_CSTGR == list.F_CSTGR
                                           && l.F_WYEARED == list.F_WYEARED
                                           && l.F_WSEMESTR == list.F_WSEMESTR
                                           && (l.F_WTYPE == (int)UlistTypeEnum.KursProject ||
                                               l.F_WTYPE == (int)UlistTypeEnum.KursWork)
                                           && l.F_CPARENT == DataOperation.Instance.GetNrecNull
                                     select new List<byte[]>() { l.F_NREC };

                        foreach (var one in query2.ToList())
                        {
                            temp_ListByte.Add(one.First());
                        }

                        structList.NrecKursListOther = temp_ListByte;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска в базе ведомости. Ошибка {e}");
            }

            return structList;
        }

        /// <summary>
        /// Данный метод возвращает информацию о ведомости для диплома по ее nrec
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public JsonStructListDip GetListDipByNrec(JsonStructListDip structList)
        {
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var query = from l in context.T_U_LIST
                                from s in context.T_U_STUDGROUP.Where(r => l.F_CSTGR == r.F_NREC).DefaultIfEmpty()
                                from exam in context.T_PERSONS.Where(r => l.F_CEXAMINER == r.F_NREC).DefaultIfEmpty()
                                from facl in context.T_CATALOGS.Where(r => l.F_CFAC == r.F_NREC).DefaultIfEmpty()
                                join chair in context.T_CATALOGS on l.F_CCHAIR equals chair.F_NREC into
                                    chairLeft //left join второй вариант
                                from chair in chairLeft.DefaultIfEmpty()
                                from d in context.T_U_DISCIPLINE.Where(r => l.F_CDIS == r.F_NREC).DefaultIfEmpty()
                                from typework in context.T_U_TYPEWORK.Where(r => l.F_CTYPEWORK == r.F_NREC).DefaultIfEmpty()
                                where l.F_NREC == structList.Nrec
                                orderby l.F_WYEARED descending
                                select new JsonStructList()
                                {
                                    NumDoc = l.F_NUMDOC ?? string.Empty,
                                    ListFacult = facl.F_NAME,
                                    StudGroup = s.F_NAME,
                                    Semester = l.F_WSEMESTR,
                                    Discipline = d.F_NAME,
                                    AudHoursTotalList = l.F_WHOURS,
                                    DateList = l.F_DATEDOC,
                                    FormAttestationList = typework.F_NAME,
                                    TypeDiffer = l.F_WTYPEDIFFER,
                                    DisciplineAbbr = d.F_ABBR,
                                    ExaminerFio = exam.F_FIO,
                                    ListChair = chair.F_NAME,
                                    Status = (ListStatus)l.F_WSTATUS,
                                    DisciplineNrec = d.F_NREC,
                                    TypeList = l.F_WTYPE,
                                    DopStatusList = l.F_WADDFLD_10_,
                                    ExaminerNrec = exam.F_NREC,
                                };

                    var result = query.FirstOrDefault();
                    if (result != null)
                    {
                        structList.NumDoc = result.NumDoc;
                        structList.ListFacult = result.ListFacult;
                        structList.StudGroup = result.StudGroup;
                        structList.Semester = result.Semester;
                        structList.Discipline = result.Discipline;
                        structList.AudHoursTotalList = result.AudHoursTotalList;
                        structList.DateList = result.DateList;
                        structList.FormAttestationList = result.FormAttestationList;
                        structList.TypeDiffer = result.TypeDiffer;
                        structList.DisciplineAbbr = result.DisciplineAbbr;
                        structList.ExaminerFio = result.ExaminerFio;
                        structList.ListChair = result.ListChair;
                        structList.Status = result.Status;
                        structList.TypeList = result.TypeList;

                        structList.DisciplineNrecString = DataOperation.Instance.ByteToString(result.DisciplineNrec);

                        structList.ExaminerNrec = result.ExaminerNrec;
                        structList.ExaminerNrecString = (result.ExaminerNrec != null) ? DataOperation.Instance.ByteToString(result.ExaminerNrec) : string.Empty;

                        structList.DopStatusList = result.DopStatusList;

                        using (var galcontext = new GalDbContext())
                        {
                            var queryGal =
                                "SELECT TOP 1 dbo.toInt64(F$NREC) as valueNrec" + " FROM T$U_LIST " +
                                $"WHERE F$NREC = {structList.NrecString} ";

                            var reader = galcontext.ExecuteQuery(queryGal);
                            if (reader.Read())
                            {
                                structList.NrecInt64 = reader.GetInt64(reader.GetOrdinal("valueNrec")).ToString();
                            }
                            else
                            {
                                structList.NrecInt64 = string.Empty;
                            }
                        }

                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска в базе ведомости для диплома. Ошибка {e}");
            }

            return structList;
        }

        /// <summary>
        /// Данный метод возвращает список студентов ведомости
        /// </summary>
        /// <param name="listNrec">
        /// Nrec ведомости
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<JsonStudentOfList> GetStudentFromListByNrecList(byte[] listNrec)
        {
            var result = new List<JsonStudentOfList>();

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var sublist = context.T_U_LIST.Where(r => r.F_CPARENT == listNrec).Select(r => r.F_NREC).ToList();
                    var allList = sublist;
                    allList.Add(listNrec);

                    var query = from stud in context.T_U_STUDENT
                                from persStud in context.T_PERSONS.Where(r => stud.F_CPERSONS == r.F_NREC).DefaultIfEmpty()
                                from m in context.T_U_MARKS.Where(r => r.F_NREC == (
                                                                      from sub_m in context.T_U_MARKS
                                                                      from sub_l in context.T_U_LIST.Where(sub_r => sub_r.F_NREC == sub_m.F_CLIST).DefaultIfEmpty()
                                                                      where allList.Contains(sub_l.F_NREC) && sub_m.F_CPERSONS == persStud.F_NREC
                                                                      orderby sub_m.F_WENDRES descending
                                                                      select sub_m.F_NREC
                                                                       ).FirstOrDefault()).DefaultIfEmpty()
                                from l in context.T_U_LIST.Where(r => r.F_NREC == m.F_CLIST).DefaultIfEmpty()
                                from cur in context.T_U_CURRICULUM.Where(r => l.F_CCUR == r.F_NREC).DefaultIfEmpty()
                                from markExam in context.T_PERSONS.Where(r => m.F_CPEREXAM == r.F_NREC).DefaultIfEmpty()
                                from cat in context.T_CATALOGS.Where(r => m.F_CMARK == r.F_NREC).DefaultIfEmpty()
                                from w in context.T_UP_WRATING.Where(r =>
                                        (l.F_NREC == r.F_CLIST && m.F_NREC == r.F_CMARKS && m.F_CPERSONS == r.F_CPERSONS))
                                    .DefaultIfEmpty()
                                from udb in context.T_U_DB_DIPLOMA
                                    .Where(r => (m.F_CPERSONS == r.F_CAUTHOR && r.F_NREC == m.F_CDB_DIP)).DefaultIfEmpty()
                                from dop in context.T_DOPINFO.Where(r =>
                                    (m.F_CPERSONS == r.F_CPERSON &&
                                     r.F_CDOPTBL == DataOperation.Instance.GetRecordBookNrecByte)).DefaultIfEmpty()
                                from tolerance in context.T_U_TOLERANCESESSION.Where(r => r.F_NREC == (
                                                                                              from sub_tolerance in context
                                                                                                  .T_U_TOLERANCESESSION
                                                                                              where sub_tolerance.F_CSTUDENT ==
                                                                                                    m.F_CPERSONS
                                                                                                    && sub_tolerance
                                                                                                        .F_WSEMESTER ==
                                                                                                    l.F_WSEMESTR
                                                                                                    && cur.F_CPARENT ==
                                                                                                    sub_tolerance.F_CPLAN
                                                                                              orderby sub_tolerance.F_NREC
                                                                                                  descending
                                                                                              select sub_tolerance.F_NREC)
                                                                                          .FirstOrDefault()).DefaultIfEmpty()
                                where allList.Contains(l.F_NREC)
                                orderby m.F_SFIO ascending
                                select new JsonStudentOfList()
                                {
                                    MarkStudNrec = m.F_NREC,
                                    Fio = m.F_SFIO,
                                    MarkString = cat.F_NAME,
                                    MarkNumber = m.F_WMARK,
                                    TotalStudHours = (w.F_THOURS != null) ? w.F_THOURS : 0,
                                    Percent = (w.F_PHOURS != null) ? w.F_PHOURS : 0,
                                    Rating = m.F_WADDFLD_4_,
                                    RecordBookExist = m.F_WADDFLD_10_,
                                    MarkExaminerNrec = markExam.F_NREC,
                                    MarkExaminerNrecFio = markExam.F_FIO,
                                    RatingSem = m.F_WADDFLD_1_,
                                    RatingAtt = m.F_WADDFLD_2_,
                                    MarkWendres = m.F_WENDRES,
                                    MarkLinkNumberNrec = m.F_CMARK,
                                    StudPersonNrec = m.F_CPERSONS,
                                    DbDipNrec = udb.F_NREC,
                                    RecordBookNumber = dop.F_SFLD_1_,
                                    Tolerance =
                                    (
                                        l.F_WTYPE == (int)UlistTypeEnum.Exam
                                            ? tolerance.F_WRESULTES
                                            : (l.F_WTYPE == (int)UlistTypeEnum.Practice ? (int?)null : tolerance.F_WRESULTZS)

                                    ),
                                    MarkListNumDoc = l.F_NUMDOC,
                                    MarkListType = l.F_WTYPE
                                };

                    foreach (var one in query.ToList())
                    {
                        one.MarkStudNrecString = DataOperation.Instance.ByteToString(one.MarkStudNrec);
                        one.StudPersonNrecString = DataOperation.Instance.ByteToString(one.StudPersonNrec);
                        try
                        {
                            one.MarkExaminerNrecString = DataOperation.Instance.ByteToString(one.MarkExaminerNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.MarkLinkNumberNrecString = DataOperation.Instance.ByteToString(one.MarkLinkNumberNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.DbDipNrecString = DataOperation.Instance.ByteToString(one.DbDipNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        one.RatingRes = one.RatingAtt + one.RatingSem;
                        result.Add(one);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод возвращает список студентов ведомости
        /// </summary>
        /// <param name="listNrec">
        /// Nrec ведомости
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<JsonStudentOfListDip> GetStudentFromListDipByNrecList(byte[] listNrec)
        {
            var result = new List<JsonStudentOfListDip>();

            try
            {
                var level = DataOperation.Instance.StringHexToByteArray("0x8001000000002ba2");

                using (var context = new OMGTU810Entities())
                {
                    var query = from l in context.T_U_LIST
                                from m in context.T_U_MARKS.Where(r => r.F_CLIST == l.F_NREC).DefaultIfEmpty()
                                from stud in context.T_U_STUDENT.Where(r => m.F_CPERSONS == r.F_NREC).DefaultIfEmpty()
                                from persStud in context.T_PERSONS.Where(r => stud.F_CPERSONS == r.F_NREC).DefaultIfEmpty()
                                from cur in context.T_U_CURRICULUM.Where(r => l.F_CCUR == r.F_NREC).DefaultIfEmpty()
                                from cat in context.T_CATALOGS.Where(r => m.F_CMARK == r.F_NREC).DefaultIfEmpty()
                                from udb in context.T_U_DB_DIPLOMA
                                    .Where(r => (m.F_CPERSONS == r.F_CAUTHOR &&
                                                 (r.F_WTYPEDOC == 5 || r.F_WTYPEDOC == 6 || r.F_WTYPEDOC == 7)))
                                    .DefaultIfEmpty()
                                from dop in context.T_DOPINFO.Where(r =>
                                    (m.F_CPERSONS == r.F_CPERSON &&
                                     r.F_CDOPTBL == DataOperation.Instance.GetRecordBookNrecByte)).DefaultIfEmpty()
                                from edu in context.T_EDUCATION.Where(r => r.F_PERSON == m.F_CPERSONS
                                                                           && r.F_IATTR == 0
                                                                           && r.F_SPECIALITY !=
                                                                           DataOperation.Instance.GetNrecNull
                                                                           && r.F_LEVEL != level
                                ).DefaultIfEmpty()

                                from tolerance in context.T_CONTDOC.Where(r => r.F_NREC ==
                                                                               (from sub_tolerance in context.T_CONTDOC
                                                                                where
                                                                            sub_tolerance.F_TYPEOPER == 30087
                                                                            && sub_tolerance.F_PERSON == m.F_CPERSONS
                                                                            && sub_tolerance.F_CNEWINF == l.F_CSTGR
                                                                            && sub_tolerance.F_CSTR ==
                                                                            (
                                                                                from sub_person in context.T_PERSONS
                                                                                where sub_person.F_NREC ==
                                                                                      m.F_CPERSONS
                                                                                select sub_person.F_APPOINTCUR
                                                                            ).FirstOrDefault()
                                                                                select sub_tolerance.F_NREC).FirstOrDefault()
                                ).DefaultIfEmpty()
                                where l.F_NREC == listNrec && udb.F_NREC != DataOperation.Instance.GetNrecNull &&
                                      (l.F_WTYPE == 5 || l.F_WTYPE == 6)
                                orderby m.F_SFIO ascending
                                select new JsonStudentOfListDip()
                                {
                                    MarkStudNrec = m.F_NREC,
                                    Fio = m.F_SFIO,
                                    MarkString = cat.F_NAME,
                                    MarkNumber = m.F_WMARK,
                                    RecordBookExist = m.F_WADDFLD_10_,
                                    MarkWendres = m.F_WENDRES,
                                    MarkLinkNumberNrec = m.F_CMARK,
                                    StudPersonNrec = m.F_CPERSONS,
                                    RecordBookNumber = dop.F_SFLD_1_,
                                    DataProto = edu.F_DAT_1,
                                    NumberProto = edu.F_STR1,
                                    EduNrec = edu.F_NREC,
                                    TitleDip = udb.F_SNAME,
                                    DbDipNrec = udb.F_NREC,
                                    MarkListNumDoc = l.F_NUMDOC,
                                    MarkListType = l.F_WTYPE,
                                    ToleranceNrec = tolerance.F_NREC
                                };

                    foreach (var one in query.ToList())
                    {
                        one.MarkStudNrecString = DataOperation.Instance.ByteToString(one.MarkStudNrec);
                        one.StudPersonNrecString = DataOperation.Instance.ByteToString(one.StudPersonNrec);

                        try
                        {
                            one.MarkLinkNumberNrecString = DataOperation.Instance.ByteToString(one.MarkLinkNumberNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.DbDipNrecString = DataOperation.Instance.ByteToString(one.DbDipNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.ToleranceNrecString = DataOperation.Instance.ByteToString(one.ToleranceNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.EduNrecString = DataOperation.Instance.ByteToString(one.EduNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        result.Add(one);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости для диплома. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод получает список всех экзаменаторов ведомости
        /// </summary>
        /// <param name="listNrec">Nrec нужно ведомости</param>
        /// <returns></returns>
        public List<JsonListExaminerOfList> GetListExaminerFromListByNrecList(byte[] listNrec)
        {
            var result = new List<JsonListExaminerOfList>();

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var query = from l in context.T_U_LIST_EXAMINER
                                from p in context.T_PERSONS.Where(r => l.F_CPERSONS == r.F_NREC).DefaultIfEmpty()
                                orderby l.F_NREC ascending
                                where l.F_CLIST == listNrec
                                select new JsonListExaminerOfList()
                                {
                                    FioExaminer = p.F_FIO,
                                    Nrec = l.F_NREC,
                                    NrecExaminer = p.F_NREC,
                                };

                    foreach (var one in query.ToList())
                    {
                        one.NrecString = string.Concat(
                            "0x",
                            BitConverter.ToString(one.Nrec).Replace("-", string.Empty));
                        one.NrecExaminerString = string.Concat(
                            "0x",
                            BitConverter.ToString(one.NrecExaminer).Replace("-", string.Empty));
                        result.Add(one);
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска всех экзаменаторов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод обновляет количество часов прошедших по ведомости
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdateListHoursByNrec(JsonStructList structList)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var query = context.T_UP_WRATING_HOURS.FirstOrDefault(x => x.F_CLIST == structList.Nrec);
                            if (query == null)
                            {
                                var model = new T_UP_WRATING_HOURS()
                                {
                                    F_AUDHOURS = structList.AudHoursCurr,
                                    F_CLIST = structList.Nrec,
                                    F_CWDATE = structList.DateOfCurHours,
                                };
                                context.T_UP_WRATING_HOURS.Add(model);
                            }
                            else
                            {
                                query.F_AUDHOURS = structList.AudHoursCurr;
                                query.F_CWDATE = structList.DateOfCurHours;
                            }

                            context.SaveChanges();
                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления часов ведомости произошла ошибка.Ошибка {e}");
                            transaction.Rollback();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод выполняет обновление оценок по ведомости для диплома
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdateDipMarkByNrecTableMark(JsonStructListDip structList)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var temp_structList = this.GetStudentFromListDipByNrecList(structList.Nrec);
                            foreach (var studentList in structList.Student)
                            {
                                var mark =
                                    context.T_U_MARKS.FirstOrDefault(x => x.F_NREC == studentList.MarkStudNrec);
                                var dip = temp_structList.Where(r => r.MarkStudNrec.SequenceEqual(studentList.MarkStudNrec))
                                    .Select(r => r.DbDipNrec).FirstOrDefault();
                                var temp_edu = temp_structList.Where(r => r.MarkStudNrec.SequenceEqual(studentList.MarkStudNrec))
                                    .Select(r => r.EduNrec).FirstOrDefault();

                                var res2 = 0;
                                if (studentList.MarkLinkNumberNrec.SequenceEqual(DataOperation.Instance.GetNrecNull))
                                {
                                    res2 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_MARKS SET " +
                                        "F$DATEMARK = dbo.ToAtlDate(GETDATE()), " +
                                        "F$WSTATUS = 0," +
                                        "F$WMARK = 0, " +
                                        "F$CPEREXAM = @examiner, " +
                                        "F$CMARK = @cmark, " +
                                        "F$WADDFLD#1# = 0," +
                                        "F$WADDFLD#2# = 0,  " +
                                        "F$WENDRES = @wendres " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", studentList.MarkStudNrec),
                                        new SqlParameter("@examiner", DataOperation.Instance.GetNrecNull),
                                        new SqlParameter("@cmark", studentList.MarkLinkNumberNrec),
                                        new SqlParameter("@wendres", MarkTypeEnum.Current)
                                    );
                                }
                                else
                                {


                                    res2 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_MARKS SET " +
                                        "F$DATEMARK = dbo.ToAtlDate(GETDATE()), " +
                                        "F$WSTATUS = (SELECT CASE WHEN F$CODE>2 THEN 2 WHEN F$CODE<0 THEN 0 ELSE 0 END FROM .dbo.T$CATALOGS WHERE F$NREC = @cmark)," +
                                        "F$WMARK = (SELECT F$CODE FROM .dbo.T$CATALOGS WHERE F$NREC=@cmark), " +
                                        "F$CMARK = @cmark, " +
                                        "F$WENDRES = @wendres, " +
                                        "F$CDB_DIP = @cdbdip " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", studentList.MarkStudNrec),
                                        new SqlParameter("@cmark", studentList.MarkLinkNumberNrec),
                                        new SqlParameter("@wendres", MarkTypeEnum.Final),
                                        new SqlParameter("@cdbdip", dip)
                                    );
                                }



                                if (studentList.NumberProto.Length != 0 && temp_edu != null)
                                {
                                    var res3 = context.Database.ExecuteSqlCommand(
                                         "UPDATE dbo.T$EDUCATION SET " +
                                         "F$STR1 = @str " +
                                         "WHERE F$NREC = @nrec",
                                         new SqlParameter("@nrec", temp_edu),
                                         new SqlParameter("@str", studentList.NumberProto)
                                     );
                                    Logger.Log.Debug($"Обновили записей в Education {res3}");
                                }

                                if (studentList.DataProto != 0 && temp_edu != null && dip != null)
                                {
                                    var res3 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$EDUCATION SET " +
                                        "F$DAT_1 = @str " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", temp_edu),
                                        new SqlParameter("@str", studentList.DataProto)
                                    );

                                    Logger.Log.Debug($"Обновили записей в Education часть 2 {res3}");

                                    var res4 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_DB_DIPLOMA SET " +
                                        "F$DDATEDOC = @str " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", dip),
                                        new SqlParameter("@str", studentList.DataProto)
                                    );

                                    Logger.Log.Debug($"Обновили записей в U_DB_DIPLOMA {res4}");
                                }

                                Logger.Log.Debug($"Обновили записей {res2}");


                                context.SaveChanges();
                            }

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления оценок по ведомостия для диплома произошла ошибка. Ошибка {e}");
                            transaction.Rollback();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод обновляет в таблицах U_WRATING_HOURS и U_MARKS информацию о рейтингах и часах студентах
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdateStudListHoursRating(JsonStructList structList)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var studentList in structList.Student)
                            {
                                var mark =
                                    context.T_U_MARKS.FirstOrDefault(x => x.F_NREC == studentList.MarkStudNrec);

                                var query = context.T_UP_WRATING.FirstOrDefault(x => x.F_CLIST == structList.Nrec && x.F_CMARKS == studentList.MarkStudNrec && x.F_CPERSONS == mark.F_CPERSONS);
                                if (query == null)
                                {
                                    var model = new T_UP_WRATING()
                                    {
                                        F_CLIST = structList.Nrec,
                                        F_CMARKS = mark.F_NREC,
                                        F_CPERSONS = mark.F_CPERSONS,
                                        F_PHOURS = studentList.Percent,
                                        F_THOURS = studentList.TotalStudHours,
                                    };

                                    context.T_UP_WRATING.Add(model);
                                    /*var resModel = context.Database.ExecuteSqlCommand(
                                        "INSERT INTO dbo.T$UP_WRATING (F$CLIST, F$CMARKS,F$CPERSONS,F$PHOURS,F$THOURS)"
                                        + "VALUES (@clist, @cmark, @cpersons, @percent, @totalStudHours)",
                                        new SqlParameter("@clist", structList.Nrec),
                                        new SqlParameter("@cmark", mark.F_NREC),
                                        new SqlParameter("@cpersons", mark.F_CPERSONS),
                                        new SqlParameter("@percent", studentList.Percent),
                                        new SqlParameter("@totalStudHours", studentList.TotalStudHours));*/
                                }
                                else
                                {
                                    query.F_PHOURS = studentList.Percent;
                                    query.F_THOURS = studentList.TotalStudHours;
                                }

                                var res2 = context.Database.ExecuteSqlCommand(
                                    "UPDATE dbo.T$U_MARKS SET F$WADDFLD#4# = @rating WHERE F$NREC = @nrec",
                                    new SqlParameter("@rating", studentList.Rating),
                                    new SqlParameter("@nrec", studentList.MarkStudNrec));

                                Logger.Log.Debug($"Обновили записей {res2}");
                                //mark.F_WRATING = studentList.Rating;

                                context.SaveChanges();
                            }

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления рейтинга и часов по студентам из ведомости произошла ошибка. Ошибка {e}");
                            transaction.Rollback();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод обновляет оценки и рейтинги в таблице U_MARKS по каждому студенту
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdateMarkAndRaingByNrecTableMark(JsonStructList structList)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var studentList in structList.Student)
                            {
                                var mark =
                                    context.T_U_MARKS.FirstOrDefault(x => x.F_NREC == studentList.MarkStudNrec);

                                //if (mark != null)
                                //{
                                //    mark.F_WMARK = (short)studentList.MarkNumber;
                                //    mark.F_CPEREXAM = studentList.MarkExaminerNrec;
                                //    mark.F_WADDFLD_1_ = studentList.RatingSem ?? 0;
                                //    mark.F_WADDFLD_2_ = studentList.RatingAtt ?? 0;
                                //}
                                var res2 = 0;
                                if (studentList.MarkLinkNumberNrec.SequenceEqual(DataOperation.Instance.GetNrecNull))
                                {
                                    res2 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_MARKS SET " +
                                        "F$DATEMARK = dbo.ToAtlDate(GETDATE()), " +
                                        "F$WSTATUS = 0," +
                                        "F$WMARK = 0, " +
                                        "F$CPEREXAM = @examiner, " +
                                        "F$CMARK = @cmark, " +
                                        "F$WADDFLD#1# = 0," +
                                        "F$WADDFLD#2# = 0,  " +
                                        "F$WADDFLD#10# = @rbexsits,  " +
                                        "F$WENDRES = @wendres " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", studentList.MarkStudNrec),
                                        new SqlParameter("@examiner", DataOperation.Instance.GetNrecNull),
                                        new SqlParameter("@cmark", studentList.MarkLinkNumberNrec),
                                        new SqlParameter("@rbexsits", studentList.RecordBookExist),
                                        new SqlParameter("@wendres", MarkTypeEnum.Current)
                                    );

                                }
                                else
                                {
                                    res2 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_MARKS SET " +
                                        "F$DATEMARK = dbo.ToAtlDate(GETDATE()), " +
                                        "F$WSTATUS = (SELECT CASE WHEN (F$CODE>2 OR F$CODE=1) THEN 2 WHEN F$CODE<0 THEN 0 ELSE 0 END FROM .dbo.T$CATALOGS WHERE F$NREC = @cmark)," +
                                        "F$WMARK = (SELECT F$CODE FROM .dbo.T$CATALOGS WHERE F$NREC=@cmark), " +
                                        "F$CPEREXAM = @examiner, " +
                                        "F$CMARK = @cmark, " +
                                        "F$WADDFLD#1# = @rsem," +
                                        "F$WADDFLD#2# = @ra,  " +
                                        "F$WADDFLD#10# = @rbexsits,  " +
                                        "F$WENDRES = @wendres " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", studentList.MarkStudNrec),
                                        new SqlParameter("@rsem", studentList.RatingSem),
                                        new SqlParameter("@ra", studentList.RatingAtt),
                                        new SqlParameter("@examiner", studentList.MarkExaminerNrec),
                                        new SqlParameter("@cmark", studentList.MarkLinkNumberNrec),
                                        new SqlParameter("@rbexsits", studentList.RecordBookExist),
                                        new SqlParameter("@wendres", MarkTypeEnum.Final)
                                    );

                                }

                                Logger.Log.Debug($"Обновили записей {res2}");


                                context.SaveChanges();
                            }

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления рейтинга и оценки по студентам из ведомости произошла ошибка. Ошибка {e}");
                            transaction.Rollback();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод формирует из галактики актуальный список оценок для зачетов и экзаменов
        /// </summary>
        /// <returns></returns>
        public List<JsonCatalogMarks> GetGatalogMarksFromDb()
        {
            var result = new List<JsonCatalogMarks>();
            var parentMark = DataOperation.Instance.GetParentCatalogsMarks().ToList();

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var query = from l in context.T_CATALOGS
                                from l2 in context.T_CATALOGS.Where(r => l.F_CPARENT == r.F_NREC).DefaultIfEmpty()
                                where parentMark.Contains(l2.F_NREC)
                                select new JsonCatalogMarks()
                                {
                                    Nrec = l.F_NREC,
                                    GroupName = l2.F_NAME,
                                    GroupNameNrec = l2.F_NREC,
                                    NameMark = l.F_NAME,
                                    CodeMark = l.F_CODE
                                };

                    foreach (var one in query.ToList())
                    {
                        one.NrecString = string.Concat(
                            "0x",
                            BitConverter.ToString(one.Nrec).Replace("-", string.Empty));
                        one.GroupNameNrecString = string.Concat(
                            "0x",
                            BitConverter.ToString(one.GroupNameNrec).Replace("-", string.Empty));

                        result.Add(one);
                    }

                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска каталога оценок. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод получает список всех студентов
        /// </summary>
        /// <returns></returns>
        public List<ListStudent> GetAllStudents(List<byte[]> studentNrec)
        {
            var result = new List<ListStudent>();
            try
            {
                var addQueryString = string.Empty;

                if (studentNrec.Any())
                {
                    using (var context = new OMGTU810Entities())
                    {
                        var personNrec =
                            context.T_U_STUDENT.Where(x => studentNrec.Contains(x.F_NREC)).Select(x => x.F_CPERSONS).ToList();

                        if (personNrec.Any())
                        {
                            foreach (var one in personNrec)
                            {
                                if (string.Equals(addQueryString, string.Empty))
                                {
                                    addQueryString = string.Concat(
                                        "0x",
                                        BitConverter.ToString(one).Replace("-", string.Empty));
                                }
                                else
                                {
                                    addQueryString = string.Concat(addQueryString, ", ", string.Concat(
                                        "0x",
                                        BitConverter.ToString(one).Replace("-", string.Empty)));

                                }

                            }
                        }
                    }

                    if (addQueryString.Length > 5)
                    {
                        addQueryString = string.Concat("tp.F$NREC IN (", addQueryString, ")");
                    }
                    else
                    {
                        addQueryString = "1=1";
                    }
                }
                else
                {
                    addQueryString = "1=1";
                }


                using (var galcontext = new GalDbContext())
                {
                    var queryGal =
                        "SELECT DISTINCT tp.F$NREC as nrec, dbo.toInt64(tp.F$NREC) as int64, tp.F$FIO as fio, tg.F$NAME as studGroup, cat.F$NAME as facLong, " +
                        "cat.F$LONGNAME as facShort, " +
                        "tp.F$SEX as Sex, " +
                        "tp.F$STRTABN as strtabn, " +
                        "ta.F$WPRIZN as formEdu, " +
                        "dbo.frmAtlDateGer(tp.F$BORNDATE) as BornDate, " +
                        "tus.F$SSTATUS as status, " +
                        "ta.F$VACATION as studCourse, " +
                        "spec.F$CODE + ' ' + spec.F$NAME as spec, " +
                        "finSourceName.F$NAME as finName, " +
                        "finSourceName.F$CODE as finNameCode, " +
                        "qual.F$NAME as Qual, " +
                        "dbo.frmAtlDateGer(taf.F$APPOINTDATE) as DateStudyStart, " +
                        "CASE WHEN contdocFirst.F$NREC IS NOT NULL THEN dbo.frmAtlDateGer(ttdFirst.F$DOCDATE) " +
                        "   WHEN tah.F$DOCDATE != 0 THEN dbo.frmAtlDateGer(tah.F$DOCDATE) " +
                        "ELSE \'\' END as OrderDateStudyStart , " +
                        "CASE WHEN contdocFirst.F$NREC IS NOT NULL THEN REPLACE(ttdFirst.F$DOCNMB, \' \', \'\') " +
                        "   WHEN tah.F$DOCNMB != \'\' THEN tah.F$DOCNMB " +
                        "ELSE \'\' END as OrderNumStudyStart, " +
                        "CASE WHEN tp.F$DISDATE != 0 THEN dbo.frmAtlDateGer(tp.F$DISDATE) ELSE dbo.frmAtlDateGer(baseCur.F$DATEEND) END as PlanStudyEnd, " +
                        "dog2.F$SFLD#1# as DogovorNum,  " +
                        "dbo.frmAtlDateGer(dog2.F$DFLD#1#) as DogovorDate,  " +
                        "rb.F$SFLD#1# as RecordBook, " +
                        "CONCAT(\'логин: \', prom.F$SFLD#1#, \', пароль: \', prom.F$SFLD#2#) as PromAuth, " +
                        "lib.F$SFLD#1# as LibTicket " +
                        "FROM T$U_STUDENT tus " +
                        "LEFT JOIN T$PERSONS tp on tus.F$CPERSONS = tp.F$NREC " +
                        "LEFT JOIN T$APPOINTMENTS ta on ta.F$NREC = " +
                        "       CASE " +
                        "       WHEN tp.F$APPOINTCUR = 0x8000000000000000 THEN tp.F$APPOINTLAST " +
                        "       ELSE tp.F$APPOINTCUR " +
                        "       END " +
                        "LEFT JOIN T$U_STUDGROUP tg on tg.F$NREC = ta.F$CCAT1 " +
                        "LEFT JOIN T$CATALOGS cat on cat.F$NREC = ta.F$PRIVPENSION " +
                        "LEFT JOIN T$CATALOGS spec on spec.F$NREC = ta.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS qual ON qual.F$NREC = ta.F$CREF1 " +
                        "LEFT JOIN T$U_STUD_FINSOURCE finSource on finSource.F$NREC = ta.F$CREF2 " +
                        "LEFT JOIN T$SPKAU finSourceName on finSourceName.F$NREC = finSource.F$CFINSOURCE " +
                        "LEFT JOIN T$APPOINTMENTS taf ON taf.F$NREC = tp.F$APPOINTFIRST " +
                        "LEFT JOIN T$APPHIST tah ON tah.F$CAPPOINT = taf.F$NREC AND tah.F$CODOPER IN (30001, 30052) " +
                        "LEFT JOIN T$CONTDOC contdocFirst ON contdocFirst.F$NREC = taf.F$CCONT AND contdocFirst.F$TYPEOPER IN (30001, 30052) " +
                        "LEFT JOIN T$CONTDOC contdocFirst2 ON contdocFirst2.F$NREC = contdocFirst.F$CDOPREF " +
                        "LEFT JOIN T$PARTDOC pdocFirst ON pdocFirst.F$NREC = contdocFirst.F$CPART " +
                        "LEFT JOIN T$TITLEDOC ttdFirst ON pdocFirst.F$CDOC = ttdFirst.F$NREC " +
                        "LEFT JOIN T$STAFFSTRUCT staff on ta.F$STAFFSTR = staff.F$NREC " +
                        "LEFT JOIN T$U_CURRICULUM baseCur on baseCur.F$NREC = staff.F$CSTR " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = tp.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                        "LEFT JOIN T$DOPINFO rb ON rb.F$CPERSON = tp.F$NREC and rb.F$CDOPTBL = 0x8001000000000003 " +
                        "LEFT JOIN T$DOPINFO prom ON prom.F$CPERSON = tp.F$NREC and prom.F$CDOPTBL = 0x800100000000001D and prom.F$SFLD#3# like \'Прометей\' " +
                        "LEFT JOIN T$DOPINFO lib ON lib.F$CPERSON = tp.F$NREC and lib.F$CDOPTBL = 0x800100000000001D and lib.F$SFLD#3# like \'Читательский билет\' " +
                        $"WHERE tp.F$FIO != '' and {addQueryString}";

                    var reader = galcontext.ExecuteQuery(queryGal);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var temp_Fio = reader.GetOrdinal("fio");
                            var temp_Status = reader.GetOrdinal("status");
                            var temp_StudGroup = reader.GetOrdinal("studGroup");
                            var temp_Int64 = reader.GetOrdinal("int64");
                            var temp_Nrec = reader.GetOrdinal("nrec");
                            var temp_FacLong = reader.GetOrdinal("facLong");
                            var temp_FacShort = reader.GetOrdinal("facShort");
                            var temp_Strtabn = reader.GetOrdinal("strtabn");
                            var temp_FormEdu = reader.GetOrdinal("formEdu");
                            var temp_studCourse = reader.GetOrdinal("studCourse");
                            var temp_Spec = reader.GetOrdinal("spec");
                            var temp_FinName = reader.GetOrdinal("finName");
                            var temp_FinNameCode = reader.GetOrdinal("FinNameCode");
                            var temp_BornDate = reader.GetOrdinal("BornDate");
                            var temp_Qual = reader.GetOrdinal("Qual");
                            var temp_Sex = reader.GetOrdinal("Sex");
                            var temp_DateStudyStart = reader.GetOrdinal("DateStudyStart");
                            var temp_OrderDateStudyStart = reader.GetOrdinal("OrderDateStudyStart");
                            var temp_OrderNumStudyStart = reader.GetOrdinal("OrderNumStudyStart");
                            var temp_PlanStudyEnd = reader.GetOrdinal("PlanStudyEnd");
                            var temp_DogovorNum = reader.GetOrdinal("DogovorNum");
                            var temp_DogovorDate = reader.GetOrdinal("DogovorDate");
                            var temp_RecordBook = reader.GetOrdinal("RecordBook");
                            var temp_LibTicket = reader.GetOrdinal("LibTicket");
                            var temp_PromAuth = reader.GetOrdinal("PromAuth");


                            var one = new ListStudent()
                            {
                                Fio = reader.IsDBNull(temp_Fio) ? null : reader.GetString(temp_Fio),
                                Status = reader.IsDBNull(temp_Status) ? null : reader.GetString(temp_Status),
                                StudGroup = reader.IsDBNull(temp_StudGroup) ? null : reader.GetString(temp_StudGroup),
                                NrecStringInt64 = reader.IsDBNull(temp_Int64) ? null : reader.GetInt64(temp_Int64).ToString(),
                                Nrec = reader.IsDBNull(temp_Nrec) ? DataOperation.Instance.GetNrecNull : reader.GetSqlBinary(temp_Nrec).Value,
                                FaculL = reader.IsDBNull(temp_FacLong) ? null : reader.GetString(temp_FacLong),
                                FaculS = reader.IsDBNull(temp_FacShort) ? null : reader.GetString(temp_FacShort),
                                Strtabn = reader.IsDBNull(temp_Strtabn) ? null : reader.GetString(temp_Strtabn),
                                FormEdu = reader.IsDBNull(temp_FormEdu) ? null : ((FormEduEnum)reader.GetInt32(temp_FormEdu)).GetDescription(),
                                Course = reader.IsDBNull(temp_studCourse) ? (short)0 : reader.GetInt16(temp_studCourse),
                                Spec = reader.IsDBNull(temp_Spec) ? null : reader.GetString(temp_Spec),
                                FinName = reader.IsDBNull(temp_FinName) ? null : reader.GetString(temp_FinName),
                                FinNameCode = reader.IsDBNull(temp_FinNameCode) ? null : reader.GetString(temp_FinNameCode),
                                BornDate = reader.IsDBNull(temp_BornDate) ? null : reader.GetString(temp_BornDate),
                                Qual = reader.IsDBNull(temp_Qual) ? null : reader.GetString(temp_Qual),
                                Sex = reader.IsDBNull(temp_Sex) ? null : reader.GetString(temp_Sex),
                                DateStudyStart = reader.IsDBNull(temp_DateStudyStart) ? null : reader.GetString(temp_DateStudyStart),
                                OrderDateStudyStart = reader.IsDBNull(temp_OrderDateStudyStart) ? null : reader.GetString(temp_OrderDateStudyStart),
                                OrderNumStudyStart = reader.IsDBNull(temp_OrderNumStudyStart) ? null : reader.GetString(temp_OrderNumStudyStart),
                                PlanStudyEnd = reader.IsDBNull(temp_PlanStudyEnd) ? null : reader.GetString(temp_PlanStudyEnd),
                                DogovorNum = reader.IsDBNull(temp_DogovorNum) ? null : reader.GetString(temp_DogovorNum),
                                DogovorDate = reader.IsDBNull(temp_DogovorDate) ? null : reader.GetString(temp_DogovorDate),
                                RecordBook = reader.IsDBNull(temp_RecordBook) ? null : reader.GetString(temp_RecordBook),
                                LibTicket = reader.IsDBNull(temp_LibTicket) ? null : reader.GetString(temp_LibTicket),
                                PromAuth = reader.IsDBNull(temp_PromAuth) ? null : reader.GetString(temp_PromAuth),
                            };
                            one.NrecString = DataOperation.Instance.ByteToString(one.Nrec);

                            result.Add(one);
                        }
                    }
                    else
                    {
                        reader.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска каталога оценок. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод выполняет удаление записи в таблице
        /// </summary>
        /// <param name="recordForDelete"></param>
        /// <returns></returns>
        public bool DeleteRecordFromTable(ListRecordDelete recordForDelete)
        {
            var result = false;
            using (var galcontext = new GalDbContext())
            {
                var queryGal =
                    $"DELETE FROM {recordForDelete.TableName} WHERE F$NREC = {recordForDelete.Nrec}";

                try
                {
                    galcontext.ExecuteQuery(queryGal);
                    result = true;
                }
                catch (Exception e)
                {
                    Logger.Log.Debug($"Ошибка при удалении записи. Ошибка {e}");
                }
            }

            return result;
        }

        /// <summary>
        /// Данный метод находит оценки студента из курсовой ведомости, относящейся к основной
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public JsonStructList GetMarkStudentOfKursListRelativeMainList(JsonStructList structList)
        {
            var markList = new List<int>()
            {
                5, 4, 3
            };

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var list = new List<byte[]>();
                    if (structList.NrecKursListOther.Any())
                    {
                        list = structList.NrecKursListOther;
                        if (structList.NrecKursList != null && structList.NrecKursList.SequenceEqual(DataOperation.Instance.GetNrecNull) == false)
                        {
                            list.Add(structList.NrecKursList);
                        }
                    }
                    else
                    {
                        list.Add(structList.NrecKursList);
                    }

                    foreach (var oneStudent in structList.Student)
                    {
                        var query = from l in context.T_U_LIST
                                    from m in context.T_U_MARKS.Where(r => l.F_NREC == r.F_CLIST).DefaultIfEmpty()
                                    where (list.Contains(l.F_NREC) || list.Contains(l.F_CPARENT))
                                          && markList.Contains(m.F_WMARK)
                                          && m.F_WENDRES != (int)MarkTypeEnum.Current
                                          && m.F_CPERSONS == oneStudent.StudPersonNrec
                                    orderby m.F_WMARK descending
                                    select m.F_WMARK;

                        oneStudent.MarkFromKursList = query.FirstOrDefault();

                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска оценок по курсовой работе. Ошибка {e}");
            }

            return structList;
        }

        /// <summary>
        /// Метод для обновления даты ведомости и дополнительного статуса
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdateDateAndDopStatus(JsonStructList structList)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            var dopStatus = (from l in context.T_U_LIST
                                             where l.F_NREC == structList.Nrec
                                             select new List<int>(){
                                l.F_WADDFLD_10_, l.F_DATEDOC
                            }).FirstOrDefault();

                            var res2 = context.Database.ExecuteSqlCommand(
                                "UPDATE dbo.T$U_LIST SET " +
                                "F$WADDFLD#10# = @dopStatus, " +
                                "F$DATEDOC = @dateList " +
                                "WHERE F$NREC = @nrec",
                                new SqlParameter("@nrec", structList.Nrec),
                                new SqlParameter("@dopStatus", structList.DopStatusList ?? dopStatus[0]),
                                new SqlParameter("@dateList", structList.DateList == 0 ? dopStatus[1] : structList.DateList)
                            );

                            Logger.Log.Debug($"Обновили записей {res2}");


                            context.SaveChanges();

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления даты ведомости и доп статуса произошла ошибка. Ошибка {e}");
                            transaction.Rollback();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении обновления даты ведомости и доп статуса произошла ошибка. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод получает структуру ведомости для запроса по курсовым темам
        /// </summary>
        /// <param name="structListKursTheme"></param>
        /// <returns></returns>
        public JsonKursTheme GetListByNrecForKursTheme(JsonKursTheme structListKursTheme)
        {
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var listTypeKurs = new List<int>()
                    {
                        (int)UlistTypeEnum.KursWork,
                        (int)UlistTypeEnum.KursProject,
                        (int)UlistTypeEnum.ExtraKursProject,
                        (int)UlistTypeEnum.ExtraKursWork,
                        (int)UlistTypeEnum.DirectKursProject,
                        (int)UlistTypeEnum.DirectKursWork,
                    };

                    var query = from l in context.T_U_LIST
                                from s in context.T_U_STUDGROUP.Where(r => l.F_CSTGR == r.F_NREC).DefaultIfEmpty()
                                from exam in context.T_PERSONS.Where(r => l.F_CEXAMINER == r.F_NREC).DefaultIfEmpty()
                                from facl in context.T_CATALOGS.Where(r => l.F_CFAC == r.F_NREC).DefaultIfEmpty()
                                from d in context.T_U_DISCIPLINE.Where(r => l.F_CDIS == r.F_NREC).DefaultIfEmpty()
                                where l.F_NREC == structListKursTheme.Nrec && listTypeKurs.Contains(l.F_WTYPE)
                                select new JsonKursTheme()
                                {
                                    NumDoc = l.F_NUMDOC ?? string.Empty,
                                    ListFacult = facl.F_NAME,
                                    StudGroup = s.F_NAME,
                                    Discipline = d.F_NAME,
                                    Status = (ListStatus)l.F_WSTATUS,
                                    DopStatusList = l.F_WADDFLD_10_,
                                    ExaminerNrec = l.F_CEXAMINER,
                                    ExaminerFio = exam.F_FIO
                                };

                    var result = query.FirstOrDefault();
                    if (result != null)
                    {
                        structListKursTheme.NumDoc = result.NumDoc;
                        structListKursTheme.ListFacult = result.ListFacult;
                        structListKursTheme.StudGroup = result.StudGroup;
                        structListKursTheme.Discipline = result.Discipline;
                        structListKursTheme.DopStatusList = result.DopStatusList;
                        structListKursTheme.ExaminerNrec = result.ExaminerNrec;
                        structListKursTheme.ExaminerFio = result.ExaminerFio;

                        try
                        {
                            structListKursTheme.ExaminerNrecString = DataOperation.Instance.ByteToString(structListKursTheme.ExaminerNrec);
                        }
                        catch (Exception)
                        {
                            //ignore
                        }

                        using (var galcontext = new GalDbContext())
                        {
                            var queryGal =
                                "SELECT TOP 1 dbo.toInt64(F$NREC) as valueNrec" + " FROM T$U_LIST " +
                                $"WHERE F$NREC = {structListKursTheme.NrecString} ";

                            var reader = galcontext.ExecuteQuery(queryGal);
                            if (reader.Read())
                            {
                                structListKursTheme.NrecInt64 = reader.GetInt64(reader.GetOrdinal("valueNrec")).ToString();
                            }
                            else
                            {
                                structListKursTheme.NrecInt64 = string.Empty;
                            }
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска в базе ведомости для курсовой работы. Ошибка {e}");
            }

            return structListKursTheme;
        }

        /// <summary>
        /// Данный метод получает темы студентов по курсовой работе на оснвое nrec ведомости
        /// </summary>
        /// <param name="listNrec"></param>
        /// <returns></returns>
        public List<JsonKursThemeOfStudent> GetStudentsKursThemeByNrecList(byte[] listNrec)
        {
            var result = new List<JsonKursThemeOfStudent>();

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var listTypeKurs = new List<int>()
                    {
                        (int)UlistTypeEnum.KursWork,
                        (int)UlistTypeEnum.KursProject,
                        (int)UlistTypeEnum.ExtraKursProject,
                        (int)UlistTypeEnum.ExtraKursWork,
                        (int)UlistTypeEnum.DirectKursProject,
                        (int)UlistTypeEnum.DirectKursWork,
                    };
                    var query = from l in context.T_U_LIST
                                from m in context.T_U_MARKS.Where(r => l.F_NREC == r.F_CLIST).DefaultIfEmpty()
                                from udb in context.T_U_DB_DIPLOMA.Where(r => (m.F_CPERSONS == r.F_CAUTHOR && r.F_NREC == m.F_CDB_DIP && l.F_CDIS == r.F_CDIS)).DefaultIfEmpty()
                                from teacher in context.T_PERSONS.Where(r => r.F_NREC == udb.F_CTEACHER).DefaultIfEmpty()
                                where l.F_NREC == listNrec && listTypeKurs.Contains(l.F_WTYPE)
                                orderby m.F_SFIO ascending
                                select new JsonKursThemeOfStudent()
                                {
                                    MarkStudNrec = m.F_NREC,
                                    Fio = m.F_SFIO,
                                    StudPersonNrec = m.F_CPERSONS,
                                    DbDipNrec = udb.F_NREC,
                                    KursTheme = udb.F_NREC != null ? udb.F_SNAME : string.Empty,
                                    KursThemeLastEdit = udb.F_NREC != null ? udb.F_ATL_LASTDATE : 0,
                                    KursThemeTeacherFio = udb.F_NREC != null ? teacher.F_FIO : string.Empty,
                                    KursThemeTeacherNrec = teacher.F_NREC,
                                };

                    foreach (var one in query.ToList())
                    {
                        one.MarkStudNrecString = DataOperation.Instance.ByteToString(one.MarkStudNrec);
                        one.StudPersonNrecString = DataOperation.Instance.ByteToString(one.StudPersonNrec);
                        try
                        {
                            one.DbDipNrecString = DataOperation.Instance.ByteToString(one.DbDipNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        try
                        {
                            one.KursThemeTeacherNrecString = DataOperation.Instance.ByteToString(one.KursThemeTeacherNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        result.Add(one);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска тем курсовых работ студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод выполняет вставку или обновление тем курсовых работ
        /// </summary>
        /// <param name="structJsonKursTheme"></param>
        /// <returns></returns>
        public bool ModifeKursThemeByMarkNrecIntoDb(JsonKursTheme structJsonKursTheme)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var studentList in structJsonKursTheme.Student)
                            {
                                var list = context.T_U_LIST.FirstOrDefault(x => x.F_NREC == structJsonKursTheme.Nrec);
                                var mark = context.T_U_MARKS.FirstOrDefault(x => x.F_NREC == studentList.MarkStudNrec);

                                var res2 = 0;
                                if (studentList.DbDipNrec.SequenceEqual(DataOperation.Instance.GetNrecNull))
                                {
                                    res2 = context.Database.ExecuteSqlCommand(
                                        "INSERT INTO dbo.T$U_DB_DIPLOMA (F$WTYPEDOC, F$SNAME, F$DDATEDOC, F$CAUTHOR, F$CTEACHER, F$CDIS, F$WADDFLD#1#, F$WADDFLD#2#) " +
                                        "VALUES (@typedoc, @name, dbo.ToAtlDate(GETDATE()), @author, @teacher, @dis, @semester, @year)",
                                        new SqlParameter("@typedoc", list.F_WTYPE),
                                        new SqlParameter("@name", studentList.KursTheme),
                                        new SqlParameter("@author", mark.F_CPERSONS),
                                        new SqlParameter("@teacher", studentList.KursThemeTeacherNrec),
                                        new SqlParameter("@dis", list.F_CDIS),
                                        new SqlParameter("@semester", list.F_WSEMESTR),
                                        new SqlParameter("@year", list.F_WYEARED)
                                    );
                                    if (res2 == 1)
                                    {
                                        var nrecRecord = context.T_U_DB_DIPLOMA.OrderByDescending(r => r.F_NREC).Select(r => r.F_NREC)
                                            .FirstOrDefault();

                                        if (nrecRecord != null && !nrecRecord.SequenceEqual(DataOperation.Instance.GetNrecNull))
                                        {
                                            res2 = context.Database.ExecuteSqlCommand(
                                                "UPDATE dbo.T$U_MARKS SET " +
                                                "F$CDB_DIP = @nrecRecord " +
                                                "WHERE F$NREC = @nrec",
                                                new SqlParameter("@nrec", studentList.MarkStudNrec),
                                                new SqlParameter("@nrecRecord", nrecRecord)
                                            );
                                        }
                                    }

                                }
                                else
                                {
                                    res2 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_DB_DIPLOMA SET " +
                                        "F$SNAME = @kursTheme, " +
                                        "F$CTEACHER = @kursTeacher, " +
                                        "F$WADDFLD#2# = @year, " +
                                        "F$WADDFLD#1# = @semestr " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", studentList.DbDipNrec),
                                        new SqlParameter("@kursTheme", studentList.KursTheme),
                                        new SqlParameter("@kursTeacher", studentList.KursThemeTeacherNrec),
                                        new SqlParameter("@semestr", list.F_WSEMESTR),
                                        new SqlParameter("@year", list.F_WYEARED)
                                    );

                                }

                                Logger.Log.Debug($"Обновили записей {res2}");


                                context.SaveChanges();
                            }

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления или вставке тем курсовых работ произошла ошибка. Ошибка {e}");
                            transaction.Rollback();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод ищет в базе галактике направления, привязанные к преподавателю по его keylinks из базы приема.
        /// </summary>
        /// <param name="personKeylinks"></param>
        /// <returns></returns>
        public List<JsonTeacherList> GetExtraListOfTeacherByGalUnidFromDb(ICollection<keylinks> personKeylinks)
        {
            int[] GrStatus = { 0, 1 };
            var result = new List<JsonTeacherList>();
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var galunid = (from one in personKeylinks where one.gal_unid != null select one.gal_unid).ToList();

                    var query = from l in context.T_U_LIST
                                from s in context.T_U_STUDGROUP.Where(r => l.F_CSTGR == r.F_NREC)
                                    .DefaultIfEmpty() // left join первый вариант
                                join chair in context.T_CATALOGS on l.F_CCHAIR equals chair.F_NREC into
                                    chairLeft //left join второй вариант
                                from chair in chairLeft.DefaultIfEmpty()
                                from facl in context.T_CATALOGS.Where(r => l.F_CFAC == r.F_NREC).DefaultIfEmpty()
                                from d in context.T_U_DISCIPLINE.Where(r => l.F_CDIS == r.F_NREC).DefaultIfEmpty()
                                from exam in context.T_PERSONS.Where(r => l.F_CEXAMINER == r.F_NREC).DefaultIfEmpty()
                                from typework in context.T_U_TYPEWORK.Where(r => l.F_CTYPEWORK == r.F_NREC).DefaultIfEmpty()
                                where (galunid.Contains(l.F_CEXAMINER)
                                       && l.F_WSTATUS == (int)ListStatus.OPEN
                                       && (GrStatus.Contains(s.F_WSTATUSGR))
                                       && (context.T_U_CURR_DIS_STUDTRANS.Any(r => r.F_CLIST == l.F_NREC) || l.F_CPARENT != DataOperation.Instance.GetNrecNull)
                                       )

                                select new JsonTeacherList
                                {
                                    Nrec = l.F_NREC,
                                    NumDoc = l.F_NUMDOC ?? string.Empty,
                                    Year = l.F_WYEARED,
                                    TypeListString = typework.F_NAME,
                                    Semester = (l.F_WSEMESTR % 2 == 0) ? "весенний" : "осенний",
                                    Status = (ListStatus)l.F_WSTATUS,
                                    FormEdu = l.F_WFORMED,
                                    StudGroup = s.F_NAME ?? string.Empty,
                                    ListChair = chair.F_LONGNAME ?? string.Empty,
                                    ListFacult = facl.F_LONGNAME ?? string.Empty,
                                    Discipline = d.F_NAME ?? string.Empty,
                                    Examiner = exam.F_FIO ?? string.Empty,
                                    DopStatusList = l.F_WADDFLD_10_,
                                    TypeListInt = l.F_WTYPE,
                                    ExaminerNrec = exam.F_NREC,
                                    StudentCount = context.T_U_MARKS.Count(r => r.F_CLIST == l.F_NREC),
                                    MarkCount = (from l_sub in context.T_U_LIST
                                                 from m_sub in context.T_U_MARKS.Where(r => r.F_CLIST == l_sub.F_NREC)
                                                 where (l_sub.F_NREC == l.F_NREC)
                                                       && m_sub.F_CMARK != DataOperation.Instance.GetNrecNull
                                                       && (m_sub.F_WENDRES == (int)MarkTypeEnum.Final ||
                                                           m_sub.F_WENDRES == (int)MarkTypeEnum.Recertification ||
                                                           m_sub.F_WENDRES == (int)MarkTypeEnum.Transfer)
                                                 select m_sub.F_NREC
                                        ).Count(),
                                };

                    foreach (var one in query.ToList())
                    {
                        one.NrecString = DataOperation.Instance.ByteToString(one.Nrec);
                        try
                        {
                            one.ExaminerNrecString = DataOperation.Instance.ByteToString(one.ExaminerNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        using (var galcontext = new GalDbContext())
                        {
                            var queryGal =
                                "SELECT TOP 1 dbo.toInt64(F$NREC) as valueNrec" + " FROM T$U_LIST " +
                                $"WHERE F$NREC = {one.NrecString} ";

                            var reader = galcontext.ExecuteQuery(queryGal);
                            if (reader.Read())
                            {
                                one.NrecInt64 = reader.GetInt64(reader.GetOrdinal("valueNrec")).ToString();
                            }
                            else
                            {
                                one.NrecInt64 = String.Empty;
                            }

                            queryGal = "SELECT F$SFIO as fio FROM T$U_MARKS " +
                                $"WHERE F$CLIST = {one.NrecString} ";
                            reader = galcontext.ExecuteQuery(queryGal);
                            while (reader.Read())
                            {
                                var oneStudent = !reader.IsDBNull(reader.GetOrdinal("fio")) ? reader.GetString(reader.GetOrdinal("fio")) : string.Empty;
                                one.Student.Add(oneStudent);
                            }
                        }

                        result.Add(one);

                    }
                    context.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска в базе направлений, привязанных к преподавателю. Ошибка {e}");
            }

            return result;
        }

        public bool UpdateEnterprise(JsonEnterprises structList)
        {
            return false;
        }

        /// <summary>
        /// Данный метод возвращает список студентов ведомости
        /// </summary>
        /// <param name="structListNrec"></param>
        /// <returns></returns>
        public List<JsonStudentOfList> GetStudentFromExtraListByNrecListFromDb(byte[] listNrec)
        {
            var result = new List<JsonStudentOfList>();

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var query = from l in context.T_U_LIST
                                from m in context.T_U_MARKS.Where(r => r.F_CLIST == l.F_NREC).DefaultIfEmpty()
                                from stud in context.T_U_STUDENT.Where(r => r.F_NREC == m.F_CPERSONS).DefaultIfEmpty()
                                from persStud in context.T_PERSONS.Where(r => stud.F_CPERSONS == r.F_NREC).DefaultIfEmpty()
                                from cur in context.T_U_CURRICULUM.Where(r => l.F_CCUR == r.F_NREC).DefaultIfEmpty()
                                from markExam in context.T_PERSONS.Where(r => m.F_CPEREXAM == r.F_NREC).DefaultIfEmpty()
                                from cat in context.T_CATALOGS.Where(r => m.F_CMARK == r.F_NREC).DefaultIfEmpty()
                                from w in context.T_UP_WRATING.Where(r =>
                                        (l.F_NREC == r.F_CLIST && m.F_NREC == r.F_CMARKS && m.F_CPERSONS == r.F_CPERSONS))
                                    .DefaultIfEmpty()
                                from udb in context.T_U_DB_DIPLOMA
                                    .Where(r => (m.F_CPERSONS == r.F_CAUTHOR && r.F_NREC == m.F_CDB_DIP)).DefaultIfEmpty()
                                from dop in context.T_DOPINFO.Where(r =>
                                    (m.F_CPERSONS == r.F_CPERSON &&
                                     r.F_CDOPTBL == DataOperation.Instance.GetRecordBookNrecByte)).DefaultIfEmpty()
                                from tolerance in context.T_U_TOLERANCESESSION.Where(r => r.F_NREC == (
                                                                                              from sub_tolerance in context
                                                                                                  .T_U_TOLERANCESESSION
                                                                                              where sub_tolerance.F_CSTUDENT ==
                                                                                                    m.F_CPERSONS
                                                                                                    && sub_tolerance
                                                                                                        .F_WSEMESTER ==
                                                                                                    l.F_WSEMESTR
                                                                                                    && cur.F_CPARENT ==
                                                                                                    sub_tolerance.F_CPLAN
                                                                                              orderby sub_tolerance.F_NREC
                                                                                                  descending
                                                                                              select sub_tolerance.F_NREC)
                                                                                          .FirstOrDefault()).DefaultIfEmpty()
                                where l.F_NREC == listNrec
                                orderby m.F_SFIO ascending
                                select new JsonStudentOfList()
                                {
                                    MarkStudNrec = m.F_NREC,
                                    Fio = m.F_SFIO,
                                    MarkString = cat.F_NAME,
                                    MarkNumber = m.F_WMARK,
                                    TotalStudHours = (w.F_THOURS != null) ? w.F_THOURS : 0,
                                    Percent = (w.F_PHOURS != null) ? w.F_PHOURS : 0,
                                    Rating = m.F_WADDFLD_4_,
                                    RecordBookExist = m.F_WADDFLD_10_,
                                    MarkExaminerNrec = markExam.F_NREC,
                                    MarkExaminerNrecFio = markExam.F_FIO,
                                    RatingSem = m.F_WADDFLD_1_,
                                    RatingAtt = m.F_WADDFLD_2_,
                                    MarkWendres = m.F_WENDRES,
                                    MarkLinkNumberNrec = m.F_CMARK,
                                    StudPersonNrec = m.F_CPERSONS,
                                    DbDipNrec = udb.F_NREC,
                                    RecordBookNumber = dop.F_SFLD_1_,
                                    Tolerance =
                                    (
                                        l.F_WTYPE == (int)UlistTypeEnum.Exam
                                            ? tolerance.F_WRESULTES
                                            : (l.F_WTYPE == (int)UlistTypeEnum.Practice ? (int?)null : tolerance.F_WRESULTZS)

                                    ),
                                    MarkListNumDoc = l.F_NUMDOC,
                                    MarkListType = l.F_WTYPE
                                };

                    foreach (var one in query.ToList())
                    {
                        one.MarkStudNrecString = DataOperation.Instance.ByteToString(one.MarkStudNrec);
                        one.StudPersonNrecString = DataOperation.Instance.ByteToString(one.StudPersonNrec);
                        try
                        {
                            one.MarkExaminerNrecString = DataOperation.Instance.ByteToString(one.MarkExaminerNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.MarkLinkNumberNrecString = DataOperation.Instance.ByteToString(one.MarkLinkNumberNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                        try
                        {
                            one.DbDipNrecString = DataOperation.Instance.ByteToString(one.DbDipNrec);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        one.RatingRes = one.RatingAtt + one.RatingSem;
                        result.Add(one);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод выполняет обновление или вставку экзаменатора
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdateExaminerIntoDb(JsonStructList structList)
        {
            var result = false;
            var curDate = this.GetGalDateFromDb();
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    T_U_LIST_EXAMINER currentListExam = null;
                    var currentList = context.T_U_LIST.FirstOrDefault(r => r.F_NREC == structList.Nrec);
                    if (currentList != null && !currentList.F_CEXAMINER.SequenceEqual(DataOperation.Instance.GetNrecNull))
                    {
                        currentListExam = context.T_U_LIST_EXAMINER.FirstOrDefault(r =>
                           r.F_CLIST == currentList.F_NREC && r.F_CPERSONS == currentList.F_CEXAMINER);
                    }

                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (!structList.ExaminerNrec.SequenceEqual(currentList.F_CEXAMINER) &&
                                !structList.ExaminerNrec.SequenceEqual(DataOperation.Instance.GetNrecNull))
                            {
                                var res2 = context.Database.ExecuteSqlCommand(
                                    "UPDATE dbo.T$U_LIST SET " +
                                    "F$CEXAMINER = @cexaminer " +
                                    "WHERE F$NREC = @nrec",
                                    new SqlParameter("@nrec", structList.Nrec),
                                    new SqlParameter("@cexaminer", structList.ExaminerNrec)
                                );
                            }
                            else if (structList.ExaminerNrec.SequenceEqual(DataOperation.Instance.GetNrecNull))
                            {
                                var res2 = context.Database.ExecuteSqlCommand(
                                    "UPDATE dbo.T$U_LIST SET " +
                                    "F$CEXAMINER = @cexaminer " +
                                    "WHERE F$NREC = @nrec",
                                    new SqlParameter("@nrec", structList.Nrec),
                                    new SqlParameter("@cexaminer", structList.ExaminerNrec)
                                );
                                if (currentListExam != null)
                                {
                                    var listDelete = new ListRecordDelete()
                                    {
                                        Nrec = DataOperation.Instance.ByteToString(currentListExam.F_NREC),
                                        TableName = "T$U_LIST_EXAMINER"
                                    };
                                    var resDel = this.DeleteRecordFromTable(listDelete);

                                }
                            }


                            foreach (var oneExaminer in structList.ListExaminer)
                            {
                                var employee = context.T_APPOINTMENTS.FirstOrDefault(r =>
                                    r.F_PERSON == oneExaminer.NrecExaminer
                                    && r.F_DEPARTMENT == currentList.F_CCHAIR
                                    && (r.F_DISMISSDATE == 0 || r.F_DISMISSDATE > curDate)
                                    && (r.F_DATEEND == 0 || r.F_DATEEND > curDate)
                                );

                                if (oneExaminer.Nrec.SequenceEqual(DataOperation.Instance.GetNrecNull))
                                {
                                    var res2 = context.Database.ExecuteSqlCommand(
                                        "INSERT INTO dbo.T$U_LIST_EXAMINER (F$CPERSONS, F$CPOST, F$CDEP, F$CLIST) " +
                                        "VALUES (@cpersons, @cpost, @dep, @clist)",
                                        new SqlParameter("@cpersons", oneExaminer.NrecExaminer),
                                        new SqlParameter("@cpost", employee.F_POST),
                                        new SqlParameter("@dep", employee.F_DEPARTMENT),
                                        new SqlParameter("@clist", currentList.F_NREC)
                                    );
                                }
                                else
                                {
                                    var res2 = context.Database.ExecuteSqlCommand(
                                        "UPDATE dbo.T$U_LIST_EXAMINER SET " +
                                        "F$CPERSONS = @cpersons, " +
                                        "F$CPOST = @cpost, " +
                                        "F$CDEP = @dep, " +
                                        "F$CLIST = @clist " +
                                        "WHERE F$NREC = @nrec",
                                        new SqlParameter("@nrec", oneExaminer.Nrec),
                                        new SqlParameter("@cpersons", oneExaminer.NrecExaminer),
                                        new SqlParameter("@cpost", employee.F_POST),
                                        new SqlParameter("@dep", employee.F_DEPARTMENT),
                                        new SqlParameter("@clist", currentList.F_NREC)
                                    );
                                }
                            }

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
                            transaction.Rollback();
                            result = false;
                        }

                    }

                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска студентов ведомости. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод получает список сотрудников
        /// </summary>
        /// <param name="actionData"></param>
        /// <returns></returns>
        public List<ListEmployee> GetStuffFromDb(ActionData actionData)
        {
            var nrec = DataOperation.Instance.StringHexToByteArray(actionData.NrecOneRecord);
            var curDate = 0;
            var result = new List<ListEmployee>();
            try
            {
                using (var galcontext = new GalDbContext())
                {
                    var queryGal =
                        "SELECT dbo.ToAtlDate(GETDATE()) as curDate";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    if (reader.Read())
                    {
                        curDate = reader.GetInt32(reader.GetOrdinal("curDate"));
                    }
                }

                if (actionData.OperationType == OperationTypeEnum.GetStuffForList)
                {
                    using (var context = new OMGTU810Entities())
                    {
                        var currentList = context.T_U_LIST.FirstOrDefault(r => r.F_NREC == nrec);
                        var paramAll = Int32.Parse(actionData.QueryParamNum1);
                        var query = from p in context.T_PERSONS
                                    join appoint in context.T_APPOINTMENTS on p.F_NREC equals appoint.F_PERSON
                                    from dep in context.T_CATALOGS.Where(r => r.F_NREC == appoint.F_DEPARTMENT).DefaultIfEmpty()
                                    from post in context.T_CATALOGS.Where(r => r.F_NREC == appoint.F_POST).DefaultIfEmpty()
                                    where (p.F_ISEMPLOYEE == "С"
                                           && (p.F_DISDATE == 0 || p.F_DISDATE > curDate)
                                           && appoint.F_NREC != DataOperation.Instance.GetNrecNull
                                           && (appoint.F_DISMISSDATE == 0 || appoint.F_DISMISSDATE > curDate)
                                           && (appoint.F_DATEEND == 0 || appoint.F_DATEEND > curDate)
                                           && (paramAll == 0 ? appoint.F_DEPARTMENT == currentList.F_CCHAIR : 1 == 1)
                                           )
                                    select new ListEmployee()
                                    {
                                        Nrec = p.F_NREC,
                                        Fio = p.F_FIO,
                                        DepNrec = dep.F_NREC,
                                        PostNrec = post.F_NREC,
                                        Dep = dep.F_NAME,
                                        Post = post.F_NAME,

                                    };

                        foreach (var one in query.ToList())
                        {
                            one.NrecString = DataOperation.Instance.ByteToString(one.Nrec);

                            try
                            {
                                one.DepNrecString = DataOperation.Instance.ByteToString(one.DepNrec);
                            }
                            catch (Exception)
                            {
                                one.DepNrecString = DataOperation.Instance.GetNrecNullString;
                            }

                            try
                            {
                                one.PostNrecString = DataOperation.Instance.ByteToString(one.PostNrec);
                            }
                            catch (Exception)
                            {
                                one.PostNrecString = DataOperation.Instance.GetNrecNullString;
                            }

                            result.Add(one);

                        }
                        context.Dispose();

                    }
                }

            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска ППС. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод получает дату в галактическом формате
        /// </summary>
        /// <returns></returns>
        private int GetGalDateFromDb()
        {
            var curDate = 0;
            using (var galcontext = new GalDbContext())
            {
                var queryGal =
                    "SELECT dbo.ToAtlDate(GETDATE()) as curDate";

                var reader = galcontext.ExecuteQuery(queryGal);
                if (reader.Read())
                {
                    curDate = reader.GetInt32(reader.GetOrdinal("curDate"));
                }
            }

            return curDate;
        }

        /// <summary>
        /// Данный метод ишет все направления для студента
        /// </summary>
        /// <param name="actionDataNrecOneRecord"></param>
        /// <returns></returns>
        public List<JsonExtraListForStudent> GetExtraListForStudentFromDb(string actionDataNrecOneRecord)
        {
            var result = new List<JsonExtraListForStudent>();
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    var studNrec = DataOperation.Instance.StringHexToByteArray(actionDataNrecOneRecord);

                    var query = from l in context.T_U_LIST
                                from m in context.T_U_MARKS.Where(r => l.F_NREC == r.F_CLIST).DefaultIfEmpty()
                                from s in context.T_U_STUDGROUP.Where(r => l.F_CSTGR == r.F_NREC)
                                    .DefaultIfEmpty() // left join первый вариант
                                join chair in context.T_CATALOGS on l.F_CCHAIR equals chair.F_NREC into
                                    chairLeft //left join второй вариант
                                from chair in chairLeft.DefaultIfEmpty()
                                from facl in context.T_CATALOGS.Where(r => l.F_CFAC == r.F_NREC).DefaultIfEmpty()
                                from d in context.T_U_DISCIPLINE.Where(r => l.F_CDIS == r.F_NREC).DefaultIfEmpty()
                                from exam in context.T_PERSONS.Where(r => m.F_CPEREXAM == r.F_NREC).DefaultIfEmpty()
                                from lec in context.T_PERSONS.Where(r => l.F_CEXAMINER == r.F_NREC).DefaultIfEmpty()
                                from typework in context.T_U_TYPEWORK.Where(r => l.F_CTYPEWORK == r.F_NREC).DefaultIfEmpty()
                                where (m.F_CPERSONS == studNrec
                                       && (l.F_WSTATUS == (int)ListStatus.OPEN || l.F_WSTATUS == (int)ListStatus.CLOSE)
                                       && (s.F_WSTATUSGR == 0)
                                       && (context.T_U_CURR_DIS_STUDTRANS.Any(r => r.F_CLIST == l.F_NREC) || l.F_CPARENT != DataOperation.Instance.GetNrecNull)
                                       )
                                orderby l.F_DATEDOC descending

                                select new JsonExtraListForStudent
                                {
                                    NumDoc = l.F_NUMDOC ?? string.Empty,
                                    Year = l.F_WYEARED,
                                    TypeListString = typework.F_NAME,
                                    Semester = l.F_WSEMESTR,
                                    StudGroup = s.F_NAME ?? string.Empty,
                                    ListChair = chair.F_LONGNAME ?? string.Empty,
                                    ListFacult = facl.F_LONGNAME ?? string.Empty,
                                    Discipline = d.F_NAME ?? string.Empty,
                                    ExaminerFio = exam.F_FIO ?? string.Empty,
                                    LecturerFio = lec.F_FIO ?? string.Empty,
                                    DisciplineAbbr = d.F_ABBR,
                                    DateList = l.F_DATEDOC,
                                    PersonNrec = m.F_CPERSONS,
                                    Status = l.F_WSTATUS,
                                };

                    foreach (var one in query.ToList())
                    {
                        one.PersonNrecString = DataOperation.Instance.ByteToString(one.PersonNrec);

                        result.Add(one);

                    }
                    context.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска в базе направлений, привязанных к студенту. Ошибка {e}");
            }

            return result;
        }


        /// <summary>
        /// Данный метод получает прикза для согласования
        /// </summary>
        /// <param name="rpd"></param>
        /// <returns></returns>
        private List<string> _getActiveOrderFromDb(int rpd)
        {
            var result = new List<string>();
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    ///TODO: убрать времняку после теста
                    //var temp = DataOperation.Instance.StringHexToByteArray("0x800100000001260d");
                    Logger.Log.Debug($"Текущее рпд {rpd}");
                    var query = context.T_TITLEDOC.Where(r => r.F_WTITL == rpd && r.F_WSTATUS == 10).Select(r => r.F_NREC).DefaultIfEmpty().ToList();

                    if (query.Any())
                    {
                        foreach (var one in query)
                        {
                            if (one != null)
                            {
                                result.Add(DataOperation.Instance.ByteToString(one));
                            }

                        }
                    }

                    context.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"При поиске активных приказов произошла ошибка. Ошибка {e}");
            }

            return result;

        }

        /// <summary>
        /// Данный метод возвращает описание приказа
        /// </summary>
        /// <param name="orderNrec"></param>
        /// <returns></returns>
        private GalOrder _getGalOrderDescriptionFromDb(string orderNrec)
        {
            var oneOrderList = new GalOrder();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "dbo.toInt64(t.F$NREC) as nrecInt64, " +
                               "CAST(dbo.frmAtlDateGer(t.F$ATL_LASTDATE) as VARCHAR) + \' \' + CAST(dbo.frAtlTime(t.F$ATL_LASTTIME) as VARCHAR) as LastDateEditTitle, " +
                               "CAST(dbo.frmAtlDateGer(p.F$ATL_LASTDATE) as VARCHAR) + \' \' + CAST(dbo.frAtlTime(p.F$ATL_LASTTIME) as VARCHAR) as LastDateEditPart, " +
                               "CAST(dbo.frmAtlDateGer(c.F$ATL_LASTDATE) as VARCHAR) + \' \' + CAST(dbo.frAtlTime(c.F$ATL_LASTTIME) as VARCHAR) as LastDateEditCont, " +
                               "t.F$NREC as nrec, " +
                               "t.F$DOCNMB as DocNmb, " +
                               "dbo.frmAtlDateGer(t.F$DOCDATE) as DocDate, " +
                               "t.F$DOCYEAR as DocYear, " +
                               "t.F$DOCTEXT as DocText, " +
                               "CONCAT(rpd.F$CODOPER, '.', rpd.F$WTDOP) as Rpd, " +
                               "rpd.F$NOPER as RpdName, " +
                               "CASE WHEN rpd.F$CODOPER=31074 THEN \'\'" +
                               "ELSE xu.XU$FULLNAME END as AuthorDoc, " +
                               "fac.F$NAME as FacultyOrder, " +
                               "fac.F$LONGNAME as FacultyOrderAbbr " +
                               "FROM dbo.T$TITLEDOC t " +
                               "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                               "LEFT JOIN dbo.T$CONTDOC c ON c.F$NREC = (SELECT TOP 1 c_sub.F$NREC FROM dbo.T$CONTDOC c_sub WHERE c_sub.F$CPART = p.F$NREC ORDER BY c_sub.F$ATL_LASTDATE DESC, c_sub.F$ATL_LASTTIME DESC) " +
                               "LEFT JOIN dbo.T$U_TYPEPR rpd ON (rpd.F$CODOPER = t.F$WTITL AND rpd.F$WTDOP = p.F$WATTR1) " +
                               "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = t.F$DOCMEMO " +
                               "LEFT JOIN X$USERS xu ON xu.ATL_NREC = t.F$ATL_OWNER " +
                               $"WHERE t.F$NREC = {orderNrec}";

                var reader = galcontext.ExecuteQuery(queryGal);
                if (reader.Read())
                {
                    oneOrderList.Nrec = !reader.IsDBNull(reader.GetOrdinal("nrec"))
                        ? reader.GetSqlBinary(reader.GetOrdinal("nrec")).Value
                        : DataOperation.Instance.GetNrecNull;
                    oneOrderList.NrecInt64 = !reader.IsDBNull(reader.GetOrdinal("nrecInt64"))
                        ? reader.GetInt64(reader.GetOrdinal("nrecInt64"))
                        : 0;
                    oneOrderList.NrecString = DataOperation.Instance.ByteToString(oneOrderList.Nrec);
                    oneOrderList.AuthorDoc = !reader.IsDBNull(reader.GetOrdinal("AuthorDoc"))
                        ? reader.GetString(reader.GetOrdinal("AuthorDoc"))
                        : string.Empty;
                    oneOrderList.DocDate = !reader.IsDBNull(reader.GetOrdinal("DocDate"))
                        ? reader.GetString(reader.GetOrdinal("DocDate"))
                        : string.Empty;
                    oneOrderList.DocNmb = !reader.IsDBNull(reader.GetOrdinal("DocNmb"))
                        ? reader.GetString(reader.GetOrdinal("DocNmb"))
                        : string.Empty;
                    oneOrderList.DocText = !reader.IsDBNull(reader.GetOrdinal("DocText"))
                        ? reader.GetString(reader.GetOrdinal("DocText"))
                        : string.Empty;
                    oneOrderList.Rpd = !reader.IsDBNull(reader.GetOrdinal("Rpd"))
                        ? reader.GetString(reader.GetOrdinal("Rpd"))
                        : string.Empty;
                    oneOrderList.RpdName = !reader.IsDBNull(reader.GetOrdinal("RpdName"))
                        ? reader.GetString(reader.GetOrdinal("RpdName"))
                        : string.Empty;
                    oneOrderList.LastDateEditTitle = !reader.IsDBNull(reader.GetOrdinal("LastDateEditTitle"))
                        ? reader.GetString(reader.GetOrdinal("LastDateEditTitle"))
                        : string.Empty;
                    oneOrderList.LastDateEditPart = !reader.IsDBNull(reader.GetOrdinal("LastDateEditPart"))
                        ? reader.GetString(reader.GetOrdinal("LastDateEditPart"))
                        : string.Empty;
                    oneOrderList.LastDateEditCont = !reader.IsDBNull(reader.GetOrdinal("LastDateEditCont"))
                        ? reader.GetString(reader.GetOrdinal("LastDateEditCont"))
                        : string.Empty;
                    oneOrderList.DocYear = !reader.IsDBNull(reader.GetOrdinal("DocYear"))
                        ? reader.GetInt32(reader.GetOrdinal("DocYear")).ToString()
                        : string.Empty;
                    oneOrderList.FacultyOrder = !reader.IsDBNull(reader.GetOrdinal("FacultyOrder"))
                        ? reader.GetString(reader.GetOrdinal("FacultyOrder"))
                        : string.Empty;
                    oneOrderList.FacultyOrderAbbr = !reader.IsDBNull(reader.GetOrdinal("FacultyOrderAbbr"))
                        ? reader.GetString(reader.GetOrdinal("FacultyOrderAbbr"))
                        : string.Empty;

                    var cultureInfo = new CultureInfo("ru-RU");

                    try
                    {
                        var tempDate1 = DateTime.Parse(oneOrderList.LastDateEditTitle, cultureInfo);
                        var tempDate2 = DateTime.Parse(oneOrderList.LastDateEditPart, cultureInfo);
                        var tempDate3 = DateTime.Parse(oneOrderList.LastDateEditCont, cultureInfo);

                        var tempListDate = new List<DateTime>()
                        {
                            tempDate1, tempDate2, tempDate3
                        };

                        oneOrderList.LastDateEdit = tempListDate.Max().ToString("dd.MM.yyyy HH:mm:ss.fffffff");
                    }
                    catch (Exception e)
                    {
                        oneOrderList.LastDateEdit = oneOrderList.LastDateEditTitle;
                    }



                    try
                    {
                        var rpd = Int32.Parse(oneOrderList.Rpd.Split('.')[0]);
                        oneOrderList.TypeSed = ((TypeSedEnum)rpd).GetDescription();
                        Logger.Log.Debug($"30006: {oneOrderList.Rpd}");
                        switch (rpd)
                        {
                            case 30004:
                                switch (oneOrderList.Rpd)
                                {
                                    case "30004.2":
                                        oneOrderList.TypeSed = "000000019";
                                        break;
                                    default:
                                        oneOrderList.TypeSed = "000000018";
                                        break;
                                }
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder2.GetDescription()
                                    : FolderSedEnum.Folder1.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                 {
                                    oneOrderList.ViewSed = ViewSedEnum.View3.GetDescription();
                                 }

                                 if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                 {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder6.GetDescription();
                                 }
                                break;
                            case 30005:
                                switch (oneOrderList.Rpd)
                                {
                                    case "30005.0":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD300050.GetDescription();
                                        break;
                                    case "30005.1":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD300051.GetDescription();
                                        break;
                                    case "30005.2":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD300052.GetDescription();
                                        break;
                                    case "30005.3":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD300053.GetDescription();
                                        break;
                                    default:
                                        oneOrderList.TypeSed = TypeSedEnum.RPD30005.GetDescription();
                                        break;
                                }

                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder2.GetDescription()
                                    : FolderSedEnum.Folder1.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View3.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder6.GetDescription();
                                }

                                break;
                            case 30006:
                                switch (oneOrderList.Rpd)
                                {
                                    case "30006.1":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD30006_1.GetDescription();
                                        break;
                                    case "30006.2":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD30006_2.GetDescription();
                                        break;
                                    default:
                                        oneOrderList.TypeSed = TypeSedEnum.RPD30006.GetDescription();
                                        break;
                                }
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder2.GetDescription()
                                    : FolderSedEnum.Folder1.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View3.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder6.GetDescription();
                                }

                                break;
                            case 30011:
                                oneOrderList.FolderSed = FolderSedEnum.Folder3.GetDescription();
                                oneOrderList.ViewSed = ViewSedEnum.View4.GetDescription();
                                break;
                            case 31074:
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder5.GetDescription()
                                    : FolderSedEnum.Folder4.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View7.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder7.GetDescription();
                                }

                                break;
                            case 30082:
                                switch (oneOrderList.Rpd)
                                {
                                    case "30082.1":
                                        oneOrderList.TypeSed = TypeSedEnum.RPD30082_1.GetDescription();
                                        break;
                                    default:
                                        oneOrderList.TypeSed = TypeSedEnum.RPD30082.GetDescription();
                                        break;
                                }
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder2.GetDescription()
                                    : FolderSedEnum.Folder1.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View3.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder6.GetDescription();
                                }
                                break;
                            case 31075:
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder5.GetDescription()
                                    : FolderSedEnum.Folder4.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View7.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder7.GetDescription();
                                }

                                break;
                            case 31076:
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder5.GetDescription()
                                    : FolderSedEnum.Folder4.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View7.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder7.GetDescription();
                                }

                                break;
                            default:
                                oneOrderList.FolderSed = string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ")
                                    ? FolderSedEnum.Folder2.GetDescription()
                                    : FolderSedEnum.Folder1.GetDescription();

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "Колледж ОмГТУ"))
                                {
                                    oneOrderList.ViewSed = ViewSedEnum.View3.GetDescription();
                                }

                                if (string.Equals(oneOrderList.FacultyOrderAbbr, "ОАиД"))
                                {
                                    oneOrderList.FolderSed = FolderSedEnum.Folder6.GetDescription();
                                }
                                break;
                        }


                    }
                    catch (Exception e)
                    {
                        Logger.Log.Error($"Error: {e}");
                        oneOrderList.TypeSed = string.Empty;
                    }


                }
            }

            return oneOrderList;
        }

        /// <summary>
        /// Данный метод получает подписантов по приказу
        /// </summary>
        /// <param name="rpd"></param>
        /// <returns></returns>
        private List<GalOrderSignature> _getGalOrderSignatureFromDb(string oneOrder)
        {
            var signature = new List<GalOrderSignature>();
            try
            {
                using (var galcontext = new GalDbContext())
                {
                    var queryGal =
                        "SELECT pfp.F$SCONST as Post, " +
                        "pfp.F$PRIORITET as Prioritet, " +
                        "pfp.F$FIO as Fio " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$U_POSTPERSFORPRINT pfp ON p.F$NREC = pfp.F$CPRIKAZ " +
                        "LEFT JOIN dbo.T$U_TYPEPR rpd ON (rpd.F$CODOPER = t.F$WTITL AND rpd.F$WTDOP = p.F$WATTR1) " +
                        $"WHERE t.F$NREC = {oneOrder} " +
                        "ORDER BY pfp.F$PRIORITET ASC";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var oneSignature = new GalOrderSignature
                            {
                                Fio = !reader.IsDBNull(reader.GetOrdinal("Fio")) ? reader.GetString(reader.GetOrdinal("Fio")) : string.Empty,
                                Post = !reader.IsDBNull(reader.GetOrdinal("Post")) ? reader.GetString(reader.GetOrdinal("Post")) : string.Empty,
                                Prioritet = !reader.IsDBNull(reader.GetOrdinal("Prioritet")) ? reader.GetInt32(reader.GetOrdinal("Prioritet")).ToString() : string.Empty,
                            };
                            signature.Add(oneSignature);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"При поиске подписантов произошла ошибка. Ошибка {e}");
            }

            return signature;

        }

        /// <summary>
        /// Данный метод возвращает правильное склонение
        /// </summary>
        /// <param name="nrec"></param>
        /// <returns></returns>
        private string _getFioCaseChanging(string nrec, string fncase, int code)
        {
            string result = string.Empty;
            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "CASE WHEN tpa.F$NREC IS NOT NULL THEN tpa.F$FIOKOGO " +
                               $"     WHEN tpa.F$NREC IS NULL AND dbo.fnCaseChanging(p.F$FIO, p.F$SEX, \'{fncase}\') != \'\' THEN dbo.fnCaseChanging(p.F$FIO, p.F$SEX, \'{fncase}\') END as FioStudentCaseChanging " +
                               "FROM T$PERSONS p " +
                               $"LEFT JOIN T$ABOUTFIO tpa ON tpa.F$CPSN = p.F$NREC AND tpa.F$WATTR = {code} " +
                               $"WHERE p.F$NREC = {nrec}";

                var reader = galcontext.ExecuteScalarQuery(queryGal);
                result = reader.ToString();

            }

            return result;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30008
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30008FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30008);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30008>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                  "person.F$NREC as PersonNrec, " +
                                  "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "qual.F$NAME as Qual, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "reason.F$NAME as DisReason, " +
                        "docReason.F$NAME as DocumentReason, " +
                        "dbo.frmAtlDateGer(c.F$DPRIK) as DateEnd, " +
                        "school.F$SNAME as SchoolName, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as dogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as dogovorDate  " +
                        ", CASE WHEN person.F$APPOINTLAST = 0x8000000000000000 THEN person.F$APPOINTCUR ELSE person.F$APPOINTLAST END as AppNrec " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON(pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON(xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_SCHOOL school on school.F$NREC = c.F$CNEW1 " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$CATALOGS qual ON qual.F$NREC = a.F$CREF1 " +
                        "LEFT JOIN dbo.T$CATALOGS reason ON reason.F$NREC = c.F$CCAT2 " +
                        "LEFT JOIN dbo.T$CATALOGS docReason ON docReason.F$NREC = c.F$CNEW3 " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30008()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            Qual = !reader.IsDBNull(reader.GetOrdinal("Qual"))
                                ? reader.GetString(reader.GetOrdinal("Qual"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DisReason = !reader.IsDBNull(reader.GetOrdinal("DisReason"))
                                ? reader.GetString(reader.GetOrdinal("DisReason"))
                                : string.Empty,
                            DocumentReason = !reader.IsDBNull(reader.GetOrdinal("DocumentReason"))
                                ? reader.GetString(reader.GetOrdinal("DocumentReason"))
                                : string.Empty,
                            DateEnd = !reader.IsDBNull(reader.GetOrdinal("DateEnd"))
                                ? reader.GetString(reader.GetOrdinal("DateEnd"))
                                : string.Empty,
                            SchoolName = !reader.IsDBNull(reader.GetOrdinal("SchoolName"))
                                ? reader.GetString(reader.GetOrdinal("SchoolName"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorDate = !reader.IsDBNull(reader.GetOrdinal("DogovorDate"))
                                ? reader.GetString(reader.GetOrdinal("DogovorDate"))
                                : string.Empty,
                            Link = String.Format("http://up.omgtu/index.php?r=site/GetStudCard&id={0}&app={1}",
                                                    DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                                                    DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("AppNrec")).Value)),
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;

        }

        /// <summary>
        /// Данный метод возвращает перечень дисциплин из справочника галактики
        /// </summary>
        /// <returns></returns>
        public List<ListDiscipline> GetDisciplinesFromDb()
        {
            var result = new List<ListDiscipline>();

            try
            {
                using (var galcontext = new GalDbContext())
                {
                    var queryGal =
                         "SELECT F$NREC as nrec, " +
                         "F$NAME as Discipline, " +
                         "F$ABBR as DisciplineAbbr, " +
                         "dbo.toInt64(F$NREC) as NrecInt64 " +
                         "FROM dbo.T$U_DISCIPLINE " +
                         "ORDER BY F$NREC ASC";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    if (reader.Read())
                    {
                        while (reader.Read())
                        {
                            var one = new ListDiscipline
                            {
                                Nrec = reader.GetSqlBinary(reader.GetOrdinal("nrec")).Value,
                                DisciplineAbbr = reader.GetString(reader.GetOrdinal("DisciplineAbbr")),
                                Discipline = reader.GetString(reader.GetOrdinal("Discipline")),
                                NrecInt64 = reader.GetInt64(reader.GetOrdinal("NrecInt64")),
                                NrecString =
                                    DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("nrec"))
                                        .Value),
                            };
                            result.Add(one);
                        }
                    }

                    galcontext.Dispose();
                    reader.Dispose();
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для поиска каталога оценок. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30042
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30042FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30042);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30042>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroupAfter, " +
                        "st.F$COURSE as StudentCourseAfter, " +
                        "spkau.F$NAME as FinSourceAfter, " +
                        "a.F$WPRIZN as FormEduAfter, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as SpecAfter, " +
                        "fac.F$LONGNAME as FacultAfter, " +
                        "dbo.frmAtlDateGer(a.F$APPOINTDATE) as DateEnd, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as DogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom,  " +
                        "dbo.frmAtlDateGer(coalesce(dog.F$DEND, dog2.F$DFLD#2#))  as DogovorEnd  " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "INNER JOIN dbo.T$APPOINTMENTS a ON a.F$CCONT = c.F$NREC AND a.F$LPRIZN = 42 " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);

                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30042()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroupAfter = !reader.IsDBNull(reader.GetOrdinal("StudentGroupAfter"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroupAfter"))
                                : string.Empty,
                            StudentCourseAfter = !reader.IsDBNull(reader.GetOrdinal("StudentCourseAfter"))
                                ? reader.GetInt32(reader.GetOrdinal("StudentCourseAfter")).ToString()
                                : string.Empty,
                            FinSourceAfter = !reader.IsDBNull(reader.GetOrdinal("FinSourceAfter"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceAfter"))
                                : string.Empty,
                            FormEduAfter = !reader.IsDBNull(reader.GetOrdinal("FormEduAfter"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEduAfter"))).GetDescription()
                                : string.Empty,
                            SpecAfter = !reader.IsDBNull(reader.GetOrdinal("SpecAfter"))
                                ? reader.GetString(reader.GetOrdinal("SpecAfter"))
                                : string.Empty,
                            FacultAfter = !reader.IsDBNull(reader.GetOrdinal("FacultAfter"))
                                ? reader.GetString(reader.GetOrdinal("FacultAfter"))
                                : string.Empty,
                            DateEnd = !reader.IsDBNull(reader.GetOrdinal("DateEnd"))
                                ? reader.GetString(reader.GetOrdinal("DateEnd"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            DogovorEnd = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd"))
                                ? reader.GetString(reader.GetOrdinal("DogovorEnd"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }

                    foreach (var oneStudentSeacrh in students)
                    {
                        queryGal = "SELECT DISTINCT TOP 1 COALESCE(t2.F$DOCDATE, t1.F$DOCDATE), " +
                                "CASE " +
                                "WHEN tcv.F$KOTPUS NOT IN(50, 51, 52) and tcv.F$NOTPUS LIKE \'академический отпуск %\' THEN REPLACE(tcv.F$NOTPUS, \'академический отпуск \', \'\') " +
                                "WHEN tcv.F$KOTPUS NOT IN(50, 51, 52) AND tcv.F$NOTPUS LIKE \'отпуск %\' THEN REPLACE(tcv.F$NOTPUS, \'отпуск \', \'\') " +
                                "ELSE \'\'  END as ParentOtpuskResom, " +
                                "CASE WHEN v1.F$NREC IS NOT NULL THEN dbo.frmAtlDateGer(t2.F$DOCDATE) " +
                                "ELSE dbo.frmAtlDateGer(t1.F$DOCDATE) END as ParentDateDok, " +
                                "CASE WHEN v1.F$NREC IS NOT NULL THEN REPLACE(t2.F$DOCNMB, \' \', \'\') " +
                                "ELSE REPLACE(t1.F$DOCNMB, \' \', \'\') END as ParentNumDok, " +
                                "CASE " +
                                "WHEN pr2.F$WTDOP IN(2, 3) THEN  \'отпуска по беременности и родам\' " +
                                "WHEN pr2.F$WTDOP IN(4, 5)  THEN \'отпуска по уходу за ребёнком до 1,5 лет\' " +
                                "WHEN pr2.F$WTDOP IN(6, 7)  THEN \'отпуска по уходу за ребёнком до 3-х лет\' " +
                                "ELSE \'академического отпуска\'  END as ParentOtpusk " +
                                "FROM T$VACATIONS v " +
                                $"INNER JOIN T$PERSONS person ON person.F$NREC = v.F$PERSON AND person.F$NREC = {DataOperation.Instance.ByteToString(oneStudentSeacrh.PersonNrec)} " +
                                "LEFT JOIN T$APPOINTMENTS a ON a.F$NREC = v.F$APPOINT " +
                                "LEFT JOIN T$KLOTPUSK tcv ON tcv.F$NREC = v.F$VACTYPE " +
                                "INNER JOIN T$CONTDOC c ON c.F$NREC = v.F$CPRIKAZ AND c.F$TYPEOPER in (30043, 30041) " +
                                "LEFT JOIN T$PARTDOC p ON p.F$NREC = c.F$CPART " +
                                "LEFT JOIN T$TITLEDOC t1 ON p.F$CDOC = t1.F$NREC " +
                                "LEFT JOIN T$U_TYPEPR pr2 ON p.F$TYPEOPER = pr2.F$CODOPER AND p.F$WATTR1 = pr2.F$WTDOP " +
                                "LEFT JOIN T$CATaLOGS tc ON tc.F$NREC = c.F$CCAT2 " +
                                "LEFT JOIN T$VACATIONS v1 ON v1.F$CADDREF = v.F$NREC " +
                                "LEFT JOIN T$CONTDOC c1 ON v1.F$CPRIKAZ = c1.F$NREC AND c1.F$TYPEOPER = 30043 " +
                                "LEFT JOIN T$CONTDOC c2 ON c2.F$NREC = c1.F$CDOPREF " +
                                "LEFT JOIN T$PARTDOC p1 ON p1.F$NREC = (CASE WHEN c1.F$CPART != 0x8000000000000000 THEN c1.F$CPART ELSE c2.F$CPART END) " +
                                "LEFT JOIN T$TITLEDOC t2 ON p1.F$CDOC = t2.F$NREC " +
                                "WHERE " +
                                $"tcv.F$NREC != 0x8001000000000009  AND pr2.F$CODOPER != 0  AND dbo.toAtlDate(\'{oneOrderList.DocDate}\') > t1.F$DOCDATE " +
                                "ORDER BY COALESCE(t2.F$DOCDATE, t1.F$DOCDATE) DESC ";

                        reader = galcontext.ExecuteQuery(queryGal);
                        while (reader.Read())
                        {
                            oneStudentSeacrh.ParentOtpuskResom = !reader.IsDBNull(reader.GetOrdinal("ParentOtpuskResom"))
                                ? reader.GetString(reader.GetOrdinal("ParentOtpuskResom"))
                                : string.Empty;
                            oneStudentSeacrh.ParentDateDok = !reader.IsDBNull(reader.GetOrdinal("ParentDateDok"))
                                ? reader.GetString(reader.GetOrdinal("ParentDateDok"))
                                : string.Empty;
                            oneStudentSeacrh.ParentNumDok = !reader.IsDBNull(reader.GetOrdinal("ParentNumDok"))
                                ? reader.GetString(reader.GetOrdinal("ParentNumDok"))
                                : string.Empty;
                            oneStudentSeacrh.ParentOtpusk = !reader.IsDBNull(reader.GetOrdinal("ParentOtpusk"))
                                ? reader.GetString(reader.GetOrdinal("ParentOtpusk"))
                                : string.Empty;

                            oneOrderList.TypeSed = string.Equals(oneStudentSeacrh.ParentOtpusk, "академического отпуска") ? TypeSedEnum.RPD30042_1.GetDescription() : TypeSedEnum.RPD30042.GetDescription();
                        }
                    }

                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30041
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30041FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30041);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30041>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as dogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom, " +
                        "CASE WHEN pr2.F$WTDOP IN (1, 3, 5, 7) THEN \'считать пролонгированным в части срока обучения до\' ELSE \'\' END AS Prolong, " +
                        "tctv.F$NOTPUS AS NOtpus, " +
                        "dbo.frmAtlDateGer(tv.F$PLANYEARBEG) AS OtpuskFrom, " +
                        "dbo.frmAtlDateGer(tv.F$PLANYEAREND) AS OtpuskEnd, " +
                        "CASE WHEN pr2.F$WTDOP IN (2, 4, 6) THEN \'\' " +
                        "WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(dog.F$DEND) " +
                        "ELSE dbo.frmAtlDateGer(dog2.F$DFLD#2#) END AS DogovorEnd " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN T$VACATIONS tv ON c.F$NREC = tv.F$CPRIKAZ " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN T$KLOTPUSK tctv ON tctv.F$NREC = tv.F$VACTYPE " +
                        "LEFT JOIN T$U_TYPEPR pr2 ON p.F$TYPEOPER = pr2.F$CODOPER AND p.F$WATTR1 = pr2.F$WTDOP " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30041()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            Prolong = !reader.IsDBNull(reader.GetOrdinal("Prolong"))
                                ? reader.GetString(reader.GetOrdinal("Prolong"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            NOtpus = !reader.IsDBNull(reader.GetOrdinal("NOtpus"))
                                ? reader.GetString(reader.GetOrdinal("NOtpus"))
                                : string.Empty,
                            OtpuskFrom = !reader.IsDBNull(reader.GetOrdinal("OtpuskFrom"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskFrom"))
                                : string.Empty,
                            OtpuskEnd = !reader.IsDBNull(reader.GetOrdinal("OtpuskEnd"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskEnd"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            DogovorEnd = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd"))
                                ? reader.GetString(reader.GetOrdinal("DogovorEnd"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30043
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30043FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30043);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30043>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = " SELECT " +
                        " tp.F$NREC as PersonNrec, " +
                        " dbo.toInt64(tp.F$NREC) as PersonNrecStringInt64, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "st.F$NAME as StudentGroup, " +
                        " tp.F$FIO AS FioStudent, " +
                        " gr.F$CODE AS GrCode, " +
                        " gr.F$NAME AS Gr, " +
                        " tp.F$SEX  as Sex, " +
                        " fak.F$NAME AS Facult, " +
                        " fak.F$LONGNAME AS FacultAbbr, " +
                        " CONVERT(VARCHAR, ta.F$VACATION) as StudentCourse, " +
                        " spec.F$CODE + \' \' + spec.F$NAME as Spec, " +
                        " ta.F$WPRIZN as FormEdu, " +
                        " ifin.F$CODE AS FinSourceAbbr, " +
                        "tp.F$STRTABN as Strtabn, " +
                        " ifin.F$NAME AS FinSource, " +
                        " CASE WHEN (SELECT TOP 1 " +
                        " tv1.F$VACTYPE " +
                        " FROM T$VACATIONS tv1  " +
                        " LEFT JOIN T$CONTDOC cd1  ON cd1.F$NREC = tv1.F$CPRIKAZ " +
                        " LEFT JOIN T$PARTDOC tp1   ON cd1.F$CPART = tp1.F$NREC " +
                        " LEFT JOIN T$TITLEDOC tt   ON tt.F$NREC = tp1.F$CDOC " +
                        " WHERE tv1.F$APPOINT = ta.F$NREC " +
                        " AND tt.F$WSTATUS = 1 " +
                        "  ORDER BY tt.F$DOCDATE DESC) = tv.F$VACTYPE THEN tctv.F$NOTPUS " +
                        "    ELSE \'ОШИБКА! ВИД ОТПУСКА НЕ СООТВЕТСВУЕТ ПРОДЛЕВАЕМОМУ!\'  " +
                        " END AS OtpuskName, " +
                        " dbo.frmAtlDateGer(tv.F$PLANYEAREND) AS OtpuskDateEnd,  " +
                        " (SELECT TOP 1 " +
                        " tt.F$DOCNMB + \' от \' + dbo.frmAtlDateGer(tt.F$DOCDATE)  " +
                        " FROM T$VACATIONS tv1  " +
                        " LEFT JOIN T$CONTDOC cd1  ON cd1.F$NREC = tv1.F$CPRIKAZ  " +
                        " LEFT JOIN T$PARTDOC tp1   ON cd1.F$CPART = tp1.F$NREC " +
                        " LEFT JOIN T$TITLEDOC tt   ON tt.F$NREC = tp1.F$CDOC " +
                        " WHERE tv1.F$APPOINT = ta.F$NREC " +
                        "  AND tt.F$WSTATUS = 1 " +
                        "  ORDER BY tt.F$DOCDATE DESC " +
                        " ) AS OtpuskOLD, " +
                        " CASE " +
                        "    WHEN td.F$NREC IS NOT NULL THEN td.F$NODOC " +
                        "    WHEN (SELECT TOP 1 td2.F$SFLD#1# FROM T$DOPINFO td2  " +
                        "                        WHERE td2.F$CPERSON = tp.F$NREC AND td2.F$CDOPTBL = 0x8001000000000007 AND td2.F$FFLDSUM#1# = 1  " +
                        "                        ORDER BY td2.F$DFLD#1# DESC) IS NOT NULL THEN (SELECT TOP 1 td2.F$SFLD#1# FROM T$DOPINFO td2 " +
                        "                                                                          WHERE td2.F$CPERSON = tp.F$NREC AND td2.F$CDOPTBL = 0x8001000000000007 AND td2.F$FFLDSUM#1# = 1  " +
                        "                                                                          ORDER BY td2.F$DFLD#1# DESC) " +
                        "  ELSE ta.F$CONTRACTNMB END AS DogovorNum , " +
                        " CASE " +
                        "    WHEN td.F$NREC IS NOT NULL THEN  dbo.frmAtlDateGer(td.F$DDOC) " +
                        "    WHEN (SELECT TOP 1 dbo.frmAtlDateGer(td4.F$DFLD#1#) " +
                        "                              FROM T$DOPINFO td4 " +
                        "                              WHERE td4.F$CPERSON = tp.F$NREC AND td4.F$CDOPTBL = 0x8001000000000007 AND td4.F$FFLDSUM#1# = 1 " +
                        "                              ORDER BY td4.F$DFLD#1# DESC) IS NOT NULL THEN (SELECT TOP 1 dbo.frmAtlDateGer(td4.F$DFLD#1#) " +
                        "                                                                              FROM T$DOPINFO td4 " +
                        "                                                                              WHERE td4.F$CPERSON = tp.F$NREC AND td4.F$CDOPTBL = 0x8001000000000007 AND td4.F$FFLDSUM#1# = 1  " +
                        "                                                                              ORDER BY td4.F$DFLD#1# DESC) " +
                        "  ELSE dbo.frmAtlDateGer(ta.F$CONTRACTDATE) " +
                        "  END DogovorDate, " +
                        " CASE WHEN ttd.F$WDOP = 1 THEN  dbo.frmAtlDateGer(td.F$DEND) " +
                        "  ELSE \'\' END AS DogovorProlong " +
                        "FROM T$TITLEDOC ttd " +
                        "LEFT JOIN T$PARTDOC pd      ON pd.F$CDOC = ttd.F$NREC " +
                        "LEFT JOIN T$CONTDOC cd      ON cd.F$CPART = pd.F$NREC " +
                        "LEFT JOIN T$PERSONS tp      ON cd.F$PERSON = tp.F$NREC " +
                        "LEFT JOIN T$CASEPSN cp      ON cp.F$PSNV = tp.F$FIO " +
                        "LEFT JOIN T$APPOINTMENTS ta ON cd.F$CSTR = ta.F$NREC " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = ta.F$CCAT1 " +
                        "LEFT JOIN T$CATALOGS gr     ON tp.F$GR = gr.F$NREC " +
                        "LEFT JOIN T$VACATIONS tv    ON tv.F$CPRIKAZ = cd.F$NREC " +
                        "LEFT JOIN T$KLOTPUSK tctv   ON tctv.F$NREC = tv.F$VACTYPE " +
                        " LEFT JOIN T$U_STUDGROUP stgr        ON ta.F$CCAT1 = stgr.F$NREC  " +
                        "  LEFT JOIN T$STAFFSTRUCT stst        ON stst.F$NREC = ta.F$STAFFSTR    " +
                        "  LEFT JOIN T$CATALOGS pledu          ON pledu.F$NREC = stst.F$DEPARTMENT  " +
                        "  LEFT JOIN T$U_STUD_FINSOURCE tusf   ON tusf.F$NREC = ta.F$CREF2  " +
                        "  LEFT JOIN T$SPKAU ifin              ON ifin.F$NREC = tusf.F$CFINSOURCE  " +
                        "  LEFT JOIN T$U_CURRICULUM bup        ON bup.F$NREC = stst.F$CSTR  " +
                        "  LEFT JOIN T$CATALOGS spec           ON spec.F$NREC = ta.F$POST  " +
                        "  LEFT JOIN T$CATALOGS fak            ON fak.F$NREC =  ta.F$PRIVPENSION   " +
                        "  LEFT JOIN T$CATALOGS fakparent      ON fakparent.F$NREC = fak.F$CPARENT " +
                        "  LEFT JOIN T$CATALOGS osn            ON cd.F$CCAT2 = osn.F$NREC " +
                        "   LEFT JOIN T$DOGOVOR td              ON ta.F$CDOG = td.F$NREC  " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = cd.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                         $"WHERE ttd.F$NREC = {oneOrder}";



                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30043()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetString(reader.GetOrdinal("StudentCourse"))
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorDate = !reader.IsDBNull(reader.GetOrdinal("DogovorDate"))
                                ? reader.GetString(reader.GetOrdinal("DogovorDate"))
                                : string.Empty,
                            DogovorProlong = !reader.IsDBNull(reader.GetOrdinal("DogovorProlong"))
                                ? reader.GetString(reader.GetOrdinal("DogovorProlong"))
                                : string.Empty,
                            FacultAbbr = !reader.IsDBNull(reader.GetOrdinal("FacultAbbr"))
                                ? reader.GetString(reader.GetOrdinal("FacultAbbr"))
                                : string.Empty,
                            FinSourceAbbr = !reader.IsDBNull(reader.GetOrdinal("FinSourceAbbr"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceAbbr"))
                                : string.Empty,
                            OtpuskDateEnd = !reader.IsDBNull(reader.GetOrdinal("OtpuskDateEnd"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskDateEnd"))
                                : string.Empty,
                            OtpuskName = !reader.IsDBNull(reader.GetOrdinal("OtpuskName"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskName"))
                                : string.Empty,
                            OtpuskOLD = !reader.IsDBNull(reader.GetOrdinal("OtpuskOLD"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskOLD"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30044
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30044FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30044);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30044>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as dogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom, " +
                        "tctv.F$NOTPUS AS NOtpus, " +
                        "dbo.frmAtlDateGer(tv.F$PLANYEARBEG) AS OtpuskFrom, " +
                        "dbo.frmAtlDateGer(tv.F$PLANYEAREND) AS OtpuskEnd " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN T$VACATIONS tv ON c.F$NREC = tv.F$CPRIKAZ " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN T$KLOTPUSK tctv ON tctv.F$NREC = tv.F$VACTYPE " +
                        "LEFT JOIN T$U_TYPEPR pr2 ON p.F$TYPEOPER = pr2.F$CODOPER AND p.F$WATTR1 = pr2.F$WTDOP " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30044()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            NOtpus = !reader.IsDBNull(reader.GetOrdinal("NOtpus"))
                                ? reader.GetString(reader.GetOrdinal("NOtpus"))
                                : string.Empty,
                            OtpuskFrom = !reader.IsDBNull(reader.GetOrdinal("OtpuskFrom"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskFrom"))
                                : string.Empty,
                            OtpuskEnd = !reader.IsDBNull(reader.GetOrdinal("OtpuskEnd"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskEnd"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 31030
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd31030FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(31030);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents31030>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "COALESCE(dog2.F$SFLD#1#, dog.F$NODOC, a.F$CONTRACTNMB) as DogovorNum,  " +
                        "CASE WHEN dbo.frmAtlDateGer(dog2.F$DFLD#1#) IS NOT NULL then dbo.frmAtlDateGer(dog2.F$DFLD#1#) ELSE dbo.frmAtlDateGer(a.F$CONTRACTDATE) END as DogovorFrom, " +
                        "CASE WHEN dbo.frmAtlDateGer(dog2.F$DFLD#2#) IS NOT NULL then dbo.frmAtlDateGer(dog2.F$DFLD#2#) " +
                        "WHEN dog.F$NREC IS NOT NULL THEN dbo.frmAtlDateGer(dog.F$DEND) ELSE dbo.frmAtlDateGer(c.F$DAT2) END AS DogovorDateProlong, " +
                        "c.F$SBOTTOM AS TypeIGA, " +
                        "dbo.frmAtlDateGer(c.F$DAT1) AS ToDateIGA " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents31030()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorDateProlong = !reader.IsDBNull(reader.GetOrdinal("DogovorDateProlong"))
                                ? reader.GetString(reader.GetOrdinal("DogovorDateProlong"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            ToDateIGA = !reader.IsDBNull(reader.GetOrdinal("ToDateIGA"))
                                ? reader.GetString(reader.GetOrdinal("ToDateIGA"))
                                : string.Empty,
                            TypeIGA = !reader.IsDBNull(reader.GetOrdinal("TypeIGA"))
                                ? reader.GetString(reader.GetOrdinal("TypeIGA"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30052
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30052FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30052);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30052>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#, a.F$CONTRACTNMB) as DogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(dog.F$DDOC) " +
                        "WHEN dbo.frmAtlDateGer(dog2.F$DFLD#1#) IS NOT NULL THEN dbo.frmAtlDateGer(dog2.F$DFLD#1#) ELSE dbo.frmAtlDateGer(a.F$CONTRACTDATE) END as DogovorDate, " +
                        "CASE WHEN t.F$WTITL = 1 THEN \'Утвердить индивидуальный план ликвидации академической задолженности.\'  ELSE \'\' END AS IndPlan, " +
                        "dbo.frmAtlDateGer(a.F$APPOINTDATE) AS AppDate, " +
                        "school.F$SNAME AS SchoolFrom " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN T$EDUCATION te ON te.F$CCONTDOC = c.F$NREC " +
                        "LEFT JOIN T$U_SCHOOL school ON school.F$NREC = te.F$NAME " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30052()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorDate = !reader.IsDBNull(reader.GetOrdinal("DogovorDate"))
                                ? reader.GetString(reader.GetOrdinal("DogovorDate"))
                                : string.Empty,
                            AppDate = !reader.IsDBNull(reader.GetOrdinal("AppDate"))
                                ? reader.GetString(reader.GetOrdinal("AppDate"))
                                : string.Empty,
                            IndPlan = !reader.IsDBNull(reader.GetOrdinal("IndPlan"))
                                ? reader.GetString(reader.GetOrdinal("IndPlan"))
                                : string.Empty,
                            SchoolFrom = !reader.IsDBNull(reader.GetOrdinal("SchoolFrom"))
                                ? reader.GetString(reader.GetOrdinal("SchoolFrom"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30080
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30080FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30080);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30080>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(p.F$DDAT1) AS DateTravel, " +
                        "CONVERT(VARCHAR, (st.F$YEARENT - 1 + a.F$VACATION)) + \'-\' + CONVERT(VARCHAR, (st.F$YEARENT + a.F$VACATION)) AS YearEd, " +
                        "CASE WHEN c.F$WATTRDOC1 = 0 THEN \'успешно аттестованных\' ELSE \'имеющих академические задолженности\' END AS TextFirst, " +
                        "a.F$VACATION + 1 AS CoursNew, " +
                        "c.F$WATTRDOC1 AS Uslovno " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC cMain ON cMain.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CDOPREF = cMain.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN T$STAFFSTRUCT staff  ON staff.F$NREC = a.F$STAFFSTR " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = staff.F$DEPARTMENT " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30080()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            CoursNew = !reader.IsDBNull(reader.GetOrdinal("CoursNew"))
                                ? reader.GetInt32(reader.GetOrdinal("CoursNew")).ToString()
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DateTravel = !reader.IsDBNull(reader.GetOrdinal("DateTravel"))
                                ? reader.GetString(reader.GetOrdinal("DateTravel"))
                                : string.Empty,
                            TextFirst = !reader.IsDBNull(reader.GetOrdinal("TextFirst"))
                                ? reader.GetString(reader.GetOrdinal("TextFirst"))
                                : string.Empty,
                            Uslovno = !reader.IsDBNull(reader.GetOrdinal("Uslovno"))
                                ? reader.GetInt32(reader.GetOrdinal("Uslovno")).ToString()
                                : string.Empty,
                            YearEd = !reader.IsDBNull(reader.GetOrdinal("YearEd"))
                                ? reader.GetString(reader.GetOrdinal("YearEd"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30006
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30006FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30006);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30006>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251( coalesce(xx.M#DATA, xxMain.M#DATA)), 'TXT', '') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(p.F$DDAT1) AS DateTravel, " +
                        "grp1.F$NAME as GroupNameOld, " +
                        "grp2.F$NAME as GroupNameNew, " +
                        "CASE WHEN pr2.F$WTDOP = 1 THEN \'В связи с малочисленностью\' WHEN pr2.F$WTDOP = 2 THEN \'Нижеперечисленных\' END AS TextFIrst, " +
                        "CASE WHEN pr2.F$WTDOP = 1 THEN \'произвести перераспределение студентов\' WHEN pr2.F$WTDOP = 2 THEN \'перевести в другую группу\' END AS TextSecond, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as DogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom,  " +
                        "dbo.frmAtlDateGer(coalesce(dog.F$DEND, dog2.F$DFLD#2#))  as DogovorEnd  " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC cMain ON cMain.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CDOPREF = cMain.F$NREC " +
                        "LEFT JOIN T$U_TYPEPR pr2  ON p.F$TYPEOPER = pr2.F$CODOPER AND p.F$WATTR1 = pr2.F$WTDOP " +
                        "LEFT JOIN T$U_STUDGROUP grp1 ON grp1.F$NREC = cMain.F$CREF1 " +
                        "LEFT JOIN T$U_STUDGROUP grp2 ON grp2.F$NREC = c.F$CADDINF " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN dbo.T$PRMEMO prMain ON (prMain.F$CDOC = cMain.F$NREC AND prMain.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN XX$MEMO xxMain ON (xxMain.M#NREC = prMain.F$NREC AND xxMain.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN T$STAFFSTRUCT staff  ON staff.F$NREC = a.F$STAFFSTR " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = staff.F$DEPARTMENT " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder} and pr2.F$WTDOP != 0 ";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30006()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DateTravel = !reader.IsDBNull(reader.GetOrdinal("DateTravel"))
                                ? reader.GetString(reader.GetOrdinal("DateTravel"))
                                : string.Empty,
                            TextFirst = !reader.IsDBNull(reader.GetOrdinal("TextFirst"))
                                ? reader.GetString(reader.GetOrdinal("TextFirst"))
                                : string.Empty,
                            GroupNameNew = !reader.IsDBNull(reader.GetOrdinal("GroupNameNew"))
                                ? reader.GetString(reader.GetOrdinal("GroupNameNew"))
                                : string.Empty,
                            TextSecond = !reader.IsDBNull(reader.GetOrdinal("textSecond"))
                                ? reader.GetString(reader.GetOrdinal("textSecond"))
                                : string.Empty,
                            GroupNameOld = !reader.IsDBNull(reader.GetOrdinal("GroupNameOld"))
                            ? reader.GetString(reader.GetOrdinal("GroupNameOld"))
                            : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            DogovorEnd = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd"))
                                ? reader.GetString(reader.GetOrdinal("DogovorEnd"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                if (students.Any())
                {
                    listOrder.Add(oneOrderList);
                }

            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30081
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30081FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30081);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30081>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(p.F$LASTDATE) AS DisDate " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC cMain ON cMain.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CDOPREF = cMain.F$NREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = a.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = cMain.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30081()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            DisDate = !reader.IsDBNull(reader.GetOrdinal("DisDate"))
                                ? reader.GetString(reader.GetOrdinal("DisDate"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.BasisOfOrder = students.Where(r => !string.IsNullOrEmpty(r.BasisOfOrder)).Select(r => r.BasisOfOrder).FirstOrDefault();

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30011
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30011FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30011);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30011>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "passp.F$NMB AS DocumentNmb, " +
                        "dbo.frmAtlDateGer(passp.F$GIVENDATE) AS DocumentGivenDate, " +
                        "dbo.frmAtlDateGer(passp.F$TODATE) AS DocumentToDate, " +
                        "dbo.frmAtlDateGer(rais.F$FROMDATE) AS RaisFromDate, " +
                        "dbo.frmAtlDateGer(rais.F$TODATE) AS RaisToDate, " +
                        "CASE WHEN passp_name.F$CODE = 111 THEN \'Уведомление\' " +
                        "WHEN passp_name.F$CODE = 110 THEN \'Справка\' " +
                        "WHEN passp_name.F$CODE = 300 THEN \'Документ, подтверждающий льготу\' END AS DocName, " +
                        "CASE WHEN passp_name.F$CODE = 111 THEN CONCAT(\'выдано\', \' \', passp.F$GIVENBY) " +
                        "ELSE CONCAT(\'выдана\', \' \', passp.F$GIVENBY) END AS DocumentPodrazd " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$RAISE rais ON rais.F$CRDOP = c.F$NREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = rais.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$PASSPORTS passp on passp.F$NREC = (SELECT TOP 1 passp_sub.F$NREC FROM  dbo.T$PASSPORTS passp_sub " +
                                   "LEFT JOIN dbo.T$CATALOGS passp_sub_name ON  passp_sub.F$DOCNAME = passp_sub_name.F$NREC " +
                                   "WHERE passp_sub.F$GIVENDATE > 129106177 AND passp_sub.F$PERSON = person.F$NREC " +
                                       "and (CONVERT(VARCHAR, passp_sub_name.F$CODE)= '300' OR CONVERT(VARCHAR, passp_sub_name.F$CODE) = '110' OR CONVERT(VARCHAR, passp_sub_name.F$CODE) = '111') " +
                                   "ORDER BY passp_sub.F$GIVENDATE DESC) " +
                       "LEFT JOIN dbo.T$CATALOGS passp_name ON  passp.F$DOCNAME = passp_name.F$NREC " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30011()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DocName = !reader.IsDBNull(reader.GetOrdinal("DocName"))
                                ? reader.GetString(reader.GetOrdinal("DocName"))
                                : string.Empty,
                            DocumentGivenDate = !reader.IsDBNull(reader.GetOrdinal("DocumentGivenDate"))
                                ? reader.GetString(reader.GetOrdinal("DocumentGivenDate"))
                                : string.Empty,
                            DocumentNmb = !reader.IsDBNull(reader.GetOrdinal("DocumentNmb"))
                                ? reader.GetString(reader.GetOrdinal("DocumentNmb"))
                                : string.Empty,
                            DocumentPodrazd = !reader.IsDBNull(reader.GetOrdinal("DocumentPodrazd"))
                                ? reader.GetString(reader.GetOrdinal("DocumentPodrazd"))
                                : string.Empty,
                            DocumentToDate = !reader.IsDBNull(reader.GetOrdinal("DocumentToDate"))
                                ? reader.GetString(reader.GetOrdinal("DocumentToDate"))
                                : string.Empty,
                            RaisFromDate = !reader.IsDBNull(reader.GetOrdinal("RaisFromDate"))
                                ? reader.GetString(reader.GetOrdinal("RaisFromDate"))
                                : string.Empty,
                            RaisToDate = !reader.IsDBNull(reader.GetOrdinal("RaisToDate"))
                                ? reader.GetString(reader.GetOrdinal("RaisToDate"))
                                : string.Empty
                        };
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30082
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30082FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30082);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30082>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(p.F$LASTDATE) AS DisDate " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = a.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30082()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            DisDate = !reader.IsDBNull(reader.GetOrdinal("DisDate"))
                                ? reader.GetString(reader.GetOrdinal("DisDate"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30082
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30002FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30002);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30002>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(p.F$LASTDATE) AS DisDate, " +
                        "cp.F$PSNV AS FioOld, " +
                        "cp.F$PSND AS FioNew, " +
                        "dbo.frmAtlDateGer(cp.F$DCHANGE) as FioNewDateChange, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as DogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom,  " +
                        "dbo.frmAtlDateGer(coalesce(dog.F$DEND, dog2.F$DFLD#2#))  as DogovorEnd  " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN T$CASEPSN cp ON cp.F$NREC = c.F$OBJNREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30002()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            //FioStudentCaseChanging = !reader.IsDBNull(reader.GetOrdinal("FioStudentCaseChanging")) ?
                            //    reader.GetString(reader.GetOrdinal("FioStudentCaseChanging")) : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            FioNew = !reader.IsDBNull(reader.GetOrdinal("FioNew"))
                                ? reader.GetString(reader.GetOrdinal("FioNew"))
                                : string.Empty,
                            FioOld = !reader.IsDBNull(reader.GetOrdinal("FioOld"))
                                ? reader.GetString(reader.GetOrdinal("FioOld"))
                                : string.Empty,
                            FioNewDateChange = !reader.IsDBNull(reader.GetOrdinal("FioNewDateChange"))
                                ? reader.GetString(reader.GetOrdinal("FioNewDateChange"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            DogovorEnd = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd"))
                                ? reader.GetString(reader.GetOrdinal("DogovorEnd"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30004
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30004FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30004);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30004>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "person.F$STRTABN as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "(SELECT TOP 1 " +
                        "CASE WHEN fioOld.F$DCHANGE >= ttd1.F$DOCDATE AND fioOld.F$DCHANGE <= t.F$DOCDATE THEN ttd1.F$DOCNMB + \', прежнее ФИО \' + fioOld.F$PSNV " +
                        " + \', дата изм. ФИО - \' + dbo.frmAtlDateGer(fioOld.F$DCHANGE) " +
                        "ELSE ttd1.F$DOCNMB " +
                        "END " +
                        "FROM T$CONTDOC tc1 " +
                        "INNER JOIN T$PARTDOC tp1 ON tc1.F$CPART = tp1.F$NREC AND tp1.F$TYPEOPER = 30008 " +
                        "LEFT JOIN T$TITLEDOC ttd1 ON tp1.F$CDOC = ttd1.F$NREC " +
                        "LEFT JOIN T$CASEPSN fioOld ON fioOld.F$CPSN = person.F$NREC AND fioOld.F$DCHANGE <= t.F$DOCDATE AND fioOld.F$DCHANGE > ttd1.F$DOCDATE " +
                        "WHERE tc1.F$NREC = disDoc.F$NREC AND tc1.F$PERSON = person.F$NREC " +
                        "AND ttd1.F$DOCDATE < t.F$DOCDATE " +
                        "ORDER BY ttd1.F$DOCDATE DESC " +
                        ") AS DisDoc, " +
                        "dbo.frmAtlDateGer(disT.F$DOCDATE) AS DisDocDate, " +
                        "disOsn.F$NAME AS DisReason, " +
                        "CASE " +
                            "WHEN p.F$WATTR1 = 1 THEN \'восстановить для подготовки и защиты дипломного проекта\' " +
                            "WHEN p.F$WATTR1 = 2 THEN \'восстановить для подготовки и защиты дипломной работы\' " +
                            "WHEN p.F$WATTR1 = 3 THEN \'восстановить для подготовки и защиты бакалаврской работы\' " +
                            "ELSE \'восстановить\' " +
                        "END AS RecoveryReason, " +
                        "dbo.frmAtlDateGer(ah.F$DREC) AS RecoveryDate, " +
                        "CASE WHEN p.F$WATTR1 > 0 THEN \'\' " +
                            "ELSE \'Утвердить индивидуальный план ликвидации академической задолженности.\' " +
                        "END AS DopInfo, " +
                        "COALESCE(dog2.F$SFLD#1#, a.F$CONTRACTNMB) as DogovorNum,  " +
                        "CASE WHEN dog2.F$DFLD#1# IS NOT NULL then dbo.frmAtlDateGer(dog2.F$DFLD#1#)  ELSE dbo.frmAtlDateGer(a.F$CONTRACTDATE) END as DogovorFrom, " +
                        "CASE WHEN dog2.F$DFLD#2# IS NOT NULL then dbo.frmAtlDateGer(dog2.F$DFLD#2#)  ELSE dbo.frmAtlDateGer(dog.F$NREC) END as DogovorEnd " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN T$APPHIST ah ON ah.F$CAPPOINT = c.F$OBJNREC " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = ah.F$CAPPOINT " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$CONTDOC disDoc ON disDoc.F$NREC = (" +
                                   "SELECT TOP 1 disDoc_sub.F$NREC " +
                                   "FROM dbo.T$CONTDOC disDoc_sub " +
                                   "INNER JOIN dbo.T$PARTDOC p_sub ON disDoc_sub.F$CPART = p_sub.F$NREC AND p_sub.F$TYPEOPER = 30008 " +
                                   "LEFT JOIN dbo.T$TITLEDOC t_sub ON p_sub.F$CDOC = t_sub.F$NREC " +
                                   "LEFT JOIN dbo.T$CATALOGS osn ON disDoc_sub.F$CCAT2 = osn.F$NREC  " +
                                   "WHERE disDoc_sub.F$PERSON = person.F$NREC AND t_sub.F$DOCDATE < t.F$DOCDATE " +
                                   "ORDER BY t_sub.F$DOCDATE DESC" +
                                   ") " +
                        "LEFT JOIN  dbo.T$PARTDOC disP ON disDoc.F$CPART = disP.F$NREC " +
                        "LEFT JOIN  dbo.T$TITLEDOC disT ON disP.F$CDOC = disT.F$NREC " +
                        "LEFT JOIN dbo.T$CATALOGS disOsn ON disDoc.F$CCAT2 = disOsn.F$NREC  " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30004()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            DogovorEnd = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd"))
                                ? reader.GetString(reader.GetOrdinal("DogovorEnd"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DisDoc = !reader.IsDBNull(reader.GetOrdinal("DisDoc"))
                                ? reader.GetString(reader.GetOrdinal("DisDoc"))
                                : string.Empty,
                            DisReason = !reader.IsDBNull(reader.GetOrdinal("DisReason"))
                                ? reader.GetString(reader.GetOrdinal("DisReason"))
                                : string.Empty,
                            DisDocDate = !reader.IsDBNull(reader.GetOrdinal("DisDocDate"))
                                ? reader.GetString(reader.GetOrdinal("DisDocDate"))
                                : string.Empty,
                            DopInfo = !reader.IsDBNull(reader.GetOrdinal("DopInfo"))
                                ? reader.GetString(reader.GetOrdinal("DopInfo"))
                                : string.Empty,
                            RecoveryDate = !reader.IsDBNull(reader.GetOrdinal("RecoveryDate"))
                                ? reader.GetString(reader.GetOrdinal("RecoveryDate"))
                                : string.Empty,
                            RecoveryReason = !reader.IsDBNull(reader.GetOrdinal("RecoveryReason"))
                                ? reader.GetString(reader.GetOrdinal("RecoveryReason"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30045
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30045FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30045);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30045>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "CASE WHEN tctv.F$NOTPUS = \'каникулы после прохождения итоговой аттестации\' " +
                            "THEN \'после прохождения итоговой аттестации каникулы\' " +
                            "ELSE \'не корректно указан вид отпуска\' " +
                        "END AS NOtpus, " +
                        "dbo.frmAtlDateGer(tv.F$PLANYEARBEG) AS OtpuskFrom, " +
                        "dbo.frmAtlDateGer(tv.F$PLANYEAREND) AS OtpuskEnd, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as DogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN T$VACATIONS tv ON c.F$NREC = tv.F$CPRIKAZ " +
                        "LEFT JOIN T$KLOTPUSK tctv ON tctv.F$NREC = tv.F$VACTYPE " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30045()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            NOtpus = !reader.IsDBNull(reader.GetOrdinal("NOtpus"))
                                ? reader.GetString(reader.GetOrdinal("NOtpus"))
                                : string.Empty,
                            OtpuskFrom = !reader.IsDBNull(reader.GetOrdinal("OtpuskFrom"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskFrom"))
                                : string.Empty,
                            OtpuskEnd = !reader.IsDBNull(reader.GetOrdinal("OtpuskEnd"))
                                ? reader.GetString(reader.GetOrdinal("OtpuskEnd"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30030
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30030FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30030);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30030>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "tuo.F$WSEMESTR AS Semestr, " +
                        "dbo.frmAtlDateGer(tuo.F$DDATEPROL) AS fromDateProl, " +
                        "COALESCE(dog2.F$SFLD#1#, dog.F$NODOC) as DogovorNum,  " +
                        "CASE WHEN dog2.F$DFLD#1# IS NOT NULL then dbo.frmAtlDateGer(dog2.F$DFLD#1#)  ELSE dbo.frmAtlDateGer(a.F$CONTRACTDATE) END as DogovorFrom " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_ORDERPROLONG tuo ON tuo.F$CORDER = c.F$NREC " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30030()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            FromDateProl = !reader.IsDBNull(reader.GetOrdinal("FromDateProl"))
                                ? reader.GetString(reader.GetOrdinal("FromDateProl"))
                                : string.Empty,
                            Semestr = !reader.IsDBNull(reader.GetOrdinal("Semestr"))
                                ? reader.GetInt32(reader.GetOrdinal("Semestr")).ToString()
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 31074
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd31074FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(31074);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents31074>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "ad.F$SADDRESS1 as HostelAddr, " +
                        "CASE WHEN ad.F$SBLOCK = \'\' THEN ad.F$SADDRESS2 ELSE ad.F$SBLOCK END AS HostelNum, " +
                        "CASE WHEN LEN(ad.F$SDOPFIELD1) > 3 THEN \'(\' + ad.F$SDOPFIELD1 + \' блок)\' ELSE \'\' END AS HostelBloc, " +
                        "ad.F$SFLAT AS HostelRoom, " +
                        "dbo.frmAtlDateGer(p.F$DDAT1) AS DateFrom, " +
                        "dbo.frmAtlDateGer(p.F$DDAT2) AS DateEnd " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$ADDRESSN ad ON ad.F$NREC = c.F$CCAT1 " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents31074()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            HostelAddr = !reader.IsDBNull(reader.GetOrdinal("HostelAddr"))
                                ? reader.GetString(reader.GetOrdinal("HostelAddr"))
                                : string.Empty,
                            HostelNum = !reader.IsDBNull(reader.GetOrdinal("HostelNum"))
                                ? reader.GetString(reader.GetOrdinal("HostelNum"))
                                : string.Empty,
                            HostelBloc = !reader.IsDBNull(reader.GetOrdinal("HostelBloc"))
                                ? reader.GetString(reader.GetOrdinal("HostelBloc"))
                                : string.Empty,
                            HostelRoom = !reader.IsDBNull(reader.GetOrdinal("HostelRoom"))
                                ? reader.GetString(reader.GetOrdinal("HostelRoom"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DateFrom = !reader.IsDBNull(reader.GetOrdinal("DateFrom"))
                                ? reader.GetString(reader.GetOrdinal("DateFrom"))
                                : string.Empty,
                            DateEnd = !reader.IsDBNull(reader.GetOrdinal("DateEnd"))
                                ? reader.GetString(reader.GetOrdinal("DateEnd"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View5.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View6.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 31075
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd31075FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(31075);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents31075>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "adFrom.F$SADDRESS1 as FromHostelAddr, " +
                        "CASE WHEN adFrom.F$SBLOCK = \'\' THEN adFrom.F$SADDRESS2 ELSE adFrom.F$SBLOCK END AS FromHostelNum, " +
                        "CASE WHEN LEN(adFrom.F$SDOPFIELD1) > 3 THEN \'(\' + adFrom.F$SDOPFIELD1 + \' блок)\' ELSE \'\' END AS FromHostelBloc, " +
                        "adFrom.F$SFLAT AS FromHostelRoom, " +
                        "adTo.F$SADDRESS1 as ToHostelAddr, " +
                        "CASE WHEN adTo.F$SBLOCK = \'\' THEN adTo.F$SADDRESS2 ELSE adTo.F$SBLOCK END AS ToHostelNum, " +
                        "CASE WHEN LEN(adTo.F$SDOPFIELD1) > 3 THEN \'(\' + adTo.F$SDOPFIELD1 + \' блок)\' ELSE \'\' END AS ToHostelBloc, " +
                        "adTo.F$SFLAT AS ToHostelRoom, " +
                        "dbo.frmAtlDateGer(p.F$DDAT1) AS DateFrom, " +
                        "dbo.frmAtlDateGer(p.F$DDAT2) AS DateEnd, " +
                        "mh.F$NAME AS ManagingHostel " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$ADDRESSN adFrom ON adFrom.F$NREC = c.F$CCAT1 " +
                        "LEFT JOIN T$ADDRESSN adTo ON adto.F$NREC = c.F$CCAT2 " +
                        "LEFT JOIN T$CATALOGS mh ON mh.F$NREC = p.F$CCAT " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents31075()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            FromHostelAddr = !reader.IsDBNull(reader.GetOrdinal("FromHostelAddr"))
                                ? reader.GetString(reader.GetOrdinal("FromHostelAddr"))
                                : string.Empty,
                            FromHostelNum = !reader.IsDBNull(reader.GetOrdinal("FromHostelNum"))
                                ? reader.GetString(reader.GetOrdinal("FromHostelNum"))
                                : string.Empty,
                            FromHostelBloc = !reader.IsDBNull(reader.GetOrdinal("FromHostelBloc"))
                                ? reader.GetString(reader.GetOrdinal("FromHostelBloc"))
                                : string.Empty,
                            FromHostelRoom = !reader.IsDBNull(reader.GetOrdinal("FromHostelRoom"))
                                ? reader.GetString(reader.GetOrdinal("FromHostelRoom"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DateFrom = !reader.IsDBNull(reader.GetOrdinal("DateFrom"))
                                ? reader.GetString(reader.GetOrdinal("DateFrom"))
                                : string.Empty,
                            DateEnd = !reader.IsDBNull(reader.GetOrdinal("DateEnd"))
                                ? reader.GetString(reader.GetOrdinal("DateEnd"))
                                : string.Empty,
                            ToHostelAddr = !reader.IsDBNull(reader.GetOrdinal("ToHostelAddr"))
                                ? reader.GetString(reader.GetOrdinal("ToHostelAddr"))
                                : string.Empty,
                            ToHostelNum = !reader.IsDBNull(reader.GetOrdinal("ToHostelNum"))
                                ? reader.GetString(reader.GetOrdinal("ToHostelNum"))
                                : string.Empty,
                            ToHostelBloc = !reader.IsDBNull(reader.GetOrdinal("ToHostelBloc"))
                                ? reader.GetString(reader.GetOrdinal("ToHostelBloc"))
                                : string.Empty,
                            ToHostelRoom = !reader.IsDBNull(reader.GetOrdinal("ToHostelRoom"))
                                ? reader.GetString(reader.GetOrdinal("ToHostelRoom"))
                                : string.Empty,
                            ManagingHostel = !reader.IsDBNull(reader.GetOrdinal("ManagingHostel"))
                                ? reader.GetString(reader.GetOrdinal("ManagingHostel"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View5.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View6.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 31076
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd31076FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(31076);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents31076>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "ad.F$SADDRESS1 as HostelAddr, " +
                        "CASE WHEN ad.F$SBLOCK = \'\' THEN ad.F$SADDRESS2 ELSE ad.F$SBLOCK END AS HostelNum, " +
                        "CASE WHEN LEN(ad.F$SDOPFIELD1) > 3 THEN \'(\' + ad.F$SDOPFIELD1 + \' блок)\' ELSE \'\' END AS HostelBloc, " +
                        "ad.F$SFLAT AS HostelRoom, " +
                        "c.F$SBOTTOM AS Reason, " +
                        "CASE WHEN p.F$DDAT1 = 0 AND c.F$WATTRDOC1 = 1 THEN \'даты выхода приказа\' ELSE dbo.frmAtlDateGer(p.F$DDAT1) END AS DateFROM, " +
                        "mh.F$NAME AS ManagingHostel " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$ADDRESSN ad ON ad.F$NREC = c.F$CCAT1 " +
                        "LEFT JOIN T$CATALOGS mh ON mh.F$NREC = p.F$CCAT " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents31076()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            HostelAddr = !reader.IsDBNull(reader.GetOrdinal("HostelAddr"))
                                ? reader.GetString(reader.GetOrdinal("HostelAddr"))
                                : string.Empty,
                            HostelNum = !reader.IsDBNull(reader.GetOrdinal("HostelNum"))
                                ? reader.GetString(reader.GetOrdinal("HostelNum"))
                                : string.Empty,
                            HostelBloc = !reader.IsDBNull(reader.GetOrdinal("HostelBloc"))
                                ? reader.GetString(reader.GetOrdinal("HostelBloc"))
                                : string.Empty,
                            HostelRoom = !reader.IsDBNull(reader.GetOrdinal("HostelRoom"))
                                ? reader.GetString(reader.GetOrdinal("HostelRoom"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DateFrom = !reader.IsDBNull(reader.GetOrdinal("DateFrom"))
                                ? reader.GetString(reader.GetOrdinal("DateFrom"))
                                : string.Empty,
                            ManagingHostel = !reader.IsDBNull(reader.GetOrdinal("ManagingHostel"))
                                ? reader.GetString(reader.GetOrdinal("ManagingHostel"))
                                : string.Empty,
                            Reason = !reader.IsDBNull(reader.GetOrdinal("Reason"))
                                ? reader.GetString(reader.GetOrdinal("Reason"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View5.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View6.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30015
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30015FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30015);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30015>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.fullAge(dbo.frAtlDate(person.F$BORNDATE), dbo.frAtlDate(tub.F$FROMDATE)) AS Age, " +
                        "CASE WHEN dbo.fullAge(dbo.frAtlDate(person.F$BORNDATE), dbo.frAtlDate(tub.F$FROMDATE)) < 18 THEN \'до 18\' ELSE \'18 и старше\' END AS TypeAge, " +
                        "CASE WHEN tc1.F$TYPEOPER = 30001 THEN (CASE WHEN person.F$SEX = \'м\' THEN \'зачисленному на \' ELSE \'зачисленной на \' END) + " +
                        "CONVERT(VARCHAR, ta1.F$VACATION) + \' курс\' WHEN tc1.F$TYPEOPER = 30052 THEN(CASE WHEN person.F$SEX = 'м' THEN " +
                        "\'зачисленному в качестве перевода из другого вуза на \' ELSE \'зачисленной в качестве перевода из другого вуза на \' END)  " +
                        "+ CONVERT(VARCHAR, ta1.F$VACATION) + \' курс\' ELSE \'ОШИБКА В ДАННЫХ: нет сведений о первом назначении\' END AS FirstAppoint, " +
                        "tubName.F$NAME AS Benefit, " +
                        "dbo.frmAtlDateGer(tub.F$FROMDATE) AS FromDate, " +
                        "CASE WHEN tub.F$ENDDATE = bup.F$DATEEND THEN \'до окончания срока обучения в ОмГТУ\' ELSE dbo.frmAtlDateGer(tub.F$ENDDATE) END AS EndDate, " +
                        "COALESCE(dog.F$NODOC, dog2.F$SFLD#1#) as DogovorNum,  " +
                        "CASE WHEN dog.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN T$U_BENEFITS tub ON tub.F$CCONTDOC = c.F$NREC " +
                        "Left JOIN T$CATALOGS  tubName ON tubName.F$NREC = tub.F$CCAT " +
                        "LEFT JOIN T$APPOINTMENTS ta1 ON ta1.F$NREC = person.F$APPOINTFIRST " +
                        "LEFT JOIN T$CONTDOC tc1 ON ta1.F$CCONT = tc1.F$NREC " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN T$STAFFSTRUCT stst ON stst.F$NREC = a.F$STAFFSTR " +
                        "LEFT JOIN T$U_CURRICULUM bup ON bup.F$NREC = stst.F$CSTR " +
                        "LEFT JOIN T$DOGOVOR dog ON dog.F$NREC = a.F$CDOG " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                       $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30015()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            DogovorNum = !reader.IsDBNull(reader.GetOrdinal("DogovorNum"))
                                ? reader.GetString(reader.GetOrdinal("DogovorNum"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            Age = !reader.IsDBNull(reader.GetOrdinal("Age"))
                                ? reader.GetInt32(reader.GetOrdinal("Age")).ToString()
                                : string.Empty,
                            Benefit = !reader.IsDBNull(reader.GetOrdinal("Benefit"))
                                ? reader.GetString(reader.GetOrdinal("Benefit"))
                                : string.Empty,
                            DogovorFrom = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom"))
                                ? reader.GetString(reader.GetOrdinal("DogovorFrom"))
                                : string.Empty,
                            EndDate = !reader.IsDBNull(reader.GetOrdinal("EndDate"))
                                ? reader.GetString(reader.GetOrdinal("EndDate"))
                                : string.Empty,
                            FirstAppoint = !reader.IsDBNull(reader.GetOrdinal("FirstAppoint"))
                                ? reader.GetString(reader.GetOrdinal("FirstAppoint"))
                                : string.Empty,
                            FromDate = !reader.IsDBNull(reader.GetOrdinal("FromDate"))
                                ? reader.GetString(reader.GetOrdinal("FromDate"))
                                : string.Empty,
                            TypeAge = !reader.IsDBNull(reader.GetOrdinal("TypeAge"))
                                ? reader.GetString(reader.GetOrdinal("TypeAge"))
                                : string.Empty,
                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30005
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30005FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30005);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30005>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                                   "p.F$WATTR1 AS DopCodRPD, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(a2.F$APPOINTDATE) AS NewAppDate, " +
                        "st2.F$NAME as NewStudentGroup, " +
                        "a2.F$VACATION as NewStudentCourse, " +
                        "spkau2.F$NAME as NewFinSource, " +
                        "spkau2.F$CODE as NewFinSourceCode, " +
                        "a2.F$WPRIZN as NewFormEdu, " +
                        "CONCAT(spec2.F$CODE, \' \', spec2.F$NAME) as NewSpec, " +
                        "fac2.F$LONGNAME as NewFacult, " +
                        "COALESCE(td.F$NODOC, dog2.F$SFLD#1#) as DogovorNum1,  " +
                        "CASE WHEN td.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom1, " +
                        "CASE WHEN td.F$NREC IS NOT NULL then dbo.frmAtlDateGer(td.F$DEND)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#2#) END as DogovorEnd1, " +
                        "CASE WHEN p.F$WATTR1=1 THEN COALESCE(td.F$NODOC, dog2.F$SFLD#1#) ELSE COALESCE(td2.F$NODOC, dog3.F$SFLD#1#) END as DogovorNum2,  " +
                        "CASE WHEN p.F$WATTR1=1 THEN COALESCE(td.F$NODOC, dog2.F$SFLD#1#) ELSE (CASE WHEN td2.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a2.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog3.F$DFLD#1#) END) END as DogovorFrom2, " +
                        "CASE WHEN p.F$WATTR1=1 THEN COALESCE(td.F$NODOC, dog2.F$SFLD#1#) ELSE (CASE WHEN td2.F$NREC IS NOT NULL then dbo.frmAtlDateGer(td2.F$DEND)  ELSE dbo.frmAtlDateGer(dog3.F$DFLD#2#) END) END as DogovorEnd2 " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$DOGOVOR td ON a.F$CDOG = td.F$NREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a2 ON a2.F$CCONT = c.F$NREC AND a2.F$LPRIZN = 55" +
                        "LEFT JOIN dbo.T$U_STUDGROUP st2 ON st2.F$NREC = a2.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin2 ON fin2.F$NREC = a2.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau2 ON spkau2.F$NREC = fin2.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS spec2 ON spec2.F$NREC = a2.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac2 ON fac2.F$NREC = a2.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$DOGOVOR td2 ON a2.F$CDOG = td2.F$NREC " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                        "LEFT JOIN T$DOPINFO dog3 ON dog3.F$NREC = (SELECT TOP 1 sub_dog3.F$NREC FROM T$DOPINFO sub_dog3 " +
                        "WHERE sub_dog3.F$CPERSON = person.F$NREC AND sub_dog3.F$CDOPTBL = 0x8001000000000007 AND sub_dog3.F$FFLDSUM#1# = 0 ORDER BY sub_dog3.F$DFLD#1# DESC) " +
                        $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30005()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            DopCodRPD = !reader.IsDBNull(reader.GetOrdinal("DopCodRPD"))
                                ? reader.GetInt32(reader.GetOrdinal("DopCodRPD")).ToString()
                                : string.Empty,
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorEnd1 = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd1"))
                               ? reader.GetString(reader.GetOrdinal("DogovorEnd1"))
                               : string.Empty,
                            DogovorEnd2 = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd2"))
                               ? reader.GetString(reader.GetOrdinal("DogovorEnd2"))
                               : string.Empty,
                            DogovorFrom1 = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom1"))
                               ? reader.GetString(reader.GetOrdinal("DogovorFrom1"))
                               : string.Empty,
                            DogovorFrom2 = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom2"))
                               ? reader.GetString(reader.GetOrdinal("DogovorFrom2"))
                               : string.Empty,
                            DogovorNum1 = !reader.IsDBNull(reader.GetOrdinal("DogovorNum1"))
                               ? reader.GetString(reader.GetOrdinal("DogovorNum1"))
                               : string.Empty,
                            DogovorNum2 = !reader.IsDBNull(reader.GetOrdinal("DogovorNum2"))
                               ? reader.GetString(reader.GetOrdinal("DogovorNum2"))
                               : string.Empty,
                            NewAppDate = !reader.IsDBNull(reader.GetOrdinal("NewAppDate"))
                               ? reader.GetString(reader.GetOrdinal("NewAppDate"))
                               : string.Empty,
                            NewFacult = !reader.IsDBNull(reader.GetOrdinal("NewFacult"))
                               ? reader.GetString(reader.GetOrdinal("NewFacult"))
                               : string.Empty,
                            NewFinSource = !reader.IsDBNull(reader.GetOrdinal("NewFinSource"))
                               ? reader.GetString(reader.GetOrdinal("NewFinSource"))
                               : string.Empty,
                            NewFinSourceCode = !reader.IsDBNull(reader.GetOrdinal("NewFinSourceCode"))
                               ? reader.GetString(reader.GetOrdinal("NewFinSourceCode"))
                               : string.Empty,
                            NewFormEdu = !reader.IsDBNull(reader.GetOrdinal("NewFormEdu"))
                               ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("NewFormEdu"))).GetDescription()
                               : string.Empty,
                            NewSpec = !reader.IsDBNull(reader.GetOrdinal("NewSpec"))
                               ? reader.GetString(reader.GetOrdinal("NewSpec"))
                               : string.Empty,
                            NewStudentCourse = !reader.IsDBNull(reader.GetOrdinal("NewStudentCourse"))
                               ? reader.GetInt16(reader.GetOrdinal("NewStudentCourse")).ToString()
                               : string.Empty,
                            NewStudentGroup = !reader.IsDBNull(reader.GetOrdinal("NewStudentGroup"))
                               ? reader.GetString(reader.GetOrdinal("NewStudentGroup"))
                               : string.Empty,

                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30051
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30051FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30051);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30051>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                                   "p.F$WATTR1 AS DopCodRPD, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(a2.F$APPOINTDATE) AS NewAppDate, " +
                        "st2.F$NAME as NewStudentGroup, " +
                        "a2.F$VACATION as NewStudentCourse, " +
                        "spkau2.F$NAME as NewFinSource, " +
                        "spkau2.F$CODE as NewFinSourceCode, " +
                        "a2.F$WPRIZN as NewFormEdu, " +
                        "CONCAT(spec2.F$CODE, \' \', spec2.F$NAME) as NewSpec, " +
                        "fac2.F$LONGNAME as NewFacult, " +
                        "COALESCE(td.F$NODOC, dog2.F$SFLD#1#) as DogovorNum1,  " +
                        "CASE WHEN td.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#1#) END as DogovorFrom1, " +
                        "CASE WHEN td.F$NREC IS NOT NULL then dbo.frmAtlDateGer(td.F$DEND)  ELSE dbo.frmAtlDateGer(dog2.F$DFLD#2#) END as DogovorEnd1, " +
                        "COALESCE(td2.F$NODOC, dog3.F$SFLD#1#) as DogovorNum2,  " +
                        "CASE WHEN td2.F$NREC IS NOT NULL then dbo.frmAtlDateGer(a2.F$CONTRACTDATE)  ELSE dbo.frmAtlDateGer(dog3.F$DFLD#1#) END as DogovorFrom2, " +
                        "CASE WHEN td2.F$NREC IS NOT NULL then dbo.frmAtlDateGer(td2.F$DEND)  ELSE dbo.frmAtlDateGer(dog3.F$DFLD#2#) END as DogovorEnd2 " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$DOGOVOR td ON a.F$CDOG = td.F$NREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a2 ON a2.F$CCONT = c.F$NREC AND a2.F$LPRIZN = 55" +
                        "LEFT JOIN dbo.T$U_STUDGROUP st2 ON st2.F$NREC = a2.F$CCAT1 " +
                        "LEFT JOIN T$STAFFSTRUCT stst2 ON a2.F$STAFFSTR = stst2.F$NREC " +
                        "LEFT JOIN dbo.T$SPKAU spkau2 ON spkau2.F$NREC = stst2.F$CNEWSPR1 " +
                        "LEFT JOIN dbo.T$CATALOGS spec2 ON spec2.F$NREC = a2.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac2 ON fac2.F$NREC = a2.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$DOGOVOR td2 ON a2.F$CDOG = td2.F$NREC " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        "LEFT JOIN T$DOPINFO dog2 ON dog2.F$NREC = (SELECT TOP 1 sub_dog2.F$NREC FROM T$DOPINFO sub_dog2 " +
                        "WHERE sub_dog2.F$CPERSON = person.F$NREC AND sub_dog2.F$CDOPTBL = 0x8001000000000007 AND sub_dog2.F$FFLDSUM#1# in (1,2) ORDER BY sub_dog2.F$DFLD#1# DESC) " +
                        "LEFT JOIN T$DOPINFO dog3 ON dog3.F$NREC = (SELECT TOP 1 sub_dog3.F$NREC FROM T$DOPINFO sub_dog3 " +
                        "WHERE sub_dog3.F$CPERSON = person.F$NREC AND sub_dog3.F$CDOPTBL = 0x8001000000000007 AND sub_dog3.F$FFLDSUM#1# = 0 ORDER BY sub_dog3.F$DFLD#1# DESC) " +
                        $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30051()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            DopCodRPD = !reader.IsDBNull(reader.GetOrdinal("DopCodRPD"))
                                ? reader.GetInt32(reader.GetOrdinal("DopCodRPD")).ToString()
                                : string.Empty,
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "vin", 32775),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            DogovorEnd1 = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd1"))
                               ? reader.GetString(reader.GetOrdinal("DogovorEnd1"))
                               : string.Empty,
                            DogovorEnd2 = !reader.IsDBNull(reader.GetOrdinal("DogovorEnd2"))
                               ? reader.GetString(reader.GetOrdinal("DogovorEnd2"))
                               : string.Empty,
                            DogovorFrom1 = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom1"))
                               ? reader.GetString(reader.GetOrdinal("DogovorFrom1"))
                               : string.Empty,
                            DogovorFrom2 = !reader.IsDBNull(reader.GetOrdinal("DogovorFrom2"))
                               ? reader.GetString(reader.GetOrdinal("DogovorFrom2"))
                               : string.Empty,
                            DogovorNum1 = !reader.IsDBNull(reader.GetOrdinal("DogovorNum1"))
                               ? reader.GetString(reader.GetOrdinal("DogovorNum1"))
                               : string.Empty,
                            DogovorNum2 = !reader.IsDBNull(reader.GetOrdinal("DogovorNum2"))
                               ? reader.GetString(reader.GetOrdinal("DogovorNum2"))
                               : string.Empty,
                            NewAppDate = !reader.IsDBNull(reader.GetOrdinal("NewAppDate"))
                               ? reader.GetString(reader.GetOrdinal("NewAppDate"))
                               : string.Empty,
                            NewFacult = !reader.IsDBNull(reader.GetOrdinal("NewFacult"))
                               ? reader.GetString(reader.GetOrdinal("NewFacult"))
                               : string.Empty,
                            NewFinSource = !reader.IsDBNull(reader.GetOrdinal("NewFinSource"))
                               ? reader.GetString(reader.GetOrdinal("NewFinSource"))
                               : string.Empty,
                            NewFinSourceCode = !reader.IsDBNull(reader.GetOrdinal("NewFinSourceCode"))
                               ? reader.GetString(reader.GetOrdinal("NewFinSourceCode"))
                               : string.Empty,
                            NewFormEdu = !reader.IsDBNull(reader.GetOrdinal("NewFormEdu"))
                               ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("NewFormEdu"))).GetDescription()
                               : string.Empty,
                            NewSpec = !reader.IsDBNull(reader.GetOrdinal("NewSpec"))
                               ? reader.GetString(reader.GetOrdinal("NewSpec"))
                               : string.Empty,
                            NewStudentCourse = !reader.IsDBNull(reader.GetOrdinal("NewStudentCourse"))
                               ? reader.GetInt16(reader.GetOrdinal("NewStudentCourse")).ToString()
                               : string.Empty,
                            NewStudentGroup = !reader.IsDBNull(reader.GetOrdinal("NewStudentGroup"))
                               ? reader.GetString(reader.GetOrdinal("NewStudentGroup"))
                               : string.Empty,

                        };

                        if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }

                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод возвращает приказы по РПД 30007
        /// </summary>
        /// <returns></returns>
        public List<GalOrder> GetOrdersRpd30007FromDb()
        {
            var listOrder = new List<GalOrder>();

            var activeOrders = this._getActiveOrderFromDb(30007);
            if (!activeOrders.Any()) return listOrder;

            foreach (var oneOrder in activeOrders)
            {
                var oneOrderList = _getGalOrderDescriptionFromDb(oneOrder);
                var signature = _getGalOrderSignatureFromDb(oneOrder);

                var students = new List<GalOrderStudents30007>();
                using (var galcontext = new GalDbContext())
                {
                    var queryGal = "SELECT " +
                                   "person.F$NREC as PersonNrec, " +
                                   "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                                   "p.F$WATTR1 AS DopCodRPD, " +
                        "person.F$FIO as FioStudent, " +
                        "person.F$SEX as Sex, " +
                        "gr.F$NAME as Gr, " +
                        "gr.F$Code as GrCode, " +
                        "REPLACE(dbo.cp866_to_1251(xx.M#DATA), \'TXT\', \'\') as BasisOfOrder, " +
                        "COALESCE(a.F$STRTABN, person.F$STRTABN) as Strtabn, " +
                        "st.F$NAME as StudentGroup, " +
                        "a.F$VACATION as StudentCourse, " +
                        "spkau.F$NAME as FinSource, " +
                        "spkau.F$CODE as FinSourceCode, " +
                        "a.F$WPRIZN as FormEdu, " +
                        "CONCAT(spec.F$CODE, \' \', spec.F$NAME) as Spec, " +
                        "planCat.F$NAME as PlanStudy, " +
                        "fac.F$LONGNAME as Facult, " +
                        "dbo.frmAtlDateGer(p.F$DDAT1) AS DateTravel, " +
                        "fac2.F$NAME as NewFacult, " +
                        "CONCAT(spec2.F$CODE, \' \', spec2.F$NAME) as NewSpec, " +
                        "a2.F$WPRIZN as NewFormEdu, " +
                        "spez2.F$NAME as NewSpecialization, " +
                        "CASE WHEN st2.F$NAME IS NULL THEN \'\' ELSE st2.F$NAME END as NewStudentGroup " +
                        "FROM dbo.T$TITLEDOC t " +
                        "LEFT JOIN dbo.T$PARTDOC p ON p.F$CDOC = t.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC cM ON cM.F$CPART = p.F$NREC " +
                        "LEFT JOIN dbo.T$CONTDOC c ON c.F$CDOPREF = cM.F$NREC " +
                        "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                        "LEFT JOIN dbo.T$CATALOGS gr ON gr.F$NREC = person.F$GR " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a ON a.F$NREC = c.F$CSTR " +
                        "LEFT JOIN dbo.T$U_STUDGROUP st ON st.F$NREC = a.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin ON fin.F$NREC = a.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau ON spkau.F$NREC = fin.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS planCat ON planCat.F$NREC = c.F$CDOPREF " +
                        "LEFT JOIN dbo.T$CATALOGS spec ON spec.F$NREC = a.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac ON fac.F$NREC = a.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$DOGOVOR td ON a.F$CDOG = td.F$NREC " +
                        "LEFT JOIN dbo.T$APPOINTMENTS a2 ON a2.F$CCONT = c.F$NREC AND a2.F$LPRIZN = 55" +
                        "LEFT JOIN dbo.T$U_STUDGROUP st2 ON st2.F$NREC = a2.F$CCAT1 " +
                        "LEFT JOIN dbo.T$U_STUD_FINSOURCE fin2 ON fin2.F$NREC = a2.F$CREF2 " +
                        "LEFT JOIN dbo.T$SPKAU spkau2 ON spkau2.F$NREC = fin2.F$CFINSOURCE " +
                        "LEFT JOIN dbo.T$CATALOGS spec2 ON spec2.F$NREC = a2.F$POST " +
                        "LEFT JOIN dbo.T$CATALOGS fac2 ON fac2.F$NREC = a2.F$PRIVPENSION " +
                        "LEFT JOIN dbo.T$STAFFSTRUCT stst2 ON stst2.F$NREC = a2.F$STAFFSTR " +
                        "LEFT JOIN dbo.T$U_CURRICULUM bup2 ON bup2.F$NREC = stst2.F$CSTR " +
                        "LEFT JOIN dbo.T$U_SPECIALIZATION spez2 ON spez2.F$NREC = bup2.F$CSPECIALIZATION " +
                        "LEFT JOIN dbo.T$DOGOVOR td2 ON a2.F$CDOG = td2.F$NREC " +
                        "LEFT JOIN dbo.T$PRMEMO pr ON (pr.F$CDOC = c.F$NREC AND pr.F$WREF = 10) " +
                        "LEFT JOIN XX$MEMO xx ON (xx.M#NREC = pr.F$NREC AND xx.M#Code = 25063) " +
                        $"WHERE t.F$NREC = {oneOrder}";

                    var reader = galcontext.ExecuteQuery(queryGal);
                    while (reader.Read())
                    {
                        var oneStudent = new GalOrderStudents30007()
                        {
                            PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                            PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                                ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                                : 0,
                            PersonNrecString =
                                DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                    .Value),
                            DopCodRPD = !reader.IsDBNull(reader.GetOrdinal("DopCodRPD"))
                                ? reader.GetInt32(reader.GetOrdinal("DopCodRPD")).ToString()
                                : string.Empty,
                            FioStudent = !reader.IsDBNull(reader.GetOrdinal("FioStudent"))
                                ? reader.GetString(reader.GetOrdinal("FioStudent"))
                                : string.Empty,
                            FioStudentCaseChanging = _getFioCaseChanging(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value), "dat", 24583),
                            Sex = !reader.IsDBNull(reader.GetOrdinal("Sex"))
                                ? reader.GetString(reader.GetOrdinal("Sex"))
                                : string.Empty,
                            FinSourceCode = !reader.IsDBNull(reader.GetOrdinal("FinSourceCode"))
                                ? reader.GetString(reader.GetOrdinal("FinSourceCode"))
                                : string.Empty,
                            Gr = !reader.IsDBNull(reader.GetOrdinal("Gr"))
                                ? reader.GetString(reader.GetOrdinal("Gr"))
                                : string.Empty,
                            GrCode = !reader.IsDBNull(reader.GetOrdinal("GrCode"))
                                ? reader.GetString(reader.GetOrdinal("GrCode"))
                                : string.Empty,
                            BasisOfOrder = !reader.IsDBNull(reader.GetOrdinal("BasisOfOrder"))
                                ? reader.GetString(reader.GetOrdinal("BasisOfOrder"))
                                : string.Empty,
                            Strtabn = !reader.IsDBNull(reader.GetOrdinal("Strtabn"))
                                ? reader.GetString(reader.GetOrdinal("Strtabn"))
                                : string.Empty,
                            StudentGroup = !reader.IsDBNull(reader.GetOrdinal("StudentGroup"))
                                ? reader.GetString(reader.GetOrdinal("StudentGroup"))
                                : string.Empty,
                            StudentCourse = !reader.IsDBNull(reader.GetOrdinal("StudentCourse"))
                                ? reader.GetInt16(reader.GetOrdinal("StudentCourse")).ToString()
                                : string.Empty,
                            FinSource = !reader.IsDBNull(reader.GetOrdinal("FinSource"))
                                ? reader.GetString(reader.GetOrdinal("FinSource"))
                                : string.Empty,
                            FormEdu = !reader.IsDBNull(reader.GetOrdinal("FormEdu"))
                                ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("FormEdu"))).GetDescription()
                                : string.Empty,
                            Spec = !reader.IsDBNull(reader.GetOrdinal("Spec"))
                                ? reader.GetString(reader.GetOrdinal("Spec"))
                                : string.Empty,
                            PlanStudy = !reader.IsDBNull(reader.GetOrdinal("PlanStudy"))
                                ? reader.GetString(reader.GetOrdinal("PlanStudy"))
                                : string.Empty,
                            Facult = !reader.IsDBNull(reader.GetOrdinal("Facult"))
                                ? reader.GetString(reader.GetOrdinal("Facult"))
                                : string.Empty,
                            NewFacult = !reader.IsDBNull(reader.GetOrdinal("NewFacult"))
                               ? reader.GetString(reader.GetOrdinal("NewFacult"))
                               : string.Empty,
                            NewFormEdu = !reader.IsDBNull(reader.GetOrdinal("NewFormEdu"))
                               ? ((FormEduEnum)reader.GetInt32(reader.GetOrdinal("NewFormEdu"))).GetDescription()
                               : string.Empty,
                            NewSpec = !reader.IsDBNull(reader.GetOrdinal("NewSpec"))
                               ? reader.GetString(reader.GetOrdinal("NewSpec"))
                               : string.Empty,
                            NewStudentGroup = !reader.IsDBNull(reader.GetOrdinal("NewStudentGroup"))
                               ? reader.GetString(reader.GetOrdinal("NewStudentGroup"))
                               : string.Empty,
                            DateTravel = !reader.IsDBNull(reader.GetOrdinal("DateTravel"))
                                ? reader.GetString(reader.GetOrdinal("DateTravel"))
                                : string.Empty,
                            NewSpecialization = !reader.IsDBNull(reader.GetOrdinal("NewSpecialization"))
                                ? reader.GetString(reader.GetOrdinal("NewSpecialization"))
                                : string.Empty,

                        };

                        /*if (string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        }
                        else if (!string.Equals(oneStudent.GrCode, "643") && oneOrderList.ViewSed == string.Empty)
                        {
                            oneOrderList.ViewSed = ViewSedEnum.View2.GetDescription();
                        }*/
                        oneOrderList.ViewSed = ViewSedEnum.View1.GetDescription();
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\r", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\n", "");
                        oneStudent.BasisOfOrder = oneStudent.BasisOfOrder.Replace("\\", "");
                        students.Add(oneStudent);
                    }
                }

                oneOrderList.Signature = signature;
                oneOrderList.Students = students;

                listOrder.Add(oneOrderList);
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод ищет  в базе историю изменения ФИО
        /// </summary>
        /// <returns></returns>
        public List<JsonHistoryFioChange> GetAllHistoryFioChangeFromDb()
        {
            var listOrder = new List<JsonHistoryFioChange>();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "person.F$NREC as PersonNrec, " +
                               "dbo.toInt64(person.F$NREC) as PersonNrecStringInt64, " +
                               "cp.F$PSNV AS FioOld, " +
                               "cp.F$PSND AS FioNew, " +
                               "dbo.frmAtlDateGer(cp.F$DCHANGE) as FioNewDateChange " +
                               "FROM dbo.T$CONTDOC c " +
                               "INNER JOIN T$CASEPSN cp on cp.F$NREC = c.F$OBJNREC " +
                               "LEFT JOIN dbo.T$PERSONS person ON person.F$NREC = c.F$PERSON " +
                               "WHERE c.F$TYPEOPER = 30002 and person.F$NREC is NOT NULL " +
                               "ORDER BY person.F$NREC ASC, cp.F$DCHANGE DESC"
                    ;


                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    var oneStudent = new JsonHistoryFioChange()
                    {
                        PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                        PersonNrecStringInt64 = !reader.IsDBNull(reader.GetOrdinal("PersonNrecStringInt64"))
                            ? reader.GetInt64(reader.GetOrdinal("PersonNrecStringInt64"))
                            : 0,
                        PersonNrecString =
                            DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("PersonNrec"))
                                .Value),
                        FioNew = !reader.IsDBNull(reader.GetOrdinal("FioNew"))
                            ? reader.GetString(reader.GetOrdinal("FioNew"))
                            : string.Empty,
                        FioOld = !reader.IsDBNull(reader.GetOrdinal("FioOld"))
                            ? reader.GetString(reader.GetOrdinal("FioOld"))
                            : string.Empty,
                        FioNewDateChange = !reader.IsDBNull(reader.GetOrdinal("FioNewDateChange"))
                            ? reader.GetString(reader.GetOrdinal("FioNewDateChange"))
                            : string.Empty,
                    };

                    listOrder.Add(oneStudent);
                }
            }

            return listOrder;
        }

        public JsonWorkCurrStruct GetGetWorkCurrStruct(JsonWorkCurrStruct structList)
        {
            var structCurr = new JsonWorkCurrStruct();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "WITH " +
                               "TYPEWORK AS (SELECT " +
                               "ttt.id , ttt.TW_ID , ttt.TW_Name , ttt.TW_Unit , ttt.WTKoeff , ttt.TW_WTYPE , ttt.TW_WTYPE_Dop , ttt.TW_Mask , ttt.TW_AllWork , ttt.TW_Aud " +
                               ", CASE WHEN ttt.TW_Aud = 1 AND (ttt.TW_WTYPE = 1 OR ttt.TW_Lec = 1 OR ttt.TW_WTYPE_Dop = 1) THEN 1 ELSE 0 END \'TWLec\' " +
                               ", CASE WHEN ttt.TW_AllWork = 1 AND ttt.TW_Aud = 1 AND (ttt.TW_WTYPE = 3 OR ttt.TW_WTYPE_Dop = 3) THEN 1 " +
                               "WHEN ttt.TW_AllWork = 1 AND (ttt.TW_Aud = 1 AND ttt.TW_WTYPE = 19) THEN 1   ELSE 0 END \'TWPZ\' " +
                               ", CASE WHEN ttt.TW_AllWork = 1 AND ttt.TW_Aud = 1 AND (ttt.TW_WTYPE = 2 OR ttt.TW_WTYPE_Dop = 2) THEN 1 ELSE 0 END \'TWLab\' " +
                               ", ttt.TW_SRS " +
                               ", CASE WHEN ttt.TW_AllWork = 1 AND (ttt.TW_SRS = 1 OR ttt.TW_WTYPE = 10 OR ttt.TW_WTYPE_Dop = 10) THEN 1 ELSE 0 END \'TWSRS\' " +
                               ", ttt.TW_KSR " +
                               ", CASE WHEN ttt.TW_AllWork = 0 AND (ttt.TW_KSR = 1 OR ttt.TW_WTYPE = 12 OR ttt.TW_WTYPE_Dop = 12) THEN 1 ELSE 0 END \'TWKSR\' " +
                               ", CASE WHEN ttt.TW_AllWork = 1 AND ttt.TW_Aud = 0 AND (ttt.TW_Practik = 1 OR ttt.TW_WTYPE = 17 OR ttt.TW_WTYPE_Dop = 17) THEN 1 ELSE 0 END \'TWPract\' " +
                               ", ttt.TW_GIA " +
                               ", CASE WHEN ttt.TW_AllWork = 1 AND ttt.TW_Aud = 0 AND (ttt.TW_WTYPE IN (15, 16) OR ttt.TW_WTYPE_Dop IN (15, 16) OR ttt.TW_GIA = 1) THEN 1 ELSE 0 END \'TWIGA\' " +
                               ", CASE WHEN ttt.TW_AllWork = 1 AND ttt.TW_WTYPE = 22 OR ttt.TW_WTYPE_Dop = 22 THEN 1 ELSE 0 END \'HourExam\' " +
                               ", ttt.TW_Att " +
                               ", CASE WHEN ttt.TW_Att = 1 AND ttt.TW_WTYPE = 4 OR ttt.TW_WTYPE_Dop = 4 THEN 1 ELSE 0 END \'TWAttEx\' " +
                               ", CASE WHEN ttt.TW_Att = 1 AND (ttt.TW_WTYPE = 5 OR ttt.TW_WTYPE_Dop = 5) AND ttt.TW_Att_Dif = 0 THEN 1 ELSE 0 END \'TWAttZh\' " +
                               ", CASE WHEN ttt.TW_Att = 1 AND (ttt.TW_WTYPE = 5 OR ttt.TW_WTYPE_Dop = 5) AND ttt.TW_Att_Dif = 1 THEN 1 ELSE 0 END \'TWAttDZh\' " +
                               ", CASE WHEN ttt.TW_Att = 1 AND (ttt.TW_WTYPE = 8 OR ttt.TW_WTYPE_Dop = 8) THEN 1 ELSE 0 END \'TWAttKP\' " +
                               ", CASE WHEN ttt.TW_Att = 1 AND (ttt.TW_WTYPE = 14 OR ttt.TW_WTYPE_Dop = 14) THEN 1 ELSE 0 END \'TWAttKR\' " +
                               ", CASE WHEN ttt.TW_Att = 1 AND (ttt.TW_Practik = 1 or (ttt.TW_WTYPE = 17 OR ttt.TW_WTYPE_Dop = 17)) THEN 1 ELSE 0 END \'TWAttPr\' " +
                               ", CASE WHEN ttt.TW_Att = 1 AND (ttt.TW_WTYPE IN (15, 16) OR ttt.TW_WTYPE_Dop IN (15, 16)) THEN 1 ELSE 0 END \'TWAttIGA\' " +
                               ", CASE WHEN ttt.TW_Att = 1 THEN " +
                               "(CASE WHEN ttt.TW_WTYPE = 4 OR ttt.TW_WTYPE_Dop = 4 THEN \'э\' " +
                               "WHEN (ttt.TW_WTYPE = 5 OR ttt.TW_WTYPE_Dop = 5) AND ttt.TW_Att_Dif = 0 THEN \'з\' " +
                               "WHEN (ttt.TW_WTYPE = 5 OR ttt.TW_WTYPE_Dop = 5) AND ttt.TW_Att_Dif = 1 THEN \'дз\' " +
                               "WHEN (ttt.TW_WTYPE = 8 OR ttt.TW_WTYPE_Dop = 8) THEN \'кп\' " +
                               "WHEN (ttt.TW_WTYPE = 14 OR ttt.TW_WTYPE_Dop = 14) THEN \'кр\' " +
                               "WHEN (ttt.TW_Practik = 1 or (ttt.TW_WTYPE = 17 OR ttt.TW_WTYPE_Dop = 17)) THEN \'п\' " +
                               "WHEN ttt.TW_WTYPE IN (15, 16) THEN \'\' " +
                               "ELSE ttt.TW_Abbr END) ELSE \'\' END \'TWAbb_1\' " +
                               ", CASE WHEN ttt.TW_Att = 1 THEN " +
                               "(CASE WHEN ttt.TW_WTYPE = 4 OR ttt.TW_WTYPE_Dop = 4 THEN \'\' " +
                               "WHEN (ttt.TW_WTYPE = 5 OR ttt.TW_WTYPE_Dop = 5) AND ttt.TW_Att_Dif = 0 THEN \'\' " +
                               "WHEN (ttt.TW_WTYPE = 5 OR ttt.TW_WTYPE_Dop = 5) AND ttt.TW_Att_Dif = 1 THEN \'*\' " +
                               "WHEN (ttt.TW_WTYPE = 8 OR ttt.TW_WTYPE_Dop = 8) THEN \'п\' " +
                               "WHEN (ttt.TW_WTYPE = 14 OR ttt.TW_WTYPE_Dop = 14) THEN \'р\' " +
                               "WHEN (ttt.TW_Practik = 1 or (ttt.TW_WTYPE = 17 OR ttt.TW_WTYPE_Dop = 17)) THEN \'*\' " +
                               "WHEN (ttt.TW_WTYPE IN (15, 16) OR ttt.TW_WTYPE_Dop IN (15, 16)) THEN \'\' " +
                               "ELSE ttt.TW_Abbr END) ELSE \'\' END \'TWAbb\' " +
                               "FROM " +
                               "(SELECT " +
                               "tut.F$NREC \'id\', tut.F$NREC \'TW_ID\', tut.F$NAME \'TW_Name\', tut.F$ABBR \'TW_Abbr\' " +
                               ", dbo.DecToBin(tut.F$WTYPEMASK, 0, 11) \'TW_Mask\' , dbo.DecToBin(tut.F$WTYPEMASK, 1, 11) \'TW_AllWork\' , dbo.DecToBin(tut.F$WTYPEMASK, 2, 11) \'TW_Aud\' , dbo.DecToBin(tut.F$WTYPEMASK, 3, 11) \'TW_SRS\' " +
                               ", dbo.DecToBin(tut.F$WTYPEMASK, 4, 11) \'TW_KSR\'  , dbo.DecToBin(tut.F$WTYPEMASK, 6, 11) \'TW_Att\'  , tut.[F$WADDFLD#1#] \'TW_Att_Dif\'   , dbo.DecToBin(tut.F$WTYPEMASK, 7, 11) \'TW_Practik\' " +
                               ", dbo.DecToBin(tut.F$WTYPEMASK, 8, 11) \'TW_Lec\'  , CASE WHEN tut.[F$WADDFLD#5#] = 15 THEN 1 ELSE 0 END \'TW_GIA\' " +
                               ", tut.F$WTYPE \'TW_WTYPE\' " +
                               ", tut.[F$WADDFLD#5#] \'TW_WTYPE_Dop\' " +
                               ", CONVERT(VARCHAR, (CASE  WHEN tut.F$WTYPE IN (0, 1, 2, 3, 6, 7, 9, 10, 11, 12, 17, 19, 22, 24, 25, 26, 28) THEN \'час\' " +
                               "WHEN tut.F$WTYPE IN (4, 5, 8, 13, 14, 15, 16, 18, 21, 23, 27)  THEN \'шт\' " +
                               "WHEN tut.F$WTYPE = 20 THEN \'недель\' " +
                               "END)) \'TW_Unit\' " +
                               ", CONVERT(INT, (CASE  WHEN tut.F$WTYPE IN (0, 1, 2, 3, 6, 7, 9, 10, 11, 12, 17, 19, 22, 24, 25, 26, 28) THEN 1 " +
                               "WHEN tut.F$WTYPE IN (4, 5, 8, 13, 14, 15, 16, 18, 21, 23, 27)  THEN 0 " +
                               "WHEN tut.F$WTYPE = 20 THEN 54 " +
                               "END)) \'WTKoeff\' " +
                               "FROM T$U_TYPEWORK tut) ttt ) " +
                               "SELECT DISTINCT " +
                               "tt.CurDisNrec ,tt.CycleNrec ,tt.[CYCLE] ,tt.CycleType ,tt.CompName ,tt.WTypeComponent ,tt.WLevelReal ,tt.CCOMPONENT ,tt.CDIS ,tt.LevelComponentForDVS ,tt.CycleReal ,tt.ComponentReal " +
                               ",tt.DC_Type ,tt.DisWPROP ,tt.DisCur ,tt.InSize ,tt.Koeff ,tt.Levelf ,tt.LevelCode  ,tt.cod  ,tt.Abbr  ,tt.Kaf  ,tt.KafAbb  ,tt.DC_Type1  ,tt.ZE_All  ,tt.Hour_All  ,tt.ZE_Pl  ,tt.ReZE  ,tt.HourCurPl  ,tt.ReHour " +
                               ",tt.Aud_Pl  ,tt.HOUREXAM  ,tt.SRS_Pl  ,tt.Lec_Pl  ,tt.Pr_Pl  ,tt.Lab_Pl " +
                               ", CASE WHEN tt.CycleType = \'г\' AND  tt.Levelf IN (0, 1) THEN SUM(tt.ksrIGA + tt.KSR_Pl) OVER (PARTITION BY tt.CycleType) ELSE (tt.ksrIGA  + tt.KSR_Pl)  END \'KSR_Pl\' " +
                               ",tt.Pr ,tt.IGA  ,tt.AttAll  ,tt.ReAtt  ,tt.AttEx  ,tt.AttZh  ,tt.AttKP ,tt.Lec1s  ,tt.PZs1s  ,tt.LRs1s  ,tt.SRSs1s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs1s > 0 THEN tt.ksrIGA ELSE tt.KSRs1s END \'KSRs1s\' " +
                               ",tt.PrIGAs1s  ,tt.Lec2s  ,tt.PZs2s  ,tt.LRs2s  ,tt.SRSs2s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs2s > 0 THEN tt.ksrIGA ELSE tt.KSRs2s END \'KSRs2s\' " +
                               ",tt.PrIGAs2s ,tt.Lec3s  ,tt.PZs3s  ,tt.LRs3s  ,tt.SRSs3s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs3s > 0 THEN tt.ksrIGA ELSE tt.KSRs3s END \'KSRs3s\' " +
                               ",tt.PrIGAs3s  ,tt.Lec4s  ,tt.PZs4s  ,tt.LRs4s  ,tt.SRSs4s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs4s > 0 THEN tt.ksrIGA ELSE tt.KSRs4s END \'KSRs4s\' " +
                               ",tt.PrIGAs4s ,tt.Lec5s  ,tt.PZs5s  ,tt.LRs5s  ,tt.SRSs5s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs5s > 0 THEN tt.ksrIGA ELSE tt.KSRs5s END \'KSRs5s\' " +
                               ",tt.PrIGAs5s ,tt.Lec6s  ,tt.PZs6s  ,tt.LRs6s  ,tt.SRSs6s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs6s > 0 THEN tt.ksrIGA ELSE tt.KSRs6s END \'KSRs6s\' " +
                               ",tt.PrIGAs6s ,tt.Lec7s  ,tt.PZs7s  ,tt.LRs7s  ,tt.SRSs7s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs7s > 0 THEN tt.ksrIGA ELSE tt.KSRs7s END \'KSRs7s\' " +
                               ",tt.PrIGAs7s ,tt.Lec8s  ,tt.PZs8s  ,tt.LRs8s  ,tt.SRSs8s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs8s > 0 THEN tt.ksrIGA ELSE tt.KSRs8s END \'KSRs8s\' " +
                               ",tt.PrIGAs8s ,tt.Lec9s  ,tt.PZs9s  ,tt.LRs9s  ,tt.SRSs9s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs9s > 0 THEN tt.ksrIGA ELSE tt.KSRs9s END \'KSRs9s\' " +
                               ",tt.PrIGAs9s ,tt.Lec10s ,tt.PZs10s  ,tt.LRs10s  ,tt.SRSs10s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs10s > 0 THEN tt.ksrIGA ELSE tt.KSRs10s END \'KSRs10s\' " +
                               ",tt.PrIGAs10s ,tt.Lec11s  ,tt.PZs11s   ,tt.LRs11s  ,tt.SRSs11s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs11s > 0 THEN tt.ksrIGA ELSE tt.KSRs11s END \'KSRs11s\' " +
                               ",tt.PrIGAs11s ,tt.Lec12s  ,tt.PZs12s  ,tt.LRs12s  ,tt.SRSs12s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs12s > 0 THEN tt.ksrIGA ELSE tt.KSRs12s END \'KSRs12s\' " +
                               ",tt.PrIGAs12s ,tt.Lec13s  ,tt.PZs13s  ,tt.LRs13s  ,tt.SRSs13s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs13s > 0 THEN tt.ksrIGA ELSE tt.KSRs13s END \'KSRs13s\' " +
                               ",tt.PrIGAs13s ,tt.Lec14s  ,tt.PZs14s  ,tt.LRs14s  ,tt.SRSs14s " +
                               ", CASE WHEN tt.CycleType = \'г\' AND tt.ksrIGA > 0 AND tt.PrIGAs14s > 0 THEN tt.ksrIGA ELSE tt.KSRs14s END \'KSRs14s\' " +
                               ",tt.PrIGAs14s  " +
                               "FROM " +
                               "( " +
                               "SELECT " +
                               "ITOG.CurDisNrec AS CurDisNrec " +
                               "  , ITOG.CycleNrec AS CycleNrec " +
                               "  , ITOG.[CYCLE] AS [CYCLE] " +
                               "  , ITOG.CycleType AS CycleType " +
                               "  , ITOG.CompName AS CompName " +
                               "  , ITOG.WTypeComponent AS WTypeComponent " +
                               "  , ITOG.WLevelReal AS WLevelReal " +
                               "  , ITOG.CCOMPONENT AS CCOMPONENT " +
                               "  , ITOG.CDIS AS CDis " +
                               "  , ITOG.LevelComponentForDVS AS LevelComponentForDVS " +
                               "  , ITOG.CycleReal AS CycleReal " +
                               "  , ITOG.ComponentReal AS ComponentReal " +
                               "  , ITOG.DC_Type AS DC_Type " +
                               "  , ITOG.DisWPROP AS DisWPROP " +
                               "  , ITOG.DisCur AS DisCur " +
                               "  , ITOG.InSize AS InSize " +
                               "  , ITOG.Koeff AS Koeff " +
                               "  , ITOG.Levelf AS Levelf " +
                               "  , ITOG.LevelCode AS LevelCode " +
                               "  , ITOG.Cod AS cod " +
                               "  , ITOG.Abbr AS Abbr " +
                               "  , ITOG.Kaf AS Kaf " +
                               "  , ITOG.KafAbb AS KafAbb " +
                               ", CASE WHEN ITOG.CompName IS NULL THEN \'ц\' ELSE \'к\' END AS DC_Type1 " +
                               ", CASE WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.ZE_All IS NOT NULL THEN (ITOG.ZE_All) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.ZE_All IS NOT NULL THEN  ITOG.ZE_All * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.ZE_All IS NOT NULL THEN (ITOG.ZE_All) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.ZE_All * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END  AS ZE_All " +
                               ", CASE  WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.Hour_All IS NOT NULL THEN (ITOG.Hour_All) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.Hour_All IS NOT NULL THEN  ITOG.Hour_All * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.Hour_All IS NOT NULL THEN (ITOG.Hour_All) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.Hour_All * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS Hour_All " +
                               ", CASE  WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.ZE_Pl IS NOT NULL THEN (ITOG.ZE_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.ZE_Pl IS NOT NULL THEN  ITOG.ZE_Pl * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.ZE_Pl IS NOT NULL THEN (ITOG.ZE_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.ZE_Pl * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS ZE_Pl " +
                               ", CASE  WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.ReZE IS NOT NULL THEN (ITOG.ReZE) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.ReZE IS NOT NULL THEN  ITOG.ReZE * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.ReZE IS NOT NULL THEN (ITOG.ReZE) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.ReZE * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS ReZE " +
                               ", CASE  WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.HourCurPl IS NOT NULL THEN (ITOG.HourCurPl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.HourCurPl IS NOT NULL THEN  ITOG.HourCurPl * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.HourCurPl IS NOT NULL THEN (ITOG.HourCurPl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.HourCurPl * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS HourCurPl " +
                               ", CASE   WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.ReHour IS NOT NULL THEN (ITOG.ReHour) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.ReHour IS NOT NULL THEN  ITOG.ReHour * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.ReHour IS NOT NULL THEN (ITOG.ReHour) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.ReHour * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS ReHour " +
                               ", CASE   WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.Aud_Pl IS NOT NULL THEN (ITOG.Aud_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.Aud_Pl IS NOT NULL THEN  ITOG.Aud_Pl * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.Aud_Pl IS NOT NULL THEN (ITOG.Aud_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.Aud_Pl * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS Aud_Pl " +
                               ", CASE   WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.HOUREXAM IS NOT NULL THEN (ITOG.HOUREXAM) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.HOUREXAM IS NOT NULL THEN  ITOG.HOUREXAM * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.HOUREXAM IS NOT NULL THEN (ITOG.HOUREXAM) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.HOUREXAM * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS HOUREXAM " +
                               ", CASE   WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.SRS_Pl IS NOT NULL THEN (ITOG.SRS_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN (CASE WHEN ITOG.SRS_Pl IS NOT NULL THEN  ITOG.SRS_Pl * ITOG.InSize ELSE 0 END) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.SRS_Pl IS NOT NULL THEN (ITOG.SRS_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WLevelReal = 1 THEN SUM(CASE WHEN ITOG.Levelf = 1 THEN (ITOG.SRS_Pl * ITOG.InSize) ELSE 0 END)  OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS SRS_Pl " +
                               ", CASE   WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.Lec_Pl IS NOT NULL THEN (ITOG.Lec_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN SUM(CASE WHEN ITOG.Lec_Pl IS NOT NULL THEN  ITOG.Lec_Pl * ITOG.InSize ELSE 0 END) OVER (PARTITION BY ITOG.CycleReal) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.Lec_Pl IS NOT NULL THEN (ITOG.Lec_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WTypeComponent != 3  THEN SUM(ITOG.Lec_Pl * ITOG.InSize) OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS Lec_Pl " +
                               ", CASE WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.Pr_Pl IS NOT NULL THEN (ITOG.Pr_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN SUM(CASE WHEN ITOG.Pr_Pl IS NOT NULL THEN  ITOG.Pr_Pl * ITOG.InSize ELSE 0 END) OVER (PARTITION BY ITOG.CycleReal) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.Pr_Pl IS NOT NULL THEN (ITOG.Pr_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WTypeComponent != 3  THEN SUM(ITOG.Pr_Pl * ITOG.InSize) OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS Pr_Pl " +
                               ", CASE WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.Lab_Pl IS NOT NULL THEN (ITOG.Lab_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN SUM(CASE WHEN ITOG.Lab_Pl IS NOT NULL THEN  ITOG.Lab_Pl * ITOG.InSize ELSE 0 END) OVER (PARTITION BY ITOG.CycleReal) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.Lab_Pl IS NOT NULL THEN (ITOG.Lab_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WTypeComponent != 3  THEN SUM(ITOG.Lab_Pl * ITOG.InSize) OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS Lab_Pl " +
                               ", " +
                               "CASE WHEN ITOG.Levelf = 2 AND ITOG.CycleType = \'г\' " +
                               "THEN CASE 	WHEN (COUNT(ITOG.CurDisNrec) OVER (PARTITION BY ITOG.CycleType)) > 3 " +
                               "THEN (CASE WHEN ITOG.DisCur LIKE \'%выпускн%\' THEN 30 ELSE 0 END) " +
                               "WHEN (COUNT(ITOG.CurDisNrec) OVER (PARTITION BY ITOG.CycleType)) < 4 THEN 24 " +
                               "ELSE 0 END  " +
                               "  ELSE 0 " +
                               "END AS ksrIGA " +
                               ", CASE  " +
                               "WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.KSR_Pl IS NOT NULL THEN (ITOG.KSR_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN SUM(CASE WHEN ITOG.KSR_Pl IS NOT NULL THEN  ITOG.KSR_Pl* ITOG.InSize  ELSE 0 END) OVER (PARTITION BY ITOG.CycleReal) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.KSR_Pl IS NOT NULL THEN (ITOG.KSR_Pl ) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WTypeComponent != 3  THEN SUM(ITOG.KSR_Pl * ITOG.InSize) OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS KSR_Pl " +
                               ", CASE WHEN ITOG.Levelf = 2 AND ITOG.WTypeComponent = 3 THEN  SUM(CASE WHEN ITOG.Prac_Pl IS NOT NULL THEN (ITOG.Prac_Pl) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.Levelf = 0  THEN SUM(CASE WHEN ITOG.Prac_Pl IS NOT NULL THEN  ITOG.Prac_Pl * ITOG.InSize ELSE 0 END) OVER (PARTITION BY ITOG.CycleReal) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.Prac_Pl IS NOT NULL THEN (ITOG.Prac_Pl * ITOG.InSize) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WTypeComponent != 3  THEN SUM(ITOG.Prac_Pl * ITOG.InSize) OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS Pr " +
                               ", CASE WHEN ITOG.Levelf = 0  THEN SUM(CASE WHEN ITOG.IGA_Pl IS NOT NULL THEN  ITOG.IGA_Pl * ITOG.InSize ELSE 0 END) OVER (PARTITION BY ITOG.CycleReal) " +
                               "WHEN ITOG.Levelf = 2  THEN SUM(CASE WHEN ITOG.IGA_Pl IS NOT NULL THEN (ITOG.IGA_Pl * ITOG.InSize) ELSE 0 END) OVER (PARTITION BY ITOG.CurDisNrec) " +
                               "WHEN ITOG.WTypeComponent != 3  THEN SUM(ITOG.IGA_Pl * ITOG.InSize) OVER (PARTITION BY ITOG.CycleReal, ITOG.LevelComponentForDVS) " +
                               "ELSE 0 END AS IGA " +
                               ", ITOG.AttAll AS AttAll " +
                               "  , ITOG.ReAtt AS ReAtt " +
                               "  , ITOG.AttEx AS AttEx " +
                               "  , ITOG.AttZh AS AttZh " +
                               "  , ITOG.AttKP AS AttKP " +
                               "  , ITOG.Lec1s AS Lec1s " +
                               "  , ITOG.PZs1s AS PZs1s " +
                               "  , ITOG.LRs1s AS LRs1s " +
                               "  , ITOG.SRSs1s AS SRSs1s " +
                               "  , ITOG.KSRs1s AS KSRs1s " +
                               "  , ITOG.PrIGAs1s AS PrIGAs1s " +
                               "  , ITOG.Lec2s AS Lec2s " +
                               "  , ITOG.PZs2s AS PZs2s " +
                               "  , ITOG.LRs2s AS LRs2s " +
                               "  , ITOG.SRSs2s AS SRSs2s " +
                               "  , ITOG.KSRs2s AS KSRs2s " +
                               "  , ITOG.PrIGAs2s AS PrIGAs2s " +
                               "  , ITOG.Lec3s AS Lec3s " +
                               "  , ITOG.PZs3s AS PZs3s " +
                               "  , ITOG.LRs3s AS LRs3s " +
                               "  , ITOG.SRSs3s AS SRSs3s " +
                               "  , ITOG.KSRs3s AS KSRs3s " +
                               "  , ITOG.PrIGAs3s AS PrIGAs3s " +
                               "  , ITOG.Lec4s AS Lec4s " +
                               "  , ITOG.PZs4s AS PZs4s " +
                               "  , ITOG.LRs4s AS LRs4s " +
                               "  , ITOG.SRSs4s AS SRSs4s " +
                               "  , ITOG.KSRs4s AS KSRs4s " +
                               "  , ITOG.PrIGAs4s AS PrIGAs4s " +
                               "  , ITOG.Lec5s AS Lec5s " +
                               "  , ITOG.PZs5s AS PZs5s " +
                               "  , ITOG.LRs5s AS LRs5s " +
                               "  , ITOG.SRSs5s AS SRSs5s " +
                               "  , ITOG.KSRs5s AS KSRs5s " +
                               "  , ITOG.PrIGAs5s AS PrIGAs5s " +
                               "  , ITOG.Lec6s AS Lec6s " +
                               "  , ITOG.PZs6s AS PZs6s " +
                               "  , ITOG.LRs6s AS LRs6s " +
                               "  , ITOG.SRSs6s AS SRSs6s " +
                               "  , ITOG.KSRs6s AS KSRs6s " +
                               "  , ITOG.PrIGAs6s AS PrIGAs6s " +
                               "  , ITOG.Lec7s AS Lec7s " +
                               "  , ITOG.PZs7s AS PZs7s " +
                               "  , ITOG.LRs7s AS LRs7s " +
                               "  , ITOG.SRSs7s AS SRSs7s " +
                               "  , ITOG.KSRs7s AS KSRs7s " +
                               "  , ITOG.PrIGAs7s AS PrIGAs7s " +
                               "  , ITOG.Lec8s AS Lec8s " +
                               "  , ITOG.PZs8s AS PZs8s " +
                               "  , ITOG.LRs8s AS LRs8s " +
                               "  , ITOG.SRSs8s AS SRSs8s " +
                               "  , ITOG.KSRs8s AS KSRs8s " +
                               "  , ITOG.PrIGAs8s AS PrIGAs8s " +
                               "  , ITOG.Lec9s AS Lec9s " +
                               "  , ITOG.PZs9s AS PZs9s " +
                               "  , ITOG.LRs9s AS LRs9s " +
                               "  , ITOG.SRSs9s AS SRSs9s " +
                               "  , ITOG.KSRs9s AS KSRs9s " +
                               "  , ITOG.PrIGAs9s AS PrIGAs9s " +
                               "  , ITOG.Lec10s AS Lec10s " +
                               "  , ITOG.PZs10s AS PZs10s " +
                               "  , ITOG.LRs10s AS LRs10s " +
                               "  , ITOG.SRSs10s AS SRSs10s " +
                               "  , ITOG.KSRs10s AS KSRs10s " +
                               "  , ITOG.PrIGAs10s AS PrIGAs10s " +
                               "  , ITOG.Lec11s AS Lec11s " +
                               "  , ITOG.PZs11s AS PZs11s " +
                               "  , ITOG.LRs11s AS LRs11s " +
                               "  , ITOG.SRSs11s AS SRSs11s " +
                               "  , ITOG.KSRs11s AS KSRs11s " +
                               "  , ITOG.PrIGAs11s AS PrIGAs11s " +
                               "  , ITOG.Lec12s AS Lec12s " +
                               "  , ITOG.PZs12s AS PZs12s " +
                               "  , ITOG.LRs12s AS LRs12s " +
                               "  , ITOG.SRSs12s AS SRSs12s " +
                               "  , ITOG.KSRs12s AS KSRs12s " +
                               "  , ITOG.PrIGAs12s AS PrIGAs12s " +
                               "  , ITOG.Lec13s AS Lec13s " +
                               "  , ITOG.PZs13s AS PZs13s " +
                               "  , ITOG.LRs13s AS LRs13s " +
                               "  , ITOG.SRSs13s AS SRSs13s " +
                               "  , ITOG.KSRs13s AS KSRs13s " +
                               "  , ITOG.PrIGAs13s AS PrIGAs13s " +
                               "  , ITOG.Lec14s AS Lec14s " +
                               "  , ITOG.PZs14s AS PZs14s " +
                               "  , ITOG.LRs14s AS LRs14s " +
                               "  , ITOG.SRSs14s AS SRSs14s " +
                               "  , ITOG.KSRs14s AS KSRs14s " +
                               "  , ITOG.PrIGAs14s AS PrIGAs14s " +
                               "FROM " +
                               "(SELECT " +
                               "* " +
                               "FROM " +
                               "(SELECT DISTINCT " +
                               "ttt.CycleNrec " +
                               "  , ttt.[CYCLE] " +
                               "  , ttt.CycleType " +
                               "  , ttt.CompName " +
                               ", ttt.WTypeComponent " +
                               "  , ttt.WLevelReal " +
                               "  , ttt.CCOMPONENT " +
                               "  , ttt.CDIS " +
                               "  , ttt.LevelComponentForDVS " +
                               "  , ttt.CycleReal " +
                               "  , ttt.ComponentReal " +
                               ", ttt.CurDisNrec " +
                               "  , ttt.Cod \'cod\' " +
                               "  , ttt.Levelf_F$WLEVEL \'Levelf\' " +
                               "  , ttt.Abbr " +
                               "  , ttt.KafAbb " +
                               "  , ttt.Kaf " +
                               ", CASE  WHEN ttt.WLevelReal = 0 THEN \'ц\' " +
                               "WHEN ttt.WLevelReal = 1 THEN \'к\' " +
                               "WHEN ttt.WLevelReal = 2 THEN \'д\' " +
                               "WHEN ttt.WLevelReal = 3 THEN \'д2\' " +
                               "WHEN ttt.WLevelReal = 4 THEN \'д3\' " +
                               "ELSE \'х\' END \'DC_Type\' " +
                               ", CASE  WHEN ttt.WLevelReal = 0 THEN ttt.DisCur " +
                               "WHEN ttt.WLevelReal = 1 THEN ttt.DisCur  " +
                               "WHEN ttt.WTypeComponent = 3 AND ttt.Levelf_F$WLEVEL = 1 THEN ttt.DisCur " +
                               "WHEN ttt.WLevelReal = 3 THEN ttt.DisCur " +
                               "WHEN ttt.WLevelReal = 4 THEN ttt.DisCur " +
                               "WHEN ttt.DisWPROP IN (3, 4) THEN ttt.DisCur " +
                               "ELSE ttt.DisCur END \'DisCur\' " +
                               ", ttt.DisWPROP " +
                               ", CASE WHEN ttt.AttAll IS NULL THEN \'\' " +
                               "ELSE SUBSTRING(ttt.AttAll, 1, LEN(ttt.AttAll)-1) END \'AttAll\' " +
                               ", CASE WHEN ttt.ReAtt IS NULL THEN \'\' " +
                               "ELSE SUBSTRING(ttt.ReAtt, 1, LEN(ttt.ReAtt)-1) END \'ReAtt\' " +
                               ", CASE WHEN ttt.AttEx IS NULL or ttt.AttEx = \'\' THEN \'\' " +
                               "ELSE SUBSTRING(ttt.AttEx, 1, LEN(ttt.AttEx)-1) END \'AttEx\' " +
                               ", CASE WHEN ttt.AttZh IS NULL or ttt.AttZh = \'\' THEN \'\' " +
                               "ELSE SUBSTRING(ttt.AttZh, 1, LEN(ttt.AttZh)-1) END \'AttZh\' " +
                               ", CASE WHEN ttt.AttKP IS NULL or ttt.AttKP = \'\' THEN \'\' " +
                               "ELSE SUBSTRING(ttt.AttKP, 1, LEN(ttt.AttKP)-1) END \'AttKP\' " +
                               ", ttt.InSize " +
                               "  , ttt.Koeff " +
                               "  , ttt.LevelCode " +
                               ", ttt.ZEAll \'ZE_All\'  " +
                               "  , ttt.HourAll \'Hour_All\' " +
                               ", ttt.ZE_Pl  " +
                               "  , ttt.ReZE " +
                               "  , ttt.HourCurPl  " +
                               "  , ttt.ReHour " +
                               "  , ttt.Aud_Pl " +
                               "  , ttt.HOUREXAM " +
                               "  , ttt.SRS_Pl " +
                               "  , ttt.Lec_Pl " +
                               "  , ttt.Pr_Pl " +
                               "  , ttt.Lab_Pl " +
                               "  , ttt.KSR_Pl " +
                               "  , ttt.Prac_Pl " +
                               "  , ttt.IGA_Pl " +
                               ", sum(CASE WHEN ttt.Sem = 1 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec1s\' " +
                               ", sum(CASE WHEN ttt.Sem = 1 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PZs1s\' " +
                               ", sum(CASE WHEN ttt.Sem = 1 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'LRs1s\' " +
                               ", sum(CASE WHEN ttt.Sem = 1 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'SRSs1s\' " +
                               ", sum(CASE WHEN ttt.Sem = 1 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)   \'KSRs1s\' " +
                               ", sum(CASE WHEN ttt.Sem = 1 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'PrIGAs1s\' " +
                               ", sum(CASE WHEN ttt.Sem = 2 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec2s\' " +
                               ", sum(CASE WHEN ttt.Sem = 2 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)   \'PZs2s\' " +
                               ", sum(CASE WHEN ttt.Sem = 2 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs2s\' " +
                               ", sum(CASE WHEN ttt.Sem = 2 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)   \'SRSs2s\' " +
                               ", sum(CASE WHEN ttt.Sem = 2 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs2s\' " +
                               ", sum(CASE WHEN ttt.Sem = 2 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs2s\' " +
                               ", sum(CASE WHEN ttt.Sem = 3 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec3s\' " +
                               ", sum(CASE WHEN ttt.Sem = 3 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs3s\' " +
                               ", sum(CASE WHEN ttt.Sem = 3 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs3s\' " +
                               ", sum(CASE WHEN ttt.Sem = 3 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs3s\' " +
                               ", sum(CASE WHEN ttt.Sem = 3 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs3s\' " +
                               ", sum(CASE WHEN ttt.Sem = 3 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs3s\' " +
                               ", sum(CASE WHEN ttt.Sem = 4 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec4s\' " +
                               ", sum(CASE WHEN ttt.Sem = 4 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs4s\' " +
                               ", sum(CASE WHEN ttt.Sem = 4 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs4s\' " +
                               ", sum(CASE WHEN ttt.Sem = 4 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs4s\' " +
                               ", sum(CASE WHEN ttt.Sem = 4 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs4s\' " +
                               ", sum(CASE WHEN ttt.Sem = 4 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs4s\' " +
                               ", sum(CASE WHEN ttt.Sem = 5 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec5s\' " +
                               ", sum(CASE WHEN ttt.Sem = 5 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs5s\' " +
                               ", sum(CASE WHEN ttt.Sem = 5 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs5s\' " +
                               ", sum(CASE WHEN ttt.Sem = 5 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs5s\' " +
                               ", sum(CASE WHEN ttt.Sem = 5 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs5s\' " +
                               ", sum(CASE WHEN ttt.Sem = 5 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs5s\' " +
                               ", sum(CASE WHEN ttt.Sem = 6 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec6s\' " +
                               ", sum(CASE WHEN ttt.Sem = 6 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs6s\' " +
                               ", sum(CASE WHEN ttt.Sem = 6 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs6s\' " +
                               ", sum(CASE WHEN ttt.Sem = 6 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs6s\' " +
                               ", sum(CASE WHEN ttt.Sem = 6 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs6s\' " +
                               ", sum(CASE WHEN ttt.Sem = 6 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs6s\' " +
                               ", sum(CASE WHEN ttt.Sem = 7 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec7s\' " +
                               ", sum(CASE WHEN ttt.Sem = 7 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs7s\' " +
                               ", sum(CASE WHEN ttt.Sem = 7 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs7s\' " +
                               ", sum(CASE WHEN ttt.Sem = 7 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs7s\' " +
                               ", sum(CASE WHEN ttt.Sem = 7 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs7s\' " +
                               ", sum(CASE WHEN ttt.Sem = 7 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs7s\' " +
                               ", sum(CASE WHEN ttt.Sem = 8 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec8s\' " +
                               ", sum(CASE WHEN ttt.Sem = 8 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs8s\' " +
                               ", sum(CASE WHEN ttt.Sem = 8 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs8s\' " +
                               ", sum(CASE WHEN ttt.Sem = 8 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs8s\' " +
                               ", sum(CASE WHEN ttt.Sem = 8 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs8s\' " +
                               ", sum(CASE WHEN ttt.Sem = 8 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs8s\' " +
                               ", sum(CASE WHEN ttt.Sem = 9 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec9s\' " +
                               ", sum(CASE WHEN ttt.Sem = 9 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs9s\' " +
                               ", sum(CASE WHEN ttt.Sem = 9 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs9s\' " +
                               ", sum(CASE WHEN ttt.Sem = 9 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs9s\' " +
                               ", sum(CASE WHEN ttt.Sem = 9 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs9s\' " +
                               ", sum(CASE WHEN ttt.Sem = 9 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs9s\' " +
                               ", sum(CASE WHEN ttt.Sem = 10 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec10s\' " +
                               ", sum(CASE WHEN ttt.Sem = 10 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs10s\' " +
                               ", sum(CASE WHEN ttt.Sem = 10 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs10s\' " +
                               ", sum(CASE WHEN ttt.Sem = 10 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs10s\' " +
                               ", sum(CASE WHEN ttt.Sem = 10 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs10s\' " +
                               ", sum(CASE WHEN ttt.Sem = 10 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs10s\' " +
                               ", sum(CASE WHEN ttt.Sem = 11 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec11s\' " +
                               ", sum(CASE WHEN ttt.Sem = 11 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs11s\' " +
                               ", sum(CASE WHEN ttt.Sem = 11 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs11s\' " +
                               ", sum(CASE WHEN ttt.Sem = 11 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs11s\' " +
                               ", sum(CASE WHEN ttt.Sem = 11 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs11s\' " +
                               ", sum(CASE WHEN ttt.Sem = 11 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs11s\' " +
                               ", sum(CASE WHEN ttt.Sem = 12 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec12s\' " +
                               ", sum(CASE WHEN ttt.Sem = 12 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs12s\' " +
                               ", sum(CASE WHEN ttt.Sem = 12 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs12s\' " +
                               ", sum(CASE WHEN ttt.Sem = 12 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs12s\' " +
                               ", sum(CASE WHEN ttt.Sem = 12 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs12s\' " +
                               ", sum(CASE WHEN ttt.Sem = 12 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs12s\' " +
                               ", sum(CASE WHEN ttt.Sem = 13 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec13s\' " +
                               ", sum(CASE WHEN ttt.Sem = 13 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs13s\' " +
                               ", sum(CASE WHEN ttt.Sem = 13 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs13s\' " +
                               ", sum(CASE WHEN ttt.Sem = 13 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs13s\' " +
                               ", sum(CASE WHEN ttt.Sem = 13 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs13s\' " +
                               ", sum(CASE WHEN ttt.Sem = 13 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs13s\' " +
                               ", sum(CASE WHEN ttt.Sem = 14 THEN ttt.Lecs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec) \'Lec14s\' " +
                               ", sum(CASE WHEN ttt.Sem = 14 THEN ttt.PZs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'PZs14s\' " +
                               ", sum(CASE WHEN ttt.Sem = 14 THEN ttt.LRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'LRs14s\' " +
                               ", sum(CASE WHEN ttt.Sem = 14 THEN ttt.SRSs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'SRSs14s\' " +
                               ", sum(CASE WHEN ttt.Sem = 14 THEN ttt.KSRs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)    \'KSRs14s\' " +
                               ", sum(CASE WHEN ttt.Sem = 14 THEN ttt.PrIGAs ELSE 0 END) OVER (PARTITION BY ttt.CurDisNrec)  \'PrIGAs14s\' " +
                               "FROM " +
                               "(SELECT DISTINCT " +
                               "curDis.F$NREC \'CurDisNrec\' " +
                               ", CASE WHEN cycleDis.F$NREC IS NOT NULL THEN cycleDis.F$NREC ELSE 0 END \'CycleNrec\' " +
                               ", CASE WHEN (curDis.F$CCOMPONENT = dbo.toComp(0) AND curDis.F$CDIS = dbo.toComp(0)) THEN 1 ELSE 0 END \'CYCLE\' " +
                               ", cycleDis.F$NAME \'CycleType\' " +
                               ", CASE WHEN tuc.F$NAME IS NULL THEN \'\' ELSE tuc.F$NAME END \'CompName\' " +
                               ", CASE WHEN tuc.F$WTYPE IS NULL THEN \'0\' ELSE tuc.F$WTYPE END \'WTypeComponent\' " +
                               ", CASE  WHEN LEN(curDis.F$CODE) >= 12 THEN (LEN(curDis.F$CODE) / 3) - 1 " +
                               "WHEN LEN(curDis.F$CODE) > 9 THEN 3 " +
                               "WHEN disParent1.F$WLEVEL = 2 THEN (curDis.F$WLEVEL + 1) " +
                               "WHEN tuc.F$WTYPE = 3 OR tuc.F$NAME LIKE \'%по выбору%\' THEN (curDis.F$WLEVEL + 1) " +
                               "WHEN curDis.F$WLEVEL = 2 AND tud.F$NAME LIKE \'%по выбору%\' THEN (curdis.F$WLEVEL + 3) " +
                               "ELSE curDis.F$WLEVEL END \'WLevelReal\' " +
                               ", CASE WHEN (curDis.F$CCOMPONENT != dbo.toComp(0) AND curDis.F$CDIS = dbo.toComp(0) AND curDis.F$CCYCLE != dbo.toComp(0)) THEN 1 ELSE 0 END \'CCOMPONENT\'  " + ", CASE WHEN (curDis.F$CCOMPONENT != dbo.toComp(0) AND curDis.F$CDIS != dbo.toComp(0) AND curDis.F$CCYCLE != dbo.toComp(0)) THEN 1 ELSE 0 END \'CDIS\' " +
                               ", CASE WHEN tucs.F$SEMESTER IS NOT NULL THEN tucs.F$SEMESTER ELSE 0 END \'Sem\' " +
                               ", CASE WHEN tuc.F$WTYPE IN (2, 3) THEN 2 WHEN tuc.F$WTYPE IS NOT NULL THEN tuc.F$WTYPE ELSE 0 END \'LevelComponentForDVS\'  " +
                               ", CASE WHEN tud.F$WPROPERTIES IS NOT NULL THEN tud.F$WPROPERTIES ELSE 0 END \'DisWPROP\'  " +
                               ", CASE WHEN curdis.F$NAME LIKE \'Цикл факультативных дисциплин по военной подготовке\' THEN \'Цикл факультативных дисциплин по военной подготовке<sup>2</sup>\' " +
                               "WHEN cycleDis.F$NAME LIKE \'%факультат%\' THEN curdis.F$NAME " +
                               "WHEN curdis.F$WLEVEL = 2 AND curDis.[F$DADDFLD#1#] != 1 THEN curdis.F$NAME + \' (А)\' " +
                               "ELSE curdis.F$NAME END \'DisCur\' " +
                               ", curDis.[F$WADDFLD#1#] \'InSize\' " +
                               ", ROUND(CONVERT(FLOAT, curDis.[F$DADDFLD#1#]), 2) \'Koeff\' " +
                               ", curDis.F$WLEVEL \'Levelf_F$WLEVEL\' " +
                               ", curdis.F$LEVELCODE \'LevelCode\' " +
                               ", curDis.F$CODE \'Cod\' " +
                               ", curDis.[F$SADDFLD#2#] \'Abbr\' " +
                               ", CASE WHEN chair.F$NREC IS NULL THEN \'\' WHEN curDis.F$WLEVEL IN (0, 1) THEN \'\' ELSE   REPLACE( (SUBSTRING(chair.F$LONGNAME, 1, CHARINDEX(\'_\', chair.F$LONGNAME))), \'_\', \'\') END \'Kaf\' " +
                               ", CASE WHEN chair.F$NREC IS NULL THEN \'\' WHEN curDis.F$WLEVEL IN (0, 1) THEN \'\' ELSE chair.F$LONGNAME END \'KafAbb\' " +
                               ", curdis.F$REATTSIZE      \'ReHour\' , curdis.F$REATTCREDSIZE  \'ReZE\' , curDis.F$DCREDITCURPLAN \'ZE_Pl\' " +
                               ", curDis.F$HOURCURPLAN \'HourCurPl\' , curDis.F$HOURLECROOM    \'Aud_Pl\' , curDis.F$HOUREXAM       \'HOUREXAM\' " +
                               ", curDis.F$HOURSTUDOWN    \'SRS_Pl\' " +
                               ", curdis.F$REATTSIZE + curdis.F$HOURCURPLAN \'HourAll\' " +
                               ", curDis.F$REATTCREDSIZE + curDis.F$DCREDITCURPLAN \'ZEAll\' " +
                               ", SUM(tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff * tw.TW_Aud * tw.TWLec) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC) \'Lec_Pl\' " +
                               ", SUM(tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff * tw.TW_Aud * tw.TWLab) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC) \'Lab_Pl\' " +
                               ", SUM(tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff * tw.TW_Aud * tw.TWPZ) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC) \'Pr_Pl\' " +
                               ", SUM(tucd.F$SIZE * tw.WTKoeff * tw.TWKSR) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC) \'KSR_Pl\' " +
                               ", SUM(tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff * tw.TWPract) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC)\'Prac_Pl\' " +
                               ", SUM(tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff * tw.TWIGA) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC)\'IGA_Pl\' " +
                               ", SUM(CASE WHEN tw.TWIGA = 1 OR tw.TWPract = 1 THEN (tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY curdis.F$NREC ORDER BY curDis.F$NREC)\'PrIGA\' " +
                               ", (SELECT " +
                               "CASE WHEN tw.TW_Att = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER) + (tw.TWAbb_1 + \', \') " +
                               "ELSE \'\' END " +
                               "FROM T$U_CURR_DIS tucd1 " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd2 ON tucd2.F$CCURR_DIS = tucd1.F$NREC " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd2.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw             ON tw.TW_ID = tucd2.F$CTYPEWORK " +
                               "WHERE 1=1 " +
                               "AND curDis.F$NREC = tucd1.F$NREC " +
                               "AND tw.TW_Att = 1 " +
                               "AND tucd2.F$WREATT != 1 " +
                               "ORDER BY tucs1.F$SEMESTER " +
                               "FOR XML PATH (\'\')) \'AttAll\' " +
                               ", (SELECT " +
                               "CASE WHEN tw.TW_Att = 1 AND tucd2.F$WREATT = 0 AND tw.TWAttEx = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER) + tw.TWAbb + \', \' " +
                               "WHEN tw.TW_Att = 1 AND tucd2.F$WREATT = 0 AND tw.TWAttIGA = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER)  + tw.TWAbb + \', \' " +
                               "ELSE \'\' END " +
                               "FROM T$U_CURR_DIS tucd1 " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd2 ON tucd2.F$CCURR_DIS = tucd1.F$NREC " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd2.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw             ON tw.TW_ID = tucd2.F$CTYPEWORK " +
                               "WHERE 1=1 " +
                               "AND curDis.F$NREC = tucd1.F$NREC " +
                               "AND tw.TW_Att = 1 " +
                               "AND tucd2.F$WREATT != 1 " +
                               "ORDER BY tucs1.F$SEMESTER " +
                               "FOR XML PATH (\'\')) \'AttEx\' " +
                               ", (SELECT " +
                               "CASE WHEN tw.TW_Att = 1 AND tucd2.F$WREATT = 0 AND tw.TWAttZh = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER) + tw.TWAbb + \', \' " +
                               "WHEN tw.TW_Att = 1 AND tucd2.F$WREATT = 0 AND tw.TWAttDZh = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER) + tw.TWAbb  + \', \' " +
                               "WHEN tw.TW_Att = 1 AND tucd2.F$WREATT = 0 AND tw.TWAttPr = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER) +  tw.TWAbb + \', \' " +
                               "ELSE \'\' END " +
                               "FROM T$U_CURR_DIS tucd1 " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd2 ON tucd2.F$CCURR_DIS = tucd1.F$NREC " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd2.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw             ON tw.TW_ID = tucd2.F$CTYPEWORK " +
                               "WHERE 1=1 " +
                               "AND curDis.F$NREC = tucd1.F$NREC " +
                               "AND tw.TW_Att = 1 " +
                               "AND tucd2.F$WREATT != 1 " +
                               "ORDER BY tucs1.F$SEMESTER " +
                               "FOR XML PATH (\'\')) \'AttZh\' " +
                               ", (SELECT " +
                               "CASE WHEN tw.TW_Att = 1 AND tucd2.F$WREATT = 0 AND tw.TWAttKP = 1 OR tw.TWAttKR = 1 THEN CONVERT(VARCHAR, tucs1.F$SEMESTER) + tw.TWAbb + \', \' " +
                               "ELSE \'\' END " +
                               "FROM T$U_CURR_DIS tucd1 " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd2 ON tucd2.F$CCURR_DIS = tucd1.F$NREC " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd2.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw             ON tw.TW_ID = tucd2.F$CTYPEWORK " +
                               "WHERE 1=1 " +
                               "AND curDis.F$NREC = tucd1.F$NREC " +
                               "AND tw.TW_Att = 1 " +
                               "AND tucd2.F$WREATT != 1 " +
                               "ORDER BY tucs1.F$SEMESTER " +
                               "FOR XML PATH (\'\')) \'AttKP\' " +
                               ", (SELECT " +
                               "CASE WHEN tucd2.F$WREATT = 1 AND tw.TW_Att = 1 THEN (CONVERT(VARCHAR, tucs1.F$SEMESTER) + tw.TWAbb_1 + \', \') " +
                               "WHEN tw.TW_ID IS NULL THEN \'\' " +
                               "else \'\' END " +
                               "FROM T$U_CURR_DIS tucd1 " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd2 ON tucd2.F$CCURR_DIS = tucd1.F$NREC " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd2.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw             ON tw.TW_ID = tucd2.F$CTYPEWORK " +
                               "WHERE 1=1 " +
                               "AND curDis.F$NREC = tucd1.F$NREC " +
                               "AND tucd2.F$WREATT = 1 " +
                               "ORDER BY tucs1.F$SEMESTER " +
                               "FOR XML PATH (\'\')) \'ReAtt\' " +
                               ", CASE " +
                               "WHEN disParent4.F$NREC IS NOT NULL AND disParent4.F$WLEVEL = 0 THEN disParent4.F$NREC " +
                               "WHEN disParent3.F$NREC IS NOT NULL AND disParent3.F$WLEVEL = 0 THEN disParent3.F$NREC " +
                               "WHEN disParent2.F$NREC IS NOT NULL AND disParent2.F$WLEVEL = 0 THEN disParent2.F$NREC " +
                               "WHEN disParent1.F$NREC IS NOT NULL AND disParent1.F$WLEVEL = 0 THEN disParent1.F$NREC " +
                               "WHEN curdis.F$WLEVEL = 0 THEN curdis.F$NREC " +
                               "ELSE 0 END \'CycleReal\' " +
                               ", CASE " +
                               "WHEN disParent4.F$NREC IS NOT NULL AND disParent4.F$WLEVEL = 1 THEN disParent4.F$NREC " +
                               "WHEN disParent3.F$NREC IS NOT NULL AND disParent3.F$WLEVEL = 1 THEN disParent3.F$NREC " +
                               "WHEN disParent2.F$NREC IS NOT NULL AND disParent2.F$WLEVEL = 1 THEN disParent2.F$NREC " +
                               "WHEN disParent1.F$NREC IS NOT NULL AND disParent1.F$WLEVEL = 1 THEN disParent1.F$NREC " +
                               "WHEN curdis.F$WLEVEL = 1 THEN curdis.F$NREC " +
                               "ELSE 0 END \'ComponentReal\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL THEN (tucd.F$SIZE * tw.TW_Aud * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER) \'Auds\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL THEN (tucd.F$SIZE * tw.TWLec * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER) \'Lecs\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL THEN (tucd.F$SIZE * tw.TWPZ * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER) \'PZs\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL THEN (tucd.F$SIZE * tw.TWLab * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER) \'LRs\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL THEN (tucd.F$SIZE * tw.TWSRS * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER) \'SRSs\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL THEN (tucd.F$SIZE * tw.TWKSR * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER)\'KSRs\' " +
                               ", SUM(CASE WHEN tucd.F$SIZE IS NOT NULL AND (tw.TWIGA =1 OR tw.TWPract = 1)  THEN (tucd.F$SIZE * tw.TW_AllWork * tw.WTKoeff) ELSE 0 END) OVER (PARTITION BY tud.F$NREC, tucs.F$SEMESTER)\'PrIGAs\' " +
                               ", (SELECT DISTINCT " +
                               "CASE WHEN tucd2.F$CCURR_DIS IS NULL THEN \'\' " +
                               "WHEN (tw.TW_Att = 1 AND tucd2.F$WREATT = 1) THEN \'(\' + tw.TWAbb + \'*) \' " +
                               "WHEN tw.TW_Att = 1 THEN tw.TWAbb + \', \' " +
                               "ELSE \'\' END " +
                               "FROM T$U_CURR_DIS tucd1 " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd2   ON tucd2.F$CCURR_DIS = tucd1.F$NREC AND tucd2.F$CCURR_DIS IS NOT NULL " +
                               "INNER JOIN T$U_CURR_SEMESTER tucs1    ON tucs1.F$NREC = tucd2.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw                 ON tw.TW_ID = tucd2.F$CTYPEWORK " +
                               "WHERE 1=1 " +
                               "AND curDis.F$NREC = tucd1.F$NREC " +
                               "AND tucd2.F$CCURR_DIS IS NOT NULL " +
                               "AND tucs.F$SEMESTER = tucs1.F$SEMESTER " +
                               "FOR XML PATH (\'\')) \'Atts\' " +
                               "FROM T$U_CURRICULUM cur " +
                               "LEFT JOIN T$U_CURR_DIS curDis       ON curDis.F$CCURR = cur.F$NREC " +
                               "LEFT JOIN T$CATALOGS chair          ON chair.F$NREC = curDis.F$CCHAIR " +
                               "LEFT JOIN T$U_CURR_DIS disParent1 ON disParent1.F$NREC = curDis.F$CPARENT " +
                               "LEFT JOIN T$U_CURR_DIS disParent2 ON disParent2.F$NREC = disParent1.F$CPARENT " +
                               "LEFT JOIN T$U_CURR_DIS disParent3 ON disParent3.F$NREC = disParent2.F$CPARENT " +
                               "LEFT JOIN T$U_CURR_DIS disParent4 ON disParent4.F$NREC = disParent3.F$CPARENT " +
                               "LEFT JOIN T$U_DISCIPLINE tud            ON tud.F$NREC = curDis.F$CDIS " +
                               "LEFT JOIN T$U_CYCLESDIS cycleDis        ON cycleDis.F$NREC = curDis.F$CCYCLE " +
                               "LEFT JOIN T$U_COMPONENTDIS tuc          ON tuc.F$NREC = curDis.F$CCOMPONENT " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd      ON tucd.F$CCURR_DIS = curDis.F$NREC AND tucd.F$CCURR_DIS IS NOT NULL " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs        ON tucs.F$NREC = tucd.F$CSEMESTER " +
                               "LEFT JOIN TYPEWORK tw                   ON tw.TW_ID = tucd.F$CTYPEWORK " +
                               $"WHERE curDis.F$NREC  = {structList.NrecString} " +
                               ") ttt " +
                               ") ttt3 " +
                               ") ITOG " +
                               ") tt "

                    ;


                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    structCurr = new JsonWorkCurrStruct()
                    {
                        CurDisNrec = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("CurDisNrec"))
                            .Value),
                        CycleNrec = !reader.IsDBNull(reader.GetOrdinal("CycleNrec"))
                            ? reader.GetInt32(reader.GetOrdinal("CycleNrec")).ToString()
                            : string.Empty,
                        CYCLE = !reader.IsDBNull(reader.GetOrdinal("CYCLE"))
                            ? reader.GetInt32(reader.GetOrdinal("CYCLE")).ToString()
                            : string.Empty,
                        CycleType = !reader.IsDBNull(reader.GetOrdinal("CycleType"))
                            ? reader.GetString(reader.GetOrdinal("CycleType"))
                            : string.Empty,
                        CompName = !reader.IsDBNull(reader.GetOrdinal("CompName"))
                            ? reader.GetString(reader.GetOrdinal("CompName"))
                            : string.Empty,
                        WTypeComponent = !reader.IsDBNull(reader.GetOrdinal("WTypeComponent"))
                            ? reader.GetInt32(reader.GetOrdinal("WTypeComponent")).ToString()
                            : string.Empty,
                        WLevelReal = !reader.IsDBNull(reader.GetOrdinal("WLevelReal"))
                            ? reader.GetInt32(reader.GetOrdinal("WLevelReal")).ToString()
                            : string.Empty,
                        CCOMPONENT = !reader.IsDBNull(reader.GetOrdinal("CCOMPONENT"))
                            ? reader.GetInt32(reader.GetOrdinal("CCOMPONENT")).ToString()
                            : string.Empty,
                        CDIS = !reader.IsDBNull(reader.GetOrdinal("CDIS"))
                            ? reader.GetInt32(reader.GetOrdinal("CDIS")).ToString()
                            : string.Empty,
                        LevelComponentForDVS = !reader.IsDBNull(reader.GetOrdinal("LevelComponentForDVS"))
                            ? reader.GetInt32(reader.GetOrdinal("LevelComponentForDVS")).ToString()
                            : string.Empty,
                        CycleReal = !reader.IsDBNull(reader.GetOrdinal("CycleReal"))
                            ? reader.GetInt32(reader.GetOrdinal("CycleReal")).ToString()
                            : string.Empty,
                        ComponentReal = !reader.IsDBNull(reader.GetOrdinal("ComponentReal"))
                            ? reader.GetInt32(reader.GetOrdinal("ComponentReal")).ToString()
                            : string.Empty,
                        DC_Type = !reader.IsDBNull(reader.GetOrdinal("DC_Type"))
                            ? reader.GetString(reader.GetOrdinal("DC_Type"))
                            : string.Empty,
                        DisWPROP = !reader.IsDBNull(reader.GetOrdinal("DisWPROP"))
                            ? reader.GetInt32(reader.GetOrdinal("DisWPROP")).ToString()
                            : string.Empty,
                        DisCur = !reader.IsDBNull(reader.GetOrdinal("DisCur"))
                            ? reader.GetString(reader.GetOrdinal("DisCur"))
                            : string.Empty,
                        InSize = !reader.IsDBNull(reader.GetOrdinal("InSize"))
                            ? reader.GetInt32(reader.GetOrdinal("InSize")).ToString()
                            : string.Empty,
                        Koeff = !reader.IsDBNull(reader.GetOrdinal("Koeff"))
                            ? reader.GetDouble(reader.GetOrdinal("Koeff")).ToString()
                            : string.Empty,
                        Levelf = !reader.IsDBNull(reader.GetOrdinal("Levelf"))
                            ? reader.GetInt32(reader.GetOrdinal("Levelf")).ToString()
                            : string.Empty,
                        LevelCode = !reader.IsDBNull(reader.GetOrdinal("LevelCode"))
                            ? reader.GetString(reader.GetOrdinal("LevelCode"))
                            : string.Empty,
                        cod = !reader.IsDBNull(reader.GetOrdinal("cod"))
                            ? reader.GetString(reader.GetOrdinal("cod"))
                            : string.Empty,
                        Abbr = !reader.IsDBNull(reader.GetOrdinal("Abbr"))
                            ? reader.GetString(reader.GetOrdinal("Abbr"))
                            : string.Empty,
                        Kaf = !reader.IsDBNull(reader.GetOrdinal("Kaf"))
                            ? reader.GetString(reader.GetOrdinal("Kaf"))
                            : string.Empty,
                        KafAbb = !reader.IsDBNull(reader.GetOrdinal("KafAbb"))
                            ? reader.GetString(reader.GetOrdinal("KafAbb"))
                            : string.Empty,
                        DC_Type1 = !reader.IsDBNull(reader.GetOrdinal("DC_Type1"))
                            ? reader.GetString(reader.GetOrdinal("DC_Type1"))
                            : string.Empty,
                        AttAll = !reader.IsDBNull(reader.GetOrdinal("AttAll"))
                            ? reader.GetString(reader.GetOrdinal("AttAll"))
                            : string.Empty,
                        ReAtt = !reader.IsDBNull(reader.GetOrdinal("ReAtt"))
                            ? reader.GetString(reader.GetOrdinal("ReAtt"))
                            : string.Empty,
                        AttEx = !reader.IsDBNull(reader.GetOrdinal("AttEx"))
                            ? reader.GetString(reader.GetOrdinal("AttEx"))
                            : string.Empty,
                        AttZh = !reader.IsDBNull(reader.GetOrdinal("AttZh"))
                            ? reader.GetString(reader.GetOrdinal("AttZh"))
                            : string.Empty,
                        AttKP = !reader.IsDBNull(reader.GetOrdinal("AttKP"))
                            ? reader.GetString(reader.GetOrdinal("AttKP"))
                            : string.Empty,
                        ZE_All = !reader.IsDBNull(reader.GetOrdinal("ZE_All"))
                            ? reader.GetDouble(reader.GetOrdinal("ZE_All")).ToString()
                            : string.Empty,
                        Hour_All = !reader.IsDBNull(reader.GetOrdinal("Hour_All"))
                            ? reader.GetDouble(reader.GetOrdinal("Hour_All")).ToString()
                            : string.Empty,
                        ZE_Pl = !reader.IsDBNull(reader.GetOrdinal("ZE_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("ZE_Pl")).ToString()
                            : string.Empty,
                        ReZE = !reader.IsDBNull(reader.GetOrdinal("ReZE"))
                            ? reader.GetDouble(reader.GetOrdinal("ReZE")).ToString()
                            : string.Empty,
                        HourCurPl = !reader.IsDBNull(reader.GetOrdinal("HourCurPl"))
                            ? reader.GetDouble(reader.GetOrdinal("HourCurPl")).ToString()
                            : string.Empty,
                        ReHour = !reader.IsDBNull(reader.GetOrdinal("ReHour"))
                            ? reader.GetInt32(reader.GetOrdinal("ReHour")).ToString()
                            : string.Empty,
                        Aud_Pl = !reader.IsDBNull(reader.GetOrdinal("Aud_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("Aud_Pl")).ToString()
                            : string.Empty,
                        HOUREXAM = !reader.IsDBNull(reader.GetOrdinal("HOUREXAM"))
                            ? reader.GetInt32(reader.GetOrdinal("HOUREXAM")).ToString()
                            : string.Empty,
                        SRS_Pl = !reader.IsDBNull(reader.GetOrdinal("SRS_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("SRS_Pl")).ToString()
                            : string.Empty,
                        Lec_Pl = !reader.IsDBNull(reader.GetOrdinal("Lec_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("Lec_Pl")).ToString()
                            : string.Empty,
                        Pr_Pl = !reader.IsDBNull(reader.GetOrdinal("Pr_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("Pr_Pl")).ToString()
                            : string.Empty,
                        Lab_Pl = !reader.IsDBNull(reader.GetOrdinal("Lab_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("Lab_Pl")).ToString()
                            : string.Empty,
                        KSR_Pl = !reader.IsDBNull(reader.GetOrdinal("KSR_Pl"))
                            ? reader.GetDouble(reader.GetOrdinal("KSR_Pl")).ToString()
                            : string.Empty,
                        Pr = !reader.IsDBNull(reader.GetOrdinal("Pr"))
                            ? reader.GetDouble(reader.GetOrdinal("Pr")).ToString()
                            : string.Empty,
                        IGA = !reader.IsDBNull(reader.GetOrdinal("IGA"))

                            ? reader.GetDouble(reader.GetOrdinal("IGA")).ToString()

                            : string.Empty,

                        Lec1s = !reader.IsDBNull(reader.GetOrdinal("Lec1s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec1s")).ToString()

                            : string.Empty,

                        PZs1s = !reader.IsDBNull(reader.GetOrdinal("PZs1s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs1s")).ToString()

                            : string.Empty,

                        LRs1s = !reader.IsDBNull(reader.GetOrdinal("LRs1s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs1s")).ToString()

                            : string.Empty,

                        SRSs1s = !reader.IsDBNull(reader.GetOrdinal("SRSs1s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs1s")).ToString()

                            : string.Empty,

                        KSRs1s = !reader.IsDBNull(reader.GetOrdinal("KSRs1s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs1s")).ToString()

                            : string.Empty,

                        PrIGAs1s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs1s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs1s")).ToString()

                            : string.Empty,

                        Lec2s = !reader.IsDBNull(reader.GetOrdinal("Lec2s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec2s")).ToString()

                            : string.Empty,

                        PZs2s = !reader.IsDBNull(reader.GetOrdinal("PZs2s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs2s")).ToString()

                            : string.Empty,

                        LRs2s = !reader.IsDBNull(reader.GetOrdinal("LRs2s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs2s")).ToString()

                            : string.Empty,

                        SRSs2s = !reader.IsDBNull(reader.GetOrdinal("SRSs2s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs2s")).ToString()

                            : string.Empty,

                        KSRs2s = !reader.IsDBNull(reader.GetOrdinal("KSRs2s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs2s")).ToString()

                            : string.Empty,

                        PrIGAs2s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs2s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs2s")).ToString()

                            : string.Empty,

                        Lec3s = !reader.IsDBNull(reader.GetOrdinal("Lec3s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec3s")).ToString()

                            : string.Empty,

                        PZs3s = !reader.IsDBNull(reader.GetOrdinal("PZs3s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs3s")).ToString()

                            : string.Empty,

                        LRs3s = !reader.IsDBNull(reader.GetOrdinal("LRs3s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs3s")).ToString()

                            : string.Empty,

                        SRSs3s = !reader.IsDBNull(reader.GetOrdinal("SRSs3s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs3s")).ToString()

                            : string.Empty,

                        KSRs3s = !reader.IsDBNull(reader.GetOrdinal("KSRs3s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs3s")).ToString()

                            : string.Empty,

                        PrIGAs3s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs3s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs3s")).ToString()

                            : string.Empty,

                        Lec4s = !reader.IsDBNull(reader.GetOrdinal("Lec4s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec4s")).ToString()

                            : string.Empty,

                        PZs4s = !reader.IsDBNull(reader.GetOrdinal("PZs4s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs4s")).ToString()

                            : string.Empty,

                        LRs4s = !reader.IsDBNull(reader.GetOrdinal("LRs4s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs4s")).ToString()

                            : string.Empty,

                        SRSs4s = !reader.IsDBNull(reader.GetOrdinal("SRSs4s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs4s")).ToString()

                            : string.Empty,

                        KSRs4s = !reader.IsDBNull(reader.GetOrdinal("KSRs4s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs4s")).ToString()

                            : string.Empty,

                        PrIGAs4s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs4s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs4s")).ToString()

                            : string.Empty,

                        Lec5s = !reader.IsDBNull(reader.GetOrdinal("Lec5s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec5s")).ToString()

                            : string.Empty,

                        PZs5s = !reader.IsDBNull(reader.GetOrdinal("PZs5s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs5s")).ToString()

                            : string.Empty,

                        LRs5s = !reader.IsDBNull(reader.GetOrdinal("LRs5s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs5s")).ToString()

                            : string.Empty,

                        SRSs5s = !reader.IsDBNull(reader.GetOrdinal("SRSs5s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs5s")).ToString()

                            : string.Empty,

                        KSRs5s = !reader.IsDBNull(reader.GetOrdinal("KSRs5s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs5s")).ToString()

                            : string.Empty,

                        PrIGAs5s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs5s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs5s")).ToString()

                            : string.Empty,

                        Lec6s = !reader.IsDBNull(reader.GetOrdinal("Lec6s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec6s")).ToString()

                            : string.Empty,

                        PZs6s = !reader.IsDBNull(reader.GetOrdinal("PZs6s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs6s")).ToString()

                            : string.Empty,

                        LRs6s = !reader.IsDBNull(reader.GetOrdinal("LRs6s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs6s")).ToString()

                            : string.Empty,

                        SRSs6s = !reader.IsDBNull(reader.GetOrdinal("SRSs6s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs6s")).ToString()

                            : string.Empty,

                        KSRs6s = !reader.IsDBNull(reader.GetOrdinal("KSRs6s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs6s")).ToString()

                            : string.Empty,

                        PrIGAs6s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs6s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs6s")).ToString()

                            : string.Empty,

                        Lec7s = !reader.IsDBNull(reader.GetOrdinal("Lec7s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec7s")).ToString()

                            : string.Empty,

                        PZs7s = !reader.IsDBNull(reader.GetOrdinal("PZs7s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs7s")).ToString()

                            : string.Empty,

                        LRs7s = !reader.IsDBNull(reader.GetOrdinal("LRs7s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs7s")).ToString()

                            : string.Empty,

                        SRSs7s = !reader.IsDBNull(reader.GetOrdinal("SRSs7s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs7s")).ToString()

                            : string.Empty,

                        KSRs7s = !reader.IsDBNull(reader.GetOrdinal("KSRs7s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs7s")).ToString()

                            : string.Empty,

                        PrIGAs7s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs7s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs7s")).ToString()

                            : string.Empty,

                        Lec8s = !reader.IsDBNull(reader.GetOrdinal("Lec8s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec8s")).ToString()

                            : string.Empty,

                        PZs8s = !reader.IsDBNull(reader.GetOrdinal("PZs8s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs8s")).ToString()

                            : string.Empty,

                        LRs8s = !reader.IsDBNull(reader.GetOrdinal("LRs8s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs8s")).ToString()

                            : string.Empty,

                        SRSs8s = !reader.IsDBNull(reader.GetOrdinal("SRSs8s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs8s")).ToString()

                            : string.Empty,

                        KSRs8s = !reader.IsDBNull(reader.GetOrdinal("KSRs8s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs8s")).ToString()

                            : string.Empty,

                        PrIGAs8s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs8s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs8s")).ToString()

                            : string.Empty,

                        Lec9s = !reader.IsDBNull(reader.GetOrdinal("Lec9s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec9s")).ToString()

                            : string.Empty,

                        PZs9s = !reader.IsDBNull(reader.GetOrdinal("PZs9s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs9s")).ToString()

                            : string.Empty,

                        LRs9s = !reader.IsDBNull(reader.GetOrdinal("LRs9s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs9s")).ToString()

                            : string.Empty,

                        SRSs9s = !reader.IsDBNull(reader.GetOrdinal("SRSs9s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs9s")).ToString()

                            : string.Empty,

                        KSRs9s = !reader.IsDBNull(reader.GetOrdinal("KSRs9s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs9s")).ToString()

                            : string.Empty,

                        PrIGAs9s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs9s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs9s")).ToString()

                            : string.Empty,

                        Lec10s = !reader.IsDBNull(reader.GetOrdinal("Lec10s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec10s")).ToString()

                            : string.Empty,

                        PZs10s = !reader.IsDBNull(reader.GetOrdinal("PZs10s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs10s")).ToString()

                            : string.Empty,

                        LRs10s = !reader.IsDBNull(reader.GetOrdinal("LRs10s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs10s")).ToString()

                            : string.Empty,

                        SRSs10s = !reader.IsDBNull(reader.GetOrdinal("SRSs10s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs10s")).ToString()

                            : string.Empty,

                        KSRs10s = !reader.IsDBNull(reader.GetOrdinal("KSRs10s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs10s")).ToString()

                            : string.Empty,

                        PrIGAs10s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs10s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs10s")).ToString()

                            : string.Empty,

                        Lec11s = !reader.IsDBNull(reader.GetOrdinal("Lec11s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec11s")).ToString()

                            : string.Empty,

                        PZs11s = !reader.IsDBNull(reader.GetOrdinal("PZs11s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs11s")).ToString()

                            : string.Empty,

                        LRs11s = !reader.IsDBNull(reader.GetOrdinal("LRs11s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs11s")).ToString()

                            : string.Empty,

                        SRSs11s = !reader.IsDBNull(reader.GetOrdinal("SRSs11s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs11s")).ToString()

                            : string.Empty,

                        KSRs11s = !reader.IsDBNull(reader.GetOrdinal("KSRs11s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs11s")).ToString()

                            : string.Empty,

                        PrIGAs11s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs11s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs11s")).ToString()

                            : string.Empty,

                        Lec12s = !reader.IsDBNull(reader.GetOrdinal("Lec12s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec12s")).ToString()

                            : string.Empty,

                        PZs12s = !reader.IsDBNull(reader.GetOrdinal("PZs12s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs12s")).ToString()

                            : string.Empty,

                        LRs12s = !reader.IsDBNull(reader.GetOrdinal("LRs12s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs12s")).ToString()

                            : string.Empty,

                        SRSs12s = !reader.IsDBNull(reader.GetOrdinal("SRSs12s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs12s")).ToString()

                            : string.Empty,

                        KSRs12s = !reader.IsDBNull(reader.GetOrdinal("KSRs12s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs12s")).ToString()

                            : string.Empty,

                        PrIGAs12s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs12s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs12s")).ToString()

                            : string.Empty,

                        Lec13s = !reader.IsDBNull(reader.GetOrdinal("Lec13s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec13s")).ToString()

                            : string.Empty,

                        PZs13s = !reader.IsDBNull(reader.GetOrdinal("PZs13s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs13s")).ToString()

                            : string.Empty,

                        LRs13s = !reader.IsDBNull(reader.GetOrdinal("LRs13s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs13s")).ToString()

                            : string.Empty,

                        SRSs13s = !reader.IsDBNull(reader.GetOrdinal("SRSs13s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs13s")).ToString()

                            : string.Empty,

                        KSRs13s = !reader.IsDBNull(reader.GetOrdinal("KSRs13s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs13s")).ToString()

                            : string.Empty,

                        PrIGAs13s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs13s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs13s")).ToString()

                            : string.Empty,

                        Lec14s = !reader.IsDBNull(reader.GetOrdinal("Lec14s"))

                            ? reader.GetDouble(reader.GetOrdinal("Lec14s")).ToString()

                            : string.Empty,

                        PZs14s = !reader.IsDBNull(reader.GetOrdinal("PZs14s"))

                            ? reader.GetDouble(reader.GetOrdinal("PZs14s")).ToString()

                            : string.Empty,

                        LRs14s = !reader.IsDBNull(reader.GetOrdinal("LRs14s"))

                            ? reader.GetDouble(reader.GetOrdinal("LRs14s")).ToString()

                            : string.Empty,

                        SRSs14s = !reader.IsDBNull(reader.GetOrdinal("SRSs14s"))

                            ? reader.GetDouble(reader.GetOrdinal("SRSs14s")).ToString()

                            : string.Empty,

                        KSRs14s = !reader.IsDBNull(reader.GetOrdinal("KSRs14s"))

                            ? reader.GetDouble(reader.GetOrdinal("KSRs14s")).ToString()

                            : string.Empty,

                        PrIGAs14s = !reader.IsDBNull(reader.GetOrdinal("PrIGAs14s"))

                            ? reader.GetDouble(reader.GetOrdinal("PrIGAs14s")).ToString()

                            : string.Empty,
                    };


                }
            }

            return structCurr;
        }

        public List<JsonEntCat> GetEntCat(List<JsonEntCat> structList)
        {
            var listOrder = new List<JsonEntCat>();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "t.F$NREC as NrecString, " +
                               "dbo.toInt64(t.F$NREC) as NrecInt64, " +
                               "t.F$NAME AS Label " +
                               "FROM dbo.T$KATORG t " +
                               "WHERE t.F$TIPORG like \'Место практики\' " +
                               "ORDER BY t.F$NAME"
                    ;


                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    var oneEnt = new JsonEntCat()
                    {
                        NrecInt64 = !reader.IsDBNull(reader.GetOrdinal("NrecInt64"))
                            ? reader.GetInt64(reader.GetOrdinal("NrecInt64")).ToString()
                            : string.Empty,
                        NrecString =
                            DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("NrecString"))
                                .Value),
                        Label = !reader.IsDBNull(reader.GetOrdinal("Label"))
                            ? reader.GetString(reader.GetOrdinal("Label"))
                            : string.Empty,

                    };

                    listOrder.Add(oneEnt);
                }
            }

            return listOrder;
        }

        public List<ListOneRecordFromRecordBook> GetRecordBookFromDb(Int64 nrec, string site)
        {
            var structList = new List<ListOneRecordFromRecordBook>();
            List<string> curr = new List<string>();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT DISTINCT tuc.F$NREC nrec " +
                               "FROM(SELECT *, SUM(subtable.valuedata) " +
                               "OVER(PARTITION BY 1 ORDER BY subtable.appdate, subtable.nrec ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING) " +
                               "- subtable.valuedata \'tag\' " +
                               "FROM (SELECT listta.F$NREC \'nrec\', listta.F$APPOINTDATE \'appdate\', " +
                               "ts.F$CSTR \'curr\', CASE WHEN ccont.F$TYPEOPER IN(30007, 30044, 30051, 30080, 31007) THEN 0 " +
                               "WHEN ccont.F$TYPEOPER IN(30042, 30004) AND LAG(ts.F$CSTR, 1, 0) OVER(ORDER BY listta.F$APPOINTDATE, " +
                               "listta.F$NREC) = ts.F$CSTR THEN 0 ELSE 1 END \'valuedata\' " +
                               "FROM T$APPOINTMENTS listta " +
                               "LEFT JOIN T$CONTDOC ccont ON ccont.F$NREC = listta.F$CCONT " +
                               "LEFT JOIN T$STAFFSTRUCT ts ON ts.F$NREC = listta.F$STAFFSTR " +
                               "LEFT JOIN T$PERSONS tp ON tp.F$NREC = listta.F$PERSON " +
                               $"WHERE listta.F$PERSON = dbo.toComp({nrec}) " +
                               "AND(SELECT dateappstart.F$APPOINTDATE FROM T$APPOINTMENTS dateappstart WHERE dateappstart.F$NREC = " +
                               "(CASE WHEN tp.F$APPOINTLAST != 0x8000000000000000 THEN F$APPOINTLAST ELSE F$APPOINTCUR END) " +
                               ") >= listta.F$APPOINTDATE) subtable ) subtable1 " +
                               "LEFT JOIN T$U_CURRICULUM tuc ON tuc.F$CPARENT = subtable1.curr OR tuc.F$NREC = subtable1.curr " +
                               "WHERE subtable1.tag = 0";


                var reader = galcontext.ExecuteQuery(queryGal);

                while (reader.Read())
                {
                    if (!reader.IsDBNull(reader.GetOrdinal("nrec")))
                    {
                        curr.Add(DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("nrec"))
                                .Value));
                    }
                }
            }

            if (!curr.Any())
            {
                return structList;
            }

            var currString = string.Join(", ", curr.ToArray());
            string queryGalDop = string.Empty;
            if (site.Equals("1"))
            {
                queryGalDop = "1 ";
            }
            else
            {
                queryGalDop = "(CASE WHEN curDis.F$CCYCLE IN (SELECT F$NREC FROM T$U_CYCLESDIS T$UC WHERE F$NAME like \'%факульт%\') " +
                               "THEN (CASE WHEN tum.F$NREC IS NOT NULL THEN 1 ELSE 0 END) ELSE 1 END) ";
            }
            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "    tp.F$NREC nrecStudent, " +
                               "    tud.F$NREC DisciplineNrecString, " +
                               "    tp.F$FIO Fio, " +
                               "    tud.F$NAME Discipline, " +
                               "    tusg.F$NUMBER GroupNumder, " +
                               "    tucc.F$COURSE Course, " +
                               "    CAST(tucs.F$SEMESTER AS VARCHAR) Semester, " +
                               "    tut.F$NAME TypeOfWork, " +
                               "    CAST(CASE " +
                               "    WHEN CAST(((SELECT ROUND(SUM(CASE WHEN ttut.F$NREC IN (0x8001000000000023) THEN ttudc.F$SIZE * 54 ELSE 0 END), 0) " +
                               "    FROM T$U_CURR_DISCONTENT ttudc INNER JOIN T$U_TYPEWORK ttut ON ttut.F$NREC = ttudc.F$CTYPEWORK " +
                               "    WHERE(ttudc.F$CCURR_DIS = curDis.F$NREC AND ttudc.F$CSEMESTER = tucs.F$NREC)) " +
                               "        + (SELECT ROUND(SUM(CASE WHEN ttut.F$NREC IN " +
                               "                          (0x800100000000003b, 0x800100000000002e, 0x800100000000002d, 0x800100000000002c, " +
                               "                          0x800100000000004a) THEN ttudc.F$SIZE ELSE 0 END), 0) " +
                               " FROM T$U_CURR_DISCONTENT ttudc INNER JOIN T$U_TYPEWORK ttut ON ttut.F$NREC = ttudc.F$CTYPEWORK " +
                               " WHERE(ttudc.F$CCURR_DIS = curDis.F$NREC AND ttudc.F$CSEMESTER = tucs.F$NREC))) AS INTEGER) != 0 " +
                               " THEN CAST(((SELECT ROUND(SUM(CASE WHEN ttut.F$NREC IN (0x8001000000000023) THEN ttudc.F$SIZE * 54 ELSE 0 END), 0) " +
                               "    FROM T$U_CURR_DISCONTENT ttudc INNER JOIN T$U_TYPEWORK ttut ON ttut.F$NREC = ttudc.F$CTYPEWORK " +
                               "    WHERE(ttudc.F$CCURR_DIS = curDis.F$NREC AND ttudc.F$CSEMESTER = tucs.F$NREC)) " +
                               " + (SELECT ROUND(SUM(CASE WHEN ttut.F$NREC IN(0x800100000000003b, 0x800100000000002e, 0x800100000000002d, " +
                               "                                             0x800100000000002c, 0x800100000000004a) THEN ttudc.F$SIZE ELSE 0 END), 0) " +
                               "    FROM T$U_CURR_DISCONTENT ttudc INNER JOIN T$U_TYPEWORK ttut ON ttut.F$NREC = ttudc.F$CTYPEWORK " +
                               "    WHERE(ttudc.F$CCURR_DIS = curDis.F$NREC AND ttudc.F$CSEMESTER = tucs.F$NREC))) AS INTEGER) " +
                               " ELSE(SELECT SUM(ttudc.F$SIZE) FROM T$U_CURR_DISCONTENT ttudc " +
                               "       INNER JOIN T$U_TYPEWORK ttut ON ttut.F$NREC = ttudc.F$CTYPEWORK " +
                               " WHERE(ttudc.F$CCURR_DIS = curDis.F$NREC AND ttudc.F$CSEMESTER = tucs.F$NREC " +
                               "  AND(ttut.F$NREC IN " +
                               "       (0x8000000000000003, 0x8000000000000002, 0x8001000000000004, 0x8000000000000004, 0x800000000000000C, " +
                               "        0x800100000000004a) OR ttut.F$WTYPEMASK & 1 = 1))) " +
                               " END AS VARCHAR) HoursOfPlan " +
                               "    , CASE " +
                               "    WHEN LEN(tul.F$WTYPE) = 1 THEN \'основная ведомость\' " +
                               "    WHEN LEN(tul.F$WTYPE) = 2 THEN \'внеочередная ведомость\' " +
                               "    WHEN LEN(tul.F$WTYPE) = 3 THEN \'экзаменационный лист\' " +
                               "    else \'нет данных\' " +
                               "    END ListType " +
                               "    , CASE when tul.F$NUMDOC is null then \'н/данных\' else tul.F$NUMDOC end Numdoc " +
                               "    , CASE " +
                               "    WHEN tum.F$WENDRES = 0 THEN \'текущ.\' " +
                               "    WHEN tum.F$WENDRES = 1 THEN \'оконч.\' " +
                               "    WHEN tum.F$WENDRES = 2 THEN \'перевод\' " +
                               "    WHEN tum.F$WENDRES = 3 THEN \'переатт.\' " +
                               "    when tum.F$WENDRES is null then \'н/данных\' " +
                               "    END as MarkStatus " +
                               "    , tum.F$WADDFLD#4# Rcw " +
                               "    , (tum.F$WADDFLD#1# + tum.F$WADDFLD#2#) R " +
                               "    , CASE when tc.F$NAME is null then \'н/данных\' " +
                               "    else tc.F$NAME END Mark " +
                               "    , CASE when tc.F$CODE IS NULL then \'долг\' " +
                               "    when tc.F$CODE = 1 then \'аттест.\' " +
                               "    when tc.F$CODE < 3 then \'долг\' " +
                               "    when tc.F$CODE IN (3, 4, 5) then \'аттест.\' " +
                               "    else \'н/данных\' " +
                               "    end AttestationInfo " +
                               "    , dbo.frmAtlDateGer(tul.F$DATEMAKE) AttestationDate " +
                               "    , dbo.frmAtlDateGer(tum.F$DATEMARK) MarkDate " +
                               "    , CASE WHEN examinerMark.F$FIO is not null THEN examinerMark.F$FIO " +
                               "        WHEN examinerList.F$FIO is not null THEN examinerList.F$FIO " +
                               "        ELSE \'н/данных\' " +
                               "        END Examiner " +
                               "    , CASE WHEN tul.F$INDIPLOM = 1 THEN \'1\' ELSE \'0\' END ListInDiplom " +
                               "    , (SELECT CAST(toler.F$WRESULTES AS VARCHAR) FROM T$U_TOLERANCESESSION toler WHERE toler.F$CSTUDENT = tp.F$NREC AND toler.F$WSEMESTER = tucs.F$SEMESTER AND toler.F$CPLAN = cur.F$NREC) Toleran " +
                               "    , CASE WHEN curDis.F$CCYCLE IN (SELECT F$NREC FROM T$U_CYCLESDIS T$UC WHERE F$NAME like \'%факульт%\') THEN \'(Факультатив)\' ELSE \'\' END Facultative " +
                               "    , CASE WHEN tucsd.F$NREC IS NOT NULL THEN \'1\' ELSE \'0\' END DisciplineSelected " +
                               "FROM T$U_STUDENT tus " +
                               "LEFT JOIN T$PERSONS tp ON (tus.F$CPERSONS = tp.F$NREC) " +
                               "INNER JOIN T$APPOINTMENTS ta ON ta.F$NREC = (CASE WHEN tp.F$APPOINTLAST != 0x8000000000000000 THEN F$APPOINTLAST ELSE F$APPOINTCUR END) " +
                               "LEFT JOIN T$SPKAU sk ON tus.F$CFINSOURCENAME = sk.F$NREC " +
                               "LEFT JOIN T$U_STUDGROUP tusg ON ta.F$CCAT1 = tusg.F$NREC " +
                               "LEFT JOIN T$CATALOGS fac ON (ta.F$PRIVPENSION = fac.F$NREC) " +
                               "LEFT JOIN T$STAFFSTRUCT st ON (ta.F$STAFFSTR = st.F$NREC) " +
                               "LEFT JOIN T$U_CURRICULUM cur ON (st.F$CSTR = cur.F$NREC) " +
                               "LEFT JOIN T$CATALOGS chair ON (cur.F$CCHAIR = chair.F$NREC) " +
                               "LEFT JOIN T$CATALOGS sp ON (cur.F$CSPECIALITY = sp.F$NREC) " +
                               "INNER JOIN T$U_CURR_DIS curDis ON (curDis.F$CCURR = cur.F$NREC AND curDis.F$DADDFLD#1# > 0) " +
                               "LEFT JOIN T$U_CURR_DISCONTENT tucd ON (tucd.F$CCURR_DIS = curDis.F$NREC) " +
                               "LEFT JOIN T$U_DISCIPLINE tud ON (curDis.F$CDIS = tud.F$NREC) " +
                               "INNER JOIN T$U_TYPEWORK tut " +
                               "    ON (tucd.F$CTYPEWORK = tut.F$NREC AND tut.F$NREC IN (SELECT tut.F$NREC " +
                               "        FROM T$U_TYPEWORK tut WHERE dbo.DecToBin(tut.F$WTYPEMASK, 6, 12) = 1) " +
                               "    ) " +
                               "LEFT JOIN T$U_CURR_SEMESTER tucs ON (tucd.F$CSEMESTER = tucs.F$NREC) " +
                               "LEFT JOIN T$U_CURR_COURSE tucc ON (tucs.F$CCURR_COURSE = tucc.F$NREC) " +
                               "LEFT JOIN T$U_LIST tul ON (tul.F$NREC = (SELECT TOP 1 tul1.F$NREC FROM T$U_LIST tul1 " +
                               "		INNER JOIN T$U_MARKS tumCheck on tumCheck.F$CLIST = tul1.F$NREC " +
                               "		WHERE tumCheck.F$CPERSONS = tp.F$NREC " +
                               "        AND tul1.F$CDIS = tud.F$NREC " +
                               "		AND ((tul1.F$CTYPEWORK = CASE WHEN tut.F$NREC = 0x800100000000004a THEN " +
                               "            CASE WHEN (SELECT COUNT(*) FROM T$U_CURR_DIS curDis1 LEFT JOIN T$U_CURR_DISCONTENT tucd1 ON tucd1.F$CCURR_DIS = curDis1.F$NREC " +
                               "            WHERE curDis1.F$NREC = curDis.F$NREC AND tucd1.F$CTYPEWORK IN (0x800100000000004b)) > 0 " +
                               "                THEN 0x800100000000004b " +
                               "            WHEN (SELECT COUNT(*) FROM T$U_CURR_DIS curDis1 LEFT JOIN T$U_CURR_DISCONTENT tucd1 ON tucd1.F$CCURR_DIS = curDis1.F$NREC LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd1.F$CSEMESTER " +
                               "            WHERE curDis1.F$NREC = curDis.F$NREC AND tucd1.F$CTYPEWORK IN (0x800100000000005B) AND tucs1.F$SEMESTER = (tucs.F$SEMESTER+1) ) > 0 " +
                               "                THEN 0x800100000000005B " +
                               "            WHEN (SELECT COUNT(*) FROM T$U_CURR_DIS curDis1 LEFT JOIN T$U_CURR_DISCONTENT tucd1 ON tucd1.F$CCURR_DIS = curDis1.F$NREC LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd1.F$CSEMESTER " +
                               "            WHERE curDis1.F$NREC = curDis.F$NREC AND tucd1.F$CTYPEWORK IN (0x800100000000005C) AND tucs1.F$SEMESTER = tucs.F$SEMESTER) > 0 " +
                               "                THEN 0x800100000000005C " +
                               "            ELSE tut.F$NREC END " +
                               "		ELSE tut.F$NREC END) " +
                               "        AND ((tul1.F$WSEMESTR = CASE WHEN tut.F$NREC = 0x800100000000004a " +
                               "            THEN (SELECT TOP 1 tucs1.F$SEMESTER FROM T$U_CURR_DIS curDis1 " +
                               "            LEFT JOIN T$U_CURR_DISCONTENT tucd1 ON tucd1.F$CCURR_DIS = curDis1.F$NREC " +
                               "            LEFT JOIN T$U_CURR_SEMESTER tucs1 ON tucs1.F$NREC = tucd1.F$CSEMESTER " +
                               "            WHERE curDis1.F$NREC = curDis.F$NREC " +
                               "            AND (tucd1.F$CTYPEWORK = 0x800100000000004b AND tucs1.F$SEMESTER >= tucs.F$SEMESTER " +
                               "            OR tucd1.F$CTYPEWORK = 0x800100000000005B AND tucs1.F$SEMESTER > tucs.F$SEMESTER " +
                               "            OR tucd1.F$CTYPEWORK = 0x800100000000005C AND tucs1.F$SEMESTER = tucs.F$SEMESTER) " +
                               "            ORDER BY tucs1.F$SEMESTER DESC) " +
                               "        ELSE tucs.F$SEMESTER END) " +
                               $"        AND tul1.F$CCUR IN ({currString}) " +
                               "        AND tumCheck.F$WENDRES IN (1,2,3)) " +
                               "        OR (tul1.F$CCUR IN ( cur.F$NREC) AND tumCheck.F$WENDRES IN (3) AND tucd.F$WREATT = 1)) " +
                               "                              ORDER BY tumCheck.F$DATEMARK DESC, " +
                               "                              CASE WHEN tul1.F$WTYPEDIFFER = 0 THEN tumCheck.F$WMARK END DESC " +
                               "                              ,CASE WHEN tul1.F$WTYPEDIFFER = 1 THEN tumCheck.F$WMARK END ASC)) " +
                               "LEFT JOIN T$U_MARKS tum ON (tul.F$NREC = tum.F$CLIST " +
                               "	AND tum.F$CPERSONS = tp.F$NREC " +
                               "	AND tum.F$WENDRES IN (1, 2, 3)) " +
                               "LEFT JOIN T$CATALOGS tc ON (tum.F$CMARK = tc.F$NREC) " +
                               "LEFT JOIN T$PERSONS examinerMark ON examinerMark.F$NREC = tum.F$CPEREXAM " +
                               "LEFT JOIN T$PERSONS examinerList ON examinerList.F$NREC = tul.F$CEXAMINER " +
                               "LEFT JOIN T$U_CURR_STUD_DIS tucsd ON tucsd.F$NREC = (SELECT TOP 1 tucsd1.F$NREC FROM T$U_CURR_STUD_DIS tucsd1 " +
                               "    LEFT JOIN T$U_CURR_DIS tucsdCurDis ON tucsdCurDis.F$NREC = tucsd1.F$CCURR_DIS " +
                               "    LEFT JOIN T$U_CURRICULUM tuc11 ON tuc11.F$NREC = tucsd1.F$CCURR " +
                               "    WHERE (tuc11.F$NREC = cur.F$NREC OR tuc11.F$CPARENT = cur.F$NREC) " +
                               "    AND tucsd1.F$CPERSONS = tus.F$CPERSONS " +
                               "    AND tucsdCurDis.F$CDIS = tud.F$NREC) " +
                               "WHERE 1=1 " +
                               $"    AND tp.F$NREC = dbo.toComp ({nrec}) " +
                               "    AND ((tucsd.F$NREC IS NOT NULL AND curDis.F$DADDFLD#1# >= 0 AND curDis.F$DADDFLD#1# <= 1) " +
                               "        OR (tucsd.F$NREC IS NULL AND curDis.F$DADDFLD#1# IN (0,1))) " +
                               "    AND 1= " + queryGalDop +
                               "ORDER BY tp.F$FIO, tucs.F$SEMESTER, tucc.F$COURSE, tut.F$NAME"
                    ;


                var reader = galcontext.ExecuteQuery(queryGal);

                while (reader.Read())
                {
                    var oneEnt = new ListOneRecordFromRecordBook()
                    {
                        NrecString = DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("nrecStudent"))
                            .Value),
                        DisciplineNrecString = !reader.IsDBNull(reader.GetOrdinal("DisciplineNrecString"))
                            ? DataOperation.Instance.ByteToString(reader.GetSqlBinary(reader.GetOrdinal("DisciplineNrecString"))
                                .Value)
                        : "0x8000000000000000",
                        Fio = !reader.IsDBNull(reader.GetOrdinal("Fio"))
                            ? reader.GetString(reader.GetOrdinal("Fio"))
                            : string.Empty,
                        Discipline = !reader.IsDBNull(reader.GetOrdinal("Discipline"))
                            ? reader.GetString(reader.GetOrdinal("Discipline"))
                            : string.Empty,
                        Examiner = !reader.IsDBNull(reader.GetOrdinal("Examiner"))
                            ? reader.GetString(reader.GetOrdinal("Examiner"))
                            : string.Empty,
                        Course = !reader.IsDBNull(reader.GetOrdinal("Course"))
                            ? reader.GetInt32(reader.GetOrdinal("Course")).ToString()
                            : string.Empty,
                        AttestationDate = !reader.IsDBNull(reader.GetOrdinal("AttestationDate"))
                            ? reader.GetString(reader.GetOrdinal("AttestationDate"))
                            : string.Empty,
                        AttestationInfo = !reader.IsDBNull(reader.GetOrdinal("AttestationInfo"))
                            ? reader.GetString(reader.GetOrdinal("AttestationInfo"))
                            : string.Empty,
                        Facultative = !reader.IsDBNull(reader.GetOrdinal("Facultative"))
                            ? reader.GetString(reader.GetOrdinal("Facultative"))
                            : string.Empty,
                        GroupNumder = !reader.IsDBNull(reader.GetOrdinal("GroupNumder"))
                            ? reader.GetString(reader.GetOrdinal("GroupNumder"))
                            : string.Empty,
                        HoursOfPlan = !reader.IsDBNull(reader.GetOrdinal("HoursOfPlan"))
                            ? reader.GetString(reader.GetOrdinal("HoursOfPlan"))
                            : string.Empty,
                        ListType = !reader.IsDBNull(reader.GetOrdinal("ListType"))
                            ? reader.GetString(reader.GetOrdinal("ListType"))
                            : string.Empty,
                        Mark = !reader.IsDBNull(reader.GetOrdinal("Mark"))
                            ? reader.GetString(reader.GetOrdinal("Mark"))
                            : string.Empty,
                        MarkDate = !reader.IsDBNull(reader.GetOrdinal("MarkDate"))
                            ? reader.GetString(reader.GetOrdinal("MarkDate"))
                            : string.Empty,
                        MarkStatus = !reader.IsDBNull(reader.GetOrdinal("MarkStatus"))
                            ? reader.GetString(reader.GetOrdinal("MarkStatus"))
                            : string.Empty,
                        Numdoc = !reader.IsDBNull(reader.GetOrdinal("Numdoc"))
                            ? reader.GetString(reader.GetOrdinal("Numdoc"))
                            : string.Empty,
                        R = !reader.IsDBNull(reader.GetOrdinal("R"))
                            ? reader.GetInt32(reader.GetOrdinal("R")).ToString()
                            : string.Empty,
                        Rcw = !reader.IsDBNull(reader.GetOrdinal("Rcw"))
                            ? reader.GetInt32(reader.GetOrdinal("Rcw")).ToString()
                            : string.Empty,
                        Semester = !reader.IsDBNull(reader.GetOrdinal("Semester"))
                            ? reader.GetString(reader.GetOrdinal("Semester")).ToString()
                            : string.Empty,
                        Toleran = !reader.IsDBNull(reader.GetOrdinal("Toleran"))
                            ? reader.GetString(reader.GetOrdinal("Toleran")).ToString()
                            : string.Empty,
                        ListInDiplom = !reader.IsDBNull(reader.GetOrdinal("ListInDiplom"))
                            ? reader.GetString(reader.GetOrdinal("ListInDiplom"))
                            : string.Empty,
                        TypeOfWork = !reader.IsDBNull(reader.GetOrdinal("TypeOfWork"))
                            ? reader.GetString(reader.GetOrdinal("TypeOfWork"))
                            : string.Empty,
                        DisciplineSelected = !reader.IsDBNull(reader.GetOrdinal("DisciplineSelected"))
                            ? reader.GetString(reader.GetOrdinal("DisciplineSelected"))
                            : string.Empty,
                    };
                    oneEnt.NrecInt64 = nrec;
                    structList.Add(oneEnt);

                }
            }

            return structList;
        }

        public List<ListWorkCurrDisciplineType> GetGetWorkCurrDisciplineTypeFromDb(string actionDataNrecOneRecord)
        {
            var listOrder = new List<ListWorkCurrDisciplineType>();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "tucom.F$NAME comp, " +
                               "cycleDis.F$NAME block " +
                               "FROM T$U_CURRICULUM tuc " +
                               "LEFT JOIN T$U_CURR_DIS tucd ON tucd.F$CCURR = tuc.F$NREC " +
                               "LEFT JOIN T$U_COMPONENTDIS tucom ON tucom.F$NREC = tucd.F$CCOMPONENT " +
                               "LEFT JOIN T$U_CYCLESDIS cycleDis ON cycleDis.F$NREC = tucd.F$CCYCLE " +
                               $"WHERE tucd.F$NREC = {actionDataNrecOneRecord} "
                    ;


                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    var oneEnt = new ListWorkCurrDisciplineType()
                    {
                        comp = !reader.IsDBNull(reader.GetOrdinal("comp"))
                            ? reader.GetString(reader.GetOrdinal("comp"))
                            : string.Empty,
                        block = !reader.IsDBNull(reader.GetOrdinal("block"))
                            ? reader.GetString(reader.GetOrdinal("block"))
                            : string.Empty,

                    };

                    listOrder.Add(oneEnt);
                }
            }

            return listOrder;
        }

        public bool UpdateRecordBookIntoDb(JsonStructList structList)
        {
            var result = false;

            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            foreach (var studentList in structList.Student)
                            {
                                var res = context.Database.ExecuteSqlCommand(
                                    "INSERT INTO dbo.T$DOPINFO (F$SFLD#1#, F$CDOPTBL,F$CPERSON)"
                                    + "VALUES (@clist, @cmark, @cpersons)",
                                    new SqlParameter("@clist", studentList.RecordBookNumber),
                                    new SqlParameter("@cmark", DataOperation.Instance.GetRecordBookNrecByte),
                                    new SqlParameter("@cpersons", studentList.StudPersonNrec)
                                );

                                Logger.Log.Debug($"Обновили записей по зачетной книжке {res}");


                                context.SaveChanges();
                            }

                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Error($"При попытке обновления зачетной книжки произошла ошибка. Ошибка {e}");
                            transaction.Rollback();
                        }


                    }
                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении запрос для обновления зачетной книжки. Ошибка {e}");
            }

            return result;
        }

        /// <summary>
        /// Получает информацию по прохождению практики
        /// </summary>
        /// <param name="listNrec">Nrec ведомости</param>
        /// <returns>Список инфы для заполнения ведомости по каждому студенту</returns>
        public List<JsonPracticeList> GetPracticeList(string actionDataNrecOneRecord)
        {
            var listOrder = new List<JsonPracticeList>();

            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "pract.F$NREC as Nrec, " +
                               "dbo.toInt64(pract.F$NREC) as NrecInt64, " +
                               "t.F$NAME AS Label, " +
                               "dbo.frmAtlDateGer(F$SBEGIN) as BeginDate, " +
                               "dbo.frmAtlDateGer(F$SEND) as EndDate, " +
                               "pract.F$CPERSON as PersonNrec, " +
                               "pract.F$CEXAMINER as ExaminerNrec, " +
                               "pract.F$NAMEDIS as Discipline, " +
                               "pract.F$WYEARED as Yeared, " +
                               "pract.F$CLIST as  ListNrec, " +
                               "pract.F$CCOMPANY as CompanyNrec " +
                               "FROM dbo.T$UP_REGISTER_PRACTICES pract " +
                               "LEFT JOIN dbo.T$KATORG t on pract.F$CCOMPANY = t.F$NREC " +
                               $"WHERE pract.F$CLIST = {actionDataNrecOneRecord}";


                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    var oneRecord = new JsonPracticeList()
                    {
                        Nrec = reader.GetSqlBinary(reader.GetOrdinal("Nrec")).Value,
                        NrecInt64 = !reader.IsDBNull(reader.GetOrdinal("NrecInt64"))
                            ? reader.GetInt64(reader.GetOrdinal("NrecInt64"))
                            : 0,
                        NrecString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("Nrec")).Value),
                        Label = !reader.IsDBNull(reader.GetOrdinal("Label"))
                            ? reader.GetString(reader.GetOrdinal("Label"))
                            : string.Empty,
                        BeginDate = !reader.IsDBNull(reader.GetOrdinal("BeginDate"))
                            ? reader.GetString(reader.GetOrdinal("BeginDate"))
                            : string.Empty,
                        EndDate = !reader.IsDBNull(reader.GetOrdinal("EndDate"))
                            ? reader.GetString(reader.GetOrdinal("EndDate"))
                            : string.Empty,
                        PersonNrec = reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value,
                        PersonNrecString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("PersonNrec")).Value),
                        ListNrec = reader.GetSqlBinary(reader.GetOrdinal("ListNrec")).Value,
                        ListNrecString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("ListNrec")).Value),
                        ExaminerNrec = reader.GetSqlBinary(reader.GetOrdinal("ExaminerNrec")).Value,
                        ExaminerNrecString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("ExaminerNrec")).Value),
                        Discipline = !reader.IsDBNull(reader.GetOrdinal("Discipline"))
                            ? reader.GetString(reader.GetOrdinal("Discipline"))
                            : string.Empty,
                        Yeared = !reader.IsDBNull(reader.GetOrdinal("Yeared"))
                            ? reader.GetInt32(reader.GetOrdinal("Yeared"))
                            : 0,
                        CompanyNrec = reader.GetSqlBinary(reader.GetOrdinal("CompanyNrec")).Value,
                        CompanyNrecString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("CompanyNrec")).Value)
                    };
                    listOrder.Add(oneRecord);
                }
            }

            return listOrder;
        }

        /// <summary>
        /// Данный метод выполняет обновление или вставку записи в таблицу T$UP_REGISTER_PRACTICES
        /// </summary>
        /// <param name="structList"></param>
        /// <returns></returns>
        public bool UpdatePracticeList(JsonPracticeList structList)
        {
            var result = false;
            var curDate = 0;
            var curTime = 0;
            byte[] practNrec = null;
            byte[] personNrec = null;
            byte[] CompanyNrec = null;
            var Yeared = 0;
            var CompanyNrecString = "";
            var personNrecString = "";
            using (var galcontext = new GalDbContext())
            {
                var queryGal =
                    "SELECT dbo.ToAtlDate(CONVERT(date, GETDATE())) as curDate, " +
                    "dbo.ToAtlDate(CONVERT(time, GETDATE())) as curTime ";
                var reader = galcontext.ExecuteQuery(queryGal);
                if (reader.Read())
                {
                    curDate = reader.GetInt32(reader.GetOrdinal("curDate"));
                    curTime = reader.GetInt32(reader.GetOrdinal("curTime"));
                }
                // Получаем T$PERSONS.F$NREC и T$U_LIST.F$WYEARED
                var regist2 = galcontext.ExecuteQuery(
                                "SELECT tum.F$CPERSONS as nrec, " +
                                "tul.F$WYEARED as yeared " +
                                "FROM dbo.T$U_LIST tul " +
                                "JOIN dbo.T$U_MARKS tum ON tum.F$CLIST = tul.F$NREC " +
                                $"WHERE tul.F$NREC = {structList.ListNrecString} " +
                                $"AND tum.F$NREC = {structList.PersonNrecString}");
                //Logger.Log.Debug($"Вывод результатов запроса  tul.F$NREC = {structList.ListNrecString}, tum.F$NREC = {structList.PersonNrecString}, {structList.ToString()}");
                if (regist2.Read())
                {
                    personNrec = regist2.GetSqlBinary(regist2.GetOrdinal("nrec")).Value;
                    personNrecString = DataOperation.Instance.ByteToString(
                            regist2.GetSqlBinary(regist2.GetOrdinal("nrec")).Value);
                    Yeared = regist2.GetInt32(regist2.GetOrdinal("yeared"));
                }
                // Получаем F$NREC из T$UP_REGISTER_PRACTICES для update, если она существует
                var regist = galcontext.ExecuteQuery(
                                "SELECT turp.F$NREC as nrec " +
                                "FROM dbo.T$UP_REGISTER_PRACTICES turp " +
                                $"WHERE turp.F$CPERSON= {personNrecString} " +
                                $"AND turp.F$CLIST= {structList.ListNrecString}");
                if (regist.Read())
                {
                    practNrec = regist.GetSqlBinary(regist.GetOrdinal("nrec")).Value;
                }
                //Получаем F$NREC места прохождения практики
                var comp = galcontext.ExecuteQuery(
                                "SELECT TOP 1 k.F$NREC as nrec " +
                                "FROM dbo.T$KATORG k " +
                                "WHERE F$TIPORG LIKE \'Место практики\' " +
                                $"AND k.F$NAME LIKE \'{structList.Label}\'");
                if (comp.Read())
                {
                    CompanyNrec = comp.GetSqlBinary(comp.GetOrdinal("nrec")).Value;
                    CompanyNrecString = DataOperation.Instance.ByteToString(
                            comp.GetSqlBinary(comp.GetOrdinal("nrec")).Value);
                }
            }
            try
            {
                using (var context = new OMGTU810Entities())
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            if (practNrec is null)
                            {
                                var res2 = context.Database.ExecuteSqlCommand(
                                "INSERT INTO dbo.T$UP_REGISTER_PRACTICES " +
                                "(F$CPERSON, F$CCOMPANY, F$CEXAMINER, F$CLIST, F$NAMEDIS, F$WYEARED, F$DATECHANGE, F$TIMECHANGE, F$CONTRACT, F$CONTRACTDATE, F$SBEGIN, F$SEND) " +
                                "VALUES( @person, @company, @examiner, @list, @discipline, @yeared, @datechange, @timechange, 0, 0, dbo.ToAtlDate(@begin), dbo.ToAtlDate(@end)) ",
                                new SqlParameter("@person", personNrec),
                                new SqlParameter("@company", CompanyNrec),
                                new SqlParameter("@examiner", structList.ExaminerNrec),
                                new SqlParameter("@list", structList.ListNrec),
                                new SqlParameter("@discipline", structList.Discipline),
                                new SqlParameter("@yeared", Yeared),
                                new SqlParameter("@datechange", curDate),
                                new SqlParameter("@timechange", curTime),
                                new SqlParameter("@begin", structList.BeginDate),
                                new SqlParameter("@end", structList.EndDate));
                            }
                            else
                            {
                                var res2 = context.Database.ExecuteSqlCommand(
                                "UPDATE dbo.T$UP_REGISTER_PRACTICES SET " +
                                "F$CCOMPANY = @company, F$CEXAMINER = @examiner, " +
                                "F$DATECHANGE = @datechange, F$TIMECHANGE = @timechange, " +
                                "F$SBEGIN = dbo.ToAtlDate(@begin), F$SEND = dbo.ToAtlDate(@end) " +
                                "WHERE F$NREC=@nrec ",
                                new SqlParameter("@company", CompanyNrec),
                                new SqlParameter("@examiner", structList.ExaminerNrec),
                                new SqlParameter("@datechange", curDate),
                                new SqlParameter("@timechange", curTime),
                                new SqlParameter("@begin", structList.BeginDate),
                                new SqlParameter("@end", structList.EndDate),
                                new SqlParameter("@nrec", practNrec));
                            }
                            transaction.Commit();
                            result = true;
                        }
                        catch (Exception e)
                        {
                            Logger.Log.Debug($"Ошибка при обновлении информации по практике. Ошибка {e}");
                            transaction.Rollback();
                            result = false;
                        }

                    }

                }
            }
            catch (Exception e)
            {
                Logger.Log.Debug($"Ошибка при выполнении обновления информации по практике. Ошибка {e}");
            }

            return result;
        }


        /// <summary>
        /// Данный метод получает nrec и ФИО студентов группы по названию
        /// </summary>
        public List<JsonGroupStudentsList> GetGroupStudentsList(string groupName)
        {
            Dictionary<string, string> Student = new Dictionary<string, string>();

            using (var context = new studbaseEntities())
            {
                var listOrder = new List<JsonGroupStudentsList>();
            }
            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "st.F$CPERSONS as Nrec, " +
                               "st.F$FIO as Fio " +
                               "FROM dbo.T$U_STUDGROUP tus " +
                               "LEFT JOIN T$U_STUDENT st ON st.F$CSTGR = tus.F$NREC " +
                               $"WHERE tus.F$NAME like \'{groupName}\'";

                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    string Nrec = DataOperation.Instance.ByteToString(
                      reader.GetSqlBinary(reader.GetOrdinal("Nrec")).Value);
                    string Fio = !reader.IsDBNull(reader.GetOrdinal("Fio"))
                        ? reader.GetString(reader.GetOrdinal("Fio"))
                        : string.Empty;
                    Student.Add(Nrec, Fio);
                };
            }
            return DBAdapterOperationPriem.GetGroupStudentsList(Student);
        }
        /// <summary>
        /// Данный метод получает информацию о всех предприятиях
        /// </summary>
        public List<JsonEnterprises> GetEnterpriseList()
        {
            var enterprises = new List<JsonEnterprises>();
            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT " +
                               "tk.F$NREC as Nrec, " +
                               "tk.F$NAME as Label, " +
                               "tk.F$TEL as Tel, " +
                               "tk.F$EMAIL as Mail, " +
                               "tk.F$SHORTNAME as Short, " +
                               "tk.F$SJURIDICALID as ID, " +
                               "tk.F$CJURIDICALADDR as NrecAddress, " +
                               "CASE  WHEN ter.F$WTYPE = 1 THEN ter.F$SNAME " +
                               "WHEN ter1.F$WTYPE = 1 THEN ter1.F$SNAME + \', \' + ter.F$SNAME " +
                               "WHEN ter2.F$WTYPE = 1 THEN ter2.F$SNAME + \', \' + ter1.F$SNAME + \', \' + ter.F$SNAME " +
                               "WHEN ter3.F$WTYPE = 1 THEN ter3.F$SNAME + \', \' + ter2.F$SNAME + \', \' + ter1.F$SNAME + \', \' + ter.F$SNAME " +
                               "WHEN ter4.F$WTYPE = 1 THEN ter4.F$SNAME + \', \' + ter3.F$SNAME + \', \' + ter2.F$SNAME + \', \' + ter1.F$SNAME + \', \' + ter.F$SNAME " +
                               "ELSE \'\' END as Address " +
                               "FROM T$KATORG tk " +
                               "LEFT JOIN T$ADDRESSN addr on addr.F$NREC = tk.F$CJURIDICALADDR " +
                               "LEFT JOIN T$STERR ter on ter.F$NREC = addr.F$CSTERR " +
                               "LEFT JOIN T$STERR ter1 on ter1.F$NREC = ter.F$CPARENT " +
                               "LEFT JOIN T$STERR ter2 on ter2.F$NREC = ter1.F$CPARENT " +
                               "LEFT JOIN T$STERR ter3 on ter3.F$NREC = ter2.F$CPARENT " +
                               "LEFT JOIN T$STERR ter4 on ter4.F$NREC = ter3.F$CPARENT " +
                               "LEFT JOIN T$STERR ter5 on ter5.F$NREC = ter4.F$CPARENT " +
                               "WHERE tk.F$TIPORG LIKE 'Место практики' ORDER BY tk.F$NREC DESC";
                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    JsonEnterprises oneRec = new JsonEnterprises()
                    {
                        NrecString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("Nrec")).Value),
                        Label = reader.GetString(reader.GetOrdinal("Label")),
                        Address = reader.GetString(reader.GetOrdinal("Address")),
                        Telephon = reader.GetString(reader.GetOrdinal("Tel")),
                        Mail = reader.GetString(reader.GetOrdinal("Mail")),
                        ID = reader.GetString(reader.GetOrdinal("ID")),
                        ShortName = reader.GetString(reader.GetOrdinal("Short")),
                        NrecAddressString = DataOperation.Instance.ByteToString(
                            reader.GetSqlBinary(reader.GetOrdinal("NrecAddress")).Value)
                    };
                    enterprises.Add(oneRec);
                };
            }
            return enterprises;
        }

        /// <summary>
        /// Данный метод получает информацию о факультете и длительности обучения
        /// </summary>
        public List<JsonCurriculumInfo> GetCurriculumInfoForHostel(string actionDataNrecOneRecord)
        {
            var curInfos = new List<JsonCurriculumInfo>();
            using (var galcontext = new GalDbContext())
            {
                var queryGal = "SELECT distinct dbo.frmAtlDate(cur.F$DATEEND) as 'End' " +
                  ", dbo.frmAtlDate(cur.F$DATEAPP) as 'Begin' " +
                  ", fac.F$NAME " +
                  ", spec.F$NAME speciality" +
                  ", fac.F$NREC, " +
                "spec.F$NREC  " +
                /*"CASE WHEN fac.F$NREC in (0x8000000000001686, 0x8000000000001687) " +
                    "THEN(SELECT TOP 1 edu_fac.F$NAME FROM T$CATALOGS spec2 " +
                        "INNER JOIN T$U_CURRICULUM cur2 ON cur2.F$CSPECIALITY = spec2.F$NREC " +
                        "LEFT JOIN T$CATALOGS edu_fac ON edu_fac.F$NREC = cur2.F$CFACULTY " +
                        "where spec2.F$NREC = spec.F$NREC AND edu_fac.F$NREC NOT IN(0x8000000000001686, 0x8000000000001687) " +
                    "AND edu_fac.F$SDOPINF like 'Ф' " +
                             " ORDER BY edu_fac.F$NREC asc) " +
                "ELSE fac.F$NAME " +
                "END as edu_fac " +*/
                "FROM[OMGTU910].[dbo].[T$U_STUDENT][tus] " +
                         "LEFT JOIN[OMGTU910].[dbo].[T$PERSONS][tp] ON[tus].[F$CPERSONS] = [tp].[F$NREC] " +
                         "INNER JOIN[OMGTU910].[dbo].[T$APPOINTMENTS][ta] ON ta.F$PERSON = tp.F$NREC " +
                        " LEFT JOIN[OMGTU910].[dbo].[T$U_STUDGROUP][tusg] ON[ta].[F$CCAT1] = [tusg].[F$NREC] " +
                        " LEFT JOIN[OMGTU910].[dbo].[T$CATALOGS][fac] ON(ta.F$PRIVPENSION = fac.F$NREC) " +
                        " LEFT JOIN[OMGTU910].[dbo].[T$STAFFSTRUCT][st] ON(ta.F$STAFFSTR = st.F$NREC) " +
                        " LEFT JOIN[OMGTU910].[dbo].[T$U_CURRICULUM][cur] ON(st.F$CSTR = cur.F$NREC) " +
                        " LEFT JOIN[OMGTU910].[dbo].[T$CATALOGS][spec] ON(cur.F$CSPECIALITY = spec.F$NREC) " +
                $"WHERE tus.F$NREC = {actionDataNrecOneRecord}";

                var reader = galcontext.ExecuteQuery(queryGal);
                while (reader.Read())
                {
                    JsonCurriculumInfo oneRec = new JsonCurriculumInfo()
                    {
                        //Faculty = reader.GetString(reader.GetOrdinal("edu_fac")),
                        Speciality = reader.GetString(reader.GetOrdinal("speciality")),
                        EndDate = reader.GetString(reader.GetOrdinal("End")),
                        BeginDate = reader.GetString(reader.GetOrdinal("Begin"))
                    };
                    curInfos.Add(oneRec);
                };
            }
            return curInfos;
        }

    }
}
