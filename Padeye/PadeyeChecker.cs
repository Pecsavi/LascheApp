namespace LascheApp.Padeye
{
    public static class PadeyeChecker
    {
        public static PadeyeCheckResult Check(PadeyeCheckInput input)
        {
            PadeyeBasicCheckInput basicInput = new PadeyeBasicCheckInput
            {
                LugType = input.LugType,
                F_Ed_kN = input.F_Ed_kN,
                F_Ed_ser_kN = input.F_Ed_ser_kN,
                PlateThickness_mm = input.PlateThickness_mm,
                HoleDiameter_mm = input.HoleDiameter_mm,
                PlateWidth_mm = input.PlateWidth_mm,
                MaterialFy_Nmm2 = input.Fy_Nmm2,
                GammaM0 = input.GammaM0,
                ShackleWLL_kN = input.ShackleWLL_kN,
                ShackleDpin_mm = input.ShackleDpin_mm,
                ShackleB1_mm = input.ShackleB1_mm,
                ShackleH_DNV_mm = input.ShackleH_DNV_mm,
                PinClearance_mm = input.PinClearance_mm,
                CheekPlateThickness_mm = input.CheekPlateThickness_mm,
                IncludeCheekPlatesInBearing = input.IncludeCheekPlatesInBearing
            };

            PadeyeEcGeometryInput ecGeometryInput = new PadeyeEcGeometryInput
            {
                F_Ed_kN = input.F_Ed_kN,
                GammaM0 = input.GammaM0,
                Fy_Nmm2 = input.Fy_Nmm2,

                PlateThickness_mm = input.PlateThickness_mm,
                CheekPlateThickness_mm = input.CheekPlateThickness_mm,
                IncludeCheekPlatesInBearing = input.IncludeCheekPlatesInBearing,

                HoleDiameter_mm = input.HoleDiameter_mm,
                EdgeDistanceA_mm = input.EdgeDistanceA_mm,
                SideDistanceC_mm = input.SideDistanceC_mm
            };

            double cheekPlateThicknessForResistance_mm = input.IncludeCheekPlatesInBearing
                ? input.CheekPlateThickness_mm
                : 0.0;

            PadeyeDnvOutOfPlaneInput dnvOutOfPlaneInput = new PadeyeDnvOutOfPlaneInput
            {
                F_Ed_kN = input.F_Ed_kN,
                Alpha_deg = input.DnvOutOfPlaneAngle_deg,
                H_DNV_mm = input.ShackleH_DNV_mm,
                Dpin_mm = input.ShackleDpin_mm,
                HoleDiameter_mm = input.HoleDiameter_mm,
                MainPlateThickness_mm = input.PlateThickness_mm,
                CheekPlateThickness_mm = cheekPlateThicknessForResistance_mm,
                Rpl_mm = input.Rpl_mm,
                Rch_mm = input.Rch_mm,
                WeldA_mm = input.CheekPlateWeldA_mm,
                Fy_Nmm2 = input.Fy_Nmm2,
                Fu_Nmm2 = input.Fu_Nmm2,
                E_Nmm2 = input.E_Nmm2,
                BetaW = input.BetaW,
                Beta = input.DnvBeta,
                GammaM = input.DnvGammaM,
                GammaM2 = input.GammaM2
            };

            PadeyeBearingInput bearingInput = new PadeyeBearingInput
            {
                F_Ed_kN = input.F_Ed_kN,
                F_Ed_ser_kN = input.F_Ed_ser_kN,
                PlateThickness_mm = input.BearingThickness_mm,
                HoleDiameter_mm = input.HoleDiameter_mm,
                PinDiameter_mm = input.ShackleDpin_mm,
                Fy_Nmm2 = input.Fy_Nmm2,
                E_Nmm2 = input.E_Nmm2,
                GammaM0 = input.GammaM0,
                GammaM6_ser = input.GammaM6_ser,
                IsReplaceablePin = input.IsReplaceablePin
            };

            PadeyeBasicCheckResult basicResult = PadeyeBasicChecker.Check(basicInput);
            PadeyeEcGeometryResult ecGeometryResult = PadeyeEcGeometryChecker.Check(ecGeometryInput);
            PadeyeDnvOutOfPlaneResult dnvOutOfPlaneResult = PadeyeDnvOutOfPlaneChecker.Check(dnvOutOfPlaneInput);
            PadeyeBearingResult bearingResult = PadeyeBearingChecker.Check(bearingInput);

            return new PadeyeCheckResult
            {
                BasicResult = basicResult,
                EcGeometryResult = ecGeometryResult,
                DnvOutOfPlaneResult = dnvOutOfPlaneResult,
                BearingResult = bearingResult
            };
        }
    }
}
