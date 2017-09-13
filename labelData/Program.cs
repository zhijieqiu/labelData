using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using java.io;
using java.util;
using edu.stanford.nlp.ling;
using edu.stanford.nlp.tagger.maxent;
namespace labelData
{
    class Program
    {
        private static float SearchTopK(string query)
        {
            List<string> lis = new List<string>();
            query = query.Replace(" ", "+");
            //           string link = @"http://stcsrv-e57:8083/api/?q=" + query + "&w=" + strW + "&t=" + topK;
            string link = @"http://13.90.251.141:8984/api/search/?q=" + query + "&t1=100&t2=5";
            //           string link = @"http://stcsrv-e57:8984/api/search/?q=" + query + "&t1=1000&t2=10000&z=1&f=1&idf=1";
            // string link = @"http://stcsrv-e57:8984/api/search/?q=" + query + "&t1=1000&t2=1000&idf=true&alpha="+alpha+"&z=true&f=false";
            //string link = @"http://stcsrv-e57:8984/api/search/?q=" + query + "&t1=1000&t2=10000&idf=true&alpha=" + alpha + "&z=true";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(link); //
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream ResStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(ResStream, Encoding.Default);

            string res = streamReader.ReadToEnd();

            JavaScriptObject json = (JavaScriptObject)JavaScriptConvert.DeserializeObject(res);
            List<string> temp = new List<string>();
            float result = 0.0f;
            JavaScriptArray docs = (JavaScriptArray)json["docs"];
            for (int i = 0; i < docs.Count; ++i)
            {
                JavaScriptObject doc = (JavaScriptObject)docs[i];
                temp.Add(doc["score"].ToString());
            }
            foreach (string item in temp)
            {
                result += float.Parse(item);
            }
            result /= temp.Count();
            return result;
        }
        public void labelData(string inputfile, string posfile, string negfile)
        {
            string[] lines = System.IO.File.ReadAllLines(inputfile);
            StreamWriter pos = new StreamWriter(posfile);
            StreamWriter neg = new StreamWriter(negfile);
            int i = 0;
            foreach (string line in lines)
            {
                try
                {
                    float score = SearchTopK(line);
                    if (score.CompareTo(0.85f) >= 0)
                    {
                        pos.WriteLine(line);

                    }
                    else
                    {
                        neg.WriteLine(line);
                    }
                    System.Console.WriteLine("success" + i.ToString());
                    i++;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                }

            }
            pos.Close();
            neg.Close();
        }
        public void labelposData(string inputfile, string posfile)
        {
            string[] lines = System.IO.File.ReadAllLines(inputfile);
            StreamWriter pos = new StreamWriter(posfile);

            int i = 0;
            foreach (string line in lines)
            {
                if (i < 60000)
                {
                    try
                    {
                        float score = SearchTopK(line);
                        if (score.CompareTo(0.85f) > 0)
                        {
                            pos.WriteLine(line);

                        }
                        System.Console.WriteLine("success" + i.ToString());
                        i++;
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine(e.Message);
                    }
                }


            }
            pos.Close();

        }
        public void produce(string pos, string neg, string output)
        {
            string[] poslines = System.IO.File.ReadAllLines(pos);
            List<string> neglines = new List<string>();
            using (StreamReader sr = new StreamReader(neg))
            {
                int i = 0;
                string line = "";
                while (i < 8000 && (line = sr.ReadLine()) != null)
                {
                    neglines.Add(line);
                    i++;
                }
            }
            int docid = 0;
            using (StreamWriter sw = new StreamWriter(output))
            {
                foreach (string line in neglines)
                {
                    sw.WriteLine("0" + "\t" + docid.ToString() + "\t" + "\t" + line);
                    docid++;
                }
                foreach (string line in poslines)
                {
                    sw.WriteLine("1" + "\t" + docid.ToString() + "\t" + "\t" + line);
                    docid++;
                }
            }
        }
        public void stastic(string input, string output)
        {
            Dictionary<string, int> dic = new Dictionary<string, int>();
            string[] lines = System.IO.File.ReadAllLines(input);
            foreach (string line in lines)
            {
                string[] parts = line.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach (string part in parts)
                {
                    if (!dic.ContainsKey(part)) dic[part] = 0;
                    dic[part] += 1;
                }
            }
            Dictionary<string, int> newdic = (from pair in dic
                                              where pair.Value >= 10
                                              select pair).ToDictionary(k => k.Key, k => k.Value);
            using (StreamWriter sw = new StreamWriter(output))
            {
                foreach (var item in newdic)
                {
                    sw.WriteLine(item.Key + "\t" + item.Value);
                }
            }

        }
        public void groundTruth(string inputfile, string outputfile)
        {
            string[] lines = System.IO.File.ReadAllLines(inputfile);
            using (StreamWriter sw = new StreamWriter(outputfile))
            {
                foreach (string line in lines)
                {
                    string[] parts = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    sw.WriteLine(parts[2] + "\t" + parts[1] + "\t" + parts[0]);
                }
            }
        }
        public void filter(string inputfile, string outputfile)
        {
            string[] lines = System.IO.File.ReadAllLines(inputfile);
            using (StreamWriter sw = new StreamWriter(outputfile))
            {
                foreach (string line in lines)
                {
                    string[] parts = line.Split("\t".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                    if (parts[0] == "1")
                    {
                        string[] wordnum = parts[2].Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
                        if (wordnum.Length >= 6)
                        {
                            sw.WriteLine(parts[2]);
                        }
                    }

                }
            }
        }

        static void test(string[] args)
        {
            
            var jarRoot = @"D:\work\stanford-postagger-2015-12-09\stanford-postagger-2015-12-09\";
            var modelsDirectory = jarRoot + @"\models";

            // Loading POS Tagger
            var tagger = new MaxentTagger(modelsDirectory + @"\english-bidirectional-distsim.tagger");

            // Text for tagging
            var text = "A Part-Of-Speech Tagger (POS Tagger) is a piece of software that reads text"
                       + "in some language and assigns parts of speech to each word (and other token),"
                       + " such as noun, verb, adjective, etc., although generally computational "
                       + "applications use more fine-grained POS tags like 'noun-plural'.";

            var sentences = MaxentTagger.tokenizeText(new java.io.StringReader(text)).toArray();
            foreach (ArrayList sentence in sentences)
            {
                var taggedSentence = tagger.tagSentence(sentence);
                System.Console.WriteLine(Sentence.listToString(taggedSentence, false));
            }
            //String query = "shared mailbox set up";
            //result = SearchTopK(query);
            //foreach (String item in result)
            //{
            //    Console.WriteLine(item);
            //}
            //Console.ReadKey();
            string basepath = @"C:\Users\v-chuhz\SentenceClassifier\";
            string inputfile = basepath + @"trainset.txt";
            string outputfile = basepath + @"groundTruth1.txt";
            //string pos = basepath + @"pos.27.txt";
            //string neg = basepath + @"neg.27.txt";
            Program g = new Program();
            g.filter(inputfile, outputfile);
            //g.labelData(inputfile, pos, neg);
            //g.groundTruth(inputfile, outputfile);
            //g.stastic(inputfile, outputfile);
            //g.produce(pos, neg, outputfile);
            //g.labelposData(inputfile, pos);
            //Random m = new Random(1);
            //int i = 0;
            //while (i < 10)
            //{
            //    int temp = m.Next(4);
            //    Console.Write(temp);
            //    i++;
            //}
            //Console.ReadKey();

        }
    }
}
  