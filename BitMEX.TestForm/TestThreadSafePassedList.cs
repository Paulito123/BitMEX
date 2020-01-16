using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitMEX.TestForm
{
    public class TestThreadSafePassedList
    {
        private readonly List<int> TellerLijst;

        public TestThreadSafePassedList(List<int> tellerLijst)
        {
            TellerLijst = tellerLijst;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (int i in TellerLijst)
            {
                sb.Append($"{i.ToString()}");
            }
            return sb.ToString();
        }
    }
}
