using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce_Website.Models
{
    public class Cart
    {

        [Key]
        public int cart_id { get; set; }
        public int product_id { get; set; }
        public int cust_id { get; set; }

        public int product_quantity { get; set; }
        public int cart_status { get; set; }

        [ForeignKey("product_id")]
        public Product products { get; set; }

        [ForeignKey("cust_id")]

        public Customer customers { get; set; }

    }
}
