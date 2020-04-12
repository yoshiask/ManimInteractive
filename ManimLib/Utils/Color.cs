using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ManimLib.Utils
{
    public static class Color
    {
        public static List<RL.Color> ColorGradient(int lengthOfOutput, params RL.Color[] referenceColors)
        {
            if (lengthOfOutput == 0)
                return new List<RL.Color>() { referenceColors[0] };

            IEnumerable<int[]> rgbs = referenceColors.Select(c => new int[] { c.R, c.G, c.B });
            List<int> alphas = Iterables.LinSpace(0, rgbs.Count() - 1, lengthOfOutput).ToList();
            alphas.ForEach(num => num %= 1);
            alphas[alphas.Count - 1] = 1;
            //...
            throw new NotImplementedException();
        }
    }
}
