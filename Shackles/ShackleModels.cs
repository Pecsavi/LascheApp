namespace LascheApp.Shackles
{
    public class ShackleData
    {
        public string Id { get; set; } = "";
        public string Manufacturer { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Standard { get; set; } = "";
        public string Comment { get; set; } = "";

        /// <summary>
        /// Nominal size, e.g. "1/2", "2", "4 3/8".
        /// </summary>
        public string NominalSize { get; set; } = "";

        /// <summary>
        /// Working load limit in kg, as given in the catalogue.
        /// </summary>
        public double WLL_kg { get; set; }

        /// <summary>
        /// Working load limit in kN.
        /// Calculated from WLL_kg.
        /// </summary>
        public double WLL_kN => WLL_kg * 9.81 / 1000.0;

        /// <summary>
        /// d1 from catalogue.
        /// </summary>
        public double D1_mm { get; set; }

        /// <summary>
        /// d2 from catalogue = shackle pin diameter.
        /// Used as Dpin in padeye calculation.
        /// </summary>
        public double Dpin_mm { get; set; }

        /// <summary>
        /// d3 from catalogue.
        /// </summary>
        public double D3_mm { get; set; }

        /// <summary>
        /// d4 nominal thread / bolt size from catalogue, e.g. "5/8", "2 1/4".
        /// Kept as string because it is given in inches.
        /// </summary>
        public string D4_inch { get; set; } = "";

        /// <summary>
        /// b1 from catalogue.
        /// Inner width at the pin / jaw.
        /// Used for padeye thickness recommendation:
        /// t_total >= 0.75 * B1_mm.
        /// </summary>
        public double B1_mm { get; set; }

        /// <summary>
        /// b2 from catalogue.
        /// Used for DNV lever arm calculation.
        /// </summary>
        public double B2_mm { get; set; }

        /// <summary>
        /// h2 from catalogue.
        /// Used for DNV lever arm calculation.
        /// </summary>
        public double H2_mm { get; set; }

        /// <summary>
        /// Lever arm used for out-of-plane padeye calculation.
        /// h = h2 - b2 / 2.
        /// </summary>
        public double H_DNV_mm => H2_mm - B2_mm / 2.0;

        /// <summary>
        /// Optional material Id from the material database.
        /// Example: S690, 42CrMo4, etc.
        /// </summary>
        public string PinMaterialId { get; set; } = "";
    }
}