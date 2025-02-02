using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageTeller
{
    public abstract class Matrix
    //public ref struct Matrix
    {
        //protected List<float> data_;
        //protected Memory<float> data_;
        protected float[] data_;
        protected long m_;
        protected long n_;

        public Matrix() : this(0, 0)
        { }

        Matrix(long m, long n)
        {
            data_ = Enumerable.Repeat(0.0f, (int) (m * n)).ToArray();
            //data_ = Enumerable.Repeat(0.0f, (int)(m * n)).ToList();
            m_ = m;
            n_ = n;
        }

        public long Size(long dim)
        {
            if (dim != 0 && dim != 1)
                throw new Exception($"Wrong dimension passed to Size {dim}");
            if (dim == 0) {
                return m_;
            }
            return n_;            
        }

        public abstract float DotRow(Vector vec, long i);        
        public abstract void AddVectorToRow(Vector vec, long i, float a);
        public abstract void AddRowToVector(Vector x, int i);
        public abstract void AddRowToVector(Vector x, int i, float a);
        public abstract void AverageRowsToVector(Vector x, ReadOnlySpan<int> rows);
        public abstract void Load(BinaryReader input);
        public abstract void Dump(TextWriter output);
    }
}
