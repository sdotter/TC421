namespace TC421
{
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Text;
    using System.Collections.Generic;
    using System.Runtime.Serialization.Formatters.Binary;
    

    [Serializable]
    public class ProjectItem
    {
        private static ProjectItem _Item;
        private string _ProjectPath;
        private Dictionary<string, ModelItem> _ModelSet;

        public string ProjectPath
        {
            set => this._ProjectPath = value;
            get => this._ProjectPath ?? (this._ProjectPath = this.GetSettingsFilePath());
        }

        public string ProjectName { set; get; }

        public Dictionary<string, ModelItem> ModelSet
        {
            set => this._ModelSet = value;
            get => this._ModelSet ?? (this._ModelSet = new Dictionary<string, ModelItem>());
        }

        public static ProjectItem Instance
        {
            get => ProjectItem._Item ?? (ProjectItem._Item = new ProjectItem());
            set => ProjectItem._Item = value;
        }

        private string GetSettingsFilePath() => new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).CreateSubdirectory("TC421").FullName + Path.DirectorySeparatorChar.ToString();

        public bool Save(string filename = "profile.json", bool IsCreat = false)
        {
            string path = string.Format("{0}", (object)this.ProjectPath);
            Directory.CreateDirectory(path);
            if (IsCreat && File.Exists(path + string.Format("\\{0}", (object)filename)))
                return false;
            using (FileStream serializationStream = new FileStream(path + string.Format("\\{0}", (object)filename), FileMode.Create))
            {
                byte[] bytesA = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((ProjectItem)this));
                serializationStream.Write(bytesA, 0, bytesA.Length);
                return true;
            }
        }

        public ProjectItem Clone()
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream serializationStream = new MemoryStream())
            {
                binaryFormatter.Serialize((Stream)serializationStream, (ProjectItem)this);
                serializationStream.Position = 0L;
                return binaryFormatter.Deserialize((Stream)serializationStream) as ProjectItem;
            }
        }

        public ProjectItem Open(string filename = null)
        {
            if (string.IsNullOrEmpty(filename)) filename = this.ProjectPath + "profile.json";
            ProjectItem._Item = JsonConvert.DeserializeObject<ProjectItem>(Encoding.ASCII.GetString(File.ReadAllBytes(filename)));
            return ProjectItem._Item;
        }
    }
}
