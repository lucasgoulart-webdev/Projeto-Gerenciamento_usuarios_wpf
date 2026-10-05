```csharp
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
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        // Dados do usuário que está conectado
        private int usuarioLogadoId;

        public Delete(int idUsuarioLogado)
        {
            InitializeComponent();
            usuarioLogadoId = idUsuarioLogado;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();

            // E-mail obrigatório
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Digite o e-mail do usuário que deseja excluir.", "Exclusão", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    // Procura o usuário pelo e-mail
                    string buscar = @"
                        SELECT
                            id,
                            nome_completo,
                            username,
                            email,
                            avatar,
                            tipo_usuario,
                            perfil_acesso,
                            status
                        FROM usuarios
                        WHERE email = @email
                        LIMIT 1";

                    int idUsuario;
                    string nomeUsuario;
                    string username;
                    string emailUsuario;
                    string avatar;
                    string tipoUsuario;
                    string perfilAcesso;
                    string status;

                    using (MySqlCommand command = new MySqlCommand(buscar, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Nenhum usuário foi encontrado com esse e-mail.", "Exclusão", MessageBoxButton.OK, MessageBoxImage.Warning);
                                TxtEmail.Clear();
                                return;
                            }

                            idUsuario = Convert.ToInt32(reader["id"]);
                            nomeUsuario = reader["nome_completo"].ToString();
                            username = reader["username"].ToString();
                            emailUsuario = reader["email"].ToString();
                            avatar = reader["avatar"].ToString();
                            tipoUsuario = reader["tipo_usuario"].ToString();
                            perfilAcesso = reader["perfil_acesso"].ToString();
                            status = reader["status"].ToString();
                        }
                    }

                    // Impede o administrador de excluir a própria conta
                    if (idUsuario == usuarioLogadoId)
                    {
                        MessageBox.Show("O administrador não pode excluir a própria conta enquanto estiver conectado.", "Exclusão não permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                        using (MySqlCommand command = new MySqlCommand(contarAdmins, connection))
                        {
                            quantidadeAdmins = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Impede a exclusão do último administrador
                        if (quantidadeAdmins <= 1)
                        {
                            MessageBox.Show("O último administrador do sistema não pode ser excluído.", "Exclusão não permitida", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Confirmação obrigatória antes da exclusão
                    MessageBoxResult confirmacao = MessageBox.Show(
                        "Deseja realmente excluir este usuário?\n\n" +
                        nomeUsuario +
                        "\n" +
                        email,
                        "Confirmar exclusão",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirmacao != MessageBoxResult.Yes)
                    {
                        TxtEmail.Clear();
                        return;
                    }

                    // Busca o username do administrador responsável
                    string adminUsername = "";

                    string buscarAdmin = @"
                        SELECT username
                        FROM usuarios
                        WHERE id = @idAdmin";

                    using (MySqlCommand commandAdmin = new MySqlCommand(buscarAdmin, connection))
                    {
                        commandAdmin.Parameters.AddWithValue("@idAdmin", usuarioLogadoId);

                        object resultado = commandAdmin.ExecuteScalar();

                        if (resultado != null)
                        {
                            adminUsername = resultado.ToString();
                        }
                    }

                    // Exclui o usuário
                    string excluir = @"
                        DELETE FROM usuarios
                        WHERE id = @id";

                    using (MySqlCommand command = new MySqlCommand(excluir, connection))
                    {
                        command.Parameters.AddWithValue("@id", idUsuario);
                        command.ExecuteNonQuery();
                    }

                    // ==============================
                    // AUDITORIA
                    // ==============================

                    string inserirAuditoria = @"
                        INSERT INTO auditoria
                        (
                            usuario_responsavel_id,
                            usuario_responsavel,
                            operacao,
                            registro_afetado,
                            valor_anterior,
                            novo_valor
                        )
                        VALUES
                        (
                            @usuario_responsavel_id,
                            @usuario_responsavel,
                            @operacao,
                            @registro_afetado,
                            @valor_anterior,
                            @novo_valor
                        )";

                    using (MySqlCommand commandAuditoria = new MySqlCommand(inserirAuditoria, connection))
                    {
                        commandAuditoria.Parameters.AddWithValue("@usuario_responsavel_id", usuarioLogadoId);
                        commandAuditoria.Parameters.AddWithValue("@usuario_responsavel", adminUsername);
                        commandAuditoria.Parameters.AddWithValue("@operacao", "Exclusão de usuário");
                        commandAuditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + idUsuario + " - Usuário: " + username);

                        string valorAnterior = "Nome: " + nomeUsuario +
                                               "; Username: " + username +
                                               "; E-mail: " + emailUsuario +
                                               "; Tipo: " + tipoUsuario +
                                               "; Perfil: " + perfilAcesso +
                                               "; Status: " + status +
                                               "; Avatar: " + avatar;

                        commandAuditoria.Parameters.AddWithValue("@valor_anterior", valorAnterior);
                        commandAuditoria.Parameters.AddWithValue("@novo_valor", DBNull.Value);

                        commandAuditoria.ExecuteNonQuery();
                    }

                    MessageBox.Show("Usuário excluído com sucesso!", "Exclusão", MessageBoxButton.OK, MessageBoxImage.Information);

                    TxtEmail.Clear();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro no banco de dados:\n" + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir usuário:\n" + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HubAdmin abrir = new HubAdmin(usuarioLogadoId);
            abrir.Show();
            this.Close();
        }
    }
}
```
