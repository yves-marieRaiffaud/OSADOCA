using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UI;
using Mathd_Lib;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CommonMethods
{
    public static class MathsOps
    {
        public static NumberStyles DOUBLE_PARSE_STYLES = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
        public static IFormatProvider DOUBLE_PARSE_FORMAT = CultureInfo.CreateSpecificCulture("en-GB");
        //======================================================================================================
        public static float mapInRange(Vector2 inputRange, Vector2 outputRange, float valueToMap)
        {
            float a1 = inputRange.x;
            float a2 = inputRange.y;
            float b1 = outputRange.x;
            float b2 = outputRange.y;

            float remapped_val = b1 + (valueToMap - a1)*(b2-b1)/(a2-a1);
            return remapped_val;
        }

        public enum isInRangeFlags { both_included, both_excluded, lowerIncluded_UpperExcluded, lowerExcluded_UpperIncluded };
        public static bool isInRange(float val, float lowerBound, float upperBound, isInRangeFlags flag=isInRangeFlags.both_included)
        {
            if(flag == isInRangeFlags.both_included) {
                if(val <= upperBound && val >= lowerBound)
                    return true;
                else
                    return false;
            }
            else if(flag == isInRangeFlags.both_excluded) {
                if(val < upperBound && val > lowerBound)
                    return true;
                else
                    return false;
            }
            else if(flag == isInRangeFlags.lowerExcluded_UpperIncluded) {
                if(val <= upperBound && val > lowerBound)
                    return true;
                else
                    return false;
            }
            else if(flag == isInRangeFlags.lowerIncluded_UpperExcluded)
            {
                if(val < upperBound && val >= lowerBound)
                    return true;
                else
                    return false;
            }
            return false;
        }
        public static bool isInRange(double val, double lowerBound, double upperBound, isInRangeFlags flag=isInRangeFlags.both_included)
        {
            if(flag == isInRangeFlags.both_included) {
                if(val <= upperBound && val >= lowerBound)
                    return true; 
                else
                    return false;
            }
            else if(flag == isInRangeFlags.both_excluded) {
                if(val < upperBound && val > lowerBound)
                    return true;
                else
                    return false;
            }
            else if(flag == isInRangeFlags.lowerExcluded_UpperIncluded) {
                if(val <= upperBound && val > lowerBound)
                    return true;
                else
                    return false;
            }
            else if(flag == isInRangeFlags.lowerIncluded_UpperExcluded) {
                if(val < upperBound && val >= lowerBound)
                    return true;
                else
                    return false;
            }
            return false;
        }

        public static bool DoubleIsValid(double val1)
        {
            if(!double.IsInfinity(val1) && !double.IsNaN(val1))
                return true;
            else
                return false;
        }
        public static bool DoubleIsValid(params double[] values)
        {
            foreach(double value in values) {
                bool isValid = DoubleIsValid(value);
                if(!isValid)
                    return false;
            }
            return true;
        }

        public static bool FloatsAreEqual(float a, float b, float tolerance=float.Epsilon)
        {
            if(Mathf.Abs(a - b) <= tolerance)
                return true;
            else
                return false; 
        }
        public static bool DoublesAreEqual(double a, double b, double tolerance=Mathd.Epsilon)
        {
            if(Mathd.Abs(a - b) <= tolerance)
                return true; 
            else
                return false;
        }

        public static float ClampAngle(float value, float lowerBound, float upperBound, float incr=2f*Mathf.PI)
        {
            while(value - upperBound > Mathf.Epsilon)
                value -= incr;
            while(value - lowerBound < Mathf.Epsilon)
                value += incr;
            return value;
        }
        public static double ClampAngle(double value, double lowerBound, double upperBound, double incr=2d*Mathd.PI)
        {
            while(value - upperBound > Mathf.Epsilon)
                value -= incr;
            while(value - lowerBound < Mathf.Epsilon)
                value += incr;
            return value;
        }

        /// <summary>
        /// Returns the boolean indicating if float 'a' is greater than float 'b', with respect to the specified tolerance.
        /// </summary>
        public static bool FloatIsGreaterThan(float a, float b, float tolerance=float.Epsilon)
        {
            if(a-b > tolerance)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Returns the boolean indicating if float 'a' is smaller than float 'b', with respect to the specified tolerance.
        /// </summary>
        public static bool FloatIsSmallerThan(float a, float b, float tolerance=float.Epsilon)
        {
            if(a-b < tolerance)
                return true;
            else
                return false;
        }
        
        /// <summary>
        /// Returns the boolean indicating if double 'a' is smaller than double 'b', with respect to the specified tolerance.
        /// </summary>
        public static bool DoubleIsSmallerThan(double a, double b, double tolerance=double.Epsilon)
        {
            if(a-b < tolerance)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Returns the boolean indicating if double 'a' is greater than double 'b', with respect to the specified tolerance.
        /// </summary>
        public static bool DoubleIsGreaterThan(double a, double b, double tolerance=double.Epsilon)
        {
            if(a-b > tolerance)
                return true;
            else
                return false;
        }

        public static float linearInterpolation(float x1, float y1, float x3, float y3, float xValToGuessY)
        {
            return (y3-y1)/(x3-x1) * (xValToGuessY-x1) + y1;
        }

        public static string DoubleToSignificantDigits(double value, int significant_digits)
        {
            string format1 = "G" + significant_digits.ToString();
            string result = value.ToString(format1, CultureInfo.InvariantCulture);
            return result;
        }
        public static string FloatToSignificantDigits(float value, int significant_digits)
        {
            string format1 = "G" + significant_digits.ToString();
            string result = value.ToString(format1, CultureInfo.InvariantCulture);
            return result;
        }
        public static string DoubleToString(double value)
        {
            string result = value.ToString("G", CultureInfo.InvariantCulture);
            return result;
        }
        public static string StringToSignificantDigits(string value, int significant_digits)
        {
            double castValue;
            ParseStringToDouble(value, out castValue);
            return DoubleToSignificantDigits(castValue, significant_digits);
        }

        public static bool ParseStringToDouble(string stringToCheck, out double result)
        {
            bool operationRes = double.TryParse(stringToCheck, DOUBLE_PARSE_STYLES, DOUBLE_PARSE_FORMAT, out result);
            if(operationRes)
                return true;
            else {
                result = double.NaN;
                return false;
            }
        }
        public static bool ParseStringToFloat(string stringToCheck, out float result)
        {
            bool operationRes = float.TryParse(stringToCheck, DOUBLE_PARSE_STYLES, DOUBLE_PARSE_FORMAT, out result);
            if(operationRes)
                return true;
            else {
                result = float.NaN;
                return false;
            }
        }
        public static bool TryParse_String_To_Bool(string value, bool elseValue)
        {
            bool parsedValue;
            if(bool.TryParse(value, out parsedValue))
                return parsedValue;
            else
                return elseValue;
        }
        public static int TryParse_String_To_Int(string value, int elseValue)
        {
            int parsedValue;
            if(int.TryParse(value, out parsedValue))
                return parsedValue;
            else
                return elseValue;
        }
        public static Vector2 StringToVector2(string stringVal)
        {
            string[] temp = stringVal.Substring(1, stringVal.Length-2).Split(',');
            double xVal = double.Parse(temp[0], CultureInfo.InvariantCulture);
            double yVal = double.Parse(temp[1], CultureInfo.InvariantCulture);
            return new Vector2((float)xVal, (float)yVal);
        }

    }
}