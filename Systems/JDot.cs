using System.Reflection;

namespace JDot_Parser.Systems
{
    public class JDot
    {

        #region Global_Variables

        const string stg_IMF = "<mdf>\n"; // IMF = Input MainSaver Flag
                                          // mdf = MainSaver Data Flag
        const string stg_OMF = "\n</mdf>"; // OMF = Output MainSaver Flag

        readonly Dictionary<Type, string> DataTypes = new()
        {
            {typeof(int), "int"},
            {typeof(double), "double"},
            {typeof(float), "float"},
            {typeof(byte), "byte"},
            {typeof(string), "string"},
            {typeof(char), "char"},
            {typeof(bool), "bool"},
        };

        readonly Dictionary<string, Type> GenericFlags = new()
        {
            {"int",typeof(int)},
            {"double", typeof(double) },
            {"float", typeof(float) },
            {"byte", typeof(byte) },
            {"string", typeof(string) },
            {"char", typeof(char) },
            {"bool", typeof(bool) },
        };


        #endregion



        #region Public_Methods


        #region Save_Data

        /// <summary>
        /// Convert a Class into String Format like a XML 
        /// </summary>
        /// <param name="Class">The Class that you want convert</param>
        /// <returns>The Class converted to String</returns>
        public string ToDataFile(object Class)
        {
            return stg_IMF + ClassToString(Class) + stg_OMF;
        }

        #endregion



        #region Get_Data

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

            // Verificar si el tipo GenClass es un tipo válido
            // (por ejemplo, una clase con un constructor sin parámetros).
            if (!GenClassType.IsClass && GenClassType.IsAbstract && 
                GenClassType.GetConstructor(Type.EmptyTypes) == null)
            {
                // Manejar el error si GenClass no es un tipo válido para la creación de instancias.
                throw new InvalidOperationException("The class is not of a valid type to create an instance");
            }
            else
            {
                // Crea una instancia del tipo GenClass usando la reflexión.
                GenClass instance = (GenClass)Activator.CreateInstance(GenClassType);

                // Aquí puedes realizar las operaciones necesarias para inicializar
                // "instance" con los datos de "Data".
                instance = (GenClass)ToClass(Data, instance, IsPath);
                return instance;
            }
        }

        #endregion


        #endregion



        #region Private_Methods


        #region Class_To_Text


