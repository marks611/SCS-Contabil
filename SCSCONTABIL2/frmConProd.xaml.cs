﻿using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;
using System;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;
// Primeiro de tudo, vou te apresentar um facilitador de coisas... chamado ReSharper...

namespace SCSCONTABIL2
{
    /// <summary>
    /// Interaction logic for frmConProd.xaml
    /// </summary>
    public partial class frmConProd : Window
    {
        Conexao conexao = new Conexao();
        public frmConProd()
        {
            InitializeComponent();
            atualizaDataGrid();
            txtICMS.Text = "0,00";
            txtPrecoTotal.Text = "0,00";
            txtPreco.Text = "0,00";
            txtFrete.Text = "0,00";
            txtNomePro.MaxLength = 39;
            txtPreco.MaxLength = 9;
            txtQtd.MaxLength = 6;
            txtPrecoTotal.MaxLength = 15;
            testar_nivel();
            
        }        

        private void btnAlt_Click(object sender, RoutedEventArgs e)
        {
            if (txtRazao.Text.Equals("") || txtNomePro.Text.Equals("") || txtPreco.Text.Equals("") ||
                txtQtd.Text.Equals(""))
            {
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Content = "Preencha todos os campos com *";
            }
            else
            {
                atualizarDados();
            }
        }
       

        private void btnBus_Click(object sender, RoutedEventArgs e)
        {
            //lista da classe abstrata Produto que receberá os produtos
            var lista = new List<Produto>();
            String busca = txtBusca.Text;
            //buscar os produtos conforme o nome que o usuario digitar
            MySqlCommand buscaProd = new MySqlCommand("select * from produto where ProNom like '%' ?nome '%'", conexao.con);
            buscaProd.Parameters.Add(new MySqlParameter("?nome", busca));
            //limpar o datagrid
            dataGrid.ItemsSource = null;
            dataGrid.Items.Clear();
            dataGrid.Items.Refresh();
            //abrir BD
            conexao.abrir();
            //ler as informações do banco de dados
            using (MySqlDataReader leitor = buscaProd.ExecuteReader())
            {
                while (leitor.Read())
                {
                    //classe abstrata para dados de produtos
                    Produto produto = new Produto();
                    //info do BD
                    produto.ProCod = Convert.ToInt32(leitor["ProCod"]);
                    produto.ProNom = leitor["ProNom"].ToString();
                    produto.ProPco = Convert.ToDecimal(leitor["ProPco"]);
                    produto.data = (DateTime)leitor["ProDat"];
                    produto.ProDat = produto.data.ToShortDateString();
                    produto.ProQtd = Convert.ToInt32(leitor["ProQtd"]);
                    //adiciona as variaveis a uma lista
                    lista.Add(produto);
                }
            }
            //adiciona a lista ao dataGrid
            dataGrid.ItemsSource = lista;
            txtBusca.Text = "";
            conexao.fechar();
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (txtRazao.Text.Equals("") || txtNomePro.Text.Equals("") || txtPreco.Text.Equals("") ||
                txtQtd.Text.Equals(""))
            {
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Content = "Preencha todos os campos com *";
            }
            else
            {
                verificarVendas();
            }
        }

        private void txtQtd_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            //só permite numeros e o backspace no textbox
            e.Handled = !recebeNumero(e.Key);
            
        }

        private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //variavel de codigo do fornecedor
            int forne = 0;
            //pega a linha selecionada no datagrid
            Produto dados = (Produto)(dataGrid.SelectedItem);
            //pega o codigo do produto
            int codigo = dados.ProCod;
            //informações do produto
            MySqlCommand buscaProd = new MySqlCommand("select * from produto where ProCod = ?codigo", conexao.con);
            buscaProd.Parameters.Add(new MySqlParameter("?codigo", codigo));


            //abrir conexao
            conexao.abrir();
            //ler as informações do produto
            using (MySqlDataReader leitor = buscaProd.ExecuteReader())
            {
                while (leitor.Read())
                {
                    txtNomePro.Text = leitor["ProNom"].ToString();
                    DateTime dia = (DateTime)leitor["ProDat"];
                    txtData.Text = dia.ToShortDateString();
                    txtPreco.Text = leitor["ProPco"].ToString();
                    txtQtd.Text = leitor["ProQtd"].ToString();
                    forne = int.Parse(leitor["ProFor"].ToString());
                    txtCod.Text = codigo.ToString();
                    txtPrecoTotal.Text = leitor["ProPcoTot"].ToString();
                    txtFrete.Text = leitor["ProFre"].ToString();
                    txtICMS.Text = leitor["ProIcms"].ToString();
                }
                leitor.Close();
            }

