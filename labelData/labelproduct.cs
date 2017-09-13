using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace labelData
{
    class labelproduct
    {
        Dictionary<int, string> indexproduct = new Dictionary<int, string>();
        static sentenceClassifier sc = new sentenceClassifier();
        void init()
        {
            sc.init();
            string path = @"C:\Users\v-chuhz\Dev\SupportAgent\ice\product\produc.label.txt";
            string[] lines = File.ReadAllLines(path);
            foreach(string line in lines)
            {
                string[] parts = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                indexproduct.Add(int.Parse(parts[0]), parts[1]);
            }
        }
        //public static void Main()
        //{
        //    string path = @"C:\Users\v-chuhz\SentenceClassifier\Query_classifier\pos_neg\trainset\groundtruth.txt";
        //    string output =@"C:\Users\v-chuhz\SentenceClassifier\Query_classifier\pos_neg\trainset\groundtruth1.txt";
        //    string[] lines = System.IO.File.ReadAllLines(path);
        //    using (System.IO.StreamWriter sw = new System.IO.StreamWriter(output))
        //    {
        //        foreach (string line in lines)
        //        {
        //            string[] parts = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries);
        //            if (parts[parts.Length - 1] == "1" || parts[parts.Length - 1] == "0")
        //            {
        //                sw.WriteLine(parts[0]+"\t"+ parts[parts.Length - 1]);
        //            }
                    
        //        }
        //    }
        //}
        //public static void Main()
        //{
        //    labelproduct lp = new labelproduct();
        //    lp.init();
        //    string path = @"C:\Users\v-chuhz\SentenceClassifier\Query_classifier\testBatch\batch_23";
        //    string output =@"C:\Users\v-chuhz\SentenceClassifier\Query_classifier\testBatch\batch_23.out";
        //    string[] lines = File.ReadAllLines(path);
        //    using (StreamWriter sw=new StreamWriter(output))
        //    {
        //        int i = 0;
        //        foreach (string line in lines)
        //        {
        //            sw.Write(line);
        //            List<int> ls = sc.predictProduct(line+"office 365");
        //            List<string> products = new List<string>();
        //            foreach (int id in ls)
        //            {
        //                products.Add(lp.indexproduct[id]);
        //            }
        //            foreach(string li in products)
        //            {
        //                sw.Write("\t" + li);
        //            }
        //            sw.WriteLine();
        //            Console.WriteLine("success" + i);
        //            i++;
        //        }
        //    }
                
        //}
        
    }
}
