using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Website.Models
{
    public class Order
    {
        [Key]
        public int order_id { get; set; }

        public int cust_id { get; set; }

        public decimal total_price { get; set; }  

        public DateTime order_date { get; set; }

        [ForeignKey("cust_id")]
        public Customer customers { get; set; }
    }
}
