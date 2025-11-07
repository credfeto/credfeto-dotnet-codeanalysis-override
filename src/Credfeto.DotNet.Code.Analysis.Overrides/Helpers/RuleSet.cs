using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Helpers;

public static class RuleSet
{
    public static async ValueTask<XmlDocument> LoadAsync(string fileName)
    {
        await using (Stream stream = File.OpenRead(fileName))
        {
            XmlDocument doc = new();
            doc.Load(stream);

            return doc;
        }
    }

    public static async ValueTask SaveAsync(string project, XmlDocument doc)
    {
        XmlWriterSettings settings = new()
        {
            Indent = true,
            IndentChars = "  ",
            NewLineOnAttributes = false,
            OmitXmlDeclaration = true,
            Async = true,
        };

        await using (XmlWriter xmlWriter = XmlWriter.Create(outputFileName: project, settings: settings))
        {
            doc.Save(xmlWriter);
            await ValueTask.CompletedTask;
        }
    }
}
