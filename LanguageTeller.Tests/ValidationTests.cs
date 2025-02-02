using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Xunit;

namespace LanguageTeller.Tests
{
    public class ValidationTests
    {
        private static CultureInfo[] Cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

        private FastText LanguageTellerFtz;        
        public ValidationTests()
        {            
            LanguageTellerFtz = new FastText();
        }

        private static string GetLanguage(string code)
        {
            // we need to manually add some codes that are in FastText, but not in .NET
            Dictionary<string, string> ThreeLetterCodeToName = new Dictionary<string, string>() {
                { "als", "Albanian" },
                { "arz", "Arabic" },
                { "azb", "Azerbaijani" },
                { "bar", "Bavarian" },
                { "bcl", "Bikol" },
                { "bpy", "Bishnupriya Manipuri" },
                { "bxr", "Buryat" },
                { "cbk", "Chavacano" },
                { "ceb", "Cebuano" },
                { "ckb", "Central Kurdish" },
                { "diq", "Dimli" },
                { "dty", "Dotyali" },
                { "eml", "" },
                { "frr", "Northern Frisian" },
                { "gom", "Goan Konkani" },
                { "hif", "Fiji Hindi" },
                { "ilo", "Iloko" },
                { "jbo", "Lojban" },
                { "krc", "Karachay-Balkar" },
                { "lez", "Lezghian" },
                { "lmo", "Lombard" },
                { "mai", "Maithili" },
                { "mhr", "Eastern Mari" },
                { "min", "" },
                { "mrj", "" },
                { "mwl", "" },
                { "myv", "" },
                { "nah", "" },
                { "nap", "" },
                { "new", "" },
                { "pam", "" },
                { "pfl", "" },
                { "pms", "" },
                { "pnb", "" },
                { "rue", "" },
                { "scn", "" },
                { "sco", "" },
                { "tyv", "" },
                { "vec", "" },
                { "vep", "" },
                { "vls", "" },
                { "war", "" },
                { "wuu", "" },
                { "xal", "" },
                { "xmf", "" },
                { "yue", "" }
            };

            Dictionary<string, string> TwoLetterCodeToName = new Dictionary<string, string>() {
                { "an", "Aragonese" },
                { "bh", "Bihari" },
                { "cv", "" },
                { "ht", "" },
                { "ie", "" },
                { "io", "" },
                { "kv", "" },
                { "li", "" },
                { "no", "" },
                { "qu", "" },
                { "sc", "" },
                { "sh", "" },
                { "su", "" },
                { "tl", "" },
                { "wa", "" },
            };


            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            if (code.Length == 3)
            {
                if (ThreeLetterCodeToName.ContainsKey(code))
                    return ThreeLetterCodeToName[code];
                else
                    return (from c in cultures
                            where c.ThreeLetterISOLanguageName == code
                            select c.EnglishName).FirstOrDefault();
            }
            else if (code.Length == 2)
            {
                if (TwoLetterCodeToName.ContainsKey(code))
                    return TwoLetterCodeToName[code];
                else
                    return (from c in cultures
                            where c.TwoLetterISOLanguageName == code
                            select c.EnglishName).FirstOrDefault();
            }
            else
            {
                return "";
            }
        }

        public static IEnumerable<object[]> GetCorrectPhrasesFtz()
        {
            IEnumerable<string> lines = File.ReadAllLines(@"..\..\..\data\langid.ftz-correct.txt");

            foreach(var l in lines)
            {
                yield return new object[] { l };
            }            
        }

        public static IEnumerable<object[]> GetWrongPhrasesFtz()
        {
            IEnumerable<string> lines = File.ReadAllLines(@"..\..\..\data\langid.ftz-wrong.txt");

            foreach (var l in lines)
            {
                yield return new object[] { l };
            }
        }

        [Theory]
        [MemberData(nameof(GetCorrectPhrasesFtz))]        
        public void TestCorrectPhrasesFtz(string text)
        {                        
            string language = text.Substring(9, 2);
            LanguageMatch lm = LanguageTellerFtz.TellLanguage(text.Remove(0, 12));

            Assert.Equal(GetLanguage(language), GetLanguage(lm.Language));
        }

        [Theory]
        [MemberData(nameof(GetWrongPhrasesFtz))]
        public void TestWrongPhrasesFtz(string text)
        {
            string language = text.Substring(9, 2);
            LanguageMatch lm = LanguageTellerFtz.TellLanguage(text.Remove(0, 12));

            Assert.NotEqual(GetLanguage(language), GetLanguage(lm.Language));
        }
    }
}
