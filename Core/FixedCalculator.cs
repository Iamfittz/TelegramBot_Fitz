using System.Linq;
using TelegramBot_Fitz.Bot;

namespace TelegramBot_Fitz.Core
{
    public class LoanCalculationResult
    {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }
    }

    public class FixedRateLoanCalculator
    {
        public decimal CalculateInterest(decimal amount, decimal[] yearlyRates)
        {
            decimal totalInterest = 0;
            for(int i = 0; i < yearlyRates.Length; i++)
            {
                decimal yearlyInterest = amount * (yearlyRates[i] / 100);
                totalInterest+= yearlyInterest;
            }
            return totalInterest;
        }

        public LoanCalculationResult CalculateLoan(decimal loanAmount, decimal[] yearlyRates)
        {
            var totalInterest = CalculateInterest(loanAmount, yearlyRates);
            var totalPayment = loanAmount + totalInterest;

            return new LoanCalculationResult
            {
                TotalInterest = totalInterest,
                TotalPayment = totalPayment
            };
        }

        public string FormatCalculationResult(LoanCalculationResult result, decimal[] yearlyRates)
        {
            var ratesByYear = string.Join("\n", yearlyRates.Select((rate, index) =>
            $"Year {index + 1} rate: {rate}%"));
            
            return $"Rates by year:\n{ratesByYear}\n\n"+
                   $"The total interest is:{result.TotalInterest:F2} USD.\n" +
                   $"The total payment is: {result.TotalPayment:F2} USD.";
        }
    }
}