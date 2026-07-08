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

        public double PlateThickness_mm { get; set; }
        public double HoleDiameter_mm { get; set; }
        public double EdgeDistanceA_mm { get; set; }
        public double SideDistanceC_mm { get; set; }
    }

    public class PadeyeEcGeometryResult
    {
        public PadeyeEcGeometryInput Input { get; set; } = new();

        public double RequiredThickness_MoglichkeitB_mm { get; set; }
        public double MaxHoleDiameter_MoglichkeitB_mm { get; set; }

        public bool ThicknessMoglichkeitB_Ok { get; set; }
        public bool HoleDiameterMoglichkeitB_Ok { get; set; }
        public double RequiredEdgeDistanceA_mm { get; set; }
        public double RequiredSideDistanceC_mm { get; set; }

        public bool EdgeDistanceA_Ok { get; set; }
        public bool SideDistanceC_Ok { get; set; }

        public double EdgeDistanceA_Utilization { get; set; }
        public double SideDistanceC_Utilization { get; set; }

        public double ThicknessMoglichkeitB_Utilization { get; set; }
        public double HoleDiameterMoglichkeitB_Utilization { get; set; }
        public List<string> Errors { get; set; } = new();

        public bool HasErrors => Errors.Count > 0;

        public double MoglichkeitA_MaxUtilization =>
            MoglichkeitA_CheckItems.Max(i => i.Utilization);

        public double MoglichkeitB_MaxUtilization =>
            MoglichkeitB_CheckItems.Max(i => i.Utilization);

        public double MaxUtilization =>
            Math.Min(
                MoglichkeitA_MaxUtilization,
                MoglichkeitB_MaxUtilization);

        public bool MoglichkeitA_Ok =>
            EdgeDistanceA_Ok &&
            SideDistanceC_Ok;

        public bool MoglichkeitB_Ok =>
            ThicknessMoglichkeitB_Ok &&
            HoleDiameterMoglichkeitB_Ok;

        public bool IsOk =>
            !HasErrors &&
            (
                MoglichkeitA_Ok ||
                MoglichkeitB_Ok
            );

        public string GoverningCheckName
        {
            get
            {
                List<CheckItem> governingList =
                    MoglichkeitA_MaxUtilization <= MoglichkeitB_MaxUtilization
                        ? MoglichkeitA_CheckItems
                        : MoglichkeitB_CheckItems;

                return governingList
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

        public List<CheckItem> MoglichkeitA_CheckItems
        {
            get
            {
                return new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = "EC geometry Möglichkeit A - edge distance a",
                        Utilization = EdgeDistanceA_Utilization,
                        IsOk = EdgeDistanceA_Ok
                    },
                    new CheckItem
                    {
                        Name = "EC geometry Möglichkeit A - side distance c",
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
                        Name = "EC geometry Möglichkeit B - thickness t",
                        Utilization = ThicknessMoglichkeitB_Utilization,
                        IsOk = ThicknessMoglichkeitB_Ok
                    },
                    new CheckItem
                    {
                        Name = "EC geometry Möglichkeit B - hole diameter d0",
                        Utilization = HoleDiameterMoglichkeitB_Utilization,
                        IsOk = HoleDiameterMoglichkeitB_Ok
                    }
                };
            }
        }
    }

}