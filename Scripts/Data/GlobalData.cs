using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SwippyBot
{
    public class GlobalData
    {
        Dictionary<string, string> globalValues;

        bool modified = false;
        DateTime lastBackup = DateTime.MinValue;

        public GlobalData()
        {

            try
            {
                globalValues = new Dictionary<string, string>();

                byte[] data;
                using (FileStream fs = new FileStream($"./Global", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);

                    fs.Close();
                }

                if (data != null && data.Length > 0)
                    Deserialize(data);

                Autosave().GetAwaiter();
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        async Task Autosave()
        {
            while (true)
            {
                if (modified)
                {
                    if ((DateTime.UtcNow - lastBackup).TotalHours >= 2)
                        Backup();

                    WriteToDisk();
                    modified = false;
                    Console.WriteLine($"Autosaved global data");
                }
                await Task.Delay(5000);
            }
        }

        public string GetValue(string key)
        {
            if (!globalValues.ContainsKey(key))
                return "";
            return globalValues[key];
        }

        public void SetValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (globalValues.ContainsKey(key))
                {
                    globalValues.Remove(key);
                    modified = true;
                }
                return;
            }

            globalValues[key] = value;
            modified = true;
        }

        public string[] GetKeys()
        {
            return globalValues.Keys.ToArray();
        }

        public void WriteToDisk()
        {
            try
            {
                using (FileStream fs = new FileStream($"./Global", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    byte[] data = Serialize();
                    fs.Write(data, 0, data.Length);

                    fs.Close();
                }
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        void Backup()
        {
            try
            {
                if (!Directory.Exists("./Backups"))
                    Directory.CreateDirectory("./Backups");

                string fileName = $"./Backups/Global_{DateTime.UtcNow.Ticks.ToString()}";
                if (File.Exists(fileName))
                    File.Delete(fileName);

                File.Copy($"./Global", fileName);
                lastBackup = DateTime.UtcNow;
            }
            catch (System.Exception ex)
            {
                Bot.ReportError(ex);
            }
        }

        byte[] Serialize()
        {
            List<byte> data = new List<byte>();
            byte[] valueData;

            //Global value count - 4
            data.AddRange(BitConverter.GetBytes(globalValues.Count));

            //Global values
            foreach (string key in globalValues.Keys)
            {
                valueData = Encoding.UTF8.GetBytes(key);
                data.AddRange(BitConverter.GetBytes(valueData.Length)); // 4
                data.AddRange(valueData); // x

                valueData = Encoding.UTF8.GetBytes(globalValues[key]);
                data.AddRange(BitConverter.GetBytes(valueData.Length)); // 4
                data.AddRange(valueData); // x
            }

            return data.ToArray();
        }

        void Deserialize(byte[] data)
        {
            globalValues = new Dictionary<string, string>();

            int valueCount, length, offset = 0;
            byte[] valueData;
            string key, value;

            //Get global value count
            valueCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            //Get global values
            for (int i = 0; i < valueCount; i++)
            {
                //Get key length
                length = BitConverter.ToInt32(data, offset);
                offset += 4;

                //Get key
                valueData = new byte[length];
                key = Encoding.UTF8.GetString(data, offset, length);
                offset += length;

                //Get value length
                length = BitConverter.ToInt32(data, offset);
                offset += 4;

                //Get value
                valueData = new byte[length];
                value = Encoding.UTF8.GetString(data, offset, length);
                offset += length;

                globalValues.Add(key, value);
            }
        }
    }
}