using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Remore.Library.Packets;
using Remore.Library.Attributes;
using System.Collections.Generic;
using System;
using System.Xml.Linq;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using System.Diagnostics;

namespace Remore.SourceGenerators
{
    //TODO: use SourceBuilder to build this generator
    [Generator]
    public class PacketSendingMethodsGenerator : ISourceGenerator
    {
        private List<Type> _packetsToGenerate;

        public void Execute(GeneratorExecutionContext context)
        {
            _packetsToGenerate = Assembly.GetAssembly(typeof(IPacket))
                .GetTypes()
                .Where(x => !x.IsInterface && typeof(IPacket).IsAssignableFrom(x))
                .Where(x => x.FullName.StartsWith("Remore.Library.Packets.Client") &&
                            x.GetCustomAttribute<SourceGeneratorIgnorePacketAttribute>() == null).ToList();
            var generatedCode = "";
            foreach (var packet in _packetsToGenerate)
            {
                var properties = packet.GetProperties().Where(x => x.Name != "Id" && x.Name != "RequestId");
                var arguments = string.Join(", ", properties.Select(x => $"{x.PropertyType.FullName} {ToCamelCase(x.Name)}").ToList());
                var methodCode = $@"
        public static async Task Send{packet.Name}(this RemoreClient client{(arguments.Length > 0 ? ", " : "")}{arguments})
        {{
            var packet = new {packet.Name}() 
            {{
                {string.Join("\n\t\t\t\t", properties.Select(x => $"{x.Name} = {ToCamelCase(x.Name)},"))}
            }};
            await client.SendPacketTCP(packet);
        }}
";
                generatedCode += $"{methodCode}\n";
            }
            var code = $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Remore.Client.Core.Utility;
using Remore.Library.Packets.Client;
using Remore.Library.Packets.Server;
using Remore.Library.Packets;
using Remore.Client.Core.Exceptions;


namespace Remore.Client.Core
{{
    public static class RemoreClientExtensions
    {{
        {generatedCode}
    }}
}}
";
            context.AddSource("RemoreClientExtensions.Generated.cs", SourceText.From(code, Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }

        private string ToCamelCase(string str)
        {
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }
    }
}