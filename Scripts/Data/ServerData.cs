using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace SwippyBot
{
    public class ServerData
    {
        public ulong ServerID { get; }

        Dictionary<string, string> serverValues;
        Dictionary<ulong, Dictionary<string, string>> userValues;

        bool modified = false;
        DateTime lastBackup = DateTime.MinValue;

        public ServerData(ulong serverID)
        {
            ServerID = serverID;

            try
            {
                serverValues = new Dictionary<string, string>();
                userValues = new Dictionary<ulong, Dictionary<string, string>>();

                if (!Directory.Exists("./ServerData"))
                    Directory.CreateDirectory("./ServerData");

                byte[] data;
                using (FileStream fs = new FileStream($"./ServerData/{ServerID}", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
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
                    //Bot.ReportError(ServerID, new Exception("Autosave"));
                    if ((DateTime.UtcNow - lastBackup).TotalHours >= 2)
                        Backup();

                    WriteToDisk();
                    modified = false;
                    Console.WriteLine($"Autosaved {ServerID} server data");
                }
                await Task.Delay(5000);
            }
        }

        public string GetServerValue(string key)
        {
            if (!serverValues.ContainsKey(key))
                return "";
            return serverValues[key];
        }

        public void SetServerValue(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                if (serverValues.ContainsKey(key))
                {
                    serverValues.Remove(key);
                    modified = true;
                }
                return;
            }

            serverValues[key] = value;
            modified = true;
        }

        public string GetUserValue(ulong userID, string key)
        {
            if (!userValues.ContainsKey(userID) || !userValues[userID].ContainsKey(key))
                return "";
            return userValues[userID][key];
        }

        public string[] GetUserKeys(ulong userID)
        {
            if (userValues.ContainsKey(userID))
                return userValues[userID].Keys.ToArray();
            return new string[0];
        }

        public string[] GetServerKeys()
        {
            return serverValues.Keys.ToArray();
        }

        public void SetUserValue(ulong userID, string key, string value)
        {
            if (!userValues.ContainsKey(userID))
                userValues.Add(userID, new Dictionary<string, string>());

            if (string.IsNullOrEmpty(value))
            {
                if (userValues[userID].ContainsKey(key))
                {
                    userValues[userID].Remove(key);
                    modified = true;
                }
                return;
            }
            userValues[userID][key] = value;
            modified = true;
        }

        public void WriteToDisk()
        {
            try
            {
                if (!Directory.Exists("./ServerData"))
                    Directory.CreateDirectory("./ServerData");

                using (FileStream fs = new FileStream($"./ServerData/{ServerID}", FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
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
                //Bot.ReportError(ServerID, new Exception("Backup"));

                if (!Directory.Exists("./ServerData"))
                    Directory.CreateDirectory("./ServerData");

                if (!Directory.Exists("./Backups"))
                    Directory.CreateDirectory("./Backups");

                if (!Directory.Exists($"./Backups/{ServerID}"))
                    Directory.CreateDirectory($"./Backups/{ServerID}");

                string fileName = $"./Backups/{ServerID}/{DateTime.UtcNow.Ticks.ToString()}";
                if (File.Exists(fileName))
                    File.Delete(fileName);

                File.Copy($"./ServerData/{ServerID}", fileName);
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

            //Server value count - 4
            data.AddRange(BitConverter.GetBytes(serverValues.Count));

            //Server values
            foreach (string key in serverValues.Keys)
            {
                valueData = Encoding.UTF8.GetBytes(key);
                data.AddRange(BitConverter.GetBytes(valueData.Length)); // 4
                data.AddRange(valueData); // x

                valueData = Encoding.UTF8.GetBytes(serverValues[key]);
                data.AddRange(BitConverter.GetBytes(valueData.Length)); // 4
                data.AddRange(valueData); // x
            }

            //User count - 4
            data.AddRange(BitConverter.GetBytes(userValues.Count));

            //User values
            foreach (ulong userKey in userValues.Keys)
            {
                if (userValues[userKey] != null)
                {
                    data.AddRange(BitConverter.GetBytes(userKey)); // 8
                    data.AddRange(BitConverter.GetBytes(userValues[userKey].Count)); // 4

                    foreach (string valueKey in userValues[userKey].Keys)
                    {
                        valueData = Encoding.UTF8.GetBytes(valueKey);
                        data.AddRange(BitConverter.GetBytes(valueData.Length)); // 4
                        data.AddRange(valueData); // x

                        valueData = Encoding.UTF8.GetBytes(userValues[userKey][valueKey]);
                        data.AddRange(BitConverter.GetBytes(valueData.Length)); // 4
                        data.AddRange(valueData); // x
                    }
                }
            }

            return data.ToArray();
        }

        void Deserialize(byte[] data)
        {
            serverValues = new Dictionary<string, string>();
            userValues = new Dictionary<ulong, Dictionary<string, string>>();

            int valueCount, length, offset = 0;
            byte[] valueData;
            string key, value;

            //Get server value count
            valueCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            //Get server values
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

                serverValues.Add(key, value);
            }

            //Get user value count
            ulong userID;
            int userCount, userValueCount;

            userCount = BitConverter.ToInt32(data, offset);
            offset += 4;

            //Get user values
            for (int i = 0; i < userCount; i++)
            {
                //Get user ID
                userID = BitConverter.ToUInt64(data, offset);
                offset += 8;

                //Get user value count
                userValueCount = BitConverter.ToInt32(data, offset);
                offset += 4;

                userValues.Add(userID, new Dictionary<string, string>());

                for (int j = 0; j < userValueCount; j++)
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

                    userValues[userID].Add(key, value);
                }
            }
        }
    }
}