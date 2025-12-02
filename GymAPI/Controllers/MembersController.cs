using GymAPI.Data;
using GymAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        private readonly GymDbContext _context;

        public MembersController(GymDbContext context)
        {
            _context = context;
        }

        // GET: api/Members
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Member>>> GetMembers()
        {
            return await _context.Members.ToListAsync();
        }

        // GET: api/Members/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Member>> GetMember(int id)
        {
            var member = await _context.Members.FindAsync(id);

            if (member == null)
            {
                return NotFound();
            }

            return member;
        }

        // PUT: api/Members/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMember(int id, Member member)
        {
            if (id != member.MemberId)
            {
                return BadRequest();
            }

            _context.Entry(member).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MemberExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return !MemberExists(id) ? NotFound(id) : StatusCode(StatusCodes.Status500InternalServerError);
                }
            }

            return NoContent();
        }

        // POST: api/Members
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Member>> PostMember(Member member)
        {
            _context.Members.Add(member);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetMember", new { id = member.MemberId }, member);
        }

        // DELETE: api/Members/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(int id)
        {
            var member = await _context.Members.FindAsync(id);
            if (member == null)
            {
                return NotFound();
            }

            _context.Members.Remove(member);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.MemberId == id);
        }

        // GET: api/Members/available_for_chip
        [HttpGet("available_for_chip")]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetMembersAvailableForChip()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var members = await _context.Members
                .Where(m => _context.Memberships
                    .Any(ms => ms.MemberId == m.MemberId && ms.EndDate >= today))

                .Where(m => !_context.Chips
                    .Any(c => c.MemberId == m.MemberId))

                .Select(m => new MemberDto
                {
                    MemberId = m.MemberId,
                    FullName = m.FirstName + " " + m.LastName + ", " + m.PhoneNumber
                })
                .ToListAsync();

            return Ok(members);
        }
    }
}
