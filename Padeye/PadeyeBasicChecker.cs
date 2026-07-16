
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

            if (input.LugType == LugType.TransportLug && input.F_Ed_ser_kN <= 0)
                result.Errors.Add("F_Ed,ser must be greater than 0.");

            if (input.PlateThickness_mm <= 0)
                result.Errors.Add("Main plate thickness tpl must be greater than 0.");

            if (input.HoleDiameter_mm <= 0)
                result.Errors.Add("Hole diameter d0 must be greater than 0.");

            if (input.PlateWidth_mm <= 0)
                result.Errors.Add("Plate width b must be greater than 0.");

            if (input.PlateWidth_mm <= input.HoleDiameter_mm)
                result.Errors.Add("Plate width b must be greater than hole diameter d0.");

            if (input.CheekPlateThickness_mm < 0)
                result.Errors.Add("Cheek plate thickness tch must not be negative.");

            if (input.LugType == LugType.TransportLug)
            {
                if (input.ShackleDpin_mm <= 0)
                    result.Errors.Add("Shackle pin diameter Dpin must be greater than 0.");

                if (input.ShackleB1_mm <= 0)
                    result.Errors.Add("Shackle B1 must be greater than 0.");

                if (input.ShackleWLL_kN <= 0)
                    result.Errors.Add("Shackle WLL must be greater than 0.");
            }

            if (input.MaterialFy_Nmm2 <= 0)
                result.Errors.Add("Material fy must be greater than 0.");

            if (input.GammaM0 <= 0)
                result.Errors.Add("gammaM0 must be greater than 0.");

            if (result.HasErrors)
                return result;

            double thicknessForB1Check_mm = input.IncludeCheekPlatesInBearing
                ? input.TotalBearingThickness_mm
                : input.PlateThickness_mm;

            double requiredThickness_mm = input.LugType == LugType.TransportLug
                ? 0.75 * input.ShackleB1_mm
                : 0.0;

            result.RecommendedHoleDiameterMin_mm = input.ShackleDpin_mm;
            result.RecommendedHoleDiameterMax_mm = input.ShackleDpin_mm + input.PinClearance_mm;
            result.RequiredThickness_mm = requiredThickness_mm;
            result.ThicknessForB1Check_mm = thicknessForB1Check_mm;

            if (input.LugType == LugType.TransportLug)
            {
                result.WllOk = input.F_Ed_ser_kN <= input.ShackleWLL_kN;
                result.WllUtilization = input.F_Ed_ser_kN / input.ShackleWLL_kN;

                result.HoleDiameterRecommendationOk =
                    input.HoleDiameter_mm > input.ShackleDpin_mm &&
                    input.HoleDiameter_mm <= result.RecommendedHoleDiameterMax_mm;

                result.HoleDiameterRecommendationUtilization =
                    input.HoleDiameter_mm <= result.RecommendedHoleDiameterMax_mm
                        ? input.ShackleDpin_mm / input.HoleDiameter_mm
                        : input.HoleDiameter_mm / result.RecommendedHoleDiameterMax_mm;

                result.ThicknessRecommendationOk =
                    thicknessForB1Check_mm >= requiredThickness_mm;

                result.ThicknessRecommendationUtilization =
                    requiredThickness_mm / thicknessForB1Check_mm;

                result.CheekPlateThicknessLimitOk =
                    !input.IncludeCheekPlatesInBearing ||
                    input.CheekPlateThickness_mm <= input.MaxCheekPlateThickness_mm;
            }
            else
            {
                result.WllOk = true;
                result.HoleDiameterRecommendationOk = true;
                result.ThicknessRecommendationOk = true;
                result.CheekPlateThicknessLimitOk = true;
            }

            double grossArea_mm2 = input.PlateWidth_mm * input.PlateThickness_mm;
            double netArea_mm2 = (input.PlateWidth_mm - input.HoleDiameter_mm) * input.PlateThickness_mm;
            double sigmaRd_Nmm2 = input.MaterialFy_Nmm2 / input.GammaM0;

            result.GrossArea_mm2 = grossArea_mm2;
            result.SigmaGrossEd_Nmm2 = input.F_Ed_kN * 1000.0 / grossArea_mm2;
            result.GrossSectionTensionOk = result.SigmaGrossEd_Nmm2 <= sigmaRd_Nmm2;
            result.GrossSectionTensionUtilization = result.SigmaGrossEd_Nmm2 / sigmaRd_Nmm2;

            result.NetArea_mm2 = netArea_mm2;
            result.SigmaEd_Nmm2 = input.F_Ed_kN * 1000.0 / netArea_mm2;
            result.SigmaRd_Nmm2 = sigmaRd_Nmm2;
            result.NetSectionTensionOk = result.SigmaEd_Nmm2 <= sigmaRd_Nmm2;
            result.NetSectionTensionUtilization = result.SigmaEd_Nmm2 / sigmaRd_Nmm2;

            return result;
        }
    }
}
