using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using BCrypt.Net; // Asegúrate de importar la biblioteca BCrypt.Net

namespace autenticacion
{
    public partial class FormAdmin : Form
    {
        string connectionString = "Server=DESKTOP-0NF1K3O\\SQLEXPRESS;Database=autenticacion;User ID=sa;Password=";

        public FormAdmin()
        {
            InitializeComponent();
            CargarNombresEmpresas(); // Carga los nombres de empresas al iniciar el formulario
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                string nuevoNombreUsuario = txtNuevoNombreUsuario.Text;
                string nuevaContrasena = txtNuevaContrasena.Text;
                bool nuevaHabilitacion = chkNuevoHabilitado.Checked;

                // Obtener el nombre de la empresa seleccionada en el ListBox
                string nombreEmpresa = listBoxEmpresas.SelectedItem?.ToString();

                if (string.IsNullOrEmpty(nombreEmpresa))
                {
                    MessageBox.Show("Selecciona una empresa válida.");
                    return;
                }

                DateTime fechaActual = DateTime.Now;
                int idEmpresa = ObtenerIDEmpresaPorNombre(nombreEmpresa);

                // Generar una sal aleatoria
                string salt = BCrypt.Net.BCrypt.GenerateSalt(12);

                // Cifrar la contraseña con la sal
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(nuevaContrasena, salt);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string queryUsuario = "INSERT INTO Usuarios (NombreUsuario, Contraseña, Habilitado, UltimaFechaInicioSesion, IDEmpresa) " +
                                          "VALUES (@NombreUsuario, @Contraseña, @Habilitado, @UltimaFechaInicioSesion, @IDEmpresa)";
                    using (SqlCommand cmdUsuario = new SqlCommand(queryUsuario, connection))
                    {
                        cmdUsuario.Parameters.AddWithValue("@NombreUsuario", nuevoNombreUsuario);
                        cmdUsuario.Parameters.AddWithValue("@Contraseña", hashedPassword); // Almacenar la contraseña cifrada
                        cmdUsuario.Parameters.AddWithValue("@Habilitado", nuevaHabilitacion);
                        cmdUsuario.Parameters.AddWithValue("@UltimaFechaInicioSesion", fechaActual);
                        cmdUsuario.Parameters.AddWithValue("@IDEmpresa", idEmpresa);
                        cmdUsuario.ExecuteNonQuery();
                    }

                    string queryConfiguracionEmpresa = "UPDATE ConfiguracionEmpresa SET NombreEmpresa = @NombreEmpresa WHERE IDEmpresa = @IDEmpresa";
                    using (SqlCommand cmdConfiguracionEmpresa = new SqlCommand(queryConfiguracionEmpresa, connection))
                    {
                        cmdConfiguracionEmpresa.Parameters.AddWithValue("@NombreEmpresa", nombreEmpresa);
                        cmdConfiguracionEmpresa.Parameters.AddWithValue("@IDEmpresa", idEmpresa);
                        cmdConfiguracionEmpresa.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Datos guardados exitosamente.");
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar los datos: " + ex.Message);
            }
        }

        // Método para obtener la contraseña cifrada de la base de datos
        private string ObtenerContrasenaCifradaDelUsuario(string nombreUsuario)
        {
            string hashedPassword = null; // Inicializa con null o un valor predeterminado si es necesario

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT Contraseña FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    object result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        hashedPassword = result.ToString(); // Aquí obtienes la contraseña cifrada desde la base de datos
                    }
                }
            }

            return hashedPassword;
        }
        


        private int ObtenerIDEmpresaPorNombre(string nombreEmpresa)
        {
            int idEmpresa = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT IDEmpresa FROM ConfiguracionEmpresa WHERE NombreEmpresa = @NombreEmpresa";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@NombreEmpresa", nombreEmpresa);
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        idEmpresa = Convert.ToInt32(result);
                    }
                }
            }

            return idEmpresa;
        }

        private void CargarNombresEmpresas()
        {
            listBoxEmpresas.Items.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT NombreEmpresa FROM ConfiguracionEmpresa";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nombreEmpresa = reader["NombreEmpresa"].ToString();
                            listBoxEmpresas.Items.Add(nombreEmpresa);
                        }
                    }
                }
            }
        }

        private void LimpiarCampos()
        {
            txtNuevoNombreUsuario.Clear();
            txtNuevaContrasena.Clear();
            chkNuevoHabilitado.Checked = false;
        }

        private void btnVolver_Click(object sender, EventArgs e)
        {
            // Oculta el formulario actual (FormAdmin)
            this.Hide();

            // Muestra el formulario Form1 (puedes reemplazar "form1" con el nombre de tu instancia de Form1 si es diferente)
            Form1 form1 = new Form1();
            form1.Show();

        }
    }
}



