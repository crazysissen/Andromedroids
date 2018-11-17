using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Andromedroids
{
    /// <summary>
    /// Automatically converts objects to and from byte arrays, not very elegant or efficient I know, whatever :P
    /// </summary>
    static class Bytestreamer
    {
        /// <summary>
        /// Converts a serializeable object object to an array of bytes
        /// </summary>
        /// <param name="target">MUST be serializeable</param>
        public static byte[] ToBytes(this object target)
        {
            if (target == null)
            {
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                formatter.Serialize(stream, target);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts a formatted byte array back to an object of type T
        /// </summary>
        public static T ToObject<T>(this byte[] bytes) 
        {
            if (bytes == null)
            {
                return default(T);
            }

            BinaryFormatter formatter = new BinaryFormatter();

            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
