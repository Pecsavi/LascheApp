namespace LascheApp.Padeye
{
    public static class PadeyeChecker
    {
        public static PadeyeCheckResult Check(PadeyeCheckInput input)
        {
            PadeyeBasicCheckInput basicInput = new PadeyeBasicCheckInput
            {
                F_Ed_kN = input.F_Ed_kN,
                PlateThickness_mm = input.PlateThickness_mm,
                HoleDiameter_mm = input.HoleDiameter_mm,
                PlateWidth_mm = input.PlateWidth_mm,

                MaterialFy_Nmm2 = input.Fy_Nmm2,
                GammaM0 = input.GammaM0,

                ShackleWLL_kN = input.ShackleWLL_kN,
                ShackleDpin_mm = input.ShackleDpin_mm,
                ShackleB1_mm = input.ShackleB1_mm,
                ShackleH_DNV_mm = input.ShackleH_DNV_mm,

                PinClearance_mm = input.PinClearance_mm
            };

            PadeyeEcGeometryInput ecGeometryInput = new PadeyeEcGeometryInput
            {
                F_Ed_kN = input.F_Ed_kN,
                GammaM0 = input.GammaM0,
                Fy_Nmm2 = input.Fy_Nmm2,

                PlateThickness_mm = input.PlateThickness_mm,
                HoleDiameter_mm = input.HoleDiameter_mm,

                EdgeDistanceA_mm = input.EdgeDistanceA_mm,
                SideDistanceC_mm = input.SideDistanceC_mm
            };

            PadeyeOutOfPlaneInput outOfPlaneInput = new PadeyeOutOfPlaneInput
            {
                F_Ed_kN = input.F_Ed_kN,
                H_DNV_mm = input.ShackleH_DNV_mm,

                PlateWidth_mm = input.PlateWidth_mm,
                PlateThickness_mm = input.PlateThickness_mm,

                Fy_Nmm2 = input.Fy_Nmm2,
                GammaM0 = input.GammaM0
            };

            PadeyeBasicCheckResult basicResult =
                PadeyeBasicChecker.Check(basicInput);

            PadeyeEcGeometryResult ecGeometryResult =
                PadeyeEcGeometryChecker.Check(ecGeometryInput);

            PadeyeOutOfPlaneResult outOfPlaneResult =
                PadeyeOutOfPlaneChecker.Check(outOfPlaneInput);

            return new PadeyeCheckResult
            {
                BasicResult = basicResult,
                EcGeometryResult = ecGeometryResult,
                OutOfPlaneResult = outOfPlaneResult
            };
        }
    }
}