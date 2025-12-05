namespace Vista
{
    // Modelo simple de producto para enlazar con el formulario
    public class Product
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }
}
