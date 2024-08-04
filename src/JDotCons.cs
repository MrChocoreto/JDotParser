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

}
