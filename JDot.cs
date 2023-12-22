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

<<<<<<< 7bd1cc03068bc56206fe8a2807e7841e1718391e
=======

        /// <summary>
        /// FileToClass
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Data"></param>
        /// <param name="IsPath"></param>
        /// <returns></returns>
>>>>>>> Errors
        public static T FileToClass<T>(string Data, bool IsPath = false) {
            return new JDotLoad().ToDataClass<T>(Data, IsPath);
        }
    }
}
