namespace JDotParser
{
    public class JDot
    {

        /// <summary>
        /// ClassToText
        /// </summary>
        /// <param name="Class">Class that want to extract data</param>
        /// <returns>String tha contains the data parsed</returns>
        public static string ClassToText(object Class)
        {
            return new JDotSave().ToDataFile(Class);
        }


        /// <summary>
        /// FileToClass
        /// </summary>
        /// <typeparam name="T">Target that want to convert to</typeparam>
        /// <param name="Data">String data or the file path</param>
        /// <param name="IsPath">True == File path, False == Data in string</param>
        /// <returns></returns>
        public static T FileToClass<T>(string Data, bool IsPath = false) {
            return new JDotLoad().ToDataClass<T>(Data, IsPath);
        }
    }
}
