using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CDSSVectorTagger;


namespace labelData
{
    class AllDictionary
    {
        private static string dir = @"C:\Users\v-chuhz\Desktop\code\data\";
        // computer cluster
        public static Dictionary<int, List<int>> ComputerClusterId()
        {
            Dictionary<int, List<int>> clusters = new Dictionary<int, List<int>>();
            //read the cluster file
            FileStream read = new FileStream(dir + @"Cluster.txt", FileMode.Open);
            BinaryReader sr = new BinaryReader(read);
            int count = sr.ReadInt32();
            int index = 0;
            while (index < count)
            {

                int number = sr.ReadInt32();
                int id = sr.ReadInt32();
                List<int> cluster = new List<int>();
                for (int i = 0; i < number; i++)
                {
                    int temp = sr.ReadInt32();
                    cluster.Add(temp);

                }
                clusters.Add(id, cluster);
                index++;
            }
            // StreamReader sr = new StreamReader(dir + @"Cluster.txt");
            //string line = "";
            //while ((line = sr.ReadLine()) != null)
            //{
            //    List<int> clusterId = new List<int>();
            //    string[] parts = line.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    string[] parts1 = parts[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    foreach (string item in parts1)
            //    {
            //        clusterId.Add(int.Parse(item));
            //    }
            //    cluster.Add(int.Parse(parts[0]), clusterId);
            //}
            return clusters;
        }
        //computer deep vector
        public static Dictionary<int, float[]> ComputerVector()
        {
            Dictionary<int, float[]> Vectors = new Dictionary<int, float[]>();
            // read the deep vector file
            FileStream read = new FileStream(dir + @"question_vector.txt", FileMode.Open);
            BinaryReader sr = new BinaryReader(read);
            int count = sr.ReadInt32();
            int index = 0;
            while (index < count)
            {
                try
                {
                    int id = sr.ReadInt32();
                    int vectorLength = sr.ReadInt32();
                    float[] vector = new float[vectorLength];
                    for (int i = 0; i < vectorLength; i++)
                    {
                        vector[i] = sr.ReadSingle();
                    }
                    Vectors.Add(id, vector);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("FAILED");
                }
                index++;

            }
            sr.Close();
            //  StreamReader sr = new StreamReader(dir + @"question_vector.tsv");
            //string line = "";
            //while ((line = sr.ReadLine()) != null)
            //{
            //    string[] parts = line.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    string[] parts1 = parts[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    float[] vec = new float[parts1.Length];
            //    for (int i = 0; i < parts1.Length; i++)
            //    {
            //        vec[i] = float.Parse(parts1[i]);
            //    }
            //    Vectors.Add(int.Parse(parts[0]), vec);
            //}
            return Vectors;
        }
        //computer centroid
        public static Dictionary<int, float[]> ComputerCentroid()
        {
            //read the centroid file
            FileStream read = new FileStream(dir + @"Centroids.txt", FileMode.Open);
            BinaryReader sr = new BinaryReader(read);
            //StreamReader sr = new StreamReader(dir + @"Centroids.txt");
            Dictionary<int, float[]> Centroids = new Dictionary<int, float[]>();
            int count = sr.ReadInt32();
            int index = 0;
            while (index < count)
            {
                try
                {
                    int id = sr.ReadInt32();
                    float[] vector = new float[128];
                    for (int i = 0; i < 128; i++)
                    {
                        vector[i] = sr.ReadSingle();
                    }
                    Centroids.Add(id, vector);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                index++;

            }
            //string line = "";
            //while ((line = sr.ReadLine()) != null)
            //{
            //    float[] result = new float[128];
            //    string[] parts = line.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    string[] parts1 = parts[1].Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //    for (int i = 0; i < parts1.Length; i++)
            //    {
            //        result[i] = float.Parse(parts1[i]);
            //    }
            //    Centroids.Add(int.Parse(parts[0]), result);
            //}
            return Centroids;
        }
        // question_answer pair
        public static Dictionary<int, Tuple<string, string, string>> IDQuestionAnswer()
        {
            StreamReader sr = new StreamReader(dir + @"QA_support_office_com_HTML.filter.tsv");
            Dictionary<int, Tuple<string, string, string>> questionAnswer = new Dictionary<int, Tuple<string, string, string>>();
            string line = "";
            while ((line = sr.ReadLine()) != null)
            {
                string[] parts = line.Split("\t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                int id = int.Parse(parts[0]);
                string[] answer = parts[2].Split("|||||".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Tuple<string, string, string> tuple = new Tuple<string, string, string>(parts[1], answer[0], answer[1]);
                if (questionAnswer.ContainsKey(id)) continue;
                else
                {
                    questionAnswer.Add(id, tuple);
                }

            }
            return questionAnswer;

        }
        public static float Distance(float[] query1, float[] query2)
        {
            double dis = 0.0f;
            double a = 0.0f, b = 0.0f, c = 0.0f;

            for (int i = 0; i < query1.Length; ++i)
            {
                a += query1[i] * query2[i];
                b += query1[i] * query1[i];
                c += query2[i] * query2[i];
            }
            dis = a / (Math.Sqrt(b) * Math.Sqrt(c));
            return 1.0f - (float)dis;
        }

        public static int[] isExist(float[] vector)
        {
            int[] isExist = new int[3];
            int level = vector.Length / 128;
            for (int i = 0; i < level; i++)
            {
                isExist[i]++;
            }
            return isExist;
        }
        public static float weightDistance(float[] level, float[] query, int[] isExist)
        {
            int len = level.Length / 128;
            float[] Weight = new float[3] { 0.8f, 0.15f, 0.05f };
            float distance = 0.0f;
            for (int i = 0; i < len; i++)
            {
                int k = 128 * i;
                float[] temp = new float[128];
                for (int j = 0; j < 128; j++)
                {
                    temp[j] = level[k + j];
                }
                distance += Distance(temp, query) * Weight[i];
            }
            float norm = 0.8f * isExist[0] + 0.15f * isExist[1] + 0.05f * isExist[2];
            distance /= norm;
            return distance;
        }
        public static int[] TopK(Dictionary<int, float> dictionary, int K, ref float distance)
        {

            var items = from pair in dictionary
                        orderby pair.Value ascending
                        select pair;
            int count = 0;
            int[] top = new int[K];
            foreach (var item in items)
            {
                if (count >= K) break;
                else
                {
                    top[count] = item.Key;
                    if (count == 0) distance = item.Value;
                    count++;
                }
            }
            return top;
        }

    }
    class Retrevial
    {

        public static VectorWrapper wrapperQ;
        public static VectorWrapper wrapperD;
        public static Dictionary<int, List<int>> cluster = new Dictionary<int, List<int>>();
        public static Dictionary<int, float[]> Vectors = new Dictionary<int, float[]>();
        public static Dictionary<int, float[]> Centroids = new Dictionary<int, float[]>();
        public static Dictionary<int, Tuple<string, string, string>> IDQuestionAnswer = new Dictionary<int, Tuple<string, string, string>>();

        public static string dir = @"C:\Users\v-chuhz\Desktop\code\";

        public static void Init()
        {
            string Settingpath = @dir + @"model\DSSM.10.model.Setting";
            string QModelPath = @dir + @"model\DSSM.10.model.Query";
            string DModelPath = @dir + @"model\DSSM.10.model.Doc";
            string TRIGRAM_VOCA_FILE = @dir + @"model\l3g.txt";
            wrapperQ = new VectorWrapper(QModelPath, Settingpath, TRIGRAM_VOCA_FILE);
            wrapperD = new VectorWrapper(DModelPath, Settingpath, TRIGRAM_VOCA_FILE);
            cluster = AllDictionary.ComputerClusterId();
            Vectors = AllDictionary.ComputerVector();
            Centroids = AllDictionary.ComputerCentroid();
            IDQuestionAnswer = AllDictionary.IDQuestionAnswer();

        }
        public static float Query(string query, float sim)
        {
            //Init();
            Dictionary<int, float> distanceId = new Dictionary<int, float>();
            int index = 1;
            // int idex = 0;
            float[] queryVector = wrapperQ.GetVector(query, wrapperQ.vocab_list);
            foreach (var item in Centroids)
            {
                int id = item.Key;
                float[] compareQuestion = new float[128];
                compareQuestion = item.Value;
                try
                {
                    float distance = AllDictionary.Distance(queryVector, compareQuestion);
                    distanceId.Add(id, distance);
                    //idex++;
                }

                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            float distance1 = 0.0f;
            int[] clusterIds = AllDictionary.TopK(distanceId, 2, ref distance1);

            // find the question
            List<int> questionIds = new List<int>();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    List<int> questionId = cluster[clusterIds[i]];
                    foreach (int item in questionId)
                    {
                        questionIds.Add(item);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            Dictionary<int, float> distanceQuestion = new Dictionary<int, float>();
            foreach (int id in questionIds)
            {
                try
                {

                    int[] isExist = AllDictionary.isExist(Vectors[id]);

                    float distance = AllDictionary.weightDistance(Vectors[id], queryVector, isExist);

                    distanceQuestion.Add(id, distance);

                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to find the question id");
                    Console.WriteLine(e.Message);
                    index++;
                }
            }
            float distance2 = 0.0f;
            int[] clusterQuestion = AllDictionary.TopK(distanceQuestion, 1, ref distance2);
            float similarity = 1 - distance2;
            
            return similarity;


        }
    }
}
