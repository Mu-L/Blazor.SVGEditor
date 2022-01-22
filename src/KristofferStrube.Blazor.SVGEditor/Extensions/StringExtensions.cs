﻿using System.Globalization;

namespace KristofferStrube.Blazor.SVGEditor.Extensions
{
    internal static class StringExtensions
    {
        internal static double ParseAsDouble(this string s) => double.Parse(s, CultureInfo.InvariantCulture);
    }
}