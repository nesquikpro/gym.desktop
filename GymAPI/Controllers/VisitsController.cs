using GymAPI.Data;
using GymAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitsController : ControllerBase
    {
        private readonly GymDbContext _context;

        public VisitsController(GymDbContext context)
        {
            _context = context;
        }

        // GET: api/Visits
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Visit>>> GetVisits()
        {
            return await _context.Visits.ToListAsync();
        }

        // GET: api/Visits/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Visit>> GetVisit(int id)
        {
            var visit = await _context.Visits.FindAsync(id);

            if (visit == null)
            {
                return NotFound();
            }

            return visit;
        }

        // PUT: api/Visits/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVisit(int id, Visit visit)
        {
            if (id != visit.VisitId)
            {
                return BadRequest();
            }

            _context.Entry(visit).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VisitExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return !VisitExists(id) ? NotFound(id) : StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            return NoContent();
        }

        // POST: api/Visits
        [HttpPost]
        public async Task<ActionResult<Visit>> PostVisit(Visit visit)
        {
            var chip = await _context.Chips.FindAsync(visit.ChipId);
            if (chip == null) return BadRequest("Чип не найден");

            int memberId = chip.MemberId;

            var memberships = await _context.Memberships
                .Where(m => m.MemberId == memberId && m.IsFrozen)
                .ToListAsync();

            foreach (var membership in memberships)
            {
                if (membership.IsFrozen)
                {
                    var remainingFrozenDays = membership.TotalFrozenDays;
                    membership.TotalFrozenDays = 0;
                    membership.EndDate = membership.EndDate.AddDays(-remainingFrozenDays);
                    membership.IsFrozen = false;
                    membership.FreezeStartDate = null;
                    membership.FreezeEndDate = null;
                }
            }

            await _context.SaveChangesAsync();

            _context.Visits.Add(visit);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVisit", new { id = visit.VisitId }, visit);
        }

        // DELETE: api/Visits/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVisit(int id)
        {
            var visit = await _context.Visits.FindAsync(id);
            if (visit == null)
            {
                return NotFound();
            }

            _context.Visits.Remove(visit);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VisitExists(int id)
        {
            return _context.Visits.Any(e => e.VisitId == id);
        }

        [HttpGet("full")]
        public async Task<ActionResult<IEnumerable<VisitFullDto>>> GetFullVisits()
        {
            var result =
                from v in _context.Visits
                join c in _context.Chips on v.ChipId equals c.ChipId
                join m in _context.Members on c.MemberId equals m.MemberId
                select new VisitFullDto
                {
                    VisitId = v.VisitId,
                    VisitDateTime = v.VisitDateTime,
                    ChipNumber = c.ChipNumber,
                    ChipId = c.ChipId,
                    MemberId = m.MemberId,
                    MemberFullName = m.FirstName + " " + m.LastName
                };

            return await result.ToListAsync();
        }
    }
}
