using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    using System.Runtime.Remoting.Messaging;

    public class JsonWorkCurrStruct
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWorkCurrStruct"/> class.
        /// </summary>
        public JsonWorkCurrStruct() 
        {
           
        }

        /// <summary>
        /// Nrec ведомости
        /// </summary>
        [JsonIgnore]
        public byte[] Nrec
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public string NrecString
        {
            get;
            set;
        }

        /// <summary>
        /// CurDisNrec
        /// </summary>
        [JsonProperty("CurDisNrec")]
        public string CurDisNrec
        {
            get;
            set;
        }

        /// <summary>
        /// CycleNrec
        /// </summary>
        [JsonProperty("CycleNrec")]
        public string CycleNrec
        {
            get;
            set;
        }

        /// <summary>
        /// CYCLE
        /// </summary>
        [JsonProperty("CYCLE")]
        public string CYCLE
        {
            get;
            set;
        }

        /// <summary>
        /// CycleType
        /// </summary>
        [JsonProperty("CycleType")]
        public string CycleType
        {
            get;
            set;
        }

        /// <summary>
        /// CompName
        /// </summary>
        [JsonProperty("CompName")]
        public string CompName
        {
            get;
            set;
        }

        /// <summary>
        /// WTypeComponent
        /// </summary>
        [JsonProperty("WTypeComponent")]
        public string WTypeComponent
        {
            get;
            set;
        }

        /// <summary>
        /// WLevelReal
        /// </summary>
        [JsonProperty("WLevelReal")]
        public string WLevelReal
        {
            get;
            set;
        }

        /// <summary>
        /// CCOMPONENT
        /// </summary>
        [JsonProperty("CCOMPONENT")]
        public string CCOMPONENT
        {
            get;
            set;
        }

        /// <summary>
        /// CDIS
        /// </summary>
        [JsonProperty("CDIS")]
        public string CDIS
        {
            get;
            set;
        }

        /// <summary>
        /// LevelComponentForDVS
        /// </summary>
        [JsonProperty("LevelComponentForDVS")]
        public string LevelComponentForDVS
        {
            get;
            set;
        }

        /// <summary>
        /// CycleReal
        /// </summary>
        [JsonProperty("CycleReal")]
        public string CycleReal
        {
            get;
            set;
        }

        /// <summary>
        /// ComponentReal
        /// </summary>
        [JsonProperty("ComponentReal")]
        public string ComponentReal
        {
            get;
            set;
        }

        /// <summary>
        /// DC_Type
        /// </summary>
        [JsonProperty("DC_Type")]
        public string DC_Type
        {
            get;
            set;
        }

        /// <summary>
        /// DisWPROP
        /// </summary>
        [JsonProperty("DisWPROP")]
        public string DisWPROP
        {
            get;
            set;
        }

        /// <summary>
        /// DisCur
        /// </summary>
        [JsonProperty("DisCur")]
        public string DisCur
        {
            get;
            set;
        }

        /// <summary>
        /// InSize
        /// </summary>
        [JsonProperty("InSize")]
        public string InSize
        {
            get;
            set;
        }

        /// <summary>
        /// Koeff
        /// </summary>
        [JsonProperty("Koeff")]
        public string Koeff
        {
            get;
            set;
        }

        /// <summary>
        /// Levelf
        /// </summary>
        [JsonProperty("Levelf")]
        public string Levelf
        {
            get;
            set;
        }

        /// <summary>
        /// LevelCode
        /// </summary>
        [JsonProperty("LevelCode")]
        public string LevelCode
        {
            get;
            set;
        }

        /// <summary>
        /// cod
        /// </summary>
        [JsonProperty("cod")]
        public string cod
        {
            get;
            set;
        }

        /// <summary>
        /// Abbr
        /// </summary>
        [JsonProperty("Abbr")]
        public string Abbr
        {
            get;
            set;
        }

        /// <summary>
        /// Kaf
        /// </summary>
        [JsonProperty("Kaf")]
        public string Kaf
        {
            get;
            set;
        }

        /// <summary>
        /// KafAbb
        /// </summary>
        [JsonProperty("KafAbb")]
        public string KafAbb
        {
            get;
            set;
        }

        /// <summary>
        /// DC_Type1
        /// </summary>
        [JsonProperty("DC_Type1")]
        public string DC_Type1
        {
            get;
            set;
        }

        /// <summary>
        /// ZE_All
        /// </summary>
        [JsonProperty("ZE_All")]
        public string ZE_All
        {
            get;
            set;
        }

        /// <summary>
        /// Hour_All
        /// </summary>
        [JsonProperty("Hour_All")]
        public string Hour_All
        {
            get;
            set;
        }

        /// <summary>
        /// ZE_Pl
        /// </summary>
        [JsonProperty("ZE_Pl")]
        public string ZE_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// ReZE
        /// </summary>
        [JsonProperty("ReZE")]
        public string ReZE
        {
            get;
            set;
        }

        /// <summary>
        /// HourCurPl
        /// </summary>
        [JsonProperty("HourCurPl")]
        public string HourCurPl
        {
            get;
            set;
        }

        /// <summary>
        /// ReHour
        /// </summary>
        [JsonProperty("ReHour")]
        public string ReHour
        {
            get;
            set;
        }

        /// <summary>
        /// Aud_Pl
        /// </summary>
        [JsonProperty("Aud_Pl")]
        public string Aud_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// HOUREXAM
        /// </summary>
        [JsonProperty("HOUREXAM")]
        public string HOUREXAM
        {
            get;
            set;
        }

        /// <summary>
        /// SRS_Pl
        /// </summary>
        [JsonProperty("SRS_Pl")]
        public string SRS_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// Lec_Pl
        /// </summary>
        [JsonProperty("Lec_Pl")]
        public string Lec_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// Pr_Pl
        /// </summary>
        [JsonProperty("Pr_Pl")]
        public string Pr_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// Lab_Pl
        /// </summary>
        [JsonProperty("Lab_Pl")]
        public string Lab_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// KSR_Pl
        /// </summary>
        [JsonProperty("KSR_Pl")]
        public string KSR_Pl
        {
            get;
            set;
        }

        /// <summary>
        /// Pr
        /// </summary>
        [JsonProperty("Pr")]
        public string Pr
        {
            get;
            set;
        }

        /// <summary>
        /// IGA
        /// </summary>
        [JsonProperty("IGA")]
        public string IGA
        {
            get;
            set;
        }

        /// <summary>
        /// AttAll
        /// </summary>
        [JsonProperty("AttAll")]
        public string AttAll
        {
            get;
            set;
        }

        /// <summary>
        /// ReAtt
        /// </summary>
        [JsonProperty("ReAtt")]
        public string ReAtt
        {
            get;
            set;
        }

        /// <summary>
        /// AttEx
        /// </summary>
        [JsonProperty("AttEx")]
        public string AttEx
        {
            get;
            set;
        }

        /// <summary>
        /// AttZh
        /// </summary>
        [JsonProperty("AttZh")]
        public string AttZh
        {
            get;
            set;
        }

        /// <summary>
        /// AttKP
        /// </summary>
        [JsonProperty("AttKP")]
        public string AttKP
        {
            get;
            set;
        }

        /// <summary>
        /// Lec1s
        /// </summary>
        [JsonProperty("Lec1s")]
        public string Lec1s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs1s
        /// </summary>
        [JsonProperty("PZs1s")]
        public string PZs1s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs1s
        /// </summary>
        [JsonProperty("LRs1s")]
        public string LRs1s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs1s
        /// </summary>
        [JsonProperty("SRSs1s")]
        public string SRSs1s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs1s
        /// </summary>
        [JsonProperty("KSRs1s")]
        public string KSRs1s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs1s
        /// </summary>
        [JsonProperty("PrIGAs1s")]
        public string PrIGAs1s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec2s
        /// </summary>
        [JsonProperty("Lec2s")]
        public string Lec2s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs2s
        /// </summary>
        [JsonProperty("PZs2s")]
        public string PZs2s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs2s
        /// </summary>
        [JsonProperty("LRs2s")]
        public string LRs2s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs2s
        /// </summary>
        [JsonProperty("SRSs2s")]
        public string SRSs2s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs2s
        /// </summary>
        [JsonProperty("KSRs2s")]
        public string KSRs2s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs2s
        /// </summary>
        [JsonProperty("PrIGAs2s")]
        public string PrIGAs2s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec3s
        /// </summary>
        [JsonProperty("Lec3s")]
        public string Lec3s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs3s
        /// </summary>
        [JsonProperty("PZs3s")]
        public string PZs3s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs3s
        /// </summary>
        [JsonProperty("LRs3s")]
        public string LRs3s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs3s
        /// </summary>
        [JsonProperty("SRSs3s")]
        public string SRSs3s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs3s
        /// </summary>
        [JsonProperty("KSRs3s")]
        public string KSRs3s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs3s
        /// </summary>
        [JsonProperty("PrIGAs3s")]
        public string PrIGAs3s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec4s
        /// </summary>
        [JsonProperty("Lec4s")]
        public string Lec4s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs4s
        /// </summary>
        [JsonProperty("PZs4s")]
        public string PZs4s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs4s
        /// </summary>
        [JsonProperty("LRs4s")]
        public string LRs4s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs4s
        /// </summary>
        [JsonProperty("SRSs4s")]
        public string SRSs4s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs4s
        /// </summary>
        [JsonProperty("KSRs4s")]
        public string KSRs4s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs4s
        /// </summary>
        [JsonProperty("PrIGAs4s")]
        public string PrIGAs4s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec5s
        /// </summary>
        [JsonProperty("Lec5s")]
        public string Lec5s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs5s
        /// </summary>
        [JsonProperty("PZs5s")]
        public string PZs5s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs5s
        /// </summary>
        [JsonProperty("LRs5s")]
        public string LRs5s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs5s
        /// </summary>
        [JsonProperty("SRSs5s")]
        public string SRSs5s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs5s
        /// </summary>
        [JsonProperty("KSRs5s")]
        public string KSRs5s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs5s
        /// </summary>
        [JsonProperty("PrIGAs5s")]
        public string PrIGAs5s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec6s
        /// </summary>
        [JsonProperty("Lec6s")]
        public string Lec6s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs6s
        /// </summary>
        [JsonProperty("PZs6s")]
        public string PZs6s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs6s
        /// </summary>
        [JsonProperty("LRs6s")]
        public string LRs6s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs6s
        /// </summary>
        [JsonProperty("SRSs6s")]
        public string SRSs6s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs6s
        /// </summary>
        [JsonProperty("KSRs6s")]
        public string KSRs6s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs6s
        /// </summary>
        [JsonProperty("PrIGAs6s")]
        public string PrIGAs6s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec7s
        /// </summary>
        [JsonProperty("Lec7s")]
        public string Lec7s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs7s
        /// </summary>
        [JsonProperty("PZs7s")]
        public string PZs7s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs7s
        /// </summary>
        [JsonProperty("LRs7s")]
        public string LRs7s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs7s
        /// </summary>
        [JsonProperty("SRSs7s")]
        public string SRSs7s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs7s
        /// </summary>
        [JsonProperty("KSRs7s")]
        public string KSRs7s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs7s
        /// </summary>
        [JsonProperty("PrIGAs7s")]
        public string PrIGAs7s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec8s
        /// </summary>
        [JsonProperty("Lec8s")]
        public string Lec8s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs8s
        /// </summary>
        [JsonProperty("PZs8s")]
        public string PZs8s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs8s
        /// </summary>
        [JsonProperty("LRs8s")]
        public string LRs8s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs8s
        /// </summary>
        [JsonProperty("SRSs8s")]
        public string SRSs8s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs8s
        /// </summary>
        [JsonProperty("KSRs8s")]
        public string KSRs8s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs8s
        /// </summary>
        [JsonProperty("PrIGAs8s")]
        public string PrIGAs8s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec9s
        /// </summary>
        [JsonProperty("Lec9s")]
        public string Lec9s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs9s
        /// </summary>
        [JsonProperty("PZs9s")]
        public string PZs9s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs9s
        /// </summary>
        [JsonProperty("LRs9s")]
        public string LRs9s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs9s
        /// </summary>
        [JsonProperty("SRSs9s")]
        public string SRSs9s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs9s
        /// </summary>
        [JsonProperty("KSRs9s")]
        public string KSRs9s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs9s
        /// </summary>
        [JsonProperty("PrIGAs9s")]
        public string PrIGAs9s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec10s
        /// </summary>
        [JsonProperty("Lec10s")]
        public string Lec10s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs10s
        /// </summary>
        [JsonProperty("PZs10s")]
        public string PZs10s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs10s
        /// </summary>
        [JsonProperty("LRs10s")]
        public string LRs10s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs10s
        /// </summary>
        [JsonProperty("SRSs10s")]
        public string SRSs10s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs10s
        /// </summary>
        [JsonProperty("KSRs10s")]
        public string KSRs10s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs10s
        /// </summary>
        [JsonProperty("PrIGAs10s")]
        public string PrIGAs10s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec11s
        /// </summary>
        [JsonProperty("Lec11s")]
        public string Lec11s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs11s
        /// </summary>
        [JsonProperty("PZs11s")]
        public string PZs11s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs11s
        /// </summary>
        [JsonProperty("LRs11s")]
        public string LRs11s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs11s
        /// </summary>
        [JsonProperty("SRSs11s")]
        public string SRSs11s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs11s
        /// </summary>
        [JsonProperty("KSRs11s")]
        public string KSRs11s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs11s
        /// </summary>
        [JsonProperty("PrIGAs11s")]
        public string PrIGAs11s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec12s
        /// </summary>
        [JsonProperty("Lec12s")]
        public string Lec12s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs12s
        /// </summary>
        [JsonProperty("PZs12s")]
        public string PZs12s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs12s
        /// </summary>
        [JsonProperty("LRs12s")]
        public string LRs12s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs12s
        /// </summary>
        [JsonProperty("SRSs12s")]
        public string SRSs12s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs12s
        /// </summary>
        [JsonProperty("KSRs12s")]
        public string KSRs12s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs12s
        /// </summary>
        [JsonProperty("PrIGAs12s")]
        public string PrIGAs12s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec13s
        /// </summary>
        [JsonProperty("Lec13s")]
        public string Lec13s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs13s
        /// </summary>
        [JsonProperty("PZs13s")]
        public string PZs13s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs13s
        /// </summary>
        [JsonProperty("LRs13s")]
        public string LRs13s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs13s
        /// </summary>
        [JsonProperty("SRSs13s")]
        public string SRSs13s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs13s
        /// </summary>
        [JsonProperty("KSRs13s")]
        public string KSRs13s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs13s
        /// </summary>
        [JsonProperty("PrIGAs13s")]
        public string PrIGAs13s
        {
            get;
            set;
        }

        /// <summary>
        /// Lec14s
        /// </summary>
        [JsonProperty("Lec14s")]
        public string Lec14s
        {
            get;
            set;
        }

        /// <summary>
        /// PZs14s
        /// </summary>
        [JsonProperty("PZs14s")]
        public string PZs14s
        {
            get;
            set;
        }

        /// <summary>
        /// LRs14s
        /// </summary>
        [JsonProperty("LRs14s")]
        public string LRs14s
        {
            get;
            set;
        }

        /// <summary>
        /// SRSs14s
        /// </summary>
        [JsonProperty("SRSs14s")]
        public string SRSs14s
        {
            get;
            set;
        }

        /// <summary>
        /// KSRs14s
        /// </summary>
        [JsonProperty("KSRs14s")]
        public string KSRs14s
        {
            get;
            set;
        }

        /// <summary>
        /// PrIGAs14s
        /// </summary>
        [JsonProperty("PrIGAs14s")]
        public string PrIGAs14s
        {
            get;
            set;
        }
    }
}
