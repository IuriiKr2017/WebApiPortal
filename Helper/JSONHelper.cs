using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;

namespace B2BWebApi.Helper
{
    public class JsonHelper
    {
        public static T GetObjectFromJSON<T>(string jsonString)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(T));
            byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);

            try
            {
                using (MemoryStream fs = new MemoryStream(jsonData))
                {
                    T obj = (T)jsonFormatter.ReadObject(fs);

                    return obj;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error parse json object - {0}. JSON - {1}", ex.Message, jsonString));
            }
        }

        public static List<T> GetObjectsFromJSON<T>(string jsonString)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(typeof(List<T>));
            byte[] jsonData = Encoding.UTF8.GetBytes(jsonString);

            try
            {
                using (MemoryStream fs = new MemoryStream(jsonData))
                {
                    List<T> obj = (List<T>)jsonFormatter.ReadObject(fs);

                    return obj;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error parse json object - {0}. JSON - {1}", ex.Message, jsonString));
            }
        }

        public static string GetJSONFromObjects<T>(List<T> objects)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<T>));
                serializer.WriteObject(memoryStream, objects);

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }

        public static string GetJSONFromObject<T>(T obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(memoryStream, obj);

                return Encoding.UTF8.GetString(memoryStream.ToArray());
            }
        }


    }
}