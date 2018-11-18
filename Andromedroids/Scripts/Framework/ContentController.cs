using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;

namespace Andromedroids
{
    /// <summary>
    /// The main content manager.
    /// </summary>
    public static class ContentController
    {
        static readonly string[] excludeFiles = new string[] { "Runescape", "HelloWorld" };

        static Dictionary<string, object> contentDictionary;

        static Dictionary<string, object> contentCollections;

        static ContentController()
        {
            contentDictionary = new Dictionary<string, object>();
            contentCollections = new Dictionary<string, object>();
        }

        public static void Initialize(ContentManager content, bool importAll)
        {
            if (importAll)
            {
                ImportAll(content);
            }
        }

        public static void ImportAll(ContentManager content)
        {
            ContentBundle bundle = AllFileNames(AppDomain.CurrentDomain.BaseDirectory + @"\" + content.RootDirectory, "", "");

            foreach (ImportObject item in bundle.Objects)
            {
                contentDictionary.Add(item.name, content.Load<object>(item.path));
            }

            foreach (ImportCollection item in bundle.Collections)
            {
                List<object> objects = new List<object>();

                foreach (string tag in item.names)
                {
                    objects.Add(contentDictionary[tag]);
                }

                contentCollections.Add(item.collectionName, objects.ToArray());
            }

            Dictionary<string, object> contentDictionaryTest = contentDictionary;

            Dictionary<string, object> contentCollectionsTest = contentCollections;

            #region Attribute Test
            //Type[] allTypes = Assembly.GetExecutingAssembly().GetTypes();

            //foreach (Type currentClass in allTypes)
            //{
            //    PropertyInfo[] properties = currentClass.GetProperties(BindingFlags.Static | BindingFlags.Public);

            //    foreach (PropertyInfo property in properties)
            //    {
            //        if (property.GetCustomAttribute<ImportAttribute>() != null)
            //        {
            //            Type type = property.GetCustomAttribute<ImportAttribute>().type;

            //            if (property.PropertyType == typeof(string[]))
            //            {
            //                string[] array = property.GetMethod.Invoke(null, null) as string[];

            //                foreach (string item in array)
            //                {
            //                    singleton.contentDictionary.Add(item, Convert.ChangeType(content.Load<object>(item), type));
            //                }

            //                continue;
            //            }

            //            throw new Exception("Tried to import a non-string array.");
            //        }
            //    }
            //}
            #endregion
        }

        public static ContentBundle AllFileNames(string basePath, string additionalPath, string appendableAdditionalPath)
        {
            List<ImportObject> allFiles = new List<ImportObject>();
            List<ImportCollection> allCollections = new List<ImportCollection>();

            DirectoryInfo directory = new DirectoryInfo(basePath + @"\" + additionalPath);
            DirectoryInfo[] directories = directory.GetDirectories();
            FileInfo[] files = directory.GetFiles();

            string currentCollection = appendableAdditionalPath.Length == 0 ? "Root" : appendableAdditionalPath.Remove(appendableAdditionalPath.Length - 1);
            List<string> currentCollectionNames = new List<string>();

            foreach (FileInfo file in files)
            {
                string currentName = Path.GetFileNameWithoutExtension(file.FullName);

                if (excludeFiles.Contains(currentName) || Path.GetExtension(file.FullName) == ".wma")
                    continue;

                allFiles.Add(new ImportObject(currentName, appendableAdditionalPath + currentName));
                currentCollectionNames.Add(currentName);
            }

            foreach (DirectoryInfo dir in directories)
            {
                ContentBundle dirImport = AllFileNames(basePath, additionalPath + @"\" + dir.Name, appendableAdditionalPath + dir.Name + "/");

                allFiles.AddRange(dirImport.Objects);
                allCollections.AddRange(dirImport.Collections);
            }

            allCollections.Add(new ImportCollection(currentCollectionNames.ToArray(), currentCollection));

            return new ContentBundle(allFiles.ToArray(), allCollections.ToArray());
        }

        public static T Get<T>(string tag)
        {
            try
            {
                if (contentDictionary[tag] is T)
                {
                    return (T)contentDictionary[tag];
                }

                return default(T);
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("ERROR: Tried to get unloaded content [" + tag + ", " + typeof(T) + "]");
                return default(T);
            }
        }

        public static T[] GetRange<T>(params string[] tags)
        {
            T[] returnArray = new T[tags.Length];

            for (int i = 0; i < returnArray.Length; ++i)
            {
                returnArray[i] = Get<T>(tags[i]);
            }

            return returnArray;

        }

        public static T[] GetCollection<T>(string tag)
        {
            if (contentCollections.ContainsKey(tag))
            {
                if (contentCollections[tag] is T[])
                {
                    return contentCollections[tag] as T[];
                }

                if (contentCollections[tag] is object[])
                {
                    object[] array = (object[])contentCollections[tag];
                    T[] newArray = new T[array.Length];

                    for (int i = 0; i < newArray.Length; i++)
                    {
                        newArray[i] = (T)array[i];
                    }

                    return newArray;
                }

                System.Diagnostics.Debug.WriteLine("ERROR: Tried to get content bundle already loaded into another type [" + tag + ", requested type: " + typeof(T[]) +  ", loaded type: " + contentCollections[tag].GetType() + "]");
                return contentCollections[tag] as T[];
            }

            System.Diagnostics.Debug.WriteLine("ERROR: Tried to get unloaded content bundle [" + tag + ", " + typeof(T[]) + "]");
            return null;
        }

        public static bool Exists(string tag) => contentDictionary.ContainsKey(tag);

        public static void Add(string tag, object obj) => contentDictionary.Add(tag, obj);

        public struct ContentBundle
        {
            public ImportObject[] Objects { get; private set; }
            public ImportCollection[] Collections { get; private set; }

            public ContentBundle(ImportObject[] objects, ImportCollection[] collections)
            {
                Objects = objects;
                Collections = collections;
            }
        }

        public struct ImportObject
        {
            public string name;
            public string path;

            public ImportObject(string name, string path)
            {
                this.name = name;
                this.path = path;
            }
        }

        public struct ImportCollection
        {
            public string[] names;
            public string collectionName;

            public ImportCollection(string[] names, string collectionName)
            {
                this.names = names;
                this.collectionName = collectionName;
            }
        }
    }
}