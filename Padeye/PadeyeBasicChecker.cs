namespace LascheApp.Padeye
{
    public static class PadeyeBasicChecker
    {
        public static PadeyeBasicCheckResult Check(PadeyeBasicCheckInput input)
        {
            PadeyeBasicCheckResult result = new PadeyeBasicCheckResult
            {
                Input = input
            };

            if (input.F_Ed_kN <= 0)
                result.Errors.Add("F_Ed must be greater than 0.");

            if (input.PlateThickness_mm <= 0)
                result.Errors.Add("Plate thickness t must be greater than 0.");

            if (input.HoleDiameter_mm <= 0)
                result.Errors.Add("Hole diameter d0 must be greater than 0.");

            if (input.PlateWidth_mm <= 0)
                result.Errors.Add("Plate width b must be greater than 0.");

            if (input.PlateWidth_mm <= input.HoleDiameter_mm)
                result.Errors.Add("Plate width b must be greater than hole diameter d0.");

            if (input.ShackleDpin_mm <= 0)
                result.Errors.Add("Shackle pin diameter Dpin must be greater than 0.");

            if (input.ShackleB1_mm <= 0)
                result.Errors.Add("Shackle B1 must be greater than 0.");

            if (input.ShackleWLL_kN <= 0)
                result.Errors.Add("Shackle WLL must be greater than 0.");

            if (input.MaterialFy_Nmm2 <= 0)
                result.Errors.Add("Material fy must be greater than 0.");

            if (input.GammaM0 <= 0)
                result.Errors.Add("gammaM0 must be greater than 0.");

            if (result.HasErrors)
                return result;

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

            double bearingArea_mm2 =
                input.ShackleDpin_mm * input.PlateThickness_mm;

            double sigmaBearingEd_Nmm2 =
                input.F_Ed_kN * 1000.0 / bearingArea_mm2;

            result.RequiredHoleDiameter_mm = requiredHoleDiameter_mm;
            result.RequiredThickness_mm = requiredThickness_mm;

            result.WllOk = input.F_Ed_kN <= input.ShackleWLL_kN;
            result.WllUtilization = input.F_Ed_kN / input.ShackleWLL_kN;
            result.HoleDiameterOk = input.HoleDiameter_mm >= requiredHoleDiameter_mm;
            result.HoleDiameterUtilization = requiredHoleDiameter_mm / input.HoleDiameter_mm;
            result.ThicknessOk = input.PlateThickness_mm >= requiredThickness_mm;

            result.GrossArea_mm2 = grossArea_mm2;
            result.SigmaGrossEd_Nmm2 = sigmaGrossEd_Nmm2;
            result.GrossSectionTensionOk = sigmaGrossEd_Nmm2 <= sigmaRd_Nmm2;
            result.GrossSectionTensionUtilization = sigmaGrossEd_Nmm2 / sigmaRd_Nmm2;

            result.NetArea_mm2 = netArea_mm2;
            result.SigmaEd_Nmm2 = sigmaEd_Nmm2;
            result.SigmaRd_Nmm2 = sigmaRd_Nmm2;
            result.NetSectionTensionOk = sigmaEd_Nmm2 <= sigmaRd_Nmm2;
            result.NetSectionTensionUtilization = sigmaEd_Nmm2 / sigmaRd_Nmm2;

            result.BearingArea_mm2 = bearingArea_mm2;
            result.SigmaBearingEd_Nmm2 = sigmaBearingEd_Nmm2;
            result.BearingOk = sigmaBearingEd_Nmm2 <= sigmaRd_Nmm2;
            result.BearingUtilization = sigmaBearingEd_Nmm2 / sigmaRd_Nmm2;

            return result;
        }
    }
}
