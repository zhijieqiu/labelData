﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace labelData
{
    class PhraseExtractTool
    {
        //open the file and split by sentence, to see 
        //need a stopword list
        void loadStopWords(string dir)
        {
            stopwords.Clear();
            string fileName = dir + "stop_words.txt";
            StreamReader sr = new StreamReader(fileName);
            string line = null;
            while ((line = sr.ReadLine())!=null)
            {
                if (stopwords.Contains(line.ToLower()) == false)
                {
                    stopwords.Add(line.ToLower());
                }
            }
            sr.Close();
        }
        public string baseDir = @"D:\work\data\queryformat\";
        HashSet<string> stopwords = new HashSet<string>();
        void scanForMI(string fileName,string outDir)
        {
            string[] innerseperator = new string[] { " ", "\"", ",", "(", ")", "|", "[", "]", "{", "}", "+", "=", "~", "`", "/", ".", "?", ";", "!", ":" };
            Stemmer stemmer = new Stemmer(baseDir);
            //init stop words 
            loadStopWords(baseDir);
            StreamReader sr = new StreamReader(fileName);
            string line = null,aline=null;
            int total = 0, i = 0, j = 0;
            while((aline = sr.ReadLine())!=null)
            {
                string[] ts = aline.Split('\t');
                line = ts[3];
                for(i=4;i<=7;i++)
                {
                    if (ts[i] != "N/A") line += (". "+ts[i] );
                }
                line = line.ToLower();
                string[] senSeg = new string[] { ",",".",";","!","?"};
                string[] segs = line.Split(senSeg,StringSplitOptions.RemoveEmptyEntries);
                foreach(string sen in segs)
                {

                    string[] words = sen.Split(innerseperator,StringSplitOptions.RemoveEmptyEntries);
                    for (i = 0; i < words.Length; i++) words[i] = stemmer.GetBaseFormWord(words[i]);
                    foreach(string word in words)
                    {
                        if (stopwords.Contains(word)) continue;
                        total++;
                        if (uniGramInfo.ContainsKey(word)) uniGramInfo[word]++;
                        else uniGramInfo[word] = 1;
                    }
                    for(i = 0; i < words.Length; i++)
                    {
                        if (stopwords.Contains(words[i]))
                            continue;
                        for (j = 0; j < words.Length; j++)
                        {
                            if (stopwords.Contains(words[j])||words[i]==words[j]) continue;
                            if (bigramInfo.ContainsKey(words[i] + "&&&" + words[j]))
                                bigramInfo[words[i] + "&&&" + words[j]]++;
                            else
                                bigramInfo[words[i] + "&&&" + words[j]] = 1;
                        }
                    }
                }
            }
            foreach (KeyValuePair<string, double> unikv in uniGramInfo.ToList())
            {
                uniGramInfoCopy[unikv.Key] = unikv.Value;
                uniGramInfo[unikv.Key] /= total;
            }
            foreach (KeyValuePair<string, double> bigram in bigramInfo.ToList())
            {
                bigramInfo[bigram.Key] /= total;
                string[] ss = new string[] { "&&&"};
                string[] tokens = bigram.Key.Split(ss,StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length>1&&uniGramInfo.ContainsKey(tokens[0]) && uniGramInfo.ContainsKey(tokens[1]))
                {
                    bigramInfo[bigram.Key] /= (uniGramInfo[tokens[0]]*uniGramInfo[tokens[1]]);
                }
            }
            string outFileName = outDir+"MIinfo.txt";
            StreamWriter sw = new StreamWriter(outFileName);
           
            foreach(KeyValuePair<string,double> bigram in bigramInfo){
                sw.WriteLine(bigram.Key+":"+bigram.Value) ;

            }
            sw.Close();
            sr.Close();
        }
        void LoadMIModel(string fileName)
        {
            StreamReader sr = new StreamReader(fileName);
            string line = null;
            bigramInfo.Clear();
            while((line = sr.ReadLine()) != null)
            {
                string[] tokens = line.Split(":".ToArray());
                bigramInfo[tokens[0]] = double.Parse(tokens[1]);
            }
            sr.Close();
        }
        Dictionary<string, double> uniGramInfo = new Dictionary<string, double>();
        Dictionary<string, double> bigramInfo = new Dictionary<string, double>();
        Dictionary<string, double> uniGramInfoCopy = new Dictionary<string, double>();
        void scanForUniGramBigram(string fileName)
        {
            string[] innerseperator = new string[] { " ", "\"", ",", "(", ")", "|", "[", "]", "{", "}", "+", "=", "~", "`", "/",".","?",";","!",":" };
            StreamReader sr = new StreamReader(fileName);
            string line = null;
            double total = 0;
            double bigramTotal = 0;
            string tline = null;
            while((tline = sr.ReadLine()) != null)
            {
                line = tline.Split('\t')[3].ToLower();
                string[] tokens = line.Split(innerseperator,StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                foreach(string token in tokens)
                {
                    total++;
                    if (uniGramInfo.ContainsKey(token) == false)
                    {
                        uniGramInfo.Add(token, 1);
                    }
                    else
                        uniGramInfo[token]++;
                    if (i != tokens.Length - 1)
                    {
                        bigramTotal++;
                        if (bigramInfo.ContainsKey(tokens[i] + "&" + tokens[i + 1]) == false)
                        {
                            bigramInfo.Add(tokens[i] + "&" + tokens[i + 1], 1);
                        }
                        else
                        {
                            bigramInfo[tokens[i] + "&" + tokens[i + 1]]++;
                        }
                    }
                    i++;
                }
            }
            
            foreach (KeyValuePair<string,double> unikv in uniGramInfo.ToList())
            {
                uniGramInfoCopy[unikv.Key] = unikv.Value;
                uniGramInfo[unikv.Key] /= total;
            }
            double aa = bigramTotal * bigramTotal;
            foreach(KeyValuePair<string,double> bigram in bigramInfo.ToList() )
            {
                bigramInfo[bigram.Key] /= aa;
            }
            sr.Close();
        }
        private static int compare2(KeyValuePair<string,double> a,KeyValuePair<string,double> b)
        {
            return -a.Value.CompareTo(b.Value);
        }
        void sortByScore()
        {
            Dictionary<string, double> bigScore = new Dictionary<string, double>();
            foreach(string bigram in bigramInfo.Keys)
            {
                string[] seprator_ = new string[]{ "&"};
                string[] tokens = bigram.Split(seprator_, StringSplitOptions.None);
                
                if (uniGramInfo.ContainsKey(tokens[0]) && uniGramInfo.ContainsKey(tokens[1]))
                {
                    if (uniGramInfoCopy[tokens[0]] < 100 || uniGramInfoCopy[tokens[1]] < 100)
                        continue;
                    bigScore[bigram] = bigramInfo[bigram] / (uniGramInfo[tokens[0]] * uniGramInfo[tokens[1]]);
                }
            }
            List<KeyValuePair<string, double>> bigramList = bigScore.ToList();
            bigramList.Sort(compare2);
            string outFileName = "D:\\tmp\\myphrase2.txt";
            StreamWriter sw = new StreamWriter(outFileName);
            foreach(KeyValuePair<string,double> kv in bigramList)
            {
                sw.WriteLine(kv.Key+":"+kv.Value);
            }
            sw.Close();
        }
        void removeTwoWords(string line)
        {
            line = line.ToLower();
            string[] innerseperator = new string[] { " ", "\"", ",", "(", ")", "|", "[", "]", "{", "}", "+", "=", "~", "`", "/", ".", "?", ";", "!", ":" };
            string[] tokens = line.Split(innerseperator,StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, double> strToScore = new Dictionary<string, double>();
            Stemmer stemmer = new Stemmer(baseDir);
            for (int i = 0; i < tokens.Length; i++) tokens[i] = stemmer.GetBaseFormWord(tokens[i].ToLower()); 
            for(int i = 0; i < tokens.Length; i++)
            {
                int cnt = 0;
                double score = 0.0;
                for(int j = 0; j < tokens.Length; j++)
                {
                    if (tokens[i] == tokens[j]) continue;
                    cnt++;
                    string a = tokens[i] + "&&&" + tokens[j], b = tokens[j] + "&&&" + tokens[i];
                    if (bigramInfo.ContainsKey(tokens[i] + "&&&" + tokens[j]))
                    {
                        double score1 = 0.0, score2 = 0.0;
                        if (bigramInfo.ContainsKey(a)) score1 = bigramInfo[a];
                        if (bigramInfo.ContainsKey(b)) score2 = bigramInfo[b];
                        score += Math.Max(score1,score2);
                    }
                    
                }
                strToScore[tokens[i]] = score / cnt;
            }
            List<KeyValuePair<string, double>> strScorelist = strToScore.ToList();
            strScorelist.Sort(compare2);
            Dictionary<string, double> stoscore = new Dictionary<string, double>();
            foreach (KeyValuePair<string,double> kv in strScorelist)
            {
                stoscore[kv.Key] = kv.Value;
                Console.WriteLine(kv.Key+":"+kv.Value);
            }
            string ret = "";
            foreach(string s in tokens)
            {
                if (stopwords.Contains(s)) continue;
                if (stoscore.ContainsKey(s) && stoscore[s] > 3) ret += (" " +s);
            }
            Console.WriteLine(ret);
        }
        static void test()
        {
            PhraseExtractTool pet = new PhraseExtractTool();
            pet.LoadMIModel("D:\\tmp\\MIinfo.txt");
            pet.loadStopWords(pet.baseDir);
            string line = null;
            while (true)
            {
                line = Console.ReadLine();
                pet.removeTwoWords(line);
            }
            pet.scanForMI(@"D:\tmp\support_office_com_clean.tsv","D:\\tmp\\");
            return;
            pet.scanForUniGramBigram("D:\\tmp\\support_office_com_clean.tsv");
            pet.sortByScore();
            Console.WriteLine("finished");
            while (true)
            {
                string a, b;
                a= Console.ReadLine();
                b = Console.ReadLine();
                if (pet.bigramInfo.ContainsKey(a + "&&&" + b))
                {
                    Console.WriteLine(pet.bigramInfo[a+"&"+b]);
                }
            }

        }
    }
}
