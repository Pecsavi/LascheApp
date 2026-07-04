namespace LascheApp.Padeye
{
    public class PadeyeBasicCheckInput
    {
        public double F_Ed_kN { get; set; }
        public double PlateThickness_mm { get; set; }
        public double HoleDiameter_mm { get; set; }

        public double ShackleWLL_kN { get; set; }
        public double ShackleDpin_mm { get; set; }
        public double ShackleB1_mm { get; set; }
        public double ShackleH_DNV_mm { get; set; }
        public double PinClearance_mm { get; set; } = 2.0;
        public double PlateWidth_mm { get; set; }
        public double MaterialFy_Nmm2 { get; set; }
        public double GammaM0 { get; set; } = 1.0;
    }

    public class PadeyeBasicCheckResult
    {
        public PadeyeBasicCheckInput Input { get; set; } = new();

        public double RequiredHoleDiameter_mm { get; set; }
        public double RequiredThickness_mm { get; set; }

        public bool WllOk { get; set; }
        public bool HoleDiameterOk { get; set; }
        public bool ThicknessOk { get; set; }

        public double GrossArea_mm2 { get; set; }
        public double SigmaGrossEd_Nmm2 { get; set; }
        public bool GrossSectionTensionOk { get; set; }

        public double NetArea_mm2 { get; set; }
        public double SigmaEd_Nmm2 { get; set; }
        public double SigmaRd_Nmm2 { get; set; }
        public bool NetSectionTensionOk { get; set; }

        public bool IsOk =>
            WllOk &&
            HoleDiameterOk &&
            ThicknessOk &&
            GrossSectionTensionOk &&
            NetSectionTensionOk;
    }
}