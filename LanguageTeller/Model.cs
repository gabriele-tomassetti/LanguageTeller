using MathNet.Numerics.Random;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LanguageTeller
{
    using Predictions = List<(float, int)>;

    public class State
    //public ref struct State
    {
        float lossValue_;
        long nexamples_;
        
        public Vector hidden;
        public Vector output;
        public Vector grad;
        public Mcg31m1 rng;

        public State(int hiddenSize, int outputSize, int seed)
        {
            hidden = new Vector(hiddenSize);
            output = new Vector(outputSize);
            grad = new Vector(hiddenSize);
            rng = new Mcg31m1(seed);           
            lossValue_ = 0.0f;
            nexamples_ = 1;
        }
        
        void incrementNExamples(float loss)
        {
            lossValue_ += loss;
            nexamples_++;
        }
    };

    public class Model
    {
        const int kUnlimitedPredictions = -1;
        const int kAllLabelsAsTarget = -1;

        protected Matrix wi_;
        protected Matrix wo_;
        protected Loss loss_;
        bool normalizeGradient_;
        
        public Model(Matrix wi, Matrix wo, Loss loss, bool normalizeGradient)
        {
            wi_ = wi;
            wo_ = wo;
            loss_ = loss;            
        }

        public static float StdLog(float x)
        {
            return (float)Math.Log(x + 1e-5f);
        }        
        
        public void Predict(List<int> input, int k, float threshold,
            ref Predictions heap, State state)
        {
            if (k == kUnlimitedPredictions)
            {
                k = (int) wo_.Size(0);
            }
            else if (k <= 0)
            {
                throw new Exception("k needs to be 1 or higher!");
            }
            if (normalizeGradient_)
            {
                throw new Exception("Model needs to be supervised for prediction!");
            }
            heap.Capacity = k + 1;
            ComputeHidden(input, ref state);
            
            loss_.Predict(k, threshold, ref heap, ref state);
        }        
        
        public void ComputeHidden(List<int> input, ref State state)
        {
            Vector hidden = state.hidden;
            wi_.AverageRowsToVector(hidden, input.ToArray());
        }        
    }
}