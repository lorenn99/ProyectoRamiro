using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Logica;

namespace Vista
{
    public partial class frmAgregarProducto : Form
    {
        public int NewId { get; private set; }

        public frmAgregarProducto()
        {
            InitializeComponent();

            this.Load += FrmAgregarProducto_Load;
            btnGuardar.Click += BtnGuardar_Click;
            btnCancelar.Click += BtnCancelar_Click;
        }

        private void FrmAgregarProducto_Load(object sender, EventArgs e)
        {
            // establecer valores por defecto si hace falta
            txtPrecio.Text = "0.00";
            txtStock.Text = "0";
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // Validaciones
            string nombre = txtNombre.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MessageBox.Show("Ingrese el nombre del producto.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrecio.Text.Trim(), out decimal precio) || precio < 0)
            {
                MessageBox.Show("Precio inválido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtStock.Text.Trim(), out int stock) || stock < 0)
            {
                MessageBox.Show("Stock inválido.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var logica = new Logica_Productos();
                int newId = logica.InsertarProducto(nombre, precio, stock);

                this.NewId = newId;

                MessageBox.Show("Producto insertado correctamente." + (newId > 0 ? " ID: " + newId.ToString() : ""), "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // cerrar formulario con DialogResult OK para que el padre recargue la lista
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar producto:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
