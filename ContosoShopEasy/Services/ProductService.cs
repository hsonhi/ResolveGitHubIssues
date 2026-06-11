using System.Text.RegularExpressions;
using ContosoShopEasy.Models;
using ContosoShopEasy.Data;

namespace ContosoShopEasy.Services
{
    public class ProductService
    {
        private readonly ProductRepository _productRepository;

        public ProductService(ProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public List<Product> GetAllProducts()
        {
            return _productRepository.GetAllProducts();
        }

        public Product? GetProductById(int id)
        {
            return _productRepository.GetProductById(id);
        }

        public List<Product> GetProductsByCategory(int categoryId)
        {
            return _productRepository.GetProductsByCategory(categoryId);
        }

        public List<Product> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<Product>();
            }

            string sanitizedSearchTerm = SanitizeSearchTerm(searchTerm);
            if (string.IsNullOrWhiteSpace(sanitizedSearchTerm))
            {
                return new List<Product>();
            }

            return _productRepository.SearchProducts(sanitizedSearchTerm);
        }

        private static string SanitizeSearchTerm(string searchTerm)
        {
            string sanitized = searchTerm.Trim();
            sanitized = Regex.Replace(sanitized, @"\s+", " ");

            // Remove SQL comment markers, quotes, and other characters not needed for search.
            sanitized = Regex.Replace(sanitized, @"(--|/\*|\*/|['\"";\\=])", string.Empty);
            sanitized = Regex.Replace(sanitized, "[\u0000-\u001F\u007F]+", string.Empty);

            return sanitized.Trim();
        }

        public List<Product> GetTopRatedProducts(int count = 10)
        {
            return _productRepository.GetAllProducts()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.Rating)
                .Take(count)
                .ToList();
        }

        public List<Product> GetFeaturedProducts(int count = 5)
        {
            return _productRepository.GetAllProducts()
                .Where(p => p.IsActive && p.StockQuantity > 0)
                .OrderByDescending(p => p.ReviewCount)
                .Take(count)
                .ToList();
        }

        public bool IsProductInStock(int productId, int quantity = 1)
        {
            var product = _productRepository.GetProductById(productId);
            return product != null && product.StockQuantity >= quantity;
        }

        public bool UpdateStock(int productId, int quantityChange)
        {
            var product = _productRepository.GetProductById(productId);
            if (product != null)
            {
                product.StockQuantity += quantityChange;
                product.LastModified = DateTime.UtcNow;
                return true;
            }
            return false;
        }
    }
}