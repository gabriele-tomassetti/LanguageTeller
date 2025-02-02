using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageTeller
{
    public class EncounteredNaNError : Exception
    {
        public EncounteredNaNError() : base("Encountered NaN.") { }
    }
}
