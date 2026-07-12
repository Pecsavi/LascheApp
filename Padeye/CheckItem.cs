namespace LascheApp.Padeye
{
    public class CheckItem
    {
        public string Name { get; set; } = "";
        public double Utilization { get; set; }
        public bool IsOk { get; set; }
        public bool ShowUtilization { get; set; } = true;

        // Warning/information item only. Does not make the whole check NOT OK.
        public bool IsWarning { get; set; } = false;
        public string? Note { get; set; }
    }
}
