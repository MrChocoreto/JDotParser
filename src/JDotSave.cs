using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class JDotSave
{

    #region Global_Variables

    const string stg_MIF = "<mdf>\n"; // IMF = Main Input Flag
                                      // mdf = Main Data Flag
    const string stg_MOF = "\n</mdf>"; // OMF = Main Output Flag


    #endregion



    #region Public_Methods


    /// <summary>
    /// Convert a Class into String Format like a XML 
    /// </summary>
    /// <param name="Class">The Class that you want convert</param>
    /// <returns>The Class converted to String</returns>
    public string ToDataFile(object Class)
    {
        return stg_MIF + ClassToString(Class) + stg_MOF;
    }


    #endregion



    #region Private_Methods

    string ClassToString(object Class)
    {
        string stg_Result = default;
        if (Class != null && !Class.GetType().IsPrimitive)
        {
            stg_Result += $"<{Class.GetType().Name}>" +
            $"{ItemsFromClass(Class, Class.GetType())}" +
            $"\n</{Class.GetType().Name}>";
        }
        return stg_Result;
    }


    /// <summary>
    /// Extract Items From a Class/List
    /// </summary>
    /// <param name="Class">Class/List</param>
    /// <param name="type">Type of the Object</param>
    /// <returns>Content of the Class</returns>
    string ItemsFromClass(object Class, Type type)
    {
        StringBuilder Result = new();
        string ItemType = default;
        string Item = default;
        // recupero todos los elementos ya sea de una clase o de una lista
        // en un Array de FieldsInfo para poder trabajar cada uno individualmente
        FieldInfo[] Fields = type.GetFields();
        foreach (FieldInfo ItemField in Fields)
        {
            object FieldValue = ItemField.GetValue(Class);

            // Agrega un nuevo elemento junto con su valor
            // mientras sea un dato primitivo,
            // por ejemplo: \n\t<<Creador: John Carmack>>
            if (!IsGenericList(ItemField) && ItemField.Name != "Empty")
            {
                bool PrimitiveExist = new JDotCons().DataTypes.TryGetValue(ItemField.FieldType, out string value);

                //Revisa si es el ultimo elemento de la lista
                if (ItemField == Fields[Fields.Length - 1])
                    if (PrimitiveExist)
                        Result.Append($"\n<<{ItemField.Name}({value}): {FieldValue}>>");
                    else
                        Result.Append($"\n\n{ClassToString(FieldValue)}\n");
                else
                    Result.Append($"\n<<{ItemField.Name}({value}): {FieldValue}>>");

            }

            // comprueba si lo que se le esta pasando es una lista
            // de tipo generico
            if (IsGenericList(ItemField))
            {
                // Result += $"\n\t<{ItemField.Name}>";
                // en caso de ser cierto lo que hace es crear una
                // lista generica de objetos
                IList<object> GenObjectList = new List<object>();
                // se compreba si el valor de la lista de elementos es un
                // IEnumerable de objetos ademas de que los crea un objeto
                // IEnumerable que contiene los elementos de la lista
                Result = DataSerializer(Result, FieldValue, GenObjectList, 
                        new string[] { Item, ItemType, ItemField.Name });
            }
        }
        return Result.ToString();
    }



    /// <summary>
    /// Data Serializer
    /// </summary>
    /// <param name="Data"></param>
    /// <param name="FieldValue"></param>
    /// <param name="GenObjectList"></param>
    /// <param name="Item_ItemType"></param>
    /// <returns></returns>
    StringBuilder DataSerializer(StringBuilder Data, object FieldValue,
        IList<object> GenObjectList, string[] Item_ItemType)
    {
        //El elemento '0' de Item_ItemType es la variable Item,
        //el elemento '1' es la variable ItemType y el elemento '2'
        //es la propiedad ItemField.Name

        StringBuilder Result = Data;
        if (FieldValue is IEnumerable<object> IEListObjects)
        {
            // y de ser cierto crea una lista con los elementos en base
            // a los IEnumerables
            GenObjectList = IEListObjects.ToList();

            // se recorre cada elemento de la lista para ver si contiene mas
            // elementos del mismo tipo dentro o son puros atributos/elementos
            // de una lista
            foreach (object ObjectList in GenObjectList)
            {
                Item_ItemType[1] = $"{ObjectList}";
                Item_ItemType[0] = GetTypeByElement(ObjectList.GetType());

                if (ObjectList != GenObjectList.First())
                {
                    //Agrega la Flag con el nombre del elemento
                    //de la Lista,y el indice del elemento
                    //entre parentesis
                    //por ejemplo: \n!<[Word(0)]>
                    if (Item_ItemType[1] == Item_ItemType[0])
                        Result.Append($"\n\n!<[{Item_ItemType[0]}({GenObjectList.IndexOf(ObjectList)})]>");
                }
                else
                {
                    //Agrega la Flag con el nombre de la Lista,
                    //y el tipo de dato que usa entre parentesis
                    //por ejemplo: \n<List_Words>(string)
                    Result.Append($"\n<{Item_ItemType[2]}>({Item_ItemType[0]})");


                    //Agrega la Flag con el nombre del elemento
                    //de la Lista,y el indice del elemento
                    //entre parentesis
                    //por ejemplo: \n!<[Word(0)]>
                    if (Item_ItemType[1] == Item_ItemType[0])
                        Result.Append($"\n\n!<[{Item_ItemType[0]}({GenObjectList.IndexOf(ObjectList)})]>");
                }


                if (Item_ItemType[1] != Item_ItemType[0])
                {
                    //Agrega cada elemento que contiene la
                    //lista compleja de segundo nivel
                    //dentro de su etiqueta.
                    //Por ejemplo: \n![Hello_World]
                    Result.Append($"\n  ![{ObjectList}]");
                }


                //Hace un uso recursivo para poder extraer la data
                //de todos los elementos que se encuentren a un
                //nivel inferior dentro del objeto evaluado
                Result.Append(ItemsFromClass(ObjectList, ObjectList.GetType()));
                if (ObjectList == GenObjectList.Last())
                {
                    //Si el tipo del ItemField es igual al ItemField(Number==Number),
                    //escribe esto por ejemplo: \n</Anime>\n\n
                    if (Item_ItemType[1] == Item_ItemType[0])
                    {
                        //Evalua si lo que tiene por detras es un
                        //salto de linea y de serlo imprime lo primero
                        //de lo contrario imprime lo segundo
                        if (Result[Result.Length - 1].ToString() == "\n")
                            Result.Append($"</{Item_ItemType[2]}>\n\n");
                        else
                            Result.Append($"\n</{Item_ItemType[2]}>\n\n");
                    }

                    //Si no escribira esto por ejemplo: \n</List_Words>\n\n
                    else
                    {
                        Result.Append($"\n</{Item_ItemType[2]}>");
                    }


                    //la diferencia entre el primer caso y el ultimo
                    //consiste en que el primer caso permite la
                    //separacion entre objetos pertenecientes a una
                    //"Lista Compleja" a diferencia del segundo caso
                    //por eso el doble salto de linea en el primer caso,
                    //esto se hace para el cierre de Flags del objeto
                    //correspondiente
                }
            }
        }
        return Result;
    }



    /// <summary>
    /// Is a Generic List
    /// </summary>
    /// <param name="fieldInfo">List</param>
    /// <returns>True or False if the FieldValue is generic or not</returns>
    static bool IsGenericList(FieldInfo fieldInfo) =>
        fieldInfo.FieldType.IsGenericType &&
        fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>);


    string GetTypeByElement(Type type)
    {
        if (new JDotCons().DataTypes.TryGetValue(type, out string value))
            return value;
        else
            return type.Name;
    }


    #endregion


}
