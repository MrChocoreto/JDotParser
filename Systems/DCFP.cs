using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace JDot_Parser.Systems
{
    public class DCFP
    {

        #region Variables

        const string stg_IMF = "<mdf>\n"; // IMF = Input MainSaver Flag
                                          // mdf = MainSaver Data Flag
        const string stg_OMF = "\n</mdf>"; // OMF = Output MainSaver Flag

        readonly Dictionary<Type, string> DataTypes = new()
    {
        {typeof(int), "int"},
        {typeof(double), "double"},
        {typeof(float), "float"},
        //{typeof(long), "long"},
        //{typeof(short), "short"},
        //{typeof(decimal), "decimal"},
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
        //{"long", typeof(long) },
        //{"short", typeof(short) },
        //{"decimal", typeof(decimal) },
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
        /// <param name="obt_Cls">The Class that you want convert</param>
        /// <returns>The Class converted to String</returns>
        public string ToDataFile(object obt_Cls)
        {
            return CCTS(obt_Cls);
        }

        #endregion



        #region Get_Data

        /// <summary>
        /// Convert a String into Class
        /// </summary>
        /// <param name="stgData">String that contain the Data</param>
        /// <param name="IsPath">Determine if the "StringData" is a Path or the Data in a string</param>
        /// <returns>String converted into the Class that you need</returns>
        public GenClass ToDataClass<GenClass>(string stgData, bool IsPath = false)
        {
            //GenClass = Generic Class
            Type GenClassType = GetTypeOfGeneric<GenClass>();

            // Verificar si el tipo GenClass es un tipo válido
            // (por ejemplo, una clase con un constructor sin parámetros).
            if (!GenClassType.IsClass && GenClassType.IsAbstract && 
                GenClassType.GetConstructor(Type.EmptyTypes) == null)
            {
                // Manejar el error si GenClass no es un tipo válido para la creación de instancias.
                throw new InvalidOperationException("GenClass no es un tipo válido para crear una instancia");
            }
            else
            {
                // Crea una instancia del tipo GenClass usando la reflexión.
                GenClass instance = (GenClass)Activator.CreateInstance(GenClassType);

                // Aquí puedes realizar las operaciones necesarias para inicializar
                // "instance" con los datos de "Data".
                instance = (GenClass)ToClass(stgData, instance, IsPath);
                return instance;
            }
        }

        #endregion


        #endregion



        #region Private_Methods


        #region Class_To_Text


        /// <summary>
        /// Convert Class To String
        /// </summary>
        /// <param name="cls"> </param>
        /// <returns>Result of convert your class to Text</returns>
        string CCTS(object cls)
        {
            string stg_Result;
            if (cls != null && !(cls.GetType().Name == "String") && !cls.GetType().IsPrimitive)
            {
                stg_Result = stg_IMF;
                stg_Result += $"<{cls.GetType().Name}>";
                stg_Result += EIFC(cls, cls.GetType());
                stg_Result += $"\n</{cls.GetType().Name}>";
            }
            else
                stg_Result = stg_IMF;
            return stg_Result + stg_OMF;
        }


        /// <summary>
        /// Extract Items From Class/List
        /// </summary>
        /// <param name="cls">Class/List</param>
        /// <param name="type">Type of the Object</param>
        /// <returns>Content of the Class</returns>
        string EIFC(object cls, Type type)
        {
            string stg_Result = default;
            string stg_Item_Type;
            string stg_Item;
            // recupero todos los elementos ya sea de una clase o de una lista
            // en un Array de Fields Info para poder trabajar cada uno individualmente
            FieldInfo[] lst_Fields = type.GetFields();
            foreach (FieldInfo item in lst_Fields)
            {
                object obt_field = item.GetValue(cls);

                // Agrega un nuevo elemento junto con su valor
                // mientras sea un dato primitivo,
                // por ejemplo: \n\t<<Creador: John Carmack>>
                if (!IsGenericList(item) && item.Name != "Empty")
                    stg_Result += $"\n\t<<{item.Name}: {item.GetValue(cls)}>>";

                // comprueba si lo que se le esta pasando es una lista generica
                else if(IsGenericList(item))
                {
                    // stg_Result += $"\n\t<{item.Name}>";
                    // en caso de ser cierto lo que hace es crear una lista generica
                    IList<object> lst_ElementsList = new List<object>();
                    // se compreba si el valor de la lista de elementos es un
                    // IEnumerable de objetos ademas de que los convierte IEnumerable
                    if (obt_field is IEnumerable<object> enumerable)
                    {
                        // de ser cierto crea una lista con los elementos en base
                        // a los IEnumerables
                        lst_ElementsList = enumerable.ToList();

                        // se recorre cada elemento de la lista para ver si contiene mas
                        // elementos del mismo tipo dentro o son puros atributos/elementos
                        // de una lista
                        foreach (object obtElement in lst_ElementsList)
                        {
                            stg_Item_Type = $"{obtElement}";
                            stg_Item = OTE(obtElement.GetType());

                            if (obtElement == lst_ElementsList.First())
                            {
                                //Agrega la Flag con el nombre de la Lista,
                                //y el tipo de dato que usa entre parentesis
                                //por ejemplo: \n\t<List_Words>(string)
                                stg_Result += $"\n\t<{item.Name}>({stg_Item})";


                                //Agrega cada elemento que contiene la
                                //lista compleja adentro de su etiqueta,
                                //por ejemplo: \n\t![Word]
                                if (stg_Item_Type == stg_Item)
                                    stg_Result += $"\n\t!<[{stg_Item}]>\n";
                            }

                            if (stg_Item_Type != stg_Item)
                            {
                                //Agrega cada elemento que contiene la lista
                                //dentro de su etiqueta, por ejemplo:
                                // \n\t\t![Hello_World]
                                stg_Result += $"\n\t\t![{obtElement}]";
                            }


                            //Hace un uso recursivo para poder extraer la data
                            //de todos los elementos
                            stg_Result += EIFC(obtElement, obtElement.GetType());
                            if (obtElement == lst_ElementsList.Last())
                            {
                                //Si el tipo del item es igual al item(Number==Number),
                                //escribe esto por ejemplo: \n\t</Anime>\n\n
                                if (stg_Item_Type == stg_Item)
                                {
                                    stg_Result += $"\n\t</{item.Name}>\n\n";
                                }

                                //Si no escribira esto por ejemplo: \n\t</List_Words>
                                else
                                {
                                    stg_Result += $"\n\t</{item.Name}>";
                                }


                                //la diferencia entre el primer caso y el ultimo
                                //consiste en que el primer caso permite la
                                //separacion entre objetos pertenecientes a una
                                //"Lista Compleja" por eso el doble salto de linea
                                //en el primer caso
                            }
                        }
                    }
                }
                
                 
            }
            return stg_Result;
        }


        /// <summary>
        /// Is a Generic List
        /// </summary>
        /// <param name="fieldInfo">List</param>
        /// <returns>True or False if the field is generic or not</returns>
        static bool IsGenericList(FieldInfo fieldInfo)
        {
            Type fieldType = fieldInfo.FieldType;

            return fieldType.IsGenericType &&
            fieldType.GetGenericTypeDefinition() == typeof(List<>);
        }


        /// <summary>
        /// Obtain Type by Element
        /// </summary>
        /// <param name="type">Type of the Object</param>
        /// <returns>A string with the "Type" of the object </returns>
        string OTE(Type type)
        {
            string stgResult;
            if (DataTypes.TryGetValue(type, out string value))
                stgResult = value;
            else
                stgResult = type.Name;
            //else if (Nullable.GetUnderlyingType(type) != null)
            //{
            //    stgResult = "null";
            //}
            return stgResult;
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
            {

                return null;
            }
        }


        /// <summary>
        /// Get Class By String
        /// </summary>
        /// <param name="Class"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        object GetClassByString(object Class, string Data)
        {
            string[] DataLines;
            DataLines = Data.Split(@"\n");

            //Debug Color
            Console.ForegroundColor = ConsoleColor.Green;
            foreach (string line in DataLines)
            {
                //TODO
                Console.WriteLine(line);
            }
            return null;
        }



        /// <summary>
        /// Get Data By Path
        /// </summary>
        /// <param name="DataPath"></param>
        /// <returns></returns>
        string[] GetDataByPath(string DataPath)
        {
            string[] FullData;
            string Lines;
            StreamReader Reader = new(DataPath);

            if (!File.Exists(DataPath))
                return null;
            else
            {
                Lines = Reader.ReadToEnd();
                Reader.Close();
                return FullData = Lines.Split("\n");
            }
        }


        #endregion


        #endregion

    }
}
