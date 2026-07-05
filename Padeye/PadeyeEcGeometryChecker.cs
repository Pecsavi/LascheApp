using System;

namespace LascheApp.Padeye
{
    public static class PadeyeEcGeometryChecker
    {
        public static PadeyeEcGeometryResult Check(PadeyeEcGeometryInput input)
        {
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

            return new PadeyeEcGeometryResult
            {
                Input = input,

                RequiredEdgeDistanceA_mm = requiredEdgeDistanceA_mm,
                RequiredSideDistanceC_mm = requiredSideDistanceC_mm,

                EdgeDistanceA_Ok =
                    input.EdgeDistanceA_mm >= requiredEdgeDistanceA_mm,

                SideDistanceC_Ok =
                    input.SideDistanceC_mm >= requiredSideDistanceC_mm,

                RequiredThickness_MoglichkeitB_mm = requiredThickness_mm,
                MaxHoleDiameter_MoglichkeitB_mm = maxHoleDiameter_mm,

                ThicknessMoglichkeitB_Ok =
                    input.PlateThickness_mm >= requiredThickness_mm,

                HoleDiameterMoglichkeitB_Ok =
                    input.HoleDiameter_mm <= maxHoleDiameter_mm
            };
        }
    }
}