using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Proryv.AskueARM2.Both.VisualCompHelpers.XamDataPresenter
{
    public class ChanneAndMeasureCategory
    {
        public readonly byte Channel;
        public readonly string MeasureCategory;

        public ChanneAndMeasureCategory(byte channel, string measureCategory)
        {
            Channel = channel;
            MeasureCategory = measureCategory;
        }
    }

    public class ChanneAndMeasureCategoryEqualityComparer : IEqualityComparer<ChanneAndMeasureCategory>
    {
        public bool Equals(ChanneAndMeasureCategory x, ChanneAndMeasureCategory y)
        {
            return x.Channel == y.Channel;
        }

        public int GetHashCode(ChanneAndMeasureCategory obj)
        {
            return obj.Channel;
        }
    }
}
