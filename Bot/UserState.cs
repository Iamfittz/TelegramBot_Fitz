namespace TelegramBot_Fitz.Bot
{
    public enum CalculationType
    {
        None,
        FixedRate,
        FloatingRate
    }

    public class UserState
    {
        public int Step { get; set; } = 0;  
        public decimal LoanAmount { get; set; }
        public int LoanYears { get; set; }
        public decimal InterestRate { get; set; }
        public CalculationType CalculationType { get; set; } = CalculationType.None; // Тип расчета (Fixed или Floating)

        public void Reset()
        {
            Step = 0;
            LoanAmount = 0;
            LoanYears = 0;
            InterestRate = 0;
            CalculationType = CalculationType.None; // сбрасываем тип расчета
        }
    }
}
