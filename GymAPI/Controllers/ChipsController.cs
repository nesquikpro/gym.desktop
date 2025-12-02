using GymAPI.Data;
using GymAPI.Models;
using GymAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GymAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChipsController : ControllerBase
    {
        private readonly GymDbContext _context;
        private readonly ChipNumberGenerator _generator;
        public ChipsController(GymDbContext context, ChipNumberGenerator generator)
        {
            _context = context;
            _generator = generator;
        }

        // GET: api/Chips
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Chip>>> GetChips()
        {
            return await _context.Chips.ToListAsync();
        }

        // GET: api/Chips/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Chip>> GetChip(int id)
        {
            var chip = await _context.Chips.FindAsync(id);

            if (chip == null)
            {
                return NotFound();
            }

            return chip;
        }

        // PUT: api/Chips/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutChip(int id, Chip chip)
        {
            if (id != chip.ChipId)
            {
                return BadRequest();
            }

            _context.Entry(chip).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ChipExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return !ChipExists(id) ? NotFound(id) : StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            return NoContent();
        }

        // POST: api/Chips
        [HttpPost]
        public async Task<ActionResult<Chip>> PostChip(Chip chip)
        {
            try
            {
                chip.ChipNumber = await _generator.GenerateAsync();

                _context.Chips.Add(chip);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetChip), new { id = chip.ChipId }, chip);
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }

        private bool ChipExists(int id)
        {
            return _context.Chips.Any(e => e.ChipId == id);
        }

        // GET: api/Chips/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Chip>>> GetActiveChips()
        {
            var chips = await _context.Chips
                .Where(c => c.IsActive)
                .Select(c => new Chip
                {
                    ChipId = c.ChipId,
                    ChipNumber = c.ChipNumber,
                    MemberId = c.MemberId,
                    IsActive = c.IsActive,
                    MemberFullName = _context.Members
                                       .Where(m => m.MemberId == c.MemberId)
                                       .Select(m => m.FirstName + " " + m.LastName)
                                       .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(chips);
        }

        // GET: api/Chips/inactive
        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<Chip>>> GetInactiveChips()
        {
            var chips = await _context.Chips
                .Where(c => !c.IsActive)
                .Select(c => new Chip
                {
                    ChipId = c.ChipId,
                    ChipNumber = c.ChipNumber,
                    MemberId = c.MemberId,
                    IsActive = c.IsActive,
                    MemberFullName = _context.Members
                                       .Where(m => m.MemberId == c.MemberId)
                                       .Select(m => m.FirstName + " " + m.LastName)
                                       .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(chips);
        }

        // DELETE: api/Chips/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChip(int id)
        {
            var chip = await _context.Chips.FindAsync(id);
            if (chip == null)
            {
                return NotFound();
            }

            _context.Chips.Remove(chip);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Chips/active_with_name
        [HttpGet("active_with_name")]
        public async Task<ActionResult<IEnumerable<Chip>>> GetActiveChipsWithName()
        {
            var activeChipsWithNames = await _context.Chips
                .Where(c => c.IsActive)
                .Select(c => new
                {
                    c.ChipId,
                    c.ChipNumber,
                    c.MemberId,
                    c.IsActive,
                    MemberFullName = _context.Members
                                       .Where(m => m.MemberId == c.MemberId)
                                       .Select(m => m.FirstName + " " + m.LastName) 
                                       .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(activeChipsWithNames);
        }
    }
}
