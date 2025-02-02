using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace LanguageTeller.Tests
{
    public class BasicTests
    {
        private LanguageTeller LanguageTellerFtz;
        public BasicTests()
        {
            LanguageTellerFtz = new LanguageTeller();
        }

        [Fact]
        public void TestLabels()
        {
            var labels = LanguageTellerFtz.GetLabels();

            Assert.Equal(176, labels.Length);
        }

        [Fact]
        public void TestLabelsTag()
        {
            var labels = LanguageTellerFtz.GetLabels(true);

            Assert.Equal(176, labels.Length);
        }
    }
}