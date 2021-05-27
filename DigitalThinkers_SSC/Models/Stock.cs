using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DigitalThinkers_SSC.Models
{
    /*
     Model for the money stock in the machine.
     */
    public class Stock
    {
        //The value of the bill or coin
        [Key]
        public int Value { get; set; }
        //The volume of the bill or coin
        public int Volume { get; set; }
    }
}
