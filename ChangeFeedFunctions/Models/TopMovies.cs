using System;
using System.Collections.Generic;
using System.Text;

namespace ChangeFeedFunctions.Models
{
    public class Detail
    {
        public string ProductName { get; set; }
        public int ProductId { get; set; }
        public int SalesQuantity { get; set; }
    }

    public class TopMovies
    {
        public List<Detail> Details { get; set; }
        public string id { get; set; }
    }
}
