using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LascheApp.Padeye
{
    public class PadeyeTensionLugReportInfo
    {
        public string Project { get; set; } = "";
        public string Gantry { get; set; } = "";
        public string Connection { get; set; } = "";
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
            sb.AppendLine($"Gantry:        {info.Gantry}");
            sb.AppendLine($"Connection:    {info.Connection}");
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
            PinCheckInput pin = pinResult.Input;

            double e_mm = ec.EdgeDistanceA_mm + ec.HoleDiameter_mm / 2.0;

            sb.AppendLine("Basic input data");
            sb.AppendLine("----------------");
            sb.AppendLine($"Plate material: {info.PlateMaterial}");
            sb.AppendLine($"Pin material:   {info.PinMaterial}");
            sb.AppendLine();
            sb.AppendLine($"F_Ed     = {Fmt2(basic.F_Ed_kN)} kN");
            sb.AppendLine($"F_Ed,ser = {Fmt2(basic.F_Ed_ser_kN)} kN");
            sb.AppendLine();
            sb.AppendLine($"tpl = {Fmt1(basic.PlateThickness_mm)} mm");

            if (basic.CheekPlateThickness_mm > 0.0)
            {
                sb.AppendLine($"tch = {Fmt1(basic.CheekPlateThickness_mm)} mm");

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
            AppendLugInput(sb, lugResult);
            AppendEcGeometry(sb, lugResult.EcGeometryResult);
            AppendBearing(sb, lugResult.BearingResult);

            sb.AppendLine();
            sb.AppendLine("2.2. Pin");
            sb.AppendLine();

            AppendPinOverall(sb, pinResult);
            AppendPinInput(sb, lugResult, pinResult);
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

        private static void AppendLugInput(
            StringBuilder sb,
            PadeyeCheckResult result)
        {
            PadeyeBasicCheckInput basic = result.BasicResult.Input;

            sb.AppendLine("\tInput");
            sb.AppendLine("\t-----");
            sb.AppendLine($"\tF_Ed = {Fmt2(basic.F_Ed_kN)} kN");
            sb.AppendLine($"\tF_Ed,ser = {Fmt2(basic.F_Ed_ser_kN)} kN");
            sb.AppendLine($"\tfy = {Fmt1(basic.MaterialFy_Nmm2)} N/mm²");
            sb.AppendLine($"\tgammaM0 = {Fmt2(basic.GammaM0)}");
            sb.AppendLine($"\tt = {Fmt1(basic.PlateThickness_mm)} mm");
            sb.AppendLine($"\td0 = {Fmt1(basic.HoleDiameter_mm)} mm");
            sb.AppendLine($"\tb = {Fmt1(basic.PlateWidth_mm)} mm");
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

            double t_mm = input.EffectiveThickness_mm;

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

            sb.AppendLine("\tMöglichkeit A");
            sb.AppendLine("\t-------------");
            sb.AppendLine($"\t\tResult Möglichkeit A: {Ok(result.MoglichkeitA_Ok)}");
            sb.AppendLine($"\t\tMax utilization A: η = {FmtEta(result.MoglichkeitA_MaxUtilization)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tF_Ed = {Fmt2(input.F_Ed_kN)} kN");
            sb.AppendLine($"\t\tfy = {Fmt1(input.Fy_Nmm2)} N/mm²");
            sb.AppendLine($"\t\tgammaM0 = {Fmt2(input.GammaM0)}");
            sb.AppendLine($"\t\tt = {Fmt1(t_mm)} mm");
            sb.AppendLine($"\t\td0 = {Fmt1(input.HoleDiameter_mm)} mm");
            sb.AppendLine();
            sb.AppendLine($"\t\te = {Fmt1(e_mm)} mm");
            sb.AppendLine($"\t\ta = e - d0 / 2 = {Fmt1(input.EdgeDistanceA_mm)} mm");
            sb.AppendLine($"\t\tRequired a = F_Ed * gammaM0 / (2 * t * fy) + 2*d0/3 = {Fmt1(result.RequiredEdgeDistanceA_mm)} mm");
            sb.AppendLine($"\t\tRequired e = required a + d0 / 2 = {Fmt1(requiredE_mm)} mm");
            sb.AppendLine($"\t\tCheck e >= required e: {Ok(result.EdgeDistanceA_Ok)}  η = {FmtEta(result.EdgeDistanceA_Utilization)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tb = {Fmt1(b_mm)} mm");
            sb.AppendLine($"\t\tc = (b - d0) / 2 = {Fmt1(input.SideDistanceC_mm)} mm");
            sb.AppendLine($"\t\tRequired c = F_Ed * gammaM0 / (2 * t * fy) + d0/3 = {Fmt1(result.RequiredSideDistanceC_mm)} mm");
            sb.AppendLine($"\t\tRequired b = 2 * required c + d0 = {Fmt1(requiredB_mm)} mm");
            sb.AppendLine($"\t\tCheck b >= required b: {Ok(result.SideDistanceC_Ok)}  η = {FmtEta(result.SideDistanceC_Utilization)}");
            sb.AppendLine();

            sb.AppendLine("\tMöglichkeit B");
            sb.AppendLine("\t-------------");
            sb.AppendLine($"\t\tResult Möglichkeit B: {Ok(result.MoglichkeitB_Ok)}");
            sb.AppendLine($"\t\tMax utilization B: η = {FmtEta(result.MoglichkeitB_MaxUtilization)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tF_Ed = {Fmt2(input.F_Ed_kN)} kN");
            sb.AppendLine($"\t\tfy = {Fmt1(input.Fy_Nmm2)} N/mm²");
            sb.AppendLine($"\t\tgammaM0 = {Fmt2(input.GammaM0)}");
            sb.AppendLine($"\t\tt = {Fmt1(t_mm)} mm");
            sb.AppendLine($"\t\td0 = {Fmt1(input.HoleDiameter_mm)} mm");
            sb.AppendLine();
            sb.AppendLine($"\t\tFRd = (t / 0.7)² * fy / gammaM0 = {Fmt2(result.ForceResistance_MoglichkeitB_kN)} kN");
            sb.AppendLine($"\t\tCheck F_Ed <= FRd: {Ok(result.ThicknessMoglichkeitB_Ok)}  η = {FmtEta(result.ForceMoglichkeitB_Utilization)}");
            sb.AppendLine();
            sb.AppendLine($"\t\tMax d0 = 2.5 * t = {Fmt1(result.MaxHoleDiameter_MoglichkeitB_mm)} mm");
            sb.AppendLine($"\t\tCheck d0 <= 2.5 * t: {Ok(result.HoleDiameterMoglichkeitB_Ok)}");
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
            sb.AppendLine($"\t\tFb,Ed = {Fmt2(input.F_Ed_kN)} kN");
            sb.AppendLine($"\t\tt = {Fmt1(input.PlateThickness_mm)} mm");
            sb.AppendLine($"\t\td = {Fmt1(input.PinDiameter_mm)} mm");
            sb.AppendLine($"\t\tfy = {Fmt1(input.Fy_Nmm2)} N/mm²");
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

                sb.AppendLine("\tContact stress (Replaceable pin)");
                sb.AppendLine("\t--------------------------------");
                sb.AppendLine($"\t\td0 = {Fmt1(input.HoleDiameter_mm)} mm");
                sb.AppendLine($"\t\td = {Fmt1(input.PinDiameter_mm)} mm");
                sb.AppendLine($"\t\tE = {Fmt1(input.E_Nmm2)} N/mm²");
                sb.AppendLine($"\t\tsigma_h,Ed = 0.591 * sqrt(E * Fb,Ed,ser * (d0 - d) / (d² * t)) = {Fmt1(result.SigmaHEd_Nmm2)} N/mm²");
                sb.AppendLine($"\t\tfh,Rd = 2.5 * fy / gammaM6,ser = {Fmt1(result.FhRd_Nmm2)} N/mm²");
                sb.AppendLine($"\t\tCheck sigma_h,Ed <= fh,Rd: {Ok(result.HolePinStressOk)}  η = {FmtEta(result.HolePinStressUtilization)}");
                sb.AppendLine();
            }
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

        private static void AppendPinInput(
            StringBuilder sb,
            PadeyeCheckResult lugResult,
            PinCheckResult pinResult)
        {
            PadeyeBasicCheckInput basic = lugResult.BasicResult.Input;
            PinCheckInput input = pinResult.Input;
            

            sb.AppendLine("\tInput");
            sb.AppendLine("\t-----");
            sb.AppendLine($"\tF_Ed = {Fmt2(input.F_Ed_kN)} kN");
            sb.AppendLine($"\tF_Ed,ser = {Fmt2(input.F_Ed_ser_kN)} kN");

            if (input.MomentCalculatedFromTensionLugGeometry)
            {
                sb.AppendLine("\tEffective inner lug thickness for pin bending:");

                if (basic.CheekPlateThickness_mm > 0.0)
                    sb.AppendLine($"\tt = tpl + 2 * tch = {Fmt1(input.InnerLugThicknessT_mm)} mm");
                else
                    sb.AppendLine($"\tt = tpl = {Fmt1(input.InnerLugThicknessT_mm)} mm");
                sb.AppendLine($"\tt2 = {Fmt1(input.OuterLugThicknessT2_mm)} mm");
                sb.AppendLine($"\ts = {Fmt1(input.GapS_mm)} mm");
                sb.AppendLine($"\tM_Ed = F_Ed / 8 * (t2 + 4 * s + 2 * t) = {Fmt2(input.M_Ed_kNmm)} · 10⁻³ kNm");
                sb.AppendLine($"\tM_Ed,ser = M_Ed * F_Ed,ser / F_Ed = {Fmt2(input.M_Ed_ser_kNmm)} · 10⁻³ kNm");
            }
            else
            {
                sb.AppendLine($"\tM_Ed = {Fmt2(input.M_Ed_kNmm)} · 10⁻³ kNm");
                sb.AppendLine($"\tM_Ed,ser = {Fmt2(input.M_Ed_ser_kNmm)} · 10⁻³ kNm");
            }

            sb.AppendLine($"\tPin diameter d = {Fmt1(input.PinDiameter_mm)} mm");
            sb.AppendLine($"\tfy,p = {Fmt1(input.PinFy_Nmm2)} N/mm²");
            sb.AppendLine($"\tfu,p = {Fmt1(input.PinFu_Nmm2)} N/mm²");
            sb.AppendLine($"\tgammaM0 = {Fmt2(input.GammaM0)}");
            sb.AppendLine($"\tgammaM2 = {Fmt2(input.GammaM2)}");
            sb.AppendLine($"\tgammaM6,ser = {Fmt2(input.GammaM6_ser)}");
            sb.AppendLine($"\tReplaceable pin checks: {(input.IsReplaceablePin ? "active" : "not active")}");
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

            return $"{status,-15} {item.Name}";
        }

        private static string FormatUtilizationSummaryLine(CheckItem item)
        {
            string status = item.IsOk ? "OK" : "NOT OK";
            return $"{status,-7} η = {FmtEta(item.Utilization)}  {item.Name}";
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

        private static string FmtEta(double value)
        {
            return value.ToString("0.000");
        }
    }
}