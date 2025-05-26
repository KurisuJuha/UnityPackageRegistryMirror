using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

public class PackageIndexer
{
    [InitializeOnLoadMethod]
    public static void Run()
    {
        var request = Client.SearchAll();

        while (!request.IsCompleted) Thread.Sleep(100);

        if (request.Status != StatusCode.Success) Debug.LogError(request.Error.message);

        var packages = request.Result;

        var output = new StringBuilder();
        output.Append($@"{{""count"":{packages.Length}, ""packages"":{{");

        for (var i = 0; i < packages.Length; i++)
        {
            var package = packages[i];
            Debug.Log($"adding {package.name}");

            output.Append($@"
""{package.name}"":{{
    ""name"":""{package.name}"",
    ""displayName"":""{package.displayName}"",
    ""description"":""{package.description.Replace("\\", "\\\\").Replace("\n", "\\n").Replace("\"", "\\\"").Replace("\t", "\\t")}"",
    ""version"":""{package.version}"",
    ""dependencies"":{{
        {
            string.Join(",", package.dependencies.Select(d => $"\"{d.name}\":\"{d.version}\""))
        }
    }},
    ""documentation"":""{package.documentationUrl}""
}}
");

            if (i < packages.Length - 1) output.Append(",");
        }

        output.Append("}}");

        Debug.Log(output.ToString());
        File.WriteAllText("unity-packages.json", output.ToString());
    }
}
