using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Reflection;

namespace LanguageTeller
{
    using Predictions = List<(float, int)>;

    class LanguageIdentification
    {       
        public int Matches { get; set; }
        public float AverageProbability { get; set; }
        public float Value { get; set; }
        public int Words { get; set; }
    }
    
    public class FastText
    {
        private bool quant_;
        private int version;
        private List<DenseMatrix> wordWectors_;

        protected Args args_;
        protected FastTextDict dict_;
        
        protected Matrix input_;
        protected Matrix output_;

        protected Model model_;

        const int FASTTEXT_VERSION = 12; /* Version 1b */
        const int FASTTEXT_FILEFORMAT_MAGIC_INT32 = 793712314;

        private float thresholdMainLanguage = 0.2f;
        public float ThresholdMainLanguage
        {
            get { return thresholdMainLanguage; }
            // since it represents a percentage, the maximum value can be 1.0f
            set { thresholdMainLanguage = value <= 1.0f ? value : 1.0f; }
        }

        public FastText(bool loadDefault = true)
        {
            if(loadDefault == true)
                LoadDefaultModel();
        }        

        /// <summary>
        /// Checks that the file contains a valid model
        /// </summary>
        /// <param name="input">Path to a model (.bin/.ftz file).</param>
        public bool CheckModel(BinaryReader input)
        {
            int magic;
            magic = input.ReadInt32(); 
            if (magic != FASTTEXT_FILEFORMAT_MAGIC_INT32)
            {
                return false;
            }
            version = input.ReadInt32();
            if (version > FASTTEXT_VERSION)
            {
                return false;
            }
            return true;
        }

        private void LoadEmbeddedModel(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new FileNotFoundException($"Resource {resourceName} not found.");

                using (BinaryReader br = new BinaryReader(stream))
                {
                    LoadModel(br);
                }
            }
        }

        public void LoadDefaultModel()
        {
            args_ = new Args();
            input_ = new DenseMatrix();
            output_ = new DenseMatrix();

            LoadEmbeddedModel("LanguageTeller.Models.lid.176.ftz");
        }

        Loss CreateLoss(ref Matrix output)
        {            
            loss_name lossName = loss_name.hs;
            
            switch (lossName)
            {
                case loss_name.hs:
                    return new HierarchicalSoftmaxLoss(output, GetTargetCounts());
                
                case loss_name.ns:
                    return new NegativeSamplingLoss(output, args_.neg, GetTargetCounts());
                
                case loss_name.softmax:
                    return new SoftmaxLoss(output);
                
                case loss_name.ova:
                    return new OneVsAllLoss(output);
                
                default:
                    throw new Exception("Unknown loss");
            }
        }

        List<long> GetTargetCounts()
        {
            if (args_.model == model_name.sup)
                return dict_.GetCounts(entry_type.label);
            else
                return dict_.GetCounts(entry_type.word);            
        }

        public void LoadModel(BinaryReader br)
        {
            if (CheckModel(br) == false)
                throw new Exception($"Model has wrong file format!");

            args_.Load(br);
            if (version == 11 && args_.model == model_name.sup)
            {
                // backward compatibility: old supervised models do not use char ngrams.
                args_.maxn = 0;
            }            
            dict_ = new FastTextDict(args_, br);

            bool quant_input;
            quant_input = br.ReadBoolean();            
            if (quant_input)
            {
                quant_ = true;                
                input_ = new QuantMatrix();
            }
            
            input_.Load(br);            

            if (!quant_input && dict_.IsPruned())
            {
                throw new Exception(
                    "Invalid model file.\n" +
                    "Please download the updated model from www.fasttext.cc.\n" +
                    "See issue #332 on Github for more information.\n");
            }
            
            args_.qout = br.ReadBoolean();            
            if (quant_ && args_.qout)
            {                
                output_ = new QuantMatrix();
            }
            output_.Load(br);
            bool normalizeGradient = (args_.model == model_name.sup);
            Loss loss = CreateLoss(ref output_);
            model_ = new Model(input_, output_, loss, normalizeGradient);               
        }

        /// <summary>
        /// Loads a trained model from a file.
        /// </summary>
        /// <param name="path">Path to a model (.bin/.ftz file).</param>
        public void LoadModel(string path)
        {
            args_ = new Args();
            input_ = new DenseMatrix();
            output_ = new DenseMatrix();
            quant_ = false;

            using (BinaryReader br = new BinaryReader(File.OpenRead(path)))
            {
                LoadModel(br);
            }
        }

        /// <summary>
        /// Gets all labels that classifier was trained on.
        /// </summary>
        /// <returns>Labels.</returns>
        public string[] GetLabels(bool justLanguageTag = false)
        {            
            List<string> labels = new List<string>();
            for(int i = 0; i < dict_.Nlabels; i++)
            {
                if (justLanguageTag)
                    labels.Add(dict_.GetLabel(i).Substring(9));
                else
                    labels.Add(dict_.GetLabel(i));
            }                

            return labels.ToArray();
        }

        private List<LanguageMatch> Identify(List<int> words, int results = 1, float threshold = 0.0f)
        {
            if (words.Count == 0)
            {
                return new List<LanguageMatch>();
            }
            
            State state = new State(args_.dim, dict_.Nlabels, 0);

            Predictions pred = new Predictions();
            model_.Predict(words, results, threshold, ref pred, state);
            List<LanguageMatch> predictions = new List<LanguageMatch>();
            
            pred.ForEach(x => predictions.Add(new LanguageMatch((float) Math.Exp(x.Item1), dict_.GetLabel(x.Item2).Remove(0,9))));

            return predictions;
        }        

