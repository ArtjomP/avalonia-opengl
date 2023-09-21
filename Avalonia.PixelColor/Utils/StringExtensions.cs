using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Avalonia.PixelColor.Utils;

public static class StringExtensions
{
    public static String Reverse(this String str)
    {
        var result = str;
        if (str is { Length: > 1})
        {
            var strBuilder = new StringBuilder();
            for (var i = str.Length - 1; i >= 0; --i)
            {
                strBuilder.Append(str[i]);
            }

            result = strBuilder.ToString();
        }

        return result;
    }

    public static String Substring(
        this String str,
        String left,
        Int32 startIndex,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        Guard.IsNotNull(left);
        Guard.HasSizeGreaterThan(left, 0);
        Guard.IsGreaterThan(startIndex, 0);
        Guard.IsLessThan(startIndex, str.Length);
        var leftPosBegin = str.IndexOf(left, startIndex, comparsion);
        String? result;
        if (leftPosBegin == -1 || String.IsNullOrEmpty(str))
        {
            result = String.Empty;
        }
        else
        {
            var leftPosEnd = leftPosBegin + left.Length;
            var length = str.Length - leftPosEnd;
            result = str.Substring(leftPosEnd, length);
        }

        return result;
    }

    public static String Substring(
        this String str,
        String left,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        return Substring(str, left, 0, comparsion);
    }

    public static String Substring(
        this String str,
        String left,
        String right,
        Int32 startIndex,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        Guard.IsNotNullOrEmpty(left);
        Guard.IsNotNullOrEmpty(right);
        Guard.IsGreaterThanOrEqualTo(startIndex, 0);
        Guard.IsLessThan(startIndex, str.Length);

        String? result;
        var leftPosBegin = str.IndexOf(left, startIndex, comparsion);
        if (leftPosBegin == -1 || String.IsNullOrEmpty(str))
        {
            result = String.Empty;
        }
        else
        {
            var leftPosEnd = leftPosBegin + left.Length;
            var rightPos = str.IndexOf(right, leftPosEnd, comparsion);
            if (rightPos == -1)
            {
                result = string.Empty;
            }
            else
            {
                var length = rightPos - leftPosEnd;
                result = str.Substring(leftPosEnd, length);
            }              
        }

        return result;           
    }

    public static String Substring(
        this String str,
        String left,
        String right,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        return str.Substring(left, right, 0, comparsion);
    }

    public static String LastSubstring(
        this String str,
        String left,
        Int32 startIndex,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        Guard.IsNotNullOrEmpty(left);
        Guard.IsGreaterThanOrEqualTo(startIndex, 0);
        Guard.IsLessThan(startIndex, str.Length);

        String? result;
        var leftPosBegin = str.LastIndexOf(left, startIndex, comparsion);
        if (leftPosBegin == -1 || String.IsNullOrEmpty(str))
        {
            result = String.Empty;
        }
        else
        {
            var leftPosEnd = leftPosBegin + left.Length;                
            var length = str.Length - leftPosEnd;
            result = str.Substring(leftPosEnd, length);
        }

        return result;            
    }

    public static String LastSubstring(
        this String str,
        String left,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        var result = String.IsNullOrEmpty(str)
            ? String.Empty
            : LastSubstring(str, left, str.Length - 1, comparsion);
        return result;
    }

    public static String LastSubstring(
        this String str,
        String left,
        String right,
        Int32 startIndex,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        Guard.IsNotNullOrEmpty(left);
        Guard.IsNotNullOrEmpty(right);
        Guard.IsGreaterThanOrEqualTo(startIndex, 0);
        Guard.IsLessThan(startIndex, str.Length);

        String? result;
        var leftPosBegin = str.LastIndexOf(left, startIndex, comparsion);
        if (leftPosBegin == -1 || String.IsNullOrEmpty(str))
        {
            result = String.Empty;
        }
        else
        {
            var leftPosEnd = leftPosBegin + left.Length;
            var rightPos = str.IndexOf(right, leftPosEnd, comparsion);
            if (rightPos == -1)
            {
                if (leftPosBegin == 0)
                {
                    result = String.Empty;
                }
                else
                {
                    result = LastSubstring(str, left, right, leftPosBegin - 1, comparsion);
                }
            }
            else
            {
                var length = rightPos - leftPosEnd;
                result = str.Substring(leftPosEnd, length);
            }
            
        }

        return result;
    }

    public static String LastSubstring(
        this String str,
        String left,
        String right,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        var result = String.IsNullOrEmpty(str)
            ? String.Empty
            : str.LastSubstring(left, right, str.Length - 1, comparsion);
        return result;
    }

    public static String[] Substrings(
        this String str,
        String left,
        String right,
        Int32 startIndex,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        Guard.IsNotNullOrEmpty(left);
        Guard.IsNotNullOrEmpty(right);
        Guard.IsGreaterThanOrEqualTo(startIndex, 0);
        Guard.IsLessThan(startIndex, str.Length);

        var result = Array.Empty<String>();
        if (!String.IsNullOrEmpty(str))
        {
            var currentStartIndex = startIndex;
            var strings = new List<string>();
            while (true)
            {
                var leftPosBegin = str.IndexOf(left, currentStartIndex, comparsion);
                if (leftPosBegin == -1)
                {
                    break;
                }

                var leftPosEnd = leftPosBegin + left.Length;
                var rightPos = str.IndexOf(right, leftPosEnd, comparsion);
                if (rightPos == -1)
                {
                    break;
                }

                var length = rightPos - leftPosEnd;
                strings.Add(str.Substring(leftPosEnd, length));
                currentStartIndex = rightPos + right.Length;
            }

            result = strings.ToArray();

        }

        return result;
    }

    public static String[] Substrings(
        this String str,
        String left,
        String right,
        StringComparison comparsion = StringComparison.Ordinal)
    {
        return str.Substrings(left, right, 0, comparsion);
    }
}
