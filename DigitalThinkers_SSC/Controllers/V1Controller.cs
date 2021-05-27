using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigitalThinkers_SSC.Models;
using Microsoft.Extensions.Logging;

namespace DigitalThinkers_SSC.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class V1Controller : ControllerBase
    {
        private readonly StockContext _context;
        private readonly ILogger<V1Controller> _logger;

        public V1Controller(
            StockContext context,
            ILogger<V1Controller> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<Dictionary<string,int>> PostStock(StockViewModel moneyViewModel)
        {
            _logger.LogInformation("PostStock called at: {time}", DateTimeOffset.Now);
            foreach (var stock in moneyViewModel.inserted)
            {
                var stockValue = int.Parse(stock.Key);
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

        [HttpGet]
        public async Task<Dictionary<string, int>> GetStock()
        {
            Dictionary<string, int> stockDictionary = ListToDictionary(await _context.StockItems.ToListAsync());

            return stockDictionary;
        }

        private Dictionary<string, int> ListToDictionary(List<Stock> stockList)
        {
            
            var stockDictionary = new Dictionary<string, int>();
            foreach (var stock in stockList)
            {
                stockDictionary.Add(stock.Value.ToString(), stock.Volume);
            }

            return stockDictionary;
        }

        [HttpPost]
        public async Task<ActionResult> Checkout(StockViewModel moneyViewModel)
        {
            var returnedBillsAndCoins = new Dictionary<string, int>();

            foreach (var stock in moneyViewModel.inserted)
            {
                moneyViewModel.price -= stock.Value * int.Parse(stock.Key);
            }

            var stockItems = _context.StockItems.ToArray();
            Array.Sort(stockItems, (a, b) => b.Value.CompareTo(a.Value));
            var isAllZeros = !stockItems.Select(s => s.Volume).Any(v => v != 0);

            while (moneyViewModel.price < 0 && stockItems.Length > 0 && !isAllZeros)
            {
                foreach (var stock in stockItems)
                {
                    var addCounter = 0;
                    while (stock.Volume > 0 && (Math.Abs(moneyViewModel.price) >= stock.Value))
                    {
                        moneyViewModel.price += stock.Value;
                        stock.Volume--;
                        addCounter++;
                    }
                    returnedBillsAndCoins.Add(stock.Value.ToString(), addCounter);
                }
                isAllZeros = !stockItems.Select(s => s.Volume).Any(v => v != 0);
            }

            if(moneyViewModel.price == 0)
            {
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(Checkout), returnedBillsAndCoins);
            }

            if(moneyViewModel.price > 0)
            {
                return BadRequest("Payment isn't sufficient.");
            }
            else
            {
                return BadRequest("Stock insufficient.");
            }
        }
    }
}
