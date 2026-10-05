using System;
using System.Windows;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para Redefinir_senha.xaml
    /// </summary>
    public partial class Redefinir_senha : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        public int idLogado;

        public Redefinir_senha(int idUsuario)
        {
            InitializeComponent();
            idLogado = idUsuario;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string novaSenha = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(novaSenha))
            {
                MessageBox.Show("Please enter a new password.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (novaSenha.Length < 8)
            {
                MessageBox.Show("The password must contain at least 8 characters.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtPassword.Clear();
                return;
            }

            string novaSenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        UPDATE usuarios
                        SET senha = @senha
                        WHERE id = @id";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@senha", novaSenhaHash);
                        command.Parameters.AddWithValue("@id", idLogado);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Password changed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                MainWindow abrir = new MainWindow(idLogado);
                abrir.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing password:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtPassword.Clear();
            }
        }
    }
}