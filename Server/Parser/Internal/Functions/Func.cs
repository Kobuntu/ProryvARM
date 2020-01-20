using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Proryv.Servers.Calculation.Parser.Internal.Functions
{
    /// <summary>
    /// Class contains methods for the string representation of numbers, dates and currency values with national speciality.
    /// </summary>
    public sealed class Func
    {
        const bool UseRoundForToCurrencyWordsFunctions = true;

        #region Gender

        /// <summary>
        /// Enum that contains genders
        /// </summary>
        public enum Gender
        {
            /// <summary>
            /// Masculine gender.
            /// </summary>
            Masculine = 0,
            /// <summary>
            /// Femail gender.
            /// </summary>
            Feminine = 1,
            /// <summary>
            /// Neutral gender.
            /// </summary>
            Neutral = 2
        }
        #endregion

        #region BaseCurrency
        /// <summary>
        /// An abstract class for the representation gender of the currency.
        /// </summary>
        public abstract class BaseCurrency
        {
            /// <summary>
            /// Gender of the integer part of the currency.
            /// </summary>
            public abstract Gender Gender
            {
                get;
            }
            /// <summary>
            /// Gender of the decimal part of the currency.
            /// </summary>
            public abstract Gender CentsGender
            {
                get;
            }
        }
        #endregion

        #region Ru
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with Russian language speciality.
        /// </summary>
        public sealed class Ru
        {
            #region Currency
            private static Hashtable Currencies = new Hashtable();
            /// <summary>
            /// Registers a new currency.
            /// </summary>
            /// <param name="currency">Specifies currency declension.</param>
            /// <param name="currencyName">Sets the currency name.</param>
            public static void RegisterCurrency(Currency currency, string currencyName)
            {
                Currencies[currencyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture)] = currency;
            }


            private static Currency GetCurrency(string currencyName)
            {
                Currency currency = Currencies[currencyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture)] as Currency;
                if (currency == null)
                    throw new ArgumentException(string.Format("Currency '{0}' is not registered", currencyName));
                return currency;
            }

            /// <summary>
            /// An abstract class for defining a currency.
            /// </summary>
            public abstract class Currency : BaseCurrency
            {
                protected abstract string[] Dollars
                {
                    get;
                }

                protected abstract string[] Cents
                {
                    get;
                }

                /// <summary>
                /// Gets the currency name in singular (e. g. dollar).
                /// </summary>
                public string DollarOne
                {
                    get
                    {
                        return Dollars[0];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural (e. g. dollars).
                /// </summary>
                public string DollarTwo
                {
                    get
                    {
                        return Dollars[1];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural in correct declension (e. g. dollars).
                /// </summary>
                public string DollarFive
                {
                    get
                    {
                        return Dollars[2];
                    }
                }

                /// <summary>
                /// Gets the currency name in singular (e. g. cent).
                /// </summary>
                public string CentOne
                {
                    get
                    {
                        return Cents[0];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural (e. g. cents).
                /// </summary>
                public string CentTwo
                {
                    get
                    {
                        return Cents[1];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural in correct declension (e. g. cents).
                /// </summary>
                public string CentFive
                {
                    get
                    {
                        return Cents[2];
                    }
                }

            }

            /// <summary>
            /// Class specifies the Russian currency.
            /// </summary>
            public class RURCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }
                /// <summary>
                /// Gets gender of the decimal part of the currency.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }


                private string[] dollars = new string[] { "рубль", "рубля", "рублей" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "копейка", "копейки", "копеек" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }


            /// <summary>
            /// Class specifies the USD currency.
            /// </summary>
            public class USDCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the dollars.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }
                /// <summary>
                /// Gets gender of the cents.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }


                private string[] dollars = new string[] { "доллар", "доллара", "долларов" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "цент", "цента", "центов" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }
            /// <summary>
            /// Class specifies Euro currency.
            /// </summary>
            public class EURCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Neutral;
                    }
                }
                /// <summary>
                /// Gets gender of the decimal part of the currency.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }


                private string[] dollars = new string[] { "евро", "евро", "евро" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "цент", "цента", "центов" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }

            /// <summary>
            /// Class defines Ukrain currency.
            /// </summary>
            public class UAHCurrency : Currency
            {

                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }

                /// <summary>
                /// Gets gender of the decimal part of the currency.                
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }


                private string[] dollars = new string[] { "гривна", "гривны", "гривен" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "копейка", "копейки", "копеек" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }

            /// <summary>
            /// Class specifies the Kazakh currency.
            /// </summary>
            public class KZTCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }

                /// <summary>
                /// Gets gender of the decimal part of the currency.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }

                private string[] dollars = new string[] { "тенге", "тенге", "тенге" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }

                private string[] cents = new string[] { "тиын", "тиына", "тиынов" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }
            #endregion

            #region Fields
            private static string[] months = new string[]
			{
				"января", "февраля", "марта", "апреля", "мая", "июня", 
				"июля", "августа", "сентября", "октября", "ноября", "декабря"
			};
            private static string[] units = new string[]
			{
				"один", "два", "три", "четыре", "пять", 
				"шесть", "семь", "восемь", "девять", "десять", "одиннадцать", 
				"двенадцать", "тринадцать", "четырнадцать", "пятнадцать",
				"шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"
			};

            private static string[] tens = new string[]
			{
				"десять",  "двадцать", "тридцать", "сорок", "пятьдесят", 
				"шестьдесят", "семьдесят", "восемьдесят", "девяносто"
			};
            private static string[] hundreds = new string[]
			{
				"сто", "двести", "триста", "четыреста", "пятьсот", 
				"шестьсот", "семьсот", "восемьсот", "девятьсот"
			};
            private static string[,] gendered = new string[,]
			{
				{ "один", "одна", "одно" },
				{ "два",  "две",  "два" }
			};
            #endregion

            #region Methods
            private static void AddUnits(StringBuilder sb, long value, Gender gender)
            {
                if (value != 0)
                {
                    if (sb.Length > 0) sb.Append(" ");
                    if (value < 3)
                    {
                        sb.Append(gendered[value - 1, (int)gender]);
                    }
                    else sb.Append(units[value - 1]);
                }
            }

            private static void AddTens(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(tens[value - 1]);
                }
            }

            private static void AddHundreds(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(hundreds[value - 1]);
                }
            }

            private static void AddThousand(StringBuilder sb, long value, Gender gender)
            {
                AddHundreds(sb, value / 100);

                value = value % 100;

                if (value < 20) AddUnits(sb, value, gender);
                else
                {
                    AddTens(sb, value / 10);
                    AddUnits(sb, value % 10, gender);
                }
            }

            private static void AddRank(StringBuilder sb, ref long rank,
                ref long value, string one, string two, string five, Gender gender)
            {
                long rankValue = value / rank;

                if (rankValue > 0)
                {
                    AddThousand(sb, rankValue, gender);

                    long units = rankValue % 10;
                    long units2 = rankValue % 100;

                    string unit = string.Empty;

                    if (units2 >= 11 && units2 < 20) unit = five;
                    else
                    {
                        if (units == 1) unit = one;
                        else if (units > 1 && units < 5) unit = two;
                        else unit = five;
                    }

                    if (rankValue > 10 && rankValue < 20) unit = five;

                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(unit);

                    value %= rank;
                }
                rank /= 1000;
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <returns>Returns the string representation of the double value.</returns>
            public static string NumToStr(double value)
            {
                return NumToStr((decimal)value);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the double value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(double value, bool uppercase)
            {
                return NumToStr((long)value, uppercase);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="gender">A Gender parameter on which gender of the resulting value depends.</param>
            /// <returns>Returns the string representation of the double value with the first symbol in the uppercase or not. Gender depends on the gender parameter.</returns>
            public static string NumToStr(double value, bool uppercase, Gender gender)
            {
                return NumToStr((long)value, uppercase, gender);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <returns>Returns the string representation of the decimal value.</returns>
            public static string NumToStr(decimal value)
            {
                return NumToStr((long)value);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(decimal value, bool uppercase)
            {
                return NumToStr((long)value, uppercase);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="gender">A Gender parameter on which gender of the resulting value depends.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not. Gender depends on gender parameter.</returns>
            public static string NumToStr(decimal value, bool uppercase, Gender gender)
            {
                return NumToStr((long)value, uppercase, gender);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <returns>Returns the string representation of the long value.</returns>
            public static string NumToStr(long value)
            {
                return NumToStr(value, true, Gender.Masculine);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the long value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(long value, bool uppercase)
            {
                return NumToStr(value, uppercase, Gender.Masculine);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="gender">A Gender parameter on which gender of the resulting value depends.</param>
            /// <returns>Returns the string representation of the long value with the first symbol in the uppercase or not. Gender depends on gender parameter.</returns>
            public static string NumToStr(long value, bool uppercase, Gender gender)
            {
                StringBuilder sb = new StringBuilder();
                if (value == 0) sb.Append("ноль");
                else
                {
                    if (value < 0)
                    {
                        sb.Append("минус");
                        value = Math.Abs(value);
                    }

                    long rank = 1000000000000000000;
                    AddRank(sb, ref rank, ref value, "квинтильон", "квинтильона", "квинтильонов", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "квадрильон", "квадрильона", "квадрильонов", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "триллион", "триллиона", "триллионов", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "миллиард", "миллиарда", "миллиардов", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "миллион", "миллиона", "миллионов", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "тысяча", "тысячи", "тысяч", Gender.Feminine);
                    AddThousand(sb, value, gender);

                }
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);

                return sb.ToString();
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the long value with or without cents and the currency name.</returns>
            public static string CurrToStr(long value, bool cents)
            {
                return CurrToStr((decimal)value, "RUR", cents);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the long value with cents and the currency name.</returns>
            public static string CurrToStr(long value, string currency, bool cents)
            {
                return CurrToStr((decimal)value, true, currency, cents);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents and the currency name.</returns>
            public static string CurrToStr(double value, bool cents)
            {
                return CurrToStr(value, "RUR", cents);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents and the currency name.</returns>
            public static string CurrToStr(decimal value, bool cents)
            {
                return CurrToStr(value, "RUR", cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents and the currency name.</returns>
            public static string CurrToStr(double value, string currency, bool cents)
            {
                //return CurrToStr(value, true, currency, cents);
                return CurrToStr((decimal)value, true, currency, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents and the currency name.</returns>
            public static string CurrToStr(decimal value, string currency, bool cents)
            {
                return CurrToStr((decimal)value, true, currency, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(double value, bool uppercase, string currency, bool cents)
            {
                return CurrToStr((decimal)value, uppercase, currency, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(decimal value, bool uppercase, string currency, bool cents)
            {
                decimal integral = Math.Truncate(value);
                long dec = 0;
                if (UseRoundForToCurrencyWordsFunctions)
                {
                    dec = (long)Math.Round((value - (long)integral) * 100);
                    if (dec > 99)
                    {
                        dec = 0;
                        integral++;
                    }
                }
                else
                {
                    dec = (long)((value - (long)integral) * 100);
                }

                string str = string.Format("{0} {1}", NumToStr(integral, uppercase, GetCurrency(currency).Gender), Decline(integral, currency));
                if (cents)
                {
                    str += string.Format(" {0:d2}", dec);
                    str += string.Format(" {0}", Decline(dec, currency, true));
                }
                return str;
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the double value with cents and the currency name.</returns>
            public static string CurrToStr(double value)
            {
                return CurrToStr((decimal)value, "RUR");
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the decimal value with cents and the currency name.</returns>
            public static string CurrToStr(decimal value)
            {
                //return CurrToStr(value, "RUR");
                return CurrToStr((decimal)value, "RUR");
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents and the currency name.</returns>
            public static string CurrToStr(double value, string currency)
            {
                return CurrToStr((decimal)value, true, currency);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents and the currency name.</returns>
            public static string CurrToStr(decimal value, string currency)
            {
                //return CurrToStr(value, true, currency);
                return CurrToStr((decimal)value, true, currency);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(long value, bool uppercase, string currency)
            {
                return CurrToStr((decimal)value, uppercase, currency, true);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(double value, bool uppercase, string currency)
            {
                return CurrToStr((decimal)value, uppercase, currency, true);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the decimal value with cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(decimal value, bool uppercase, string currency)
            {
                //return CurrToStr(value, uppercase, currency);
                return CurrToStr((double)value, uppercase, currency);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">This parameter will be used if the value parameter ends with 1.</param>
            /// <param name="two">This parameter will be used if the value parameter ends with 2,3,4.</param>
            /// <param name="five">This parameter will be used if the value parameter ends with other numbers and values which ends with numbers between 10 and 20.</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(decimal value, string one, string two, string five)
            {
                return Decline((long)value, one, two, five);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">This parameter will be used if the value parameter ends with 1.</param>
            /// <param name="two">This parameter will be used if the value parameter ends with 2,3,4.</param>
            /// <param name="five">This parameter will be used if the value parameter ends with other numbers and values which ends with numbers between 10 and 20.</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(double value, string one, string two, string five)
            {
                return Decline((long)value, one, two, five);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">This parameter will be used if the value parameter ends with 1.</param>
            /// <param name="two">This parameter will be used if the value parameter ends with 2,3,4.</param>
            /// <param name="five">This parameter will be used if the value parameter ends with other numbers and values which ends with numbers between 10 and 20.</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(long value, string one, string two, string five)
            {
                long units = value % 100;

                if (units >= 10 && units < 20) return five;
                units = units % 10;

                if (units == 1) return one;
                else if (units > 1 && units < 5) return two;
                return five;
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns correct declension of rubles or kopecs of the currency. Depends on the value parameter.</returns>
            public static string Decline(decimal value, string currency)
            {
                return Decline(value, currency, false);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(double value, string currency)
            {
                return Decline(value, currency, false);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the correct declension of roubles or kopecs names of the currency.</returns>
            public static string Decline(long value, string currency)
            {
                return Decline(value, currency, false);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then the result of the function is the name of cents in correct declension. If this parameter is false then the result of the function is the name of dollars in correct declension. Depends on the currency parameter.</param>
            /// <returns>Returns the correct declention of roubles or kopecs names of the currency.</returns>
            public static string Decline(decimal value, string currency, bool cents)
            {
                return Decline((long)value, currency, cents);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then the result of the function is the name of cents in correct declension. If this parameter is false then the result of the function is the name of dollars in correct declension. Depends on the currency parameter.</param>
            /// <returns>Returns the corect declension of roubles or kopecs names of the currency.</returns>
            public static string Decline(double value, string currency, bool cents)
            {
                return Decline((long)value, currency, cents);
            }

            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then the result of the function is the name of cents in correct declension. If this parameter is false then the result of the function is the name of dollars in correct declension. Depends on the currency parameter.</param>
            /// <returns>Returns the corect declension of roubles or kopecs names of the currency.</returns>
            public static string Decline(long value, string currency, bool cents)
            {
                Currency curr = GetCurrency(currency);
                if (cents)
                {
                    return Decline(value, curr.CentOne, curr.CentTwo, curr.CentFive);
                }
                else
                {
                    return Decline(value, curr.DollarOne, curr.DollarTwo, curr.DollarFive);
                }
            }

            /// <summary>
            /// Returns the string representation of the date.
            /// </summary>
            /// <param name="date">A DateTime value containing a date to convert.</param>
            /// <returns>Returns the string representation of the date.</returns>
            public static string DateToStr(DateTime date)
            {
                return DateToStr(date, false);
            }

            /// <summary>
            /// Returns the string representation of the date.
            /// </summary>
            /// <param name="date">A DateTime value containing a date to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the date.</returns>
            public static string DateToStr(DateTime date, bool uppercase)
            {
                StringBuilder sb = new StringBuilder(
                    string.Format("{0} {1} {2}", date.Day, months[date.Month - 1], date.Year));
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);
                return sb.ToString();
            }
            #endregion

            static Ru()
            {
                RegisterCurrency(new EURCurrency(), "EUR");
                RegisterCurrency(new USDCurrency(), "USD");
                RegisterCurrency(new RURCurrency(), "RUR");
                RegisterCurrency(new UAHCurrency(), "UAH");
                RegisterCurrency(new KZTCurrency(), "KZT");
            }

            private Ru()
            {
            }
        }
        #endregion

        #region Ua
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with Ukrainian language speciality.
        /// </summary>
        public sealed class Ua
        {
            #region Currency
            private static Hashtable Currencies = new Hashtable();
            /// <summary>
            /// Registers a new currency.
            /// </summary>
            /// <param name="currency">Specifies currency declension.</param>
            /// <param name="currencyName">Sets the currency name.</param>
            public static void RegisterCurrency(Currency currency, string currencyName)
            {
                Currencies[currencyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture)] = currency;
            }


            private static Currency GetCurrency(string currencyName)
            {
                Currency currency = Currencies[currencyName.ToUpper(System.Globalization.CultureInfo.InvariantCulture)] as Currency;
                if (currency == null)
                    throw new ArgumentException(string.Format("Currency '{0}' is not registered", currencyName));
                return currency;
            }

            /// <summary>
            /// An abstract class for defining a currency.
            /// </summary>
            public abstract class Currency : BaseCurrency
            {
                protected abstract string[] Dollars
                {
                    get;
                }


                protected abstract string[] Cents
                {
                    get;
                }


                /// <summary>
                /// Gets the currency name in singular (e. g. dollar).
                /// </summary>
                public string DollarOne
                {
                    get
                    {
                        return Dollars[0];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural (e. g. dollars).
                /// </summary>
                public string DollarTwo
                {
                    get
                    {
                        return Dollars[1];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural in correct declension (e. g. dollars).
                /// </summary>
                public string DollarFive
                {
                    get
                    {
                        return Dollars[2];
                    }
                }

                /// <summary>
                /// Gets the currency name in singular (e. g. cent).
                /// </summary>
                public string CentOne
                {
                    get
                    {
                        return Cents[0];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural (e. g. cents).
                /// </summary>
                public string CentTwo
                {
                    get
                    {
                        return Cents[1];
                    }
                }

                /// <summary>
                /// Gets the currency name in plural in correct declension (e. g. cents).
                /// </summary>
                public string CentFive
                {
                    get
                    {
                        return Cents[2];
                    }
                }

            }

            /// <summary>
            /// Class specifies the Russian currency.
            /// </summary>
            public class RURCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }
                /// <summary>
                /// Gets gender of the decimal part of the currency.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }


                private string[] dollars = new string[] { "рубль", "рубля", "рублів" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "копійка", "копійки", "копійок" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }


            /// <summary>
            /// Class specifies the USD currency.
            /// </summary>
            public class USDCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the dollars.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }
                /// <summary>
                /// Gets gender of the cents.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }


                private string[] dollars = new string[] { "долар", "долара", "доларів" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "цент", "цента", "центів" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }
            /// <summary>
            /// Class specifies Euro currency.
            /// </summary>
            public class EURCurrency : Currency
            {
                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Neutral;
                    }
                }
                /// <summary>
                /// Gets gender of the decimal part of the currency.
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Masculine;
                    }
                }


                private string[] dollars = new string[] { "євро", "євро", "євро" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "цент", "цента", "центів" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }

            /// <summary>
            /// Class defines Ukrain currency.
            /// </summary>
            public class UAHCurrency : Currency
            {

                /// <summary>
                /// Gets gender of the integer part of the currency.
                /// </summary>
                public override Gender Gender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }

                /// <summary>
                /// Gets gender of the decimal part of the currency.                
                /// </summary>
                public override Gender CentsGender
                {
                    get
                    {
                        return Gender.Feminine;
                    }
                }


                private string[] dollars = new string[] { "гривня", "гривні", "гривень" };
                protected override string[] Dollars
                {
                    get
                    {
                        return dollars;
                    }
                }


                private string[] cents = new string[] { "копійка", "копійки", "копійок" };
                protected override string[] Cents
                {
                    get
                    {
                        return cents;
                    }
                }
            }
            #endregion

            #region Fields
            private static string[] months = new string[]
			{
				"січня", "лютого", "березня", "квітня", "травня", "червня", 
				"липня", "серпня", "вересня", "жовтня", "листопада", "грудня"
			};
            private static string[] units = new string[]
			{
				"один", "два", "три", "чотири", "п'ять",
				"шість", "сім", "вісім", "дев'ять", "десять", "одинадцять",
				"дванадцять", "тринадцять", "чотирнадцять", "п'ятнадцять",
				"шістнадцять", "сімнадцять", "вісімнадцять", "дев'ятнадцять"
			};

            private static string[] tens = new string[]
			{
				"десять", "двадцять", "тридцять", "сорок", "п'ятдесят", 
				"шістдесят", "сімдесят", "вісімдесят", "дев'яносто"
			};
            private static string[] hundreds = new string[]
			{
				"сто", "двісті", "триста", "чотириста", "п'ятсот", 
				"шістсот", "сімсот", "вісімсот", "дев'ятсот"
			};
            private static string[,] gendered = new string[,]
			{
				{ "один", "одна", "одне" },
				{ "два", "дві", "два" }
			};
            #endregion

            #region Methods
            private static void AddUnits(StringBuilder sb, long value, Gender gender)
            {
                if (value != 0)
                {
                    if (sb.Length > 0) sb.Append(" ");
                    if (value < 3)
                    {
                        sb.Append(gendered[value - 1, (int)gender]);
                    }
                    else sb.Append(units[value - 1]);
                }
            }

            private static void AddTens(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(tens[value - 1]);
                }
            }

            private static void AddHundreds(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(hundreds[value - 1]);
                }
            }

            private static void AddThousand(StringBuilder sb, long value, Gender gender)
            {
                AddHundreds(sb, value / 100);

                value = value % 100;

                if (value < 20) AddUnits(sb, value, gender);
                else
                {
                    AddTens(sb, value / 10);
                    AddUnits(sb, value % 10, gender);
                }
            }

            private static void AddRank(StringBuilder sb, ref long rank,
                ref long value, string one, string two, string five, Gender gender)
            {
                long rankValue = value / rank;

                if (rankValue > 0)
                {
                    AddThousand(sb, rankValue, gender);

                    long units = rankValue % 10;

                    string unit = string.Empty;
                    if (units == 1) unit = one;
                    else if (units > 1 && units < 5) unit = two;
                    else unit = five;

                    if (rankValue > 10 && rankValue < 20) unit = five;

                    if (sb.Length > 0) sb.Append(" ");
                    sb.Append(unit);

                    value %= rank;
                }
                rank /= 1000;
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <returns>Returns the string representation of the double value.</returns>
            public static string NumToStr(double value)
            {
                return NumToStr((decimal)value);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the double value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(double value, bool uppercase)
            {
                return NumToStr((long)value, uppercase);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="gender">A Gender parameter on which gender of the resulting value depends.</param>
            /// <returns>Returns the string representation of the double value with the first symbol in the uppercase or not. Gender depends on the gender parameter.</returns>
            public static string NumToStr(double value, bool uppercase, Gender gender)
            {
                return NumToStr((long)value, uppercase, gender);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <returns>Returns the string representation of the decimal value.</returns>
            public static string NumToStr(decimal value)
            {
                return NumToStr((long)value);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(decimal value, bool uppercase)
            {
                return NumToStr((long)value, uppercase);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="gender">A Gender parameter on which gender of the resulting value depends.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not. Gender depends on gender parameter.</returns>
            public static string NumToStr(decimal value, bool uppercase, Gender gender)
            {
                return NumToStr((long)value, uppercase, gender);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <returns>Returns the string representation of the long value.</returns>
            public static string NumToStr(long value)
            {
                return NumToStr(value, true, Gender.Masculine);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the long value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(long value, bool uppercase)
            {
                return NumToStr(value, uppercase, Gender.Masculine);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="gender">A Gender parameter on which gender of the resulting value depends.</param>
            /// <returns>Returns the string representation of the long value with the first symbol in the uppercase or not. Gender depends on gender parameter.</returns>
            public static string NumToStr(long value, bool uppercase, Gender gender)
            {
                StringBuilder sb = new StringBuilder();
                if (value == 0) sb.Append("ноль");
                else
                {
                    if (value < 0)
                    {
                        sb.Append("минус");
                        value = Math.Abs(value);
                    }

                    long rank = 1000000000000000000;
                    AddRank(sb, ref rank, ref value, "квінтильйон", "квінтильйона", "квінтильйонів", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "квадрильйон", "квадрильйона", "квадрильйонів", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "трильйон", "трильйона", "трильйонів", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "мільярд", "мільярда", "мільярдів", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "мільйон", "мільйона", "мільйонів", Gender.Masculine);
                    AddRank(sb, ref rank, ref value, "тисяча", "тисячі", "тисяч", Gender.Feminine);
                    AddThousand(sb, value, gender);

                }
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);

                return sb.ToString();
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the long value with or without cents and the currency name.</returns>
            public static string CurrToStr(long value, bool cents)
            {
                return CurrToStr((decimal)value, "UAH", cents);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the long value with cents and the currency name.</returns>
            public static string CurrToStr(long value, string currency, bool cents)
            {
                return CurrToStr((decimal)value, true, currency, cents);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents and the currency name.</returns>
            public static string CurrToStr(double value, bool cents)
            {
                return CurrToStr(value, "UAH", cents);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents and the currency name.</returns>
            public static string CurrToStr(decimal value, bool cents)
            {
                return CurrToStr(value, "UAH", cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents and the currency name.</returns>
            public static string CurrToStr(double value, string currency, bool cents)
            {
                return CurrToStr((decimal)value, true, currency, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents and the currency name.</returns>
            public static string CurrToStr(decimal value, string currency, bool cents)
            {
                return CurrToStr((decimal)value, true, currency, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(double value, bool uppercase, string currency, bool cents)
            {
                return CurrToStr((decimal)value, uppercase, currency, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(decimal value, bool uppercase, string currency, bool cents)
            {
                decimal integral = Math.Truncate(value);
                long dec = 0;
                if (UseRoundForToCurrencyWordsFunctions)
                {
                    dec = (long)Math.Round((value - (long)integral) * 100);
                    if (dec > 99)
                    {
                        dec = 0;
                        integral++;
                    }
                }
                else
                {
                    dec = (long)((value - (long)integral) * 100);
                }

                string str = string.Format("{0} {1}", NumToStr(integral, uppercase, GetCurrency(currency).Gender), Decline(integral, currency));

                if (cents)
                {
                    str += string.Format(" {0:d2}", dec);
                    str += string.Format(" {0}", Decline(dec, currency, true));
                }
                return str;
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the double value with cents and the currency name.</returns>
            public static string CurrToStr(double value)
            {
                return CurrToStr((decimal)value, "UAH");
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the decimal value with cents and the currency name.</returns>
            public static string CurrToStr(decimal value)
            {
                return CurrToStr((decimal)value, "UAH");
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents and the currency name.</returns>
            public static string CurrToStr(double value, string currency)
            {
                return CurrToStr((decimal)value, true, currency);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents and the currency name.</returns>
            public static string CurrToStr(decimal value, string currency)
            {
                return CurrToStr((decimal)value, true, currency);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the double value with cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(double value, bool uppercase, string currency)
            {
                return CurrToStr((decimal)value, uppercase, currency, true);
            }


            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the string representation of the decimal value with cents with the first symbol in the uppercase or not and the currency name.</returns>
            public static string CurrToStr(decimal value, bool uppercase, string currency)
            {
                return CurrToStr((double)value, uppercase, currency);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">This parameter will be used if the value parameter ends with 1.</param>
            /// <param name="two">This parameter will be used if the value parameter ends with 2,3,4.</param>
            /// <param name="five">This parameter will be used if the value parameter ends with other numbers and values which ends with numbers between 10 and 20.</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(decimal value, string one, string two, string five)
            {
                return Decline((long)value, one, two, five);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">This parameter will be used if the value parameter ends with 1.</param>
            /// <param name="two">This parameter will be used if the value parameter ends with 2,3,4.</param>
            /// <param name="five">This parameter will be used if the value parameter ends with other numbers and values which ends with numbers between 10 and 20.</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(double value, string one, string two, string five)
            {
                return Decline((long)value, one, two, five);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">This parameter will be used if the value parameter ends with 1.</param>
            /// <param name="two">This parameter will be used if the value parameter ends with 2,3,4.</param>
            /// <param name="five">This parameter will be used if the value parameter ends with other numbers and values which ends with numbers between 10 and 20.</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(long value, string one, string two, string five)
            {
                long units = value % 100;

                if (units >= 10 && units < 20) return five;
                units = units % 10;

                if (units == 1) return one;
                else if (units > 1 && units < 5) return two;
                return five;
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns correct declension of rubles or kopecs of the currency. Depends on the value parameter.</returns>
            public static string Decline(decimal value, string currency)
            {
                return Decline(value, currency, false);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the value of one, two or five parameter. Depends on the value parameter.</returns>
            public static string Decline(double value, string currency)
            {
                return Decline(value, currency, false);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <returns>Returns the correct declension of roubles or kopecs names of the currency.</returns>
            public static string Decline(long value, string currency)
            {
                return Decline(value, currency, false);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then the result of the function is the name of cents in correct declension. If this parameter is false then the result of the function is the name of dollars in correct declension. Depends on the currency parameter.</param>
            /// <returns>Returns the correct declention of roubles or kopecs names of the currency.</returns>
            public static string Decline(decimal value, string currency, bool cents)
            {
                return Decline((long)value, currency, cents);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then the result of the function is the name of cents in correct declension. If this parameter is false then the result of the function is the name of dollars in correct declension. Depends on the currency parameter.</param>
            /// <returns>Returns the corect declension of roubles or kopecs names of the currency.</returns>
            public static string Decline(double value, string currency, bool cents)
            {
                return Decline((long)value, currency, cents);
            }


            /// <summary>
            /// Returns the correct declension of names of roubles or kopecs of the currency.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="currency">The name of the currency(RUR, EUR, USD, UAH).</param>
            /// <param name="cents">If this parameter is true then the result of the function is the name of cents in correct declension. If this parameter is false then the result of the function is the name of dollars in correct declension. Depends on the currency parameter.</param>
            /// <returns>Returns the corect declension of roubles or kopecs names of the currency.</returns>
            public static string Decline(long value, string currency, bool cents)
            {
                Currency curr = GetCurrency(currency);
                if (cents)
                {
                    return Decline(value, curr.CentOne, curr.CentTwo, curr.CentFive);
                }
                else
                {
                    return Decline(value, curr.DollarOne, curr.DollarTwo, curr.DollarFive);
                }
            }


            /// <summary>
            /// Returns the string representation of the date.
            /// </summary>
            /// <param name="date">A DateTime value containing a date to convert.</param>
            /// <returns>Returns the string representation of the date.</returns>
            public static string DateToStr(DateTime date)
            {
                return DateToStr(date, false);
            }


            /// <summary>
            /// Returns the string representation of the date.
            /// </summary>
            /// <param name="date">A DateTime value containing a date to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the date.</returns>
            public static string DateToStr(DateTime date, bool uppercase)
            {
                StringBuilder sb = new StringBuilder(
                    string.Format("{0} {1} {2}", date.Day, months[date.Month - 1], date.Year));
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);
                return sb.ToString();
            }
            #endregion

            static Ua()
            {
                RegisterCurrency(new EURCurrency(), "EUR");
                RegisterCurrency(new USDCurrency(), "USD");
                RegisterCurrency(new RURCurrency(), "RUR");
                RegisterCurrency(new UAHCurrency(), "UAH");
            }

            private Ua()
            {
            }
        }
        #endregion

        #region En
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with English language speciality.
        /// </summary>
        public sealed class En
        {
            #region Fields
            private static string[] months = new string[]
			{					
				"January", "February", "March", "April", "May", "June",
				"July", "August", "September", "October", "November", "December"
			};

            private static string[] units = new string[]
			{
				"one", "two", "three", "four", "five", 
				"six", "seven", "eight", "nine", "ten", "eleven", 
				"twelve", "thirteen", "fourteen", "fifteen",
				"sixteen", "seventeen", "eighteen", "nineteen"
			};
            private static string[] tens = new string[]
			{
				"ten",  "twenty", "thirty", "forty", "fifty", 
				"sixty", "seventy", "eighty", "ninety"
			};
            #endregion

            #region Methods
            private static void AddUnits(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(units[value - 1]);
                }
            }


            private static void AddTens(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(tens[value - 1]);
                }
            }


            private static void AddRank(StringBuilder sb, ref long rank,
                ref long value, string unit)
            {
                long rankValue = value / rank;

                if (rankValue > 0)
                {
                    long hundreds = rankValue / 100;
                    long tens = rankValue / 10 % 10;
                    long units = rankValue % 10;

                    if (tens == 1)
                    {
                        tens = 0;
                        units = rankValue % 100;
                    }

                    if (sb.Length > 0)
                    {
                        if (hundreds > 0) sb.Append(" ");
                        else if (tens + units > 0) sb.Append(" and ");
                    }

                    if (hundreds > 0)
                    {
                        AddUnits(sb, hundreds);
                        sb.Append(" hundred");
                        if (tens + units > 0) sb.Append(" and ");
                    }

                    if (tens > 0)
                    {
                        AddTens(sb, tens);
                        if (units > 0) sb.Append("-");
                    }

                    if (units > 0) AddUnits(sb, units);

                    sb.Append(" ");
                    sb.Append(unit);

                    value %= rank;
                }

                rank /= 1000;
            }

            /// <summary>
            /// Returns the correct noun declension.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">If the value ends with "one" then this parameter is the result of the function.</param>
            /// <param name="two">If the value does not end with "one" then this parameter is the result of the function.</param>
            public static string Decline(long value, string one, string two)
            {
                long units = value % 100;

                if (units == 1) return one;
                return two;
            }

            /// <summary>
            /// Returns the correct noun declension.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="showCents">If showCents is true then the resulting string will be "cent" or "cents". If showCents is false resulting string will be "dollar" or "dollars".</param>
            /// <param name="dollars">A string in format "dollar/dollars" ("pound/pounds"), etc.</param>
            /// <param name="cents">If parameter is true then cents of the value will be added to the resulting string.</param>
            public static string Decline(long value, bool showCents, string dollars, string cents)
            {
                if (showCents)
                {
                    string[] strCents = cents.Split(new char[] { '/' });
                    return Decline(value, strCents[0], strCents[1]);
                }
                else
                {
                    string[] strDollars = dollars.Split(new char[] { '/' });
                    return Decline(value, strDollars[0], strDollars[1]);
                }
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <returns>Returns the string representation of the long value.</returns>
            public static string NumToStr(long value)
            {
                return NumToStr(value, true);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(long value, bool uppercase)
            {
                StringBuilder sb = new StringBuilder();
                if (value == 0) sb.Append("zero");
                else
                {
                    bool isNegative = false;
                    if (value < 0)
                    {
                        isNegative = true;
                        value = Math.Abs(value);
                    }

                    long rank = 1000000000000000000;
                    AddRank(sb, ref rank, ref value, "quintillion");
                    AddRank(sb, ref rank, ref value, "quadrillion");
                    AddRank(sb, ref rank, ref value, "trillion");
                    AddRank(sb, ref rank, ref value, "billion");
                    AddRank(sb, ref rank, ref value, "million");
                    AddRank(sb, ref rank, ref value, "thousand");
                    AddRank(sb, ref rank, ref value, "");

                    if (isNegative) sb.Insert(0, "minus ");
                }
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);

                return sb.ToString();
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the double value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(double value, bool uppercase)
            {
                return NumToStr((long)value, uppercase);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <returns>Returns the string representation of the double value.</returns>
            public static string NumToStr(double value)
            {
                return NumToStr((long)value);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(decimal value, bool uppercase)
            {
                return NumToStr((long)value, uppercase);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <returns>Returns the string representation of the decimal value.</returns>
            public static string NumToStr(decimal value)
            {
                return NumToStr((long)value);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the long value.</returns>
            public static string CurrToStr(long value)
            {
                return CurrToStr((decimal)value);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the long value with or without cents.</returns>
            public static string CurrToStr(long value, bool showCents)
            {
                return CurrToStr((decimal)value, showCents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the long value with or without cents with the first symbol in the uppercase or not.</returns>
            public static string CurrToStr(long value, bool uppercase, bool cents)
            {
                return CurrToStr((decimal)value, uppercase, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the double value with cents.</returns>
            public static string CurrToStr(double value)
            {
                return CurrToStr((decimal)value);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents.</returns>
            public static string CurrToStr(double value, bool showCents)
            {
                return CurrToStr((decimal)value, showCents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="cents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the double value with or without cents with the first symbol in the uppercase or not.</returns>
            public static string CurrToStr(double value, bool uppercase, bool cents)
            {
                return CurrToStr((decimal)value, uppercase, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <returns>Returns the string representation of the decimal value with cents.</returns>
            public static string CurrToStr(decimal value)
            {
                return CurrToStr(value, true, true);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents.</returns>
            public static string CurrToStr(decimal value, bool showCents)
            {
                return CurrToStr(value, true, showCents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents with the first symbol in the uppercase or not.</returns>
            public static string CurrToStr(decimal value, bool uppercase, bool showCents)
            {
                return CurrToStr(value, uppercase, showCents, "dollar/dollars", "cent/cents");
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <param name="dollars">A string in format "dollar/dollars" ("pound/pounds"), etc.</param>
            /// <param name="cents">A string in format "cent/cents" ("penny/pence"), etc.</param>
            /// <returns>Returns the string representation of the long value with or without cents with the first symbol in the uppercase or not. Names of cents and dollars of the currency is to be specified in the dollars and cents parameters.</returns>
            public static string CurrToStr(long value, bool uppercase, bool showCents, string dollars, string cents)
            {
                return CurrToStr((decimal)value, uppercase, showCents, dollars, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <param name="dollars">A string in format "dollar/dollars" ("pound/pounds"), etc.</param>
            /// <param name="cents">A string in format "cent/cents" ("penny/pence"), etc.</param>
            /// <returns>Returns the string representation of the double value with or without cents with the first symbol in the uppercase or not. Names of cents and dollars of the currency is to be specified in the dollars and cents parameters.</returns>
            public static string CurrToStr(double value, bool uppercase, bool showCents, string dollars, string cents)
            {
                return CurrToStr((decimal)value, uppercase, showCents, dollars, cents);
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <param name="dollars">A string in format "dollar/dollars" ("pound/pounds"), etc.</param>
            /// <param name="cents">A string in format "cent/cents" ("penny/pence"), etc.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents with the first symbol in the uppercase or not. Names of cents and dollars of the currency is to be specified in the dollars and cents parameters.</returns>
            public static string CurrToStr(decimal value, bool uppercase, bool showCents, string dollars, string cents)
            {
                decimal integral = Math.Truncate(value);
                long dec = 0;
                if (UseRoundForToCurrencyWordsFunctions)
                {
                    dec = (long)Math.Round((value - (long)integral) * 100);
                    if (dec > 99)
                    {
                        dec = 0;
                        integral++;
                    }
                }
                else
                {
                    dec = (long)((value - (long)integral) * 100);
                }

                string str = NumToStr((long)integral, uppercase);
                if (value == 0) str += " ";
                if (!str.EndsWith(" ")) str += " ";
                str = string.Format("{0}{1}", str, Decline((long)integral, false, dollars, cents));

                if (showCents)
                {
                    str += " and ";
                    str += string.Format("{0}", NumToStr(dec, false));
                    if (dec == 0) str += " ";
                    str += string.Format("{0}", Decline(dec, true, dollars, cents));
                }
                return str;
            }


            /// <summary>
            /// Returns the string representation of the date.
            /// </summary>
            /// <param name="date">A DateTime value containing a date to convert.</param>
            /// <returns>Returns the string representation of the date.</returns>
            public static string DateToStr(DateTime date)
            {
                return DateToStr(date, false);
            }

            /// <summary>
            /// Returns the string representation of the date.
            /// </summary>
            /// <param name="date">A DateTime value containing a date to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the date.</returns>
            public static string DateToStr(DateTime date, bool uppercase)
            {
                StringBuilder sb = new StringBuilder(
                    string.Format("{0} {1} {2}", date.Day, months[date.Month - 1], date.Year));
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);
                return sb.ToString();
            }
            #endregion
        }
        #endregion

        #region Pl
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with Polish language speciality.
        /// </summary>
        public sealed class Pl
        {
            #region Fields
            private static string[] units = new string[]
			{
				"jeden", "dwa", "trzy", "cztery", "pięć", 
				"sześć", "siedem", "osiem", "dziewięć", "dziesięć", "jedenaście", 
				"dwanaście", "trzynaście", "czternaście", "piętnaście",
				"szesnaście", "siedemnaście", "osiemnaście", "dziewiętnaście"
			};
            private static string[] tens = new string[]
			{
				"dziesięć",  "dwadzieścia", "trzydzieści", "czterdzieści", "pięćdziesiąt", 
				"sześćdziesiąt", "siedemdziesiąt", "osiemdziesiąt", "dziewięćdziesiąt"
			};
            private static string[] hundreds = new string[]
			{
				"sto",  "dwieście", "trzysta", "czterysta", "pięćset", 
				"sześćset", "siedemset", "osiemset", "dziewięćset"
			};
            private static string[] thousends = new string[]
			{
				"tysiąc",  "tysiące", "tysięcy"
			};
            private static string[] million = new string[]
			{
				"milion",  "miliony", "milionów"
			};
            private static string[] billion = new string[]
			{
				"miliard",  "miliardy", "miliardów"
			};
            private static string[] trillion = new string[]
			{
				"bilion",  "biliony", "bilionów"
			};
            private static string[] quadrillion = new string[]
			{
				"biliard",  "biliardy", "biliardów"
			};
            private static string[] quintillion = new string[]
			{
				"trylion",  "tryliony", "trylionów"
			};

            private static string[] zloty = new string[]
			{
				"złoty",  "złote", "złotych"
			};
            private static string[] grosz = new string[]
			{
				"grosz",  "grosze", "groszy"
			};
            private static string[] dollar = new string[]
			{
				"dolar",  "dolary", "dolarów"
			};
            private static string[] cent = new string[]
			{
				"cent",  "centy", "centów"
			};
            private static string[] euro = new string[]
            {
                "euro", "euro", "euro"
            };

            private static string[] months = new string[]
			{					
				"Styczeń", "Luty", "Marzec", "Kwiecień", "Maj", "Czerwiec",
				"Lipiec", "Sierpień", "Wrzesień", "Październik", "Listopad", "Grudzień"
			};
            #endregion

            #region Methods
            public static string NumToStr(long value, bool uppercase)
            {
                StringBuilder sb = new StringBuilder();
                if (value == 0) sb.Append("zero ");
                else
                {
                    bool isNegative = false;
                    if (value < 0)
                    {
                        isNegative = true;
                        value = Math.Abs(value);
                    }

                    long rank = 1000000000000000000;
                    AddRank(sb, ref rank, ref value, quintillion);
                    AddRank(sb, ref rank, ref value, quadrillion);
                    AddRank(sb, ref rank, ref value, trillion);
                    AddRank(sb, ref rank, ref value, billion);
                    AddRank(sb, ref rank, ref value, million);
                    AddRank(sb, ref rank, ref value, thousends);
                    AddRank(sb, ref rank, ref value, null);

                    if (isNegative) sb.Insert(0, "minus ");
                }
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);
                //sb.Remove(sb.Length - 1, 1);
                return sb.ToString().TrimEnd();
            }

            private static void AddUnits(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(units[value - 1]);
                }
            }
            private static void AddTens(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(tens[value - 1]);
                }
            }
            private static void AddHundreds(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(hundreds[value - 1]);
                }
            }
            private static void AddRank(StringBuilder sb, ref long rank, ref long value, string[] unit)
            {
                long rankValue = value / rank;
                if (rankValue > 0)
                {
                    long hundreds = rankValue / 100;
                    long tens = rankValue / 10 % 10;
                    long units = rankValue % 10;

                    if (tens == 1)
                    {
                        tens = 0;
                        units = rankValue % 100;
                    }
                    if (sb.Length > 0)
                    {
                        if (hundreds > 0) sb.Append(" ");
                        else if (tens + units > 0) sb.Append(" ");
                    }
                    if (hundreds > 0)
                    {
                        AddHundreds(sb, hundreds);
                        if (tens + units > 0) sb.Append(" ");
                    }
                    if (tens > 0)
                    {
                        AddTens(sb, tens);
                        if (units > 0) sb.Append(" ");
                    }
                    if (units > 0) AddUnits(sb, units);

                    sb.Append(" ");
                    if (unit != null)
                    {
                        switch (units)
                        {
                            case 0:
                            case 1:
                                if ((tens == 0) && (hundreds == 0)) sb.Append(unit[0]); else sb.Append(unit[2]);
                                break;
                            case 2:
                            case 3:
                            case 4:
                                sb.Append(unit[1]);
                                break;
                            default:
                                sb.Append(unit[2]);
                                break;
                        }
                    }
                    value %= rank;
                }
                rank /= 1000;
            }

            private static string Decline(long value, string[] post)
            {
                long units = value % 10;
                long munits = value / 10;
                switch (units)
                {
                    case 1:
                        return (munits == 1) ? post[2] : post[0];
                    case 2:
                    case 3:
                    case 4:
                        return (munits == 1) ? post[2] : post[1];
                    default:
                        return post[2];
                }
            }
            private static string Decline(long value, bool showCents, string[] dollars, string[] cents)
            {
                if (showCents)
                {
                    return Decline(value, cents);
                }
                else
                {
                    return Decline(value, dollars);
                }
            }
            private static string CurrToStr(decimal value, bool uppercase, bool showCents, string[] dollars, string[] cents)
            {
                decimal integral = Math.Truncate(value);
                long dec = 0;
                if (UseRoundForToCurrencyWordsFunctions)
                {
                    dec = (long)Math.Round((value - (long)integral) * 100);
                    if (dec > 99)
                    {
                        dec = 0;
                        integral++;
                    }
                }
                else
                {
                    dec = (long)((value - (long)integral) * 100);
                }

                StringBuilder str = new StringBuilder(Pl.NumToStr((long)integral, uppercase)).Append(" ");
                str.Append(Decline((long)integral, false, dollars, cents));

                if (showCents)
                {
                    str.Append(" i ");
                    str.Append(Pl.NumToStr(dec, false)).Append(" ");
                    str.Append(Decline(dec, true, dollars, cents));
                }
                return str.ToString();
            }

            public static string CurrToStr(decimal value, string currencyISO, bool showCents, bool uppercase)
            {
                switch (currencyISO)
                {
                    case "USD":
                        return CurrToStr(value, uppercase, showCents, dollar, cent);

                    case "EUR":
                        return CurrToStr(value, uppercase, showCents, euro, cent);

                    default:    //"PLN"
                        return CurrToStr(value, uppercase, showCents, zloty, grosz);
                }
            }

            public static string DateToStr(DateTime date, bool uppercase)
            {
                StringBuilder sb = new StringBuilder(
                    string.Format("{0} {1} {2}", date.Day, months[date.Month - 1], date.Year));
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);
                return sb.ToString();
            }
            #endregion
        }
        #endregion

        #region PtBr
        /// <summary>
        /// Class contains methods for the string representation of currency values with Portuguese (Brazil) language speciality.
        /// </summary>
        public sealed class PtBr
        {
            #region Fields
            private static string[] unid = new string[]
			{
                "ZERO ",
                "UM ",
                "DOIS ",
                "TRÊS ",
                "QUATRO ",
                "CINCO ",
                "SEIS ",
                "SETE ",
                "OITO ",
                "NOVE ",
                "DEZ ",
                "ONZE ",
                "DOZE ",
                "TREZE ",
                "CATORZE ", //"QUATOEZE ",
                "QUINZE ",
                "DEZESSEIS ",
                "DEZESSETE ",
                "DEZOITO ",
                "DEZENOVE "
			};

            private static string[] dezena = new string[]
			{
                "ZERO ",
                "DEZ ",
                "VINTE ",
                "TRINTA ",
                "QUARENTA ",
                "CINQÜENTA ",
                "SESSENTA ",
                "SETENTA ",
                "OITENTA ",
                "NOVENTA "
			};

            private static string[] centena = new string[]
			{
                "ZERO ",
                "CENTO ",
                "DUZENTOS ",
                "TREZENTOS ",
                "QUATROCENTOS ",
                "QUINHENTOS ",
                "SEISCENTOS ",
                "SETECENTOS ",
                "OITOCENTOS ",
                "NOVECENTOS "
            };
            #endregion

            #region Methods
            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <returns>Returns the string representation of the decimal value.</returns>
            public static string NumToStr(decimal value)
            {
                string[] grupo = new string[5];
                string[] texto = new string[5];

                string valor = value.ToString("0000000000.00");
                grupo[1] = valor.Substring(1, 3);
                grupo[2] = valor.Substring(4, 3);
                grupo[3] = valor.Substring(7, 3);
                grupo[4] = "0" + valor.Substring(11, 2);

                for (int contador = 1; contador <= 4; contador++)
                {
                    string parte = grupo[contador];
                    int tempInt = int.Parse(parte);
                    int tamanho = (tempInt < 10 ? 1 : (tempInt < 100 ? 2 : (tempInt < 1000 ? 3 : 0)));
                    if (tamanho == 3)
                    {
                        if (!parte.EndsWith("00", StringComparison.InvariantCulture))
                        {
                            texto[contador] += centena[int.Parse(parte.Substring(0, 1))] + "E ";
                            tamanho = 2;
                        }
                        else
                        {
                            texto[contador] += (parte.StartsWith("1", StringComparison.InvariantCulture) ? "CEM " : centena[int.Parse(parte.Substring(0, 1))]);
                        }
                    }
                    if (tamanho == 2)
                    {
                        if (int.Parse(parte.Substring(1, 2)) < 20)
                        {
                            texto[contador] += unid[int.Parse(parte.Substring(1, 2))];
                        }
                        else
                        {
                            texto[contador] += dezena[int.Parse(parte.Substring(1, 1))];
                            if (!parte.EndsWith("0", StringComparison.InvariantCulture))
                            {
                                texto[contador] += "E ";
                                tamanho = 1;
                            }
                        }
                    }
                    if (tamanho == 1)
                    {
                        texto[contador] += unid[int.Parse(parte.Substring(2, 1))];
                    }
                }

                string final = string.Empty;
                if ((int.Parse(grupo[1] + grupo[2] + grupo[3]) == 0) && (int.Parse(grupo[4]) != 0))
                {
                    final = texto[4] + (int.Parse(grupo[4]) == 1 ? "CENTAVO" : "CENTAVOS");
                }
                else
                {
                    final = (int.Parse(grupo[1]) != 0 ? texto[1] + (int.Parse(grupo[1]) > 1 ? "MILHÕES " : "MILHÃO ") : "");
                    if (int.Parse(grupo[2] + grupo[3]) == 0)
                    {
                        final += "DE ";
                    }
                    else
                    {
                        final += (int.Parse(grupo[2]) != 0 ? texto[2] + "MIL " : "");
                    }
                    final += texto[3] + (int.Parse(grupo[1] + grupo[2] + grupo[3]) == 1 ? "REAL " : "REAIS ");
                    final += (int.Parse(grupo[4]) != 0 ? "E " + texto[4] + (int.Parse(grupo[4]) == 1 ? "CENTAVO" : "CENTAVOS") : "");
                }

                string output = string.Empty;
                if (value == 0)
                {
                    output = unid[0];
                }
                else
                {
                    output = final.Trim();
                }

                return output.ToLowerInvariant();
            }
            #endregion
        }
        #endregion

        #region Pt
        /// <summary>
        /// Class contains methods for the string representation of number values with Portuguese language speciality.
        /// </summary>
        public sealed class Pt
        {
            #region Fields
            private static string[] units = new string[]
			{
				"um",
                "dois",
                "três",
                "quatro",
                "cinco", 
				"seis",
                "sete",
                "oito",
                "nove",
                "dez",
                "onze", 
				"doze",
                "treze",
                "catorze",  //quatorze
                "quinze",
				"dezasseis",
                "dezassete",
                "dezoito",
                "dezanove"
			};
            private static string[] tens = new string[]
			{
				"dez",
                "vinte",
                "trinta",
                "quarenta",
                "cinquenta", 
				"sessenta",
                "setenta",
                "oitenta",
                "noventa"
			};
            private static string[] months = new string[]
			{
                "Janeiro",
                "Fevereiro",
                "Março",
                "Abril",
                "Maio",
                "Junho",
                "Julho",
                "Agosto",
                "Setembro",
                "Outubro",
                "Novembro",
                "Dezembro"
            };
            #endregion

            #region Methods
            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <returns>Returns the string representation of the decimal value with the first symbol in the uppercase or not.</returns>
            public static string NumToStr(long value, bool uppercase)
            {
                bool greaterOrEqualThanThousand = value >= 1000;
                StringBuilder sb = new StringBuilder();
                if (value == 0) sb.Append("zero");
                else
                {
                    bool isNegative = false;
                    if (value < 0)
                    {
                        isNegative = true;
                        value = Math.Abs(value);
                    }

                    long rank = 1000000000000000000;
                    AddRank(sb, ref rank, ref value, value.ToString().StartsWith("1") ? "quintilião" : "quintiliões", greaterOrEqualThanThousand);
                    AddRank(sb, ref rank, ref value, value.ToString().StartsWith("1") ? "quadrilião" : "quadriliões", greaterOrEqualThanThousand);
                    AddRank(sb, ref rank, ref value, value.ToString().StartsWith("1") ? "trilião" : "triliões", greaterOrEqualThanThousand);
                    AddRank(sb, ref rank, ref value, value.ToString().StartsWith("1") ? "bilião" : "biliões", greaterOrEqualThanThousand);
                    AddRank(sb, ref rank, ref value, value.ToString().StartsWith("1") ? "milhão" : "milhões", greaterOrEqualThanThousand);
                    AddRank(sb, ref rank, ref value, "mil", greaterOrEqualThanThousand);
                    AddRank(sb, ref rank, ref value, "", greaterOrEqualThanThousand);

                    if (isNegative) sb.Insert(0, "menos ");
                }
                if (uppercase) sb[0] = Char.ToUpper(sb[0]);

                return sb.ToString();
            }

            private static void AddRank(StringBuilder sb, ref long rank,
                ref long value, string unit, bool greaterOrEqualThanThousand)
            {
                long rankValue = value / rank;

                if (rankValue > 0)
                {
                    long hundreds = rankValue / 100;
                    long tens = rankValue / 10 % 10;
                    long units = rankValue % 10;

                    if (tens == 1)
                    {
                        tens = 0;
                        units = rankValue % 100;
                    }

                    if (sb.Length > 0)
                    {
                        if (hundreds > 0)
                        {
                            if (greaterOrEqualThanThousand && tens + units == 0)
                                sb.Append(" e ");
                            else
                                sb.Append(" ");
                        }
                        else if (tens + units > 0) sb.Append(" e ");
                    }

                    if (hundreds > 0)
                    {
                        switch (hundreds)
                        {
                            case 1:
                                if (tens + units > 0)
                                {
                                    sb.Append("cento");
                                }
                                else
                                    sb.Append("cem");
                                break;
                            case 2:
                                sb.Append("duzentos");
                                break;
                            case 3:
                                sb.Append("trezentos");
                                break;
                            case 4:
                                sb.Append("quatrocentos");
                                break;
                            case 5:
                                sb.Append("quinhentos");
                                break;
                            case 6:
                                sb.Append("seiscentos");
                                break;
                            case 7:
                                sb.Append("setecentos");
                                break;
                            case 8:
                                sb.Append("oitocentos");
                                break;
                            case 9:
                                sb.Append("novecentos");
                                break;
                        }

                        if (tens + units > 0)
                        {
                            sb.Append(" e ");
                        }
                    }

                    if (tens > 0)
                    {
                        AddTens(sb, tens);
                        if (unit != String.Empty && units == 0)
                            sb.Append(" ");
                        if (units > 0) sb.Append(" e ");
                    }

                    if ((greaterOrEqualThanThousand && units > 1) || (units > 0 && unit == String.Empty))
                    {
                        AddUnits(sb, units);
                    }

                    if (!(sb.Length == 0 || sb[sb.Length - 1] == ' ')) sb.Append(" ");
                    sb.Append(unit);

                    value %= rank;
                }

                rank /= 1000;
            }

            private static void AddUnits(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(units[value - 1]);
                }
            }

            private static void AddTens(StringBuilder sb, long value)
            {
                if (value != 0)
                {
                    sb.Append(tens[value - 1]);
                }
            }
            /// <summary>
            /// Returns the correct noun declension.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="showCents">If showCents is true then the resulting string will be "cent" or "cents". If showCents is false resulting string will be "dollar" or "dollars".</param>
            /// <param name="dollars">A string in format "dollar/dollars" ("pound/pounds"), etc.</param>
            /// <param name="cents">If parameter is true then cents of the value will be added to the resulting string.</param>
            private static string Decline(long value, bool showCents, string dollars, string cents)
            {
                if (showCents)
                {
                    string[] strCents = cents.Split(new char[] { '/' });
                    return Decline(value, strCents[0], strCents[1]);
                }
                else
                {
                    string[] strDollars = dollars.Split(new char[] { '/' });
                    return Decline(value, strDollars[0], strDollars[1]);
                }
            }

            /// <summary>
            /// Returns the correct noun declension.
            /// </summary>
            /// <param name="value">A value containing a currency on which the resulting value depends.</param>
            /// <param name="one">If the value ends with "one" then this parameter is the result of the function.</param>
            /// <param name="two">If the value does not end with "one" then this parameter is the result of the function.</param>
            private static string Decline(long value, string one, string two)
            {
                long units = value % 100;

                if (units == 1) return one;
                return two;
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a currency to convert.</param>
            /// <param name="uppercase">If this parameter is true then the first symbol of the resulting string will be in the uppercase.</param>
            /// <param name="showCents">If this parameter is true then cents of the value will be added to the resulting string.</param>
            /// <returns>Returns the string representation of the decimal value with or without cents with the first symbol in the uppercase or not.</returns>
            public static string CurrToStr(decimal value, bool uppercase, bool showCents)
            {
                string dollars = "euro/euros";
                string cents = "cêntimo/cêntimos";
                string str = NumToStr((long)value, uppercase);
                if (value == 0) str += " ";
                if (!str.EndsWith(" ")) str += " ";
                str = string.Format("{0}{1}", str, Decline((long)value, false, dollars, cents));

                if (showCents)
                {
                    str += " e ";
                    long dec = 0;
                    if (UseRoundForToCurrencyWordsFunctions)
                    {
                        dec = (long)Math.Round((value - (long)value) * 100);
                    }
                    else
                    {
                        dec = (long)((value - (long)value) * 100);
                    }
                    str += string.Format("{0}", NumToStr(dec, false));
                    if (dec == 0) str += " ";
                    str += string.Format("{0}", Decline(dec, true, dollars, cents));
                }
                return str;
            }

            public static string DateToStr(DateTime value)
            {
                return string.Format("{0} de {1} de {2}", value.Day, months[value.Month - 1], value.Year);
            }
            #endregion
        }
        #endregion

        #region Fr
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with French (Standard) language speciality.
        /// </summary>
        public sealed class Fr
        {
            #region Constants
            const string ZeroWord = "zéro";
            const string LessWord = "moins";
            #endregion

            #region Arrays
            private static string[,] triplets;
            private static string[] lessTwentys;
            private static string[] tens;
            #endregion

            #region Static Constructor
            /// <summary>
            /// fill in arrays containing words
            /// </summary>
            static Fr()
            {
                triplets = new string[7, 2];
                lessTwentys = new string[20];
                tens = new string[10];
                // singulars words used to separate tripplets 
                triplets[0, 0] = "";
                triplets[1, 0] = " mille";
                triplets[2, 0] = " million";
                triplets[3, 0] = " milliard";
                triplets[4, 0] = " trillion";
                triplets[5, 0] = " quadrillion";
                triplets[6, 0] = " qunintillion";
                // plurals words used to separate tripplets 
                triplets[0, 1] = "";
                triplets[1, 1] = " mille";// mille don't take s
                triplets[2, 1] = " millions";
                triplets[3, 1] = " milliards";
                triplets[4, 1] = " trillions";
                triplets[5, 1] = " quadrillions";
                triplets[6, 1] = " qunintillions";
                // word from 1 to 19
                lessTwentys[0] = "";
                lessTwentys[1] = "un";
                lessTwentys[2] = "deux";
                lessTwentys[3] = "trois";
                lessTwentys[4] = "quatre";
                lessTwentys[5] = "cinq";
                lessTwentys[6] = "six";
                lessTwentys[7] = "sept";
                lessTwentys[8] = "huit";
                lessTwentys[9] = "neuf";
                lessTwentys[10] = "dix";
                lessTwentys[11] = "onze";
                lessTwentys[12] = "douze";
                lessTwentys[13] = "treize";
                lessTwentys[14] = "quatorze";
                lessTwentys[15] = "quinze";
                lessTwentys[16] = "seize";
                lessTwentys[17] = "dix-sept";
                lessTwentys[18] = "dix-huit";
                lessTwentys[19] = "dix-neuf";
                // words used as tens headers
                tens[2] = "vingt";
                tens[3] = "trente";
                tens[4] = "quarante";
                tens[5] = "cinquante";
                tens[6] = "soixante";
                tens[7] = "soixante";
                tens[8] = "quatre-vingt";
                tens[9] = "quatre-vingt";
            }
            #endregion

            #region Calculation Methods
            /// <summary>
            /// main calculations including currency, décimals 
            /// </summary>
            /// <param name="number">decimal number to convert</param>
            /// <param name="currencyISO">currency to use in the string</param>
            /// <param name="decimals">number of decimal used</param>
            /// <returns></returns>
            public static string ConvertToWord(decimal number, string currencyISO, int decimals)
            {
                CultureInfo culture = new CultureInfo("fr-FR");
                // evaluate if the number is too long
                if (number > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), number);
                // determinate the two parts of the string
                decimal integerPart = Decimal.Truncate(number);
                decimal decimalPart = Math.Abs((number - integerPart) * (decimal)Math.Pow(10, decimals));
                if (decimalPart > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), decimalPart);
                string integerString = ConvertToWord((long)integerPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "Gender", culture)), 0);
                string decimalString = ConvertToWord((long)decimalPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "CentGender", culture)), 0);
                // determinate the currencies
                string mainCurrency = "";
                string centCurrency = "";
                string postCurrency = "et";
                NumToWordHelper.determinateCurrencies(culture, currencyISO, integerPart, decimalPart,
                  ref mainCurrency, ref centCurrency);
                // construct the final string and return it
                string stringValue = NumToWordHelper.AddWords(
                  integerString, decimalString, mainCurrency, centCurrency, postCurrency);
                return stringValue.Trim();
            }

            /// <summary>
            /// convert a number to its literal representation in French
            /// </summary>
            /// <param name="number">number to convert</param>
            /// <param name="Gender">neutral male or female</param>
            /// <param name="tranche"> uses tranches for thousands to remove s to cent and vingt</param>
            /// <returns></returns>
            private static string ConvertToWord(long number, int Gender, int tranche)
            {
                string result = "";
                bool lessThanZero = (number < 0);
                if (lessThanZero)
                    number = Math.Abs(number);
                if (number == 0)
                    result = ZeroWord;
                else if (number < 20)
                    if (number == 1 && Gender == 2)
                        result = lessTwentys[number] + "e"; // une
                    else
                        result = lessTwentys[number]; // un deux trois ...
                else if (number < 100)
                {
                    string separator = "";
                    switch (number % 10)
                    {
                        case 0: // X0
                            switch (number / 10)
                            {
                                case 7: separator = "-"; break; // 70
                                case 8:
                                    if (tranche != 1)
                                        separator = "s"; break; // 80 takes a s except before mille
                                case 9: separator = "-"; break; // 90
                                default: separator = ""; break; // X0
                            }
                            break;
                        case 1: // X1
                            switch (number / 10)
                            {
                                case 8: separator = " "; break; // 81
                                case 9: separator = " "; break; // 91
                                default: separator = "-et-"; break; // 31 41 ....
                            }
                            break;
                        default: separator = "-"; break; // 62 63 82 83 ....
                    }
                    result = tens[number / 10] + separator;
                    if (number / 10 == 7 || number / 10 == 9)
                    {
                        if (separator == "")
                            separator = " ";
                        result += ConvertToWord((number % 10) + 10, Gender, 0); // 71 72 73 ...
                    }
                    else
                    {
                        if (number % 10 != 0) // 0 -> ""
                            result += ConvertToWord((number % 10), Gender, 0); // 71 72 73 ...
                    }
                }
                else if (number < 1000)
                {
                    switch (number / 100)
                    {
                        case 1: result = "cent"; break; // 100
                        default: // X00
                            result = ConvertToWord(number / 100, Gender, 0) + " cent";
                            if (number % 100 == 0 && tranche != 1) // cent takes s except before mille
                                result += "s";
                            break;
                    }
                    if (number % 100 > 0)
                        result = result + " " + ConvertToWord(number % 100, Gender, 0);
                }
                else
                    result = CalculateOver(number, Gender);

                if (lessThanZero)
                    result = LessWord + " " + result;
                return result.Trim();
            }

            /// <summary>
            /// Calculates numbers over 1 000 000
            /// </summary>
            /// <param name="number">number to calculate</param>
            /// <param name="gender">gender neutral male or female</param>
            /// <returns></returns>
            private static string CalculateOver(long number, int gender)
            {
                string result = "";
                string rightPart = "";
                // calculate the level which depend on the power of the number
                int level = (number.ToString().Length - 1) / 3;
                long power = (long)Math.Pow(10, level * 3);
                if (number % power > 0)
                    rightPart = ConvertToWord(number % power, gender, level);
                long leftNumber = number / power;
                // concatenate 3 first numbers to n*3 last numbers
                switch (leftNumber)
                {
                    case 0: break;// never comme there
                    case 1: // un mille -> mille
                        if (level == 1)
                            result = triplets[level, 0] + " " + rightPart;
                        else
                            result = ConvertToWord(leftNumber, gender, level) + triplets[level, 0] + " " + rightPart;
                        break;
                    default:
                        result = ConvertToWord(leftNumber, gender, level) + triplets[level, 1] + " " + rightPart;
                        break;
                }
                return result;
            }
            #endregion
        }
        #endregion

        #region Es
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with Spanish (Spain Traditional) language speciality.
        /// </summary>
        public sealed class Es
        {
            #region Constants
            const string ZeroWord = "cero";
            const string LessWord = "menos";
            #endregion

            #region Arrays
            private static string[,] triplets;
            private static string[] lessTwenty;
            private static string[] tens;
            private static string[,] currencies = new string[,] { { "dólar", "dólares", "centavo", "centavos" }, { "euro", "euros", "céntimo", "céntimos" } };
            #endregion

            #region Static Constructor
            /// <summary>
            /// fill in arrays containing words
            /// </summary>
            static Es()
            {
                triplets = new string[7, 2];
                lessTwenty = new string[20];
                tens = new string[10];
                // singulars words used to separate tripplets 
                triplets[0, 0] = "";
                triplets[1, 0] = " mil";
                triplets[2, 0] = " millón";
                triplets[3, 0] = " mil milliones";// not used with this algorithm
                triplets[4, 0] = " billón";
                triplets[5, 0] = " mil billón";// not used with this algorithm
                triplets[6, 0] = " trillón";
                // plurals words used to separate tripplets 
                triplets[0, 1] = "";
                triplets[1, 1] = " mil";
                triplets[2, 1] = " millones";
                triplets[3, 1] = " mil milliones";// not used with this algorithm
                triplets[4, 1] = " billónes";
                triplets[5, 1] = " mil billónes";// not used with this algorithm
                triplets[6, 1] = " trillónes";
                // word from 1 to 19
                lessTwenty[0] = "";
                lessTwenty[1] = "uno";
                lessTwenty[2] = "dos";
                lessTwenty[3] = "tres";
                lessTwenty[4] = "cuatro";
                lessTwenty[5] = "cinco";
                lessTwenty[6] = "seis";
                lessTwenty[7] = "siete";
                lessTwenty[8] = "ocho";
                lessTwenty[9] = "nueve";
                lessTwenty[10] = "diez";
                lessTwenty[11] = "once";
                lessTwenty[12] = "doce";
                lessTwenty[13] = "trece";
                lessTwenty[14] = "catorce";
                lessTwenty[15] = "quince";
                lessTwenty[16] = "dieciséis";
                lessTwenty[17] = "diecisiete";
                lessTwenty[18] = "dieciocho";
                lessTwenty[19] = "diecinueve";
                // words used as tens headers
                tens[2] = "veinte";
                tens[3] = "treinta";
                tens[4] = "cuarenta";
                tens[5] = "cincuenta";
                tens[6] = "sesenta";
                tens[7] = "setenta";
                tens[8] = "ochenta";
                tens[9] = "noventa";
            }
            #endregion

            #region Calculation Methods
            /// <summary>
            /// main calculations including currency, décimals 
            /// </summary>
            /// <param name="number">decimal number to convert</param>
            /// <param name="currencyISO">currency to use in the string</param>
            /// <param name="decimals">number of decimal used</param>
            /// <returns></returns>
            public static string ConvertToWord(decimal number, string currencyISO, int decimals)
            {
                CultureInfo culture = new CultureInfo("es-ES");
                // evaluate if the number is too long
                if (number > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), number);
                // determinate the two parts of the string
                decimal integerPart = Decimal.Truncate(number);
                decimal decimalPart = Math.Abs((number - integerPart) * (decimal)Math.Pow(10, decimals));
                if (decimalPart > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), decimalPart);
                string integerString = ConvertToWord((long)integerPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "Gender", culture)), 0);
                string decimalString = ConvertToWord((long)decimalPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "CentGender", culture)), 0);
                // determinate the currencies
                string mainCurrency = "";
                string centCurrency = "";
                string postCurrency = "y";
                NumToWordHelper.determinateCurrencies(culture, currencyISO, integerPart, decimalPart, ref mainCurrency, ref centCurrency);
                determinateCurrenciesEurUsd(culture, currencyISO, integerPart, decimalPart, ref mainCurrency, ref centCurrency);
                // construct the final string and return it
                string stringValue = NumToWordHelper.AddWords(
                  integerString, decimalString, mainCurrency, centCurrency, postCurrency);
                return stringValue.Trim();
            }

            private static void determinateCurrenciesEurUsd(CultureInfo culture, string currencyISO, decimal integerPart, decimal decimalPart, ref string mainCurrency, ref string centCurrency)
            {
                int currencyIndex = -1;
                if (currencyISO == "USD") currencyIndex = 0;
                if (currencyISO == "EUR") currencyIndex = 1;
                if (currencyIndex != -1)
                {
                    // main currency plural or single
                    if (integerPart / 1000000 > 0 && integerPart % 1000000 == 0)
                        mainCurrency = currencies[currencyIndex, 1];    //bigseparator
                    else if (integerPart > 1)
                        mainCurrency = currencies[currencyIndex, 1];
                    else
                        mainCurrency = currencies[currencyIndex, 0];
                    // cent plural or single
                    if (decimalPart > 1)
                        centCurrency = currencies[currencyIndex, 3];
                    else
                        centCurrency = currencies[currencyIndex, 2];
                }
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <param name="value">A value containing a number to convert.</param>
            /// <returns>Returns the string representation of the long value.</returns>
            public static string NumToStr(long number, bool upperCase)
            {
                CultureInfo culture = new CultureInfo("es-ES");
                // evaluate if the number is too long
                if (number > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), number);
                string integerString = ConvertToWord(number, 1, 0);
                if (upperCase && integerString.Length > 1)
                {
                    integerString = Char.ToUpperInvariant(integerString[0]) + integerString.Substring(1);
                }
                return integerString.Trim();
            }

            /// <summary>
            /// Converts the specified value to its equivalent string representation.
            /// </summary>
            /// <returns>Returns the string representation of the long value.</returns>
            public static string NumToStr(long number, bool upperCase, bool female)
            {
                CultureInfo culture = new CultureInfo("es-ES");
                // evaluate if the number is too long
                if (number > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), number);
                string integerString = ConvertToWord(number, female ? 2 : 1, 0);
                if (upperCase && integerString.Length > 1)
                {
                    integerString = Char.ToUpperInvariant(integerString[0]) + integerString.Substring(1);
                }
                return integerString.Trim();
            }

            /// <summary>
            /// convert a number to its literal representation in French
            /// </summary>
            /// <param name="number">number to convert</param>
            /// <param name="Gender">neutral male or female</param>
            /// <param name="tranche"> uses tranches for thousands to remove s to cent and vingt</param>
            /// <returns></returns>
            private static string ConvertToWord(long number, int Gender, int tranche)
            {
                string result = "";
                bool lessThanZero = (number < 0);
                if (lessThanZero)
                    number = Math.Abs(number);
                if (number == 0)
                    result = ZeroWord;

                else if (number < 20)
                    if (number == 1 && Gender == 2) // una
                        result = "una";
                    else
                        result = lessTwenty[number]; // uno dos tres ....
                else if (number < 100)
                {
                    if (number % 10 == 0) // 0 -> ""
                        result = tens[number / 10];
                    else
                        if (number / 10 == 2)
                            result = "veinti" + ConvertToWord((number % 10), Gender, 0); // 2X
                        else
                            result = tens[number / 10] + " y " + ConvertToWord((number % 10), Gender, 0); // 71 72 73 ...
                }
                else if (number < 1000) // XXX
                {
                    switch (number / 100)
                    {
                        case 1: result = number == 100 ? "cien" : "ciento"; break;
                        case 5: result = Gender == 1 ? "quinientos" : "quinientas"; break;
                        case 7: result = Gender == 1 ? "setecientos" : "setecientas"; break;
                        case 9: result = Gender == 1 ? "novecientos" : "novecientas"; break;
                        default: result = ConvertToWord(number / 100, Gender, 0) + (Gender == 1 ? "cientos" : "cientas"); break;
                    }
                    if (number % 100 > 0)
                        result = result + " " + ConvertToWord(number % 100, Gender, 0);
                }
                else if (number < 1000000)
                    if (number / 1000 == 1)
                        result = triplets[1, 0] + (number % 1000 == 0 ? "" : " " + ConvertToWord(number % 1000, Gender, 0));
                    else
                        result = ConvertToWord(number / 1000, 0, 1) + triplets[1, 1] + (number % 1000 == 0 ? "" : " " + ConvertToWord(number % 1000, Gender, 0));
                else if (number < 1000000000000)
                    if (number / 1000000 == 1)
                        result = ConvertToWord(number / 1000000, 0, 2) + triplets[2, 0] + (number % 1000000 == 0 ? "" : " " + ConvertToWord(number % 1000000, Gender, 0));
                    else
                        result = ConvertToWord(number / 1000000, 0, 2) + triplets[2, 1] + (number % 1000000 == 0 ? "" : " " + ConvertToWord(number % 1000000, Gender, 0));
                else if (number < 1000000000000000000)
                    if (number / 1000000000000 == 1)
                        result = ConvertToWord(number / 1000000000000, 0, 2) + triplets[4, 0] + (number % 1000000000000 == 0 ? "" : " " + ConvertToWord(number % 1000000000000, Gender, 0));
                    else
                        result = ConvertToWord(number / 1000000000000, 0, 2) + triplets[4, 1] + (number % 1000000000000 == 0 ? "" : " " + ConvertToWord(number % 1000000000000, Gender, 0));
                else // very big numbers
                    if (number / 1000000000000000000 == 1)
                        result = ConvertToWord(number / 1000000000000000000, 0, 2) + triplets[6, 0] + (number % 1000000000000000000 == 0 ? "" : " " + ConvertToWord(number % 1000000000000000000, Gender, 0));
                    else
                        result = ConvertToWord(number / 1000000000000000000, 0, 2) + triplets[6, 1] + (number % 1000000000000000000 == 0 ? "" : " " + ConvertToWord(number % 1000000000000000000, Gender, 0));
                if (lessThanZero)
                    result = LessWord + " " + result;
                return result.Trim();
            }
            #endregion
        }
        #endregion

        #region Fa
        /// <summary>
        /// Class contains methods for the string representation of numbers with Persian (Farsi) language speciality.
        /// </summary>
        public sealed class Fa
        {
            #region ConvertToWord
            /// <summary>
            /// Convert long number to string
            /// </summary>
            /// <param name="Number">long number to convert</param>
            /// <returns>resulting string</returns>
            public static string ConvertToWord(long Number)
            {
                List<long> Num = new List<long>();
                List<string> Word = new List<string>();
                string Text = string.Empty;

                Number = Math.Abs(Number);
                if (Number == 0)
                {
                    return "صفر";
                }
                while (true)
                {
                    long A = 0;
                    long B = 0;
                    A = Number / 1000;
                    B = Number % 1000;
                    Num.Add(B);
                    if (A >= 1000)
                    {
                        Number = A;
                    }
                    else
                    {
                        if (A != 0)
                        {
                            Num.Add(A);
                        }
                        break;
                    }
                }

                for (int I = 0; I <= Num.Count - 1; I++)
                {
                    Word.Add(ChangingNum(Num[I]));
                }

                for (int Counter = Word.Count - 1; Counter >= 0; Counter += -1)
                {
                    if (Counter == 5)
                    {
                        if (!string.IsNullOrEmpty(Word[5]))
                        {
                            if (!string.IsNullOrEmpty(Word[4]) || !string.IsNullOrEmpty(Word[3]) || !string.IsNullOrEmpty(Word[2]) || !string.IsNullOrEmpty(Word[1]) || !string.IsNullOrEmpty(Word[0]))
                            {
                                Text += Word[5] + " بيليارد و ";
                            }
                            else
                            {
                                Text += Word[5] + " بيليارد";
                                break;
                            }
                        }
                    }
                    else if (Counter == 4)
                    {
                        if (!string.IsNullOrEmpty(Word[4]))
                        {
                            if (!string.IsNullOrEmpty(Word[3]) || !string.IsNullOrEmpty(Word[2]) || !string.IsNullOrEmpty(Word[1]) || !string.IsNullOrEmpty(Word[0]))
                            {
                                Text += Word[4] + " بيليون و ";
                            }
                            else
                            {
                                Text += Word[4] + " بيليون";
                                break;
                            }
                        }
                    }
                    else if (Counter == 3)
                    {
                        if (!string.IsNullOrEmpty(Word[3]))
                        {
                            if (!string.IsNullOrEmpty(Word[2]) || !string.IsNullOrEmpty(Word[1]) || !string.IsNullOrEmpty(Word[0]))
                            {
                                Text += Word[3] + " ميليارد و ";
                            }
                            else
                            {
                                Text += Word[3] + " ميليارد";
                                break;
                            }
                        }
                    }
                    else if (Counter == 2)
                    {
                        if (!string.IsNullOrEmpty(Word[2]))
                        {
                            if (!string.IsNullOrEmpty(Word[1]) || !string.IsNullOrEmpty(Word[0]))
                            {
                                Text += Word[2] + " ميليون و ";
                            }
                            else
                            {
                                Text += Word[2] + " ميليون";
                                break;
                            }
                        }
                    }
                    else if (Counter == 1)
                    {
                        if (!string.IsNullOrEmpty(Word[1]))
                        {
                            if (!string.IsNullOrEmpty(Word[0]))
                            {
                                Text += Word[1] + " هزار و ";
                            }
                            else
                            {
                                Text += Word[1] + " هزار";
                                break;
                            }
                        }
                    }
                    else
                    {
                        Text += Word[0];
                    }
                }

                return Text;
            }

            #region private ChangingNum
            private static string ChangingNum(long Number)
            {
                List<string> N = new List<string>();
                string Yekan = string.Empty;
                string Dahgan = string.Empty;
                string Sadgan = string.Empty;
                string Value = string.Empty;

                while (true)
                {
                    int A = 0;
                    int B = 0;
                    A = (int)(Number / 10);
                    B = (int)(Number % 10);
                    N.Add(B.ToString());
                    if (A >= 10)
                    {
                        Number = A;
                    }
                    else
                    {
                        N.Add(A.ToString());
                        break;
                    }
                }

                if (N.Count == 3)
                {
                    switch (N[2])
                    {
                        case "0":
                            Sadgan = "";
                            break;
                        case "1":
                            Sadgan = "صد";
                            break;
                        case "2":
                            Sadgan = "دويست";
                            break;
                        case "3":
                            Sadgan = "سيصد";
                            break;
                        case "4":
                            Sadgan = "چهارصد";
                            break;
                        case "5":
                            Sadgan = "پانصد";
                            break;
                        case "6":
                            Sadgan = "ششصد";
                            break;
                        case "7":
                            Sadgan = "هفتصد";
                            break;
                        case "8":
                            Sadgan = "هشتصد";
                            break;
                        case "9":
                            Sadgan = "نهصد";
                            break;
                    }
                }

                switch (N[0])
                {
                    case "0":
                        Yekan = "";
                        break;
                    case "1":
                        Yekan = "يک";
                        break;
                    case "2":
                        Yekan = "دو";
                        break;
                    case "3":
                        Yekan = "سه";
                        break;
                    case "4":
                        Yekan = "چهار";
                        break;
                    case "5":
                        Yekan = "پنج";
                        break;
                    case "6":
                        Yekan = "شش";
                        break;
                    case "7":
                        Yekan = "هفت";
                        break;
                    case "8":
                        Yekan = "هشت";
                        break;
                    case "9":
                        Yekan = "نه";
                        break;
                }

                switch (N[1])
                {
                    case "0":
                        Dahgan = "";
                        break;
                    case "1":
                        switch (N[0])
                        {
                            case "0":
                                Yekan = "ده";
                                break;
                            case "1":
                                Yekan = "يازده";
                                break;
                            case "2":
                                Yekan = "دوازده";
                                break;
                            case "3":
                                Yekan = "سيزده";
                                break;
                            case "4":
                                Yekan = "چهارده";
                                break;
                            case "5":
                                Yekan = "پانزده";
                                break;
                            case "6":
                                Yekan = "شانزده";
                                break;
                            case "7":
                                Yekan = "هفده";
                                break;
                            case "8":
                                Yekan = "هيجده";
                                break;
                            case "9":
                                Yekan = "نوزده";
                                break;
                        }
                        break;

                    case "2":
                        Dahgan = "بيست";
                        break;
                    case "3":
                        Dahgan = "سي";
                        break;
                    case "4":
                        Dahgan = "چهل";
                        break;
                    case "5":
                        Dahgan = "پنجاه";
                        break;
                    case "6":
                        Dahgan = "شصت";
                        break;
                    case "7":
                        Dahgan = "هفتاد";
                        break;
                    case "8":
                        Dahgan = "هشتاد";
                        break;
                    case "9":
                        Dahgan = "نود";
                        break;
                }

                if (!string.IsNullOrEmpty(Sadgan))
                {
                    Value += Sadgan;
                    if (!string.IsNullOrEmpty(Dahgan))
                    {
                        Value += " و " + Dahgan;
                        if (!string.IsNullOrEmpty(Yekan))
                        {
                            Value += " و " + Yekan;
                        }
                    }
                    else if (!string.IsNullOrEmpty(Yekan))
                    {
                        Value += " و " + Yekan;
                    }
                }
                else if (!string.IsNullOrEmpty(Dahgan))
                {
                    Value += Dahgan;
                    if (!string.IsNullOrEmpty(Yekan))
                    {
                        Value += " و " + Yekan;
                    }
                }
                else
                {
                    Value += Yekan;
                }

                return Value;
            }
            #endregion

            #endregion
        }
        #endregion

        #region Nl
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with Dutch (Standard) language speciality.
        /// </summary>
        public sealed class Nl
        {
            #region Constants
            const string ZeroWord = "nul";
            const string LessWord = "minus";
            #endregion

            #region Arrays
            private static string[,] triplets;
            private static string[] lessTwenty;
            private static string[] tens;
            #endregion

            #region Static Constructor
            /// <summary>
            /// fill in arrays containing words
            /// </summary>
            static Nl()
            {
                triplets = new string[7, 2];
                lessTwenty = new string[20];
                tens = new string[10];
                // singulars words used to separate tripplets 
                triplets[0, 0] = "";
                triplets[1, 0] = " duizend";
                triplets[2, 0] = " miljoen";
                triplets[3, 0] = " miljard";
                triplets[4, 0] = " biljoen";
                triplets[5, 0] = " biljard";
                triplets[6, 0] = " triljoen";
                // plurals words used to separate tripplets 
                triplets[0, 1] = "";
                triplets[1, 1] = " duizend";
                triplets[2, 1] = " miljoen";
                triplets[3, 1] = " miljard";
                triplets[4, 1] = " biljoen";
                triplets[5, 1] = " biljard";
                triplets[6, 1] = " triljoen";
                // word from 1 to 19
                lessTwenty[0] = "";
                lessTwenty[1] = "een";
                lessTwenty[2] = "twee";
                lessTwenty[3] = "drie";
                lessTwenty[4] = "vier";
                lessTwenty[5] = "vijf";
                lessTwenty[6] = "zes";
                lessTwenty[7] = "zeven";
                lessTwenty[8] = "acht";
                lessTwenty[9] = "negen";
                lessTwenty[10] = "tien";
                lessTwenty[11] = "elf";
                lessTwenty[12] = "twaalf";
                lessTwenty[13] = "dertien";
                lessTwenty[14] = "veertien";
                lessTwenty[15] = "vijftien";
                lessTwenty[16] = "zestien";
                lessTwenty[17] = "zeventien";
                lessTwenty[18] = "achtien";
                lessTwenty[19] = "negentien";
                // words used as tens headers
                tens[2] = "twintig";
                tens[3] = "dertig";
                tens[4] = "veertig";
                tens[5] = "vijftig";
                tens[6] = "zestig";
                tens[7] = "zeventig";
                tens[8] = "tachtig";
                tens[9] = "negentig";
            }
            #endregion

            #region Calculation Methods
            /// <summary>
            /// main calculations including currency, décimals 
            /// </summary>
            /// <param name="number">decimal number to convert</param>
            /// <param name="currencyISO">currency to use in the string</param>
            /// <param name="decimals">number of decimal used</param>
            /// <returns></returns>
            public static string ConvertToWord(decimal number, string currencyISO, int decimals)
            {
                CultureInfo culture = new CultureInfo("nl-NL");
                // evaluate if the number is too long
                if (number > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), number);
                // determinate the two parts of the string
                decimal integerPart = Decimal.Truncate(number);
                decimal decimalPart = Math.Abs((number - integerPart) * (decimal)Math.Pow(10, decimals));
                if (decimalPart > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), decimalPart);
                string integerString = ConvertToWord((long)integerPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "Gender", culture)), 0);
                string decimalString = ConvertToWord((long)decimalPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "CentGender", culture)), 0);
                // determinate the currencies
                string mainCurrency = "";
                string centCurrency = "";
                string postCurrency = "en";
                NumToWordHelper.determinateCurrencies(culture, currencyISO, integerPart, decimalPart,
                  ref mainCurrency, ref centCurrency);
                // construct the final string and return it
                string stringValue = NumToWordHelper.AddWords(
                  integerString, decimalString, mainCurrency, centCurrency, postCurrency);
                return stringValue.Trim();
            }

            /// <summary>
            /// convert a number to its literal representation in French
            /// </summary>
            /// <param name="number">number to convert</param>
            /// <param name="Gender">neutral male or female</param>
            /// <param name="tranche"> uses tranches for thousands to remove s to cent and vingt</param>
            /// <returns></returns>
            private static string ConvertToWord(long number, int Gender, int tranche)
            {
                string result = "";
                bool lessThanZero = (number < 0);
                if (lessThanZero)
                    number = Math.Abs(number);
                if (number == 0)
                    result = ZeroWord;
                else if (number < 20)
                    result = lessTwenty[number];
                else if (number < 100)
                {
                    if (number % 10 == 0) // 0 -> ""
                        result = tens[number / 10];
                    else//'een honderd, drie en twintig
                        result = ConvertToWord((number % 10), Gender, 0) + " en " + tens[number / 10]; // 71 72 73 ...
                }
                else if (number < 1000) // XXX
                {
                    result = ConvertToWord(number / 100, Gender, 0) + " Honderd";
                    if (number % 100 > 0)
                        result = result + ", " + ConvertToWord(number % 100, Gender, 0);
                }
                else
                    result = CalculateOver(number, Gender);
                if (lessThanZero)
                    result = LessWord + " " + result;
                return result.Trim();
            }

            /// <summary>
            /// Calculates numbers over 1 000 000
            /// </summary>
            /// <param name="number">number to calculate</param>
            /// <param name="gender">gender neutral male or female</param>
            /// <returns></returns>
            private static string CalculateOver(long number, int gender)
            {
                string result = "";
                string rightPart = "";
                // calculate the level which depend on the power of the number
                int level = (number.ToString().Length - 1) / 3;
                long power = (long)Math.Pow(10, level * 3);
                if (number % power > 0)
                    rightPart = ConvertToWord(number % power, gender, level);
                long leftNumber = number / power;
                // concatenate 3 first numbers to n*3 last numbers
                switch (leftNumber)
                {
                    case 0: break;// never comme there
                    default:
                        result = ConvertToWord(leftNumber, gender, level) +
                          triplets[level, 0] + " " + rightPart;
                        break;
                }
                return result;
            }
            #endregion
        }
        #endregion

        #region EnGb
        /// <summary>
        /// Class contains methods for the string representation of numbers, dates and currency values with English (United Kingdom) language speciality.
        /// </summary>
        public sealed class EnGb
        {
            #region Constants
            const string ZeroWord = "zero";
            const string LessWord = "minus";
            #endregion

            #region Arrays
            private static string[,] triplets;
            private static string[] lessTwenty;
            private static string[] tens;
            #endregion

            #region Static Constructor
            /// <summary>
            /// fill in arrays containing words
            /// </summary>
            static EnGb()
            {
                triplets = new string[7, 2];
                lessTwenty = new string[20];
                tens = new string[10];
                // singulars words used to separate tripplets 
                triplets[0, 0] = "";
                triplets[1, 0] = " thousand";
                triplets[2, 0] = " million";
                triplets[3, 0] = " billion";
                triplets[4, 0] = " trillion";
                triplets[5, 0] = " quadrillion";
                triplets[6, 0] = " qunintillion";
                // plurals words used to separate tripplets 
                triplets[0, 1] = "";
                triplets[1, 1] = " thousands";
                triplets[2, 1] = " millions";
                triplets[3, 1] = " milliards";
                triplets[4, 1] = " trillions";
                triplets[5, 1] = " quadrillions";
                triplets[6, 1] = " qunintillions";
                // word from 1 to 19
                lessTwenty[0] = "";
                lessTwenty[1] = "one";
                lessTwenty[2] = "two";
                lessTwenty[3] = "three";
                lessTwenty[4] = "four";
                lessTwenty[5] = "five";
                lessTwenty[6] = "six";
                lessTwenty[7] = "seven";
                lessTwenty[8] = "eight";
                lessTwenty[9] = "nine";
                lessTwenty[10] = "ten";
                lessTwenty[11] = "eleven";
                lessTwenty[12] = "twelve";
                lessTwenty[13] = "thirteen";
                lessTwenty[14] = "fourteen";
                lessTwenty[15] = "fifteen";
                lessTwenty[16] = "sixteen";
                lessTwenty[17] = "seventeen";
                lessTwenty[18] = "eighteen";
                lessTwenty[19] = "nineteen";
                // words used as tens headers
                tens[2] = "twenty";
                tens[3] = "thirty";
                tens[4] = "fourty";
                tens[5] = "fifty";
                tens[6] = "sixty";
                tens[7] = "seventy";
                tens[8] = "eighty";
                tens[9] = "ninety";
            }
            #endregion

            #region Calculation Methods
            /// <summary>
            /// main calculations including currency, décimals 
            /// </summary>
            /// <param name="number">decimal number to convert</param>
            /// <param name="currencyISO">currency to use in the string</param>
            /// <param name="decimals">number of decimal used</param>
            /// <returns></returns>
            public static string ConvertToWord(decimal number, string currencyISO, int decimals)
            {
                CultureInfo culture = new CultureInfo("en-GB");
                // evaluate if the number is too long
                if (number > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), number);
                // determinate the two parts of the string
                decimal integerPart = Decimal.Truncate(number);
                decimal decimalPart = Math.Abs((number - integerPart) * (decimal)Math.Pow(10, decimals));
                if (decimalPart > NumToWordHelper.MaxValue)
                    throw new NumToWordException(Resource.ResourceManager.GetString("TooLongError", culture), decimalPart);
                string integerString = ConvertToWord((long)integerPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "Gender", culture)), 0);
                string decimalString = ConvertToWord((long)decimalPart,
                  int.Parse(Resource.ResourceManager.GetString(currencyISO + "CentGender", culture)), 0);
                // determinate the currencies
                string mainCurrency = "";
                string centCurrency = "";
                string postCurrency = "and";
                NumToWordHelper.determinateCurrencies(culture, currencyISO, integerPart, decimalPart,
                  ref mainCurrency, ref centCurrency);
                // construct the final string and return it
                string stringValue = NumToWordHelper.AddWords(
                  integerString, decimalString, mainCurrency, centCurrency, postCurrency);
                return stringValue.Trim();
            }

            /// <summary>
            /// convert a number to its literal representation in French
            /// </summary>
            /// <param name="number">number to convert</param>
            /// <param name="Gender">neutral male or female</param>
            /// <param name="tranche"> uses tranches for thousands to remove s to cent and vingt</param>
            /// <returns></returns>
            private static string ConvertToWord(long number, int Gender, int tranche)
            {
                string result = "";
                bool lessThanZero = (number < 0);
                if (lessThanZero)
                    number = Math.Abs(number);
                if (number == 0)
                    result = ZeroWord;
                else if (number < 20)
                    result = lessTwenty[number];
                else if (number < 100)
                {
                    if (number % 10 == 0) // 0 -> ""
                        result = tens[number / 10];
                    else
                        result = tens[number / 10] + "-" + ConvertToWord((number % 10), Gender, 0); // 71 72 73 ...
                }
                else if (number < 1000) // XXX
                {
                    result = ConvertToWord(number / 100, Gender, 0) + " hundred";
                    if (number % 100 > 0)
                        result = result + " and " + ConvertToWord(number % 100, Gender, 0);
                }
                else
                    result = CalculateOver(number, Gender);
                if (lessThanZero)
                    result = LessWord + " " + result;
                return result.Trim();
            }

            /// <summary>
            /// Calculates numbers over 1 000 000
            /// </summary>
            /// <param name="number">number to calculate</param>
            /// <param name="gender">gender neutral male or female</param>
            /// <returns></returns>
            private static string CalculateOver(long number, int gender)
            {
                string result = "";
                string rightPart = "";
                // calculate the level which depend on the power of the number
                int level = (number.ToString().Length - 1) / 3;
                long power = (long)Math.Pow(10, level * 3);
                if (number % power > 0)
                    rightPart = ConvertToWord(number % power, gender, level);
                long leftNumber = number / power;
                // concatenate 3 first numbers to n*3 last numbers
                switch (leftNumber)
                {
                    case 0: break;// never comme there
                    default:
                        result = ConvertToWord(leftNumber, gender, level) +
                          triplets[level, 0] + " " + rightPart;
                        break;
                }
                return result;
            }
            #endregion
        }
        #endregion

        #region EnIn
        /// <summary>
        /// Class contains methods for the string representation of currency values with English(Indian) language speciality.
        /// </summary>
        public sealed class EnIn
        {
            public static string NumberToStr(Int64 value, bool blankIfZero = false)
            {
                string st = NumberToWords(value, blankIfZero);
                TextInfo TxtInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
                return TxtInfo.ToTitleCase(st);
            }

            public static string CurrencyToStr(string currencyBasicUnit, string currencyFractionalUnit, decimal value, int decimalPlaces, bool blankIfZero = false)
            {
                if (System.Convert.ToDouble(value) < 0)
                {
                    throw new Exception("Cannot convert negative numbers.");
                }

                Int64 decimalValue = 0;
                Int64 integerValue = (Int64)value;
                int decmalPlaceholderPosition = value.ToString().Replace(",", ".").LastIndexOf(".");
                if (decmalPlaceholderPosition > 0)
                {
                    string tString = value.ToString().Substring(decmalPlaceholderPosition + 1);
                    tString = tString.PadRight(decimalPlaces, '0');
                    decimalValue = System.Convert.ToInt64(tString);
                }

                string returnString = NumberToWords(integerValue, blankIfZero).Trim();
                string decimalString = NumberToWords(decimalValue, true).Trim();

                if (returnString.Length > 0 && decimalString.Length > 0)
                {
                    returnString = returnString + " and " + currencyFractionalUnit + " " + decimalString;
                }
                else if (returnString.Length == 0 && decimalString.Length > 0)
                {
                    WordsDictionary.TryGetValue("K_0", out returnString);
                    returnString = returnString + " and " + currencyFractionalUnit.ToLower() + " " + decimalString;
                }

                TextInfo TxtInfo = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo;
                return TxtInfo.ToTitleCase(currencyBasicUnit + " " + returnString + " Only");
            }

            private static string NumberToWords(Int64 number, bool blankIfZero)
            {
                string returnValue = "";
                string tString = "";
                Int64 integerValue = 0;
                Int64 decimalValue = 0;

                if (number == 0)
                {
                    WordsDictionary.TryGetValue("K_" + number.ToString(), out returnValue);
                    if (blankIfZero)
                        returnValue = "";
                }
                else if (number >= 1 && number < 20)
                {
                    WordsDictionary.TryGetValue("K_" + number.ToString(), out returnValue);
                }
                else if (number >= 20 && number < 100)
                {
                    integerValue = (number / 10) * 10;
                    WordsDictionary.TryGetValue("K_" + integerValue.ToString(), out returnValue);
                    decimalValue = (number % 10);
                    returnValue = returnValue + " " + NumberToWords(decimalValue, true);
                }
                else if (number >= 100 && number < 1000)
                {
                    integerValue = (number / 100);
                    returnValue = NumberToWords(integerValue, true);
                    WordsDictionary.TryGetValue("K_100", out tString);
                    decimalValue = (number % 100);
                    returnValue = returnValue + " " + tString + " " + NumberToWords(decimalValue, true);
                }
                else if (number >= 1000 && number < 100000)
                {
                    integerValue = (number / 1000);
                    returnValue = NumberToWords(integerValue, true);
                    WordsDictionary.TryGetValue("K_1000", out tString);
                    decimalValue = (number % 1000);
                    returnValue = returnValue + " " + tString + " " + NumberToWords(decimalValue, true);
                }
                else if (number >= 100000 && number < 10000000)
                {
                    integerValue = (number / 100000);
                    returnValue = NumberToWords(integerValue, true);
                    WordsDictionary.TryGetValue("K_100000", out tString);
                    decimalValue = (number % 100000);
                    returnValue = returnValue + " " + tString + " " + NumberToWords(decimalValue, true);
                }
                else if (number >= 10000000 && number < 1000000000)
                {
                    integerValue = (number / 10000000);
                    returnValue = NumberToWords(integerValue, true);
                    WordsDictionary.TryGetValue("K_10000000", out tString);
                    decimalValue = (number % 10000000);
                    returnValue = returnValue + " " + tString + " " + NumberToWords(decimalValue, true);
                }
                else if (number >= 1000000000 && number < 1000000000000000)
                {
                    integerValue = (number / 10000000);
                    returnValue = NumberToWords(integerValue, true);
                    WordsDictionary.TryGetValue("K_10000000", out tString);
                    returnValue = returnValue + " " + tString;
                    tString = "";
                    decimalValue = (number % 10000000);
                    returnValue = returnValue + " " + NumberToWords(decimalValue, true);
                }
                else
                {
                    throw new Exception("number to big to convert.");
                }
                return returnValue;
            }

            private static Dictionary<string, string> _wordsDictionary = null;
            private static Dictionary<string, string> WordsDictionary
            {
                get
                {
                    if (_wordsDictionary == null)
                    {
                        Dictionary<string, string> wordsDictionary = new Dictionary<string, string>();

                        wordsDictionary["K_0"] = "Zero";
                        wordsDictionary["K_1"] = "One";
                        wordsDictionary["K_2"] = "Two";
                        wordsDictionary["K_3"] = "Three";
                        wordsDictionary["K_4"] = "Four";
                        wordsDictionary["K_5"] = "Five";
                        wordsDictionary["K_6"] = "Six";
                        wordsDictionary["K_7"] = "Seven";
                        wordsDictionary["K_8"] = "Eight";
                        wordsDictionary["K_9"] = "Nine";

                        wordsDictionary["K_10"] = "Ten";
                        wordsDictionary["K_11"] = "Eleven";
                        wordsDictionary["K_12"] = "Twelve";
                        wordsDictionary["K_13"] = "Thirteen";
                        wordsDictionary["K_14"] = "Fourteen";
                        wordsDictionary["K_15"] = "Fifteen";
                        wordsDictionary["K_16"] = "Sixteen";
                        wordsDictionary["K_17"] = "Seventeen";
                        wordsDictionary["K_18"] = "Eighteen";
                        wordsDictionary["K_19"] = "Nineteen";

                        wordsDictionary["K_20"] = "Twenty";
                        wordsDictionary["K_30"] = "Thirty";
                        wordsDictionary["K_40"] = "Forty";
                        wordsDictionary["K_50"] = "Fifty";
                        wordsDictionary["K_60"] = "Sixty";
                        wordsDictionary["K_70"] = "Seventy";
                        wordsDictionary["K_80"] = "Eighty";
                        wordsDictionary["K_90"] = "Ninety";

                        wordsDictionary["K_100"] = "Hundred";
                        wordsDictionary["K_1000"] = "Thousand";
                        wordsDictionary["K_100000"] = "Lac";
                        wordsDictionary["K_10000000"] = "Crore";

                        _wordsDictionary = wordsDictionary;
                    }
                    return _wordsDictionary;
                }
            }
        }
        #endregion

        #region NumToWord Helper - for Fr, Es, EnGb, Nl
        /// <summary>
        /// NumToWord Helper
        /// </summary>
        public static class NumToWordHelper
        {
            /// <summary>
            /// Maximum parsable value for integer or decimal part
            /// </summary>
            public const decimal MaxValue = 1000000000000000m;

            /// <summary>
            /// add a word to a string
            /// </summary>
            /// <param name="value">base string</param>
            /// <param name="word">word to add</param>
            /// <param name="separator">separator between the string and the word</param>
            private static void AddWord(ref string value, string word, string separator)
            {
                if (!string.IsNullOrEmpty(word))
                    value = value + separator + word;
            }

            /// <summary>
            /// Concatenate the final string
            /// </summary>
            /// <param name="integerString">integer part</param>
            /// <param name="decimalString">decimal part</param>
            /// <param name="mainCurrency">final string for the main currency</param>
            /// <param name="centCurrency">final string for the cent currency</param>
            /// <param name="postCurrency">word used between intager an decimal part</param>
            /// <returns></returns>
            public static string AddWords(string integerString, string decimalString, string mainCurrency, string centCurrency, string postCurrency)
            {
                string stringValue = "";
                AddWord(ref stringValue, integerString, " ");
                AddWord(ref stringValue, mainCurrency, " ");
                AddWord(ref stringValue, postCurrency, " ");
                AddWord(ref stringValue, decimalString, " ");
                AddWord(ref stringValue, centCurrency, " ");
                return stringValue;
            }

            /// <summary>
            /// Determinate the exact word used for the currencies
            /// </summary>
            /// <param name="culture">the culture from which to get the culture</param>
            /// <param name="currencyISO">ISO currency code</param>
            /// <param name="integerPart">integer Part of the number</param>
            /// <param name="decimalPart">decimal Part of the number</param>
            /// <param name="mainCurrency">return the exact word to use for the main currency</param>
            /// <param name="centCurrency">return the exact word to use for the cent currency</param>
            public static void determinateCurrencies(CultureInfo culture, string currencyISO, decimal integerPart, decimal decimalPart, ref string mainCurrency, ref string centCurrency)
            {
                // main currency plural or single
                if (integerPart / 1000000 > 0 && integerPart % 1000000 == 0)
                    mainCurrency = Resource.ResourceManager.GetString(currencyISO + "BigSeparator", culture);
                else if (integerPart > 1)
                    mainCurrency = Resource.ResourceManager.GetString(currencyISO + "Plural", culture);
                else
                    mainCurrency = Resource.ResourceManager.GetString(currencyISO + "Single", culture);
                // cent plural or single
                if (decimalPart > 1)
                    centCurrency = Resource.ResourceManager.GetString(currencyISO + "CentPlural", culture);
                else
                    centCurrency = Resource.ResourceManager.GetString(currencyISO + "CentSingle", culture);
            }
        }
        #endregion

        #region NumToWordException - for Fr, Es, EnGb, Nl
        /// <summary>
        /// Exception raised when the number can't be converted
        /// </summary>
        public class NumToWordException : Exception
        {
            /// <summary>
            /// NumToWordException constructor
            /// </summary>
            /// <param name="message">message to show</param>
            /// <param name="number">number to show</param>
            public NumToWordException(string message, decimal number)
                : base(message + " : " + number.ToString())
            {
            }
        }
        #endregion

        #region Resource
        /// <summary>
        ///   A strongly-typed resource class, for looking up localized strings, etc.
        /// </summary>
        // This class was auto-generated by the StronglyTypedResourceBuilder
        // class via a tool like ResGen or Visual Studio.
        // To add or remove a member, edit your .ResX file then rerun ResGen
        // with the /str option, or rebuild your VS project.
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        internal class Resource
        {

            private static global::System.Resources.ResourceManager resourceMan;

            private static global::System.Globalization.CultureInfo resourceCulture;

            [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
            internal Resource()
            {
            }

            /// <summary>
            ///   Returns the cached ResourceManager instance used by this class.
            /// </summary>
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
            internal static global::System.Resources.ResourceManager ResourceManager
            {
                get
                {
                    if (object.ReferenceEquals(resourceMan, null))
                    {
                        global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Stimulsoft.Report.Resource", typeof(Resource).Assembly);
                        resourceMan = temp;
                    }
                    return resourceMan;
                }
            }

            /// <summary>
            ///   Overrides the current thread's CurrentUICulture property for all
            ///   resource lookups using this strongly typed resource class.
            /// </summary>
            [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
            internal static global::System.Globalization.CultureInfo Culture
            {
                get
                {
                    return resourceCulture;
                }
                set
                {
                    resourceCulture = value;
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to d&apos;euros.
            /// </summary>
            internal static string EURBigSeparator
            {
                get
                {
                    return ResourceManager.GetString("EURBigSeparator", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to 1.
            /// </summary>
            internal static string EURCentGender
            {
                get
                {
                    return ResourceManager.GetString("EURCentGender", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to cents.
            /// </summary>
            internal static string EURCentPlural
            {
                get
                {
                    return ResourceManager.GetString("EURCentPlural", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to cent.
            /// </summary>
            internal static string EURCentSingle
            {
                get
                {
                    return ResourceManager.GetString("EURCentSingle", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to 1.
            /// </summary>
            internal static string EURGender
            {
                get
                {
                    return ResourceManager.GetString("EURGender", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to euros.
            /// </summary>
            internal static string EURPlural
            {
                get
                {
                    return ResourceManager.GetString("EURPlural", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to euro.
            /// </summary>
            internal static string EURSingle
            {
                get
                {
                    return ResourceManager.GetString("EURSingle", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to de livres.
            /// </summary>
            internal static string GBPBigSeparator
            {
                get
                {
                    return ResourceManager.GetString("GBPBigSeparator", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to 1.
            /// </summary>
            internal static string GBPCentGender
            {
                get
                {
                    return ResourceManager.GetString("GBPCentGender", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to pence.
            /// </summary>
            internal static string GBPCentPlural
            {
                get
                {
                    return ResourceManager.GetString("GBPCentPlural", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to penny.
            /// </summary>
            internal static string GBPCentSingle
            {
                get
                {
                    return ResourceManager.GetString("GBPCentSingle", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to 2.
            /// </summary>
            internal static string GBPGender
            {
                get
                {
                    return ResourceManager.GetString("GBPGender", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to livres.
            /// </summary>
            internal static string GBPPlural
            {
                get
                {
                    return ResourceManager.GetString("GBPPlural", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to livre.
            /// </summary>
            internal static string GBPSingle
            {
                get
                {
                    return ResourceManager.GetString("GBPSingle", resourceCulture);
                }
            }

            /// <summary>
            ///   Looks up a localized string similar to le nombre est trop grand.
            /// </summary>
            internal static string TooLongError
            {
                get
                {
                    return ResourceManager.GetString("TooLongError", resourceCulture);
                }
            }
        }
        #endregion

        #region Convert
        /// <summary>
        /// Class for converting numbers to its string representation for the numbering of the lists.
        /// </summary>
        public sealed class Convert
        {
            #region Fields
            private static int[] Arabics = { 1, 5, 10, 50, 100, 1000 };
            private static char[] Romans = { 'I', 'V', 'X', 'L', 'C', 'M' };
            private static int[] Subs = { 0, 0, 0, 2, 2, 4 };
            private static char[] ABC = {
											'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 
											'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
										};
            private static char[] ABCRu = {
											'А', 'Б', 'В', 'Г', 'Д', 'Е', 'Ж', 'З', 'И', 'К', 'Л', 'М', 'Н', 
											'О', 'П', 'Р', 'С', 'Т', 'У', 'Ф', 'Х', 'Ц', 'Ч', 'Ш', 'Щ', 'Э', 
                                            'Ю', 'Я'
										};
            #endregion
            /// <summary>
            /// Converts Arabic numerals to Roman.
            /// </summary>
            /// <param name="value">Arabic numerals for converting to the Roman format.</param>
            /// <returns>Returns Arabics numerals in Roman.</returns>
            public static string ToRoman(int value)
            {
                StringBuilder str = new StringBuilder();
                while (value > 0)
                {
                    for (int i = 5; i >= 0; i--)
                    {
                        if (value >= Arabics[i])
                        {
                            str.Append(Romans[i]);
                            value -= Arabics[i];
                            break;
                        }

                        bool flag = false;
                        for (int j = Subs[i]; j < i; j++)
                        {
                            if (Arabics[j] == Arabics[i] - Arabics[j]) continue;
                            if (value >= Arabics[i] - Arabics[j])
                            {
                                str.Append(Romans[j]);
                                str.Append(Romans[i]);
                                value -= Arabics[i] - Arabics[j];
                                flag = true;
                                break;
                            }
                        }
                        if (flag) break;
                    }
                }
                return str.ToString();
            }

            /// <summary>
            /// Converts the number to A B C D representation for numbering of the list.
            /// </summary>
            /// <param name="value">A number for converting into the A B C representation.</param>
            /// <returns>String representation of the value in A B C D format.</returns>
            public static string ToABC(int value)
            {
                if (value < 1) return string.Empty;
                StringBuilder str = new StringBuilder();
                int count = 0;
                while (value > 26)
                {
                    count++;
                    value -= 26;
                }
                return new string(ABC[value - 1], count + 1);
            }

            /// <summary>
            /// Converts the number to A B C D representation for numbering of the list.
            /// </summary>
            /// <param name="value">A number for converting into the A B C representation.</param>
            /// <returns>String representation of the value in A B C D format.</returns>
            public static string ToABCNumeric(int value)
            {
                int count = 0;
                while (value > 26)
                {
                    count++;
                    value -= 26;
                }
                if (count == 0)
                    return ((char)(value + 64)).ToString();
                else
                    return ((char)(value + 64)).ToString() + count.ToString();
            }


            /// <summary>
            /// Converts the number to А Б В representation for numbering of the list in russian.
            /// </summary>
            /// <param name="value">A number for converting into the А Б В representation.</param>
            /// <returns>String representation of the value in А Б В format.</returns>
            public static string ToABCRu(int value)
            {
                if (value < 1) return string.Empty;
                StringBuilder str = new StringBuilder();
                int count = 0;
                while (value > 26)
                {
                    count++;
                    value -= 26;
                }
                return new string(ABCRu[value - 1], count + 1);
            }

            /// <summary>
            /// Converts the number to the arabic representation.
            /// </summary>
            /// <param name="value">A number for converting into the arabic representation.</param>
            /// <param name="useEasternDigits">Use eastern or standard arabic digits.</param>
            /// <returns>String representation of the value with arabic digits.</returns>
            public static string ToArabic(int value, bool useEasternDigits)
            {
                return ToArabic(value.ToString(), useEasternDigits);
            }

            /// <summary>
            /// Converts all digits in the string to the arabic representation.
            /// </summary>
            /// <param name="value">A string for converting into the arabic representation.</param>
            /// <param name="useEasternDigits">Use eastern or standard arabic digits.</param>
            /// <returns>String with arabic digits.</returns>
            public static string ToArabic(string value, bool useEasternDigits)
            {
                StringBuilder sb = new StringBuilder();
                for (int index = 0; index < value.Length; index++)
                {
                    int num = (int)value[index];
                    if ((num >= 0x0030) && (num <= 0x0039))
                    {
                        num += 0x0660 - 0x0030;
                        if (useEasternDigits)
                        {
                            num += 0x06f0 - 0x0660;
                        }
                    }
                    sb.Append((char)num);
                }
                return sb.ToString();
            }

        }
        #endregion

        #region EngineHelper
        /// <summary>
        /// Class contains additional methods for the report rendering.
        /// </summary>
        public sealed class EngineHelper
        {
            /// <summary>
            /// Method writes all the values of the list through the separator as a querry string.
            /// </summary>
            /// <param name="list">List of values</param>
            /// <param name="dateTimeFormat">DateTime format</param>
            /// <returns>Resulting querry string</returns>
            public static string ToQueryString<T>(List<T> list, string quotationMark, string dateTimeFormat)
            {
                StringBuilder sb = new StringBuilder();
                int index = 0;
                foreach (object obj in list)
                {
                    if (index > 0) sb.Append(", ");
                    string st;
                    if (string.IsNullOrEmpty(dateTimeFormat))
                    {
                        st = obj.ToString();
                    }
                    else
                    {
                        st = System.Convert.ToDateTime(obj).ToString(dateTimeFormat);
                    }
                    if (!string.IsNullOrEmpty(quotationMark))
                    {
                        st = st.Replace(quotationMark, quotationMark + quotationMark);
                    }
                    sb.Append(quotationMark + st + quotationMark);
                    index++;
                }
                return sb.ToString();
            }
        }
        #endregion

        #region MonthToStr
        /// <summary>
        /// MonthToStr helper.
        /// </summary>
        public sealed class MonthToStr
        {
            #region Fields
            private static List<string[]> months = null;
            private static List<bool> defaultUpperCaseList = null;
            private static Hashtable cultureIndexes = null;
            #endregion

            public static string MonthName(DateTime dateTime)
            {
                //return System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
                return dateTime.ToString("MMMM");
            }

            public static string MonthName(DateTime dateTime, bool localized)
            {
                if (localized)
                {
                    switch (dateTime.Month)
                    {
                        case 1:
                            return "Январь";
                        case 2:
                            return "Февраль";
                        case 3:
                            return "Март";
                        case 4:
                            return "Апрель";
                        case 5:
                            return "Май";
                        case 6:
                            return "Июнь";
                        case 7:
                            return "Июль";
                        case 8:
                            return "Август";
                        case 9:
                            return "Сентябрь";
                        case 10:
                            return "Октябрь";
                        case 11:
                            return "Ноябрь";
                        case 12:
                            return "Декабрь";
                    }
                }
                return MonthName(dateTime);
            }

            public static string MonthName(DateTime dateTime, string culture)
            {
                int month = dateTime.Month;
                string monthName = string.Empty;

                try
                {
                    //get from system
                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(culture);
                    monthName = ci.DateTimeFormat.GetMonthName(month);
                }
                catch
                {
                    //use internal data
                    string cultureName = culture.ToLowerInvariant();
                    if (!cultureIndexes.ContainsKey(cultureName))
                    {
                        throw new ArgumentException(string.Format("Function \"MonthName\": There is no data for specified culture \"{0}\".", culture));
                    }
                    int index = (int)cultureIndexes[cultureName];
                    if (month < months[index].Length) monthName = months[index][month - 1];
                    if (defaultUpperCaseList[index])
                    {
                        monthName = monthName.Substring(0, 1).ToUpperInvariant() + monthName.Substring(1);
                    }
                }

                return monthName;
            }

            public static string MonthName(DateTime dateTime, string culture, bool upperCase)
            {
                string monthName = MonthName(dateTime, culture).ToLowerInvariant();
                if (upperCase)
                {
                    return monthName.Substring(0, 1).ToUpperInvariant() + monthName.Substring(1);
                }
                return monthName;
            }


            private string GetCulturesList()
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                List<string> mm = new List<string>();

                foreach (CultureInfo culture in cultures)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int index = 1; index <= 12; index++)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append("\"");
                        sb.Append(culture.DateTimeFormat.GetMonthName(index));
                        sb.Append("\"");
                    }
                    string months = sb.ToString();

                    string cn = culture.Name;
                    string cn2 = string.Empty;
                    if ((cn.Substring(0, 1) == cn.Substring(3, 1).ToLowerInvariant()) && (cn.Substring(1, 1) == cn.Substring(4, 1).ToLowerInvariant()))
                    {
                        cn2 = ", \"" + cn.Substring(0, 2) + "\"";
                    }

                    mm.Add(months + "_\"" + cn + "\"" + cn2);
                }

                mm.Sort();

                StringBuilder sb2 = new StringBuilder();
                int ii = 0;
                while (ii < mm.Count)
                {
                    string st2 = mm[ii];
                    string[] parts = st2.Split(new char[] { '_' });
                    string months = parts[0];
                    string stCultures = parts[1];
                    ii++;
                    while (ii < mm.Count)
                    {
                        string st3 = mm[ii];
                        string[] parts2 = st3.Split(new char[] { '_' });
                        if (parts2[0] != months) break;
                        stCultures += ", " + parts2[1];
                        ii++;
                    }
                    sb2.Append(string.Format("AddCulture(new string[] {0} {2} {1}, new string[] {0} {3} {1}, false);", "{", "}", months, stCultures));
                    sb2.Append("\r\n");
                }

                return sb2.ToString();
            }

            public static void AddCulture(string[] monthsNames, string[] cultureNames, bool defaultUpperCase)
            {
                int index = months.Count;
                months.Add(monthsNames);
                defaultUpperCaseList.Add(defaultUpperCase);
                foreach (string culture in cultureNames)
                {
                    cultureIndexes[culture.ToLowerInvariant()] = index;
                }
            }

            static MonthToStr()
            {
                months = new List<string[]>();
                defaultUpperCaseList = new List<bool>();
                cultureIndexes = new Hashtable();

                AddCulture(new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }, new string[] { "en" }, false);
                AddCulture(new string[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" }, new string[] { "ru" }, false);
                AddCulture(new string[] { "enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre" }, new string[] { "es" }, false);
                AddCulture(new string[] { "gennaio", "febbraio", "marzo", "aprile", "maggio", "giugno", "luglio", "agosto", "settembre", "ottobre", "novembre", "dicembre" }, new string[] { "it" }, false);
                AddCulture(new string[] { "ianuarie", "februarie", "martie", "aprilie", "mai", "iunie", "iulie", "august", "septembrie", "octombrie", "noiembrie", "decembrie" }, new string[] { "ro" }, false);
                AddCulture(new string[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" }, new string[] { "pt" }, false);
                AddCulture(new string[] { "Jannar", "Frar", "Marzu", "April", "Mejju", "Ġunju", "Lulju", "Awissu", "Settembru", "Ottubru", "Novembru", "Diċembru" }, new string[] { "mt" }, false);
                AddCulture(new string[] { "január", "február", "március", "április", "május", "június", "július", "augusztus", "szeptember", "október", "november", "december" }, new string[] { "hu" }, false);
                AddCulture(new string[] { "január", "február", "marec", "apríl", "máj", "jún", "júl", "august", "september", "október", "november", "december" }, new string[] { "sk" }, false);
                AddCulture(new string[] { "janúar", "febrúar", "mars", "apríl", "maí", "júní", "júlí", "ágúst", "september", "október", "nóvember", "desember" }, new string[] { "is" }, false);
                AddCulture(new string[] { "januar", "februar", "mars", "apríl", "mai", "juni", "juli", "august", "september", "oktober", "november", "desember" }, new string[] { "fo" }, false);
                AddCulture(new string[] { "Januar", "Februar", "März", "April", "Mai", "Juni", "Juli", "August", "September", "Oktober", "November", "Dezember" }, new string[] { "de" }, false);
                AddCulture(new string[] { "januari", "februari", "maart", "april", "mei", "juni", "juli", "augustus", "september", "oktober", "november", "december" }, new string[] { "nl" }, false);
                AddCulture(new string[] { "Januari", "Februari", "Maret", "April", "Mei", "Juni", "Juli", "Agustus", "September", "Oktober", "Nopember", "Desember" }, new string[] { "id" }, false);
                AddCulture(new string[] { "janvāris", "februāris", "marts", "aprīlis", "maijs", "jūnijs", "jūlijs", "augusts", "septembris", "oktobris", "novembris", "decembris" }, new string[] { "lv" }, false);
                AddCulture(new string[] { "janvier", "février", "mars", "avril", "mai", "juin", "juillet", "août", "septembre", "octobre", "novembre", "décembre" }, new string[] { "fr" }, false);
                AddCulture(new string[] { "leden", "únor", "březen", "duben", "květen", "červen", "červenec", "srpen", "září", "říjen", "listopad", "prosinec" }, new string[] { "cs" }, false);
                AddCulture(new string[] { "Mutarama", "Gashyantare", "Werurwe", "Mata", "Gicurasi", "Kamena", "Nyakanga", "Kanama", "Nzeli", "Ukwakira", "Ugushyingo", "Ukuboza" }, new string[] { "rw" }, false);
                AddCulture(new string[] { "Ocak", "Şubat", "Mart", "Nisan", "Mayıs", "Haziran", "Temmuz", "Ağustos", "Eylül", "Ekim", "Kasım", "Aralık" }, new string[] { "tr" }, false);
                AddCulture(new string[] { "ođđajagemánnu", "guovvamánnu", "njukčamánnu", "cuoŋománnu", "miessemánnu", "geassemánnu", "suoidnemánnu", "borgemánnu", "čakčamánnu", "golggotmánnu", "skábmamánnu", "juovlamánnu" }, new string[] { "se" }, false);
                AddCulture(new string[] { "sausis", "vasaris", "kovas", "balandis", "gegužė", "birželis", "liepa", "rugpjūtis", "rugsėjis", "spalis", "lapkritis", "gruodis" }, new string[] { "lt" }, false);
                AddCulture(new string[] { "siječanj", "veljača", "ožujak", "travanj", "svibanj", "lipanj", "srpanj", "kolovoz", "rujan", "listopad", "studeni", "prosinac" }, new string[] { "hr" }, false);
                AddCulture(new string[] { "styczeń", "luty", "marzec", "kwiecień", "maj", "czerwiec", "lipiec", "sierpień", "wrzesień", "październik", "listopad", "grudzień" }, new string[] { "pl" }, false);
                AddCulture(new string[] { "tammikuu", "helmikuu", "maaliskuu", "huhtikuu", "toukokuu", "kesäkuu", "heinäkuu", "elokuu", "syyskuu", "lokakuu", "marraskuu", "joulukuu" }, new string[] { "fi" }, false);
                AddCulture(new string[] { "јануари", "февруари", "март", "април", "мај", "јуни", "јули", "август", "септември", "октомври", "ноември", "декември" }, new string[] { "mk" }, false);
                AddCulture(new string[] { "Януари", "Февруари", "Март", "Април", "Май", "Юни", "Юли", "Август", "Септември", "Октомври", "Ноември", "Декември" }, new string[] { "bg" }, false);
                AddCulture(new string[] { "มกราคม", "กุมภาพันธ์", "มีนาคม", "เมษายน", "พฤษภาคม", "มิถุนายน", "กรกฎาคม", "สิงหาคม", "กันยายน", "ตุลาคม", "พฤศจิกายน", "ธันวาคม" }, new string[] { "th" }, false);

            }
        }
        #endregion

        #region DayOfWeekToStr
        /// <summary>
        /// DayOfWeek helper.
        /// </summary>
        public sealed class DayOfWeekToStr
        {
            #region Fields
            private static List<string[]> days = null;
            private static List<bool> defaultUpperCaseList = null;
            private static Hashtable cultureIndexes = null;
            #endregion

            /// <summary>
            /// Returns the day of the week.
            /// </summary>
            public static string DayOfWeek(DateTime date)
            {
                return date.DayOfWeek.ToString();
            }

            /// <summary>
            /// Returns the day of the week.
            /// </summary>
            public static string DayOfWeek(DateTime date, bool localized)
            {
                if (localized)
                {
                    switch (date.DayOfWeek)
                    {
                        case System.DayOfWeek.Sunday:
                            return "Воскресение";
                        case System.DayOfWeek.Monday:
                            return "Понедельник";
                        case System.DayOfWeek.Tuesday:
                            return "Вторник";
                        case System.DayOfWeek.Wednesday:
                            return "Среда";
                        case System.DayOfWeek.Thursday:
                            return "Четверг";
                        case System.DayOfWeek.Friday:
                            return "Пятница";
                        case System.DayOfWeek.Saturday:
                            return "Суббота";
                    }
                }
                return DayOfWeek(date);
            }

            public static string DayOfWeek(DateTime dateTime, string culture)
            {
                string dayName = string.Empty;

                try
                {
                    //get from system
                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(culture);
                    dayName = ci.DateTimeFormat.GetDayName(dateTime.DayOfWeek);
                }
                catch
                {
                    //use internal data
                    int day = (int)dateTime.DayOfWeek;
                    string cultureName = culture.ToLowerInvariant();
                    if (!cultureIndexes.ContainsKey(cultureName))
                    {
                        throw new ArgumentException(string.Format("Function \"DayOfWeek\": There is no data for specified culture \"{0}\".", culture));
                    }
                    int index = (int)cultureIndexes[cultureName];
                    if (day < days[index].Length) dayName = days[index][day];
                    if (defaultUpperCaseList[index])
                    {
                        dayName = dayName.Substring(0, 1).ToUpperInvariant() + dayName.Substring(1);
                    }
                }

                return dayName;
            }

            public static string DayOfWeek(DateTime dateTime, string culture, bool upperCase)
            {
                string dayName = DayOfWeek(dateTime, culture).ToLowerInvariant();
                if (upperCase)
                {
                    return dayName.Substring(0, 1).ToUpperInvariant() + dayName.Substring(1);
                }
                return dayName;
            }


            private string GetCulturesList()
            {
                var cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                List<string> mm = new List<string>();

                foreach (CultureInfo culture in cultures)
                {
                    string[] days = culture.DateTimeFormat.DayNames;

                    StringBuilder sb = new StringBuilder();
                    for (int index = 1; index <= 7; index++)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }
                        sb.Append("\"");
                        sb.Append(days[index - 1]);
                        sb.Append("\"");
                    }
                    string months = sb.ToString();

                    string cn = culture.Name;
                    string cn2 = string.Empty;
                    if ((cn.Substring(0, 1) == cn.Substring(3, 1).ToLowerInvariant()) && (cn.Substring(1, 1) == cn.Substring(4, 1).ToLowerInvariant()))
                    {
                        cn2 = ", \"" + cn.Substring(0, 2) + "\"";
                    }

                    mm.Add(months + "_\"" + cn + "\"" + cn2);
                }

                mm.Sort();

                StringBuilder sb2 = new StringBuilder();
                int ii = 0;
                while (ii < mm.Count)
                {
                    string st2 = mm[ii];
                    string[] parts = st2.Split(new char[] { '_' });
                    string months = parts[0];
                    string stCultures = parts[1];
                    ii++;
                    while (ii < mm.Count)
                    {
                        string st3 = mm[ii];
                        string[] parts2 = st3.Split(new char[] { '_' });
                        if (parts2[0] != months) break;
                        stCultures += ", " + parts2[1];
                        ii++;
                    }
                    sb2.Append(string.Format("AddCulture(new string[] {0} {2} {1}, new string[] {0} {3} {1}, false);", "{", "}", months, stCultures));
                    sb2.Append("\r\n");
                }

                return sb2.ToString();
            }

            public static void AddCulture(string[] monthsNames, string[] cultureNames, bool defaultUpperCase)
            {
                int index = days.Count;
                days.Add(monthsNames);
                defaultUpperCaseList.Add(defaultUpperCase);
                foreach (string culture in cultureNames)
                {
                    cultureIndexes[culture.ToLowerInvariant()] = index;
                }
            }

            static DayOfWeekToStr()
            {
                days = new List<string[]>();
                defaultUpperCaseList = new List<bool>();
                cultureIndexes = new Hashtable();

                AddCulture(new string[] { "dimanche", "lundi", "mardi", "mercredi", "jeudi", "vendredi", "samedi" }, new string[] { "fr" }, false);
                AddCulture(new string[] { "domenica", "lunedì", "martedì", "mercoledì", "giovedì", "venerdì", "sabato" }, new string[] { "it" }, false);
                AddCulture(new string[] { "domingo", "lunes", "martes", "miércoles", "jueves", "viernes", "sábado" }, new string[] { "es" }, false);
                AddCulture(new string[] { "domingo", "segunda-feira", "terça-feira", "quarta-feira", "quinta-feira", "sexta-feira", "sábado" }, new string[] { "pt" }, false);
                AddCulture(new string[] { "duminică", "luni", "marţi", "miercuri", "joi", "vineri", "sâmbătă" }, new string[] { "ro" }, false);
                AddCulture(new string[] { "Il-Ħadd", "It-Tnejn", "It-Tlieta", "L-Erbgħa", "Il-Ħamis", "Il-Ġimgħa", "Is-Sibt" }, new string[] { "mt" }, false);
                AddCulture(new string[] { "Ku wa mbere", "Ku wa kabiri", "Ku wa gatatu", "Ku wa kane", "Ku wa gatanu", "Ku wa gatandatu", "Ku cyumweru" }, new string[] { "rw" }, false);
                AddCulture(new string[] { "Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu" }, new string[] { "id" }, false);
                AddCulture(new string[] { "nedeľa", "pondelok", "utorok", "streda", "štvrtok", "piatok", "sobota" }, new string[] { "sk" }, false);
                AddCulture(new string[] { "nedjelja", "ponedjeljak", "utorak", "srijeda", "četvrtak", "petak", "subota" }, new string[] { "hr" }, false);
                AddCulture(new string[] { "niedziela", "poniedziałek", "wtorek", "środa", "czwartek", "piątek", "sobota" }, new string[] { "pl" }, false);
                AddCulture(new string[] { "Pazar", "Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi" }, new string[] { "tr" }, false);
                AddCulture(new string[] { "sekmadienis", "pirmadienis", "antradienis", "trečiadienis", "ketvirtadienis", "penktadienis", "šeštadienis" }, new string[] { "lt" }, false);
                AddCulture(new string[] { "Sonntag", "Montag", "Dienstag", "Mittwoch", "Donnerstag", "Freitag", "Samstag" }, new string[] { "de" }, false);
                AddCulture(new string[] { "sotnabeaivi", "mánnodat", "disdat", "gaskavahkku", "duorastat", "bearjadat", "lávvardat" }, new string[] { "se" }, false);
                AddCulture(new string[] { "sunnudagur", "mánadagur", "týsdagur", "mikudagur", "hósdagur", "fríggjadagur", "leygardagur" }, new string[] { "fo" }, false);
                AddCulture(new string[] { "sunnudagur", "mánudagur", "þriðjudagur", "miðvikudagur", "fimmtudagur", "föstudagur", "laugardagur" }, new string[] { "is" }, false);
                AddCulture(new string[] { "sunnuntai", "maanantai", "tiistai", "keskiviikko", "torstai", "perjantai", "lauantai" }, new string[] { "fi" }, false);
                AddCulture(new string[] { "svētdiena", "pirmdiena", "otrdiena", "trešdiena", "ceturtdiena", "piektdiena", "sestdiena" }, new string[] { "lv" }, false);
                AddCulture(new string[] { "vasárnap", "hétfő", "kedd", "szerda", "csütörtök", "péntek", "szombat" }, new string[] { "hu" }, false);
                AddCulture(new string[] { "zondag", "maandag", "dinsdag", "woensdag", "donderdag", "vrijdag", "zaterdag" }, new string[] { "nl" }, false);
                AddCulture(new string[] { "воскресенье", "понедельник", "вторник", "среда", "четверг", "пятница", "суббота" }, new string[] { "ru" }, false);
                AddCulture(new string[] { "недела", "понеделник", "вторник", "среда", "четврток", "петок", "сабота" }, new string[] { "mk" }, false);
                AddCulture(new string[] { "неделя", "понеделник", "вторник", "сряда", "четвъртък", "петък", "събота" }, new string[] { "bg" }, false);
                AddCulture(new string[] { "Ням", "Даваа", "Мягмар", "Лхагва", "Пүрэв", "Баасан", "Бямба" }, new string[] { "mn" }, false);
                AddCulture(new string[] { "อาทิตย์", "จันทร์", "อังคาร", "พุธ", "พฤหัสบดี", "ศุกร์", "เสาร์" }, new string[] { "th" }, false);

            }
        }
        #endregion

        private Func()
        {
        }
    }
}
