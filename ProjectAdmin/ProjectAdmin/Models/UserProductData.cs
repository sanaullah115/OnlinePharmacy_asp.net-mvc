//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProjectAdmin.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserProductData
    {
        public int PID { get; set; }
        public string ProductName { get; set; }
        public Nullable<long> Price { get; set; }
        public Nullable<int> Quantity { get; set; }
        public string PhoneNumber { get; set; }
        public string CreditCardNumber { get; set; }
        public string PinCode { get; set; }
        public string Docmuent { get; set; }
        public Nullable<int> ProID { get; set; }
        public Nullable<int> SignInfk { get; set; }
    
        public virtual Product Product { get; set; }
        public virtual SignUp SignUp { get; set; }
    }
}