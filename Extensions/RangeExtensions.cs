using System;
using System.Collections.Generic;
using System.Linq;

namespace Uploader.Extensions
{
  public static class RangeExtensions
  {
    public static IEnumerable<int> AsIEnumerable(this Range range)
    {
      if (range.Start.IsFromEnd || range.End.IsFromEnd)
      {
        throw new ArgumentException("Cannot use range with from end enabled", nameof(range));
      }
      return Enumerable.Range(range.Start.Value, range.End.Value - range.Start.Value);
    }
  }
}