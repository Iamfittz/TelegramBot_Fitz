using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramBot_Fitz.Core;

namespace TelegramBot_Fitz
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welocme to the loan calculator");

            var loanCalculator = new LoanCalculator();

            loanCalculator.Run();
        }
        }
    }

