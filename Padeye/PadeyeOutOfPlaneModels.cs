using System.Collections.Generic;

namespace LascheApp.Padeye
{
    public class PadeyeOutOfPlaneInput
    {
        public double F_Ed_kN { get; set; }
        public double H_DNV_mm { get; set; }

        public double PlateWidth_mm { get; set; }
        public double PlateThickness_mm { get; set; }

        public double Fy_Nmm2 { get; set; }
        public double GammaM0 { get; set; } = 1.0;
    }

    public class PadeyeOutOfPlaneResult
    {
        public PadeyeOutOfPlaneInput Input { get; set; } = new();

        public List<string> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;

        public double M_Ed_Nmm { get; set; }
        public double SectionModulus_mm3 { get; set; }

        public double SigmaBendingEd_Nmm2 { get; set; }
        public double SigmaRd_Nmm2 { get; set; }

        public bool BendingOk { get; set; }

        public double BendingUtilization { get; set; }

        public bool IsOk => !HasErrors && BendingOk;

        public double MaxUtilization => BendingUtilization;

        public string GoverningCheckName => "Out-of-plane bending";
    }
}