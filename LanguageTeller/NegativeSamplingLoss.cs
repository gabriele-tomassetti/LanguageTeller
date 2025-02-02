using MathNet.Numerics.Distributions;
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageTeller
{
    class NegativeSamplingLoss : BinaryLogisticLoss
    {

        protected static int NEGATIVE_TABLE_SIZE = 10000000;

        protected int neg_ = 0;
        protected List<int> negatives_ = new List<int>();

        protected DiscreteUniform uniform_;
        protected int GetNegative(int target, Random rng)
        {
            uniform_.RandomSource = rng;

            int negative;
            do
            {
                negative = negatives_[uniform_.Sample()];
            } while (target == negative);
            return negative;
        }

        public NegativeSamplingLoss(Matrix wo, int neg, List<long> targetCounts) : base(wo)
        {
            neg_ = neg;
            float z = 0.0f;
            for (int i = 0; i < targetCounts.Count; i++)
            {
                z += (float)Math.Pow(targetCounts[i], 0.5f);
            }
            for (int i = 0; i < targetCounts.Count; i++)
            {
                float c = (float)Math.Pow(targetCounts[i], 0.5f);
                for (int j = 0; j < c * NegativeSamplingLoss.NEGATIVE_TABLE_SIZE / z;
                     j++)
                {
                    negatives_.Add(i);
                }
            }
            uniform_ = new DiscreteUniform(0, negatives_.Count - 1);
        }

        public override float Forward(List<int> targets, int targetIndex,
                  ref State state, float lr, bool backprop)
        {
            if (targetIndex < 0)
                throw new Exception("TargetIndex < 0");
            if (targetIndex > targets.Count)
                throw new Exception("targetIndex > targets.Count");

            int target = targets[targetIndex];
            float loss = BinaryLogistic(target, ref state, true, lr, backprop);

            for (int n = 0; n < neg_; n++)
            {
                var negativeTarget = GetNegative(target, state.rng);
                loss += BinaryLogistic(negativeTarget, ref state, false, lr, backprop);
            }
            return loss;
        }
    }
}
