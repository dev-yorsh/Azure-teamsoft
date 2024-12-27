namespace Company.Function.Jpr.Models
{
    public class Producto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? Categoria { get; set; }
        public string? Marca { get; set; }
    }
}