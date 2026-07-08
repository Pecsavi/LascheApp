using System.Collections.Generic;
using System;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PadeyeBasicCheckInput
    {
        public LugType LugType { get; set; } = LugType.TransportLug;
        public double F_Ed_kN { get; set; }
        public double F_Ed_ser_kN { get; set; }
        public double PlateThickness_mm { get; set; }
        public double HoleDiameter_mm { get; set; }

        public double ShackleWLL_kN { get; set; }
        public double ShackleDpin_mm { get; set; }
        public double ShackleB1_mm { get; set; }
        public double ShackleH_DNV_mm { get; set; }
        public double PinClearance_mm { get; set; } = 2.0;
        public double PlateWidth_mm { get; set; }
        public double MaterialFy_Nmm2 { get; set; }
        public double GammaM0 { get; set; } = 1.0;
    }

    public class PadeyeBasicCheckResult
    {
        public PadeyeBasicCheckInput Input { get; set; } = new();

        public double RequiredHoleDiameter_mm { get; set; }
        public double RequiredThickness_mm { get; set; }

        public bool WllOk { get; set; }
        public bool HoleDiameterOk { get; set; }
        public bool ThicknessOk { get; set; }

        public double GrossArea_mm2 { get; set; }
        public double SigmaGrossEd_Nmm2 { get; set; }
        public bool GrossSectionTensionOk { get; set; }

        public double NetArea_mm2 { get; set; }
        public double SigmaEd_Nmm2 { get; set; }
        public double SigmaRd_Nmm2 { get; set; }
        public bool NetSectionTensionOk { get; set; }
     

        public List<string> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;
        public double WllUtilization { get; set; }
        public double HoleDiameterUtilization { get; set; }
        public double ThicknessUtilization { get; set; }

        public double GrossSectionTensionUtilization { get; set; }
        public double NetSectionTensionUtilization { get; set; }
       

        public double MaxUtilization
        {
            get
            {
                List<CheckItem> utilizationItems = CheckItems
                    .Where(i => i.ShowUtilization)
                    .ToList();

                return utilizationItems.Count == 0
                    ? 0.0
                    : utilizationItems.Max(i => i.Utilization);
            }
        }

        public bool ShackleChecksRequired => Input.LugType == LugType.TransportLug;

        public bool IsOk =>
            !HasErrors &&
            (!ShackleChecksRequired || WllOk) &&
            (!ShackleChecksRequired || HoleDiameterOk) &&
            (!ShackleChecksRequired || ThicknessOk) &&
            GrossSectionTensionOk &&
            NetSectionTensionOk;

        public string GoverningCheckName =>
         CheckItems
             .Where(i => i.ShowUtilization)
             .OrderByDescending(i => i.Utilization)
             .FirstOrDefault()?.Name ?? "";

        public List<CheckItem> CheckItems
        {
            get
            {
                List<CheckItem> items = new();

                if (Input.LugType == LugType.TransportLug)
                {
                    items.Add(new CheckItem
                    {
                        Name = "Shackle WLL",
                        Utilization = WllUtilization,
                        IsOk = WllOk
                    });

                    items.Add(new CheckItem
                    {
                        Name = "Hole diameter clearance",
                        Utilization = HoleDiameterUtilization,
                        IsOk = HoleDiameterOk,
                        ShowUtilization = false
                    });

                    items.Add(new CheckItem
                    {
                        Name = "Shackle B1 thickness recommendation",
                        Utilization = ThicknessUtilization,
                        IsOk = ThicknessOk,
                        ShowUtilization = false
                    });
                }

                items.Add(new CheckItem
                {
                    Name = "Gross section tension",
                    Utilization = GrossSectionTensionUtilization,
                    IsOk = GrossSectionTensionOk
                });

                items.Add(new CheckItem
                {
                    Name = "Net section tension",
                    Utilization = NetSectionTensionUtilization,
                    IsOk = NetSectionTensionOk
                });

                return items;
            }
        }
    }
}