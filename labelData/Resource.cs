using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;


namespace labelData
{
    /// <summary>
    /// Class for static resource (templates and dictionary files)
    /// </summary>
    public static class Resource
    {
        public static bool Load(string dir)
        {
            bool success = true;
            success &= LoadStemmingTerms(dir+ @"qc\data\StemmingDict.extended.txt");
            success &= LoadQC(dir);
            return success;
        }


        private static bool LoadStemmingTerms(string file)
        {
            StreamReader srStemmingTerm = new StreamReader(file);

            if (srStemmingTerm != null)
            {
                string line = "";
                while ((line = srStemmingTerm.ReadLine()) != null)
                {
                    if (line.Trim().Length == 0) continue;
                    string[] words = line.Trim().Trim().Split('\t');
                    if (words.Length == 2)
                    {
                        if (m_stemmingDict.ContainsKey(words[0]) == false)
                        {
                            m_stemmingDict.Add(words[0], words[1]);
                        }
                    }
                }
                srStemmingTerm.Close();
                return true;
            }
            else return false;
        }
        private static bool LoadQC(string dir)
        {
            string path =dir +@"qc\data\package";
            string actionfile = dir+@"qc\data\action_sure.txt";
            string targetfile= dir + @"qc\data\target.txt";
            string proPath = dir+@"qc\data\product_list.txt";
            string sureFile = dir+@"qc\data\sureList.txt";
            string[] alines = System.IO.File.ReadAllLines(actionfile);
            foreach (string line in alines)
            {
                if (!m_Actions.Contains(line.ToLower())) m_Actions.Add(line.ToLower());
            }
            
            alines = System.IO.File.ReadAllLines(targetfile);
            foreach (string line in alines)
            {
                if (!m_Target.Contains(line.ToLower())) m_Target.Add(line.ToLower());
            }
            alines = System.IO.File.ReadAllLines(proPath);
            foreach (string line in alines)
            {
                if (m_productList2.Contains(line.ToLower()) == false)
                    m_productList2.Add(line.ToLower());
            }
            alines = System.IO.File.ReadAllLines(sureFile);
            foreach (string line in alines)
            {
                if (line.StartsWith("\t"))
                {
                    string t = line.Substring(1);
                    string[] tokens = t.Split(":".ToArray());
                    if (m_sureList.Contains(tokens[0]) == false)
                        m_sureList.Add(tokens[0].ToLower());

                }
                else
                {
                    string[] tokens = line.Split(":".ToArray());
                    double prop = double.Parse(tokens[1]);
                    if (prop >= 0.9 && m_badList.Contains(tokens[0]) == false)
                    {
                        m_badList.Add(tokens[0].ToLower());
                    }
                }
            }
            ICEClassifierLocalForTheme.LocalClassifier.Init(path);
            //m_fea.init(dir+"qc\\");
            //m_sc.init(dir+"qc\\");
            return true;
        }


        public static Dictionary<string, string> m_stemmingDict = new Dictionary<string, string>();
        public static HashSet<string> m_Actions = new HashSet<string>();
        public static HashSet<string> m_Phrase = new HashSet<string>();
        public static HashSet<string> m_Target = new HashSet<string>();
        public static HashSet<string> m_productList2 = new HashSet<string>();
        public static HashSet<string> m_sureList = new HashSet<string>();
        public static HashSet<string> m_badList = new HashSet<string>();
        //public static feature m_fea = new feature();
       public static sentenceComplete m_sc= new sentenceComplete();
    }

}