using Datos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logica
{
    public class Logica_Productos
    {
        public DataTable ListarProductos()
        {
            try
            {
                return Datos_Productos.ListarProductos();
            }
            catch (Exception ex)
            {
                // Encapsular excepción con contexto adicional y volver a lanzar
                throw new Exception("Logica_Productos: error al obtener lista de productos. " + ex.Message, ex);
            }
        }

        public int InsertarProducto(string nombre, decimal precio, int stock)
        {
            try
            {
                return Datos_Productos.InsertarProducto(nombre, precio, stock);
            }
            catch (Exception ex)
            {
                throw new Exception("Logica_Productos: error al insertar producto. " + ex.Message, ex);
            }
        }
    }
}
