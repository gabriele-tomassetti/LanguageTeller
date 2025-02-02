using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageTeller
{
    class BinaryLogisticLoss : Loss
    {
        protected float BinaryLogistic(int target, ref State state,
                                        bool labelIsPositive, float lr, bool backprop)
        {
            float score = Sigmoid(wo_.DotRow(state.hidden, target));
            if (backprop)
            {
                float alpha = lr * ((float)Convert.ToDouble(labelIsPositive) - score);
                state.grad.AddRow(ref wo_, target, alpha);
                wo_.AddVectorToRow(state.hidden, target, alpha);
            }
            if (labelIsPositive)
            {
                return -Log(score);
            }
            else
            {
                return -Log(1.0f - score);

            }
        }


        public override void ComputeOutput(ref State state)
        {
            state.output.Mul(wo_, state.hidden);
            int osz = state.output.Count;
            for (int i = 0; i < osz; i++)
            {
                state.output[i] = Sigmoid(state.output[i]);
            }
        }

        public BinaryLogisticLoss(Matrix wo) : base(wo) { }
    }
}
