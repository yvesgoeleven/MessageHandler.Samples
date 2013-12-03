namespace PhoneApp1
{
    public class ViewModel
    {
        public ViewModel()
        {
            Temperature = new Temperature();
        }

        public static Temperature Temperature { get; set; }

    }
}
