using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageTeller
{
    class SoftmaxLoss : Loss
    {
        public SoftmaxLoss(Matrix wo) : base(wo)
        { }

        public override float Forward(List<int> targets, int targetIndex,
                   ref State state, float lr, bool backprop)
        {
            ComputeOutput(ref state);

            int target = targets[targetIndex];

            if (backprop)
            {
                int osz = (int)wo_.Size(0);
                for (int i = 0; i < osz; i++)
                {
                    float label = (i == target) ? 1.0f : 0.0f;
                    float alpha = lr * (label - state.output[i]);
                    state.grad.AddRow(ref wo_, i, alpha);
                    wo_.AddVectorToRow(state.hidden, i, alpha);
                }
            }
            return -Log(state.output[target]);
        }

        public override void ComputeOutput(ref State state)
        {
            state.output.Mul(wo_, state.hidden);
            float max = state.output[0], z = 0.0f;
            int osz = state.output.Count;
            for (int i = 0; i < osz; i++)
            {
                max = Math.Max(state.output[i], max);
            }
            for (int i = 0; i < osz; i++)
            {
                state.output[i] = (float)Math.Exp(state.output[i] - max);
                z += state.output[i];
            }
            for (int i = 0; i < osz; i++)
            {
                state.output[i] /= z;
            }
        }
    }
}
