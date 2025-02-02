using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LanguageTeller
{
    public enum model_name { cbow = 1, sg, sup };
    public enum loss_name { hs = 1, ns, softmax, ova };

    public class Args
    {
        protected string LossToString(loss_name ln)
        {
            switch (ln)
            {
                case loss_name.hs:
                    return "hs";
                case loss_name.ns:
                    return "ns";
                case loss_name.softmax:
                    return "softmax";
                case loss_name.ova:
                    return "one-vs-all";
            }
            return "Unknown loss!"; // should never happen
        }

        protected string BoolToString(bool b)
        {
            if (b)
            {
                return "true";
            }
            else
            {
                return "false";
            }
        }
        protected string ModelToString(model_name mn)
        {
            switch (mn) {
                case model_name.cbow:
                return "cbow";
                case model_name.sg:
                return "sg";
                case model_name.sup:
                return "sup";
            }

            return "Unknown model name!"; // should never happen
        }

        public string input;
        public string output;
        public double lr;
        public int lrUpdateRate;
        public int dim;
        public int ws;
        public int epoch;
        public int minCount;
        public int minCountLabel;
        public int neg;
        public int wordNgrams;
        public loss_name loss;
        public model_name model;
        public int bucket;
        public int minn;
        public int maxn;
        public int thread;
        public double t;
        public string label;
        public int verbose;
        public string pretrainedVectors;
        public bool saveOutput;

        public bool qout;
        public bool retrain;
        public bool qnorm;
        public int cutoff;
        public int dsub;

        public Args()
        {
            lr = 0.05;
            dim = 100;
            ws = 5;
            epoch = 5;
            minCount = 5;
            minCountLabel = 0;
            neg = 5;
            wordNgrams = 1;
            loss = loss_name.ns;
            model = model_name.sg;
            bucket = 2000000;
            minn = 3;
            maxn = 6;
            thread = 12;
            lrUpdateRate = 100;
            t = 1e-4;
            label = "__label__";
            verbose = 2;
            pretrainedVectors = "";
            saveOutput = false;

            qout = false;
            retrain = false;
            qnorm = false;
            cutoff = 0;
            dsub = 2;
        }        
        
        public void ParseArgs(List<string> args)
        {
            string command = args[1];

            if (command == "supervised")
            {
                model = model_name.sup;
                loss = loss_name.softmax;
                minCount = 1;
                minn = 0;
                maxn = 0;
                lr = 0.1;
            }
            else if (command == "cbow")
            {
                model = model_name.cbow;
            }
            for (int ai = 2; ai < args.Count; ai += 2)
            {
                if (args[ai][0] != '-')
                {
                    Console.Error.WriteLine("Provided argument without a dash! Usage:");
                    PrintHelp();
                    System.Environment.Exit(1);
                }
                try
                {
                    if (args[ai] == "-h")
                    {
                        Console.Error.WriteLine("Here is the help! Usage:");
                        PrintHelp();
                        System.Environment.Exit(1);
                    }
                    else if (args[ai] == "-input")
                    {
                        input = args.ElementAt(ai + 1);
                    }
                    else if (args[ai] == "-output")
                    {
                        output = args.ElementAt(ai + 1);
                    }
                    else if (args[ai] == "-lr")
                    {
                        lr = float.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-lrUpdateRate")
                    {
                        lrUpdateRate = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-dim")
                    {
                        dim = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-ws")
                    {
                        ws = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-epoch")
                    {
                        epoch = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-minCount")
                    {
                        minCount = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-minCountLabel")
                    {
                        minCountLabel = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-neg")
                    {
                        neg = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-wordNgrams")
                    {
                        wordNgrams = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-loss")
                    {
                        if (args.ElementAt(ai + 1) == "hs")
                        {
                            loss = loss_name.hs;
                        }
                        else if (args.ElementAt(ai + 1) == "ns")
                        {
                            loss = loss_name.ns;
                        }
                        else if (args.ElementAt(ai + 1) == "softmax")
                        {
                            loss = loss_name.softmax;
                        }
                        else if (
                          args.ElementAt(ai + 1) == "one-vs-all" || args.ElementAt(ai + 1) == "ova")
                        {
                            loss = loss_name.ova;
                        }
                        else
                        {
                            Console.Error.WriteLine($"Unknown loss: {args.ElementAt(ai + 1)}");
                            PrintHelp();
                            System.Environment.Exit(1);
                        }
                    }
                    else if (args[ai] == "-bucket")
                    {
                        bucket = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-minn")
                    {
                        minn = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-maxn")
                    {
                        maxn = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-thread")
                    {
                        thread = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-t")
                    {
                        t = float.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-label")
                    {
                        label = args.ElementAt(ai + 1);
                    }
                    else if (args[ai] == "-verbose")
                    {
                        verbose = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-pretrainedVectors")
                    {
                        pretrainedVectors = args.ElementAt(ai + 1);
                    }
                    else if (args[ai] == "-saveOutput")
                    {
                        saveOutput = true;
                        ai--;
                    }
                    else if (args[ai] == "-qnorm")
                    {
                        qnorm = true;
                        ai--;
                    }
                    else if (args[ai] == "-retrain")
                    {
                        retrain = true;
                        ai--;
                    }
                    else if (args[ai] == "-qout")
                    {
                        qout = true;
                        ai--;
                    }
                    else if (args[ai] == "-cutoff")
                    {
                        cutoff = int.Parse(args.ElementAt(ai + 1));
                    }
                    else if (args[ai] == "-dsub")
                    {
                        dsub = int.Parse(args.ElementAt(ai + 1));
                    }
                    else
                    {
                        Console.Error.WriteLine($"Unknown argument: {args.ElementAt(ai)}");
                        PrintHelp();
                        System.Environment.Exit(1);                        
                    }
                }
                catch (IndexOutOfRangeException e)
                {
                    Console.Error.WriteLine($"{args.ElementAt(ai)} is missing an argument");
                    PrintHelp();
                    System.Environment.Exit(1);                   
                }
            }
            if (String.IsNullOrEmpty(input) || String.IsNullOrEmpty(output))
            {               
                Console.Error.WriteLine("Empty input or output path.");
                PrintHelp();
                System.Environment.Exit(1);
            }
            if (wordNgrams <= 1 && maxn == 0)
            {
                bucket = 0;
            }
        }

        public void PrintHelp()
        {
            PrintBasicHelp();
            PrintDictionaryHelp();
            PrintTrainingHelp();
            PrintQuantizationHelp();
        }

        public void PrintBasicHelp()
        { }

        public void PrintDictionaryHelp()
        { }

        public void PrintTrainingHelp()
        { }

        public void PrintQuantizationHelp()
        { }

        public void Save(FileStream output)
        { }

        public void Load(BinaryReader input)
        {
            dim = input.ReadInt32();
            ws = input.ReadInt32();
            epoch = input.ReadInt32();
            minCount = input.ReadInt32();
            neg = input.ReadInt32();
            wordNgrams = input.ReadInt32();
            loss = (loss_name) input.ReadInt32();
            model = (model_name) input.ReadInt32();
            bucket = input.ReadInt32();
            minn = input.ReadInt32();
            maxn = input.ReadInt32();
            lrUpdateRate = input.ReadInt32();
            t = input.ReadDouble();            
        }

        public void Dump(TextWriter output)
        {
            output.WriteLine($"dim {dim}");
            output.WriteLine($"ws {ws}");
            output.WriteLine($"epoch {epoch}");
            output.WriteLine($"minCount {minCount}");
            output.WriteLine($"neg {neg}");
            output.WriteLine($"wordNgrams {wordNgrams}");
            output.WriteLine($"loss {loss}");
            output.WriteLine($"model {model}");
            output.WriteLine($"bucket {bucket}");
            output.WriteLine($"minn {minn}");
            output.WriteLine($"maxn {maxn}");
            output.WriteLine($"lrUpdateRate {lrUpdateRate}");
            output.WriteLine($"t {t}");        
        }
    }
}
