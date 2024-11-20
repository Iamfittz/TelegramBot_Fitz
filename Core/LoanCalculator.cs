using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public class LoanCalculator
    {
        public decimal CalculateInterest(decimal amount, int years, decimal rate)
        {
            return amount * (rate / 100) * years;
        }

        public void Run()
        {
            Console.WriteLine("Enter the amount");
            string InputAmount = Console.ReadLine();
            if (!decimal.TryParse(InputAmount, out decimal loanAmount) || loanAmount <= 0)
            {
                Console.WriteLine("Number must be a positive");
                return;
            }

            Console.WriteLine("Enter the number of years");
            string InputYears = Console.ReadLine();
            if (!int.TryParse(InputYears, out int loanYears) || loanYears <= 0)
            {
                Console.WriteLine("Number must be a positive");
                return;
            }

            Console.WriteLine("Enter the interest rates, eg. 4 for 4%");
            string InputRate = Console.ReadLine();
            if (!decimal.TryParse(InputRate, out decimal rate) || rate <= 0)
            {
                Console.WriteLine("Number must be a positive");
                return;
            }

            decimal totalInterest = loanAmount * (rate / 100) * loanYears;
            decimal totalPayment = loanAmount + totalInterest;


            Console.WriteLine($"\nThe total interest for {loanYears}: {totalInterest:F2} usd");
            Console.WriteLine($"Total payment is {totalPayment:F2} usd");
        }
    }
}
