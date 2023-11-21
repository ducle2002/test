namespace NumToWords.VietNamese
{
    /// <summary>
    /// Constant definitions for the formatting algorithm.
    /// </summary>
    internal static class Constants
    {
        public const string Minus = "trừ";
        public const string Zero = "không";

        public static readonly string[][] Digits =
        {
            null,   // 0 - not used
            new[] {"một", "một", "một"},
            new[] {"hai", "hai", "hai"},
            new[] {"ba", "ba", "ba"},
            new[] {"bốn", "bốn", "bốn"},
            new[] {"năm", "năm", "năm"},
            new[] {"sáu", "sáu", "sáu"},
            new[] {"bảy", "bảy", "bảy"},
            new[] {"tám", "tám", "tám"},
            new[] {"chín", "chín", "chín"},
        };

        public static readonly string[] Tens =
        {
                 "mười",
             "mười một",
             "mười hai",
             "mười ba",
             "mười bốn",
             "mười lăm",
             "mười sáu",
             "mười bảy",
             "mười tám",
             "mười chín"
        };

        public static readonly string[] Dozens =
        {
            null,   //  0 - not used
            null,   // 10 - not used
              "hai mươi",
             "ba mươi",
             "bốn mươi",
             "năm mươi",
             "sáu mươi",
             "bảy mươi",
             "tám mươi",
             "chín mươi"
        };

        public static readonly string[] Hundreds =
        {
               null,   // 0 - not used
              "một trăm",
             "hai trăm",
             "ba trăm",
             "bốn trăm",
             "năm trăm",
             "sáu trăm",
             "bảy trăm",
             "tám trăm",
             "chín trăm"
        };

        public static readonly UnitOfMeasure Е3Unit = new UnitOfMeasure(Gender.Feminine, "nghìn", "nghìn", "nghìn");
        public static readonly UnitOfMeasure Е6Unit = new UnitOfMeasure(Gender.Masculine, "triệu", "triệu", "triệu");
        public static readonly UnitOfMeasure Е9Unit = new UnitOfMeasure(Gender.Masculine, "tỉ", "tỉ", "tỉ");
    }
}
