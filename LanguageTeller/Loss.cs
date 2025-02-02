using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MathNet.Numerics.Distributions;

namespace LanguageTeller
{
    using Predictions = List<(float, int)>;
    
    public class Loss
    {        
        protected List<float> t_sigmoid_ = new List<float>();
        protected List<float> t_log_ = new List<float>();
        protected Matrix wo_;

        protected const long SIGMOID_TABLE_SIZE = 512;
        protected const long MAX_SIGMOID = 8;
        protected const long LOG_TABLE_SIZE = 512;

        protected static int ComparePairs((float, int) l, (float, int) r)
        {
            return (int) (l.Item1 - r.Item1);
        }

        protected float Log(float x)
        {
            if (x > 1.0)
            {
                return 0.0f;
            }
            
            long i = (long) (x * LOG_TABLE_SIZE);
            return t_log_[(int)i];
        }

        private void FindKBest(int k, float threshold, ref Predictions heap, ref Vector output)
        {            
            for (int i = 0; i < output.Count; i++)
            {
                if (output[i] < threshold)
                {
                    continue;
                }
                if (heap.Count == k && Model.StdLog(output[i]) < heap.First().Item1)
                {
                    continue;
                }
                heap.Add((Model.StdLog(output[i]), i));
                heap.Sort(ComparePairs);
                if (heap.Count > k)
                {
                    heap.RemoveAt(0);
                }
            }
        }

        protected float Sigmoid(float x)
        {
            if (x < -MAX_SIGMOID)
            {
                return 0.0f;
            }
            else if (x > MAX_SIGMOID)
            {
                return 1.0f;
            }
            else
            {
                long i =
                    (long)((x + MAX_SIGMOID) * SIGMOID_TABLE_SIZE / MAX_SIGMOID / 2);

                return t_sigmoid_[(int)i];
            }
        }

        public virtual float Forward(List<int> targets, int targetIndex,            
            ref State state, float lr, bool backprop)
        {
            return 0.0f;
        }

        public Loss(Matrix wo)
        {
            t_sigmoid_.Capacity = (int) SIGMOID_TABLE_SIZE + 1;
            for (int i = 0; i < SIGMOID_TABLE_SIZE + 1; i++)
            {
                float x = (float)(i * 2 * MAX_SIGMOID) / SIGMOID_TABLE_SIZE - MAX_SIGMOID;
                t_sigmoid_.Add(1.0f / (1.0f + (float)Math.Exp(-x)));
            }

            t_log_.Capacity = (int) LOG_TABLE_SIZE + 1;
            for (int i = 0; i < LOG_TABLE_SIZE + 1; i++)
            {
                float x = ((float)(i) + 1e-5f) / LOG_TABLE_SIZE;
                t_log_.Add((float)Math.Log(x));
            }

            wo_ = wo;
        }

        public virtual void ComputeOutput(ref State state)
        {
        }

        public virtual void Predict(int k, float threshold, ref Predictions heap, ref State state)
        {
            ComputeOutput(ref state);
            FindKBest(k, threshold, ref heap, ref state.output);            
            heap.Sort(ComparePairs);            
        }
    }        
}
