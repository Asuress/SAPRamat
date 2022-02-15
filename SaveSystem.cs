using System.Text.Json;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace SAPRamat
{
    class SaveSystem
    {
        public static void SaveViaJson<T>(T obj, string path)
        {
            string jsonStr = JsonSerializer.Serialize(obj, new JsonSerializerOptions() { WriteIndented = true});
            if (File.Exists(path))
            {
                File.WriteAllText(path, jsonStr);
            }
            else
            {
                using (FileStream fStream = File.Create(path))
                {
                    fStream.Write(new UTF8Encoding().GetBytes(jsonStr));
                    fStream.Close();
                }
            }
        }
        public static T LoadViaJson<T>(string path)
        {
            var jsonstr = File.ReadAllText(path);
            T variable = JsonSerializer.Deserialize<T>(jsonstr);
            return variable;
        }

        public static bool Load()
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".json";
            if (Directory.Exists($"{Directory.GetCurrentDirectory()}\\saves"))
            {
                dlg.InitialDirectory = $@"{Directory.GetCurrentDirectory()}\saves";
            }
            else
            {
                Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\saves");
                dlg.InitialDirectory = $@"{Directory.GetCurrentDirectory()}\saves";
            }

            var result = dlg.ShowDialog();

            if (result != false)
            {
                string path = dlg.FileName;
                Project.Current = LoadViaJson<Project>(path);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Save()
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                DefaultExt = "json",
                InitialDirectory = Directory.Exists($@"{Directory.GetCurrentDirectory()}\saves")
                ? $@"{Directory.GetCurrentDirectory()}\saves"
                : $@"{Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\saves")}"
            };

            if (dlg.ShowDialog() == true)
            {
                SaveViaJson(Project.Current, dlg.FileName);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