        /// <summary>
        /// Predicts a single label from input text.
        /// </summary>
        /// <param name="text">Text to predict a label from.</param>
        /// <returns>Single prediction.</returns>
        public LanguageMatch TellLanguage(string text)
        {            
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                return TellLanguage(stream);
            }
        }

        /// <summary>
        /// Predicts a single label from input text.
        /// </summary>
        /// <param name="text">Text to predict a label from.</param>
        /// <returns>Single prediction.</returns>
        public IEnumerable<LanguageMatch> TellMainLanguages(string text)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                return TellMainLanguages(stream);
            }
        }

        private Dictionary<string, LanguageIdentification> AnalyzeLanguages(List<(int, LanguageMatch)> predictions)
        {
            Dictionary<string, LanguageIdentification> languages = new Dictionary<string, LanguageIdentification>();

            foreach (var p in predictions)
            {
                if (languages.ContainsKey(p.Item2.Language))
                {
                    languages[p.Item2.Language].Value += p.Item2.Probability * p.Item1;

                    languages[p.Item2.Language].AverageProbability =
                        ((languages[p.Item2.Language].AverageProbability * languages[p.Item2.Language].Matches) + p.Item2.Probability) / (languages[p.Item2.Language].Matches + 1);

                    languages[p.Item2.Language].Matches += 1;
                    languages[p.Item2.Language].Words += p.Item1;
                }
                else
                {
                    languages.Add(p.Item2.Language, new LanguageIdentification()
                    {
                        Matches = 1,
                        Value = p.Item2.Probability * p.Item1,
                        AverageProbability = p.Item2.Probability,
                        Words = p.Item1
                    });
                }
            }            

            return languages;
        }

        private IEnumerable<LanguageMatch> DetermineMain(List<(int, LanguageMatch)> predictions, int totalWords)
        {
            Dictionary<string, LanguageIdentification> languages = AnalyzeLanguages(predictions);

            IEnumerable<LanguageMatch> main =
                from l in languages
                where (float) l.Value.Words / totalWords >= ThresholdMainLanguage                
                orderby (float) l.Value.Words / totalWords descending
                select new LanguageMatch(l.Value.AverageProbability, l.Key, (float)l.Value.Words / totalWords);
            
            return main;
        }

        private LanguageMatch DetermineBest(List<(int, LanguageMatch)> predictions)
        {
            Dictionary<string, LanguageIdentification> languages = AnalyzeLanguages(predictions);

            if (languages.Count > 0)
            {
                var prediction = languages.OrderByDescending(x => x.Value.Value).First();

                return new LanguageMatch(prediction.Value.AverageProbability, prediction.Key);
            }
            else
            {
                return new LanguageMatch(false);
            }
        }

        /// <summary>
        /// Predicts a single label from input text.
        /// </summary>
        /// <param name="text">Text to predict a label from.</param>
        /// <returns>Single prediction.</returns>
        public LanguageMatch TellLanguage(Stream input)
        {            
            using(BinaryReader sr = new BinaryReader(input))
            {
                List<int> words = new List<int>();
                List<int> labels = new List<int>();

                int wordsFound = 0;                
                List<(int, LanguageMatch)> predictions = new List<(int, LanguageMatch)>();
                
                while ((wordsFound = dict_.GetLine(sr, ref words, ref labels)) != 0)
                {
                    var listPred = Identify(words);
                    listPred.ForEach(x => predictions.Add((wordsFound, x)));                    
                }
                
                return DetermineBest(predictions);                
            }            
        }
        
        /// <summary>
        /// Returns the main languages in the text.
        /// </summary>
        /// <param name="text">Text to predict a label from.</param>
        /// <returns>Single prediction.</returns>
        public IEnumerable<LanguageMatch> TellMainLanguages(Stream input)
        {
            using (BinaryReader sr = new BinaryReader(input))
            {
                List<int> words = new List<int>();
                List<int> labels = new List<int>();

                int wordsFound = 0;
                int totalWords = 0;
                List<(int, LanguageMatch)> predictions = new List<(int, LanguageMatch)>();
               

                while ((wordsFound = dict_.GetLine(sr, ref words, ref labels)) != 0)
                {
                    totalWords += wordsFound;
                    var listPred = Identify(words);
                    listPred.ForEach(x => predictions.Add((wordsFound, x)));
                }

                return DetermineMain(predictions, totalWords);
            }
        }

        /// <summary>
        /// Returns a language for each line       
        /// </summary>
        /// <param name="text">Text to predict a label from.</param>
        /// <returns>Single prediction.</returns>
        public IEnumerable<LanguageMatch> TellAllLanguages(Stream input)
        {
            using (BinaryReader sr = new BinaryReader(input))
            {
                List<int> words = new List<int>();
                List<int> labels = new List<int>();

                int wordsFound = 0;
                int totalWords = 0;
                List<(int, LanguageMatch)> predictions = new List<(int, LanguageMatch)>();

                while ((wordsFound = dict_.GetLine(sr, ref words, ref labels)) != 0)
                {
                    totalWords += wordsFound;
                    var listPred = Identify(words);
                    listPred.ForEach(x => predictions.Add((wordsFound, x)));
                }

                return (from p in predictions
                       select p.Item2);
            }
        }

        /// <summary>
        /// Returns a language for each line       
        /// </summary>
        /// <param name="text">Text to predict a label from.</param>
        /// <returns>Single prediction.</returns>
        public IEnumerable<LanguageMatch> TellAllanguages(string text)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                writer.Write(text);
                writer.Flush();
                stream.Position = 0;

                return TellAllLanguages(stream);
            }
        }
    }            
}
