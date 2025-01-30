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
        public decimal CalculateInterest(decimal amount, int years, decimal rate)
        {
            return amount * (rate / 100) * years;
        }

        public decimal CalculateTotalPayment(decimal loanAmount, int loanYears, decimal interestRate)
        {
            decimal totalInterest = CalculateInterest(loanAmount, loanYears, interestRate);
            return loanAmount + totalInterest;
        }

        public LoanCalculationResult CalculateLoan(decimal loanAmount, int loanYears, decimal interestRate)
        {
            var totalInterest = CalculateInterest(loanAmount, loanYears, interestRate);
            var totalPayment = CalculateTotalPayment(loanAmount, loanYears, interestRate);

            return new LoanCalculationResult
            {
                TotalInterest = totalInterest,
                TotalPayment = totalPayment
            };
        }

        public string FormatCalculationResult(LoanCalculationResult result, int loanYears)
        {
            return $"The total interest for {loanYears} years is: {result.TotalInterest:F2} USD.\n" +
                   $"The total payment is: {result.TotalPayment:F2} USD.";
        }
    }
}