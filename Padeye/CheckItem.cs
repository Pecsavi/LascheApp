namespace LascheApp.Padeye
{
    public class CheckItem
    {
        public string Name { get; set; } = "";
        public double Utilization { get; set; }
        public bool IsOk { get; set; }
        public bool ShowUtilization { get; set; } = true;
    }
}