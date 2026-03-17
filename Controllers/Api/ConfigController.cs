using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PingMonitorApp.Data;
using PingMonitorApp.Models;

namespace PingMonitorApp.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConfigController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("recipients")]
        public async Task<IActionResult> GetRecipients()
        {
            var list = await _context.EmailRecipients.ToListAsync();
            return Ok(list);
        }

        [HttpPost("recipients")]
        public async Task<IActionResult> AddRecipient([FromBody] EmailRecipient recipient)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            _context.EmailRecipients.Add(recipient);
            await _context.SaveChangesAsync();
            return Ok(recipient);
        }

        [HttpDelete("recipients/{id}")]
        public async Task<IActionResult> DeleteRecipient(int id)
        {
            var item = await _context.EmailRecipients.FindAsync(id);
            if (item == null) return NotFound();

            _context.EmailRecipients.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPatch("recipients/{id}/toggle")]
        public async Task<IActionResult> ToggleRecipient(int id)
        {
            var item = await _context.EmailRecipients.FindAsync(id);
            if (item == null) return NotFound();

            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        [HttpPut("recipients/{id}")]
        public async Task<IActionResult> UpdateRecipient(int id, [FromBody] EmailRecipient recipient)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            
            var item = await _context.EmailRecipients.FindAsync(id);
            if (item == null) return NotFound();

            item.Name = recipient.Name;
            item.Email = recipient.Email;

            await _context.SaveChangesAsync();
            return Ok(item);
        }
    }
}
