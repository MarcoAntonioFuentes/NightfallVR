#if UNITY_ANDROID
using System.IO;
using UnityEditor.Android;

namespace CyberShield.Editor
{
    // Dependencies specified by Google's Cardboard Unity quickstart, applied only to builds using Cardboard.
    public sealed class NightfallGradle : IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 100;
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            if(!Directory.Exists("Packages/com.google.xr.cardboard"))return;
            string file=System.IO.Path.Combine(path,"build.gradle");
            string gradle=File.ReadAllText(file);
            if(!gradle.Contains("com.google.protobuf:protobuf-javalite"))
                gradle+="\ndependencies {\n implementation 'androidx.appcompat:appcompat:1.6.1'\n implementation 'com.google.android.gms:play-services-vision:20.1.3'\n implementation 'com.google.android.material:material:1.12.0'\n implementation 'com.google.protobuf:protobuf-javalite:3.19.4'\n}\n";
            File.WriteAllText(file,gradle);
            string properties=System.IO.Path.Combine(Directory.GetParent(path).FullName,"gradle.properties");
            string text=File.Exists(properties)?File.ReadAllText(properties):"";
            if(!text.Contains("android.useAndroidX="))text+="\nandroid.useAndroidX=true\n";
            if(!text.Contains("android.enableJetifier="))text+="android.enableJetifier=true\n";
            File.WriteAllText(properties,text);
        }
    }
}
#endif
