//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TangaDummy
{
    using System;
    using System.Collections.Generic;
    
    public partial class ConfirmedPayment
    {
        public long ConfirmedPaymentsID { get; set; }
        public string MerchantRequestID { get; set; }
        public string CheckoutRequestID { get; set; }
        public Nullable<int> Amount { get; set; }
        public string ReceiptNumber { get; set; }
        public Nullable<System.DateTime> TransactionDate { get; set; }
        public string PhoneNumber { get; set; }
    }
}
