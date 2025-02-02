using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageTeller
{
    class OneVsAllLoss : BinaryLogisticLoss
    {
        public OneVsAllLoss(Matrix wo) : base(wo) { }
        public override float Forward(List<int> targets, int targetIndex,
            ref State state, float lr, bool backprop)
        {
            float loss = 0.0f;
            int osz = state.output.Count;
            for (int i = 0; i < osz; i++)
            {
                bool isMatch = targets.Contains(i);
                loss += BinaryLogistic(i, ref state, isMatch, lr, backprop);
            }

            return loss;
        }
    }
}
