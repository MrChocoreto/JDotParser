using JDot_Parser.Systems;
namespace JDot_Parser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DCFP _DCFP = new();
            DataBase data = new();
            Anime anime = new();
            anime.Name = "High School DXD";
            anime.Tags.Add("Romance");
            anime.Tags.Add("Action");
            data.AnimeList.Add(anime);

            StreamWriter streamWriter = new(@"D:\Hola.txt");
            streamWriter.Write(_DCFP.ToDataFile(data));
            streamWriter.Close();
            Console.WriteLine(_DCFP.ToDataFile(data));

            data = _DCFP.ToDataClass<DataBase>(@"D:\Hola.txt", true);
            string hola = "";
        }
    }
}