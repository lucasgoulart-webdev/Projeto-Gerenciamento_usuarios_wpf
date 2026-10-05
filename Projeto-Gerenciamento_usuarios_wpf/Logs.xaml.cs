
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Internal logic for Logs.xaml
    /// </summary>
    public partial class Logs : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        private int idLogado;

        public Logs(int idUsuario)
        {
            InitializeComponent();

            idLogado = idUsuario;

            // Selects "All" by default
            CmbOperacao.SelectedIndex = 0;

            // Loads the logs when the page opens
            CarregarLogs();
        }

        // ==============================
        // LOG CLASS
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
        // LOAD LOGS
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
                MessageBox.Show("Error loading audit logs:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading audit logs:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // SEARCH / FILTER
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

                    // Searches by responsible user, operation or affected record
                    if (!string.IsNullOrWhiteSpace(pesquisa))
                    {
                        sql += @"
                            AND (
                                usuario_responsavel LIKE @pesquisa
                                OR operacao LIKE @pesquisa
                                OR registro_afetado LIKE @pesquisa
                            )";
                    }

                    // Operation filter
                    if (!string.IsNullOrWhiteSpace(operacaoSelecionada) && operacaoSelecionada != "Todas" && operacaoSelecionada != "All")
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

                        if (!string.IsNullOrWhiteSpace(operacaoSelecionada) && operacaoSelecionada != "Todas" && operacaoSelecionada != "All")
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
                        MessageBox.Show("No audit records were found.", "Audit Logs", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error searching audit logs:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching audit logs:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // BACK
        // ==============================

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            HubAdmin abrir = new HubAdmin(idLogado);
            abrir.Show();
            this.Close();
        }
    }
}

