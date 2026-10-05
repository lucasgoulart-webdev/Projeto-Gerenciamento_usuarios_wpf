```csharp
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para Logs.xaml
    /// </summary>
    public partial class Logs : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        private int idLogado;

        public Logs(int idUsuario)
        {
            InitializeComponent();

            idLogado = idUsuario;

            // Deixa "Todas" selecionado inicialmente
            CmbOperacao.SelectedIndex = 0;

            // Carrega os logs assim que a tela abre
            CarregarLogs();
        }

        // ==============================
        // CLASSE DOS LOGS
        // ==============================

        public class AuditoriaLog
        {
            public DateTime DataHora { get; set; }
            public string UsuarioResponsavel { get; set; }
            public string Operacao { get; set; }
            public string RegistroAfetado { get; set; }
            public string ValorAnterior { get; set; }
            public string NovoValor { get; set; }
        }

        // ==============================
        // CARREGAR LOGS
        // ==============================

        private void CarregarLogs()
        {
            List<AuditoriaLog> logs = new List<AuditoriaLog>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT
                            data_hora,
                            usuario_responsavel,
                            operacao,
                            registro_afetado,
                            valor_anterior,
                            novo_valor
                        FROM auditoria
                        ORDER BY data_hora DESC";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AuditoriaLog log = new AuditoriaLog();

                                log.DataHora = Convert.ToDateTime(reader["data_hora"]);
                                log.UsuarioResponsavel = reader["usuario_responsavel"].ToString();
                                log.Operacao = reader["operacao"].ToString();
                                log.RegistroAfetado = reader["registro_afetado"].ToString();

                                log.ValorAnterior = reader["valor_anterior"] == DBNull.Value
                                    ? "-"
                                    : reader["valor_anterior"].ToString();

                                log.NovoValor = reader["novo_valor"] == DBNull.Value
                                    ? "-"
                                    : reader["novo_valor"].ToString();

                                logs.Add(log);
                            }
                        }
                    }

                    GridLogs.ItemsSource = logs;
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro ao carregar os logs:\n" + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar os logs:\n" + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // PESQUISAR / FILTRAR
        // ==============================

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string pesquisa = TxtPesquisa.Text.Trim();
            string operacaoSelecionada = "";

            if (CmbOperacao.SelectedItem is ComboBoxItem item)
            {
                operacaoSelecionada = item.Content.ToString();
            }

            List<AuditoriaLog> logs = new List<AuditoriaLog>();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT
                            data_hora,
                            usuario_responsavel,
                            operacao,
                            registro_afetado,
                            valor_anterior,
                            novo_valor
                        FROM auditoria
                        WHERE 1 = 1";

                    // Pesquisa pelo responsável, operação ou registro afetado
                    if (!string.IsNullOrWhiteSpace(pesquisa))
                    {
                        sql += @"
                            AND (
                                usuario_responsavel LIKE @pesquisa
                                OR operacao LIKE @pesquisa
                                OR registro_afetado LIKE @pesquisa
                            )";
                    }

                    // Filtro de operação
                    if (!string.IsNullOrWhiteSpace(operacaoSelecionada) && operacaoSelecionada != "Todas")
                    {
                        sql += " AND operacao LIKE @operacao";
                    }

                    sql += " ORDER BY data_hora DESC";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        if (!string.IsNullOrWhiteSpace(pesquisa))
                        {
                            command.Parameters.AddWithValue("@pesquisa", "%" + pesquisa + "%");
                        }

                        if (!string.IsNullOrWhiteSpace(operacaoSelecionada) && operacaoSelecionada != "Todas")
                        {
                            command.Parameters.AddWithValue("@operacao", "%" + operacaoSelecionada + "%");
                        }

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                AuditoriaLog log = new AuditoriaLog();

                                log.DataHora = Convert.ToDateTime(reader["data_hora"]);
                                log.UsuarioResponsavel = reader["usuario_responsavel"].ToString();
                                log.Operacao = reader["operacao"].ToString();
                                log.RegistroAfetado = reader["registro_afetado"].ToString();

                                log.ValorAnterior = reader["valor_anterior"] == DBNull.Value
                                    ? "-"
                                    : reader["valor_anterior"].ToString();

                                log.NovoValor = reader["novo_valor"] == DBNull.Value
                                    ? "-"
                                    : reader["novo_valor"].ToString();

                                logs.Add(log);
                            }
                        }
                    }

                    GridLogs.ItemsSource = logs;

                    if (logs.Count == 0)
                    {
                        MessageBox.Show("Nenhum registro de auditoria foi encontrado.", "Auditoria", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Erro ao pesquisar os logs:\n" + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao pesquisar os logs:\n" + ex.Message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // VOLTAR
        // ==============================

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            HubAdmin abrir = new HubAdmin(idLogado);
            abrir.Show();
            this.Close();
        }
    }
}
```
