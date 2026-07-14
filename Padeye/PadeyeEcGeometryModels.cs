using System;
using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PadeyeEcGeometryInput
    {
        public double F_Ed_kN { get; set; }
        public double GammaM0 { get; set; } = 1.0;
        public double Fy_Nmm2 { get; set; }

        // Main lug plate thickness: tpl
        public double PlateThickness_mm { get; set; }

        // Cheek plate thickness: tch
        public double CheekPlateThickness_mm { get; set; }

        public bool IncludeCheekPlatesInBearing { get; set; }

        // Thickness used in EC geometry formulas.
        // Report notation: t.
        public double EffectiveThickness_mm =>
            IncludeCheekPlatesInBearing
                ? PlateThickness_mm + 2.0 * CheekPlateThickness_mm
                : PlateThickness_mm;

        public double HoleDiameter_mm { get; set; }
        public double EdgeDistanceA_mm { get; set; }
        public double SideDistanceC_mm { get; set; }
    }

    public class PadeyeEcGeometryResult
    {
        public PadeyeEcGeometryInput Input { get; set; } = new();

        public double RequiredEdgeDistanceA_mm { get; set; }
        public double RequiredSideDistanceC_mm { get; set; }

        public bool EdgeDistanceA_Ok { get; set; }
        public bool SideDistanceC_Ok { get; set; }

        public double EdgeDistanceA_Utilization { get; set; }
        public double SideDistanceC_Utilization { get; set; }

        public double RequiredThickness_MoglichkeitB_mm { get; set; }
        public double ForceResistance_MoglichkeitB_kN { get; set; }
        public double ForceMoglichkeitB_Utilization { get; set; }

        public double MaxHoleDiameter_MoglichkeitB_mm { get; set; }
        public double RequiredEdgeDistance_MoglichkeitB_mm { get; set; }
        public double RequiredPlateWidth_MoglichkeitB_mm { get; set; }

        public bool ThicknessMoglichkeitB_Ok { get; set; }
        public bool HoleDiameterMoglichkeitB_Ok { get; set; }
        public bool EdgeDistanceMoglichkeitB_Ok { get; set; }
        public bool PlateWidthMoglichkeitB_Ok { get; set; }

        public double ThicknessMoglichkeitB_Utilization
        {
            get => ForceMoglichkeitB_Utilization;
            set => ForceMoglichkeitB_Utilization = value;
        }

        public double HoleDiameterMoglichkeitB_Utilization { get; set; }
        public double EdgeDistanceMoglichkeitB_Utilization { get; set; }
        public double PlateWidthMoglichkeitB_Utilization { get; set; }

      
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;

        public bool MoglichkeitA_Ok =>
            EdgeDistanceA_Ok &&
            SideDistanceC_Ok;

        public bool MoglichkeitB_Ok =>
             ThicknessMoglichkeitB_Ok &&
             HoleDiameterMoglichkeitB_Ok &&
             EdgeDistanceMoglichkeitB_Ok &&
             PlateWidthMoglichkeitB_Ok;

        public bool IsOk =>
            !HasErrors &&
            (
                MoglichkeitA_Ok ||
                MoglichkeitB_Ok
            );

        public List<CheckItem> MoglichkeitA_CheckItems
        {
            get
            {
                return new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = "EC geometry Method A - edge distance a",
                        Utilization = EdgeDistanceA_Utilization,
                        IsOk = EdgeDistanceA_Ok
                    },
                    new CheckItem
                    {
                        Name = "EC geometry Method A - side distance c",
                        Utilization = SideDistanceC_Utilization,
                        IsOk = SideDistanceC_Ok
                    }
                };
            }
        }

        public List<CheckItem> MoglichkeitB_CheckItems
        {
            get
            {
                return new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = "EC geometry Method B - minimum thickness",
                        Utilization = ThicknessMoglichkeitB_Utilization,
                        IsOk = ThicknessMoglichkeitB_Ok
                    },
                    new CheckItem
                    {
                        Name = "EC geometry Method B - maximum hole diameter",
                        Utilization = HoleDiameterMoglichkeitB_Utilization,
                        IsOk = HoleDiameterMoglichkeitB_Ok
                    },
                    new CheckItem
                    {
                        Name = "EC geometry Method B - edge distance e",
                        Utilization = EdgeDistanceMoglichkeitB_Utilization,
                        IsOk = EdgeDistanceMoglichkeitB_Ok
                    },
                    new CheckItem
                    {
                        Name = "EC geometry Method B - plate width b",
                        Utilization = PlateWidthMoglichkeitB_Utilization,
                        IsOk = PlateWidthMoglichkeitB_Ok
                    }
                };
            }
        }

        public double MoglichkeitA_MaxUtilization =>
            MoglichkeitA_CheckItems
                .Where(i => i.ShowUtilization)
                .Max(i => i.Utilization);

        public double MoglichkeitB_MaxUtilization =>
            MoglichkeitB_CheckItems
                .Where(i => i.ShowUtilization)
                .Max(i => i.Utilization);

        public double MaxUtilization =>
            Math.Min(
                MoglichkeitA_MaxUtilization,
                MoglichkeitB_MaxUtilization);

        public string GoverningCheckName
        {
            get
            {
                List<CheckItem> governingList =
                    MoglichkeitA_MaxUtilization <= MoglichkeitB_MaxUtilization
                        ? MoglichkeitA_CheckItems
                        : MoglichkeitB_CheckItems;

                return governingList
                    .Where(i => i.ShowUtilization)
                    .OrderByDescending(i => i.Utilization)
                    .FirstOrDefault()?.Name ?? "";
            }
        }

        public string SummaryCheckName
        {
            get
            {
                if (MoglichkeitA_Ok && MoglichkeitB_Ok)
                    return "EC geometry check - Method A and B OK";

                if (MoglichkeitA_Ok)
                    return "EC geometry check - Method A OK";

                if (MoglichkeitB_Ok)
                    return "EC geometry check - Method B OK";

                return "EC geometry check - Method A and B NOT OK";
            }
        }

        public List<CheckItem> SummaryCheckItems
        {
            get
            {
                return new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = SummaryCheckName,
                        Utilization = MaxUtilization,
                        IsOk = IsOk,
                        ShowUtilization = false
                    }
                };
            }
        }
    }
}
