using System.ComponentModel.DataAnnotations;

namespace ProductApi.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nama produk wajib diisi")]
        [MaxLength(100, ErrorMessage = "Nama produk maksimal 100 karakter")]
        public string Name { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Harga tidak boleh negatif")]
        public decimal Price { get; set; }
    }
}
