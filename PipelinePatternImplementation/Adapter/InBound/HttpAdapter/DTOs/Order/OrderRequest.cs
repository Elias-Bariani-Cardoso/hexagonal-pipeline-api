using System.ComponentModel.DataAnnotations;

namespace Adapter.InBound.HttpAdapter.DTOs.Order
{
    public class OrderRequest
    {
        [Required(ErrorMessage = "O campo orderId é obrigatório.")]
        public string? orderId { get; set; }
        [Required(ErrorMessage = "O campo amount é obrigatório.")]
        public double? amount { get; set; }
        [Required(ErrorMessage = "O campo status é obrigatório.")]
        public string? status { get; set; }
    }
}
