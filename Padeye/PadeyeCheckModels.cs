using System;
using System.Collections.Generic;
using System.Linq;

namespace LascheApp.Padeye
{
    public class PadeyeCheckResult
    {
        public PadeyeBasicCheckResult BasicResult { get; set; } = new();
        public PadeyeEcGeometryResult EcGeometryResult { get; set; } = new();

        public bool IsOk =>
            BasicResult.IsOk &&
            EcGeometryResult.IsOk;

        public List<CheckItem> GoverningCheckItems
        {
            get
            {
                List<CheckItem> items = new();

                if (!BasicResult.HasErrors)
                    items.AddRange(BasicResult.CheckItems);

                if (!EcGeometryResult.HasErrors)
                {
                    if (EcGeometryResult.MoglichkeitA_MaxUtilization <= EcGeometryResult.MoglichkeitB_MaxUtilization)
                        items.AddRange(EcGeometryResult.MoglichkeitA_CheckItems);
                    else
                        items.AddRange(EcGeometryResult.MoglichkeitB_CheckItems);
                }

                return items;
            }
        }

        public double MaxUtilization =>
            GoverningCheckItems.Count == 0
                ? 0.0
                : GoverningCheckItems.Max(i => i.Utilization);

        public string GoverningCheckName =>
            GoverningCheckItems
                .OrderByDescending(i => i.Utilization)
                .FirstOrDefault()?.Name ?? "";
    }
}