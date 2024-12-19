using ServiceLayer.Data;
using ServiceLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Services
{
    public class ExamOrderProductService
    {
        public static readonly ExamContext _context = new();
        public async Task<List<ExamOrderProduct>> GetProductsInOrder(int orderId)
        {
            return await _context.ExamOrderProducts.Where(o => o.OrderId == orderId).ToListAsync();
        }

        public async Task<string> GetProductAmountInOrderWithArticle(int orderId, string article)
        {
            var amount = await _context.ExamOrderProducts.Where(o => o.OrderId == orderId && o.ProductArticleNumber == article).Select(o => o.Amount).FirstOrDefaultAsync();
            return amount.ToString();
        }

        public decimal? GetDiscountOrder(int orderId)
        {
            return _context.ExamOrderProducts
                .Include(o => o.ProductArticleNumberNavigation) 
                .Include(o => o.Order) 
                .Where(o => o.Order.OrderId == orderId)
                .Select(o => new
                {
                    Discount = (o.ProductArticleNumberNavigation.ProductCost - o.ProductArticleNumberNavigation.ProductCost * (100 - o.ProductArticleNumberNavigation.ProductDiscountAmount) / 100) * o.Amount
                })
                .Sum(x => x.Discount);
        }

        public async Task AddOrderProductAsync(int orderID, string productArticleNumber, int amount)
        {
            var orderProduct = new ExamOrderProduct() { OrderId = orderID, ProductArticleNumber = productArticleNumber, Amount = (short)amount };
            await _context.ExamOrderProducts.AddAsync(orderProduct);
            await _context.SaveChangesAsync();
        }

        public decimal? GetSumOrder(int orderId)
        {
            return _context.ExamOrderProducts
                .Include(o => o.ProductArticleNumberNavigation) 
                .Include(o => o.Order) 
                .Where(o => o.Order.OrderId == orderId)
                .Select(o => new
                {
                    Cost = o.ProductArticleNumberNavigation.ProductCost * (100 - o.ProductArticleNumberNavigation.ProductDiscountAmount) / 100 * o.Amount
                })
                .Sum(x => x.Cost);
        }
    }
}
