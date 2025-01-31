using System.Linq;
using TelegramBot_Fitz.Bot;

namespace TelegramBot_Fitz.Core
{
    public class LoanCalculationResult
    {
        public decimal TotalInterest { get; set; }
        public decimal TotalPayment { get; set; }

        public YearlyCalculation[] YearlyCalculations { get; set; }
    }

    public class YearlyCalculation
    {
        public int Year { get; set; }
        public decimal Rate { get; set; }
        public decimal Interest { get; set; }
        public decimal AccumulatedAmount { get; set; }
    }

        public class FixedRateLoanCalculator
    {
        public decimal CalculateInterest(decimal amount, decimal[] yearlyRates, InterestCalculationType calculationType)
        {
            decimal totalInterest = 0;
            decimal currentAmount = amount;

            for(int i = 0; i < yearlyRates.Length; i++)
            {
                decimal yearlyInterest;

                if(calculationType == InterestCalculationType.Simple)
                {
                    yearlyInterest = amount * (yearlyRates[i] / 100);
                    totalInterest += yearlyInterest;
                }
                else
                {
                    yearlyInterest = currentAmount * (yearlyRates[i] / 100);
                    currentAmount += yearlyInterest;
                    totalInterest += yearlyInterest;
                }
            }
            return totalInterest;
        }

        public LoanCalculationResult CalculateLoan(decimal loanAmount, decimal[] yearlyRates, InterestCalculationType calculationType)
        {
            var yearlyCalculations = new YearlyCalculation[yearlyRates.Length];
            decimal totalInterest = 0;
            decimal currentAmount = loanAmount;

            // Расчет для каждого года
            for (int i = 0; i < yearlyRates.Length; i++)
            {
                decimal yearlyInterest;
                if (calculationType == InterestCalculationType.Simple)
                {
                    yearlyInterest = loanAmount * (yearlyRates[i] / 100);
                    yearlyCalculations[i] = new YearlyCalculation
                    {
                        Year = i + 1,
                        Rate = yearlyRates[i],
                        Interest = yearlyInterest,
                        AccumulatedAmount = loanAmount + yearlyInterest
                    };
                }
                else // Compound
                {
                    yearlyInterest = currentAmount * (yearlyRates[i] / 100);
                    currentAmount += yearlyInterest;
                    yearlyCalculations[i] = new YearlyCalculation
                    {
                        Year = i + 1,
                        Rate = yearlyRates[i],
                        Interest = yearlyInterest,
                        AccumulatedAmount = currentAmount
                    };
                }
                totalInterest += yearlyInterest;
            }

            return new LoanCalculationResult
            {
                TotalInterest = totalInterest,
                TotalPayment = loanAmount + totalInterest,
                YearlyCalculations = yearlyCalculations
            };
        }


    }
}