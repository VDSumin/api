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
    
    public partial class hostel_catalog
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public hostel_catalog()
        {
            this.hostel_agreement = new HashSet<hostel_agreement>();
            this.hostel_contract = new HashSet<hostel_contract>();
            this.hostel_cost_calculation = new HashSet<hostel_cost_calculation>();
            this.hostel_cost = new HashSet<hostel_cost>();
            this.hostel_manual_agreement = new HashSet<hostel_manual_agreement>();
        }
    
        public int id { get; set; }
        public string description { get; set; }
        public string parent { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hostel_agreement> hostel_agreement { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hostel_contract> hostel_contract { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hostel_cost_calculation> hostel_cost_calculation { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hostel_cost> hostel_cost { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<hostel_manual_agreement> hostel_manual_agreement { get; set; }
    }
}
