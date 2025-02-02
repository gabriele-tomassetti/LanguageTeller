using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageTeller
{            
    public class DenseMatrix : Matrix
    {
        //public List<float> Data {
        public float[] Data
        {
            get
            {
                return data_;
            }
        }

        public DenseMatrix() : this(0, 0)
        { }

        DenseMatrix(long m, long n)
        {
            //data_ = Enumerable.Repeat(0.0f, (int)(m * n)).ToList();
            data_ = Enumerable.Repeat(0.0f, (int)(m * n)).ToArray();
            m_ = m;
            n_ = n;
        }

        public float At(long i, long j)
        {            
            return data_[(int)(i * n_ + j)];
        }

        public void Zero()
        {
            //data_ = Enumerable.Repeat(0.0f, data_.Count).ToList();
            Array.Clear(data_, 0, data_.Length);
        }
        
        public override void AddVectorToRow(Vector vec, long i, float a)
        {
            for (long j = 0; j < n_; j++)
            {
                float[] d;
                data_[i * n_ + j] += a * vec[(int)j];
                //data_[(int)(i * n_ + j)] += a * vec[(int)j];
            }
        }
        
        public override void AddRowToVector(Vector x, int i, float a)
        {
            for (long j = 0; j < n_; j++)
            {
                x[(int)j] += a * At(i, j);
            }
        }
                   
        public override void AddRowToVector(Vector x, int i)
        {            
            for (long j = 0; j < n_; j++)
            {
                x[(int)j] += At(i, j);
            }
        }


        public override float DotRow(Vector vec, long i)
        {
            if (i < 0)
                throw new Exception("i < 0");
            if (i > m_)
                throw new Exception("i > m_");
            if (vec.Count != n_)
                throw new Exception("vec.Count != n_");
            float d = 0.0f;

            for (long j = 0; j < n_; j++)
            {
                d += At(i, j) * vec[(int)j];
            }
            if (float.IsNaN(d))
            {
                throw new EncounteredNaNError();                
            }
            return d;
        }

        public override void Load(BinaryReader input)
        {
            m_ = input.ReadInt64();
            n_ = input.ReadInt64();
            //data_ = new List<float>((int)(m_ * n_));
            data_ = new float[m_ * n_];

            for (int a = 0; a < m_ * n_; a++)
            {
                //data_.Add(input.ReadSingle());            
                data_[a] = input.ReadSingle();
            }
        }

        public override void Dump(TextWriter output)
        {
            output.WriteLine($"{m_} {n_} ");
            for (long i = 0; i < m_; i++)
            {
                for (long j = 0; j < n_; j++)
                {
                    if (j > 0)
                    {
                        output.Write(" ");
                    }
                    output.Write(At(i, j));
                }
                output.WriteLine();
            }
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
