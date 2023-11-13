using JDot_Parser.Systems;
namespace JDot_Parser
{
    internal class Program
    {
        static void Main(string[] args)
        {

            #region Data_Test

            DCFP _DCFP = new();
            DataBase data = new();
            DataBase NewData = new();

            Anime anime = new();
            anime.Name = "High School DXD";
            anime.Tags.Add("Romance");
            anime.Tags.Add("Action");

            Anime anime2 = new();
            anime2.Name = "Nazo no Kanojo X";
            anime2.Tags.Add("School");
            anime2.Tags.Add("Ecchi");

            Manga manga = new();
            manga.Name = "Berserk";


            data.AnimeList.Add(anime);
            data.AnimeList.Add(anime2);
            data.MangaList.Add(manga);

            StreamWriter streamWriter = new(@"D:\Emu.txt");
            streamWriter.Write(_DCFP.ToDataFile(data));
            streamWriter.Close();
            Console.WriteLine(_DCFP.ToDataFile(data));

            NewData = _DCFP.ToDataClass<DataBase>(@"D:\Emu.txt", true);

            Console.ReadKey();
            #endregion



            #region Other_Test

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
}
