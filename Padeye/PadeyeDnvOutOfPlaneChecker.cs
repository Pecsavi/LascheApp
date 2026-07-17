using System;

namespace LascheApp.Padeye
{
    public static class PadeyeDnvOutOfPlaneChecker
    {
        public static PadeyeDnvOutOfPlaneResult Check(PadeyeDnvOutOfPlaneInput input)
        {
            PadeyeDnvOutOfPlaneResult result = new PadeyeDnvOutOfPlaneResult
            {
                Input = input
            };

            result.OutOfPlaneChecksActive = Math.Abs(input.Alpha_deg) > 1e-9;
            result.CheekPlateWeldCheckActive = input.CheekPlateThickness_mm > 0.0;

            if (!result.IsActive)
                return result;

            if (input.F_Ed_kN <= 0)
                result.Errors.Add("F_Ed must be greater than 0.");

            if (input.Alpha_deg < 0 || input.Alpha_deg >= 90.0)
                result.Errors.Add("Out-of-plane angle alpha must be >= 0 and < 90 degrees.");

            if (result.OutOfPlaneChecksActive && input.H_DNV_mm <= 0)
                result.Errors.Add("H_DNV must be greater than 0.");

            if (input.Dpin_mm <= 0)
                result.Errors.Add("Dpin must be greater than 0.");

            if (input.HoleDiameter_mm <= 0)
                result.Errors.Add("Hole diameter d0 must be greater than 0.");

            if (input.MainPlateThickness_mm <= 0)
                result.Errors.Add("Main plate thickness tpl must be greater than 0.");

            if (input.CheekPlateThickness_mm < 0)
                result.Errors.Add("Cheek plate thickness tch must not be negative.");

            if (input.TotalThickness_mm <= 0)
                result.Errors.Add("Total thickness t must be greater than 0.");

            if (input.Fy_Nmm2 <= 0)
                result.Errors.Add("fy must be greater than 0.");

            if (input.E_Nmm2 <= 0)
                result.Errors.Add("E must be greater than 0.");

            if (input.Beta <= 0)
                result.Errors.Add("beta must be greater than 0.");

            if (input.GammaM <= 0)
                result.Errors.Add("gammaM must be greater than 0.");

            if (result.OutOfPlaneChecksActive)
            {
                if (input.EndDistanceE_mm <= 0)
                    result.Errors.Add("e must be greater than 0 for DNV tear-out check.");

                if (2.0 * input.EndDistanceE_mm <= input.HoleDiameter_mm)
                    result.Errors.Add("2 * e must be greater than d0 for DNV tear-out check.");
            }

            if (result.CheekPlateWeldCheckActive)
            {
                if (input.Rch_mm <= 0)
                    result.Errors.Add("Rch must be greater than 0 for cheek plate weld check.");

                if (input.WeldA_mm <= 0)
                    result.Errors.Add("Weld throat a must be greater than 0 for cheek plate weld check.");

                if (input.Fu_Nmm2 <= 0)
                    result.Errors.Add("fu must be greater than 0 for cheek plate weld check.");

                if (input.BetaW <= 0)
                    result.Errors.Add("betaW must be greater than 0 for cheek plate weld check.");

                if (input.GammaM2 <= 0)
                    result.Errors.Add("gammaM2 must be greater than 0 for cheek plate weld check.");
            }

            if (result.HasErrors)
                return result;

            double alphaRad = input.Alpha_deg * Math.PI / 180.0;
            double fEd_N = input.F_Ed_kN * 1000.0;
            double fd_N = fEd_N * Math.Cos(alphaRad);
            double fdl_N = fEd_N * Math.Sin(alphaRad);
            double t = input.TotalThickness_mm;

            double delta = 4.0 * Math.Tan(alphaRad) * input.H_DNV_mm / t + 1.0;
            double betaEffective = delta >= 1.3
                ? input.Beta * (delta - 0.3)
                : input.Beta;

            result.Alpha_rad = alphaRad;
            result.Fd_kN = fd_N / 1000.0;
            result.Fdl_kN = fdl_N / 1000.0;
            result.Me_kNm = fdl_N * input.H_DNV_mm / 1_000_000.0;
            result.Delta = delta;
            result.BetaEffective = betaEffective;
            result.SigmaRd_Nmm2 = input.Fy_Nmm2 / input.GammaM;

            if (input.EndDistanceE_mm > 0)
            {
                result.Rpad_mm =
                    (input.EndDistanceE_mm * input.MainPlateThickness_mm +
                     2.0 * input.Rch_mm * input.CheekPlateThickness_mm) /
                    t;
            }

            if (result.OutOfPlaneChecksActive)
            {
                result.DpinToHoleRatio = input.Dpin_mm / input.HoleDiameter_mm;

                double outOfPlaneRatio =
                    Math.Abs(Math.Sin(alphaRad));

                result.PinDiameterRecommendationActive =
                    outOfPlaneRatio > 0.10;

                result.PinDiameterRecommendationOk =
                    !result.PinDiameterRecommendationActive ||
                    result.DpinToHoleRatio >= 0.94;

                result.PinDiameterRecommendationUtilization =
                    result.DpinToHoleRatio > 0.0
                        ? 0.94 / result.DpinToHoleRatio
                        : 0.0;

                result.BearingFormulaWithClearance = result.DpinToHoleRatio < 0.96;
                

                if (result.BearingFormulaWithClearance)
                {
                    result.SigmaEd1_Nmm2 =
                        0.18 *
                        Math.Sqrt(
                            fd_N *
                            (1.0 / input.Dpin_mm - 1.0 / input.HoleDiameter_mm) *
                            input.E_Nmm2 *
                            betaEffective /
                            t);
                }
                else
                {
                    result.SigmaEd1_Nmm2 =
                        0.036 *
                        Math.Sqrt(
                            fd_N *
                            input.E_Nmm2 *
                            betaEffective /
                            (input.HoleDiameter_mm * t));
                }

                result.DnvBearingOk = result.SigmaEd1_Nmm2 <= result.SigmaRd_Nmm2;
                result.DnvBearingUtilization = result.SigmaEd1_Nmm2 / result.SigmaRd_Nmm2;

                result.SigmaEd2_Nmm2 =
                    1.7 * fd_N /
                    ((2.0 * input.EndDistanceE_mm - input.HoleDiameter_mm) * t);

                result.TearOutOk = result.SigmaEd2_Nmm2 <= result.SigmaRd_Nmm2;
                result.TearOutUtilization = result.SigmaEd2_Nmm2 / result.SigmaRd_Nmm2;
            }

            if (result.CheekPlateWeldCheckActive)
            {
                result.Dch_mm = 2.0 * input.Rch_mm;

                double deltaFactor = Math.Abs(input.Alpha_deg) > 1e-9 ? delta : 1.0;

                result.SigmaEd3_Nmm2 =
                    fd_N * input.CheekPlateThickness_mm /
                    (1.5 * t * result.Dch_mm * input.WeldA_mm) *
                    deltaFactor;

                result.Fvwd_Nmm2 =
                    input.Fu_Nmm2 /
                    (Math.Sqrt(3.0) * input.BetaW * input.GammaM2);

                result.CheekPlateWeldOk = result.SigmaEd3_Nmm2 <= result.Fvwd_Nmm2;
                result.CheekPlateWeldUtilization = result.SigmaEd3_Nmm2 / result.Fvwd_Nmm2;
            }

            return result;
        }
    }
}
