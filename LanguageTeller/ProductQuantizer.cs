using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LanguageTeller
{
    public class ProductQuantizer
    {
        const int nbits_ = 8;
        const int ksub_ = 1 << nbits_;
        const int max_points_per_cluster_ = 256;
        const int max_points_ = max_points_per_cluster_ * ksub_;
        const int seed_ = 1234;
        const int niter_ = 25;
        const float eps_ = 1e-7F;

        int dim_;
        int nsubq_;
        int dsub_;
        int lastdsub_;

        public List<float> centroids_ = new List<float>();

        Random rng;

        public ProductQuantizer()
        { }

        public void Load(BinaryReader input)
        {
            dim_ = input.ReadInt32();
            nsubq_ = input.ReadInt32();
            dsub_ = input.ReadInt32();
            lastdsub_ = input.ReadInt32();

            if (centroids_.Count < dim_ * ksub_)
            {
                for (int a = centroids_.Count; a < dim_ * ksub_; a++)
                {
                    centroids_.Add(default(float));
                }
            }            
            for (int i = 0; i < centroids_.Count; i++)
            {
                centroids_[i] = input.ReadSingle();                
            }
        }

        public int GetCentroids(int m, Byte i)
        {
            if (m == nsubq_ - 1)
            {
                return m * ksub_ * dsub_ + i * lastdsub_;
            }
            return (m * ksub_ + i) * dsub_;
        }

        public float Mulcode(Vector x, List<Byte> codes, int t, float alpha)
        {
            float res = 0.0f;
            var d = dsub_;
            int code = nsubq_ * t;
            for (var m = 0; m < nsubq_; m++)
            {
                int c = GetCentroids(m, codes[code + m]);
                if (m == nsubq_ - 1)
                {
                    d = lastdsub_;
                }
                for (var n = 0; n < d; n++)
                {
                    res += x[m * dsub_ + n] * centroids_[c + n];
                }
            }
            return res * alpha;
        }

        public void Addcode(Vector x, ref List<Byte> codes, int t, float alpha)
        {
            var d = dsub_;            
            int code = nsubq_ * t;
            for (var m = 0; m < nsubq_; m++)
            {
                int c = GetCentroids(m, codes[code + m]);
                if (m == nsubq_ - 1) { d = lastdsub_; }
                for (var n = 0; n < d; n++)
                {
                    x[m * dsub_ + n] += alpha * centroids_[c + n];
                }
            }
        }
    }
}