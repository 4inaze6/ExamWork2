using ServiceLayer.Data;
using ServiceLayer.DTOs;
using ServiceLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Services
{
    public class ExamOrderService
    {
        private static readonly ExamContext _context = new();

        public async Task<OrderSummaryDTO> GetOrderByIdAsync(int id)
        {
            var order = await _context.ExamOrders.Include(p => p.ExamOrderProducts).ThenInclude(op => op.ProductArticleNumberNavigation)
            .Select(g => new OrderSummaryDTO
            {
                OrderID = g.OrderId,
                UserID = g.UserId,
                OrderStatus = g.OrderStatus,
                OrderDate = g.OrderDate,
                OrderDeliveryDate = g.OrderDeliveryDate,
                OrderPickupPoint = g.OrderPickupPoint,
                OrderPickupCode = g.OrderPickupCode,
                TotalCost = g.ExamOrderProducts
                    .Sum(op => op.ProductArticleNumberNavigation.ProductCost * (100 - op.ProductArticleNumberNavigation.ProductDiscountAmount) / 100 * op.Amount)
            }).FirstOrDefaultAsync(g => g.OrderID == id);

            return order;
        }

        public async Task UpdateExamOrderDeliveryDate(DateTime newDeliveryDate, int orderId)
        {
            var updatingOrder = await _context.ExamOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            updatingOrder.OrderDeliveryDate = newDeliveryDate;
            await _context.SaveChangesAsync();
        }

        public async Task<List<OrderSummaryDTO>> GetOrdersAsync()
        {
            var orders = await _context.ExamOrders.Include(p => p.ExamOrderProducts).ThenInclude(op => op.ProductArticleNumberNavigation)
            .Select(g => new OrderSummaryDTO
            {
                OrderID = g.OrderId,
                UserID = g.UserId,
                OrderStatus = g.OrderStatus,
                OrderDate = g.OrderDate,
                OrderDeliveryDate = g.OrderDeliveryDate,
                OrderPickupPoint = g.OrderPickupPoint,
                OrderPickupCode = g.OrderPickupCode,
                TotalCost = g.ExamOrderProducts
                    .Sum(op => op.ProductArticleNumberNavigation.ProductCost * (100 - op.ProductArticleNumberNavigation.ProductDiscountAmount) / 100 * op.Amount)
            }).ToListAsync();

            return orders;
        }

        public async Task UpdateExamOrderStatus(string newStatus, int orderId)
        {
            var updatingOrder = await _context.ExamOrders.FirstOrDefaultAsync(o => o.OrderId == orderId);
            updatingOrder.OrderStatus = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task<List<int>> GetExistingPickupCodesAsync()
        {
            return await _context.ExamOrders.Select(o => o.OrderPickupCode).ToListAsync();
        }

        public async Task<List<ExamOrder>> GetOrdersByUserIdAsync(int id)
        {
            return await _context.ExamOrders.Where(o => o.UserId == id).ToListAsync();
        }

        public async Task AddExamOrderAsync(int? userID, string orderStatus, DateTime orderDate, DateTime orderDeliveryDate, int orderPickupPoint, int orderPickupCode)//создание нового заказа в БД
        {
            var newOrder = new ExamOrder() { UserId = userID != 0 ? userID : null, OrderStatus = orderStatus, OrderDate = orderDate, OrderDeliveryDate = orderDeliveryDate, OrderPickupPoint = orderPickupPoint, OrderPickupCode = orderPickupCode };
            await _context.ExamOrders.AddAsync(newOrder);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLastOrderIDAsync()
        {
            return await _context.ExamOrders.MaxAsync(o => o.OrderId);
        }
    }
}