        /// <summary>
        /// Class To String
        /// </summary>
        /// <param name="Class"> </param>
        /// <returns>Result of convert your class to Text</returns>
        string ClassToString(object Class)
        {
            string stg_Result = default;
            if (Class != null && !Class.GetType().IsPrimitive)
            {
                stg_Result += $"<{Class.GetType().Name}>";
                stg_Result += ItemsFromClass(Class, Class.GetType());
                stg_Result += $"\n</{Class.GetType().Name}>";
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
            string Result = default;
            string ItemType;
            string Item;
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
                    //Revisa si es el ultimo elemento de la lista
                    if (ItemField == Fields[Fields.Length - 1])
                        if (DataTypes.TryGetValue(ItemField.FieldType, out string value))
                            Result += $"\n<<{ItemField.Name}: {FieldValue}>>\n";
                        else
                            Result += $"\n\n{ClassToString(FieldValue)}\n";
                    else
                        Result += $"\n<<{ItemField.Name}: {FieldValue}>>";

                // comprueba si lo que se le esta pasando es una lista
                // de tipo generico
                if(IsGenericList(ItemField))
                {
                    // Result += $"\n\t<{ItemField.Name}>";
                    // en caso de ser cierto lo que hace es crear una
                    // lista generica de objetos
                    IList<object> GenObjectList = new List<object>();
                    // se compreba si el valor de la lista de elementos es un
                    // IEnumerable de objetos ademas de que los crea un objeto
                    // IEnumerable que contiene los elementos de la lista
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
                            ItemType = $"{ObjectList}";
                            Item = GetTypeByElement(ObjectList.GetType());
                            
                            if (ObjectList != GenObjectList.First())
                            {
                                if (ItemType == Item)
                                    Result += $"\n\n!<[{Item}({GenObjectList.IndexOf(ObjectList)})]>";
                            }
                            else
                            {
                                //Agrega la Flag con el nombre de la Lista,
                                //y el tipo de dato que usa entre parentesis
                                //por ejemplo: \n<List_Words>(string)
                                Result += $"\n<{ItemField.Name}>({Item})";


                                //Agrega cada elemento que contiene la
                                //lista compleja de primer nivel adentro
                                //de su etiqueta.
                                //Por ejemplo: \n![Word]
                                if (ItemType == Item)
                                    Result += $"\n\n!<[{Item}({GenObjectList.IndexOf(ObjectList)})]>";
                            }
                                

                            if (ItemType != Item)
                            {
                                //Agrega cada elemento que contiene la
                                //lista compleja de segundo nivel
                                //dentro de su etiqueta.
                                //Por ejemplo: \n![Hello_World]
                                Result += $"\n![{ObjectList}]";
                            }


                            //Hace un uso recursivo para poder extraer la data
                            //de todos los elementos que se encuentren a un
                            //nivel inferior dentro del objeto evaluado
                            Result += ItemsFromClass(ObjectList, ObjectList.GetType());
                            if (ObjectList == GenObjectList.Last())
                            {
                                //Si el tipo del ItemField es igual al ItemField(Number==Number),
                                //escribe esto por ejemplo: \n</Anime>\n\n
                                if (ItemType == Item)
                                {
                                    //Evalua si lo que tiene por detras es un
                                    //salto de linea y de serlo imprime lo primero
                                    //de lo contrario imprime lo segundo
                                    if (Result[Result.Length-1].ToString() == "\n")
                                        Result += $"</{ItemField.Name}>\n\n";
                                    else
                                        Result += $"\n</{ItemField.Name}>\n\n";
                                }

                                //Si no escribira esto por ejemplo: \n</List_Words>\n\n
                                else
                                {
                                    Result += $"\n</{ItemField.Name}>";
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
                }
            }
            return Result;
        }


        /// <summary>
        /// Is a Generic List
        /// </summary>
        /// <param name="fieldInfo">List</param>
        /// <returns>True or False if the FieldValue is generic or not</returns>
        static bool IsGenericList(FieldInfo fieldInfo)
        {
            return fieldInfo.FieldType.IsGenericType &&
            fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>);
        }


        /// <summary>
        /// Get Type By Element
        /// </summary>
        /// <param name="type">Type of the Object</param>
        /// <returns>A string with the "Type" of the object </returns>
        string GetTypeByElement(Type type)
        {
            if (DataTypes.TryGetValue(type, out string value))
                return value;
            else
                return type.Name;
        }


        #endregion



        #region Text_To_Class


        /// <summary>
        /// Get type of the "Generic Type" set
        /// </summary>
        /// <typeparam name="GenClass">Is the Generic type</typeparam>
        /// <returns>The type that correspond with the Generic type</returns>
        Type GetTypeOfGeneric<GenClass>()
        {
            return typeof(GenClass);
        }


        /// <summary>
        /// Convet String To Class
        /// </summary>
        /// <param name="Data"></param>
        /// <param name="cls">Class</param>
        /// <param name="IsPath"></param>
        /// <returns></returns>
        object ToClass(string Data, object cls, bool IsPath)
        {
            object obj_Result = default;
            if ((Data == null || Data == "") && cls.GetType().IsPrimitive 
                && !cls.GetType().IsClass)
            {
                //Regresa un objeto vacio del tipo que se
                //le esta pasando en caso de que no cumpla
                //con las condiciones
                return cls;
            }
            else
            {
                //Se pasa a convertir la data en el objeto
                //que se espera
                return obj_Result = IsPath ? GetClassByPath(cls, Data) :
                                     GetClassByString(cls, Data);
            }
        }


        /// <summary>
        /// Get Class By Path
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="DataPath"></param>
        /// <returns></returns>
        object GetClassByPath(object Class, string DataPath)
        {
            string[] DataLines = GetDataByPath(DataPath);
            if (DataLines == null)
                return Class;
            else
                return MergeDataToClass(Class, DataLines);
        }


        /// <summary>
        /// Get Class By String
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        object GetClassByString(object Class, string Data)
        {
            return MergeDataToClass(Class, Data.Split(@"\n"));
        }


        /// <summary>
        /// Get Data By Path
        /// </summary>
        /// <param name="DataPath"></param>
        /// <returns></returns>
        string[] GetDataByPath(string DataPath)
        {
            string DataLines;
            if (!File.Exists(DataPath))
                return null;
            else
            {
                StreamReader Reader = new(DataPath);
                DataLines = Reader.ReadToEnd();
                Reader.Close();
                return DataLines.Split("\n");
            }
        }


        /// <summary>
        /// Merge the data to a class
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="DataLines"></param>
        /// <returns></returns>
        object MergeDataToClass(object Class, string[] DataLines)
        {
            string Result = default;
            string ItemType;
            string Item;
            // recupero todos los elementos ya sea de una clase o de una lista
            // en un Array de FieldsInfo para poder trabajar cada uno individualmente
            FieldInfo[] Fields = Class.GetType().GetFields();
            foreach (FieldInfo ItemField in Fields)
            {
                object field = ItemField.GetValue(Class);

                // Agrega un nuevo elemento junto con su valor
                // mientras sea un dato primitivo,
                // por ejemplo: \n\t<<Creador: John Carmack>>
                if (!IsGenericList(ItemField) && ItemField.Name != "Empty")
                    //Revisa si es el ultimo elemento de la lista
                    if (ItemField == Fields[Fields.Length - 1])
                        Result += $"\n<<{ItemField.Name}: {ItemField.GetValue(Class)}>>\n";
                    else
                        Result += $"\n<<{ItemField.Name}: {ItemField.GetValue(Class)}>>";

                // comprueba si lo que se le esta pasando es una lista
                // de tipo generico
                else if (IsGenericList(ItemField))
                {
                    // Result += $"\n\t<{ItemField.Name}>";
                    // en caso de ser cierto lo que hace es crear una
                    // lista generica de objetos
                    IList<object> GenObjectList = new List<object>();
                    // se compreba si el valor de la lista de elementos es un
                    // IEnumerable de objetos ademas de que los crea un objeto
                    // IEnumerable que contiene los elementos de la lista
                    if (field is IEnumerable<object> IEListObjects)
                    {
                        // y de ser cierto crea una lista con los elementos en base
                        // a los IEnumerables
                        GenObjectList = IEListObjects.ToList();

                        // se recorre cada elemento de la lista para ver si contiene mas
                        // elementos del mismo tipo dentro o son puros atributos/elementos
                        // de una lista
                        foreach (object ObjectList in GenObjectList)
                        {
                            ItemType = $"{ObjectList}";
                            Item = GetTypeByElement(ObjectList.GetType());

                            if (ObjectList != GenObjectList.First())
                            {
                                if (ItemType == Item)
                                    Result += $"\n\n!<[{Item}({GenObjectList.IndexOf(ObjectList)})]>";
                            }
                            else
                            {
                                //Agrega la Flag con el nombre de la Lista,
                                //y el tipo de dato que usa entre parentesis
                                //por ejemplo: \n<List_Words>(string)
                                Result += $"\n<{ItemField.Name}>({Item})";


                                //Agrega cada elemento que contiene la
                                //lista compleja de primer nivel adentro
                                //de su etiqueta.
                                //Por ejemplo: \n![Word]
                                if (ItemType == Item)
                                    Result += $"\n\n!<[{Item}({GenObjectList.IndexOf(ObjectList)})]>";
                            }


                            if (ItemType != Item)
                            {
                                //Agrega cada elemento que contiene la
                                //lista compleja de segundo nivel
                                //dentro de su etiqueta.
                                //Por ejemplo: \n![Hello_World]
                                Result += $"\n![{ObjectList}]";
                            }


                            //Hace un uso recursivo para poder extraer la data
                            //de todos los elementos que se encuentren a un
                            //nivel inferior dentro del objeto evaluado
                            Result += ItemsFromClass(ObjectList, ObjectList.GetType());
                            if (ObjectList == GenObjectList.Last())
                            {
                                //Si el tipo del ItemField es igual al ItemField(Number==Number),
                                //escribe esto por ejemplo: \n</Anime>\n\n
                                if (ItemType == Item)
                                {
                                    //Evalua si lo que tiene por detras es un
                                    //salto de linea y de serlo imprime lo primero
                                    //de lo contrario imprime lo segundo
                                    if (Result[Result.Length - 1].ToString() == "\n")
                                        Result += $"</{ItemField.Name}>\n\n";
                                    else
                                        Result += $"\n</{ItemField.Name}>\n\n";
                                }

                                //Si no escribira esto por ejemplo: \n</List_Words>\n\n
                                else
                                {
                                    Result += $"\n</{ItemField.Name}>";
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
                }
            }
            return null;
        }


        #endregion


        #endregion

    }
}
