using System.ComponentModel;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks.Dataflow;

public class JDotLoad
{

    #region Variables

    JDotCons jDotCons = new();

    #endregion



    #region Public_Methods
    /// <summary>
    /// Convert a String into Class
    /// </summary>
    /// <param name="Data">String that contain the item</param>
    /// <param name="IsPath">Determine if the "StringData" is a Path or the item in a string</param>
    /// <returns>String converted into the Class that you need</returns>
    public GenClass ToDataClass<GenClass>(string Data, bool IsPath = false)
    {
        GenClass instance = InstanceMaker<GenClass>();
        if (instance != null)
        {
            // Aquí puedes realizar las operaciones necesarias para inicializar
            // "item" con los datos de "item".
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
    /// Get item By Path
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
            return MergeDataToClass<object>(Class, DataLines);
    }


    /// <summary>
    /// Get item By String
    /// </summary>
    /// <param name="Class"></param>
    /// <param name="Data"></param>
    /// <returns></returns>
    object GetDataByString(object Class, string Data)
    {
        string[] DataLines = Data.Split("\n");
        return MergeDataToClass<object>(Class, DataLines);
    }


    /// <summary>
    /// Get item By Path
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
    /// Merge the item to Class
    /// </summary>
    /// <param name="Class"></param>
    /// <param name="DataLines"></param>
    /// <returns></returns>
    object MergeDataToClass<T>(object Class, string[] DataLines)
    {
        int DataLinesIndex = 2;
        if (QuickBreak(DataLines,Class))
            return Class;
        // recupero todos los elementos ya sea de una clase o de una genList
        // en un Array de FieldsInfo para poder trabajar cada uno individualmente
        FieldInfo[] Fields = Class.GetType().GetFields();
        foreach (FieldInfo ItemField in Fields)
        {
            // Carga los datos de un elemento siempre que
            // sea un dato primitivo,
            // por ejemplo: \n\t<<Creador(string): John Carmack>>
            if (!IsGenericList(ItemField))
                ItemField.SetValue(Class, GetPrimitiveValue(DataLines[DataLinesIndex], ItemField.Name));
            else
            {
                //Extraer el tipo de dato de la genList
                Type type = ItemField.FieldType.GetGenericArguments()[0];
                Type? typeList = typeof(List<>).MakeGenericType(type);
                object? genList = default;
                object? objectInstance = default;

                //Listas de Objetos
                if (!IsPrimitiveValue(type)){
                    // Crear una instancia de List<TipoClase> usando reflexión
                    genList = InstanceMaker<T>(typeList);
                    AddElementsToGenList(DataLines, typeList, type, genList, DataLinesIndex);
                }

                //Listas de Primitivas
                else{
                    //Crear una instancia de List<TipoClase> usando reflexión
                    genList = Activator.CreateInstance(typeList);
                    AddElementsToGenList(DataLines, typeList, type, genList, DataLinesIndex, true);
                }
                ItemField.SetValue(Class, genList);
                Console.WriteLine($"{type}: {objectInstance?.GetType()}");
            }
            DataLinesIndex++;
        }
        return Class;
    }

    bool QuickBreak(string[] data, object genClass)
    {
        string classStruct = genClass.GetType().Name;
        if (data[0] != JDotCons.stg_MIF || data[^1] != JDotCons.stg_MOF
            || data[1] != $"<{classStruct}>")
        {
            return true;
        }
        return false;
    }

    object? MakePrimitiveInstance(Type type)
    {
        if (type != typeof(string))
            return Activator.CreateInstance(type);
        else
            return string.Empty;
    }




    void AddElementsToGenList(string[] data, Type listType, Type itemType, object? genList, 
        int index = 0, bool isPrimitive = false)
    {
        object? objectInstance = default;
        for (int i = index; i < data.Length; i++)
        {
            if (isPrimitive)
                objectInstance = MakePrimitiveInstance(itemType);
            else
                objectInstance = InstanceMaker<object>(itemType);

            MethodInfo? metodoAdd = listType.GetMethod("Add");
            metodoAdd?.Invoke(genList, new[] { objectInstance });
        }
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
        data[1] = data[1].Replace(">>", "").Replace(")", "");
        data[0] = data[0].Replace("<<", "").Replace(">>", "");
        string dataValue = data[1];
        data = data[0].Split("(");
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


    bool IsPrimitiveValue(Type item) {
        if (GetTypeByGenericFlag(item.Name.ToLower()) != null
            || item.IsPrimitive)
            return true;
        return false;
    }

    /// <summary>
    /// Is a Generic List
    /// </summary>
    /// <param name="fieldInfo">The FieldInfo from the genList</param>
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


    GenClass InstanceMaker<GenClass>(Type GenClassType = null)
    {
        //GenClass = Generic Class
        if (GenClassType == null)
            GenClassType = GetTypeOfGeneric<GenClass>();

        GenClass instance;

        // Verificar si el tipo GenClass es un tipo válido
        // (por ejemplo, una clase con un constructor sin parámetros).
        if (!GenClassType.IsClass)
        {
            // Manejar el error si GenClass no es un tipo válido para la creación de instancias.
            throw new InvalidOperationException("The class is not of a valid type to create an item");
        }
        else
        {
            // Crea una instancia del tipo GenClass usando la reflexión.
            instance = (GenClass)Activator.CreateInstance(GenClassType);
        }
        return instance;
    }


    #endregion


}
