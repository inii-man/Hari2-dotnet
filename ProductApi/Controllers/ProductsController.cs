using Microsoft.AspNetCore.Mvc;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET /api/products
        // Mendukung: search, filter harga, pagination
        // Contoh: /api/products?search=laptop&minPrice=100&maxPrice=5000&page=1&pageSize=10
        // ============================================================
        [HttpGet]
        public IActionResult Get(
            [FromQuery] string? search,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = _context.Products.AsQueryable();

            // Search by name (bonus challenge)
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
            }

            // Filter by price range (bonus challenge)
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Total count sebelum pagination (untuk metadata)
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Pagination (bonus challenge)
            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Return data + metadata pagination
            return Ok(new
            {
                data = products,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalItems = totalItems,
                    totalPages = totalPages
                }
            });
        }

        // ============================================================
        // GET /api/products/{id}
        // Ambil satu produk berdasarkan ID
        // ============================================================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null)
                return NotFound(new { message = $"Product dengan ID {id} tidak ditemukan" });

            return Ok(product);
        }

        // ============================================================
        // POST /api/products
        // Tambah produk baru
        // ============================================================
        [HttpPost]
        public IActionResult Create(Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Products.Add(product);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }

        // ============================================================
        // PUT /api/products/{id}
        // Update produk yang sudah ada
        // ============================================================
        [HttpPut("{id}")]
        public IActionResult Update(int id, Product product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var data = _context.Products.Find(id);

            if (data == null)
                return NotFound(new { message = $"Product dengan ID {id} tidak ditemukan" });

            data.Name = product.Name;
            data.Price = product.Price;
            _context.SaveChanges();

            return Ok(data);
        }

        // ============================================================
        // DELETE /api/products/{id}
        // Hapus produk berdasarkan ID
        // ============================================================
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var data = _context.Products.Find(id);

            if (data == null)
                return NotFound(new { message = $"Product dengan ID {id} tidak ditemukan" });

            _context.Products.Remove(data);
            _context.SaveChanges();

            return Ok(new { message = $"Product '{data.Name}' berhasil dihapus" });
        }
    }
}
