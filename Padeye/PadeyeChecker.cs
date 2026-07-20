using System;


namespace LascheApp.Padeye
{
    public static class PadeyeChecker
    {
        public static PadeyeCheckResult Check(PadeyeCheckInput input)
        {
            bool cheekPlatesRequestedForBearing =
                input.IncludeCheekPlatesInBearing;

            bool cheekPlateBearingGeometryOk =
                CheckCheekPlateBearingGeometry(
                    input,
                    out double requiredCheekPlateE_MethodA_mm,
                    out double requiredCheekPlateE_MethodB_mm,
                    out double requiredCheekPlateE_mm);

            bool includeCheekPlatesInBearingEffective =
                cheekPlatesRequestedForBearing &&
                cheekPlateBearingGeometryOk;

            double bearingThickness_mm =
                includeCheekPlatesInBearingEffective
                    ? input.PlateThickness_mm + 2.0 * input.CheekPlateThickness_mm
                    : input.PlateThickness_mm;

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
                IncludeCheekPlatesInBearingRequested = cheekPlatesRequestedForBearing,
                IncludeCheekPlatesInBearing = includeCheekPlatesInBearingEffective,
                CheekPlateRadiusRch_mm = input.Rch_mm,
                CheekPlateBearingGeometryOk = cheekPlateBearingGeometryOk,
                CheekPlateBearingRequiredE_MethodA_mm = requiredCheekPlateE_MethodA_mm,
                CheekPlateBearingRequiredE_MethodB_mm = requiredCheekPlateE_MethodB_mm,
                CheekPlateBearingRequiredE_mm = requiredCheekPlateE_mm
            };

            PadeyeEcGeometryInput ecGeometryInput = new PadeyeEcGeometryInput
            {
                F_Ed_kN = input.F_Ed_kN,
                GammaM0 = input.GammaM0,
                Fy_Nmm2 = input.Fy_Nmm2,

                PlateThickness_mm = input.PlateThickness_mm,
                CheekPlateThickness_mm = input.CheekPlateThickness_mm,
                IncludeCheekPlatesInBearing = includeCheekPlatesInBearingEffective,

                HoleDiameter_mm = input.HoleDiameter_mm,
                EdgeDistanceA_mm = input.EdgeDistanceA_mm,
                SideDistanceC_mm = input.SideDistanceC_mm
            };

            double cheekPlateThicknessForResistance_mm =
             includeCheekPlatesInBearingEffective
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
                EndDistanceE_mm = input.EndDistanceE_mm,
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
                PlateThickness_mm = bearingThickness_mm,
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

            OuterLugCheckResult outerLugResult = OuterLugChecker.Check(new OuterLugCheckInput
            {
                IsActive = input.LugType == LugType.TensionLug && input.OuterLugThicknessT2_mm > 0.0,
                F_Ed_kN = input.F_Ed_kN / 2.0,
                F_Ed_ser_kN = input.F_Ed_ser_kN / 2.0,
                ThicknessT2_mm = input.OuterLugThicknessT2_mm,
                PlateWidth_mm = input.PlateWidth_mm,
                EndDistanceE_mm = input.EndDistanceE_mm,
                HoleDiameter_mm = input.OuterLugHoleDiameter_mm > 0.0
                    ? input.OuterLugHoleDiameter_mm
                    : input.HoleDiameter_mm,
                PinDiameter_mm = input.OuterLugPinDiameter_mm > 0.0
                    ? input.OuterLugPinDiameter_mm
                    : input.ShackleDpin_mm,
                Fy_Nmm2 = input.OuterLugFy_Nmm2,
                Fu_Nmm2 = input.OuterLugFu_Nmm2,
                E_Nmm2 = input.E_Nmm2,
                GammaM0 = input.GammaM0,
                GammaM6_ser = input.GammaM6_ser,
                IsReplaceablePin = input.IsReplaceablePin
            });

            return new PadeyeCheckResult
            {
                BasicResult = basicResult,
                EcGeometryResult = ecGeometryResult,
                DnvOutOfPlaneResult = dnvOutOfPlaneResult,
                BearingResult = bearingResult,
                OuterLugResult = outerLugResult
            };
        }
  

        private static bool CheckCheekPlateBearingGeometry(
            PadeyeCheckInput input,
            out double requiredE_MethodA_mm,
            out double requiredE_MethodB_mm,
            out double requiredE_mm)
        {
            requiredE_MethodA_mm = 0.0;
            requiredE_MethodB_mm = 0.0;
            requiredE_mm = 0.0;

            if (!input.IncludeCheekPlatesInBearing)
                return false;

            if (input.CheekPlateThickness_mm <= 0.0)
                return false;

            if (input.Rch_mm <= 0.0)
                return false;

            if (input.F_Ed_kN <= 0.0 ||
                input.GammaM0 <= 0.0 ||
                input.Fy_Nmm2 <= 0.0 ||
                input.HoleDiameter_mm <= 0.0)
                return false;

            double tTotal_mm =
                input.PlateThickness_mm +
                2.0 * input.CheekPlateThickness_mm;

            if (tTotal_mm <= 0.0)
                return false;

            double fEd_N =
                input.F_Ed_kN * 1000.0;

            double forceTerm_mm =
                fEd_N * input.GammaM0 /
                (2.0 * tTotal_mm * input.Fy_Nmm2);

            requiredE_MethodA_mm =
                forceTerm_mm +
                2.0 * input.HoleDiameter_mm / 3.0 +
                input.HoleDiameter_mm / 2.0;

            requiredE_MethodB_mm =
                1.6 * input.HoleDiameter_mm;

            requiredE_mm =
                System.Math.Min(
                    requiredE_MethodA_mm,
                    requiredE_MethodB_mm);

            return
                input.Rch_mm >= requiredE_MethodA_mm ||
                input.Rch_mm >= requiredE_MethodB_mm;
        }
    }
}
