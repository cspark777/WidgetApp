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
using System.Windows.Shapes;

namespace WidgetApp
{
    /// <summary>
    /// Interaction logic for printWindow.xaml
    /// </summary>
    public partial class printWindow : Window
    {
        public printWindow(zipDetail zDetail)
        {
            InitializeComponent();
            fillZipDetails(zDetail);
            fillBreakoutDetails(zDetail);
            fillSpecialRules(zDetail);
            fillCommunityDetails(zDetail);
            printWin.Height = 800;
        }

        /// <summary>
        /// function that provides zipcode details to the zipcode details box
        /// </summary>
        public void fillZipDetails(zipDetail zDetail)
        {
            zipcodeLbl.Content = zDetail.zipCode;
            salesTaxRateValLbl.Content = zDetail.salesTaxRate;
            useTaxRateValLbl.Content = zDetail.useTaxRate;
            cityValLbl.Content = zDetail.city;
            countyLbl.Content = zDetail.county;
            stateLbl.Content = zDetail.state;

            calcAmtValLbl.Content = zDetail.salesDollarAmt;
            calcSaleValLbl.Content = zDetail.salesTaxAmt;
            calcTotalValLbl.Content = zDetail.salesTotalAmt;

            useCalcAmtValLbl.Content = zDetail.useDollarAmt;
            useCalcValLbl.Content = zDetail.useTaxAmt;
            useCalcTotalValLbl.Content = zDetail.useTotalAmt;

            if(useTaxRateValLbl.Content.ToString() == "System.Windows.Documents.Hyperlink")
            {
                useTaxRateValLbl.Content = "Purchase";
            }

            if (useTaxRateValLbl.Content.ToString() == "Purchase")
            {
                useCalcAmtHeadLbl.Visibility = Visibility.Hidden;
                useCalcLbl.Visibility = Visibility.Hidden;
                useCalcTotalLbl.Visibility = Visibility.Hidden;
                useCalcAmtValLbl.Visibility = Visibility.Hidden;
                useCalcValLbl.Visibility = Visibility.Hidden;
                useCalcTotalValLbl.Visibility = Visibility.Hidden;
                separator4.Visibility = Visibility.Hidden;
            }
        }

        public void fillBreakoutDetails(zipDetail zDetail)
        {
            salesStateLbl.Content = "State Of " + zDetail.state;
            salesCountyLbl.Content = "County Of " + zDetail.county;
            salesCityLbl.Content = "City Of " + zDetail.city;

            useStateLbl.Content = "State Of " + zDetail.state;
            useCountyLbl.Content = "County Of " + zDetail.county;
            useCityLbl.Content = "City Of " + zDetail.city;

            salesStateRateLbl.Content = zDetail.salesStateLbl;
            salesCountyRateLbl.Content = zDetail.salesCountyLbl;
            salesCityRateLbl.Content = zDetail.salesCityLbl;
            salesSpecialRateLbl.Content = zDetail.salesSplDtLbl;
            salesTotalRateLbl.Content = zDetail.salesTotalLbl;

            useStateRateLbl.Content = zDetail.useStateLbl;
            useCountyRateLbl.Content = zDetail.useCountyLbl;
            useCityRateLbl.Content = zDetail.useCityLbl;
            useSpecialRateLbl.Content = zDetail.useSplDtLbl;
            useTotalRateLbl.Content = zDetail.useTotalLbl;

            if (useTaxRateValLbl.Content.ToString() == "Purchase")
            {
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
                separator6.Visibility = Visibility.Hidden;
                useSpecialLbl.Visibility = Visibility.Hidden;
            }

        }

        public void fillSpecialRules(zipDetail zDetail)
        {
            if (zDetail.textBlk != "")
            {
                rulesGrid.Visibility = Visibility.Visible;
                splRulesTxtBlk.Text = zDetail.textBlk;
            }
            else
            {
                rulesGrid.Visibility = Visibility.Hidden;
            }
        }

        public void fillCommunityDetails(zipDetail zDetail)
        {
            if (zDetail.commCityStLbl != "")
            {
                otherCommunityGrid.Visibility = Visibility.Visible;
                communityGroupBoxLbl.Content = "Other Communities Using " + zDetail.zipCode;
                commCityStateLbl.Content = zDetail.commCityStLbl;
                commSalesStateLbl.Content = zDetail.commState;
                commSalesCountyLbl.Content = zDetail.commCounty;
                commSalesCityLbl.Content = zDetail.commCity;
                commSalesSpecialLbl.Content = zDetail.commSplDt;
                commSalesStateRateLbl.Content = zDetail.commStateVal;
                commSalesCountyRateLbl.Content = zDetail.commCountyVal;
                commSalesCityRateLbl.Content = zDetail.commCityVal;
                commSalesSpecialRateLbl.Content = zDetail.commSplDtVal;
                commSalesTotalRateLbl.Content = zDetail.commTotalVal;
            }
            else
            {
                otherCommunityGrid.Visibility = Visibility.Hidden;
            }
        }

        private void printBtn_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(printGrid, "Print from Widget (JKR)");
            }   
        }

        private void closeBtn_Click(object sender, RoutedEventArgs e)
        {
            printWin.Close();
        }
    }
}
