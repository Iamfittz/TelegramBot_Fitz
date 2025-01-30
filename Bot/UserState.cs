namespace TelegramBot_Fitz.Bot
{
    public enum CalculationType
    {
        None,
        FixedRate,
        FloatingRate,
        OIS
    }

    public class UserState
    {
        public int Step { get; set; } = 0;  
        public decimal LoanAmount { get; set; }
        public int LoanYears { get; set; }
        public int Days {  get; set; }
        public decimal FirstRate { get; set; }   
        public decimal SecondRate { get; set; }
        public CalculationType CalculationType { get; set; } = CalculationType.None; // Тип расчета (Fixed или Floating)

        public void Reset()
        {
            Step = 0;
            LoanAmount = 0;
            LoanYears = 0;
            Days = 0;
            FirstRate = 0;
            SecondRate = 0;
            CalculationType = CalculationType.None; // сбрасываем тип расчета
            
        }
    }
}
