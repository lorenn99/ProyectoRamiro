using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Data;
using Logica;

namespace Vista
{
    public partial class Form1 : Form
    {
        private BindingList<Product> productos = new BindingList<Product>();
        private BindingSource bs = new BindingSource();
        private int nextId = 1;

        public Form1()
        {
            InitializeComponent();

            // event handlers
            this.Load += Form1_Load;
            btnAgregar.Click += BtnAgregar_Click;
            btnActualizar.Click += BtnActualizar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            txtBuscar.TextChanged += TxtBuscar_TextChanged;
            dgvProductos.SelectionChanged += DgvProductos_SelectionChanged;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Cargar productos desde la capa de lógica (que consume Datos y el stored procedure)
            LoadProductosFromDb();

            // Deshabilitar CRUD en UI ya que la lista proviene de la BD (evita inconsistencias)
            btnAgregar.Enabled = false;
            btnActualizar.Enabled = false;
            btnEliminar.Enabled = false;
        }

        private void LoadProductosFromDb()
        {
            try
            {
                var logica = new Logica_Productos();
                DataTable dt = logica.ListarProductos();
                if (dt == null)
                {
                    MessageBox.Show("No se pudo obtener la lista de productos desde la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                dgvProductos.DataSource = dt;

                // Ajustes visuales si la tabla tiene al menos 4 columnas esperadas: id, nombre, precio, stock
                if (dgvProductos.Columns.Count >= 4)
                {
                    dgvProductos.Columns[0].HeaderText = "ID";
                    dgvProductos.Columns[1].HeaderText = "Nombre";
                    dgvProductos.Columns[2].HeaderText = "Precio";
                    dgvProductos.Columns[3].HeaderText = "Stock";
                    dgvProductos.Columns[0].Width = 60;
                }
            }
            catch (Exception ex)
            {
                // Mostrar detalles completos para diagnóstico
                MessageBox.Show("Error al cargar productos:\n" + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Ayuda rápida
                MessageBox.Show("Compruebe la cadena de conexión en Datos/ConnectionBD.cs y que el stored procedure 'sp_ListarProductos' exista en la base de datos GestorStock.", "Sugerencia", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnAgregar_Click(object sender, EventArgs e)
        {
            // Abrir formulario de agregar como diálogo
            using (var form = new frmAgregarProducto())
            {
                var res = form.ShowDialog(this);
                if (res == DialogResult.OK)
                {
                    // recargar lista desde BD
                    LoadProductosFromDb();

                    // si el formulario devolvió un ID, intentar seleccionar esa fila
                    if (form.NewId > 0 && dgvProductos.DataSource is DataTable dt)
                    {
                        // buscar fila con esa id en la primera columna o por nombre de columna "Id"/"ID"
                        string idCol = null;
                        if (dt.Columns.Contains("Id")) idCol = "Id";
                        else if (dt.Columns.Contains("ID")) idCol = "ID";
                        else if (dt.Columns.Count > 0) idCol = dt.Columns[0].ColumnName;

                        if (!string.IsNullOrEmpty(idCol))
                        {
                            var rows = dt.Select($"[{idCol}] = {form.NewId}");
                            if (rows.Length > 0)
                            {
                                // seleccionar en grid
                                foreach (DataGridViewRow row in dgvProductos.Rows)
                                {
                                    var drv = row.DataBoundItem as DataRowView;
                                    if (drv != null)
                                    {
                                        if (Convert.ToInt32(drv[idCol]) == form.NewId)
                                        {
                                            row.Selected = true;
                                            dgvProductos.FirstDisplayedScrollingRowIndex = row.Index;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BtnActualizar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;

            if (!ValidateInputs(out string nombre, out decimal precio, out int stock)) return;

            // Si el DataSource es DataTable, no hacemos actualización aquí
            if (dgvProductos.DataSource is DataTable)
            {
                MessageBox.Show("La actualización debe realizarse en la base de datos.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Product p = (Product)dgvProductos.CurrentRow.DataBoundItem;
            p.Nombre = nombre;
            p.Precio = precio;
            p.Stock = stock;

            // refrescar binding
            bs.ResetBindings(false);
            ClearInputs();
        }

        private void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;

            if (dgvProductos.DataSource is DataTable)
            {
                MessageBox.Show("La eliminación debe realizarse en la base de datos.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Product p = (Product)dgvProductos.CurrentRow.DataBoundItem;
            var resp = MessageBox.Show($"Eliminar '{p.Nombre}'?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (resp == DialogResult.Yes)
            {
                productos.Remove(p);
                ClearInputs();
            }
        }

        private void TxtBuscar_TextChanged(object sender, EventArgs e)
        {
            // Si estamos mostrando la tabla desde BD, usar filtro sobre DataTable
            if (dgvProductos.DataSource is DataTable dt)
            {
                string q = txtBuscar.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(q))
                {
                    dgvProductos.DataSource = dt;
                }
                else
                {
                    try
                    {
                        // Crear vista filtrada si existe columna Nombre (case-insensitive)
                        string colName = dt.Columns.Cast<DataColumn>().FirstOrDefault(c => c.ColumnName.ToLower().Contains("nombre"))?.ColumnName;
                        if (!string.IsNullOrEmpty(colName))
                        {
                            DataView dv = new DataView(dt);
                            dv.RowFilter = $"[{colName}] LIKE '%" + txtBuscar.Text.Replace("'", "''") + "%'";
                            dgvProductos.DataSource = dv;
                        }
                    }
                    catch { }
                }
                return;
            }

            // si es lista en memoria
            string q2 = txtBuscar.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(q2))
            {
                bs.DataSource = productos;
            }
            else
            {
                var filtered = productos.Where(x => x.Nombre != null && x.Nombre.ToLower().Contains(q2)).ToList();
                bs.DataSource = new BindingList<Product>(filtered);
            }
            dgvProductos.Refresh();
        }

        private void DgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductos.CurrentRow == null) return;

            if (dgvProductos.DataSource is DataTable || dgvProductos.DataSource is DataView)
            {
                var drv = dgvProductos.CurrentRow.DataBoundItem as DataRowView;
                if (drv != null)
                {
                    // intentar obtener columnas por nombre común, si no, por índice
                    string nombre = drv.Row.Table.Columns.Contains("Nombre") ? drv["Nombre"].ToString() : (drv.Row.ItemArray.Length > 1 ? drv[1].ToString() : "");
                    string precio = drv.Row.Table.Columns.Contains("Precio") ? drv["Precio"].ToString() : (drv.Row.ItemArray.Length > 2 ? drv[2].ToString() : "");
                    string stock = drv.Row.Table.Columns.Contains("Stock") ? drv["Stock"].ToString() : (drv.Row.ItemArray.Length > 3 ? drv[3].ToString() : "");

                    txtNombre.Text = nombre;
                    txtPrecio.Text = precio;
                    txtStock.Text = stock;
                }
                return;
            }

            Product p = (Product)dgvProductos.CurrentRow.DataBoundItem;
            txtNombre.Text = p.Nombre;
            txtPrecio.Text = p.Precio.ToString();
            txtStock.Text = p.Stock.ToString();
        }

        private bool ValidateInputs(out string nombre, out decimal precio, out int stock)
        {
            nombre = txtNombre.Text.Trim();
            precio = 0;
            stock = 0;

            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingrese el nombre del producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtPrecio.Text.Trim(), out precio) || precio < 0)
            {
                MessageBox.Show("Precio inválido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!int.TryParse(txtStock.Text.Trim(), out stock) || stock < 0)
            {
                MessageBox.Show("Stock inválido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void ClearInputs()
        {
            txtNombre.Text = "";
            txtPrecio.Text = "";
            txtStock.Text = "";
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {
            // cargar productos al inicio y habilitar boton agregar
            LoadProductosFromDb();

            btnAgregar.Enabled = true;
            // mantener actualizacion/eliminacion deshabilitados porque requieren implementacion en BD
            btnActualizar.Enabled = false;
            btnEliminar.Enabled = false;
        }

        private void btnAgregar_Click_1(object sender, EventArgs e)
        {
            // Abrimos el formulario de agregar producto
            frmAgregarProducto frm = new frmAgregarProducto();

            // Mostramos el nuevo formulario
            frm.Show();

            // Cerramos este formulario actual (Form1)
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Abre el formulario para agregar producto
            frmAgregarProducto formularioAgregar = new frmAgregarProducto();
            formularioAgregar.Show();

          
        }
    }
}
