public class JDotLoad
{

    #region Variables
    JDotCons jDotCons = new();


    #endregion




    #region Public_Methods
    /// <summary>
    /// Convert a String into Class
    /// </summary>
    /// <param name="Data">String that contain the data</param>
    /// <param name="IsPath">Determine if the "StringData" is a Path or the data in a string</param>
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
            // "instance" con los datos de "data".
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
    /// Get data By Path
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
    /// Get data By String
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
    /// Get data By Path
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
    /// Merge the data to Class
    /// </summary>
    /// <param name="Class"></param>
    /// <param name="DataLines"></param>
    /// <returns></returns>
    object MergeDataToClass(object Class, string[] DataLines)
    {
        int DataLinesIndex = 2;

        // recupero todos los elementos ya sea de una clase o de una lista
        // en un Array de FieldsInfo para poder trabajar cada uno individualmente
        FieldInfo[] Fields = Class.GetType().GetFields();
        foreach (FieldInfo ItemField in Fields)
        {
            // Carga los datos de un elemento siempre que
            // sea un dato primitivo,
            // por ejemplo: \n\t<<Creador(string): John Carmack>>
            if (!IsGenericList(ItemField))
                ItemField.SetValue(Class, GetPrimitiveValue(DataLines[DataLinesIndex], ItemField.Name));
            else{
                //ToDo: Cargar los datos de una lista
            }
            DataLinesIndex++;
        }
        return Class;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataLine"></param>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    object GetPrimitiveValue(string dataLine, string fieldName)
    {
        string[] data = dataLine.Split(": ");
        data[1] = data[1].Replace(">>", "");
        data[0] = data[0].Replace("<<", "");
        data[0] = data[0].Replace(">>", "");
        string dataValue = data[1];
        data = data[0].Split("(");
        data[1] = data[1].Replace(")", "");
        try
        {
            Type typeOfData = GetTypeByGenericFlag(data[1]);
            if (typeOfData != null)
                return Convert.ChangeType(dataValue, typeOfData);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Excepción: " + ex.Message);
        }


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
        if (jDotCons.GenericFlags.TryGetValue(type, out Type value))
            return value;
        return null;
    }


    #endregion

}
