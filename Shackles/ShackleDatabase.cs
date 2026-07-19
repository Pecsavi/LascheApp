using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LascheApp.Shackles
{
    public class ShackleDatabase
    {
        private readonly string _filePath;

        public List<ShackleData> Shackles { get; private set; } = new();

        public ShackleDatabase(string filePath)
        {
            _filePath = filePath;
        }

        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                Shackles = CreateDefaultShackles();
                Save();
                return;
            }

            string json = File.ReadAllText(_filePath);

            Shackles = JsonSerializer.Deserialize<List<ShackleData>>(json, JsonOptions())
                       ?? new List<ShackleData>();
        }

        public void Save()
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(Shackles, JsonOptions());
            File.WriteAllText(_filePath, json);
        }

        public ShackleData GetById(string shackleId)
        {
            ShackleData? shackle = Shackles.FirstOrDefault(s => s.Id == shackleId);

            if (shackle == null)
                throw new InvalidOperationException($"Shackle with Id '{shackleId}' not found.");

            return shackle;
        }

        public ShackleData? TryGetById(string shackleId)
        {
            return Shackles.FirstOrDefault(s => s.Id == shackleId);
        }

        public void AddOrUpdate(ShackleData shackle)
        {
            if (string.IsNullOrWhiteSpace(shackle.Id))
                throw new ArgumentException("Shackle Id must not be empty.");

            int index = Shackles.FindIndex(s => s.Id == shackle.Id);

            if (index >= 0)
                Shackles[index] = shackle;
            else
                Shackles.Add(shackle);

            Save();
        }

        public void Delete(string shackleId)
        {
            ShackleData? shackle = TryGetById(shackleId);

            if (shackle == null)
                return;

            Shackles.Remove(shackle);
            Save();
        }

        public void ReplaceAll(IEnumerable<ShackleData> shackles)
        {
            List<ShackleData> replacement = shackles.ToList();

            if (replacement.Any(s => string.IsNullOrWhiteSpace(s.Id)))
                throw new ArgumentException("Every shackle must have an Id.");

            if (replacement.GroupBy(s => s.Id, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1))
                throw new ArgumentException("Shackle Id values must be unique.");

            Shackles = replacement;
            Save();
        }

        public void RestoreDefaults()
        {
            Shackles = CreateDefaultShackles();
            Save();
        }

        /// <summary>
        /// Returns the smallest shackle with WLL_kN >= requiredLoad_kN.
        /// </summary>
        public ShackleData? GetSmallestSuitableByWll(double requiredLoad_kN)
        {
            return Shackles
                .Where(s => s.WLL_kN >= requiredLoad_kN)
                .OrderBy(s => s.WLL_kN)
                .FirstOrDefault();
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        private static List<ShackleData> CreateDefaultShackles()
        {
            return new List<ShackleData>
            {
                CreateHC2(
                    nominalSize: "1/2",
                    wll_kg: 2000,
                    d1_mm: 13,
                    dpin_mm: 16,
                    d3_mm: 30,
                    d4_inch: "5/8",
                    b1_mm: 21,
                    b2_mm: 33,
                    h2_mm: 48),

                CreateHC2(
                    nominalSize: "5/8",
                    wll_kg: 3250,
                    d1_mm: 16,
                    dpin_mm: 19,
                    d3_mm: 40,
                    d4_inch: "3/4",
                    b1_mm: 27,
                    b2_mm: 43,
                    h2_mm: 60),

                CreateHC2(
                    nominalSize: "3/4",
                    wll_kg: 4750,
                    d1_mm: 19,
                    dpin_mm: 22,
                    d3_mm: 48,
                    d4_inch: "7/8",
                    b1_mm: 32,
                    b2_mm: 51,
                    h2_mm: 71),

                CreateHC2(
                    nominalSize: "7/8",
                    wll_kg: 6500,
                    d1_mm: 22,
                    dpin_mm: 25,
                    d3_mm: 54,
                    d4_inch: "1",
                    b1_mm: 36,
                    b2_mm: 58,
                    h2_mm: 84),

                CreateHC2(
                    nominalSize: "1",
                    wll_kg: 8500,
                    d1_mm: 25,
                    dpin_mm: 29,
                    d3_mm: 60,
                    d4_inch: "1 1/8",
                    b1_mm: 43,
                    b2_mm: 68,
                    h2_mm: 95),

                CreateHC2(
                    nominalSize: "1 1/8",
                    wll_kg: 9500,
                    d1_mm: 29,
                    dpin_mm: 32,
                    d3_mm: 67,
                    d4_inch: "1 1/4",
                    b1_mm: 46,
                    b2_mm: 74,
                    h2_mm: 108),

                CreateHC2(
                    nominalSize: "1 1/4",
                    wll_kg: 12000,
                    d1_mm: 32,
                    dpin_mm: 35,
                    d3_mm: 76,
                    d4_inch: "1 3/8",
                    b1_mm: 52,
                    b2_mm: 82,
                    h2_mm: 119),

                CreateHC2(
                    nominalSize: "1 3/8",
                    wll_kg: 13500,
                    d1_mm: 35,
                    dpin_mm: 38,
                    d3_mm: 84,
                    d4_inch: "1 1/2",
                    b1_mm: 57,
                    b2_mm: 92,
                    h2_mm: 133),

                CreateHC2(
                    nominalSize: "1 1/2",
                    wll_kg: 17000,
                    d1_mm: 38,
                    dpin_mm: 41,
                    d3_mm: 92,
                    d4_inch: "1 5/8",
                    b1_mm: 60,
                    b2_mm: 98,
                    h2_mm: 146),

                CreateHC2(
                    nominalSize: "1 3/4",
                    wll_kg: 25000,
                    d1_mm: 44,
                    dpin_mm: 51,
                    d3_mm: 110,
                    d4_inch: "2",
                    b1_mm: 73,
                    b2_mm: 127,
                    h2_mm: 178),

                CreateHC2(
                    nominalSize: "2",
                    wll_kg: 35000,
                    d1_mm: 51,
                    dpin_mm: 57,
                    d3_mm: 127,
                    d4_inch: "2 1/4",
                    b1_mm: 83,
                    b2_mm: 146,
                    h2_mm: 197),

                CreateHC2(
                    nominalSize: "2 1/2",
                    wll_kg: 55000,
                    d1_mm: 63,
                    dpin_mm: 70,
                    d3_mm: 152,
                    d4_inch: "2 3/4",
                    b1_mm: 105,
                    b2_mm: 184,
                    h2_mm: 267),

                CreateHC2(
                    nominalSize: "3",
                    wll_kg: 85000,
                    d1_mm: 76,
                    dpin_mm: 82,
                    d3_mm: 165,
                    d4_inch: "3 1/4",
                    b1_mm: 127,
                    b2_mm: 200,
                    h2_mm: 330),

                CreateHC2(
                    nominalSize: "3 1/2",
                    wll_kg: 120000,
                    d1_mm: 89,
                    dpin_mm: 95,
                    d3_mm: 203,
                    d4_inch: "3 3/4",
                    b1_mm: 146,
                    b2_mm: 230,
                    h2_mm: 381),

                CreateHC2(
                    nominalSize: "4",
                    wll_kg: 150000,
                    d1_mm: 102,
                    dpin_mm: 108,
                    d3_mm: 229,
                    d4_inch: "4 1/4",
                    b1_mm: 165,
                    b2_mm: 260,
                    h2_mm: 432),

                CreateHC2(
                    nominalSize: "4 3/8",
                    wll_kg: 175000,
                    d1_mm: 111,
                    dpin_mm: 130,
                    d3_mm: 262,
                    d4_inch: "5 1/8",
                    b1_mm: 184,
                    b2_mm: 290,
                    h2_mm: 464)
            };
        }

        private static ShackleData CreateHC2(
            string nominalSize,
            double wll_kg,
            double d1_mm,
            double dpin_mm,
            double d3_mm,
            string d4_inch,
            double b1_mm,
            double b2_mm,
            double h2_mm)
        {
            return new ShackleData
            {
                Id = CreateHC2Id(wll_kg),
                Manufacturer = "",
                Name = $"HC 2 - {wll_kg / 1000.0:0.###} t",
                Type = "Bow shackle with pin, nut and cotter pin",
                Standard = "Hochfeste Schaekel Typ HC 2",
                Comment = "Catalogue data.",

                NominalSize = nominalSize,
                WLL_kg = wll_kg,

                D1_mm = d1_mm,
                Dpin_mm = dpin_mm,
                D3_mm = d3_mm,
                D4_inch = d4_inch,

                B1_mm = b1_mm,
                B2_mm = b2_mm,
                H2_mm = h2_mm,

                PinMaterialId = ""
            };
        }

        private static string CreateHC2Id(double wll_kg)
        {
            return $"HC2_WLL_{wll_kg:0}KG";
        }
    }
}
