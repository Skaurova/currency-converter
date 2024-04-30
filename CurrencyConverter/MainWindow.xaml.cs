using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CurrencyConverter
{

    public partial class MainWindow : Window
    {

        SqlConnection connect = new SqlConnection();
        SqlDataAdapter dataAdapter = new SqlDataAdapter();
        SqlCommand command = new SqlCommand();

        private int currencyId = 0;
        private double from = 0;
        private double to = 0;

        public MainWindow()
        {
            InitializeComponent();
            BindCurrency();
            BindCurrency();
            GetData();
        }

        public void dbConnection()
        {
            string Con = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
            connect = new SqlConnection(Con);
            connect.Open();
        }

        private void BindCurrency()
        {
            dbConnection();
            DataTable dt = new DataTable();
            command = new SqlCommand("select Id, CurrencyName from Currency", connect);
            command.CommandType = CommandType.Text;
            dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(dt);

            DataRow dr = dt.NewRow();
            dr["Id"] = 0;
            dr["CurrencyName"] = "SELECT";
            dt.Rows.InsertAt(dr, 0);
            if (dt != null && dt.Rows.Count > 0)
            {
                cmbFromCurrency.ItemsSource = dt.DefaultView;
                cmbToCurrency.ItemsSource = dt.DefaultView;
            }
            connect.Close();

            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Id";
            cmbFromCurrency.SelectedIndex = 0;

            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Id";
            cmbToCurrency.SelectedIndex = 0;
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {

                MessageBox.Show("Please enter currency.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                txtCurrency.Focus();
                return;
            }

            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select currency from.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbFromCurrency.Focus();
                return;
            }

            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0)
            {
                MessageBox.Show("Please select currency to.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                cmbToCurrency.Focus();
                return;
            }

            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {

                ConvertedValue = double.Parse(txtCurrency.Text);
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
            else
            {

                ConvertedValue = (double.Parse(cmbFromCurrency.SelectedValue.ToString()) * double.Parse(txtCurrency.Text)) / double.Parse(cmbToCurrency.SelectedValue.ToString());
                lblCurrency.Content = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
        }

        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
                cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0)
                cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Content = "";
            txtCurrency.Focus();
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();
                    return;
                }
                else
                {
                    if (currencyId > 0)
                    {
                        if (MessageBox.Show("Are you sure you want to update?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            dbConnection();
                            DataTable dt = new DataTable();
                            command = new SqlCommand("Update Currency set Amount = @CA, CurrencyName = @CN where Id = @CI", connect);
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@CI", currencyId);
                            command.Parameters.AddWithValue("@CA", txtAmount.Text);
                            command.Parameters.AddWithValue("@CN", txtCurrencyName.Text);
                            command.ExecuteNonQuery();
                            connect.Close();
                            MessageBox.Show("Updated successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }

                    else
                    {
                        if (MessageBox.Show("Are you sure you want to update?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            dbConnection();
                            DataTable dt = new DataTable();
                            command = new SqlCommand("Insert into Currency(Amount, CurrencyName) values(@CA, @CN)", connect);
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddWithValue("@CA", txtAmount.Text);
                            command.Parameters.AddWithValue("@CN", txtCurrencyName.Text);
                            command.ExecuteNonQuery();
                            connect.Close();
                            MessageBox.Show("Saved successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void GetData()
        {
            dbConnection();
            DataTable dt = new DataTable();
            command = new SqlCommand("Select * from Currency", connect);
            command.CommandType = CommandType.Text;
            dataAdapter = new SqlDataAdapter(command);
            dataAdapter.Fill(dt);
            if (dt != null && dt.Rows.Count > 0)
            {
                dgvCurrency.ItemsSource = dt.DefaultView;
            }
            else
            {
                dgvCurrency.ItemsSource = null;
            }
            connect.Close();
        }

        private void ClearData()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                currencyId = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid dataGrid = (DataGrid)sender;
                DataRowView row = dataGrid.CurrentItem as DataRowView;

                if (row != null)
                {
                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (dataGrid.SelectedCells.Count > 0)
                        {
                            currencyId = Int32.Parse(row["Id"].ToString());

                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 0)
                            {
                                txtAmount.Text = row["Amount"].ToString();
                                txtCurrencyName.Text = row["CurrencyName"].ToString();
                                btnSave.Content = "Update";
                            }
                            if (dataGrid.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if (MessageBox.Show("Are you sure you want to delete?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    dbConnection();
                                    DataTable dt = new DataTable();
                                    command = new SqlCommand("DELETE FROM Currency WHERE Id = @CI", connect);
                                    command.CommandType = CommandType.Text;
                                    command.Parameters.AddWithValue("@CI", currencyId);
                                    command.ExecuteNonQuery();
                                    connect.Close();

                                    MessageBox.Show("Data deleted successfully.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearData();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
            {
                try
                {
                    ClearData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    } 

