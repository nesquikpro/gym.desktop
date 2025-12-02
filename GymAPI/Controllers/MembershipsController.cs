using GymAPI.Data;
using GymAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembershipsController : ControllerBase
    {
        private readonly GymDbContext _context;

        public MembershipsController(GymDbContext context)
        {
            _context = context;
        }

        // GET: api/Memberships
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Membership>>> GetMemberships()
        {
            return await _context.Memberships.ToListAsync();
        }

        // GET: api/Memberships/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Membership>> GetMembership(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);

            if (membership == null)
            {
                return NotFound();
            }

            return membership;
        }

        // PUT: api/Memberships/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMembership(int id, Membership updated)
        {
            if (id != updated.MembershipId)
                return BadRequest();

            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
                return NotFound();

            int prevFrozenDays = membership.TotalFrozenDays;

            membership.StartDate = updated.StartDate;
            membership.IsPaidByCard = updated.IsPaidByCard;
            membership.PaymentQRCode = updated.PaymentQRCode;

            bool freezeChanged =
                membership.IsFrozen != updated.IsFrozen ||
                membership.FreezeStartDate != updated.FreezeStartDate ||
                membership.FreezeEndDate != updated.FreezeEndDate;

            if (freezeChanged)
            {
                membership.EndDate = membership.EndDate.AddDays(-membership.TotalFrozenDays);

                if (updated.IsFrozen &&
                    updated.FreezeStartDate.HasValue &&
                    updated.FreezeEndDate.HasValue)
                {
                    int newFreezeDays = (updated.FreezeEndDate.Value.DayNumber
                     - updated.FreezeStartDate.Value.DayNumber) + 1;

                    membership.IsFrozen = true;
                    membership.FreezeStartDate = updated.FreezeStartDate;
                    membership.FreezeEndDate = updated.FreezeEndDate;

                    membership.TotalFrozenDays = newFreezeDays;
                    membership.EndDate = membership.EndDate.AddDays(newFreezeDays);
                }
                else
                {
                    membership.IsFrozen = false;
                    membership.FreezeStartDate = null;
                    membership.FreezeEndDate = null;
                    membership.TotalFrozenDays = 0;
                }
            }

            _context.Entry(membership).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                return StatusCode(500, "Ошибка обновления абонемента.");
            }

            return NoContent();
        }

        // POST: api/Memberships
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Membership>> PostMembership(Membership membership)
        {
            var existingMember = await _context.Members.FindAsync(membership.MemberId);
            if (existingMember == null)
            {
                return BadRequest("Member not found.");
            }

            _context.Memberships.Add(membership);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMembership", new { id = membership.MembershipId }, membership);
        }

        // DELETE: api/Memberships/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMembership(int id)
        {
            var membership = await _context.Memberships.FindAsync(id);
            if (membership == null)
            {
                return NotFound();
            }

            var chips = _context.Chips.Where(c => c.MemberId == membership.MemberId);
            _context.Chips.RemoveRange(chips);

            _context.Memberships.Remove(membership);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MembershipExists(int id)
        {
            return _context.Memberships.Any(e => e.MembershipId == id);
        }
    }
}
