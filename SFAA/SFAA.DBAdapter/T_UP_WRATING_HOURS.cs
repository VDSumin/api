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
    using System.ComponentModel.DataAnnotations;
    
    public partial class T_UP_WRATING_HOURS
    {
        [Key]
        public byte[] F_NREC { get; set; }
        public byte[] F_CLIST { get; set; }
        public int F_AUDHOURS { get; set; }
        public int F_CWDATE { get; set; }
    }
}