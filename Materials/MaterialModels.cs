using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace LascheApp.Materials
{
    public class MaterialGrade
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Standard { get; set; } = "";
        public string Comment { get; set; } = "";

        /// <summary>
        /// Elastic modulus in N/mm².
        /// Typical structural steel: 210000 N/mm².
        /// </summary>
        public double E_Nmm2 { get; set; } = 210000.0;

        /// <summary>
        /// EC3 weld correlation factor βw.
        /// Used for fillet weld resistance.
        /// </summary>
        public double BetaW { get; set; } = 0.9;

        /// <summary>
        /// Thickness-dependent fy/fu values.
        /// Units: thickness in mm, stresses in N/mm².
        /// </summary>
        public List<MaterialThicknessRange> Ranges { get; set; } = new();

        public MaterialThicknessRange? GetRange(double thickness_mm)
        {
            return Ranges.FirstOrDefault(r =>
                thickness_mm >= r.ThicknessMin_mm &&
                thickness_mm < r.ThicknessMax_mm);
        }

        public double GetFy(double thickness_mm)
        {
            MaterialThicknessRange? range = GetRange(thickness_mm);

            if (range == null)
                throw new InvalidOperationException(
                    $"No thickness range found for material '{Name}' and thickness {thickness_mm} mm.");

            if (!range.Fy_Nmm2.HasValue)
                throw new InvalidOperationException(
                    $"No fy value defined for material '{Name}' and thickness {thickness_mm} mm.");

            return range.Fy_Nmm2.Value;
        }

        public double GetFu(double thickness_mm)
        {
            MaterialThicknessRange? range = GetRange(thickness_mm);

            if (range == null)
                throw new InvalidOperationException(
                    $"No thickness range found for material '{Name}' and thickness {thickness_mm} mm.");

            if (!range.Fu_Nmm2.HasValue)
                throw new InvalidOperationException(
                    $"No fu value defined for material '{Name}' and thickness {thickness_mm} mm.");

            return range.Fu_Nmm2.Value;
        }
    }

    public class MaterialThicknessRange
    {
        public double ThicknessMin_mm { get; set; }
        public double ThicknessMax_mm { get; set; }

        public double? Fy_Nmm2 { get; set; }
        public double? Fu_Nmm2 { get; set; }
    }

    public class MaterialPropertiesAtThickness
    {
        public string MaterialId { get; set; } = "";
        public string MaterialName { get; set; } = "";
        public string Standard { get; set; } = "";

        public double Thickness_mm { get; set; }

        public double Fy_Nmm2 { get; set; }
        public double Fu_Nmm2 { get; set; }
        public double E_Nmm2 { get; set; }
        public double BetaW { get; set; }
    }
}