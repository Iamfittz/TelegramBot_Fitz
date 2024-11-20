using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot_Fitz.Core
{
    public class FixedRateLoanCalculator
    {
        public decimal CalculateInterest(decimal amount, int years, decimal rate)
        {
            return amount * (rate / 100) * years;
        }
        // Метод для расчёта полной суммы платежей для фиксированной ставки
        public decimal CalculateTotalPayment(decimal loanAmount, int loanYears, decimal interestRate)
        {
            decimal totalInterest = loanAmount * (interestRate / 100) * loanYears; // Общий процент за срок
            return loanAmount + totalInterest; // Сумма кредита + проценты
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


            Console.WriteLine($"\nThe total interest for {loanYears}: {totalInterest:F2} uah");
            Console.WriteLine($"Total payment is {totalPayment:F2} uah");
        }
    }
}
