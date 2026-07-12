using System;
using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PadeyeDnvOutOfPlaneInput
    {
        public double F_Ed_kN { get; set; }
        public double Alpha_deg { get; set; }
        public double H_DNV_mm { get; set; }

        public double Dpin_mm { get; set; }
        public double HoleDiameter_mm { get; set; }

        public double MainPlateThickness_mm { get; set; }
        public double CheekPlateThickness_mm { get; set; }
        public double TotalThickness_mm => MainPlateThickness_mm + 2.0 * CheekPlateThickness_mm;

        public double Rpl_mm { get; set; }
        public double Rch_mm { get; set; }
        public double WeldA_mm { get; set; }

        public double Fy_Nmm2 { get; set; }
        public double Fu_Nmm2 { get; set; }
        public double E_Nmm2 { get; set; } = 210000.0;
        public double BetaW { get; set; } = 0.9;

        public double Beta { get; set; } = 0.7;
        public double GammaM { get; set; } = 1.15;
        public double GammaM2 { get; set; } = 1.25;
    }

    public class PadeyeDnvOutOfPlaneResult
    {
        public PadeyeDnvOutOfPlaneInput Input { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;

        public bool OutOfPlaneChecksActive { get; set; }
        public bool CheekPlateWeldCheckActive { get; set; }
        public bool IsActive => OutOfPlaneChecksActive || CheekPlateWeldCheckActive;

        public double Alpha_rad { get; set; }
        public double Fd_kN { get; set; }
        public double Fdl_kN { get; set; }
        public double Me_kNm { get; set; }

        public double Delta { get; set; }
        public double BetaEffective { get; set; }
        public double Rpad_mm { get; set; }

        public double SigmaRd_Nmm2 { get; set; }

        public double DpinToHoleRatio { get; set; }
        public bool PinDiameterRecommendationActive { get; set; }
        public bool PinDiameterRecommendationOk { get; set; }
        public double PinDiameterRecommendationUtilization { get; set; }
        public bool BearingFormulaWithClearance { get; set; }
        public double SigmaEd1_Nmm2 { get; set; }
        public bool DnvBearingOk { get; set; }
        public double DnvBearingUtilization { get; set; }

        public double SigmaEd2_Nmm2 { get; set; }
        public bool TearOutOk { get; set; }
        public double TearOutUtilization { get; set; }

        public double Dch_mm { get; set; }
        public double SigmaEd3_Nmm2 { get; set; }
        public double Fvwd_Nmm2 { get; set; }
        public bool CheekPlateWeldOk { get; set; }
        public double CheekPlateWeldUtilization { get; set; }

        public bool IsOk =>
            !HasErrors &&
            (!OutOfPlaneChecksActive || (DnvBearingOk && TearOutOk)) &&
            (!CheekPlateWeldCheckActive || CheekPlateWeldOk);

        public List<CheckItem> CheckItems
        {
            get
            {
                if (HasErrors)
                    return new List<CheckItem>();

                List<CheckItem> items = new();

                if (OutOfPlaneChecksActive)
                {
                    items.Add(new CheckItem
                    {
                        Name = PinDiameterRecommendationOk
                        ? "Pin diameter recommendation for significant angled pull fulfilled: Dpin / DH >= 0.94"
                        : "Pin diameter recommendation for significant angled pull NOT fulfilled: Dpin / DH >= 0.94",
                        Utilization = PinDiameterRecommendationUtilization,
                        IsOk = PinDiameterRecommendationOk,
                        ShowUtilization = false
                    });
                    items.Add(new CheckItem
                    {
                        Name = "Bearing at angled pull (according to DNV standard)",
                        Utilization = DnvBearingUtilization,
                        IsOk = DnvBearingOk
                    });

                    items.Add(new CheckItem
                    {
                        Name = "tear-out at angled pull (according to DNV standard)",
                        Utilization = TearOutUtilization,
                        IsOk = TearOutOk
                    });
                }

                if (CheekPlateWeldCheckActive)
                {
                    items.Add(new CheckItem
                    {
                        Name = "Cheek plate weld",
                        Utilization = CheekPlateWeldUtilization,
                        IsOk = CheekPlateWeldOk
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
