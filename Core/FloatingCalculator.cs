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
        //метод  для расчета по плавающей ставке
        public decimal CalculateTotalPayment()
        {
            int halfYersinMonth = 6; ;

            // Рассчитываем для первого периода (6 месяцев)
            decimal firstPeriodInterest = LoanAmount*(FirstRate/100)*(halfYersinMonth/12m);
            decimal firstPeriodTotal = LoanAmount + firstPeriodInterest;

            // Рассчитываем для второго периода (6 месяцев)

            decimal secondPeriodInterest = LoanAmount * (SecondRate / 100) * (halfYersinMonth / 12m);
            decimal secondPeriodTotal = LoanAmount + secondPeriodInterest;

            // Общая сумма платежей
            decimal totalPayment = firstPeriodTotal + secondPeriodTotal;
            return totalPayment;
        }
        // Метод для вычисления общей суммы процентов по обоим периодам

        public decimal CalculateTotalInterest()
        {
            decimal firstPeriodInterest = LoanAmount* (FirstRate/100)*(6/12m);
            decimal secondPeriodInterest = LoanAmount * (SecondRate / 100) * (6 / 12m);
            return firstPeriodInterest + secondPeriodInterest;
        }
    }
}
