using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labelData
{
    class Predict
    {
        static string modelfile = @"data\13.model.ini";
        static FastTree FT;
        sentenceClassifier sc;
        logist log;
        public void init(string modelfile)
        {
            sc = new sentenceClassifier();
            sc.init();
            //log = new logist();
            FT = new FastTree();
            FT.SetModelDimension(8);
            FT.Load(modelfile);
        }
        // label+feature
        public string predict_fasttree(string query)
        {
            int label = 0;          
            Dictionary<int, double> allfeature = sc.allfeature(query);
            double sum = FT.Cal_Sim_FastTree(allfeature);
            if (sum > 0) label=1;
            string result = label.ToString() ;
            foreach(var pair in allfeature)
            {
                result += "\t" + pair.Value;
            }
            return result;
        }
        public int predict_log(string query)
        {
            Dictionary<int, double> allfeature = sc.allfeature(query);
            double sum = log.cal_sim(allfeature);
            if (sum > 0.85) return 1;
            else return 0;
        }
        /*static void Main(string[] args)
        {
            Predict p = new Predict();
            p.init(modelfile);
            string path = @"data\groundtruth1.txt";
            string output = @"data\groundtruth4.1.txt";
            string[] lines = System.IO.File.ReadAllLines(path);
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(output))
            {
                foreach (string line in lines)
                {
                    string[] parts = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    string pre = p.predict_fasttree(parts[0]);
                    sw.WriteLine(line + "\t" + pre);
                }
            }

        }*/
        //static void Main(string[] args)
        //{
        //    Predict p = new Predict();
        //    p.init(modelfile);
        //    while (true)
        //    {
        //        Console.WriteLine("Enter query:"); // Prompt
        //        string query = Console.ReadLine(); // Get string from user
        //        if (query == "exit") // Check string
        //        {
        //            break;
        //        }
        //        int flag = p.predict_fasttree(query);
        //        if (flag == 1)
        //        {
        //            Console.WriteLine("This query need not to be asked question!");
        //        }
        //        else
        //        {
        //            Console.WriteLine("please ask question!");
        //        }

        //    }
        //    string goodquery = "I want to learn how to use onedrive to share files with staff";
        //    string badquery = "i want to know how to install office in my macbook";
        //    Console.WriteLine("good " + p.predict(goodquery));
        //    Console.WriteLine("bad " + p.predict(badquery));


        //    Console.ReadKey();
        //}
    }
}
