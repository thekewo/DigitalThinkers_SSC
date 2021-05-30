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
        private readonly ILogger _logger;

        public V1Controller(
            StockContext context,
            ILogger<V1Controller> logger)
        {
            _context = context;
            _logger = logger;
        }

        /*
         * Gets a json that is mapped into a StockViewModel. The StockViewModel contains what kind of bill or coin
         * and how much of it are put into the machine. Then it saves this data into the memory.
         * If the value already present in the database then it updates it's volume.
         * If not then adds the value paired with it's volume.
         */
        [HttpPost]
        public async Task<Dictionary<string,int>> Stock(StockViewModel stockViewModel)
        {
            _logger.LogInformation("PostStock called at: {time}", DateTimeOffset.Now);
            foreach (var stock in stockViewModel.inserted)
            {
                var stockValue = int.Parse(stock.Key);
                if (_context.StockItems.Select(s => s.Value).Contains(stockValue))
                {
                    var stockInDb = _context.StockItems.Find(stockValue);
                    stockInDb.Volume = stock.Value;
                }
                else
                {
                    _logger.LogInformation($"Added value with volume to the database: {stockValue} {stock.Value}");
                    _context.StockItems.Add(new Stock() { Value = stockValue, Volume = stock.Value });
                    await _context.SaveChangesAsync();
                }
            }

            _logger.LogInformation("PostStock finished at: {time}", DateTimeOffset.Now);
            return stockViewModel.inserted;
        }

        /*
         * Returns the stored bill and coin list with their volume.
         */
        [HttpGet]
        public async Task<Dictionary<string, int>> Stock()
        {
            _logger.LogInformation("GetStock called at: {time}", DateTimeOffset.Now);
            Dictionary<string, int> stockDictionary = ListToDictionary(await _context.StockItems.ToListAsync());

            _logger.LogInformation("GetStock finished at: {time}", DateTimeOffset.Now);
            return stockDictionary;
        }

        /*
         * Converts a list of Stock objects into a Dictionary<string, int>
         */
        private Dictionary<string, int> ListToDictionary(List<Stock> stockList)
        {
            _logger.LogInformation("ListToDictionary called at: {time}", DateTimeOffset.Now);
            var stockDictionary = new Dictionary<string, int>();
            foreach (var stock in stockList)
            {
                stockDictionary.Add(stock.Value.ToString(), stock.Volume);
            }

            _logger.LogInformation("ListToDictionary finished at: {time}", DateTimeOffset.Now);
            return stockDictionary;
        }

        /*
         * Gets a json that is mapped into a StockViewModel. The StockViewModel contains what kind of bill or coin
         * and how much of it are put into the machine and the price of the item that the customer wants to buy.
         * Then the price of the item is reduced by the value of bills and coins put into the machine as payment.
         * Then if the price is less then 0, then the machine needs to give back bills or coins from it's stock.
         * Then if the price is more then 0, it means that the machine has insufficient stock, so it can't make the transaction.
         */
        [HttpPost]
        public async Task<ActionResult> Checkout(StockViewModel stockViewModel)
        {
            _logger.LogInformation("Checkout called at: {time}", DateTimeOffset.Now);
            var returnedBillsAndCoins = new Dictionary<string, int>();

            _logger.LogInformation("Initial price value: {stockViewModel.price}.", stockViewModel.price);
            foreach (var stock in stockViewModel.inserted)
            {
                stockViewModel.price -= stock.Value * int.Parse(stock.Key);
            }

            /*
             * Stock in the machine, sorted in decreasing order.
             * Then check if the volume of the bills and coins are all zero.
             */
            var stockItems = _context.StockItems.ToArray();
            Array.Sort(stockItems, (a, b) => b.Value.CompareTo(a.Value));
            var isAllZeros = !stockItems.Select(s => s.Volume).Any(v => v != 0);

            while (stockViewModel.price < 0 && stockItems.Length > 0 && !isAllZeros)
            {
                foreach (var stock in stockItems)
                {
                    var addCounter = 0;
                    /*
                     * Math.Abs needed, because at this point the price of the item is negative.
                     * A check needed to see if bill or coin in the machine can be given as change.
                     * If it is possible to give back as change, then the volume of it needs to be decreased.
                     */
                    while (stock.Volume > 0 && (Math.Abs(stockViewModel.price) >= stock.Value))
                    {
                        stockViewModel.price += stock.Value;
                        stock.Volume--;
                        addCounter++;
                    }
                    //Returned bills and coins and their volume
                    returnedBillsAndCoins.Add(stock.Value.ToString(), addCounter);
                }
                isAllZeros = !stockItems.Select(s => s.Volume).Any(v => v != 0);
                _logger.LogInformation("Stock isAllZeros check result: {isAllZeros}.", isAllZeros);
            }

            if(stockViewModel.price == 0)
            {
                await _context.SaveChangesAsync();

                _logger.LogInformation("Checkout finished at: {time}. Payment and Stock was sufficient for the transaction", DateTimeOffset.Now);
                return CreatedAtAction(nameof(Checkout), returnedBillsAndCoins);
            }

            if(stockViewModel.price > 0)
            {
                _logger.LogInformation("Checkout finished at: {time}. Payment insufficient.", DateTimeOffset.Now);
                return BadRequest("Payment insufficient.");
            }
            else
            {
                _logger.LogInformation("Checkout finished at: {time}. Stock insufficient.", DateTimeOffset.Now);
                return BadRequest("Stock insufficient.");
            }
        }

        /*
         * Return with an array containing the denominations the machine currently accepts.
         * A bill or coin cannot be accepted if the machine would not be able to give back proper change.
         */
        [HttpGet]
        public async Task<ActionResult> BlockedBills()
        {
            _logger.LogInformation("BlockedBills called at: {time}", DateTimeOffset.Now);
            var listOfBillsAndCoins = new List<int>()
            {
                20000,
                10000,
                5000,
                2000,
                1000,
                500,
                200,
                100,
                50,
                20,
                10,
                5 
            };

            /*
             * Stock in the machine, sorted in decreasing order.
             * Then check if the volume of the bills and coins are all zero.
             */
            var stockItems = _context.StockItems.ToArray();
            Array.Sort(stockItems, (a, b) => b.Value.CompareTo(a.Value));
            var isAllZeros = !stockItems.Select(s => s.Volume).Any(v => v != 0);

            if(isAllZeros)
            {
                _logger.LogInformation("Checkout finished at: {time}. The machine's stock is empty.", DateTimeOffset.Now);
                return CreatedAtAction(nameof(BlockedBills), new List<string>());
            }

            var sum = stockItems.Select(s => s.Value * s.Volume).Sum();

            for (int i = listOfBillsAndCoins.Count -1; i >= 0; i--)
            {
                if(listOfBillsAndCoins[i] > sum)
                {
                    _logger.LogInformation("Removed {value} from the list of possible denominations.", listOfBillsAndCoins[i]);
                    listOfBillsAndCoins.Remove(listOfBillsAndCoins[i]);
                }
            }

            _logger.LogInformation("Checkout finished at: {time}. Returned the list of possible denominations.", DateTimeOffset.Now);
            return CreatedAtAction(nameof(BlockedBills), listOfBillsAndCoins.Select(bas => bas.ToString()));
        }
    }
}
