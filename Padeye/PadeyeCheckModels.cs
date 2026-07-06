using System;
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

        public double PlateThickness_mm { get; set; }
        public double PlateWidth_mm { get; set; }
        public double HoleDiameter_mm { get; set; }

        public double EdgeDistanceA_mm { get; set; }
        public double SideDistanceC_mm { get; set; }

        public double Fy_Nmm2 { get; set; }
        public double GammaM0 { get; set; } = 1.0;

        public double ShackleWLL_kN { get; set; }
        public double ShackleDpin_mm { get; set; }
        public double ShackleB1_mm { get; set; }
        public double ShackleH_DNV_mm { get; set; }

        public double PinClearance_mm { get; set; } = 2.0;

        public bool IsReplaceablePin { get; set; } = true;


    }
    public class PadeyeCheckResult
    {
        public PadeyeBasicCheckResult BasicResult { get; set; } = new();
        public PadeyeEcGeometryResult EcGeometryResult { get; set; } = new();

        public PadeyeOutOfPlaneResult OutOfPlaneResult { get; set; } = new();

        public PadeyeBearingResult BearingResult { get; set; } = new();

        public LugType LugType => BasicResult.Input.LugType;

        public bool OutOfPlaneCheckRequired => LugType == LugType.TransportLug;

        public bool IsOk =>
             BasicResult.IsOk &&
             EcGeometryResult.IsOk &&
             (!OutOfPlaneCheckRequired || OutOfPlaneResult.IsOk) &&
             BearingResult.IsOk;

        public List<CheckItem> GoverningCheckItems
        {
            get
            {
                List<CheckItem> items = new();

                if (!BasicResult.HasErrors)
                    items.AddRange(BasicResult.CheckItems);

                if (!EcGeometryResult.HasErrors)
                {
                    if (EcGeometryResult.MoglichkeitA_MaxUtilization <= EcGeometryResult.MoglichkeitB_MaxUtilization)
                        items.AddRange(EcGeometryResult.MoglichkeitA_CheckItems);
                    else
                        items.AddRange(EcGeometryResult.MoglichkeitB_CheckItems);
                }

                if (OutOfPlaneCheckRequired && !OutOfPlaneResult.HasErrors)
                {
                    items.Add(new CheckItem
                    {
                        Name = OutOfPlaneResult.GoverningCheckName,
                        Utilization = OutOfPlaneResult.MaxUtilization,
                        IsOk = OutOfPlaneResult.IsOk
                    });
                }

                if (!BearingResult.HasErrors)
                    items.AddRange(BearingResult.CheckItems);

                return items;
            }
        }

        public double MaxUtilization =>
            GoverningCheckItems.Count == 0
                ? 0.0
                : GoverningCheckItems.Max(i => i.Utilization);

        public string GoverningCheckName =>
            GoverningCheckItems
                .OrderByDescending(i => i.Utilization)
                .FirstOrDefault()?.Name ?? "";
    }
}