            //informações do fornecedor
            MySqlCommand buscaFor = new MySqlCommand("select * from fornecedor where ForCod = ?codigo", conexao.con);
            buscaFor.Parameters.Add(new MySqlParameter("?codigo", forne));

            using (MySqlDataReader leitor = buscaFor.ExecuteReader())
            {
                while (leitor.Read())
                {
                    txtRazao.Text = leitor["ForRaz"].ToString();
                    txtNome.Text = leitor["ForNom"].ToString();
                    txtImu.Text = leitor["ForImu"].ToString();
                    txtIes.Text = leitor["ForIes"].ToString();
                    txtCnpj.Text = leitor["ForCnp"].ToString();
                }
                leitor.Close();
            }
            conexao.fechar();
        }

        private void txtPreco_KeyDown(object sender, KeyEventArgs e)
        {//bloqueia a digitação de valores diferentes de numeros no textbox
            e.Handled = !recebeNumero(e.Key);
        }

        private bool recebeNumero(Key inKey)
        {
            if (inKey < Key.D0 || inKey > Key.D9)
            {
                if (inKey < Key.NumPad0 || inKey > Key.NumPad9)
                {
                    return false;
                }
            }
            return true;
        }

        private void limpar()
        {

            txtRazao.Text = "";
            txtBusca.Text = "";
            txtNome.Text = "";
            txtIes.Text = "";
            txtImu.Text = "";
            txtCod.Text = "";
            txtNomePro.Text = "";
            txtPreco.Text = "";
            txtData.Text = "";
            txtQtd.Text = "";
            txtCnpj.Text = "";
            txtPrecoTotal.Text = "";
            
            txtFrete.Text = "";
            txtICMS.Text = "";
        }

        private void Moeda(ref TextBox txt)
        {
            //faz uma mascara para digitar o preco
            String n = "";
            Double v = 0;
            try
            {
                n = txt.Text.Replace(",", "").Replace(".", "");
                if (n.Equals(""))
                {
                    n = "";
                }
                n = n.PadLeft(3, '0');
                if ((n.Length > 3) && (n.Substring(0, 1) == "0"))
                {
                    n = n.Substring(1, n.Length - 1);
                }
                v = Convert.ToDouble(n) / 100;
                txt.Text = string.Format("{0:N}", v);
                txt.SelectionStart = txt.Text.Length;
            }
            catch (Exception e)
            {

            }
        }


        private void deletarDados()
        {
            try
            {
                //confirma se o usuario vai excluir o produto
                MessageBoxResult op = Xceed.Wpf.Toolkit.MessageBox.Show("Tem certeza que deseja excluir o produto: " + txtNomePro.Text + "?", "Excluir",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (op.ToString().ToUpper() == "YES")
                {
                    MySqlCommand deletar = new MySqlCommand("delete from produto where ProCod = ?codigo", conexao.con);
                    deletar.Parameters.Add(new MySqlParameter("?codigo", txtCod.Text));
                    conexao.abrir();
                    deletar.ExecuteNonQuery();
                    conexao.fechar();
                    lblStatus.Foreground = Brushes.Green;
                    lblStatus.Content = "Deletado com sucesso";
                    limpar();
                    atualizaDataGrid();
                    txtBusca.Focus();
                }
            }
            catch (Exception erro)
            {
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Content = erro.Message;
                Xceed.Wpf.Toolkit.MessageBox.Show(erro.ToString());
            }
        }


        private void atualizarDados()
        {
            try
            {
                String precoFi = txtPrecoTotal.Text.Replace(".", "").Replace(",", ".");
                String frete = txtFrete.Text.Replace(".", "").Replace(",", ".");
                String icms = txtICMS.Text.Replace(".", "").Replace(",", ".");
                DateTime data = Convert.ToDateTime(txtData.Text);
                String preco = txtPreco.Text.Replace(".", "").Replace(",", ".");
                MySqlCommand alterar = new MySqlCommand("update produto set ProNom = ?nome, ProPco = ?preco, ProDat = ?data, ProQtd = ?qtd, ProPcoTot = ?total, ProFre = ?frete, " +
                    "ProIcms = ?icms where ProCod = ?codigo", conexao.con);
                alterar.Parameters.Add(new MySqlParameter("?nome", txtNomePro.Text));
                alterar.Parameters.Add(new MySqlParameter("?preco", preco));
                alterar.Parameters.Add(new MySqlParameter("?data", data));
                alterar.Parameters.Add(new MySqlParameter("?qtd", txtQtd.Text));
                alterar.Parameters.Add(new MySqlParameter("?total", precoFi));
                alterar.Parameters.Add(new MySqlParameter("?frete", frete));
                alterar.Parameters.Add(new MySqlParameter("?icms", icms));
                alterar.Parameters.Add(new MySqlParameter("?codigo", txtCod.Text));
                conexao.abrir();
                alterar.ExecuteNonQuery();
                conexao.fechar();
                lblStatus.Foreground = Brushes.Green;
                lblStatus.Content = "Alterado com sucesso";
                limpar();
                atualizaDataGrid();
                txtBusca.Focus();

            }
            catch (Exception erro)
            {
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Content = erro.Message;
                Xceed.Wpf.Toolkit.MessageBox.Show(erro.ToString());
            }
        }


        private void atualizaDataGrid()
        {
            //lista que sera adicionada ao datagrid
            var lista = new List<Produto>();

            //limpar o datagrid
            dataGrid.ItemsSource = null;
            dataGrid.Items.Clear();
            dataGrid.Items.Refresh();

            //abre BD
            conexao.abrir();

            MySqlCommand datagrid = new MySqlCommand("select * from produto", conexao.con);
            using (MySqlDataReader leitor = datagrid.ExecuteReader())
            {
                while (leitor.Read())
                {
                    //classe abstrata para dados de produtos
                    Produto produto = new Produto();
                    produto.ProCod = Convert.ToInt32(leitor["ProCod"]);
                    produto.ProNom = leitor["ProNom"].ToString();
                    produto.data = (DateTime)leitor["ProDat"];
                    produto.ProDat = produto.data.ToShortDateString();
                    produto.ProQtd = Convert.ToInt32(leitor["ProQtd"]);
                    //adiciona as variaveis a uma lista
                    lista.Add(produto);

                }
                leitor.Close();

            }
            //adiciona a lista ao datagrid
            dataGrid.ItemsSource = lista;

            conexao.fechar();
        }

        private void testar_nivel()
        {
            //abrir conexão
            Conexao conexao = new Conexao();
            conexao.abrir();
            //Instancia da classe frmLogin para pegar a informação do nome do usuario
            frmLogin login = new frmLogin();
            //busca tipo do usuario
            MySqlCommand comandos = new MySqlCommand("select UsuTip from usuario where UsuNom = ?usuario", conexao.con);
            comandos.Parameters.Add(new MySqlParameter("?usuario", login.getUsuario()));
            //É executado e lido o comando.
            MySqlDataReader reader = comandos.ExecuteReader();
            String resultado = null;
            //vai ler o resultado do tipo do usuario
            while (reader.Read())
            {
                resultado = reader["UsuTip"].ToString();
            }
            //Se o usuario estiver nivel abaixo de A ele terá limitações
            if (resultado == "USER")
            {
                //bloqueia todos os botões de cadastro
                btnAlt.IsEnabled = false;
                btnDel.IsEnabled = false;
                
            }
            
            conexao.fechar();
        }

        private void txtPreco_TextChanged(object sender, TextChangedEventArgs e)
        {
            //chama o metodo que formatara o valor
            Moeda(ref txtPreco);
            
            String precoS = txtPreco.Text.Replace(".", "").Replace(",", ".");
            if (!txtQtd.Text.Equals("") && !precoS.Equals("0.00"))
            {

                Decimal precoF = Convert.ToDecimal(precoS);
                int unidades = Convert.ToInt32(txtQtd.Text);
                txtPrecoTotal.Text = Convert.ToString(precoF * unidades);
            }
        }

        private void btnVol_Click(object sender, RoutedEventArgs e)
        {
            //instância da classe frmPrincipal
            frmPrincipal frmpri = new frmPrincipal();
            //Fecha esse form e abre o frmPrincipal
            frmpri.Show();
            this.Close();
        }

        private void txtPreco_LostFocus(object sender, RoutedEventArgs e)
        {
            String precoS = txtPreco.Text.Replace(".", "").Replace(",", ".");
            if (!txtQtd.Text.Equals("") && !precoS.Equals("0.00"))
            {

                Decimal precoF = Convert.ToDecimal(precoS);
                int unidades = Convert.ToInt32(txtQtd.Text);
                txtPrecoTotal.Text = Convert.ToString(precoF * unidades);
            }
        }

        private void txtQtd_LostFocus(object sender, RoutedEventArgs e)
        {
            
        }

        private void txtPrecoTotal_TextChanged(object sender, TextChangedEventArgs e)
        {
            //chama o metodo que formatara o valor
            Moeda(ref txtPrecoTotal);

            String precoS = txtPrecoTotal.Text.Replace(".", "").Replace(",", ".");
            Decimal precoF = Convert.ToDecimal(precoS);
            if (precoF < 100m)
            {
                
                txtICMS.Text = "0.18";
                txtFrete.Text = "0.01";

            }
            else
            {
                Decimal icms = decimal.Multiply(decimal.Divide(precoF, 100m), 18m);
                txtICMS.Text = Convert.ToString(icms);
                Decimal frete = decimal.Divide(precoF, 100m);
                txtFrete.Text = Convert.ToString(frete);
            }

        }

        private void txtFrete_TextChanged(object sender, TextChangedEventArgs e)
        {
            //chama o metodo que formatara o valor
            Moeda(ref txtFrete);
        }

        private void txtICMS_TextChanged(object sender, TextChangedEventArgs e)
        {
            //chama o metodo que formatara o valor
            Moeda(ref txtICMS);
        }

        

        private void txtQtd_LostFocus_1(object sender, RoutedEventArgs e)
        {
            String precoS = txtPreco.Text.Replace(".", "").Replace(",", ".");
            if (!precoS.Equals("0.00") && !txtQtd.Equals(""))
            {
                Decimal precoF = Convert.ToDecimal(precoS);
                int unidades = Convert.ToInt32(txtQtd.Text);
                txtPrecoTotal.Text = Convert.ToString(precoF * unidades);
            }
        }


        private void verificarVendas()
        {
            bool encontrado = false;
            try
            {
                //Variavel com os comandos de consulta do codigo
                MySqlCommand consultaPeps = new MySqlCommand("select * from venda where VenPro = ?nome", conexao.con);
                //adiciona parametros ao comando String, evita problemas com SQL Inject
                consultaPeps.Parameters.Add(new MySqlParameter("?nome", txtNomePro.Text));

                //Variavel com os comandos de consulta do codigo
                MySqlCommand consultaUeps = new MySqlCommand("select * from vendaUEPS where VenPro = ?nome", conexao.con);
                //adiciona parametros ao comando String, evita problemas com SQL Inject
                consultaUeps.Parameters.Add(new MySqlParameter("?nome", txtNomePro.Text));
                //abrir conexao com BD
                conexao.abrir();
                using (MySqlDataReader Peps = consultaPeps.ExecuteReader())
                {
                    //verificar se o produto está cadastrado em alguma venda

                    if (Peps.HasRows)
                    {   //se estiver em uso procura pelo proximo
                        conexao.fechar();
                        lblStatus.Foreground = Brushes.Red;
                        lblStatus.Content = "Impossivel a exclusão. Produto está cadastrado em uma venda";
                        encontrado = true;
                    }
                    else
                    {
                        //fechar reader
                        Peps.Close();
                        encontrado = false;

                    }

                }
                if (encontrado == false)
                {
                    using (MySqlDataReader Ueps = consultaUeps.ExecuteReader())
                    {
                        //verificar se o produto está cadastrado em alguma venda
                        if (Ueps.HasRows)
                        {   //se estiver em uso procura pelo proximo
                            conexao.fechar();
                            lblStatus.Foreground = Brushes.Red;
                            lblStatus.Content = "Impossivel a exclusão. Produto está cadastrado em uma venda";
                        }
                        else
                        {
                            //fechar reader
                            Ueps.Close();
                            //fechar conexao
                            conexao.fechar();
                            deletarDados();
                        }
                    }
                }
            }
            catch (Exception erro)
            {
                //fechar conexao
                conexao.fechar();
                lblStatus.Foreground = Brushes.Red;
                lblStatus.Content = erro.Message;
            }
        }
    }
    
}
