using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Datos.Conecction
{
    public class ConnectionBD
    {
        private static string host_name = "(local)";

        private static string cadenaConexion = "Data Source=" + host_name + ";Initial Catalog=GestorStock;Integrated Security=True;TrustServerCertificate=True;";

        public static SqlConnection ObtenerConexion()
        {
            return new SqlConnection(cadenaConexion);
        }
    }
}
