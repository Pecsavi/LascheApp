using System.Collections.Generic;

namespace LascheApp.Padeye
{
    public class PadeyeBearingInput
    {
        public double F_Ed_kN { get; set; }
        public double F_Ed_ser_kN { get; set; }

        public double PlateThickness_mm { get; set; }
        public double HoleDiameter_mm { get; set; }
        public double PinDiameter_mm { get; set; }

        public double Fy_Nmm2 { get; set; }
        public double E_Nmm2 { get; set; }

        public double GammaM0 { get; set; } = 1.0;
        public double GammaM6_ser { get; set; } = 1.0;

        public bool IsReplaceablePin { get; set; } = true;
    }

    public class PadeyeBearingResult
    {
        public PadeyeBearingInput Input { get; set; } = new();

        public List<string> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;

        public double FbRd_kN { get; set; }
        public double FbRdSer_kN { get; set; }

        public double SigmaHEd_Nmm2 { get; set; }
        public double FhRd_Nmm2 { get; set; }

        public bool BearingDesignOk { get; set; }
        public bool BearingServiceOk { get; set; }
        public bool HolePinStressOk { get; set; }
        public bool PinHoleGeometryOk { get; set; }

        public double BearingDesignUtilization { get; set; }
        public double BearingServiceUtilization { get; set; }
        public double HolePinStressUtilization { get; set; }
        public bool IsOk =>
            !HasErrors &&
            PinHoleGeometryOk &&
            BearingDesignOk &&
            (
                !Input.IsReplaceablePin ||
                (
                    BearingServiceOk &&
                    HolePinStressOk
                )
            );
        public List<CheckItem> CheckItems
        {
            get
            {
                List<CheckItem> items = new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = PinHoleGeometryOk
                            ? "Pin-hole geometry OK: d < d0"
                            : "Pin-hole geometry NOT OK: pin diameter d must be smaller than hole diameter d0",
                        Utilization = 0.0,
                        IsOk = PinHoleGeometryOk,
                        ShowUtilization = false
                    },
                    new CheckItem
                    {
                        Name = "Pin-hole bearing design",
                        Utilization = BearingDesignUtilization,
                        IsOk = BearingDesignOk
                    }
                };

                if (Input.IsReplaceablePin)
                {
                    items.Add(new CheckItem
                    {
                        Name = "Replaceable pin service bearing",
                        Utilization = BearingServiceUtilization,
                        IsOk = BearingServiceOk
                    });

                    if (PinHoleGeometryOk)
                    {
                        items.Add(new CheckItem
                        {
                            Name = "Replaceable pin contact stress",
                            Utilization = HolePinStressUtilization,
                            IsOk = HolePinStressOk
                        });
                    }
                }

                return items;
            }
        }

        public double MaxUtilization
        {
            get
            {
                double max = 0.0;

                foreach (CheckItem item in CheckItems)
                {
                    if (item.Utilization > max)
                        max = item.Utilization;
                }

                return max;
            }
        }

        public string GoverningCheckName
        {
            get
            {
                CheckItem? governing = null;

                foreach (CheckItem item in CheckItems)
                {
                    if (governing == null || item.Utilization > governing.Utilization)
                        governing = item;
                }

                return governing?.Name ?? "";
            }
        }
    }
}
