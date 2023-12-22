namespace JDotParser
{
    public class JDot
    {

        /// <summary>
        /// ClassToText
        /// </summary>
        /// <param name="Class"></param>
        /// <returns></returns>
        public static string ClassToText(object Class)
        {
            return new JDotSave().ToDataFile(Class);
        }


        public static T FileToClass<T>(string Data, bool IsPath = false) {
            return new JDotLoad().ToDataClass<T>(Data, IsPath);
        }
    }
}
