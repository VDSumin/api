//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SFAA.DBAdapter
{
    using System;
    using System.Collections.Generic;
    
    public partial class hostel_cost_calculation
    {
        public int id { get; set; }
        public int calculationDescriptionId { get; set; }
        public int costId { get; set; }
        public decimal amountOfPayments { get; set; }
    
        public virtual hostel_catalog hostel_catalog { get; set; }
        public virtual hostel_cost hostel_cost { get; set; }
    }
}