using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalThinkers_SSC.Models
{
    public class StockViewModel
    {
        public Dictionary<string, int> inserted { get; set; }
        public int price { get; set; }
    }
}
