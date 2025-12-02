using GymAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace GymAPI.Services
{
    public class ChipNumberGenerator
    {
        private readonly GymDbContext _context;

        public ChipNumberGenerator(GymDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync()
        {
            var conn = _context.Database.GetDbConnection();
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT NEXT VALUE FOR ChipNumberSeq";

            var result = await cmd.ExecuteScalarAsync();
            long nextValue = Convert.ToInt64(result);

            return $"CHIP-{nextValue:D3}";
        }
    }
}
