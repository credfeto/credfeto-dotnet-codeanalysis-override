using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Credfeto.DotNet.Code.Analysis.Overrides.Helpers;

public static class RuleSet
{
    public static async ValueTask<XmlDocument> LoadAsync(string fileName, CancellationToken cancellationToken)
    {
        byte[] bytes = await File.ReadAllBytesAsync(path: fileName, cancellationToken: cancellationToken);

        await using (MemoryStream stream = new(bytes))
        {
            XmlDocument doc = new();
            doc.Load(stream);

            return doc;
        }
    }

    public static async ValueTask SaveAsync(string project, XmlDocument doc, CancellationToken cancellationToken)
    {
        XmlWriterSettings settings = new()
        {
            Async = true,
            Indent = true,
            IndentChars = "  ",
            NewLineOnAttributes = false,
            OmitXmlDeclaration = true,
        };

        byte[] bytes;

        await using (MemoryStream stream = new())
        {
            await using (XmlWriter xmlWriter = XmlWriter.Create(output: stream, settings: settings))
            {
                doc.Save(xmlWriter);
            }

            bytes = stream.ToArray();
        }

        await File.WriteAllBytesAsync(path: project, bytes: bytes, cancellationToken: cancellationToken);
    }
}
