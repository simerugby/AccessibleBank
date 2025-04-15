using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccessibleBank.Data;
using AccessibleBank.Models;
using System.Security.Claims;

namespace AccessibleBank.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly BankingContext _context;

        public TransactionsController(BankingContext context)
        {
            _context = context;
        }

        // POST: api/transactions
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] Transaction transaction)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var fromAccount = await _context.Accounts.FindAsync(transaction.FromAccountId);
            var toAccount = await _context.Accounts.FindAsync(transaction.ToAccountId);

            if (fromAccount == null || toAccount == null)
                return NotFound("One or both accounts not found.");

            if (fromAccount.UserId != userId)
                return Unauthorized("You don't own the source account.");

            if (fromAccount.Balance < transaction.Amount)
                return BadRequest("Insufficient funds.");

            if (transaction.FromAccountId == transaction.ToAccountId)
                return BadRequest("You cannot transfer to the same account.");

            if (fromAccount.Currency != toAccount.Currency)
                return BadRequest("Cannot transfer between accounts with different currencies.");

            fromAccount.Balance -= transaction.Amount;
            toAccount.Balance += transaction.Amount;

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        // GET: api/transactions
        [HttpGet]
        public async Task<IActionResult> GetMyTransactions([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] decimal? minAmount = null, [FromQuery] decimal? maxAmount = null, [FromQuery] DateTime? dateFrom = null, [FromQuery] DateTime? dateTo = null)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var accountIds = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => a.Id)
                .ToListAsync();
            
            var query = _context.Transactions
                .Where(t => accountIds.Contains(t.FromAccountId) || accountIds.Contains(t.ToAccountId));

            if (minAmount.HasValue)
                query = query.Where(t => t.Amount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(t => t.Amount <= maxAmount.Value);

            if (dateFrom.HasValue)
                query = query.Where(t => t.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(t => t.Date <= dateTo.Value);

            var transactions = await query
                .OrderByDescending(t => t.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(transactions);
        }
    }
}
