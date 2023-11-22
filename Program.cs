using JDot_Parser.Systems;
namespace JDot_Parser
{
    internal class Program
    {
        static void Main(string[] args)
        {

            #region Data_Test

            JDot Parser = new();
            //DataBase data = new();
            //DataBase data1 = new();
            //DataBase NewData = new();

            //Anime anime = new();
            //anime.Name = "High School DXD";
            //anime.Tags.Add("Romance");
            //anime.Tags.Add("Action");

            //Anime anime2 = new();
            //anime2.Name = "Nazo no Kanojo X";
            //anime2.Tags.Add("School");
            //anime2.Tags.Add("Ecchi");
            //data1.AnimeList.Add(anime2);
            //anime.data_anime = data1;

            //Manga manga = new();
            //manga.Name = "Berserk";


            //data.AnimeList.Add(anime);
            //data.MangaList.Add(manga);
            MyClass data = new();

            StreamWriter streamWriter = new(@"D:\Download\Emu.txt");
            streamWriter.Write(Parser.ToDataFile(data));
            streamWriter.Close();
            Console.WriteLine(Parser.ToDataFile(data));

            //NewData = Parser.ToDataClass<DataBase>(@"D:\Emu.txt", true);

            Console.ReadKey();
            #endregion



            #region Other_Tests

            //string All = @"hola\nmundo\nyo soy\nWilley Wonka";
            //Console.WriteLine(All);

            //string[] list;
            //list = All.Split(@"\n");
            //Console.ForegroundColor = ConsoleColor.Yellow;
            //foreach (string line in list)
            //{
            //    Console.WriteLine(line);
            //}

            //Print the arguments
            //for (int i = 1; i < args.Length; i++)
            //{
            //    Console.WriteLine($"{args[i]}");
            //}
            //Console.ReadKey();
            #endregion

        }
    }
    class MyClass
    {
        public string hello = "Hello";
        public int num = 1;
    }

    class MyClass2
    {
        public string bye = "bye bye";
        public int num2 = 2;
        public bool xd = false;
    }
}
