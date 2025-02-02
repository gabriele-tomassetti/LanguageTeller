using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageTeller
{
    public enum entry_type : int { word = 0, label = 1 };    

    public class entry
    {
        public String word;
        public long count;
        public entry_type type;
        public List<int> subwords;
    };

    public class FastTextDict
    {        
        protected const int MAX_VOCAB_SIZE = 30000000;
        protected const int MAX_LINE_SIZE = 1024;
        protected Args args_;
        protected List<int> word2int_;
        protected List<entry> words_;

        protected List<double> pdiscard_;
        protected int size_;
        protected int nwords_;
        protected int nlabels_;
        protected long ntokens_;

        protected long pruneidx_size_;
        protected Dictionary<int, int> pruneidx_;

        const string EOS = "</s>";
        const string BOW = "<";
        const string EOW = ">";

        public FastTextDict(Args args)
        {
            args_ = args;
            word2int_ = Enumerable.Repeat(-1, MAX_VOCAB_SIZE).ToList();
            size_ = 0;
            nwords_ = 0;
            nlabels_ = 0;
            ntokens_ = 0;
            pruneidx_size_ = -1;
            words_ = new List<entry>();
            pruneidx_ = new Dictionary<int, int>();
            pdiscard_ = new List<double>();
        }

        public FastTextDict(Args args, BinaryReader input)
        {
            args_ = args;            
            size_ = 0;
            nwords_ = 0;
            nlabels_ = 0;
            ntokens_ = 0;
            pruneidx_size_ = -1;
            words_ = new List<entry>();
            pruneidx_ = new Dictionary<int, int>();
            pdiscard_ = new List<double>();

            Load(input);
        }

        public int Nlabels
        {
            get
            {
                return nlabels_;
            }
        }

        public void Load(BinaryReader input)
        {
            words_.Clear();            
            size_ = input.ReadInt32();
            nwords_ = input.ReadInt32();
            nlabels_ = input.ReadInt32();
            ntokens_ = input.ReadInt64();
            pruneidx_size_ = input.ReadInt64();
            
            for (int i = 0; i < size_; i++)
            {
                char c;
                byte b;
                entry e = new entry();
                e.word = String.Empty;
                
                while ((c = input.ReadChar()) != 0)
                {
                    e.word += c;                
                }                
                
                e.count = input.ReadInt64();
                e.type = (entry_type)input.ReadSByte();
                words_.Add(e);
            }
            pruneidx_.Clear();
            for (long i = 0; i < pruneidx_size_; i++)
            {
                int first = -1;
                int second = -1;
                
                first = input.ReadInt32();
                second = input.ReadInt32();
                
                pruneidx_[first] = second;                
            }            
            InitTableDiscard();
            InitNgrams();

            int word2intsize = (int)Math.Ceiling(size_ / 0.7);
            word2int_ = Enumerable.Repeat(-1, word2intsize).ToList();
            for (int i = 0; i < size_; i++) {                
                word2int_[Find(words_[i].word)] = i;                
            }            
        }

        public void InitTableDiscard()
        {
            if (pdiscard_.Count < size_)
            {
                for (int a = pdiscard_.Count; a < size_; a++)
                {
                    pdiscard_.Add(0.0);
                }
            }

            for (int i = 0; i < size_; i++)
            {
                float f = (float)words_[i].count / (float)ntokens_;
                pdiscard_[i] = Math.Sqrt(args_.t / f) + args_.t / f;
            }
        }

        void ComputeSubwords(string word, ref List<int> ngrams)
        {
            for (int i = 0; i < word.Length; i++)
            {
                string ngram = "";
                if ((word[i] & 0xC0) == 0x80) continue;
                for (int j = i, n = 1; j < word.Length && n <= args_.maxn; n++)
                {
                    ngram += word[j++];
                    while (j < word.Length && (word[j] & 0xC0) == 0x80)
                    {
                        ngram += word[j++];
                    }                    
                    if (n >= args_.minn && !(n == 1 && (i == 0 || j == word.Length)))
                    {
                        int h = (int) (Hash(ngram) % args_.bucket);
                        
                        PushHash(ref ngrams, h);
                    }
                }
            }
        }

        void ComputeSubwords(string word, ref List<int> ngrams,
                            ref List<string> substrings)
        {
            for (int i = 0; i < word.Length; i++)
            {
                string ngram = "";
                if ((word[i] & 0xC0) == 0x80)
                {
                    continue;
                }
                for (int j = i, n = 1; j < word.Length && n <= args_.maxn; n++)
                {
                    ngram += word[j++];
                    while (j < word.Length && (word[j] & 0xC0) == 0x80)
                    {
                        ngram += word[j++];
                    }
                    if (n >= args_.minn && !(n == 1 && (i == 0 || j == word.Length)))
                    {
                        int h = (int)Hash(ngram) % args_.bucket;
                        PushHash(ref ngrams, h);
                        if (substrings != null)
                        {
                            substrings.Add(ngram);
                        }
                    }
                }
            }
        }

        void PushHash(ref List<int> hashes, int id)
        {
            if (pruneidx_size_ == 0 || id < 0)
            {
                return;
            }
            if (pruneidx_size_ > 0)
            {
                if (pruneidx_.ContainsKey(id))
                {
                    id = pruneidx_[id];
                }
                else
                {
                    return;
                }
            }
            hashes.Add(nwords_ + id);
        }

        public void InitNgrams()
        {            
            for (int i = 0; i < size_; i++)
            {
                string word = BOW + words_[i].word + EOW;                
                words_[i].subwords = new List<int>();
                words_[i].subwords.Clear();
                words_[i].subwords.Add(i);                
                if (words_[i].word != EOS)
                {                    
                    ComputeSubwords(word, ref words_[i].subwords);
                }
            }
        }

        uint Hash(string str)
        {
            uint h = 2166136261;
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            
            for (int i = 0; i < bytes.Length; i++)
            {                
                h = h ^ (uint)((sbyte)bytes[i]);                
                h = h * 16777619;                
            }

            return h;
        }

        int Find(string w)
        {
            return Find(w, Hash(w));
        }

        int Find(string w, uint h)
        {
            int word2intsize = word2int_.Count;
            int id = (int) (h % word2intsize);
            
            while (word2int_[id] != -1 && words_[word2int_[id]].word != w)
            {                
                id = (id + 1) % word2intsize;
            }
            
            return id;
        }

        public void Dump(TextWriter output)
        {
            output.WriteLine(words_.Count);
            foreach(var it in words_)
            {
                string entryType = "word";
                if (it.type == entry_type.label)
                {
                    entryType = "label";
                }
                output.WriteLine($"{it.word} {it.count} {entryType}");
            }
        }

        public bool IsPruned()
        {
            return pruneidx_size_ >= 0;
        }

        public List<long> GetCounts(entry_type type)
        {
            List<long> counts = new List<long>();
            foreach (var w in words_)
            {
                if (w.type == type)
                {
                    counts.Add(w.count);
                }
            }
            return counts;
        }

        public int GetLine(BinaryReader input, ref List<int> words,
            ref List<int> labels)
        {
            List<int> word_hashes = new List<int>();
            string token = null;
            int ntokens = 0;

            
            words.Clear();
            labels.Clear();
            
            while (ReadWord(input, ref token))
            {
                
                byte[] bytes = new byte[token.Length];
                for (int a = 0; a < bytes.Length; a++)
                    bytes[a] = (byte)token[a];

                token = Encoding.UTF8.GetString(bytes);
                uint h = Hash(token);                
                int wid = GetId(token, h);                                
                
                
                entry_type type = wid < 0 ? GetType(token) : GetType(wid);

                if (wid == 0)
                    break;

                ntokens++;
                if (type == entry_type.word)
                {
                    AddSubwords(ref words, token, wid);
                    word_hashes.Add((int) h);                    
                }
                else if (type == entry_type.label && wid >= 0)
                {
                    labels.Add(wid - nwords_);
                }
                if (token == EOS)
                {
                    break;
                }
            }            
            
            AddWordNgrams(ref words, word_hashes, args_.wordNgrams);
            
            return ntokens;
        }

        public void AddWordNgrams(ref List<int> line, List<int> hashes, int n)
        {
            for (int i = 0; i < hashes.Count; i++)
            {
                ulong h = (ulong) hashes[i];
                for (int j = i + 1; j < hashes.Count && j < i + n; j++)
                {
                    h = h * 116049371 + (ulong) hashes[j];
                    PushHash(ref line, (int) (h % (ulong) args_.bucket));
                }
            }
        }

        public void AddSubwords(ref List<int> line, string token, int wid)
        {
            if (wid < 0)
            {   // out of vocab
                
                if (token != EOS)
                {
                    ComputeSubwords(BOW + token + EOW, ref line);
                }
            }
            else
            {
                if (args_.maxn <= 0)
                { // in vocab w/o subwords                    
                    line.Add(wid);
                }
                else
                { // in vocab w/ subwords                    
                    List<int> ngrams = GetSubwords(wid);
                    line.AddRange(ngrams);                    
                }
            }
        }

        public List<int> GetSubwords(int i)
        {            
            if (i < 0)
                throw new Exception("i < 0");
            if (i > nwords_)
                throw new Exception("i > nwords_");

            return words_[i].subwords;
        }

        public void Reset(StreamReader input)
        {
            if (input.EndOfStream)
            {
                input.BaseStream.Position = 0;            
            }
        }

        public int GetId(string w, uint h)
        {
            int id = Find(w, h);
            return word2int_[id];
        }

        int GetId(string w)
        {
            int h = Find(w);
            return word2int_[h];
        }

        public entry_type GetType(int id)
        {
            if (id < 0)
                throw new Exception("id < 0");
            if (id >= size_)
                throw new Exception("id >= size_");

            return words_[id].type;
        }

        public entry_type GetType(string w)
        {
            return (w.IndexOf(args_.label) == 0) ? entry_type.label : entry_type.word;
        }

        public bool ReadWord(BinaryReader input, ref string word)
        {
            int i;
            char c;

            word = String.Empty;
            
            if (input.PeekChar() == -1)
                return false;

            while ((i = input.ReadByte()) != -1)
            {                
                c = (char) i;
                
                if (c == ' ' || c == '\n' || c == '\r' || c == '\t' || c == '\v' || c == '\f' || c == '\0')
                {                    
                    if (String.IsNullOrEmpty(word))
                    {
                        if (c == '\n')
                        {
                            word += EOS;
                            
                            return true;
                        }
                        continue;
                    }
                    else
                    {
                        if (c == '\n')
                            input.BaseStream.Position--;
                        
                        return true;
                    }                    
                }                
                word += c;

                if (input.PeekChar() == -1)
                    break;
            }
            
            return !String.IsNullOrEmpty(word);
        }

        public string GetLabel(int lid)
        {
            if (lid < 0 || lid >= nlabels_)
            {
                throw new Exception(
                    "Label id is out of range [0, " + nlabels_.ToString() + "]");
            }
            return words_[lid + nwords_].word.ToString();
        }
    }
}