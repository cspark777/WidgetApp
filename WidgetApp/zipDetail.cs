using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*******************************************************************************
 *  Class used to store zipcode details, and this object of this class having  *
 *  all the values are passed to the print window. Print window gets this      *
 *  object and then it displays appropriately according to the print format    * 
 *  and thus preventing the print window to access the server to get the zip   *
 *  code details again.                                                        *
 *  ***************************************************************************/


namespace WidgetApp
{
    public class zipDetail
    {
        public string zipCode { get; set; }
        public string salesTaxRate { get; set; }
        public string useTaxRate { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string state { get; set; }

        public string salesDollarAmt { get; set; }
        public string salesTaxAmt { get; set; }
        public string salesTotalAmt { get; set; }

        public string useDollarAmt { get; set; }
        public string useTaxAmt { get; set; }
        public string useTotalAmt { get; set; }

        /* Breakout details */
        //sales tax
        public string salesStateLbl { get; set; }
        public string salesCountyLbl { get; set; }
        public string salesCityLbl { get; set; }
        public string salesSplDtLbl { get; set; }
        public string salesTotalLbl { get; set; }

        /* Breakout details */
        //use tax
        public string useStateLbl { get; set; }
        public string useCountyLbl { get; set; }
        public string useCityLbl { get; set; }
        public string useSplDtLbl { get; set; }
        public string useTotalLbl { get; set; }

        /* special rules */
        public string textBlk { get; set; }

        /* other community */
        public string commCityStLbl { get; set; }
        public string commState { get; set; }
        public string commCounty { get; set; }
        public string commCity { get; set; }
        public string commSplDt { get; set; }
        public string commStateVal { get; set; }
        public string commCountyVal { get; set; }
        public string commCityVal { get; set; }
        public string commSplDtVal { get; set; }
        public string commTotalVal { get; set; }
    }
}
