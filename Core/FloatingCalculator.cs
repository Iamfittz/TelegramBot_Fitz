using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public class FloatingRateLoanCalculator
    {
        public decimal LoanAmount { get; set; }
        public int TotalYears {  get; set; }
        public decimal FirstRate {  get; set; }
        public decimal SecondRate { get; set; }
        public decimal CalculateFirstPeriodInterest()
        {
            return LoanAmount*(FirstRate/100)*(6/12m);
        }
        public decimal CalculateSecondPeriodInterest()
        {
            return LoanAmount * (SecondRate / 100) * (6 / 12m);
        }

        public decimal CalculateTotalInterest()
        {
            return CalculateFirstPeriodInterest() + CalculateSecondPeriodInterest();
        }

        public decimal CalculateTotalPayment()
        {
            return LoanAmount + CalculateTotalInterest();
        }
    }
}
