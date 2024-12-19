using ServiceLayer.Data;
using ServiceLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Services
{
    public class ExamPickupPointService
    {
        private readonly ExamContext _context = new();

        public async Task<List<ExamPickupPoint>> GetPickupPointsAsync()
        {
            return await _context.ExamPickupPoints.ToListAsync();
        }
    }
}
