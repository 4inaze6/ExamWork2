using ServiceLayer.Data;
using ServiceLayer.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.Services
{
    public class ExamProductService
    {
        private static readonly ExamContext _context = new();

        public async Task<List<ProductDTO>> GetProductsAsync(string subs, string? sortMethod, decimal minCost, decimal? maxCost, string manuf)//получение списка продуктов из БД с фильтрацией
        {
            var productList = _context.ExamProducts.Include(p => p.ExamOrderProducts)
                .Select(prod => new ProductDTO
                {
                    ProductName = prod.ProductName,
                    ProductArticleNumber = prod.ProductArticleNumber,
                    ProductCategory = prod.ProductCategory,
                    ProductPhoto = prod.ProductPhoto,
                    ProductStatus = prod.ProductStatus,
                    ProductDescription = prod.ProductDescription,
                    ProductCost = prod.ProductCost,
                    ProductDiscountAmount = prod.ProductDiscountAmount,
                    ProductManufacturer = prod.Manufacturer.Name,
                    ProductQuantityInStock = prod.ProductQuantityInStock,
                    TotalCost = prod.ProductDiscountAmount.HasValue ? prod.ProductCost * (100 - prod.ProductDiscountAmount.Value) / 100 : prod.ProductCost
                })
                .Where(p => p.TotalCost >= minCost).Where(p => EF.Functions.Like(p.ProductName, $"%{subs}%"));

            if (maxCost != null)
            {
                productList = productList.Where(p => p.TotalCost <= maxCost);
            }

            if (sortMethod == "asc")
            {
                return await productList.OrderBy(x => x.TotalCost).ToListAsync();
            }

            if (!string.IsNullOrEmpty(manuf))
            {
                productList = productList.Where(p => p.ProductManufacturer == manuf);
            }
            else if (sortMethod == "desc")
            {
                return await productList.OrderByDescending(x => x.TotalCost).ToListAsync();
            }
            return await productList.ToListAsync();
        }

        public async Task<string> GetProductNameByArticleAsync(string article)
        {
            return await _context.ExamProducts.Where(p => p.ProductArticleNumber == article).Select(p => p.ProductName).FirstOrDefaultAsync();
        }

        public async Task<int> GetProductsCountAsync()
        {
            return await _context.ExamProducts.CountAsync();
        }

        public async Task<List<string>> GetManufacturersAsync()
        {
            List<string> manufacturers = ["Все производители", .. await _context.ExamManufacturers.Select(p => p.Name).Distinct().ToListAsync()];
            return manufacturers;
        }
    }
}
