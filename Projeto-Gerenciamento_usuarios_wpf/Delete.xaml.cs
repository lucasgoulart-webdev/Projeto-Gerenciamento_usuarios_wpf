using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para Delete.xaml
    /// </summary>
    public partial class Delete : Window
    {
        private string conexao =
            "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        // Dados do usuário que está conectado
        private int usuarioLogadoId;
        private string tipoUsuarioLogado;

        public Delete(int idUsuarioLogado, string tipoUsuario)
        {
            InitializeComponent();

            usuarioLogadoId = idUsuarioLogado;
            tipoUsuarioLogado = tipoUsuario;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();

            // Verifica se quem está tentando excluir é administrador
            if (tipoUsuarioLogado != "Admin")
            {
                MessageBox.Show(
                    "Apenas administradores podem excluir usuários.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            // E-mail obrigatório
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Digite o e-mail do usuário que deseja excluir.",
                    "Exclusão",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(conexao))
                {
                    connection.Open();

                    // Procura o usuário pelo e-mail
                    string buscar = @"
                        SELECT
                            id,
                            nome_completo,
                            tipo_usuario
                        FROM usuarios
                        WHERE email = @email
                        LIMIT 1";

                    int idUsuario;
                    string nomeUsuario;
                    string tipoUsuario;

                    using (MySqlCommand command =
                        new MySqlCommand(buscar, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@email", email);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    "Nenhum usuário foi encontrado com esse e-mail.",
                                    "Exclusão",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            idUsuario =
                                Convert.ToInt32(reader["id"]);

                            nomeUsuario =
                                reader["nome_completo"].ToString();

                            tipoUsuario =
                                reader["tipo_usuario"].ToString();
                        }
                    }

                    // Impede o administrador de excluir a própria conta
                    if (idUsuario == usuarioLogadoId)
                    {
                        MessageBox.Show(
                            "O administrador não pode excluir a própria conta enquanto estiver conectado.",
                            "Exclusão não permitida",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    // Verifica se o usuário que será excluído é administrador
                    if (tipoUsuario == "Admin")
                    {
                        string contarAdmins = @"
                            SELECT COUNT(*)
                            FROM usuarios
                            WHERE tipo_usuario = 'Admin'";

                        int quantidadeAdmins;

                        using (MySqlCommand command =
                            new MySqlCommand(
                                contarAdmins,
                                connection))
                        {
                            quantidadeAdmins =
                                Convert.ToInt32(
                                    command.ExecuteScalar());
                        }

                        // Impede a exclusão do último administrador
                        if (quantidadeAdmins <= 1)
                        {
                            MessageBox.Show(
                                "O último administrador do sistema não pode ser excluído.",
                                "Exclusão não permitida",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            return;
                        }
                    }

                    // Confirmação obrigatória antes da exclusão
                    MessageBoxResult confirmacao =
                        MessageBox.Show(
                            "Deseja realmente excluir este usuário?\n\n" +
                            nomeUsuario +
                            "\n" +
                            email,
                            "Confirmar exclusão",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question);

                    if (confirmacao != MessageBoxResult.Yes)
                    {
                        return;
                    }

                    // Exclui o usuário
                    string excluir = @"
                        DELETE FROM usuarios
                        WHERE id = @id";

                    using (MySqlCommand command =
                        new MySqlCommand(excluir, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id", idUsuario);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        "Usuário excluído com sucesso!",
                        "Exclusão",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    TxtEmail.Clear();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Erro no banco de dados:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao excluir usuário:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
