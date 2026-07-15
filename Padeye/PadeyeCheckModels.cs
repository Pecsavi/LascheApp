using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PadeyeCheckInput
    {
        public LugType LugType { get; set; } = LugType.TransportLug;
        public double F_Ed_kN { get; set; }
        public double F_Ed_ser_kN { get; set; }

        public double E_Nmm2 { get; set; } = 210000.0;
        public double GammaM6_ser { get; set; } = 1.0;

        // Main lug plate thickness tpl.
        public double PlateThickness_mm { get; set; }
        public double PlateWidth_mm { get; set; }
        public double HoleDiameter_mm { get; set; }

        public double EdgeDistanceA_mm { get; set; }
        public double SideDistanceC_mm { get; set; }

        public double Fy_Nmm2 { get; set; }
        public double Fu_Nmm2 { get; set; }
        public double BetaW { get; set; } = 0.9;
        public double GammaM0 { get; set; } = 1.0;

        public double ShackleWLL_kN { get; set; }
        public double ShackleDpin_mm { get; set; }
        public double ShackleB1_mm { get; set; }
        public double ShackleH_DNV_mm { get; set; }

        public double PinClearance_mm { get; set; } = 3.0;
        public bool IsReplaceablePin { get; set; } = true;

        public double DnvOutOfPlaneAngle_deg { get; set; }
        public double DnvBeta { get; set; } = 0.7;
        public double DnvGammaM { get; set; } = 1.15;
        public double GammaM2 { get; set; } = 1.25;

        public double CheekPlateThickness_mm { get; set; }
        public double Rpl_mm { get; set; }
        public double Rch_mm { get; set; }
        public double CheekPlateWeldA_mm { get; set; }

        // False by default. True only if cheek plates are welded before final drilling/reaming.
        public bool IncludeCheekPlatesInBearing { get; set; } = false;

        public double BearingThickness_mm =>
            IncludeCheekPlatesInBearing
                ? PlateThickness_mm + 2.0 * CheekPlateThickness_mm
                : PlateThickness_mm;
    }

    public class PadeyeCheckResult
    {
        public PadeyeBasicCheckResult BasicResult { get; set; } = new();
        public PadeyeEcGeometryResult EcGeometryResult { get; set; } = new();
        public PadeyeOutOfPlaneResult OutOfPlaneResult { get; set; } = new();
        public PadeyeDnvOutOfPlaneResult DnvOutOfPlaneResult { get; set; } = new();
        public PadeyeBearingResult BearingResult { get; set; } = new();

        public LugType LugType => BasicResult.Input.LugType;

        public bool DnvOutOfPlaneCheckRequired =>
             (LugType == LugType.TransportLug && DnvOutOfPlaneResult.IsActive) ||
             (LugType == LugType.TensionLug && DnvOutOfPlaneResult.CheekPlateWeldCheckActive);

        public bool IsOk =>
             BasicResult.IsOk &&
             EcGeometryResult.IsOk &&
             (!DnvOutOfPlaneCheckRequired || DnvOutOfPlaneResult.IsOk) &&
             BearingResult.IsOk;

        public List<CheckItem> GoverningCheckItems
        {
            get
            {
                List<CheckItem> items = new();

                if (!BasicResult.HasErrors)
                    items.AddRange(BasicResult.CheckItems);

                if (!EcGeometryResult.HasErrors)
                    items.AddRange(EcGeometryResult.SummaryCheckItems);

                if (DnvOutOfPlaneCheckRequired && !DnvOutOfPlaneResult.HasErrors)
                    items.AddRange(DnvOutOfPlaneResult.CheckItems);

                if (!BearingResult.HasErrors)
                    items.AddRange(BearingResult.CheckItems);

                return items;
            }
        }

        public double MaxUtilization
        {
            get
            {
                List<CheckItem> utilizationItems = GoverningCheckItems
                    .Where(i => i.ShowUtilization)
                    .ToList();

                return utilizationItems.Count == 0
                    ? 0.0
                    : utilizationItems.Max(i => i.Utilization);
            }
        }

        public string GoverningCheckName =>
            GoverningCheckItems
                .Where(i => i.ShowUtilization)
                .OrderByDescending(i => i.Utilization)
                .FirstOrDefault()?.Name ?? "";
    }
}
