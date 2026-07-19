using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LascheApp.Padeye
{
    public class PadeyeTensionLugReportInfo
    {
        public string Project { get; set; } = "";
        public string Subject { get; set; } = "";
        public string PreparedBy { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Today;

        public string PlateMaterial { get; set; } = "";
        public string PinMaterial { get; set; } = "";
    }

    public static class PadeyeTensionLugReportFormatter
    {
        public static string Format(
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult,
            PadeyeTensionLugReportInfo info)
        {
            StringBuilder sb = new();

            AppendProjectInformation(sb, info);
            AppendSeparator(sb);

            AppendHeader(sb, lugResult, pinResult);
            AppendBasicInputData(sb, lugResult, pinResult, info);
            AppendSeparator(sb);

            AppendCheckSummary(sb, lugResult, pinResult);
            AppendSeparator(sb);

            AppendCalculation(sb, lugResult, pinResult);

            return sb.ToString().TrimEnd();
        }

        private static void AppendProjectInformation(
            StringBuilder sb,
            PadeyeTensionLugReportInfo info)
        {
            sb.AppendLine("Project information");
            sb.AppendLine("===================");
            sb.AppendLine();
            sb.AppendLine($"Project:       {info.Project}");
            sb.AppendLine($"Subject:       {info.Subject}");
            sb.AppendLine($"Prepared by:   {info.PreparedBy}");
            sb.AppendLine($"Date:          {info.Date:yyyy-MM-dd}");
        }

        private static void AppendHeader(
            StringBuilder sb,
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult)
        {
            bool overallOk = lugResult.IsOk && pinResult.IsOk;

            List<CheckItem> allItems = GetAllCheckItems(lugResult, pinResult);
            double maxUtilization = GetMaxUtilization(allItems);
            string governingCheck = GetGoverningCheckName(allItems);

            sb.AppendLine("Tension Lug verification (DIN EN 1993-1-8)");
            sb.AppendLine("==========================================");
            sb.AppendLine();
            sb.AppendLine($"Overall result: {Ok(overallOk)}");
            sb.AppendLine($"Max utilization: η = {FmtEta(maxUtilization)}");
            sb.AppendLine($"Governing utilization: {governingCheck}");

            List<string> failedChecks = GetFailedChecks(lugResult, pinResult);

            if (failedChecks.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Failed check(s):");

                foreach (string failedCheck in failedChecks)
                    sb.AppendLine($"- {failedCheck}");
            }

            sb.AppendLine();
        }

        private static void AppendBasicInputData(
            StringBuilder sb,
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult,
            PadeyeTensionLugReportInfo info)
        {
            PadeyeBasicCheckInput basic = lugResult.BasicResult.Input;
            PadeyeEcGeometryInput ec = lugResult.EcGeometryResult.Input;
            PadeyeBearingInput bearing = lugResult.BearingResult.Input;
            PadeyeDnvOutOfPlaneInput dnv = lugResult.DnvOutOfPlaneResult.Input;
            PinCheckInput pin = pinResult.Input;

            double e_mm = ec.EdgeDistanceA_mm + ec.HoleDiameter_mm / 2.0;

            sb.AppendLine("Input data");
            sb.AppendLine("==========");
            sb.AppendLine();
            sb.AppendLine("Materials");
            sb.AppendLine("---------");
            sb.AppendLine($"E = {Fmt1(dnv.E_Nmm2)} N/mm²");
            sb.AppendLine($"- Lug: {info.PlateMaterial} (fy = {Fmt1(basic.MaterialFy_Nmm2)} N/mm²; fu = {Fmt1(dnv.Fu_Nmm2)} N/mm²; betaW = {Fmt3(dnv.BetaW)})");
            if (lugResult.OuterLugResult.IsActive)
            {
                OuterLugCheckInput outer = lugResult.OuterLugResult.Input;
                sb.AppendLine($"- Outer lug plates t2: {info.PlateMaterial} (fy = {Fmt1(outer.Fy_Nmm2)} N/mm²; fu = {Fmt1(outer.Fu_Nmm2)} N/mm²)");
            }
            sb.AppendLine($"- Pin: {info.PinMaterial} (fy = {Fmt1(pin.PinFy_Nmm2)} N/mm²; fu = {Fmt1(pin.PinFu_Nmm2)} N/mm²)");
            sb.AppendLine();

            sb.AppendLine("Loads");
            sb.AppendLine("-----");
            sb.AppendLine($"F_Ed,ser = {Fmt2(basic.F_Ed_ser_kN)} kN");
            sb.AppendLine($"F_Ed     = {Fmt2(basic.F_Ed_kN)} kN");
            if (lugResult.OuterLugResult.IsActive)
            {
                sb.AppendLine($"F_Ed,ser,t2 = F_Ed,ser / 2 = {Fmt2(lugResult.OuterLugResult.Input.F_Ed_ser_kN)} kN");
                sb.AppendLine($"F_Ed,t2     = F_Ed / 2     = {Fmt2(lugResult.OuterLugResult.Input.F_Ed_kN)} kN");
            }
            sb.AppendLine();

            sb.AppendLine("Geometry");
            sb.AppendLine("--------");
            sb.AppendLine($"tpl = {Fmt1(basic.PlateThickness_mm)} mm");

            if (basic.CheekPlateThickness_mm > 0.0)
            {
                string cheekPlateNote = basic.IncludeCheekPlatesInBearingRequested
                    ? "requested for resistance; applicability is verified separately"
                    : "considered in geometry only, not in resistance";
                sb.AppendLine($"tch = {Fmt1(basic.CheekPlateThickness_mm)} mm ({cheekPlateNote})");

                if (basic.IncludeCheekPlatesInBearing)
                    sb.AppendLine($"t = tpl + 2 * tch = {Fmt1(basic.TotalBearingThickness_mm)} mm");
                else
                    sb.AppendLine($"t = tpl = {Fmt1(basic.PlateThickness_mm)} mm");
            }
            else
            {
                sb.AppendLine($"t = tpl = {Fmt1(basic.PlateThickness_mm)} mm");
            }

            sb.AppendLine($"d0 = {Fmt1(basic.HoleDiameter_mm)} mm");
            sb.AppendLine($"b = {Fmt1(basic.PlateWidth_mm)} mm");
            sb.AppendLine($"e = {Fmt1(e_mm)} mm");
            if (basic.CheekPlateThickness_mm > 0.0)
            {
                sb.AppendLine($"Rch = {Fmt1(dnv.Rch_mm)} mm");
                sb.AppendLine($"a_weld = {Fmt1(dnv.WeldA_mm)} mm");
            }
            sb.AppendLine();
            sb.AppendLine($"Pin diameter d = {Fmt1(pin.PinDiameter_mm)} mm");

            if (pin.MomentCalculatedFromTensionLugGeometry)
            {
                sb.AppendLine($"t2 = {Fmt1(pin.OuterLugThicknessT2_mm)} mm");
                sb.AppendLine($"s = {Fmt1(pin.GapS_mm)} mm");
            }

            sb.AppendLine();
            sb.AppendLine($"Replaceable pin checks: {(bearing.IsReplaceablePin ? "active" : "not active")}");
            sb.AppendLine();
        }

        private static void AppendCheckSummary(
            StringBuilder sb,
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult)
        {
            List<CheckItem> allItems = GetAllCheckItems(lugResult, pinResult);

            List<CheckItem> geometryItems = allItems
                .Where(i => !i.ShowUtilization)
                .ToList();

            List<CheckItem> utilizationItems = allItems
                .Where(i => i.ShowUtilization)
                .OrderByDescending(i => i.Utilization)
                .ToList();

            sb.AppendLine("1. Check summary");
            sb.AppendLine("----------------");

            sb.AppendLine();
            sb.AppendLine("Geometry checks");
            sb.AppendLine("---------------");

            if (geometryItems.Count == 0)
            {
                sb.AppendLine("-");
            }
            else
            {
                foreach (CheckItem item in geometryItems)
                    sb.AppendLine(FormatGeometrySummaryLine(item));
            }

            sb.AppendLine();
            sb.AppendLine("Utilization checks");
            sb.AppendLine("------------------");

            if (utilizationItems.Count == 0)
            {
                sb.AppendLine("-");
            }
            else
            {
                foreach (CheckItem item in utilizationItems)
                    sb.AppendLine(FormatUtilizationSummaryLine(item));
            }
        }

        private static void AppendCalculation(
            StringBuilder sb,
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult)
        {
            sb.AppendLine("2. Calculation");
            sb.AppendLine("==============");
            sb.AppendLine();

            sb.AppendLine("2.1. Lug");
            sb.AppendLine();

            AppendLugOverall(sb, lugResult);
            AppendEcGeometry(sb, lugResult.EcGeometryResult);
            AppendBearing(sb, lugResult.BearingResult);
            AppendOuterLugPlates(sb, lugResult.OuterLugResult);
            AppendCheekPlateWeld(sb, lugResult.DnvOutOfPlaneResult);

            sb.AppendLine();
            sb.AppendLine("2.2. Pin");
            sb.AppendLine();

            AppendPinOverall(sb, pinResult);
            AppendPinChecks(sb, pinResult);
        }

        private static void AppendLugOverall(
            StringBuilder sb,
            PadeyeCheckResult result)
        {
            sb.AppendLine($"\tLug result: {Ok(result.IsOk)}");
            sb.AppendLine($"\tMax utilization: η = {FmtEta(result.MaxUtilization)}");
            sb.AppendLine($"\tGoverning check: {result.GoverningCheckName}");
            sb.AppendLine();
        }

        private static void AppendEcGeometry(
            StringBuilder sb,
            PadeyeEcGeometryResult result)
        {
            PadeyeEcGeometryInput input = result.Input;

            if (result.HasErrors)
            {
                AppendErrorBlock(sb, "\tEC geometry check", result.Errors);
                return;
            }

            double e_mm =
                input.EdgeDistanceA_mm +
                input.HoleDiameter_mm / 2.0;

            double requiredE_mm =
                result.RequiredEdgeDistanceA_mm +
                input.HoleDiameter_mm / 2.0;

            double b_mm =
                2.0 * input.SideDistanceC_mm +
                input.HoleDiameter_mm;

            double requiredB_mm =
                2.0 * result.RequiredSideDistanceC_mm +
                input.HoleDiameter_mm;

            sb.AppendLine("\tMethod A");
            sb.AppendLine("\t--------");
            sb.AppendLine();
            sb.AppendLine("\t\t1. Minimum distances from load");
            sb.AppendLine("\t\t--------------------------------");
            sb.AppendLine($"\t\tamin = F_Ed * gammaM0 / (2 * t * fy) + 2 * d0 / 3 = {Fmt1(result.RequiredEdgeDistanceA_mm)} mm");
            sb.AppendLine($"\t\tcmin = F_Ed * gammaM0 / (2 * t * fy) + d0 / 3 = {Fmt1(result.RequiredSideDistanceC_mm)} mm");
            sb.AppendLine();
            sb.AppendLine("\t\t2. Required overall geometry");
            sb.AppendLine("\t\t------------------------------");
            sb.AppendLine($"\t\temin = amin + d0 / 2 = {Fmt1(requiredE_mm)} mm");
            sb.AppendLine($"\t\tbmin = 2 * cmin + d0 = {Fmt1(requiredB_mm)} mm");
            sb.AppendLine();
            sb.AppendLine($"\t\tProvided e = {Fmt1(e_mm)} mm");
            sb.AppendLine($"\t\tCheck e >= emin: {Ok(result.EdgeDistanceA_Ok)}");
            sb.AppendLine($"\t\tProvided b = {Fmt1(b_mm)} mm");
            sb.AppendLine($"\t\tCheck b >= bmin: {Ok(result.SideDistanceC_Ok)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tMethod A result: {Ok(result.MoglichkeitA_Ok)}");
            sb.AppendLine();

            sb.AppendLine("\tMethod B");
            sb.AppendLine("\t--------");
            sb.AppendLine($"\t\td0,max = 2.5 * t = {Fmt1(result.MaxHoleDiameter_MoglichkeitB_mm)} mm");
            sb.AppendLine($"\t\tCheck d0 <= 2.5 * t: {Ok(result.HoleDiameterMoglichkeitB_Ok)}");
            sb.AppendLine();

            sb.AppendLine($"\t\ttmin = 0.7 * sqrt(F_Ed * gammaM0 / fy) = {Fmt1(result.RequiredThickness_MoglichkeitB_mm)} mm");
            sb.AppendLine($"\t\tCheck t >= tmin: {Ok(result.ThicknessMoglichkeitB_Ok)}");
            sb.AppendLine();

            sb.AppendLine($"\t\temin = 1.6 * d0 = {Fmt1(result.RequiredEdgeDistance_MoglichkeitB_mm)} mm");
            sb.AppendLine($"\t\tCheck e >= emin: {Ok(result.EdgeDistanceMoglichkeitB_Ok)}");
            sb.AppendLine();

            sb.AppendLine($"\t\tbmin = 2.5 * d0 = {Fmt1(result.RequiredPlateWidth_MoglichkeitB_mm)} mm");
            sb.AppendLine($"\t\tCheck b >= bmin: {Ok(result.PlateWidthMoglichkeitB_Ok)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tMethod B result: {Ok(result.MoglichkeitB_Ok)}");
            sb.AppendLine();
        }

        private static void AppendBearing(
            StringBuilder sb,
            PadeyeBearingResult result)
        {
            PadeyeBearingInput input = result.Input;

            if (result.HasErrors)
            {
                AppendErrorBlock(sb, "\tPin-hole bearing", result.Errors);
                return;
            }

            sb.AppendLine("\tPin-hole bearing design");
            sb.AppendLine("\t-----------------------");
            sb.AppendLine($"\t\tGeometry check d < d0: {Ok(result.PinHoleGeometryOk)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tFb,Rd = 1.5 * t * d * fy / gammaM0 = {Fmt2(result.FbRd_kN)} kN");
            sb.AppendLine($"\t\tCheck Fb,Ed <= Fb,Rd: {Ok(result.BearingDesignOk)}  η = {FmtEta(result.BearingDesignUtilization)}");
            sb.AppendLine();

            if (input.IsReplaceablePin)
            {
                sb.AppendLine("\tService bearing (Replaceable pin)");
                sb.AppendLine("\t---------------------------------");
                sb.AppendLine($"\t\tFb,Ed,ser = {Fmt2(input.F_Ed_ser_kN)} kN");
                sb.AppendLine($"\t\tFb,Rd,ser = 0.6 * t * d * fy / gammaM6,ser = {Fmt2(result.FbRdSer_kN)} kN");
                sb.AppendLine($"\t\tCheck Fb,Ed,ser <= Fb,Rd,ser: {Ok(result.BearingServiceOk)}  η = {FmtEta(result.BearingServiceUtilization)}");
                sb.AppendLine();

                if (result.PinHoleGeometryOk)
                {
                    sb.AppendLine("\tContact stress (Replaceable pin)");
                    sb.AppendLine("\t--------------------------------");
                    sb.AppendLine($"\t\tsigma_h,Ed = 0.591 * sqrt(E * Fb,Ed,ser * (d0 - d) / (d² * t)) = {Fmt1(result.SigmaHEd_Nmm2)} N/mm²");
                    sb.AppendLine($"\t\tfh,Rd = 2.5 * fy / gammaM6,ser = {Fmt1(result.FhRd_Nmm2)} N/mm²");
                    sb.AppendLine($"\t\tCheck sigma_h,Ed <= fh,Rd: {Ok(result.HolePinStressOk)}  η = {FmtEta(result.HolePinStressUtilization)}");
                    sb.AppendLine();
                }
                else
                {
                    sb.AppendLine("\tContact stress (Replaceable pin)");
                    sb.AppendLine("\t--------------------------------");
                    sb.AppendLine("\t\tNOT CALCULATED: pin diameter d must be smaller than hole diameter d0.");
                    sb.AppendLine();
                }
            }
        }

        private static void AppendOuterLugPlates(StringBuilder sb, OuterLugCheckResult result)
        {
            if (!result.IsActive)
                return;

            OuterLugCheckInput input = result.Input;
            PadeyeEcGeometryResult geometry = result.GeometryResult;
            PadeyeBearingResult bearing = result.BearingResult;

            sb.AppendLine("\tOuter lug plates t2");
            sb.AppendLine("\t-------------------");
            sb.AppendLine($"\t\tResult: {Ok(result.IsOk)}");
            sb.AppendLine($"\t\tMax utilization: η = {FmtEta(result.MaxUtilization)}");
            sb.AppendLine();

            sb.AppendLine("\t\tPin-hole bearing design (one outer plate)");
            sb.AppendLine("\t\t----------------------------------------");
            sb.AppendLine($"\t\tFb,Rd = 1.5 * t2 * d * fy,t2 / gammaM0 = {Fmt2(bearing.FbRd_kN)} kN");
            sb.AppendLine($"\t\tCheck F_Ed,t2 <= Fb,Rd: {Ok(bearing.BearingDesignOk)}  η = {FmtEta(bearing.BearingDesignUtilization)}");
            sb.AppendLine();

            if (input.IsReplaceablePin)
            {
                sb.AppendLine("\t\tService bearing (one outer plate)");
                sb.AppendLine("\t\t---------------------------------");
                sb.AppendLine($"\t\tFb,Rd,ser = 0.6 * t2 * d * fy,t2 / gammaM6,ser = {Fmt2(bearing.FbRdSer_kN)} kN");
                sb.AppendLine($"\t\tCheck F_Ed,ser,t2 <= Fb,Rd,ser: {Ok(bearing.BearingServiceOk)}  η = {FmtEta(bearing.BearingServiceUtilization)}");
                sb.AppendLine();

                if (bearing.PinHoleGeometryOk)
                {
                    sb.AppendLine("\t\tContact stress (one outer plate)");
                    sb.AppendLine("\t\t--------------------------------");
                    sb.AppendLine($"\t\tsigma_h,Ed = 0.591 * sqrt(E * F_Ed,ser,t2 * (d0 - d) / (d² * t2)) = {Fmt1(bearing.SigmaHEd_Nmm2)} N/mm²");
                    sb.AppendLine($"\t\tfh,Rd = 2.5 * fy,t2 / gammaM6,ser = {Fmt1(bearing.FhRd_Nmm2)} N/mm²");
                    sb.AppendLine($"\t\tCheck sigma_h,Ed <= fh,Rd: {Ok(bearing.HolePinStressOk)}  η = {FmtEta(bearing.HolePinStressUtilization)}");
                    sb.AppendLine();
                }
            }

            if (!bearing.IsOk)
            {
                sb.AppendLine("\t\tRequired t2 from bearing checks");
                sb.AppendLine("\t\t-------------------------------");
                sb.AppendLine($"\t\tt2,design = t2 * ηdesign = {Fmt1(result.RequiredThicknessBearingDesign_mm)} mm");
                if (input.IsReplaceablePin)
                {
                    sb.AppendLine($"\t\tt2,service = t2 * ηservice = {Fmt1(result.RequiredThicknessBearingService_mm)} mm");
                    if (bearing.PinHoleGeometryOk)
                        sb.AppendLine($"\t\tt2,contact = t2 * ηcontact² = {Fmt1(result.RequiredThicknessContactStress_mm)} mm");
                }
                sb.AppendLine($"\t\tRequired t2, rounded up to 10 mm = {Fmt1(result.RequiredThicknessFromBearingRounded_mm)} mm");
                sb.AppendLine();
                return;
            }

            sb.AppendLine("\t\tGeometry - Method B thickness conditions");
            sb.AppendLine("\t\t----------------------------------------");
            sb.AppendLine($"\t\ttmin,force = 0.7 * sqrt(F_Ed,t2 * gammaM0 / fy,t2) = {Fmt1(result.ThicknessFromForce_mm)} mm");
            sb.AppendLine($"\t\tCheck t2 >= tmin,force: {Ok(geometry.ThicknessMoglichkeitB_Ok)}");
            sb.AppendLine($"\t\ttmin,hole = d0 / 2.5 = {Fmt1(result.ThicknessFromHole_mm)} mm");
            sb.AppendLine($"\t\tCheck t2 >= tmin,hole (d0 <= 2.5 * t2): {Ok(geometry.HoleDiameterMoglichkeitB_Ok)}");
            sb.AppendLine($"\t\temin,B = 1.6 * d0 = {Fmt1(geometry.RequiredEdgeDistance_MoglichkeitB_mm)} mm; provided e = {Fmt1(input.EndDistanceE_mm)} mm: {Ok(geometry.EdgeDistanceMoglichkeitB_Ok)}");
            sb.AppendLine($"\t\tbmin,B = 2.5 * d0 = {Fmt1(geometry.RequiredPlateWidth_MoglichkeitB_mm)} mm; provided b = {Fmt1(input.PlateWidth_mm)} mm: {Ok(geometry.PlateWidthMoglichkeitB_Ok)}");
            sb.AppendLine();

            if (result.MethodBSelected)
            {
                sb.AppendLine("\t\tMethod B selected");
                sb.AppendLine($"\t\temin = 1.6 * d0 = {Fmt1(geometry.RequiredEdgeDistance_MoglichkeitB_mm)} mm");
                sb.AppendLine($"\t\tbmin = 2.5 * d0 = {Fmt1(geometry.RequiredPlateWidth_MoglichkeitB_mm)} mm");
            }
            else if (result.MethodASelected)
            {
                double requiredE_A_mm = geometry.RequiredEdgeDistanceA_mm + input.HoleDiameter_mm / 2.0;
                double requiredB_A_mm = 2.0 * geometry.RequiredSideDistanceC_mm + input.HoleDiameter_mm;
                sb.AppendLine("\t\tMethod B is not fulfilled; Method A selected");
                sb.AppendLine($"\t\tamin = F_Ed,t2 * gammaM0 / (2 * t2 * fy,t2) + 2 * d0 / 3 = {Fmt1(geometry.RequiredEdgeDistanceA_mm)} mm");
                sb.AppendLine($"\t\tcmin = F_Ed,t2 * gammaM0 / (2 * t2 * fy,t2) + d0 / 3 = {Fmt1(geometry.RequiredSideDistanceC_mm)} mm");
                sb.AppendLine($"\t\temin = amin + d0 / 2 = {Fmt1(requiredE_A_mm)} mm; provided e = {Fmt1(input.EndDistanceE_mm)} mm: {Ok(geometry.EdgeDistanceA_Ok)}");
                sb.AppendLine($"\t\tbmin = 2 * cmin + d0 = {Fmt1(requiredB_A_mm)} mm; provided b = {Fmt1(input.PlateWidth_mm)} mm: {Ok(geometry.SideDistanceC_Ok)}");
            }
            else
            {
                double requiredE_A_mm = geometry.RequiredEdgeDistanceA_mm + input.HoleDiameter_mm / 2.0;
                double requiredB_A_mm = 2.0 * geometry.RequiredSideDistanceC_mm + input.HoleDiameter_mm;
                sb.AppendLine("\t\tMethod B NOT OK; Method A checked with the provided geometry");
                sb.AppendLine($"\t\temin,A = {Fmt1(requiredE_A_mm)} mm; provided e = {Fmt1(input.EndDistanceE_mm)} mm: {Ok(geometry.EdgeDistanceA_Ok)}");
                sb.AppendLine($"\t\tbmin,A = {Fmt1(requiredB_A_mm)} mm; provided b = {Fmt1(input.PlateWidth_mm)} mm: {Ok(geometry.SideDistanceC_Ok)}");
                sb.AppendLine("\t\tLug t2 geometry result: NOT OK");
            }
            sb.AppendLine();
        }
        private static void AppendCheekPlateWeld(
            StringBuilder sb,
            PadeyeDnvOutOfPlaneResult result)
        {
            if (!result.CheekPlateWeldCheckActive)
                return;

            PadeyeDnvOutOfPlaneInput input = result.Input;

            if (result.HasErrors)
            {
                AppendErrorBlock(sb, "\tCheek plate weld", result.Errors);
                return;
            }

            sb.AppendLine("\tCheek plate weld");
            sb.AppendLine("\t----------------");
            sb.AppendLine($"\t\tFd = F_Ed * cos(alpha) = {Fmt2(result.Fd_kN)} kN");
            sb.AppendLine($"\t\talpha = {Fmt1(input.Alpha_deg)}°");
            sb.AppendLine($"\t\ttch = {Fmt1(input.CheekPlateThickness_mm)} mm");
            sb.AppendLine($"\t\tt = tpl + 2 * tch = {Fmt1(input.TotalThickness_mm)} mm");
            sb.AppendLine($"\t\tDCH = 2 * Rch = {Fmt1(result.Dch_mm)} mm");
            sb.AppendLine($"\t\ta = {Fmt1(input.WeldA_mm)} mm");

            if (Math.Abs(input.Alpha_deg) > 1e-9)
            sb.AppendLine($"\t\tdelta = {Fmt3(result.Delta)}");
            else
                sb.AppendLine("\t\tdelta = 1.000 (straight tension)");

            sb.AppendLine("\t\tDemand according to DNV-OS-H205 B.2.6:");
            sb.AppendLine($"\t\tsigma_Ed,w = Fd * tch / (1.5 * t * DCH * a) * delta = {Fmt1(result.SigmaEd3_Nmm2)} N/mm²");
            sb.AppendLine();

            sb.AppendLine("\t\tWeld resistance according to EN 1993-1-8:");
            sb.AppendLine($"\t\tfvw,Rd = fu / (sqrt(3) * betaW * gammaM2) = {Fmt1(result.Fvwd_Nmm2)} N/mm²");
            sb.AppendLine();

            sb.AppendLine($"\t\tCheck sigma_Ed,w <= fvw,Rd: {Ok(result.CheekPlateWeldOk)}  η = {FmtEta(result.CheekPlateWeldUtilization)}");
            sb.AppendLine(); 
        }
        private static void AppendPinOverall(
            StringBuilder sb,
            PinCheckResult result)
        {
            sb.AppendLine($"\tPin result: {Ok(result.IsOk)}");
            sb.AppendLine($"\tMax utilization: η = {FmtEta(result.MaxUtilization)}");
            sb.AppendLine($"\tGoverning check: {result.GoverningCheckName}");
            sb.AppendLine();
        }

        private static void AppendPinChecks(
            StringBuilder sb,
            PinCheckResult result)
        {
            if (result.HasErrors)
            {
                AppendErrorBlock(sb, "\tPin verification", result.Errors);
                return;
            }

            sb.AppendLine("\tSection values");
            sb.AppendLine("\t--------------");
            sb.AppendLine($"\t\tA = π * d² / 4 = {Fmt1(result.Area_mm2)} mm²");
            sb.AppendLine($"\t\tWel = π * d³ / 32 = {Fmt1(result.SectionModulus_mm3)} mm³");
            sb.AppendLine();

            sb.AppendLine("\tPin shear");
            sb.AppendLine("\t---------");
            sb.AppendLine($"\t\tFv,Ed = F_Ed / 2 = {Fmt2(result.FvEd_kN)} kN");
            sb.AppendLine($"\t\tFv,Rd = 0.6 * A * fu,p / gammaM2 = {Fmt2(result.FvRd_kN)} kN");
            sb.AppendLine($"\t\tCheck Fv,Ed <= Fv,Rd: {Ok(result.ShearOk)}  η = {FmtEta(result.ShearUtilization)}");
            sb.AppendLine();

            sb.AppendLine("\tPin bending");
            sb.AppendLine("\t-----------");
            sb.AppendLine($"\t\tM_Ed = {Fmt2(result.MEd_kNmm)} · 10⁻³ kNm");
            sb.AppendLine($"\t\tM_Rd = 1.5 * Wel * fy,p / gammaM0 = {Fmt2(result.MRd_kNmm)} · 10⁻³ kNm");
            sb.AppendLine($"\t\tCheck M_Ed <= M_Rd: {Ok(result.BendingOk)}  η = {FmtEta(result.BendingUtilization)}");
            sb.AppendLine();

            sb.AppendLine("\tPin shear + bending interaction");
            sb.AppendLine("\t-------------------------------");
            sb.AppendLine($"\t\tη = (M_Ed / M_Rd)² + (Fv,Ed / Fv,Rd)² = {FmtEta(result.CombinedUtilization)}");
            sb.AppendLine($"\t\tCheck η <= 1.000: {Ok(result.CombinedOk)}");
            sb.AppendLine();

            if (result.Input.IsReplaceablePin)
            {
                sb.AppendLine("\tReplaceable pin service bending");
                sb.AppendLine("\t-------------------------------");
                sb.AppendLine($"\t\tM_Ed,ser = {Fmt2(result.MEdSer_kNmm)} · 10⁻³ kNm");
                sb.AppendLine($"\t\tM_Rd,ser = 0.8 * Wel * fy,p / gammaM6,ser = {Fmt2(result.MRdSer_kNmm)} · 10⁻³ kNm");
                sb.AppendLine($"\t\tCheck M_Ed,ser <= M_Rd,ser: {Ok(result.ServiceBendingOk)}  η = {FmtEta(result.ServiceBendingUtilization)}");
                sb.AppendLine();
            }
        }

        private static List<CheckItem> GetAllCheckItems(
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult)
        {
            List<CheckItem> items = new();

            if (!lugResult.BasicResult.HasErrors)
                items.AddRange(lugResult.BasicResult.CheckItems);

            if (!lugResult.EcGeometryResult.HasErrors)
                items.AddRange(lugResult.EcGeometryResult.SummaryCheckItems);

            if (!lugResult.BearingResult.HasErrors)
                items.AddRange(lugResult.BearingResult.CheckItems);

            if (lugResult.OuterLugResult.IsActive)
                items.AddRange(lugResult.OuterLugResult.CheckItems);

            if (!pinResult.HasErrors)
                items.AddRange(pinResult.CheckItems);

            return items;
        }

        private static List<string> GetFailedChecks(
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult)
        {
            List<string> failedChecks = new();

            if (lugResult.BasicResult.HasErrors)
            {
                foreach (string error in lugResult.BasicResult.Errors)
                    failedChecks.Add($"Basic input error: {error}");
            }

            if (lugResult.EcGeometryResult.HasErrors)
            {
                foreach (string error in lugResult.EcGeometryResult.Errors)
                    failedChecks.Add($"EC geometry input error: {error}");
            }

            if (lugResult.BearingResult.HasErrors)
            {
                foreach (string error in lugResult.BearingResult.Errors)
                    failedChecks.Add($"Pin-hole bearing input error: {error}");
            }

            if (lugResult.OuterLugResult.IsActive)
            {
                foreach (string error in lugResult.OuterLugResult.BearingResult.Errors)
                    failedChecks.Add($"Outer lug t2 bearing input error: {error}");
                foreach (string error in lugResult.OuterLugResult.GeometryResult.Errors)
                    failedChecks.Add($"Outer lug t2 geometry input error: {error}");
            }

            if (pinResult.HasErrors)
            {
                foreach (string error in pinResult.Errors)
                    failedChecks.Add($"Pin input error: {error}");
            }

            foreach (CheckItem item in GetAllCheckItems(lugResult, pinResult))
            {
                if (!item.IsOk && !item.IsWarning)
                    failedChecks.Add(item.Name);
            }

            return failedChecks.Distinct().ToList();
        }

        private static double GetMaxUtilization(List<CheckItem> items)
        {
            List<CheckItem> utilizationItems = items
                .Where(i => i.ShowUtilization)
                .ToList();

            return utilizationItems.Count == 0
                ? 0.0
                : utilizationItems.Max(i => i.Utilization);
        }

        private static string GetGoverningCheckName(List<CheckItem> items)
        {
            return items
                .Where(i => i.ShowUtilization)
                .OrderByDescending(i => i.Utilization)
                .FirstOrDefault()?.Name ?? "";
        }

        private static string FormatGeometrySummaryLine(CheckItem item)
        {
            string status;

            if (item.IsOk)
                status = "OK";
            else if (item.IsWarning)
                status = "WARNING";
            else
                status = "NOT OK";

            return $"{status}\t{item.Name}";
        }

        private static string FormatUtilizationSummaryLine(CheckItem item)
        {
            string status = item.IsOk ? "OK" : "NOT OK";
            return $"{status}\tη = {FmtEta(item.Utilization)}  {item.Name}";
        }

        private static void AppendErrorBlock(
            StringBuilder sb,
            string title,
            List<string> errors)
        {
            sb.AppendLine(title);
            sb.AppendLine(new string('-', title.Trim().Length));
            sb.AppendLine("\tInput error");
            sb.AppendLine();

            foreach (string error in errors)
                sb.AppendLine($"\t{error}");

            sb.AppendLine();
        }

        private static void AppendSeparator(StringBuilder sb)
        {
            sb.AppendLine("______________________________________________________");
            sb.AppendLine();
        }

        private static string Ok(bool ok)
        {
            return ok ? "OK" : "NOT OK";
        }

        private static string Fmt1(double value)
        {
            return value.ToString("0.0");
        }

        private static string Fmt2(double value)
        {
            return value.ToString("0.00");
        }

        private static string Fmt3(double value)
        {
            return value.ToString("0.000");
        }
        private static string FmtEta(double value)
        {
            return value.ToString("0.000");
        }
    }
}
