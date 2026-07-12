using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PadeyeBasicCheckInput
    {
        public LugType LugType { get; set; } = LugType.TransportLug;
        public double F_Ed_kN { get; set; }
        public double F_Ed_ser_kN { get; set; }

        // Main lug plate thickness according to DNV notation: tpl
        public double PlateThickness_mm { get; set; }
        public double HoleDiameter_mm { get; set; }

        public double ShackleWLL_kN { get; set; }
        public double ShackleDpin_mm { get; set; }
        public double ShackleB1_mm { get; set; }
        public double ShackleH_DNV_mm { get; set; }

        // Transport lug recommendation: Dpin < d0 <= Dpin + PinClearance_mm
        public double PinClearance_mm { get; set; } = 3.0;

        public double PlateWidth_mm { get; set; }
        public double MaterialFy_Nmm2 { get; set; }
        public double GammaM0 { get; set; } = 1.0;

        public double CheekPlateThickness_mm { get; set; }
        public bool IncludeCheekPlatesInBearing { get; set; }

        public double TotalBearingThickness_mm =>
            IncludeCheekPlatesInBearing
                ? PlateThickness_mm + 2.0 * CheekPlateThickness_mm
                : PlateThickness_mm;

        public double MaxCheekPlateThickness_mm => PlateThickness_mm / 2.0;


    }

    public class PadeyeBasicCheckResult
    {
        public PadeyeBasicCheckInput Input { get; set; } = new();

        public double RecommendedHoleDiameterMin_mm { get; set; }
        public double RecommendedHoleDiameterMax_mm { get; set; }

        public double RequiredThickness_mm { get; set; }
        public double ThicknessForB1Check_mm { get; set; }

        public bool WllOk { get; set; }
        public bool HoleDiameterRecommendationOk { get; set; }
        public bool ThicknessRecommendationOk { get; set; }
        public bool CheekPlateThicknessLimitOk { get; set; } = true;

        // Kept for possible later use, but no longer part of Transport Lug report/governing result.
        public double GrossArea_mm2 { get; set; }
        public double SigmaGrossEd_Nmm2 { get; set; }
        public bool GrossSectionTensionOk { get; set; }
        public double GrossSectionTensionUtilization { get; set; }

        public double NetArea_mm2 { get; set; }
        public double SigmaEd_Nmm2 { get; set; }
        public double SigmaRd_Nmm2 { get; set; }
        public bool NetSectionTensionOk { get; set; }
        public double NetSectionTensionUtilization { get; set; }

        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;

        public double WllUtilization { get; set; }
        public double HoleDiameterRecommendationUtilization { get; set; }
        public double ThicknessRecommendationUtilization { get; set; }
        // Backward-compatible aliases for older report/Form1 code.
        // Do not use these names in new code.
        public double RequiredHoleDiameter_mm => RecommendedHoleDiameterMax_mm;

        public bool HoleDiameterOk => HoleDiameterRecommendationOk;
        public bool ThicknessOk => ThicknessRecommendationOk;

        public double HoleDiameterUtilization => HoleDiameterRecommendationUtilization;
        public double ThicknessUtilization => ThicknessRecommendationUtilization;

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
            CheekPlateThicknessLimitOk;

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
                        Name = HoleDiameterRecommendationOk
                            ? "Hole diameter clearance recommendation OK"
                            : $"Hole diameter clearance recommendation NOT fulfilled: required {Input.ShackleDpin_mm:0.0} mm < d0 <= {RecommendedHoleDiameterMax_mm:0.0} mm",
                        Utilization = HoleDiameterRecommendationUtilization,
                        IsOk = HoleDiameterRecommendationOk,
                        ShowUtilization = false,
                        IsWarning = !HoleDiameterRecommendationOk
                    });

                    items.Add(new CheckItem
                    {
                        Name = ThicknessRecommendationOk
                            ? "Shackle B1 thickness recommendation OK"
                            : $"Shackle B1 thickness recommendation NOT fulfilled: required thickness >= {RequiredThickness_mm:0.0} mm",
                        Utilization = ThicknessRecommendationUtilization,
                        IsOk = ThicknessRecommendationOk,
                        ShowUtilization = false,
                        IsWarning = !ThicknessRecommendationOk
                    });

                    if (Input.IncludeCheekPlatesInBearing)
                    {
                        items.Add(new CheckItem
                        {
                            Name = "Cheek plates included in bearing resistance. Valid only if cheek plates are welded before final drilling/reaming of the hole.",
                            Utilization = 0.0,
                            IsOk = true,
                            ShowUtilization = false,
                            IsWarning = true
                        });

                        items.Add(new CheckItem
                        {
                            Name = CheekPlateThicknessLimitOk
                                ? "Cheek plate thickness limit OK: tch <= tpl / 2"
                                : $"Cheek plate thickness limit NOT fulfilled: tch <= tpl / 2 = {Input.MaxCheekPlateThickness_mm:0.0} mm",
                            Utilization = 0.0,
                            IsOk = CheekPlateThicknessLimitOk,
                            ShowUtilization = false,
                            IsWarning = !CheekPlateThicknessLimitOk
                        });
                    }
                }

                return items;
            }
        }
    }
}
