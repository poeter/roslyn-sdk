using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace Analyzer1
{
    [Generator]
    public class SettingsXmlGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            // Using the context, get any additional files that end in .xmlsettings
            IEnumerable<AdditionalText> settingsFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith(".xmlsettings"));
            foreach (AdditionalText settingsFile in settingsFiles)
            {
                ProcessSettingsFile(settingsFile, context);
            }
        }

        private void ProcessSettingsFile(AdditionalText xmlFile, GeneratorExecutionContext context)
        {
            // try and load the settings file
            XmlDocument xmlDoc = new XmlDocument();
            string text = xmlFile.GetText(context.CancellationToken).ToString();
            try
            {
                xmlDoc.LoadXml(text);
            }
            catch
            {
                //TODO: issue a diagnostic that says we couldn't parse it
                return;
            }


            // create a class in the XmlSetting class that represnts this entry, and a static field that contains a singleton instance.
            string fileName = Path.GetFileName(xmlFile.Path);
            string name = xmlDoc.DocumentElement.Name;

            StringBuilder sb = new StringBuilder($@"
namespace AutoSettings
{{
    using System;
    using System.Xml;

    public partial class {name}
    {{
 ");

            for (int i = 0; i < xmlDoc.DocumentElement.ChildNodes.Count; i++)
            {
                XmlElement setting = (XmlElement)xmlDoc.DocumentElement.ChildNodes[i];
                string settingName = setting.GetAttribute("name");
                string settingType = setting.GetAttribute("type");

                sb.Append($@" 
        public {settingType} {settingName} {{ get; set;}}
    ");
            }

            sb.AppendLine("}");
            sb.AppendLine("}");

            context.AddSource($"{name}.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
        }

        public void Initialize(GeneratorInitializationContext context)
        {
        }
    }
}
