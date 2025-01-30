using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public class OISCalculationResult
    {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal DailyRate { get; set; }
    }
        public class OISCalculator
        {
            public decimal NotionalAmount { get; set; }
            public int Days { get; set; }
            public decimal OvernightRate { get; set; }

            public decimal CalculateDailyRate()
            {
                return OvernightRate / 360;
            }

            public decimal CalculateInterst()
            {
                decimal dailyRate = CalculateDailyRate();
                return NotionalAmount * (dailyRate / 100) * Days;
            }

            public decimal CalculateTotalPayment()
            {
                return NotionalAmount + CalculateInterst();
            }

            public OISCalculationResult CalculateOIS()
            {
                decimal dailyRate = CalculateDailyRate();
                decimal totalInterest = CalculateInterst();
                decimal totalPayment = CalculateTotalPayment();

                return new OISCalculationResult
                {
                    DailyRate = dailyRate,
                    TotalInterest = totalInterest,
                    TotalPayment = totalPayment
                };
            }

            public string FormatCalculationResult(OISCalculationResult result)
            {
                return $"OIS Calculation Results:\n" +
                   $"Daily Rate: {result.DailyRate:F6}%\n" +
                   $"Total Interest: {result.TotalInterest:F2} USD\n" +
                   $"Total Payment: {result.TotalPayment:F2} USD\n" +
                   $"Period: {Days} days\n" +
                   $"Overnight Rate: {OvernightRate}%";
            }
        }
}
