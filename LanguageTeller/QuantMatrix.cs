using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageTeller
{
    public class QuantMatrix : Matrix
    {
        ProductQuantizer pq_;
        ProductQuantizer npq_;

        List<Byte> codes_;
        List<Byte> norm_codes_;

        bool qnorm_;

        int codesize_;

        public long GetM
        {
            get
            {
                return m_;
            }
        }

        public long GetN
        {
            get
            {
                return n_;
            }
        }

        public QuantMatrix() : base()
        {
            qnorm_ = false;
            codesize_ = 0;
        }
           
        public QuantMatrix(ref DenseMatrix mat, int dsub, bool qnorm)
        { }

        public void QuantizeNorm(Vector vec)
        { }
        
        public void Quantize(ref DenseMatrix mat)
        {}

        public override void AddVectorToRow(Vector vec, long i, float a)
        {
            throw new Exception("Operation not permitted on quantized matrices.");
        }
        
        public override void AddRowToVector(Vector x, int i, float a)
        {
            float norm = 1;
            if (qnorm_)
            {
                norm = npq_.GetCentroids(0, norm_codes_[i]);
            }
            pq_.Addcode(x, ref codes_, i, a * norm);
        }
        
        public override void AddRowToVector(Vector x, int i)
        {
            float norm = 1;
            if (qnorm_)
            {
                norm = npq_.GetCentroids(0, norm_codes_[i]);
            }
            pq_.Addcode(x, ref codes_, i, norm);
        }

        public void AddToVector(Vector x, int t)
        {
            float norm = 1;
            if (qnorm_)
            {
                norm = npq_.centroids_[npq_.GetCentroids(0, norm_codes_[t])];
            }
            pq_.Addcode(x, ref codes_, t, norm);
        }

        public override float DotRow(Vector vec, long i)
        {            
            if (i < 0)
                throw new Exception("i < 0");
            if (i > m_)
                throw new Exception("i > m_");
            if (vec.Count != n_)
                throw new Exception("vec.Count != n_");

            float norm = 1;
            if (qnorm_) {                
                norm = npq_.centroids_[npq_.GetCentroids(0, norm_codes_[(int)i])];
            }
            return pq_.Mulcode(vec, codes_, (int) i, norm);            
        }

        public override void Load(BinaryReader input)
        {
            qnorm_ = input.ReadBoolean();
            m_ = input.ReadInt64();
            n_ = input.ReadInt64();
            codesize_ = input.ReadInt32();            
            codes_ = new List<byte>(codesize_);

            for (int a = 0; a < codesize_; a++)
            {
                codes_.Add(input.ReadByte());
            }
            pq_ = new ProductQuantizer();
            pq_.Load(input);      
            if (qnorm_)
            {                
                norm_codes_ = new List<byte>((int)m_);
                for (int a = 0; a < m_; a++)
                {
                    norm_codes_.Add(input.ReadByte());
                }                
                npq_ = new ProductQuantizer();
                npq_.Load(input);
            }
        }

        public override void Dump(TextWriter output)
        {
             throw new Exception("Operation not permitted on quantized matrices.");
        }

        public override void AverageRowsToVector(Vector x, ReadOnlySpan<int> rows)
        {
            x.Zero();
            foreach(int i in rows)
            {
                AddRowToVector(x, i);
            }
            x.Multiply(1.0f / rows.Length);
        }
    }
}
