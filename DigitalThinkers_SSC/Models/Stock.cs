using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalThinkers_SSC.Models
{
    public class Stock
    {
        [Key]
        public int Value { get; set; }
        public int Volume { get; set; }
    }
}
