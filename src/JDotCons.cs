public class JDotCons
{
    public const string stg_MIF = "<mdf>"; // IMF = Main Input Flag
                                            // mdf = Main Data Flag
    public const string stg_MOF = "</mdf>"; // OMF = Main Output Flag


    public readonly IReadOnlyDictionary<Type, string> DataTypes = new Dictionary<Type, string>
    {
        {typeof(int), "int"},
        {typeof(double), "double"},
        {typeof(float), "float"},
        {typeof(byte), "byte"},
        {typeof(string), "string"},
        {typeof(char), "char"},
        {typeof(bool), "bool"},
    };

    public readonly IReadOnlyDictionary<string, Type> GenericFlags = new Dictionary<string, Type>
    {
        {"int",typeof(int)},
        {"double", typeof(double) },
        {"float", typeof(float) },
        {"byte", typeof(byte) },
        {"string", typeof(string) },
        {"char", typeof(char) },
        {"bool", typeof(bool) },
    };


    public static string MergeTagToData(string data, TagHierarchy tag, bool OpenTag = false)
    {
        string result = string.Empty;
        if (tag == TagHierarchy.ItemList
            || tag == TagHierarchy.ItemComplexList 
            || tag == TagHierarchy.Primitive)
        {
            switch (tag)
            {
                case TagHierarchy.ItemList: result =  $"\n ![{data}]"; break;
                case TagHierarchy.ItemComplexList: result = $"\n\n!<[{data}]>"; break;
                case TagHierarchy.Primitive: result = $"\n<<{data}>>"; break;
                default: result = string.Empty; break;
            }
            return result ;
        }

        if (OpenTag) { 
            switch (tag)
            {
                case TagHierarchy.Main: result = $"<{data}>"; break;
                case TagHierarchy.List: result = $"\n<{data}>"; break;

                default: result = string.Empty;  break;
            }
        }
        else
        {
            switch (tag)
            {
                case TagHierarchy.Main: result = $"\n</{data}>"; break;
                case TagHierarchy.List: result = $"</{data}>\n\n"; break;

                default: result = string.Empty; break;
            }
        }

        return result;
    }

    public enum TagHierarchy
    {
        Main = 0,
        Primitive,
        List,
        ItemList,
        ItemComplexList
    }

}
