using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using BCrypt.Net; // Asegúrate de importar la biblioteca BCrypt.Net

namespace autenticacion
{
    public partial class Form1 : Form
    {
        // Cadena de conexión a la base de datos
        string connectionString = "Server=DESKTOP-0NF1K3O\\SQLEXPRESS;Database=autenticacion;User ID=sa;Password=";

        public Form1()
        {
            InitializeComponent();
        }

        private void btnIniciarSesion_Click(object sender, EventArgs e)
        {
            string nombreUsuario = txtNombreUsuario.Text;
            string contrasena = txtContrasena.Text;

            if (nombreUsuario == "admin" && contrasena == "admin123")
            {
                // Si las credenciales son las del administrador, abre la ventana FormAdmin.
                FormAdmin formAdmin = new FormAdmin();
                formAdmin.Show();
                this.Hide(); // Oculta el formulario actual (Form1).
            }
            else if (AutenticarUsuario(nombreUsuario, contrasena))
            {
                MessageBox.Show("Inicio de sesión exitoso");
                // Aquí puedes continuar con la lógica de tu aplicación después del inicio de sesión exitoso.
            }
            else
            {
                MessageBox.Show("Inicio de sesión fallido. Debe contactarse con el Administrador.");
            }
        }

        private bool AutenticarUsuario(string nombreUsuario, string contrasena)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT NombreUsuario, Contraseña, Habilitado, IntentosFallidosInicioSesion FROM Usuarios WHERE NombreUsuario = @NombreUsuario";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string hashedPassword = reader["Contraseña"].ToString();
                            bool usuarioHabilitado = (bool)reader["Habilitado"];
                            int intentosFallidos = reader["IntentosFallidosInicioSesion"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IntentosFallidosInicioSesion"]);

                            if (!usuarioHabilitado)
                            {
                                return false;
                            }

                            if (BCrypt.Net.BCrypt.Verify(contrasena, hashedPassword))
                            {
                                intentosFallidos = 0;
                                ActualizarIntentosFallidos(nombreUsuario, intentosFallidos);
                                string nombreEmpresa = ObtenerNombreEmpresaAsociada(nombreUsuario);
                                MostrarNombreEmpresaEnFormulario(nombreEmpresa);
                                return true;
                            }
                            else
                            {
                                intentosFallidos++;
                                ActualizarIntentosFallidos(nombreUsuario, intentosFallidos);

                                if (intentosFallidos >= 3)
                                {
                                    BloquearUsuario(nombreUsuario);
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }

        private void ActualizarIntentosFallidos(string nombreUsuario, int intentosFallidos)
        {
            // Implementa la actualización de los intentos fallidos en la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Usuarios SET IntentosFallidosInicioSesion = @IntentosFallidos WHERE NombreUsuario = @NombreUsuario";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@IntentosFallidos", intentosFallidos);
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void BloquearUsuario(string nombreUsuario)
        {
            // Implementa el bloqueo del usuario en la base de datos
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE Usuarios SET Habilitado = 0 WHERE NombreUsuario = @NombreUsuario";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string ObtenerNombreEmpresaAsociada(string nombreUsuario)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT NombreEmpresa FROM ConfiguracionEmpresa WHERE IDEmpresa = (SELECT IDEmpresa FROM Usuarios WHERE NombreUsuario = @NombreUsuario)";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString() : string.Empty;
                }
            }
        }

        private void MostrarNombreEmpresaEnFormulario(string nombreEmpresa)
        {
            labelNombreEmpresa.Text = "Nombre Empresa: " + nombreEmpresa;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario actual (Form1).

        }
    }
}

