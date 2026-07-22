using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LascheApp.Padeye
{
    public class PadeyeTransportLugReportInfo
    {
        public string Project { get; set; } = "";
        public string Subject { get; set; } = "";
        public string PreparedBy { get; set; } = "";
        public DateTime Date { get; set; } = DateTime.Today;

        public string PlateMaterial { get; set; } = "";
        public string ShackleName { get; set; } = "";
    }

    public static class PadeyeTransportLugReportFormatter
    {
        public static string Format(
            PadeyeCheckResult result,
            PadeyeTransportLugReportInfo info)
        {
            StringBuilder sb = new();

            AppendProjectInformation(sb, info);
            AppendSeparator(sb);

            AppendHeader(sb, result);
            AppendBasicInputData(sb, result, info);
            AppendSeparator(sb);

            AppendCheckSummary(sb, result);
            AppendSeparator(sb);

            AppendCalculation(sb, result);

            return sb.ToString().TrimEnd();
        }

        private static void AppendProjectInformation(
            StringBuilder sb,
            PadeyeTransportLugReportInfo info)
        {
            sb.AppendLine("Project information");
            sb.AppendLine("===================");
            sb.AppendLine();
            sb.AppendLine($"Project:       {info.Project}");
            sb.AppendLine($"Subject:       {info.Subject}");
            sb.AppendLine($"Prepared by:   {info.PreparedBy}");
            sb.AppendLine($"Date:          {info.Date:yyyy-MM-dd}");
        }

        private static void AppendHeader(StringBuilder sb, PadeyeCheckResult result)
        {
            sb.AppendLine("Transport Lug verification (DIN EN 1993-1-8 / DNV)");
            sb.AppendLine("===================================================");
            sb.AppendLine();
            sb.AppendLine($"Overall result: {Ok(result.IsOk)}");
            sb.AppendLine($"Max utilization: η = {Eta(result.MaxUtilization)}");
            sb.AppendLine($"Governing utilization: {result.GoverningCheckName}");

            List<string> failedChecks = GetFailedChecks(result);

            if (failedChecks.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Failed check(s):");

                foreach (string failedCheck in failedChecks)
                    sb.AppendLine($"- {failedCheck}");
            }

            sb.AppendLine();
        }
        private static List<string> GetFailedChecks(PadeyeCheckResult result)
        {
            List<string> failedChecks = new();

            if (result.BasicResult.HasErrors)
            {
                foreach (string error in result.BasicResult.Errors)
                    failedChecks.Add($"Basic input error: {error}");
            }
            else
            {
                PadeyeBasicCheckResult basic = result.BasicResult;

                if (!basic.WllOk)
                    failedChecks.Add("Shackle WLL not sufficient");

                if (!basic.CheekPlateThicknessLimitOk)
                    failedChecks.Add("Cheek plate thickness limit not fulfilled: tch <= tpl / 2");

                // Deliberately not included:
                // - hole diameter clearance recommendation
                // - shackle B1 thickness recommendation
                // These are warnings only.
            }

            if (result.EcGeometryResult.HasErrors)
            {
                foreach (string error in result.EcGeometryResult.Errors)
                    failedChecks.Add($"EC geometry input error: {error}");
            }
            else if (!result.EcGeometryResult.IsOk)
            {
                failedChecks.Add("EC geometry check not fulfilled: neither Method A nor Method B is OK");
            }

            if (result.BearingResult.HasErrors)
            {
                foreach (string error in result.BearingResult.Errors)
                    failedChecks.Add($"Pin-hole bearing input error: {error}");
            }
            else
            {
                PadeyeBearingResult bearing = result.BearingResult;

                if (!bearing.BearingDesignOk)
                    failedChecks.Add("Pin-hole bearing design not fulfilled");

                if (!bearing.PinHoleGeometryOk)
                    failedChecks.Add("Pin-hole geometry not fulfilled: pin diameter d must be smaller than hole diameter d0");

                if (bearing.Input.IsReplaceablePin && !bearing.BearingServiceOk)
                    failedChecks.Add("Replaceable pin service bearing not fulfilled");

                if (bearing.Input.IsReplaceablePin && !bearing.HolePinStressOk)
                    failedChecks.Add("Replaceable pin contact stress not fulfilled");
            }

            if (result.DnvOutOfPlaneCheckRequired)
            {
                if (result.DnvOutOfPlaneResult.HasErrors)
                {
                    foreach (string error in result.DnvOutOfPlaneResult.Errors)
                        failedChecks.Add($"DNV input error: {error}");
                }
                else
                {
                    PadeyeDnvOutOfPlaneResult dnv = result.DnvOutOfPlaneResult;

                    if (dnv.OutOfPlaneChecksActive && !dnv.DnvBearingOk)
                        failedChecks.Add("DNV bearing pressure for angled pull not fulfilled");

                    if (dnv.OutOfPlaneChecksActive && !dnv.TearOutOk)
                        failedChecks.Add("Tear-out by angled pull according to DNV standard not fulfilled");

                    if (dnv.CheekPlateWeldCheckActive && !dnv.CheekPlateWeldOk)
                        failedChecks.Add("Cheek plate weld not fulfilled");
                }
            }

            if (!result.IsOk && failedChecks.Count == 0)
            {
                failedChecks.Add("Internal result is NOT OK, but no failed check was reported. This indicates a report/check logic mismatch.");
            }

            return failedChecks;
        }
        private static void AppendBasicInputData(
            StringBuilder sb,
            PadeyeCheckResult result,
            PadeyeTransportLugReportInfo info)
        {
            PadeyeBasicCheckInput basic = result.BasicResult.Input;
            PadeyeEcGeometryInput geometry = result.EcGeometryResult.Input;
            PadeyeBearingInput bearing = result.BearingResult.Input;
            PadeyeDnvOutOfPlaneInput dnv = result.DnvOutOfPlaneResult.Input;
            double e_mm = geometry.EdgeDistanceA_mm + geometry.HoleDiameter_mm / 2.0;

            sb.AppendLine("Input data");
            sb.AppendLine("==========");
            sb.AppendLine();
            sb.AppendLine("Materials");
            sb.AppendLine("---------");
            sb.AppendLine($"E = {Fmt1(dnv.E_Nmm2)} N/mm²");
            sb.AppendLine($"- Lug: {info.PlateMaterial} (fy = {Fmt1(basic.MaterialFy_Nmm2)} N/mm²; fu = {Fmt1(dnv.Fu_Nmm2)} N/mm²; betaW = {Fmt3(dnv.BetaW)})");
            sb.AppendLine($"- Shackle: {info.ShackleName}");
            sb.AppendLine();

            sb.AppendLine("Loads");
            sb.AppendLine("-----");
            sb.AppendLine($"FEd,ser = {Fmt2(basic.F_Ed_ser_kN)} kN");
            sb.AppendLine($"FEd     = {Fmt2(basic.F_Ed_kN)} kN");
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
            }
            sb.AppendLine(basic.IncludeCheekPlatesInBearing
                ? $"t = tpl + 2 * tch = {Fmt1(basic.TotalBearingThickness_mm)} mm"
                : $"t = tpl = {Fmt1(basic.TotalBearingThickness_mm)} mm");
            sb.AppendLine($"e = {Fmt1(e_mm)} mm");
            sb.AppendLine($"b = {Fmt1(basic.PlateWidth_mm)} mm");
            if (basic.CheekPlateThickness_mm > 0.0)
            {
                sb.AppendLine($"Rch = {Fmt1(dnv.Rch_mm)} mm");
                sb.AppendLine($"a_weld = {Fmt1(dnv.WeldA_mm)} mm");
            }
            sb.AppendLine($"d0 = {Fmt1(basic.HoleDiameter_mm)} mm");
            sb.AppendLine();
            sb.AppendLine($"WLL = {Fmt2(basic.ShackleWLL_kN)} kN");
            sb.AppendLine($"Dpin = {Fmt1(basic.ShackleDpin_mm)} mm");
            sb.AppendLine($"B1 = {Fmt1(basic.ShackleB1_mm)} mm");
            sb.AppendLine($"H_DNV = {Fmt1(basic.ShackleH_DNV_mm)} mm");
            sb.AppendLine();
            sb.AppendLine($"alpha = {Fmt1(dnv.Alpha_deg)}°");
            sb.AppendLine($"beta = {Fmt3(dnv.Beta)}");
            sb.AppendLine();
            sb.AppendLine($"Replaceable pin checks: {(bearing.IsReplaceablePin ? "active" : "not active")}");
            sb.AppendLine();
        }

        private static void AppendCheckSummary(StringBuilder sb, PadeyeCheckResult result)
        {
            List<CheckItem> allItems = result.GoverningCheckItems;

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

        private static void AppendCalculation(StringBuilder sb, PadeyeCheckResult result)
        {
            sb.AppendLine("2. Calculation");
            sb.AppendLine("==============");
            sb.AppendLine();
            sb.AppendLine("2.1. Lug");
            sb.AppendLine();

            AppendLugOverall(sb, result);
            AppendBasicPadeyeChecks(
    sb,
    result.BasicResult,
    result.DnvOutOfPlaneResult);
            AppendEcGeometry(sb, result.EcGeometryResult);
            AppendBearing(sb, result.BearingResult);
            AppendCheekPlateWeld(sb, result.DnvOutOfPlaneResult);
            AppendDnvBearingAndTearOut(sb, result.DnvOutOfPlaneResult);
        }

        private static void AppendLugOverall(StringBuilder sb, PadeyeCheckResult result)
        {
            sb.AppendLine($"\tLug result: {(result.IsOk ? "OK" : "NOT OK")}");
            sb.AppendLine($"\tMax utilization: η = {FmtEta(result.MaxUtilization)}");
            sb.AppendLine($"\tGoverning check: {result.GoverningCheckName}");
            sb.AppendLine();
        }

        private static void AppendBasicPadeyeChecks(
            StringBuilder sb,
            PadeyeBasicCheckResult result,
            PadeyeDnvOutOfPlaneResult dnvResult)
        {
            PadeyeBasicCheckInput input = result.Input;

            if (result.HasErrors)
            {
                AppendErrorBlock(sb, "\tBasic padeye checks", result.Errors);
                return;
            }

            sb.AppendLine("\tShackle WLL");
            sb.AppendLine("\t-----------");
            sb.AppendLine($"\tF_Ed,ser = {Fmt2(input.F_Ed_ser_kN)} kN");
            sb.AppendLine($"\tWLL = {Fmt2(input.ShackleWLL_kN)} kN");
            sb.AppendLine($"\tCheck F_Ed,ser <= WLL: {Ok(result.WllOk)}  η = {FmtEta(result.WllUtilization)}");
            sb.AppendLine();

            sb.AppendLine("\tHole clearance");
            sb.AppendLine("\t--------------");
            sb.AppendLine($"\tDpin = {Fmt1(input.ShackleDpin_mm)} mm");
            sb.AppendLine($"\td0 = {Fmt1(input.HoleDiameter_mm)} mm");
            sb.AppendLine($"\tRecommended d0 = Dpin + {Fmt1(input.PinClearance_mm)} mm = {Fmt1(result.RequiredHoleDiameter_mm)} mm");
            sb.AppendLine($"\tCheck d0 >= recommended d0: {Ok(result.HoleDiameterOk)}");
            sb.AppendLine();

            if (dnvResult.PinDiameterRecommendationActive)
            {
                sb.AppendLine("\tPin diameter recommendation for significant angled pull");
                sb.AppendLine("\t-------------------------------------------------------");
                sb.AppendLine($"\tDpin / d0 = {Fmt3(dnvResult.DpinToHoleRatio)}");
                sb.AppendLine("\tRecommended according to DNV-OS-H205: Dpin / d0 >= 0.94");
                sb.AppendLine($"\tRecommendation fulfilled: {Ok(dnvResult.PinDiameterRecommendationOk)}");
                sb.AppendLine("\tThis is a recommendation only and does not govern the overall result.");
                sb.AppendLine();
            }

            sb.AppendLine("\tShackle B1 thickness recommendation");
            sb.AppendLine("\t-----------------------------------");
            sb.AppendLine($"\tB1 = {Fmt1(input.ShackleB1_mm)} mm");

            if (input.IncludeCheekPlatesInBearing)
            {
                sb.AppendLine($"\ttpl = {Fmt1(input.PlateThickness_mm)} mm");
                sb.AppendLine($"\ttch = {Fmt1(input.CheekPlateThickness_mm)} mm");
                sb.AppendLine($"\tt = tpl + 2 * tch = {Fmt1(result.ThicknessForB1Check_mm)} mm");
                sb.AppendLine($"\tRecommended t = 0.75 * B1 = {Fmt1(result.RequiredThickness_mm)} mm");
                sb.AppendLine($"\tCheck t_total >= 0.75 * B1: {Ok(result.ThicknessOk)}");
            }
            else
            {
                sb.AppendLine($"\ttpl = {Fmt1(input.PlateThickness_mm)} mm");
                sb.AppendLine($"\tt = tpl = {Fmt1(result.ThicknessForB1Check_mm)} mm");

                if (input.IncludeCheekPlatesInBearingRequested &&
                    !input.IncludeCheekPlatesInBearing)
                {
                    sb.AppendLine();
                    sb.AppendLine("\tWARNING: Cheek plates were requested, but cannot be included in the effective thickness.");
                    sb.AppendLine($"\tRch = {Fmt1(input.CheekPlateRadiusRch_mm)} mm < required e_min = {Fmt1(input.CheekPlateBearingRequiredE_mm)} mm");
                    sb.AppendLine("\tTherefore the B1 thickness recommendation is checked with t = tpl only.");
                }

                sb.AppendLine($"\tRecommended t = 0.75 * B1 = {Fmt1(result.RequiredThickness_mm)} mm");
                sb.AppendLine($"\tCheck t >= 0.75 * B1: {Ok(result.ThicknessOk)}");
            }

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

        private static void AppendCheekPlateWeld(
            StringBuilder sb,
            PadeyeDnvOutOfPlaneResult result)
        {
            if (!result.IsActive)
                return;

            PadeyeDnvOutOfPlaneInput input = result.Input;

            if (result.HasErrors)
            {
                AppendErrorBlock(sb, "\tCheek plate weld / DNV input", result.Errors);
                return;
            }

            if (!result.CheekPlateWeldCheckActive)
                return;

            string deltaNote = Math.Abs(input.Alpha_deg) > 1e-9
                ? " (out-of-plane load factor)"
                : " (straight tension)";

            sb.AppendLine("\tCheek plate weld");
            sb.AppendLine("\t----------------");
            sb.AppendLine($"\t\tFd = F_Ed * cos(alpha) = {Fmt2(result.Fd_kN)} kN");
            sb.AppendLine($"\t\talpha = {Fmt1(input.Alpha_deg)}°");
            sb.AppendLine($"\t\ttch = {Fmt1(input.CheekPlateThickness_mm)} mm");
            sb.AppendLine($"\t\tt = tpl + 2 * tch = {Fmt1(input.TotalThickness_mm)} mm");
            sb.AppendLine($"\t\tDCH = 2 * Rch = {Fmt1(result.Dch_mm)} mm");
            sb.AppendLine($"\t\ta = {Fmt1(input.WeldA_mm)} mm");
            sb.AppendLine($"\t\tdelta = {Fmt3(result.Delta)}{deltaNote}");
            sb.AppendLine();

            sb.AppendLine("\t\tDemand according to DNV-OS-H205 B.2.6:");
            sb.AppendLine($"\t\tsigma_Ed,w = Fd * tch / (1.5 * t * DCH * a) * delta = {Fmt1(result.SigmaEd3_Nmm2)} N/mm²");
            sb.AppendLine();

            sb.AppendLine("\t\tWeld resistance according to EN 1993-1-8:");
            sb.AppendLine($"\t\tfvw,Rd = fu / (sqrt(3) * betaW * gammaM2) = {Fmt1(result.Fvwd_Nmm2)} N/mm²");
            sb.AppendLine();

            sb.AppendLine($"\t\tCheck sigma_Ed,w <= fvw,Rd: {Ok(result.CheekPlateWeldOk)}  η = {FmtEta(result.CheekPlateWeldUtilization)}");
            sb.AppendLine();
        }

        private static void AppendDnvBearingAndTearOut(
            StringBuilder sb,
            PadeyeDnvOutOfPlaneResult result)
        {
            if (!result.IsActive || result.HasErrors || !result.OutOfPlaneChecksActive)
                return;

            PadeyeDnvOutOfPlaneInput input = result.Input;

            string branch = result.DpinToHoleRatio < 0.96
                ? "Dpin / d0 < 0.96"
                : "Dpin / d0 >= 0.96";

            string sigmaEd1Formula = result.BearingFormulaWithClearance
                ? "sigma_Ed,1 = 0.18 * sqrt(Fd * (1 / Dpin - 1 / d0) * E * beta,eff / t)"
                : "sigma_Ed,1 = 0.036 * sqrt(Fd * E * beta,eff / (d0 * t))";

            sb.AppendLine("\tBearing pressure - only for angled pull");
            sb.AppendLine("\t---------------------------------------");
            sb.AppendLine($"\t\tFds = {Fmt2(input.F_Ed_kN)} kN");
            sb.AppendLine($"\t\talpha = {Fmt1(input.Alpha_deg)}°");
            sb.AppendLine($"\t\tFd = Fds * cos(alpha) = {Fmt2(result.Fd_kN)} kN");
            sb.AppendLine($"\t\tFdl = Fds * sin(alpha) = {Fmt2(result.Fdl_kN)} kN");
            sb.AppendLine($"\t\th = {Fmt1(input.H_DNV_mm)} mm");
            sb.AppendLine($"\t\tMe = Fdl * h = {Fmt2(result.Me_kNm)} kNm");
            sb.AppendLine();
            sb.AppendLine($"\t\tDpin / d0 = {Fmt3(result.DpinToHoleRatio)}");
            sb.AppendLine($"\t\tFormula branch: {branch}");
            sb.AppendLine($"\t\tdelta = 4 * tan(alpha) * h / t + 1 = {Fmt3(result.Delta)}");
            sb.AppendLine($"\t\tbeta,eff = {(result.Delta >= 1.3 ? "beta * (delta - 0.3)" : "beta")} = {Fmt3(result.BetaEffective)}");
            sb.AppendLine($"\t\tsigma_Rd = fy / gammaM = {Fmt1(result.SigmaRd_Nmm2)} N/mm²");
            sb.AppendLine($"\t\t{sigmaEd1Formula}");
            sb.AppendLine($"\t\tsigma_Ed,1 = {Fmt1(result.SigmaEd1_Nmm2)} N/mm²");
            sb.AppendLine($"\t\tCheck sigma_Ed,1 <= sigma_Rd: {Ok(result.DnvBearingOk)}  η = {FmtEta(result.DnvBearingUtilization)}");
            sb.AppendLine();

            sb.AppendLine("\tTear out - only for angled pull");
            sb.AppendLine("\t-------------------------------");
            sb.AppendLine($"\t\te = {Fmt1(input.EndDistanceE_mm)} mm");
            sb.AppendLine($"\t\td0 = {Fmt1(input.HoleDiameter_mm)} mm");
            sb.AppendLine($"\t\tt = {Fmt1(input.TotalThickness_mm)} mm");
            sb.AppendLine($"\t\tsigma_Ed,2 = 1.7 * Fd / ((2 * e - d0) * t) = {Fmt1(result.SigmaEd2_Nmm2)} N/mm²");
            sb.AppendLine($"\t\tCheck sigma_Ed,2 <= sigma_Rd: {Ok(result.TearOutOk)}  η = {FmtEta(result.TearOutUtilization)}");
            sb.AppendLine();
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

        private static string FormatGeometrySummaryLine(CheckItem item)
        {
            string status;

            if (item.IsOk)
                status = "OK";
            else if (
                item.IsWarning ||
                item.Name.Contains("clearance", StringComparison.OrdinalIgnoreCase) ||
                item.Name.Contains("recommendation", StringComparison.OrdinalIgnoreCase))
                status = "WARNING";
            else
                status = "NOT OK";

            return $"{status}\t{item.Name}";
        }
        private static string Eta(double value)
        {
            return value.ToString("0.000");
        }
        private static string FormatUtilizationSummaryLine(CheckItem item)
        {
            string status = item.IsOk ? "OK" : "NOT OK";
            return $"{status}\tη = {FmtEta(item.Utilization)}  {item.Name}";
        }

        private static string Ok(bool ok)
        {
            return ok ? "OK" : "NOT OK";
        }

        private static string Fmt1(double value)
        {
            return ReportNumberFormatter.Format(value, 1);
        }

        private static string Fmt2(double value)
        {
            return ReportNumberFormatter.Format(value, 2);
        }

        private static string Fmt3(double value)
        {
            return ReportNumberFormatter.Format(value, 3);
        }

        private static string FmtEta(double value)
        {
            return ReportNumberFormatter.Format(value, 3);
        }
    }
}
