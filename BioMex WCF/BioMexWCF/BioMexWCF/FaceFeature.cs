//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BioMexWCF
{
    using System;
    using System.Collections.Generic;
    
    public partial class FaceFeature
    {
        public int FF_ID { get; set; }
        public string FF_KEY_POINTS { get; set; }
        public string FF_DESCRIPTORS { get; set; }
    
        public virtual Feature Feature { get; set; }
    }
}