namespace JDotParser
{
    public class JDot
    {
        public static string ClassToText(object Class)
        {
            return new JDotSave().ToDataFile(Class);
        }


        public static T FileToClass<T>(string Data, bool IsPath = false) {
            return new JDotLoad().ToDataClass<T>(Data, IsPath);
        }
    }
}
