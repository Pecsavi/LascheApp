using System;

namespace LascheApp.Padeye
{
    public static class PadeyeEcGeometryChecker
    {
        public static PadeyeEcGeometryResult Check(PadeyeEcGeometryInput input)
        {
            PadeyeEcGeometryResult result = new()
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

            if (input.CheekPlateThickness_mm < 0)
                result.Errors.Add("Cheek plate thickness tch must not be negative.");

            if (input.HoleDiameter_mm <= 0)
                result.Errors.Add("Hole diameter d0 must be greater than 0.");

            if (input.EdgeDistanceA_mm <= 0)
                result.Errors.Add("Edge distance a must be greater than 0.");

            if (input.SideDistanceC_mm <= 0)
                result.Errors.Add("Side distance c must be greater than 0.");

            if (result.HasErrors)
                return result;

            double fEd_N = input.F_Ed_kN * 1000.0;

            double t_mm = input.EffectiveThickness_mm;


            if (t_mm <= 0)
            {
                result.Errors.Add("Effective thickness t must be greater than 0.");
                return result;
            }

            double forceTerm_mm =
                fEd_N * input.GammaM0 /
                (2.0 * t_mm * input.Fy_Nmm2);



            result.RequiredEdgeDistanceA_mm =
                forceTerm_mm + 2.0 * input.HoleDiameter_mm / 3.0;

            result.RequiredSideDistanceC_mm =
                forceTerm_mm + input.HoleDiameter_mm / 3.0;

            result.EdgeDistanceA_Ok =
                input.EdgeDistanceA_mm >= result.RequiredEdgeDistanceA_mm;

            result.SideDistanceC_Ok =
                input.SideDistanceC_mm >= result.RequiredSideDistanceC_mm;

            // EC geometry is a dimensional pass/fail check, not a resistance
            // utilization check. Keep utilization at zero by design.
            result.EdgeDistanceA_Utilization = 0.0;
            result.SideDistanceC_Utilization = 0.0;
            
            double e_mm =
                input.EdgeDistanceA_mm +
                input.HoleDiameter_mm / 2.0;

            double b_mm =
                2.0 * input.SideDistanceC_mm +
                input.HoleDiameter_mm;

            result.RequiredThickness_MoglichkeitB_mm =
                0.7 * Math.Sqrt(fEd_N * input.GammaM0 / input.Fy_Nmm2);

            double forceResistanceB_N =
                Math.Pow(t_mm / 0.7, 2.0) *
                input.Fy_Nmm2 /
                input.GammaM0;

            result.ForceResistance_MoglichkeitB_kN =
                forceResistanceB_N / 1000.0;

            result.MaxHoleDiameter_MoglichkeitB_mm =
                2.5 * t_mm;

            result.RequiredEdgeDistance_MoglichkeitB_mm =
                1.6 * input.HoleDiameter_mm;

            result.RequiredPlateWidth_MoglichkeitB_mm =
                2.5 * input.HoleDiameter_mm;

            result.ThicknessMoglichkeitB_Ok =
                t_mm >= result.RequiredThickness_MoglichkeitB_mm;

            result.HoleDiameterMoglichkeitB_Ok =
                input.HoleDiameter_mm <= result.MaxHoleDiameter_MoglichkeitB_mm;

            result.EdgeDistanceMoglichkeitB_Ok =
                e_mm >= result.RequiredEdgeDistance_MoglichkeitB_mm;

            result.PlateWidthMoglichkeitB_Ok =
                b_mm >= result.RequiredPlateWidth_MoglichkeitB_mm;

            result.ForceMoglichkeitB_Utilization = 0.0;
            result.HoleDiameterMoglichkeitB_Utilization = 0.0;
            result.EdgeDistanceMoglichkeitB_Utilization = 0.0;
            result.PlateWidthMoglichkeitB_Utilization = 0.0;

            return result;
            
        }
    }
}
