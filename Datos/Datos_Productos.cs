using Datos.Conecction;
using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Datos
{
    public class Datos_Productos
    {
        public static DataTable ListarProductos()
        {
            DataTable tabla = new DataTable();

            try
            {
                using (SqlConnection conexion = ConnectionBD.ObtenerConexion())
                {
                    conexion.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_TraerProductos", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(tabla);
                        }
                    }
                }

                return tabla;
            }
            catch (SqlException ex)
            {
                // Dejar que la excepción suba con información detallada
                throw new Exception("Error SQL al listar productos: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al listar productos: " + ex.Message, ex);
            }
        }

        public static int InsertarProducto(string nombre, decimal precio, int stock)
        {
            try
            {
                using (SqlConnection conexion = ConnectionBD.ObtenerConexion())
                {
                    conexion.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_InsertarProducto", conexion))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Precio", precio);
                        cmd.Parameters.AddWithValue("@Stock", stock);

                        // parámetro de salida para el id generado (si el SP lo devuelve)
                        var pId = new SqlParameter("@Id", System.Data.SqlDbType.Int) { Direction = ParameterDirection.Output };
                        cmd.Parameters.Add(pId);

                        cmd.ExecuteNonQuery();

                        int id = 0;
                        if (pId.Value != DBNull.Value)
                            id = Convert.ToInt32(pId.Value);

                        return id;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error SQL al insertar producto: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Error inesperado al insertar producto: " + ex.Message, ex);
            }
        }
    }
}
