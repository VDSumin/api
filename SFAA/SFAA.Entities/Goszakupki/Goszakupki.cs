using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Util;
using Newtonsoft.Json;

namespace SFAA.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class Goszakupki
    {
        /// <summary>
        /// code
        /// </summary>
        [JsonProperty("code")]
        public string Code
        {
            get;
            set;
        }

        /// <summary>
        /// title
        /// </summary>
        [JsonProperty("title")]
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// link
        /// </summary>
        [JsonProperty("link")]
        public string Link
        {
            get;
            set;
        }

        /// <summary>
        /// Наименование объекта закупки
        /// </summary>
        [JsonProperty("nameOfTheObject")]
        public string NameOfTheObject
        {
            get;
            set;
        }

        /// <summary>
        /// Размещение выполняется по
        /// </summary>
        [JsonProperty("accommodationIsPerformedBy")]
        public string AccommodationIsPerformedBy
        {
            get;
            set;
        }

        /// <summary>
        /// Наименование Заказчика
        /// </summary>
        [JsonProperty("customerName")]
        public string CustomerName
        {
            get;
            set;
        }

        /// <summary>
        /// Начальная цена контракта
        /// </summary>
        [JsonProperty("initialContractPrice")]
        public string InitialContractPrice
        {
            get;
            set;
        }

        /// <summary>
        /// Валюта
        /// </summary>
        [JsonProperty("currency")]
        public string Currency
        {
            get;
            set;
        }

        /// <summary>
        /// Размещено
        /// </summary>
        [JsonProperty("posted")]
        public string Posted
        {
            get;
            set;
        }

        /// <summary>
        /// Обновлено
        /// </summary>
        [JsonProperty("updated")]
        public string Updated
        {
            get;
            set;
        }

        /// <summary>
        /// Этап размещения
        /// </summary>
        [JsonProperty("placementStage")]
        public string PlacementStage
        {
            get;
            set;
        }

        /// <summary>
        /// Идентификационный код закупки (ИКЗ)
        /// </summary>
        [JsonProperty("procurementIdentificationCode")]
        public string ProcurementIdentificationCode
        {
            get;
            set;
        }

        /// <summary>
        /// pubDate
        /// </summary>
        [JsonProperty("pubDate")]
        public string PubDate
        {
            get;
            set;
        }

        /// <summary>
        /// Автор
        /// </summary>
        [JsonProperty("author")]
        public string Author
        {
            get;
            set;
        }

        /// <summary>
        /// дата публикации извещения
        /// </summary>
        [JsonProperty("dateOfPublicationOfTheNotice")]
        public string DateOfPublicationOfTheNotice
        {
            get;
            set;
        }

        /// <summary>
        /// дата окончания подачи заявок
        /// </summary>
        [JsonProperty("filingDate")]
        public string FilingDate
        {
            get;
            set;
        }

        /// <summary>
        /// дата рассмотрения 1-х частей заявок
        /// </summary>
        [JsonProperty("dateOfConsiderationOf-1-PartApplications")]
        public string DateOfConsiderationOf1PartApplications
        {
            get;
            set;
        }

        /// <summary>
        /// дата проведения электронного аукциона
        /// </summary>
        [JsonProperty("electronicAuctionDate")]
        public string ElectronicAuctionDate
        {
            get;
            set;
        }

        /// <summary>
        /// дата окончания рассмотрения 2-х частей заявок
        /// </summary>
        [JsonProperty("endDateForConsiderationOf-2-PartsApplications")]
        public string EndDateForConsiderationOf2PartsApplications
        {
            get;
            set;
        }

        /// <summary>
        /// дата подписания Договора победителей торгов
        /// </summary>
        [JsonProperty("dateOfSigningTheTenderWinnersAgreement")]
        public string DateOfSigningTheTenderWinnersAgreement
        {
            get;
            set;
        }

        /// <summary>
        /// дата подписания Договора заказчиком
        /// </summary>
        [JsonProperty("dateOfSigningTheContractByTheCustomer")]
        public string DateOfSigningTheContractByTheCustomer
        {
            get;
            set;
        }

        /// <summary>
        /// дата прекращения работы с закупкой
        /// </summary>
        [JsonProperty("dateOfTerminationOfWorkWithThePurchase")]
        public string DateOfTerminationOfWorkWithThePurchase
        {
            get;
            set;
        }

        /// <summary>
        /// дата начала срока подачи ценовых предложений
        /// </summary>
        [JsonProperty("dateOfThePeriodForQuotations")]
        public string DateOfThePeriodForQuotations
        {
            get;
            set;
        }

    }
}
