using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Data.SqlClient;
using System.Web;
using System.Data;

using Microsoft.Win32;                  //for registry activities
using System.Security.AccessControl;    //for Security Access in Registry
using System.Net.NetworkInformation;    //for getting mac id
using System.Management;                //for getting processor ID
using System.Diagnostics;               //for opening a browser (starting a process)
using System.Net;                       //for getting ip address



namespace WidgetApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /* Dictionary holds community sales tax rate's. Key is the community name and Value is list of strings such as
         * city,
         * state,
         * county,
         * CompareCity,
         * sales tax rate sum,
         * state rate,
         * county rate,
         * city rate,
         * special rate,
       */
        Dictionary<string, List<string>> communityDict = new Dictionary<string, List<string>>();
        zipDetail zDetail = new zipDetail();  //object for the zipDetail class to hold the information to send it to the print page
        private RegistryKey baseRegistryKey = Registry.LocalMachine;
        bool showCommunityMsgFlg = false;
        bool loginFlg = false;
        bool showUseTaxFlg = true;

        public MainWindow()
        {
            InitializeComponent();

            checkUserLogin();               //checks the registry for user credentials and calls the stored procedure with the username and password(if found in registry)

            if (loginFlg == true)
            {
                showLookupScreen();
            }
            else
            {
                showUserLogin();
            }
        }


        public void showLookupScreen()
        {
            zipCodeTxt.Text = "Enter ZIP Code Here!";
            cityLbl.Content = "";
            zipCodeHeadLbl.Content = "";
            salesTaxRateLbl.Content = "";
            salesTaxRateFracLbl.Content = "";
            errorAmtLbl.Content = "";
            detailsBtn.Visibility = Visibility.Hidden;

            loginGrid.Visibility = Visibility.Hidden;
            grid1.Visibility = Visibility.Visible;
            grid2.Visibility = Visibility.Hidden;

            widgetWindow.Width = 214;
            widgetWindow.Height = 250;

            grid1.Margin = new Thickness(0, 0, 0, 0);

            widgetWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 250;
            widgetWindow.Top = 50;
            widgetWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            clearCommunityLabels();
        }


        public void checkUserLogin()
        {
            try
            {
                if (File.Exists("..\\user.ini"))
                {
                    StreamReader fileReader = new StreamReader("..\\user.ini");
                    string userName = fileReader.ReadLine();
                    string password = fileReader.ReadLine();
                    fileReader.Close();

                    if (userName == "" || password == "")
                    {
                        loginFlg = false;
                    }
                    else
                    {
                        bool internetConnAvail = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                        if (internetConnAvail == true)
                        {
                            loginInternetErrorLbl.Content = "";

                            string server = "dbWidget.Zip2Tax.com";
                            string dbUsername = "z2t_WidgetUser";
                            string dbPassword = "d6bU6avut7+*eM=2u-Ru";
                            string dbName = "z2t_WebPublic";
                            string connString = "Server=" + server + "; Database=" + dbName + "; User Id=" + dbUsername + "; password=" + dbPassword + "; Application Name=Z2T_Widget_V1_0;";

                            using (SqlConnection conn = new SqlConnection(connString))
                            {
                                string sql = "z2t_Widget_lookup_login";
                                SqlCommand cmdHeader = new SqlCommand(sql, conn);
                                conn.Open();

                                cmdHeader.CommandType = CommandType.StoredProcedure;

                                cmdHeader.Parameters.Add(new SqlParameter("@login", SqlDbType.NVarChar, 20));
                                cmdHeader.Parameters["@login"].Value = userName;

                                cmdHeader.Parameters.Add(new SqlParameter("@password", SqlDbType.NVarChar, 20));
                                cmdHeader.Parameters["@password"].Value = password;

                                cmdHeader.Parameters.Add(new SqlParameter("@machineID", SqlDbType.NVarChar, 80));
                                cmdHeader.Parameters["@machineID"].Value = macProcessorID();

                                cmdHeader.Parameters.Add(new SqlParameter("@ipAddress", SqlDbType.NVarChar, 20));
                                cmdHeader.Parameters["@ipAddress"].Value = ipAddress();

                                cmdHeader.Parameters.Add(new SqlParameter("@zipcode", SqlDbType.NVarChar, 10));
                                cmdHeader.Parameters["@zipcode"].Value = DBNull.Value;

                                cmdHeader.Parameters.Add("@loginStatus", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                                cmdHeader.Parameters.Add("@expirationDays", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                                cmdHeader.Parameters.Add("@version", SqlDbType.NVarChar, 5).Direction = ParameterDirection.Output;

                                //Execute your command
                                cmdHeader.ExecuteNonQuery();

                                conn.Close();

                                //Get the return value from the stored procedure
                                string returnvalue = cmdHeader.Parameters["@loginStatus"].Value.ToString();

                                int retVal = int.Parse(returnvalue);

                                if (retVal > 0)
                                {
                                    loginFlg = true;
                                }
                            }
                        }
                        else
                        {
                            /* warning msg in the login screen to connect to network */
                            loginInternetErrorLbl.Content = "Check your network connection";
                            loginFlg = false;
                        }
                    }
                }
            }
            catch
            {
                loginFlg = false;
            }
        }


        public void showUserLogin()
        {
            loginGrid.Visibility = Visibility.Visible;
            grid1.Visibility = Visibility.Hidden;
            grid2.Visibility = Visibility.Hidden;
            widgetWindow.Width = 214;
            widgetWindow.Height = 250;
            loginGrid.Margin = new Thickness(0, 0, 0, 0);
            widgetWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 250;
            widgetWindow.Top = 50;
            widgetWindow.WindowStartupLocation = WindowStartupLocation.Manual;
        }

        private void getRateBtn_Click(object sender, RoutedEventArgs e)
        {
            showCommunityMsgFlg = false;

            string server = "dbWidget.Zip2Tax.com";
            string dbUsername = "z2t_WidgetUser";
            string dbPassword = "d6bU6avut7+*eM=2u-Ru";
            string dbName = "z2t_WebPublic";
            string connString = "Server=" + server + "; Database=" + dbName + "; User Id=" + dbUsername + "; password=" + dbPassword + "; Application Name=Z2T_Widget_V1_0;";

            communityLBox.Items.Clear();    //clearing the listbox
            communityDict.Clear();          //clearing the community dictionary (to remove the old key value pairs)
            clearCommunityLabels();         //clearing all the labels in the community group box

            bool internetConnAvail = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();

            string userName = "invalidUsername";
            string password = "invalidPassword";

            if (File.Exists("..\\user.ini"))
            {
                StreamReader fileReader = new StreamReader("..\\user.ini");
                userName = fileReader.ReadLine();
                password = fileReader.ReadLine();
                fileReader.Close();
            }

            if (internetConnAvail == true)
            {
                internetErrorTxtBlk.Text = "";
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string sql = "z2t_Widget_lookup_login";
                    string zipCode = zipCodeTxt.Text.ToString();
                    SqlCommand cmdHeader = new SqlCommand(sql, conn);
                    conn.Open();

                    cmdHeader.CommandType = CommandType.StoredProcedure;

                    cmdHeader.Parameters.Add(new SqlParameter("@login", SqlDbType.NVarChar, 20));
                    cmdHeader.Parameters["@login"].Value = userName;

                    cmdHeader.Parameters.Add(new SqlParameter("@password", SqlDbType.NVarChar, 20));
                    cmdHeader.Parameters["@password"].Value = password;

                    cmdHeader.Parameters.Add(new SqlParameter("@machineID", SqlDbType.NVarChar, 80));
                    cmdHeader.Parameters["@machineID"].Value = DBNull.Value;

                    cmdHeader.Parameters.Add(new SqlParameter("@ipAddress", SqlDbType.NVarChar, 20));
                    cmdHeader.Parameters["@ipAddress"].Value = DBNull.Value;

                    cmdHeader.Parameters.Add(new SqlParameter("@zipcode", SqlDbType.NVarChar, 10));
                    cmdHeader.Parameters["@zipcode"].Value = zipCode;

                    cmdHeader.Parameters.Add("@loginStatus", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                    cmdHeader.Parameters.Add("@expirationDays", SqlDbType.Int, 4).Direction = ParameterDirection.Output;
                    cmdHeader.Parameters.Add("@version", SqlDbType.NVarChar, 5).Direction = ParameterDirection.Output;

                    cmdHeader.ExecuteNonQuery();

                    //Get the return value from the stored procedure
                    string returnvalue = cmdHeader.Parameters["@loginStatus"].Value.ToString();
                    int retVal = int.Parse(returnvalue);

                    //Get the return value from the stored procedure
                    string expReturnvalue = cmdHeader.Parameters["@expirationDays"].Value.ToString();
                    int expRetVal = int.Parse(expReturnvalue);

                    //Get the return value from the stored procedure
                    string versionVal = cmdHeader.Parameters["@version"].Value.ToString();

                    if ((retVal == 0) && (versionVal == "v1.0"))
                    {
                        logoutBtn_Click(sender, e);
                    }
                    else if((retVal==1) && (expRetVal<0) && (versionVal == "v1.0"))
                    {
                        MessageBox.Show("User subscription expired", "Zip2Tax");
                        //logoutBtn_Click(sender, e);
                    }
                    else if (versionVal == "v1.0")
                    {
                        SqlDataReader rdr = cmdHeader.ExecuteReader();

                        string city, state, SalesTaxRate, zip;

                        city = "";
                        state = "";
                        SalesTaxRate = "";
                        zip = "";

                        cityLbl.Content = "Incorrect ZIP Code";
                        zipCodeHeadLbl.Content = "";
                        salesTaxRateLbl.Content = "";
                        salesTaxRateFracLbl.Content = "";
                        detailsBtn.Visibility = Visibility.Hidden;
                        splRulesTxtBlk.Text = "";

                        while (rdr.Read())
                        {
                            //display using Response.Write
                            if ((rdr["PrimaryRecord"].ToString()) == "P")
                            {
                                city = rdr["DisplayCity"].ToString();
                                state = rdr["State"].ToString();
                                SalesTaxRate = rdr["SalesTaxRate"].ToString();

                                string finalSalesTaxRateDbl;
                                double salesTaxRateDbl = Convert.ToDouble(SalesTaxRate);
                                string salesTaxRateDec = salesTaxRateDbl.ToString();

                                if (rdr["UseTaxRate"].ToString() == "")
                                {
                                    useTaxRateValLbl.Content = "";
                                }
                                else
                                {
                                    string useTaxRateDec = (Convert.ToDouble(rdr["UseTaxRate"].ToString())).ToString();
                                    useTaxRateValLbl.Content = useTaxRateDec + " %";
                                }

                                string improperFrac = (dec2frac(salesTaxRateDbl)).Trim();
                                int fracPos = improperFrac.IndexOf("/");

                                if (fracPos != -1)
                                {
                                    int fracLen = improperFrac.Length;
                                    string numerator = improperFrac.Substring(0, fracPos);
                                    string denominator = improperFrac.Substring(fracPos + 1, fracLen - (fracPos + 1));
                                    int numeratorInt = int.Parse(numerator);
                                    int denominatorInt = int.Parse(denominator);
                                    int mixedWholeNum = numeratorInt / denominatorInt;
                                    int mixedNumerator = numeratorInt % denominatorInt;

                                    finalSalesTaxRateDbl = mixedWholeNum.ToString() + " " + mixedNumerator.ToString() + "/" + denominator;
                                }
                                else
                                {
                                    finalSalesTaxRateDbl = improperFrac;
                                }

                                cityLbl.Content = city + ", " + state;
                                zipCodeHeadLbl.Content = rdr["Zip_Code"].ToString();
                                salesTaxRateLbl.Content = salesTaxRateDec + " %";
                                salesTaxRateFracLbl.Content = "(" + finalSalesTaxRateDbl + " %)";

                                detailsBtn.Visibility = Visibility.Visible;

                                zip = rdr["Zip_Code"].ToString();

                                zipcodeLbl.Content = rdr["Zip_Code"].ToString();
                                salesTaxRateValLbl.Content = salesTaxRateDec + " %";

                                cityValLbl.Content = rdr["DisplayCity"].ToString();
                                countyLbl.Content = rdr["County"].ToString();
                                stateLbl.Content = rdr["State"].ToString();

                                splRulesTxtBlk.Text = splRulesTxtBlk.Text + "\n" + rdr["NoteCategory"].ToString() + " : " + rdr["Note"].ToString();
                                setSalesExpanderData(rdr);
                                setUseExpanderData(rdr);
                            }
                            else
                            {
                                loadCommunityListbox(rdr);
                            }
                        }
                        rdr.Close();

                        communityGroupBoxLbl.Content = "Other Communities Using " + zip;

                        if (communityLBox.HasItems == false)
                            communityGroupBox.Visibility = Visibility.Hidden;
                        else
                            communityGroupBox.Visibility = Visibility.Visible;

                        zipCodeTxt.Text = "Enter ZIP Code Here!";

                        if (cityLbl.Content.ToString() == "Incorrect ZIP Code")
                        {
                            widgetWindow.Width = 214;
                            widgetWindow.Height = 250;
                            detailsBtn.Content = "Show Details";

                            grid2.Visibility = Visibility.Hidden;
                            grid1.Margin = new Thickness(0, 0, 0, 0);

                            widgetWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 250;
                            widgetWindow.Top = 50;

                        }

                        if ((useTaxRateValLbl.Content.ToString() == "") && (cityLbl.Content.ToString() != "Incorrect ZIP Code"))
                        {
                            showUseTaxFlg = false;
                            BanUseTax();        //if user is not subscribed to use tax
                        }
                        else
                        {
                            showUseTaxFlg = true;
                            removeBanUseTax();
                        }

                    }
                    else
                    {
                        string upgradeMsg = "Please upgrade to version " + versionVal + "!";
                        MessageBox.Show(upgradeMsg, "Zip2Tax");
                    }
                }
                clearCalculateLabelValues();
                showCommMsg();
            }
            else         /* if internet connection is not there, then it will show a warning msg */
            {
                widgetWindow.Width = 214;
                widgetWindow.Height = 250;
                grid2.Visibility = Visibility.Hidden;
                grid1.Margin = new Thickness(0, 0, 0, 0);
                widgetWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 250;
                widgetWindow.Top = 50;
                cityLbl.Content = "";
                zipCodeHeadLbl.Content = "";
                salesTaxRateLbl.Content = "";
                salesTaxRateFracLbl.Content = "";
                detailsBtn.Visibility = Visibility.Hidden;
                detailsBtn.Content = "Show Details";
                internetErrorTxtBlk.Text = "Check your computer's network connection";
            }

        }

        public void BanUseTax()
        {
            Run linkText = new Run("Purchase");
            Hyperlink link = new Hyperlink(linkText);
            link.NavigateUri = new Uri("http://www.zip2tax.com/z2t_services.asp");
            
            link.RequestNavigate += new RequestNavigateEventHandler(delegate(object sender, RequestNavigateEventArgs e)
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
                e.Handled = true;
            });

            useTaxRateValLbl.Content = link;



            label3.Visibility = Visibility.Hidden;
            useStateLbl.Visibility = Visibility.Hidden;
            useCountyLbl.Visibility = Visibility.Hidden;
            useCityLbl.Visibility = Visibility.Hidden;
            useStateRateLbl.Visibility = Visibility.Hidden;
            useCountyRateLbl.Visibility = Visibility.Hidden;
            useCityRateLbl.Visibility = Visibility.Hidden;
            useSpecialRateLbl.Visibility = Visibility.Hidden;
            useTotalRateLbl.Visibility = Visibility.Hidden;
            useTotalLbl.Visibility = Visibility.Hidden;
            separator4.Visibility = Visibility.Hidden;

            useSpecialLbl.Visibility = Visibility.Hidden;
            useCalcAmtHeadLbl.Visibility = Visibility.Hidden;
            useCalcLbl.Visibility = Visibility.Hidden;
            useCalcTotalLbl.Visibility = Visibility.Hidden;
            useCalcAmtValLbl.Visibility = Visibility.Hidden;
            useCalcValLbl.Visibility = Visibility.Hidden;
            useCalcTotalValLbl.Visibility = Visibility.Hidden;
            separator2.Visibility = Visibility.Hidden;
        }

        public void removeBanUseTax()
        {
            label3.Visibility = Visibility.Visible;
            useStateLbl.Visibility = Visibility.Visible;
            useCountyLbl.Visibility = Visibility.Visible;
            useCityLbl.Visibility = Visibility.Visible;
            useStateRateLbl.Visibility = Visibility.Visible;
            useCountyRateLbl.Visibility = Visibility.Visible;
            useCityRateLbl.Visibility = Visibility.Visible;
            useSpecialRateLbl.Visibility = Visibility.Visible;
            useTotalRateLbl.Visibility = Visibility.Visible;
            useTotalLbl.Visibility = Visibility.Visible;
            separator4.Visibility = Visibility.Visible;

            useSpecialLbl.Visibility = Visibility.Visible;
            useCalcAmtHeadLbl.Visibility = Visibility.Visible;
            useCalcLbl.Visibility = Visibility.Visible;
            useCalcTotalLbl.Visibility = Visibility.Visible;
            useCalcAmtValLbl.Visibility = Visibility.Visible;
            useCalcValLbl.Visibility = Visibility.Visible;
            useCalcTotalValLbl.Visibility = Visibility.Visible;
            separator2.Visibility = Visibility.Visible;
        }


        public void showCommMsg()
        {
            if (showCommunityMsgFlg == false)
                commMsgLbl.Visibility = Visibility.Visible;
            else
                commMsgLbl.Visibility = Visibility.Hidden;
        }


        /// <summary>
        /// Function for loading the communities in the list box 
        /// </summary>
        public void loadCommunityListbox(SqlDataReader rdr)
        {
            if (communityLBox.Items.Contains(rdr["DisplayCity"].ToString()) == false)
            {
                communityLBox.Items.Add(rdr["DisplayCity"].ToString());

                /* creating a new list for a community and then adding the list to the community dictionary */
                List<string> communityStringList = new List<string>();
                communityStringList.Add(rdr["DisplayCity"].ToString());
                communityStringList.Add(rdr["State"].ToString());
                communityStringList.Add(rdr["County"].ToString());
                communityStringList.Add(rdr["CompareCity"].ToString());
                communityStringList.Add(rdr["SalesTaxRate"].ToString());
                communityStringList.Add(rdr["SalesTaxRateState"].ToString());
                communityStringList.Add(rdr["SalesTaxRateCounty"].ToString());
                communityStringList.Add(rdr["SalesTaxRateCity"].ToString());
                communityStringList.Add(rdr["SalesTaxRateSpecial"].ToString());

                communityDict.Add(rdr["DisplayCity"].ToString(), communityStringList);
            }

        }


        /// <summary>
        /// Function which sets the sales tax data in sales expander
        /// </summary>
        public void setSalesExpanderData(SqlDataReader rdr)
        {
            salesStateLbl.Content = "State Of " + rdr["State"].ToString();
            salesCountyLbl.Content = "County Of " + rdr["County"].ToString();
            salesCityLbl.Content = "City Of " + rdr["DisplayCity"].ToString();
            salesStateRateLbl.Content = "% " + rdr["SalesTaxRateState"].ToString();
            salesCountyRateLbl.Content = "% " + rdr["SalesTaxRateCounty"].ToString();
            salesCityRateLbl.Content = "% " + rdr["SalesTaxRateCity"].ToString();
            salesSpecialRateLbl.Content = "% " + rdr["SalesTaxRateSpecial"].ToString();
            salesTotalRateLbl.Content = "% " + rdr["SalesTaxRate"].ToString();
        }

        /// <summary>
        /// Function which sets the use tax data in sales expander
        /// </summary>
        public void setUseExpanderData(SqlDataReader rdr)
        {
            useStateLbl.Content = "State Of " + rdr["State"].ToString();
            useCountyLbl.Content = "County Of " + rdr["County"].ToString();
            useCityLbl.Content = "City Of " + rdr["DisplayCity"].ToString();
            useStateRateLbl.Content = "% " + rdr["UseTaxRateState"].ToString();
            useCountyRateLbl.Content = "% " + rdr["UseTaxRateCounty"].ToString();
            useCityRateLbl.Content = "% " + rdr["UseTaxRateCity"].ToString();
            useSpecialRateLbl.Content = "% " + rdr["UseTaxRateSpecial"].ToString();
            useTotalRateLbl.Content = "% " + rdr["UseTaxRate"].ToString();
        }

        public void clearCalculateLabelValues()
        {
            /* clear sales tax amount details */
            calcAmtValLbl.Content = "";
            calcSaleValLbl.Content = "";
            calcTotalValLbl.Content = "";

            /* clear use tax amount details */
            useCalcAmtValLbl.Content = "";
            useCalcValLbl.Content = "";
            useCalcTotalValLbl.Content = "";
        }

        private void zipCodeTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            zipCodeTxt.Text = "";
        }

        private void zipCodeTxt_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                getRateBtn_Click(sender, e);
                invisibleFrame.Focus();
            }
        }

        /// <summary>
        /// Process performed on clicking the details button
        /// </summary>
        private void detailsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (detailsBtn.Content.ToString() == "Show Details")
            {
                widgetWindow.Width = 670;
                widgetWindow.Height = 590;
                detailsBtn.Content = "Hide Details";

                grid2.Margin = new Thickness(10, 10, 0, 0);
                grid1.Margin = new Thickness(460, 0, 0, 0);

                grid2.Visibility = Visibility.Visible;

                widgetWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 706;
                widgetWindow.Top = 50;

            }
            else if (detailsBtn.Content.ToString() == "Hide Details")
            {
                widgetWindow.Width = 214;
                widgetWindow.Height = 250;
                detailsBtn.Content = "Show Details";

                grid2.Visibility = Visibility.Hidden;
                grid1.Margin = new Thickness(0, 0, 0, 0);

                widgetWindow.Left = System.Windows.SystemParameters.PrimaryScreenWidth - 250;
                widgetWindow.Top = 50;
            }

        }

        public void calcSalesTax()
        {
            string dollarAmt = amountTxt.Text.ToString();
            float dollarAmt_float = float.Parse(dollarAmt);
            string dollarAmt_currencyFormat = String.Format("{0:N2}", dollarAmt_float);
            calcAmtValLbl.Content = dollarAmt_currencyFormat;

            float modifiedDollarAmt_float = float.Parse(dollarAmt_currencyFormat);

            string salesTaxRate = (salesTaxRateValLbl.Content.ToString()).Substring(0, (salesTaxRateValLbl.Content.ToString()).IndexOf(" "));
            float salesTaxRate_float = float.Parse(salesTaxRate);
            string SalesTaxRate_currencyFormat = String.Format("{0:N2}", salesTaxRate_float);

            float modifiedSalesTaxRate = float.Parse(SalesTaxRate_currencyFormat);

            float calcSalesTaxRate_float = (modifiedDollarAmt_float * modifiedSalesTaxRate) / 100;
            string salesTaxAmtFinal = String.Format("{0:N2}", calcSalesTaxRate_float);
            calcSaleValLbl.Content = salesTaxAmtFinal;

            float totalAmt = modifiedDollarAmt_float + calcSalesTaxRate_float;
            string totalAmt_CurrencyFormat = String.Format("{0:N2}", totalAmt);
            calcTotalValLbl.Content = totalAmt_CurrencyFormat;
        }

        public void calcUseTax()
        {
            string dollarAmt = amountTxt.Text.ToString();
            float dollarAmt_float = float.Parse(dollarAmt);
            string dollarAmt_currencyFormat = String.Format("{0:N2}", dollarAmt_float);
            useCalcAmtValLbl.Content = dollarAmt_currencyFormat;

            float modifiedDollarAmt_float = float.Parse(dollarAmt_currencyFormat);

            string useTaxRate = (useTaxRateValLbl.Content.ToString()).Substring(0, (useTaxRateValLbl.Content.ToString()).IndexOf(" "));

            //string useTaxRate = useTaxRateValLbl.Content.ToString();
            float useTaxRate_float = float.Parse(useTaxRate);
            string useTaxRate_currencyFormat = String.Format("{0:N2}", useTaxRate_float);

            float modifiedUseTaxRate = float.Parse(useTaxRate_currencyFormat);

            float calcUseTaxRate_float = (modifiedDollarAmt_float * modifiedUseTaxRate) / 100;
            string useTaxAmtFinal = String.Format("{0:N2}", calcUseTaxRate_float);
            useCalcValLbl.Content = useTaxAmtFinal;

            float totalAmt = modifiedDollarAmt_float + calcUseTaxRate_float;
            string totalAmt_CurrencyFormat = String.Format("{0:N2}", totalAmt);
            useCalcTotalValLbl.Content = totalAmt_CurrencyFormat;
        }

        public void callCalculateProc()
        {
            try
            {
                string userName = "invalidUsername";
                string zipcode = zipcodeLbl.Content.ToString();

                if (File.Exists("..\\user.ini"))
                {
                    StreamReader fileReader = new StreamReader("..\\user.ini");
                    userName = fileReader.ReadLine();
                    fileReader.Close();
                }

                bool internetConnAvail = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (internetConnAvail == true)
                {
                    loginInternetErrorLbl.Content = "";

                    string server = "dbWidget.Zip2Tax.com";
                    string dbUsername = "z2t_WidgetUser";
                    string dbPassword = "d6bU6avut7+*eM=2u-Ru";
                    string dbName = "z2t_WebPublic";
                    string connString = "Server=" + server + "; Database=" + dbName + "; User Id=" + dbUsername + "; password=" + dbPassword + "; Application Name=Z2T_Widget_V1_0;";

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string sql = "z2t_Widget_Log_Calculation";
                        SqlCommand cmdHeader = new SqlCommand(sql, conn);
                        conn.Open();

                        cmdHeader.CommandType = CommandType.StoredProcedure;

                        cmdHeader.Parameters.Add(new SqlParameter("@zipCode", SqlDbType.NVarChar, 10));
                        cmdHeader.Parameters["@zipCode"].Value = zipcode;

                        cmdHeader.Parameters.Add(new SqlParameter("@userName", SqlDbType.NVarChar, 20));
                        cmdHeader.Parameters["@userName"].Value = userName;

                        cmdHeader.Parameters.Add(new SqlParameter("@machineID", SqlDbType.NVarChar, 80));
                        cmdHeader.Parameters["@machineID"].Value = DBNull.Value;

                        cmdHeader.Parameters.Add(new SqlParameter("@amount", SqlDbType.Decimal, 15));
                        cmdHeader.Parameters["@amount"].Value = Decimal.Parse(amountTxt.Text);

                        //Execute your command
                        cmdHeader.ExecuteNonQuery();

                        conn.Close();
                    }
                }
            }
            catch
            {
            }
        }


        private void calcBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isNumeric(amountTxt.Text.ToString(), System.Globalization.NumberStyles.Any) == true)
            {
                calcSalesTax();
                if (showUseTaxFlg != false)
                {
                    calcUseTax();
                }
                callCalculateProc();
                amountTxt.Text = "Enter Dollar Amount";
                errorAmtLbl.Content = "";
            }
            else
            {
                amountTxt.Text = "Enter Dollar Amount";
                errorAmtLbl.Content = "Invalid Amount Entered !";
                clearCalculateLabelValues();
            }
        }

        private void communityLBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (communityLBox.HasItems == true)
            {
                separator5.Visibility = Visibility.Visible;

                string communityKey = communityLBox.SelectedItem.ToString();
                List<string> communityList = communityDict[communityKey];
                string city = communityList[0];
                string state = communityList[1];
                string county = communityList[2];
                string compCity = communityList[3];
                string salesTaxRateSum = communityList[4];
                string stateRate = communityList[5];
                string countyRate = communityList[6];
                string cityRate = communityList[7];
                string specialRate = communityList[8];

                commCityStateLbl.Content = city + ", " + state;
                commSalesStateLbl.Content = "State Of " + state;
                commSalesCountyLbl.Content = "County Of " + county;
                commSalesCityLbl.Content = "City Of " + compCity;
                commSalesSpecialLbl.Content = "Special District";
                commSalesTotalLbl.Content = "Total";

                commSalesStateRateLbl.Content = "% " + stateRate;
                commSalesCountyRateLbl.Content = "% " + countyRate;
                commSalesCityRateLbl.Content = "% " + cityRate;
                commSalesSpecialRateLbl.Content = "% " + specialRate;
                commSalesTotalRateLbl.Content = "% " + salesTaxRateSum;

                showCommunityMsgFlg = true;
                showCommMsg();
            }
        }


        /// <summary>
        /// Clearing labels in community group box at the start and also on getting the new zipcode details
        /// </summary>
        public void clearCommunityLabels()
        {
            commCityStateLbl.Content = "";
            separator5.Visibility = Visibility.Hidden;

            commSalesStateLbl.Content = "";
            commSalesCountyLbl.Content = "";
            commSalesCityLbl.Content = "";
            commSalesSpecialLbl.Content = "";
            commSalesTotalLbl.Content = "";

            commSalesStateRateLbl.Content = "";
            commSalesCountyRateLbl.Content = "";
            commSalesCityRateLbl.Content = "";
            commSalesSpecialRateLbl.Content = "";
            commSalesTotalRateLbl.Content = "";
        }

        /// <summary>
        /// function which converts the a decimal number to number in fraction (not a mixed fraction) format
        /// </summary>
        /// <param name="dbl">double value which has to be converted to fraction format</param>
        /// <returns>fraction number in string format</returns>
        private static string dec2frac(double dbl)
        {
            char neg = ' ';
            double dblDecimal = dbl;
            if (dblDecimal == (int)dblDecimal) return dblDecimal.ToString(); //return no if it's not a decimal
            if (dblDecimal < 0)
            {
                dblDecimal = Math.Abs(dblDecimal);
                neg = '-';
            }
            var whole = (int)Math.Truncate(dblDecimal);
            string decpart = dblDecimal.ToString().Replace(Math.Truncate(dblDecimal) + ".", "");
            double rN = Convert.ToDouble(decpart);
            double rD = Math.Pow(10, decpart.Length);

            string rd = recur(decpart);
            int rel = Convert.ToInt32(rd);
            if (rel != 0)
            {
                rN = rel;
                rD = (int)Math.Pow(10, rd.Length) - 1;
            }
            //just a few prime factors for testing purposes
            var primes = new[] { 47, 43, 37, 31, 29, 23, 19, 17, 13, 11, 7, 5, 3, 2 };
            foreach (int i in primes) reduceNo(i, ref rD, ref rN);

            rN = rN + (whole * rD);
            return string.Format("{0}{1}/{2}", neg, rN, rD);
        }

        /// <summary>
        /// Finds out the recurring decimal in a specified number
        /// </summary>
        /// <param name="db">Number to check</param>
        /// <returns></returns>
        private static string recur(string db)
        {
            if (db.Length < 13) return "0";
            var sb = new StringBuilder();
            for (int i = 0; i < 7; i++)
            {
                sb.Append(db[i]);
                int dlength = (db.Length / sb.ToString().Length);
                int occur = occurence(sb.ToString(), db);
                if (dlength == occur || dlength == occur - sb.ToString().Length)
                {
                    return sb.ToString();
                }
            }
            return "0";
        }

        /// <summary>
        /// Checks for number of occurence of specified no in a number
        /// </summary>
        /// <param name="s">The no to check occurence times</param>
        /// <param name="check">The number where to check this</param>
        /// <returns></returns>
        private static int occurence(string s, string check)
        {
            int i = 0;
            int d = s.Length;
            string ds = check;
            for (int n = (ds.Length / d); n > 0; n--)
            {
                if (ds.Contains(s))
                {
                    i++;
                    ds = ds.Remove(ds.IndexOf(s), d);
                }
            }
            return i;
        }

        /// <summary>
        /// Reduces a fraction given the numerator and denominator
        /// </summary>
        /// <param name="i">Number to use in an attempt to reduce fraction</param>
        /// <param name="rD">the Denominator</param>
        /// <param name="rN">the Numerator</param>
        private static void reduceNo(int i, ref double rD, ref double rN)
        {
            //keep reducing until divisibility ends
            while ((rD % i) == 0 && (rN % i) == 0)
            {
                rN = rN / i;
                rD = rD / i;
            }
        }

        private void amountTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            amountTxt.Text = "";
        }

        private void amountTxt_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (isNumeric(amountTxt.Text.ToString(), System.Globalization.NumberStyles.Any) == true)
                {
                    calcBtn_Click(sender, e);
                    invisibleFrame.Focus();
                }
                else
                {
                    amountTxt.Text = "Enter Dollar Amount";
                    errorAmtLbl.Content = "Invalid Amount Entered !";
                    clearCalculateLabelValues();
                    invisibleFrame.Focus();
                }
            }
        }

        public bool isNumeric(string val, System.Globalization.NumberStyles NumberStyle)
        {
            Double result;
            return Double.TryParse(val, NumberStyle,
                System.Globalization.CultureInfo.CurrentCulture, out result);
        }

        /// <summary>
        /// method for printing the widget contents
        /// </summary>
        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            collectParameterValuesForPrint();
            var printWin = new printWindow(zDetail);
            printWin.ShowDialog();
        }

        public void collectParameterValuesForPrint()
        {
            zDetail.zipCode = zipcodeLbl.Content.ToString();
            zDetail.salesTaxRate = salesTaxRateValLbl.Content.ToString();
            zDetail.useTaxRate = useTaxRateValLbl.Content.ToString();
            zDetail.city = cityValLbl.Content.ToString();
            zDetail.county = countyLbl.Content.ToString();
            zDetail.state = stateLbl.Content.ToString();

            zDetail.salesDollarAmt = calcAmtValLbl.Content.ToString();
            zDetail.salesTaxAmt = calcSaleValLbl.Content.ToString();
            zDetail.salesTotalAmt = calcTotalValLbl.Content.ToString();

            zDetail.useDollarAmt = useCalcAmtValLbl.Content.ToString();
            zDetail.useTaxAmt = useCalcValLbl.Content.ToString();
            zDetail.useTotalAmt = useCalcTotalValLbl.Content.ToString();

            zDetail.salesStateLbl = salesStateRateLbl.Content.ToString();
            zDetail.salesCountyLbl = salesCountyRateLbl.Content.ToString();
            zDetail.salesCityLbl = salesCityRateLbl.Content.ToString();
            zDetail.salesSplDtLbl = salesSpecialRateLbl.Content.ToString();
            zDetail.salesTotalLbl = salesTotalRateLbl.Content.ToString();

            zDetail.useStateLbl = useStateRateLbl.Content.ToString();
            zDetail.useCountyLbl = useCountyRateLbl.Content.ToString();
            zDetail.useCityLbl = useCityRateLbl.Content.ToString();
            zDetail.useSplDtLbl = useSpecialRateLbl.Content.ToString();
            zDetail.useTotalLbl = useTotalRateLbl.Content.ToString();

            zDetail.textBlk = splRulesTxtBlk.Text;

            zDetail.commCityStLbl = commCityStateLbl.Content.ToString();
            zDetail.commState = commSalesStateLbl.Content.ToString();
            zDetail.commCounty = commSalesCountyLbl.Content.ToString();
            zDetail.commCity = commSalesCityLbl.Content.ToString();
            zDetail.commSplDt = commSalesSpecialLbl.Content.ToString();
            zDetail.commStateVal = commSalesStateRateLbl.Content.ToString();
            zDetail.commCountyVal = commSalesCountyRateLbl.Content.ToString();
            zDetail.commCityVal = commSalesCityRateLbl.Content.ToString();
            zDetail.commSplDtVal = commSalesSpecialRateLbl.Content.ToString();
            zDetail.commTotalVal = commSalesTotalRateLbl.Content.ToString();
        }


        private void logoutBtn_Click(object sender, RoutedEventArgs e)
        {
            callLogoutProc();
            clearUserFile();
            loginFlg = false;
            showUserLogin();
            detailsBtn.Content = "Show Details";
        }

        public void callLogoutProc()
        {
            try
            {
                string userName = "invalidUsername";

                if (File.Exists("..\\user.ini"))
                {
                    StreamReader fileReader = new StreamReader("..\\user.ini");
                    userName = fileReader.ReadLine();
                    fileReader.Close();
                }

                bool internetConnAvail = System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
                if (internetConnAvail == true)
                {
                    loginInternetErrorLbl.Content = "";

                    string server = "dbWidget.Zip2Tax.com";
                    string dbUsername = "z2t_WidgetUser";
                    string dbPassword = "d6bU6avut7+*eM=2u-Ru";
                    string dbName = "z2t_WebPublic";
                    string connString = "Server=" + server + "; Database=" + dbName + "; User Id=" + dbUsername + "; password=" + dbPassword + "; Application Name=Z2T_Widget_V1_0;";

                    using (SqlConnection conn = new SqlConnection(connString))
                    {
                        string sql = "z2t_Widget_logout";
                        SqlCommand cmdHeader = new SqlCommand(sql, conn);
                        conn.Open();

                        cmdHeader.CommandType = CommandType.StoredProcedure;

                        cmdHeader.Parameters.Add(new SqlParameter("@login", SqlDbType.NVarChar, 20));
                        cmdHeader.Parameters["@login"].Value = userName;

                        cmdHeader.Parameters.Add(new SqlParameter("@machineID", SqlDbType.NVarChar, 80));
                        cmdHeader.Parameters["@machineID"].Value = macProcessorID();

                        //Execute your command
                        cmdHeader.ExecuteNonQuery();

                        conn.Close();
                    }
                }
            }
            catch
            {
            }
        }

        public void clearUserFile()
        {
            if (File.Exists("..\\user.ini"))
            {
                File.Delete("..\\user.ini");
            }
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                FileStream fs = File.Create("..\\user.ini");
                fs.Close();
                StreamWriter fileWrite = new StreamWriter("..\\user.ini");
                fileWrite.WriteLine(userNameTxtBox.Text);
                fileWrite.WriteLine(passwordTxtBox.Password);
                fileWrite.Close();
                checkUserLogin();
                if (loginFlg == true)
                {
                    showLookupScreen();
                }
                else
                {
                    if (loginInternetErrorLbl.Content.ToString() != "Check your network connection")
                    {
                        MessageBox.Show("Invalid Credentials!", "Zip2Tax");
                        userNameTxtBox.Text = "Username";
                        passwordTxtBox.Password = "Password";
                    }
                }
            }
            catch
            {
                loginFlg = false;
            }
        }

        private void userNameTxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            userNameTxtBox.Text = "";
        }

        private void passwordTxtBox_GotFocus(object sender, RoutedEventArgs e)
        {
            passwordTxtBox.Password = "";
        }

        private void passwordTxtBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                loginInvisibleframe.Focus();
                loginBtn_Click(sender, e);
            }
        }

        public string ipAddress()
        {
            try
            {
                String direction = "";
                WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
                using (WebResponse response = request.GetResponse())
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    direction = stream.ReadToEnd();
                }
                //Search for the ip in the html
                int first = direction.IndexOf("Address: ") + 9;
                int last = direction.LastIndexOf("</body>");
                direction = direction.Substring(first, last - first);

                return direction;
            }
            catch
            {
                return "IP can't be found";
            }
        }


        public string macProcessorID()
        {
            try
            {
                ManagementObjectCollection mbsList = null;
                ManagementObjectSearcher mbs = new ManagementObjectSearcher("Select * From Win32_DiskDrive");
                mbsList = mbs.Get();
                string harddiskId = "";
                foreach (ManagementObject mo in mbsList)
                {
                    /* getting hard disk serial number */
                    harddiskId = mo["SerialNumber"].ToString();
                }

                const int MIN_MAC_ADDR_LENGTH = 12;
                string macAddress = "";
                long maxSpeed = -1;

                foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                {

                    string tempMac = nic.GetPhysicalAddress().ToString();
                    if (nic.Speed > maxSpeed && !String.IsNullOrEmpty(tempMac) && tempMac.Length >= MIN_MAC_ADDR_LENGTH)
                    {
                        maxSpeed = nic.Speed;
                        macAddress = tempMac;
                    }
                }

                string combinedID = macAddress + harddiskId;
                return combinedID;
            }
            catch
            {
                return "error getting IDs";
            }
        }


    }
}
