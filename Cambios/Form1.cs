
namespace Cambios
{
    using Cambios.Modelos;
    using Cambios.Servicos;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {
        #region Atributos

        private NetworkService networkService;

        private ApiService apiService;

        private List<Rate> Rates;

        private DialogService dialogService;

        private DataService dataService;

        #endregion

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new ApiService();
            dialogService = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;

            lblResultado.Text = "A atualizar taxas...";

            var connection = networkService.CherckConnection();

            if (!connection.IsSuccess)
            {
                LoadLocalRates();
                load = false;
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if (Rates.Count == 0)
            {
                lblResultado.Text = "Não há ligação à internet" + Environment.NewLine + "e não foram previamente carregadas as taxas" +
                    Environment.NewLine + "Tente mais tarde!";

                return;
            }

            comboBoxOrigem.DataSource = Rates;
            comboBoxOrigem.DisplayMember = "Name";

            //Corrige bug da microsoft
            comboBoxDestino.BindingContext = new BindingContext();

            comboBoxDestino.DataSource = Rates;
            comboBoxDestino.DisplayMember = "Name";



            lblResultado.Text = "Taxas atualizadas...";

            if (load)
            {
                lblStatus.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                lblStatus.Text = string.Format("Taxas carregadas da base de dados.");
            }

            progressBar1.Value = 100;
            btConverter.Enabled = true;
            btTroca.Enabled = true;
        }

        private void LoadLocalRates()
        {
           Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            dataService.DeleteData();

            dataService.SaveData(Rates);
        }

        private void btConverter_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(tbValor.Text))
            {
                dialogService.ShowMessage("Erro", "Insira um valor para converter!");
                return;
            }

            decimal valor;

            if (!decimal.TryParse(tbValor.Text, out valor))
            {
                dialogService.ShowMessage("Erro de converção", "O valor tem de ser numérico");
                return;
            }

            if (comboBoxOrigem.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro" , "Tem que escolher uma moeda de origem para converter ");
                return;
            }

            if (comboBoxDestino.SelectedItem == null)
            {
                dialogService.ShowMessage("Erro", "Tem que escolher uma moeda de destino para converter ");
                return;
            }

            var taxaOrigem = (Rate)comboBoxOrigem.SelectedItem;


            var taxaDestino = (Rate)comboBoxDestino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            lblResultado.Text = string.Format("{0} {1:C2} = {2} {3:C2}" ,
                taxaOrigem.Code, 
                valor, 
                taxaDestino.Code, 
                valorConvertido);
        }

        private void btTroca_Click(object sender, EventArgs e)
        {
            Trocar();
        }

        private void Trocar()
        {
            var aux = comboBoxOrigem.SelectedItem;
            comboBoxOrigem.SelectedItem = comboBoxDestino.SelectedItem;
            comboBoxDestino.SelectedItem = aux;

            Converter();
        }
    }
}
