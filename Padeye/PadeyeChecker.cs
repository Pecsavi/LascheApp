namespace LascheApp.Padeye
{
    public static class PadeyeChecker
    {
        public static PadeyeCheckResult Check(
            PadeyeBasicCheckInput basicInput,
            PadeyeEcGeometryInput ecGeometryInput)
        {
            PadeyeBasicCheckResult basicResult =
                PadeyeBasicChecker.Check(basicInput);

            PadeyeEcGeometryResult ecGeometryResult =
                PadeyeEcGeometryChecker.Check(ecGeometryInput);

            return new PadeyeCheckResult
            {
                BasicResult = basicResult,
                EcGeometryResult = ecGeometryResult
            };
        }
    }
}