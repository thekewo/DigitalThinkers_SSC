using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalThinkers_SSC.Models;

namespace DigitalThinkers_SSC.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class V1Controller : ControllerBase
    {
        private readonly StockContext _context;

        public V1Controller(StockContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<Dictionary<string,int>> Stock(StockViewModel moneyViewModel)
        {
            foreach (var stock in moneyViewModel.inserted)
            {
                var stockValue = int.Parse(stock.Key);
                var sadsa = _context.StockItems.Select(s => s.Value);
                if (_context.StockItems.Select(s => s.Value).Contains(stockValue))
                {
                    var stockInDb = _context.StockItems.Find(stockValue);
                    stockInDb.Volume = stock.Value;
                }
                else
                {
                    _context.StockItems.Add(new Stock() { Value = stockValue, Volume = stock.Value });
                    await _context.SaveChangesAsync();
                }
            }

            return moneyViewModel.inserted;
        }
    }
}
