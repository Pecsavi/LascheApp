using System;

namespace LascheApp.Padeye
{
    public static class PinChecker
    {
        public static PinCheckResult Check(PinCheckInput input)
        {
            PinCheckResult result = new PinCheckResult
            {
                Input = input
            };

            if (input.F_Ed_kN <= 0)
                result.Errors.Add("F_Ed must be greater than zero.");

            if (input.F_Ed_ser_kN <= 0)
                result.Errors.Add("F_Ed,ser must be greater than zero.");

            if (input.M_Ed_kNmm < 0)
                result.Errors.Add("M_Ed must not be negative.");

            if (input.M_Ed_ser_kNmm < 0)
                result.Errors.Add("M_Ed,ser must not be negative.");

            if (input.PinDiameter_mm <= 0)
                result.Errors.Add("Pin diameter d must be greater than zero.");

            if (input.PinFy_Nmm2 <= 0)
                result.Errors.Add("Pin fy,p must be greater than zero.");

            if (input.PinFu_Nmm2 <= 0)
                result.Errors.Add("Pin fu,p must be greater than zero.");

            if (input.GammaM0 <= 0)
                result.Errors.Add("gammaM0 must be greater than zero.");

            if (input.GammaM2 <= 0)
                result.Errors.Add("gammaM2 must be greater than zero.");

            if (input.GammaM6_ser <= 0)
                result.Errors.Add("gammaM6,ser must be greater than zero.");

            if (result.HasErrors)
                return result;

            double d = input.PinDiameter_mm;

            double area = Math.PI * d * d / 4.0;
            double wel = Math.PI * Math.Pow(d, 3.0) / 32.0;

            double fEd_N = input.F_Ed_kN * 1000.0;

            // EC3 Table 3.10: shear force per pin shear plane.
            // For double shear: Fv,Ed = F_Ed / 2.
            double fvEd_N = fEd_N / 2.0;

            double fvRd_N =
                0.6 * area * input.PinFu_Nmm2 / input.GammaM2;

            double mEd_Nmm =
                input.M_Ed_kNmm * 1000.0;

            double mEdSer_Nmm =
                input.M_Ed_ser_kNmm * 1000.0;

            double mRd_Nmm =
                1.5 * wel * input.PinFy_Nmm2 / input.GammaM0;

            double mRdSer_Nmm =
                0.8 * wel * input.PinFy_Nmm2 / input.GammaM6_ser;

            result.Area_mm2 = area;
            result.SectionModulus_mm3 = wel;

            result.FvEd_kN = fvEd_N / 1000.0;
            result.FvRd_kN = fvRd_N / 1000.0;

            result.MEd_kNmm = input.M_Ed_kNmm;
            result.MRd_kNmm = mRd_Nmm / 1000.0;

            result.MEdSer_kNmm = input.M_Ed_ser_kNmm;
            result.MRdSer_kNmm = mRdSer_Nmm / 1000.0;

            result.ShearUtilization = fvEd_N / fvRd_N;
            result.BendingUtilization = mEd_Nmm / mRd_Nmm;
            result.ServiceBendingUtilization = mEdSer_Nmm / mRdSer_Nmm;

            result.CombinedUtilization =
                result.BendingUtilization * result.BendingUtilization +
                result.ShearUtilization * result.ShearUtilization;

            result.ShearOk = result.ShearUtilization <= 1.0;
            result.BendingOk = result.BendingUtilization <= 1.0;
            result.ServiceBendingOk = result.ServiceBendingUtilization <= 1.0;
            result.CombinedOk = result.CombinedUtilization <= 1.0;

            return result;
        }
    }
}