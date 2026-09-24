using UnityEngine;

namespace Ghosty
{
    public class SettingsData : ScriptableObject
    {
        public bool openCodeStartup;
        public bool uploadOnExport;
        public bool askBeforeUpload = true;
        public string mapId = "";
        public string filePath = "";
        //public string luauSaveData = ""; // not in this version
    }
}
