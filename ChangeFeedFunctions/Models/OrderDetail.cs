using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeFeedFunctions.Models
{
    public class OrderDetail
    {
        public string Email { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public decimal? UnitPrice { get; set; }
    }
}
