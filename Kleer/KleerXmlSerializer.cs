using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Kleer;

public static class KleerXmlSerializer
{
    // Cache overrides per assembly
    private static readonly ConcurrentDictionary<Assembly, XmlAttributeOverrides> OverridesCache = new();
    // Cache serializers per root type (but reuse the same overrides)
    private static readonly ConcurrentDictionary<Type, XmlSerializer> SerializerCache = new();

    private static XmlAttributeOverrides BuildOverrides(Assembly assembly)
    {
        var overrides = new XmlAttributeOverrides();
        var processed = new HashSet<Type>();

        foreach (var type in assembly.GetTypes())
        {
            var includes = type.GetCustomAttributes(typeof(XmlIncludeAttribute), false);
            foreach (XmlIncludeAttribute inc in includes)
            {
                var incType = inc.Type;
                if (!incType.Name.EndsWith("Redefinition", StringComparison.Ordinal) ||
                    !processed.Add(incType)) continue;
                var redefAttrs = new XmlAttributes
                {
                    XmlType = new XmlTypeAttribute(
                        incType.Name.Replace("Redefinition", "_redef"))
                };
                overrides.Add(incType, redefAttrs);
            }
        }

        return overrides;
    }

    public static XmlSerializer Create<T>()
    {
        return SerializerCache.GetOrAdd(typeof(T), t =>
        {
            var assembly = typeof(T).Assembly;
            var overrides = OverridesCache.GetOrAdd(assembly, BuildOverrides);
            return new XmlSerializer(t, overrides);
        });
    }

    public static T Deserialize<T>(string xml)
    {
        var serializer = Create<T>();
        using var reader = new StringReader(xml);
        return (T)serializer.Deserialize(reader);
    }

    public static string Serialize<T>(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        var serializer = Create<T>();

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            Encoding = new UTF8Encoding(false),
            Indent = false
        };

        using var sw = new StringWriter();
        using var xw = XmlWriter.Create(sw, settings);

        var ns = new XmlSerializerNamespaces();
        ns.Add("", ""); // suppress xmlns attributes

        serializer.Serialize(xw, obj, ns);
        return sw.ToString();
    }
}
