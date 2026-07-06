using System;

namespace LascheApp.Padeye
{
    public static class PadeyeBearingChecker
    {
        public static PadeyeBearingResult Check(PadeyeBearingInput input)
        {
            PadeyeBearingResult result = new PadeyeBearingResult
            {
                Input = input
            };

            if (input.F_Ed_kN <= 0)
                result.Errors.Add("F_Ed must be greater than 0.");

            if (input.F_Ed_ser_kN <= 0)
                result.Errors.Add("F_Ed,ser must be greater than 0.");

            if (input.PlateThickness_mm <= 0)
                result.Errors.Add("Plate thickness t must be greater than 0.");

            if (input.HoleDiameter_mm <= 0)
                result.Errors.Add("Hole diameter d0 must be greater than 0.");

            if (input.PinDiameter_mm <= 0)
                result.Errors.Add("Pin diameter d must be greater than 0.");

            if (input.HoleDiameter_mm < input.PinDiameter_mm)
                result.Errors.Add("Hole diameter d0 must be greater than or equal to pin diameter d.");

            if (input.Fy_Nmm2 <= 0)
                result.Errors.Add("fy must be greater than 0.");

            if (input.E_Nmm2 <= 0)
                result.Errors.Add("E must be greater than 0.");

            if (input.GammaM0 <= 0)
                result.Errors.Add("gammaM0 must be greater than 0.");

            if (input.GammaM6_ser <= 0)
                result.Errors.Add("gammaM6,ser must be greater than 0.");

            if (result.HasErrors)
                return result;

            double fEd_N =
                input.F_Ed_kN * 1000.0;

            double fEdSer_N =
                input.F_Ed_ser_kN * 1000.0;

            double fbRd_N =
                1.5 *
                input.PlateThickness_mm *
                input.PinDiameter_mm *
                input.Fy_Nmm2 /
                input.GammaM0;

            double fbRdSer_N =
                0.6 *
                input.PlateThickness_mm *
                input.PinDiameter_mm *
                input.Fy_Nmm2 /
                input.GammaM6_ser;

            double holePinDifference_mm =
                input.HoleDiameter_mm - input.PinDiameter_mm;

            double sigmaHEd_Nmm2 =
                0.591 *
                Math.Sqrt(
                    input.E_Nmm2 *
                    fEdSer_N *
                    holePinDifference_mm /
                    (
                        input.PinDiameter_mm *
                        input.PinDiameter_mm *
                        input.PlateThickness_mm
                    ));

            double fhRd_Nmm2 =
                2.5 *
                input.Fy_Nmm2 /
                input.GammaM6_ser;

            result.FbRd_kN = fbRd_N / 1000.0;
            result.FbRdSer_kN = fbRdSer_N / 1000.0;

            result.SigmaHEd_Nmm2 = sigmaHEd_Nmm2;
            result.FhRd_Nmm2 = fhRd_Nmm2;

            result.BearingDesignOk =
                fEd_N <= fbRd_N;

            result.BearingServiceOk =
                fEdSer_N <= fbRdSer_N;

            result.HolePinStressOk =
                sigmaHEd_Nmm2 <= fhRd_Nmm2;

            result.BearingDesignUtilization =
                fEd_N / fbRd_N;

            result.BearingServiceUtilization =
                fEdSer_N / fbRdSer_N;

            result.HolePinStressUtilization =
                sigmaHEd_Nmm2 / fhRd_Nmm2;

            return result;
        }
    }
}