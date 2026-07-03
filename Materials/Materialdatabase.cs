using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;

namespace LascheApp.Materials
{
    public class MaterialDatabase
    {
        private readonly string _filePath;

        public List<MaterialGrade> Materials { get; private set; } = new();

        public MaterialDatabase(string filePath)
        {
            _filePath = filePath;
        }

        public void Load()
        {
            if (!File.Exists(_filePath))
            {
                Materials = CreateDefaultMaterials();
                Save();
                return;
            }

            string json = File.ReadAllText(_filePath);

            Materials = JsonSerializer.Deserialize<List<MaterialGrade>>(json, JsonOptions())
                        ?? new List<MaterialGrade>();
        }

        public void Save()
        {
            string? directory = Path.GetDirectoryName(_filePath);

            if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            string json = JsonSerializer.Serialize(Materials, JsonOptions());
            File.WriteAllText(_filePath, json);
        }

        public MaterialGrade GetById(string materialId)
        {
            MaterialGrade? material = Materials.FirstOrDefault(m => m.Id == materialId);

            if (material == null)
                throw new InvalidOperationException($"Material with Id '{materialId}' not found.");

            return material;
        }

        public MaterialGrade? TryGetById(string materialId)
        {
            return Materials.FirstOrDefault(m => m.Id == materialId);
        }

        public MaterialPropertiesAtThickness GetProperties(string materialId, double thickness_mm)
        {
            MaterialGrade material = GetById(materialId);

            return new MaterialPropertiesAtThickness
            {
                MaterialId = material.Id,
                MaterialName = material.Name,
                Standard = material.Standard,
                Thickness_mm = thickness_mm,
                Fy_Nmm2 = material.GetFy(thickness_mm),
                Fu_Nmm2 = material.GetFu(thickness_mm),
                E_Nmm2 = material.E_Nmm2,
                BetaW = material.BetaW
            };
        }

        public void AddOrUpdate(MaterialGrade material)
        {
            if (string.IsNullOrWhiteSpace(material.Id))
                throw new ArgumentException("Material Id must not be empty.");

            int index = Materials.FindIndex(m => m.Id == material.Id);

            if (index >= 0)
                Materials[index] = material;
            else
                Materials.Add(material);

            Save();
        }

        public void Delete(string materialId)
        {
            MaterialGrade? material = TryGetById(materialId);

            if (material == null)
                return;

            Materials.Remove(material);
            Save();
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }


        private static MaterialGrade CreateMaterial(
            string id,
            string name,
            string standard,
            double betaW,
            double[] fy,
            double[] fu,
            string comment = "")
        {
            return new MaterialGrade
            {
                Id = id,
                Name = name,
                Standard = standard,
                Comment = comment,
                E_Nmm2 = 210000,
                BetaW = betaW,
                Ranges = CreateRanges(fy, fu)
            };
        }

        private static List<MaterialThicknessRange> CreateRanges(double[] fy, double[] fu)
        {
            double[] limits = { 0, 16, 40, 63, 80, 100, 150, 200, 250, 400 };

            if (fy.Length != limits.Length - 1)
                throw new ArgumentException("fy array must contain 9 values.");

            if (fu.Length != limits.Length - 1)
                throw new ArgumentException("fu array must contain 9 values.");

            List<MaterialThicknessRange> ranges = new();

            for (int i = 0; i < limits.Length - 1; i++)
            {
                if (fy[i] <= 0 || fu[i] <= 0)
                    continue;

                ranges.Add(new MaterialThicknessRange
                {
                    ThicknessMin_mm = limits[i],
                    ThicknessMax_mm = limits[i + 1],
                    Fy_Nmm2 = fy[i] > 0 ? fy[i] : null,
                    Fu_Nmm2 = fu[i] > 0 ? fu[i] : null
                });
            }

            return ranges;
        }

        private static List<MaterialGrade> CreateDefaultMaterials()
        {
            return new List<MaterialGrade>
    {
        CreateMaterial(
            id: "S235",
            name: "S235",
            standard: "DIN EN 1993-1-1:2010 + EN 10025-2:2004",
            betaW: 0.8,
            fy: new double[] { 235, 235, 215, 215, 215, 195, 185, 175, 165 },
            fu: new double[] { 360, 360, 360, 360, 360, 350, 340, 340, 330 }
        ),

        CreateMaterial(
            id: "S275",
            name: "S275",
            standard: "DIN EN 1993-1-1:2010 + EN 10025-2:2004",
            betaW: 0.85,
            fy: new double[] { 275, 275, 255, 255, 235, 225, 215, 205, 195 },
            fu: new double[] { 430, 430, 410, 410, 410, 400, 380, 380, 380 }
        ),

        CreateMaterial(
            id: "S355",
            name: "S355",
            standard: "DIN EN 1993-1-1:2010 + EN 10025-2:2004",
            betaW: 0.9,
            fy: new double[] { 355, 355, 335, 335, 315, 295, 285, 275, 265 },
            fu: new double[] { 490, 490, 470, 470, 470, 450, 450, 450, 450 }
        ),

        CreateMaterial(
            id: "S460",
            name: "S460",
            standard: "DIN EN 1993-1-1:2010 + EN 10025-2:2004",
            betaW: 1.0,
            fy: new double[] { 460, 460, 430, 430, 400, 380, 370, 0, 0 },
            fu: new double[] { 540, 540, 540, 540, 540, 530, 530, 0, 0 }
        ),

        CreateMaterial(
            id: "Q235",
            name: "Q235",
            standard: "GB/T 700-2006",
            betaW: 1.0,
            fy: new double[] { 235, 225, 215, 215, 215, 195, 185, 175, 0 },
            fu: new double[] { 370, 370, 370, 370, 370, 370, 370, 370, 0 }
        ),

        CreateMaterial(
            id: "Q345",
            name: "Q345",
            standard: "GB/T 1591-2008",
            betaW: 1.0,
            fy: new double[] { 345, 335, 325, 315, 305, 285, 275, 265, 0 },
            fu: new double[] { 470, 470, 470, 470, 470, 450, 450, 450, 0 }
        ),

        CreateMaterial(
            id: "C45E",
            name: "C45E",
            standard: "EN 10083-3:2006",
            betaW: 1.0,
            fy: new double[] { 340, 305, 305, 305, 305, 275, 275, 275, 240 },
            fu: new double[] { 620, 580, 580, 580, 580, 560, 560, 560, 540 }
        ),

        CreateMaterial(
            id: "C45E_QT",
            name: "C45E QT",
            standard: "EN 10083-3:2006",
            betaW: 1.0,
            fy: new double[] { 490, 430, 370, 370, 370, 0, 0, 0, 0 },
            fu: new double[] { 850, 650, 630, 630, 630, 0, 0, 0, 0 }
        ),

        CreateMaterial(
            id: "34CrMo4",
            name: "34CrMo4",
            standard: "EN 10083-3:2006",
            betaW: 1.0,
            fy: new double[] { 800, 650, 550, 550, 550, 500, 450, 450, 0 },
            fu: new double[] { 1000, 900, 800, 800, 800, 750, 700, 700, 0 }
        ),

        CreateMaterial(
            id: "42CrMo4",
            name: "42CrMo4",
            standard: "EN 10083-3:2006",
            betaW: 1.0,
            fy: new double[] { 900, 750, 650, 650, 650, 550, 500, 500, 390 },
            fu: new double[] { 1100, 1000, 900, 900, 900, 800, 750, 750, 600 }
        )
    };
        }



    }
}
