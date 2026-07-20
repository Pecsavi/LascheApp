using System;
using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class OuterLugCheckInput
    {
        public bool IsActive { get; set; }
        public double F_Ed_kN { get; set; }
        public double F_Ed_ser_kN { get; set; }
        public double ThicknessT2_mm { get; set; }
        public double PlateWidth_mm { get; set; }
        public double EndDistanceE_mm { get; set; }
        public double HoleDiameter_mm { get; set; }
        public double PinDiameter_mm { get; set; }
        public double Fy_Nmm2 { get; set; }
        public double Fu_Nmm2 { get; set; }
        public double E_Nmm2 { get; set; }
        public double GammaM0 { get; set; } = 1.0;
        public double GammaM6_ser { get; set; } = 1.0;
        public bool IsReplaceablePin { get; set; }
    }

    public class OuterLugCheckResult
    {
        public OuterLugCheckInput Input { get; set; } = new();
        public PadeyeBearingResult BearingResult { get; set; } = new();
        public PadeyeEcGeometryResult GeometryResult { get; set; } = new();

        public bool IsActive => Input.IsActive;
        // Method B is checked first with the actual user geometry. If any of
        // its four conditions fails, Method A is checked with the same e and b.
        public bool MethodBSelected =>
            GeometryResult.ThicknessMoglichkeitB_Ok &&
            GeometryResult.HoleDiameterMoglichkeitB_Ok &&
            GeometryResult.EdgeDistanceMoglichkeitB_Ok &&
            GeometryResult.PlateWidthMoglichkeitB_Ok;
        public bool MethodASelected =>
            !MethodBSelected && GeometryResult.MoglichkeitA_Ok;
        public bool GeometryOk => MethodBSelected || MethodASelected;
        public double ThicknessFromForce_mm => GeometryResult.RequiredThickness_MoglichkeitB_mm;
        public double ThicknessFromHole_mm => Input.HoleDiameter_mm / 2.5;
        public double RequiredMinimumThickness_mm => Math.Max(ThicknessFromForce_mm, ThicknessFromHole_mm);

        public double RequiredE_mm => MethodBSelected
            ? GeometryResult.RequiredEdgeDistance_MoglichkeitB_mm
            : GeometryResult.RequiredEdgeDistanceA_mm + Input.HoleDiameter_mm / 2.0;

        public double RequiredB_mm => MethodBSelected
            ? GeometryResult.RequiredPlateWidth_MoglichkeitB_mm
            : 2.0 * GeometryResult.RequiredSideDistanceC_mm + Input.HoleDiameter_mm;

        public double RequiredThicknessBearingDesign_mm =>
            Input.ThicknessT2_mm * BearingResult.BearingDesignUtilization;

        public double RequiredThicknessBearingService_mm => Input.IsReplaceablePin
            ? Input.ThicknessT2_mm * BearingResult.BearingServiceUtilization
            : 0.0;

        public double RequiredThicknessContactStress_mm =>
            Input.IsReplaceablePin && BearingResult.PinHoleGeometryOk
                ? Input.ThicknessT2_mm * Math.Pow(BearingResult.HolePinStressUtilization, 2.0)
                : 0.0;

        public double RequiredThicknessFromBearing_mm => Math.Max(
            RequiredThicknessBearingDesign_mm,
            Math.Max(RequiredThicknessBearingService_mm, RequiredThicknessContactStress_mm));

        public double RequiredThicknessFromBearingRounded_mm =>
            Math.Ceiling(RequiredThicknessFromBearing_mm / 10.0) * 10.0;

        public bool IsOk => !IsActive || (BearingResult.IsOk && GeometryOk);

        public List<CheckItem> CheckItems
        {
            get
            {
                if (!IsActive)
                    return new List<CheckItem>();

                return new List<CheckItem>
                {
                    new CheckItem
                    {
                        Name = IsOk
                            ? $"Lug t2 check - Method {(MethodBSelected ? "B" : "A")}"
                            : !BearingResult.PinHoleGeometryOk
                                ? "Lug t2 check NOT OK - pin diameter d_t2 must be smaller than d0_t2"
                                : !BearingResult.IsOk
                                    ? $"Lug t2 check NOT OK - increase t2 to at least {RequiredThicknessFromBearingRounded_mm:0.0} mm"
                                    : "Lug t2 geometry NOT OK - provided e and b do not satisfy Method A or B",
                        IsOk = IsOk,
                        Utilization = MaxUtilization
                    }
                };
            }
        }

        public double MaxUtilization => new[]
            {
                BearingResult.BearingDesignUtilization,
                Input.IsReplaceablePin ? BearingResult.BearingServiceUtilization : 0.0,
                Input.IsReplaceablePin ? BearingResult.HolePinStressUtilization : 0.0
            }
            .Max();
    }

    public static class OuterLugChecker
    {
        public static OuterLugCheckResult Check(OuterLugCheckInput input)
        {
            double edgeDistanceA_mm = input.EndDistanceE_mm - input.HoleDiameter_mm / 2.0;
            double sideDistanceC_mm = (input.PlateWidth_mm - input.HoleDiameter_mm) / 2.0;

            PadeyeBearingResult bearing = PadeyeBearingChecker.Check(new PadeyeBearingInput
            {
                F_Ed_kN = input.F_Ed_kN,
                F_Ed_ser_kN = input.F_Ed_ser_kN,
                PlateThickness_mm = input.ThicknessT2_mm,
                HoleDiameter_mm = input.HoleDiameter_mm,
                PinDiameter_mm = input.PinDiameter_mm,
                Fy_Nmm2 = input.Fy_Nmm2,
                E_Nmm2 = input.E_Nmm2,
                GammaM0 = input.GammaM0,
                GammaM6_ser = input.GammaM6_ser,
                IsReplaceablePin = input.IsReplaceablePin
            });

            PadeyeEcGeometryResult geometry = PadeyeEcGeometryChecker.Check(new PadeyeEcGeometryInput
            {
                F_Ed_kN = input.F_Ed_kN,
                GammaM0 = input.GammaM0,
                Fy_Nmm2 = input.Fy_Nmm2,
                PlateThickness_mm = input.ThicknessT2_mm,
                CheekPlateThickness_mm = 0.0,
                IncludeCheekPlatesInBearing = false,
                HoleDiameter_mm = input.HoleDiameter_mm,
                EdgeDistanceA_mm = edgeDistanceA_mm,
                SideDistanceC_mm = sideDistanceC_mm
            });

            return new OuterLugCheckResult
            {
                Input = input,
                BearingResult = bearing,
                GeometryResult = geometry
            };
        }
    }
}
