namespace LascheApp.Padeye
{
    public class PadeyeEcGeometryInput
    {
        public double F_Ed_kN { get; set; }
        public double GammaM0 { get; set; } = 1.0;
        public double Fy_Nmm2 { get; set; }

        public double PlateThickness_mm { get; set; }
        public double HoleDiameter_mm { get; set; }
        public double EdgeDistanceA_mm { get; set; }
        public double SideDistanceC_mm { get; set; }
    }

    public class PadeyeEcGeometryResult
    {
        public PadeyeEcGeometryInput Input { get; set; } = new();

        public double RequiredThickness_MoglichkeitB_mm { get; set; }
        public double MaxHoleDiameter_MoglichkeitB_mm { get; set; }

        public bool ThicknessMoglichkeitB_Ok { get; set; }
        public bool HoleDiameterMoglichkeitB_Ok { get; set; }
        public double RequiredEdgeDistanceA_mm { get; set; }
        public double RequiredSideDistanceC_mm { get; set; }

        public bool EdgeDistanceA_Ok { get; set; }
        public bool SideDistanceC_Ok { get; set; }

        public bool MoglichkeitA_Ok =>
    EdgeDistanceA_Ok &&
    SideDistanceC_Ok;

        public bool MoglichkeitB_Ok =>
            ThicknessMoglichkeitB_Ok &&
            HoleDiameterMoglichkeitB_Ok;

        public bool IsOk =>
            MoglichkeitA_Ok ||
            MoglichkeitB_Ok;
    }
}