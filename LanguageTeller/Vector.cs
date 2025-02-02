using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LanguageTeller
{
    public class Vector
    //public ref struct Vector
    {
        //private Span<float> data_;        
        // protected List<float> data_;
        protected float[] data_;

        public Vector(int size)
        {
            // initialize vector of number size elements
            data_ = Enumerable.Repeat(0.0f, size).ToArray();
        }

        public Span<float> Data
        {
            get
            {
                return data_;
            }
        }

        public int Count
        {
            get
            {
                return data_.Length;
            }
        }

        public float this[int i]
        {
            get
            {
                return data_[i];
            }
            set
            {
                data_[i] = value;
            }
        }

        public void Zero()
        {
            //data_ = Enumerable.Repeat(0.0f, data_.Length).ToArray();
            //data_.Fill(0.0f);
            Array.Clear(data_, 0, data_.Length);
        }

        public void Multiply(float a)
        {
            for (long i = 0; i < Count; i++)
            {
                data_[(int) i] *= a;
            }
        }

        public double Normalize()
        {
            double sum = 0;
            for (long i = 0; i < Count; i++)
            {
                sum += data_[(int)i] * data_[(int)i];
            }

            return Math.Sqrt(sum);
        }

        public void AddVector(Vector source)
        {
            if(Count != source.Count)
            {
                throw new Exception("The source vector has a different size");
            }

            for (long i = 0; i < Count; i++)
            {
                data_[(int)i] += source.data_[(int)i];
            }
        }

        public void AddVector(Vector source, float s)
        {
            if (Count != source.Count)
            {
                throw new Exception("The source vector has a different size");
            }

            for (long i = 0; i < Count; i++)
            {
                data_[(int)i] += s * source.data_[(int)i];
            }
        }

        public void AddRow(ref Matrix A, long i)
        {
            if(i < 0)
                throw new Exception("The index i is less than 0");

            if(i >= A.Size(0))
                throw new Exception("The index i is larger than the size of A dimension 0");

            if (Count != A.Size(1))
                throw new Exception("The size of the vector different form the size of A dimension 1");

            A.AddRowToVector(this, (int) i);
        }

        public void AddRow(ref Matrix A, long i, float a)
        {
            if (i < 0)
                throw new Exception("The index i is less than 0");

            if (i >= A.Size(0))
                throw new Exception("The index i is larger than the size of A dimension 0");

            if (Count != A.Size(1))
                throw new Exception("The size of the vector different form the size of A dimension 1");

            A.AddRowToVector(this, (int)i, a);
        }                

        public void Mul(Matrix A, Vector vec)
        {
            if (A.Size(0) != Count)
                throw new Exception("The size of this vector different form the size of A dimension 0");
            if (A.Size(1) != vec.Count)
                throw new Exception("The size of the vector vec is different form the size of A dimension A");
            
            for (long i = 0; i < Count; i++)
            {
                data_[(int)i] = A.DotRow(vec, i);
            }
        }

        public int Argmax()
        {
            return 0;
        }
    }
}
