using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace labelData
{
    class EvaluatorDT
    {
        public int eid;
        public int numNodes;
        public List<int> splitFeatures = new List<int>();
        public List<double> threshold = new List<double>();
        public List<int> lteChild = new List<int>();
        public List<int> gtChild = new List<int>();
        public List<double> outputs = new List<double>();

        public bool Load(Dictionary<string, string> inputLines, Dictionary<int, int> input2Feature)
        {
            // evaluator Id
            string line = inputLines["Evaluator"];
            eid = int.Parse(line);
            // node num
            line = inputLines["NumInternalNodes"];            
            numNodes = int.Parse(line);
            // split features
            line = inputLines["SplitFeatures"];            
            string[] inputIds = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (inputIds.Length != numNodes)
                return false;
            foreach (string iid in inputIds)
            {
                string[] cols = iid.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                int inputid = int.Parse(cols[1]);
                splitFeatures.Add(input2Feature[inputid]);
            }
            // ltechild
            line = inputLines["LTEChild"];
            string[] ltes = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (ltes.Length != numNodes)
                return false;
            foreach (string lc in ltes)
            {
                lteChild.Add(int.Parse(lc));
            }
            // gtchild
            line = inputLines["GTChild"];            
            string[] gtes = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (gtes.Length != numNodes)
                return false;
            foreach (string lc in gtes)
            {
                gtChild.Add(int.Parse(lc));
            }
            // threshold
            line = inputLines["Threshold"];            
            string[] thresh = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (thresh.Length != numNodes)
                return false;
            foreach (string th in thresh)
            {
                threshold.Add(double.Parse(th));
            }
            // output
            line = inputLines["Output"];            
            string[] otps = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (otps.Length != numNodes+1)
                return false;
            foreach (string ot in otps)
            {
                outputs.Add(double.Parse(ot));
            }
            return true;
        }

        public double Evaluate(Dictionary<int, double> fValuas)
        {
            int nodeIdx = 0;
            while (nodeIdx >= 0)
            {
                int fId = splitFeatures[nodeIdx];
                double fv = 0.0;
                if (fValuas.ContainsKey(fId))
                    fv = fValuas[fId];
                if (fv <= threshold[nodeIdx])
                    nodeIdx = lteChild[nodeIdx];
                else
                    nodeIdx = gtChild[nodeIdx];
            }
            return outputs[-nodeIdx - 1];
        }
    }

    class EvaluatorSum
    {
        public int numNodes;
        public double bias;
        public Dictionary<int, double> weights = new Dictionary<int, double>();
        public bool LoadWeights(Dictionary<string, string> inputLines)
        {
            // num nodes
            numNodes = int.Parse(inputLines["NumNodes"]);
            // nodes & weight
            string[] nodes = inputLines["Nodes"].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            string[] wgts = inputLines["Weights"].Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (nodes.Length != wgts.Length || nodes.Length != numNodes)
                return false;
            for (int i = 0; i < numNodes; ++i)
            {
                weights.Add(int.Parse(nodes[i].Substring(2)), double.Parse(wgts[i]));
            }
            // bias
            bias = double.Parse(inputLines["Bias"]);
            return true;
        }
        public double Evaluate(Dictionary<int, double> eScores)
        {
            double sum = 0;
            foreach (var kv in eScores)
            {
                sum += weights[kv.Key] * kv.Value;
            }
            return sum;
        }
    }

    class EvaluatorSig
    {
        public int numNodes;
        public double bias;
        public double weight;
        public bool LoadSigmoid(Dictionary<string, string> inputLines)
        {
            // num nodes
            numNodes = int.Parse(inputLines["NumNodes"]);
            // bias & weight
            bias = double.Parse(inputLines["Bias"]);
            weight = double.Parse(inputLines["Weights"]);
            return true;
        }
        public double Evaluate(double esum)
        {
            return 1.0 / (1.0 + Math.Exp(-1.0 * weight * esum - bias)); 
        }
    }

    public class FastTree
    {
        private Dictionary<int, int> inputId2FeatureId = new Dictionary<int, int>();
        private List<EvaluatorDT> decisionTrees = new List<EvaluatorDT>();        
        private EvaluatorSum aggregatorSum = new EvaluatorSum();
        private EvaluatorSig aggregatorSig = new EvaluatorSig();
        public double threshold;
        private int numInputs;
        private int numEvaluators;
        private bool inited;

        private static int model_dimension = 438;

        public void SetModelDimension(int m_dim)
        {
            model_dimension = m_dim;
        }


        private void ExtractKeyValue(string line, string del, Dictionary<string, string> kvdict)
        {
            string[] cols = line.Split(del.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            kvdict.Add(cols[0], cols[1]);
        }

        public bool Load(string modelFile)
        {
            inited = false;
            using (FileStream frfs = File.Open(modelFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (StreamReader sr = new StreamReader(frfs))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (String.IsNullOrEmpty(line))
                            continue;
                        if (line.Contains("[TreeEnsemble]"))
                        {
                            line = sr.ReadLine();
                            line = line.Substring(line.IndexOf('=') + 1);
                            numInputs = int.Parse(line);
                            line = sr.ReadLine();
                            line = line.Substring(line.IndexOf('=') + 1);
                            numEvaluators = int.Parse(line);
                            threshold = 0.01;
                            line = sr.ReadLine();
                            if (String.IsNullOrEmpty(line) == false && line.Contains("Threshold"))
                            {
                                line = line.Substring(line.IndexOf('=') + 1);
                                threshold = double.Parse(line);
                            }
                        }
                        else if (line.Contains("[Input:"))
                        {
                            Dictionary<string, string> inputLines = new Dictionary<string, string>();
                            line = line.Trim("[]".ToCharArray());
                            ExtractKeyValue(line, ":", inputLines);
                            line = sr.ReadLine();
                            while (String.IsNullOrEmpty(line) == false)
                            {
                                ExtractKeyValue(line, "=", inputLines);
                                line = sr.ReadLine();
                            }
                            // add input id mapping
                            int iid = int.Parse(inputLines["Input"]);
                            string fname = inputLines["Name"];
                            fname = fname.Substring(1);
                            inputId2FeatureId.Add(iid, int.Parse(fname));
                        }
                        else if (line.Contains("[Evaluator:"))
                        {
                            Dictionary<string, string> inputLines = new Dictionary<string, string>();
                            line = line.Trim("[]".ToCharArray());
                            ExtractKeyValue(line, ":", inputLines);
                            line = sr.ReadLine();
                            while (String.IsNullOrEmpty(line) == false)
                            {
                                ExtractKeyValue(line, "=", inputLines);
                                line = sr.ReadLine();
                            }
                            // dt, sum, sig
                            if (inputLines["EvaluatorType"] == "DecisionTree")
                            {
                                EvaluatorDT dt = new EvaluatorDT();
                                dt.Load(inputLines, inputId2FeatureId);
                                decisionTrees.Add(dt);
                            }
                            else if (inputLines["EvaluatorType"] == "Aggregator")
                            {
                                if (inputLines["Type"] == "Linear")
                                {
                                    aggregatorSum.LoadWeights(inputLines);
                                }
                                if (inputLines["Type"] == "Sigmoid")
                                {
                                    aggregatorSig.LoadSigmoid(inputLines);
                                }
                            }
                        }
                    }
                }
            }
            inited = true;

            //FormatUtil.Load(paramFile);

            return inited;
        }

        
        /*
        public double Predict(Dictionary<int, double> rawCateIdScore1, Dictionary<int, double> rawCateIdScore2) 
        {
            Dictionary<int, double> v1 = FormatUtil.GenFeaturesForScopeICE(rawCateIdScore1, threshold);
            Dictionary<int, double> v2 = FormatUtil.GenFeaturesForScopeICE(rawCateIdScore2, threshold);

            Dictionary<int, double> rs = FormatUtil.ComputeFeature(v1, v2);

            return Cal_Sim_FastTree(rs);
        }

        public double PredictDotProduct(Dictionary<int, double> rawCateIdScore1, Dictionary<int, double> rawCateIdScore2)
        {
            Dictionary<int, double> v1 = FormatUtil.GenFeaturesForScopeICE(rawCateIdScore1, threshold);
            Dictionary<int, double> v2 = FormatUtil.GenFeaturesForScopeICE(rawCateIdScore2, threshold);

            Dictionary<int, double> rs = FormatUtil.ComputeFeatureDotProduct(v1, v2);

            return Cal_Sim_FastTree(rs);
        }

        // 6360, 1st half features are split from cosine fomula, 2nd part is from Math.Abs()
        public double Predict6360(Dictionary<int, double> rawCateIdScore1, Dictionary<int, double> rawCateIdScore2)
        {
            Dictionary<int, double> rs = FormatUtil.ComputeFeature6360(rawCateIdScore1, rawCateIdScore2);

            return Cal_Sim_FastTree(rs);
        }

        public double Predict(string cateid_scores1, string cateid_scores2, int topN1, int topN2, int Dim) //give me the "largest feature index"
        {
            if (Dim > model_dimension)
            {
                Console.WriteLine("wrong dimension assignment, Dim should not be larger than {0}", model_dimension);
                return -1.0;
            }

            Dictionary<int, double> v1 = FormatUtil.ExtractVec(cateid_scores1, topN1, Dim);
            Dictionary<int, double> v2 = FormatUtil.ExtractVec(cateid_scores2, topN2, Dim);

            Dictionary<int, double> rs = FormatUtil.ComputeFeature(v1, v2);

            return Cal_Sim_FastTree(rs);
        }
        */
        public double Cal_Sim_FastTree(Dictionary<int, double> rs)
        {
            if (rs == null)
                return 0;
            if (rs.Count == 0)
                return 0;

            Dictionary<int, double> dtscores = new Dictionary<int, double>();
            
            for (int i = 0; i < decisionTrees.Count; i++)
            {
                double s = decisionTrees[i].Evaluate(rs);
                dtscores.Add(decisionTrees[i].eid, s);
            }
            double sums = aggregatorSum.Evaluate(dtscores);
            //double sigs = aggregatorSig.Evaluate(sums);

            return sums;
        }
        
    }
}
