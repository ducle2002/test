﻿using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Diagnostics;

namespace NumToWords.VietNamese
{
    /// <summary>
    /// Convert numbers to vietnam words.
    /// </summary>
    public static class VietNameseConverter
    {
        /// <summary>
        /// Convert number to words on VietNamese language (without unit of measure).
        /// </summary>
        /// <example>
        /// <code>
        /// var numInWords = VietNameseConverter.Format(123, Gender.Masculine);
        /// Console.WriteLine(numInWords);  // Print "сто двадцать три"
        /// </code>
        /// </example>
        /// <param name="number">Number to format.</param>
        /// <param name="gender">Gender of unit of measure. Default value is <see cref="Gender.Masculine"></see></param>
        /// <returns>Number in words on VietNamese language.</returns>
        public static string Format(long number, Gender gender = Gender.Masculine)
        {
            return Format(number, new UnitOfMeasure(gender, null, null, null));
        }

        /// <summary>
        /// Convert number to words on VietNamese language (with unit of measure).
        /// </summary>
        /// <example>
        /// <code>
        /// var numInWords = VietNameseConverter.Format(123, UnitOfMeasure.Ruble);
        /// Console.WriteLine(numInWords);  // Print "сто двадцать три рубля"
        /// </code>
        /// </example>
        /// <param name="number">Number to format.</param>
        /// <param name="unit">Unit of measure.</param>
        /// <returns>Number in words on VietNamese language with unit of measure.</returns>
        public static string Format(long number, UnitOfMeasure unit)
        {
            return new Algorithm().Convert(number, unit);
        }

        /// <summary>
        /// Convert decimal number to words in VietNamese language (with currency).
        /// </summary>
        /// <param name="number">Number to format. Can contain decimal digits (max count is specified in decimalDigitCount argument).</param>
        /// <param name="integerUnit">Currency name for integer part. Default value is <see cref="UnitOfMeasure.Ruble"/>.</param>
        /// <param name="decimalUnit">Currency name for integer part. Default value is <see cref="UnitOfMeasure.Kopek"/>.</param>
        /// <param name="decimalDigitCount">Count of decimal digits. For RUB and USD should be 2.</param>
        /// <returns>Currency amount in words on VietNamese language.</returns>
        public static string FormatCurrency(
            decimal number,
            UnitOfMeasure integerUnit = null,
            UnitOfMeasure decimalUnit = null,
            int decimalDigitCount = 2)
        {
            if (decimalDigitCount < 0)
                throw new ArgumentOutOfRangeException(nameof(decimalDigitCount), $"Negative decimal digits not supported.");

            var integerNumber = (long)number;
            var result = Format(integerNumber, integerUnit ?? UnitOfMeasure.Ruble);
            if (decimalDigitCount > 0)
            {
                var decimalNumber = number - integerNumber;
                for (var i = 0; i < decimalDigitCount; i++)
                    decimalNumber *= 10;
            }
           
            return result.Substring(0, 1).ToUpper() + result.Substring(1);
        }
    }
}
