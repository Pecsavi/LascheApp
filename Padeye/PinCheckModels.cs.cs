using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PinCheckInput
    {
        public double F_Ed_kN { get; set; }
        public double F_Ed_ser_kN { get; set; }

        public double M_Ed_kNmm { get; set; }
        public double M_Ed_ser_kNmm { get; set; }

        public double PinDiameter_mm { get; set; }

        public double PinFy_Nmm2 { get; set; } // fyp
        public double PinFu_Nmm2 { get; set; } // fup

        public double GammaM0 { get; set; } = 1.0;
        public double GammaM2 { get; set; } = 1.25;
        public double GammaM6_ser { get; set; } = 1.0;

        public bool IsReplaceablePin { get; set; } = true;
    }

    public class PinCheckResult
    {
        public PinCheckInput Input { get; set; } = new PinCheckInput();

        public List<string> Errors { get; set; } = new List<string>();
        public bool HasErrors => Errors.Count > 0;

        public double Area_mm2 { get; set; }
        public double SectionModulus_mm3 { get; set; }

        public double FvEd_kN { get; set; }
        public double FvRd_kN { get; set; }

        public double MEd_kNmm { get; set; }
        public double MRd_kNmm { get; set; }

        public double MEdSer_kNmm { get; set; }
        public double MRdSer_kNmm { get; set; }

        public bool ShearOk { get; set; }
        public bool BendingOk { get; set; }
        public bool ServiceBendingOk { get; set; }
        public bool CombinedOk { get; set; }

        public double ShearUtilization { get; set; }
        public double BendingUtilization { get; set; }
        public double ServiceBendingUtilization { get; set; }
        public double CombinedUtilization { get; set; }

        public bool IsOk =>
            !HasErrors &&
            ShearOk &&
            BendingOk &&
            CombinedOk &&
            (!Input.IsReplaceablePin || ServiceBendingOk);

        public List<CheckItem> CheckItems
        {
            get
            {
                if (HasErrors)
                    return new List<CheckItem>();

                List<CheckItem> items = new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = "Pin shear",
                        Utilization = ShearUtilization,
                        IsOk = ShearOk
                    },
                    new CheckItem
                    {
                        Name = "Pin bending",
                        Utilization = BendingUtilization,
                        IsOk = BendingOk
                    },
                    new CheckItem
                    {
                        Name = "Pin shear + bending interaction",
                        Utilization = CombinedUtilization,
                        IsOk = CombinedOk
                    }
                };

                if (Input.IsReplaceablePin)
                {
                    items.Add(new CheckItem
                    {
                        Name = "Replaceable pin service bending",
                        Utilization = ServiceBendingUtilization,
                        IsOk = ServiceBendingOk
                    });
                }

                return items;
            }
        }

        public double MaxUtilization =>
            CheckItems.Count == 0 ? 0.0 : CheckItems.Max(i => i.Utilization);

        public string GoverningCheckName =>
            CheckItems.Count == 0
                ? "-"
                : CheckItems.OrderByDescending(i => i.Utilization).First().Name;
    }
}