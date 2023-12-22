using System.ComponentModel.DataAnnotations;

public class JDotLoad
{

    #region Public_Methods
    /// <summary>
    /// Convert a String into Class
    /// </summary>
    /// <param name="Data">String that contain the Data</param>
    /// <param name="IsPath">Determine if the "StringData" is a Path or the Data in a string</param>
    /// <returns>String converted into the Class that you need</returns>
    public GenClass ToDataClass<GenClass>(string Data, bool IsPath = false)
    {
        //GenClass = Generic Class
        Type GenClassType = GetTypeOfGeneric<GenClass>();
        GenClass instance;
        // Verificar si el tipo GenClass es un tipo válido
        // (por ejemplo, una clase con un constructor sin parámetros).
        if (!GenClassType.IsClass)
        {
            // Manejar el error si GenClass no es un tipo válido para la creación de instancias.
            throw new InvalidOperationException("The class is not of a valid type to create an instance");
        }
        else
        {
            // Crea una instancia del tipo GenClass usando la reflexión.
            instance = (GenClass)Activator.CreateInstance(GenClassType);

            // Aquí puedes realizar las operaciones necesarias para inicializar
            // "instance" con los datos de "Data".
            try
            {
                instance = (GenClass)ToClass(Data, instance, IsPath);
            }
            catch (Exception ex)
            {
                // Manejar la excepción según tus necesidades (por ejemplo, log o lanzar una nueva excepción).
                Console.WriteLine("Error en ToClass: " + ex.Message);
            }


        }
        return instance;
    }

    #endregion


    #region Private_Methods


    /// <summary>
    /// Get type of the "Generic Type" set
    /// </summary>
    /// <typeparam name="GenClass">Is the Generic type</typeparam>
    /// <returns>The type that correspond with the Generic type</returns>
    Type GetTypeOfGeneric<GenClass>() => typeof(GenClass);


    /// <summary>
    /// Convet String To Class
    /// </summary>
    /// <param name="Data"></param>
    /// <param name="Class">Class</param>
    /// <param name="IsPath"></param>
    /// <returns></returns>
    object ToClass(string Data, object Class, bool IsPath)
    {
        return IsPath ? GetDataByPath(Class, Data) : GetDataByString(Class, Data);
    }


    /// <summary>
    /// Get Data By Path
    /// </summary>
    /// <param name="Class"></param>
    /// <param name="DataPath"></param>
    /// <returns></returns>
    object GetDataByPath(object Class, string DataPath)
    {
        string[] DataLines = SearchDataByPath(DataPath);
        if (DataLines == null)
            return Class;
        else
            return MergeDataToClass(Class, DataLines);
    }


    /// <summary>
    /// Get Data By String
    /// </summary>
    /// <param name="Class"></param>
    /// <param name="Data"></param>
    /// <returns></returns>
    object GetDataByString(object Class, string Data)
    {
        string[] DataLines = Data.Split("\n");
        return MergeDataToClass(Class, DataLines);
    }


    /// <summary>
    /// Get Data By Path
    /// </summary>
    /// <param name="DataPath"></param>
    /// <returns></returns>
    string[] SearchDataByPath(string DataPath)
    {
        if (!File.Exists(DataPath))
            return null;
        else
        {
            StreamReader Reader = new(DataPath);
            string DataLines = Reader.ReadToEnd();
            Reader.Close();
            return DataLines.Split("\n");
        }
    }


    /// <summary>
    /// Merge the Data to Class
    /// </summary>
    /// <param name="Class"></param>
    /// <param name="DataLines"></param>
    /// <returns></returns>
    object MergeDataToClass(object Class, string[] DataLines)
    {
        string ItemType;
        string Item;
        int DataLinesIndex = 2;

        // recupero todos los elementos ya sea de una clase o de una lista
        // en un Array de FieldsInfo para poder trabajar cada uno individualmente
        FieldInfo[] Fields = Class.GetType().GetFields();
        foreach (FieldInfo ItemField in Fields)
        {
            // Recupera el tipo de dato del elemento
            object field = ItemField.GetValue(Class);

            // Carga los datos de un elemento siempre que
            // sea un dato primitivo,
            // por ejemplo: \n\t<<Creador(string): John Carmack>>
            if (!IsGenericList(ItemField))
                ItemField.SetValue(Class, GetPrimitiveValue(DataLines[DataLinesIndex], ItemField.Name));
            else
                //ToDo: Cargar los datos de una lista
            DataLinesIndex++;
        }
        return Class;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="DataLine"></param>
    /// <param name="FieldName"></param>
    /// <returns></returns>
    object GetPrimitiveValue(string DataLine, string FieldName)
    {
        string[] Data = DataLine.Split(": ");
        Data[1] = Data[1].Replace(">>", "");
        Data[0] = Data[0].Replace("<<", "");
        Data[0] = Data[0].Replace(">>", "");
        string DataValue = Data[1];
        Data = Data[0].Split("(");
        Data[1] = Data[1].Replace(")", "");
        Type ResultType = GetTypeByGenericFlag(Data[1]);

        if (ResultType != null)
            return Convert.ChangeType(DataValue, ResultType);
        else
            return null;
    }




    /// <summary>
    /// Is a Generic List
    /// </summary>
    /// <param name="fieldInfo">List</param>
    /// <returns>True or False if the FieldValue is generic or not</returns>
    static bool IsGenericList(FieldInfo fieldInfo) =>
        fieldInfo.FieldType.IsGenericType &&
        fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>);



    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    Type GetTypeByGenericFlag(string type)
    {
        if (new JDotCons().GenericFlags.TryGetValue(type, out Type value))
            return value;
        else
            return null;
    }


    #endregion

}
