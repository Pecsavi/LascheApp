namespace LascheApp.Padeye
{
    public static class PadeyeBasicChecker
    {
        public static PadeyeBasicCheckResult Check(PadeyeBasicCheckInput input)
        {
            double requiredHoleDiameter_mm =
                input.ShackleDpin_mm + input.PinClearance_mm;

            double requiredThickness_mm =
                0.75 * input.ShackleB1_mm;
            double grossArea_mm2 =
     input.PlateWidth_mm * input.PlateThickness_mm;

            double sigmaGrossEd_Nmm2 =
                input.F_Ed_kN * 1000.0 / grossArea_mm2;

            double netArea_mm2 =
    (input.PlateWidth_mm - input.HoleDiameter_mm) * input.PlateThickness_mm;

            double sigmaEd_Nmm2 =
                input.F_Ed_kN * 1000.0 / netArea_mm2;

            double sigmaRd_Nmm2 =
                input.MaterialFy_Nmm2 / input.GammaM0;


            return new PadeyeBasicCheckResult
            {
                Input = input,

                RequiredHoleDiameter_mm = requiredHoleDiameter_mm,
                RequiredThickness_mm = requiredThickness_mm,

                WllOk = input.F_Ed_kN <= input.ShackleWLL_kN,
                HoleDiameterOk = input.HoleDiameter_mm >= requiredHoleDiameter_mm,
                ThicknessOk = input.PlateThickness_mm >= requiredThickness_mm,

                GrossArea_mm2 = grossArea_mm2,
                SigmaGrossEd_Nmm2 = sigmaGrossEd_Nmm2,
                GrossSectionTensionOk = sigmaGrossEd_Nmm2 <= sigmaRd_Nmm2,

                NetArea_mm2 = netArea_mm2,
                SigmaEd_Nmm2 = sigmaEd_Nmm2,
                SigmaRd_Nmm2 = sigmaRd_Nmm2,
                NetSectionTensionOk = sigmaEd_Nmm2 <= sigmaRd_Nmm2
            };
        }
    }
}
