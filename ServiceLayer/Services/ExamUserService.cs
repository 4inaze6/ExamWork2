using ServiceLayer.Data;
using ServiceLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Services
{
    public class ExamUserService
    {
        public static readonly ExamContext _context = new();

        public async Task<ExamUser?> GetUserByLoginAndPasswordAsync(string login, string password)
        {
            return await _context.ExamUsers.FirstOrDefaultAsync(u => u.UserLogin == login && u.UserPassword == password);
        }

        public async Task<string?> GetUserFullNameWithOrderIdAsync(int orderId)
        {
            var user = await _context.ExamOrders
                .Where(o => o.OrderId == orderId)
                .Select(u => new
                {
                    FullName = $"{u.User.UserSurname} {u.User.UserName} {u.User.UserPatronymic}"
                }).FirstOrDefaultAsync();
            return $"{user?.FullName}";
        }
    }
}
