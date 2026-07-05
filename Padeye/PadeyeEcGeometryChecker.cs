using System;

namespace LascheApp.Padeye
{
    public static class PadeyeEcGeometryChecker
    {
        public static PadeyeEcGeometryResult Check(PadeyeEcGeometryInput input)
        {
            PadeyeEcGeometryResult result = new PadeyeEcGeometryResult
            {
                Input = input
            };

            if (input.F_Ed_kN <= 0)
                result.Errors.Add("F_Ed must be greater than 0.");

            if (input.GammaM0 <= 0)
                result.Errors.Add("gammaM0 must be greater than 0.");

            if (input.Fy_Nmm2 <= 0)
                result.Errors.Add("fy must be greater than 0.");

            if (input.PlateThickness_mm <= 0)
                result.Errors.Add("Plate thickness t must be greater than 0.");

            if (input.HoleDiameter_mm <= 0)
                result.Errors.Add("Hole diameter d0 must be greater than 0.");

            if (input.EdgeDistanceA_mm <= 0)
                result.Errors.Add("Edge distance a must be greater than 0.");

            if (input.SideDistanceC_mm <= 0)
                result.Errors.Add("Side distance c must be greater than 0.");

            if (result.HasErrors)
                return result;

            double fEd_N = input.F_Ed_kN * 1000.0;

            double forceTerm_mm =
                fEd_N * input.GammaM0 /
                (2.0 * input.PlateThickness_mm * input.Fy_Nmm2);

            double requiredEdgeDistanceA_mm =
                forceTerm_mm + 2.0 * input.HoleDiameter_mm / 3.0;

            double requiredSideDistanceC_mm =
                forceTerm_mm + input.HoleDiameter_mm / 3.0;

            double requiredThickness_mm =
                0.7 * Math.Sqrt(fEd_N * input.GammaM0 / input.Fy_Nmm2);

            double maxHoleDiameter_mm =
                2.5 * input.PlateThickness_mm;

            result.RequiredEdgeDistanceA_mm = requiredEdgeDistanceA_mm;
            result.RequiredSideDistanceC_mm = requiredSideDistanceC_mm;

            result.EdgeDistanceA_Ok =
                input.EdgeDistanceA_mm >= requiredEdgeDistanceA_mm;

            result.SideDistanceC_Ok =
                input.SideDistanceC_mm >= requiredSideDistanceC_mm;

            result.EdgeDistanceA_Utilization =
                requiredEdgeDistanceA_mm / input.EdgeDistanceA_mm;

            result.SideDistanceC_Utilization =
                requiredSideDistanceC_mm / input.SideDistanceC_mm;

            result.RequiredThickness_MoglichkeitB_mm = requiredThickness_mm;
            result.MaxHoleDiameter_MoglichkeitB_mm = maxHoleDiameter_mm;

            result.ThicknessMoglichkeitB_Ok =
                input.PlateThickness_mm >= requiredThickness_mm;

            result.HoleDiameterMoglichkeitB_Ok =
                input.HoleDiameter_mm <= maxHoleDiameter_mm;

            result.ThicknessMoglichkeitB_Utilization =
                requiredThickness_mm / input.PlateThickness_mm;

            result.HoleDiameterMoglichkeitB_Utilization =
                input.HoleDiameter_mm / maxHoleDiameter_mm;

            return result;
        }
    }
}