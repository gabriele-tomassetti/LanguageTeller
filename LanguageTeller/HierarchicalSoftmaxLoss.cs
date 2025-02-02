using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace LanguageTeller
{
    using Predictions = List<(float, int)>;

    class HierarchicalSoftmaxLoss : BinaryLogisticLoss
    {
        protected class Node
        {
            public int parent;
            public int left;
            public int right;
            public long count;
            public bool binary;
        };

        protected List<List<int>> paths_ = new List<List<int>>();
        protected List<List<bool>> codes_ = new List<List<bool>>();
        protected List<Node> tree_ = new List<Node>();
        protected int osz_;

        protected void BuildTree(List<long> counts)
        {
            for (int a = tree_.Count; a < 2 * osz_ - 1; a++)
            {
                tree_.Add(new Node());
            }
            for (int i = 0; i < 2 * osz_ - 1; i++)
            {
                tree_[i].parent = -1;
                tree_[i].left = -1;
                tree_[i].right = -1;
                tree_[i].count = (long)1e15;
                tree_[i].binary = false;
            }
            for (int i = 0; i < osz_; i++)
            {
                tree_[i].count = counts[i];
            }
            int leaf = osz_ - 1;
            int node = osz_;
            for (int i = osz_; i < 2 * osz_ - 1; i++)
            {
                int[] mini = new int[2];
                for (int j = 0; j < 2; j++)
                {
                    if (leaf >= 0 && tree_[leaf].count < tree_[node].count)
                    {
                        mini[j] = leaf--;
                    }
                    else
                    {
                        mini[j] = node++;
                    }
                }
                tree_[i].left = mini[0];
                tree_[i].right = mini[1];
                tree_[i].count = tree_[mini[0]].count + tree_[mini[1]].count;
                tree_[mini[0]].parent = i;
                tree_[mini[1]].parent = i;
                tree_[mini[1]].binary = true;
            }
            for (int i = 0; i < osz_; i++)
            {
                List<int> path = new List<int>();
                List<bool> code = new List<bool>();
                int j = i;
                while (tree_[j].parent != -1)
                {
                    path.Add(tree_[j].parent - osz_);
                    code.Add(tree_[j].binary);
                    j = tree_[j].parent;
                }
                paths_.Add(path);
                codes_.Add(code);
            }
        }

        protected void Dfs(int k, float threshold, int node,
                           float score, ref Predictions heap, ref Vector hidden)
        {
            if (score < Log(threshold))
            {
                return;
            }
            if (heap.Count == k && score < heap.First().Item1)
            {
                return;
            }

            if (tree_[node].left == -1 && tree_[node].right == -1)
            {
                heap.Add((score, node));
                heap.Sort((x, y) => ComparePairs(x, y));
                if (heap.Count > k)
                {
                    heap.RemoveAt(0);
                }

                return;
            }

            float f;
            f = wo_.DotRow(hidden, node - osz_);
            f = 1.0f / (1 + (float)Math.Exp(-f));

            Dfs(k, threshold, tree_[node].left, score + Log(1.0f - f), ref heap, ref hidden);
            Dfs(k, threshold, tree_[node].right, score + Log(f), ref heap, ref hidden);
        }

        public HierarchicalSoftmaxLoss(Matrix wo, List<long> targetCounts) : base(wo)
        {
            paths_ = new List<List<int>>();
            codes_ = new List<List<bool>>();
            tree_ = new List<Node>();
            osz_ = targetCounts.Count;
            BuildTree(targetCounts);
        }

        public override void Predict(int k, float threshold, ref Predictions heap, ref State state)
        {
            Dfs(k, threshold, 2 * osz_ - 2, 0.0f, ref heap, ref state.hidden);
            heap.Sort(ComparePairs);
        }
    }
}
