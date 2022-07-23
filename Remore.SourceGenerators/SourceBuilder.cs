using System;
using System.Collections.Generic;
using System.Text;

namespace Remore.SourceGenerators
{
    public class SourceBuilder
    {
        private StringBuilder _sb;
        private int _currentDepth = 0;
        public SourceBuilder()
        {
            _sb = new StringBuilder();
        }

        public void AddUsing(string @using)
        {
            AppendLine($"using {@using};");
        }
        public void OpenNamespaceScope(string @namespace)
        {
            AppendLine("namespace " + @namespace);
            AppendLine("{");
            _currentDepth++;
        }
        public void OpenClassScope(string accessModifiers, string @class)
        {
            AppendLine(accessModifiers + " class " + @class);
            AppendLine("{");
            _currentDepth++;
        }
        public void OpenMethodScope(string accessModifiers, string returnType, string name, params string[] arguments)
        {
            AppendLine($"{accessModifiers} {returnType} {name}({string.Join(", ", arguments)})");
            AppendLine("{");
            _currentDepth++;
        }
        public void AddMethodCall(string methodName, int plusDepth = 0, params string[] arguments)
        {
            _currentDepth += plusDepth;
            AppendLine($"{methodName}({string.Join(", ", arguments)});");
            _currentDepth -= plusDepth;
        }
        public void AddLine(string line, int plusDepth = 0)
        {
            _currentDepth += plusDepth;
            AppendLine(line);
            _currentDepth -= plusDepth;
        }
        public void CloseMethodScope()
        {
            _currentDepth--;
            AppendLine("}");
        }
        public void CloseClassScope()
        {
            _currentDepth--;
            AppendLine("}");
        }
        public void CloseNamespaceScope()
        {
            _currentDepth--;
            AppendLine("}");
        }
        public void NewLine()
        {
            AppendLine("", false);
        }

        public override string ToString()
        {
            return _sb.ToString();
        }
        private void AppendLine(string line, bool withDepth = true)
        {
            _sb.AppendLine((withDepth ? new string(' ', _currentDepth * 4) : "") + line);
        }
        private void Append(string text, bool withDepth = false)
        {
            _sb.Append((withDepth ? new string(' ', _currentDepth * 4) : "") + text);
        }
    }
}
