using System;

namespace LascheApp.Padeye
{
    public static class PadeyeOutOfPlaneChecker
    {
        public static PadeyeOutOfPlaneResult Check(PadeyeOutOfPlaneInput input)
        {
            PadeyeOutOfPlaneResult result = new PadeyeOutOfPlaneResult
            {
                Input = input
            };

            if (input.F_Ed_kN <= 0)
                result.Errors.Add("F_Ed must be greater than 0.");

            if (input.H_DNV_mm <= 0)
                result.Errors.Add("H_DNV must be greater than 0.");

            if (input.OutOfPlaneAngle_deg < 0 || input.OutOfPlaneAngle_deg > 90)
                result.Errors.Add("Out-of-plane angle must be between 0° and 90°.");

            if (input.OutOfPlaneLoadFactorInput.HasValue &&
                (input.OutOfPlaneLoadFactorInput.Value < 0 || input.OutOfPlaneLoadFactorInput.Value > 1))
                result.Errors.Add("Out-of-plane load factor must be between 0.0 and 1.0.");

            if (input.PlateWidth_mm <= 0)
                result.Errors.Add("Plate width b must be greater than 0.");

            if (input.PlateThickness_mm <= 0)
                result.Errors.Add("Plate thickness t must be greater than 0.");

            if (input.Fy_Nmm2 <= 0)
                result.Errors.Add("fy must be greater than 0.");

            if (input.GammaM0 <= 0)
                result.Errors.Add("gammaM0 must be greater than 0.");

            if (result.HasErrors)
                return result;

            double outOfPlaneLoadFactor = input.OutOfPlaneLoadFactorInput.HasValue
                ? input.OutOfPlaneLoadFactorInput.Value
                : Math.Sin(input.OutOfPlaneAngle_deg * Math.PI / 180.0);

            double fOutOfPlaneEd_N =
                input.F_Ed_kN * 1000.0 * outOfPlaneLoadFactor;

            double mEd_Nmm =
                fOutOfPlaneEd_N * input.H_DNV_mm;

            double sectionModulus_mm3 =
                input.PlateWidth_mm *
                input.PlateThickness_mm *
                input.PlateThickness_mm /
                6.0;

            double sigmaBendingEd_Nmm2 =
                mEd_Nmm / sectionModulus_mm3;

            double sigmaRd_Nmm2 =
                input.Fy_Nmm2 / input.GammaM0;

            result.OutOfPlaneLoadFactor = outOfPlaneLoadFactor;
            result.F_OutOfPlane_Ed_kN = fOutOfPlaneEd_N / 1000.0;

            result.M_Ed_Nmm = mEd_Nmm;
            result.SectionModulus_mm3 = sectionModulus_mm3;

            result.SigmaBendingEd_Nmm2 = sigmaBendingEd_Nmm2;
            result.SigmaRd_Nmm2 = sigmaRd_Nmm2;

            result.BendingOk =
                sigmaBendingEd_Nmm2 <= sigmaRd_Nmm2;

            result.BendingUtilization =
                sigmaBendingEd_Nmm2 / sigmaRd_Nmm2;

            return result;
        }
    }
}
