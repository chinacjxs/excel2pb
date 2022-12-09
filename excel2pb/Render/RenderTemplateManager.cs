using System.Collections.Generic;
using System.IO;
using DotLiquid;

namespace excel2pb
{
    public class RenderTemplateManager : Singleton<RenderTemplateManager>
    {
        List<string> m_SearchPath = new List<string>();

        Dictionary<string, Template> m_Templates = new Dictionary<string, Template>();

        public Template GetTemplate(string name)
        {
            Template template = null;
            if(!m_Templates.TryGetValue(name,out template))
            {
                string templateStr = GetTemplateStr(name);
                if(templateStr != null)
                    template = Template.Parse(templateStr);

                if (template.Errors.Count != 0)
                    throw template.Errors[1];

                m_Templates.Add(name, template);
            }
            return template;
        }

        string GetTemplateStr(string name)
        {
            string str = null;
            for (int i = 0; i < m_SearchPath.Count; i++)
            {
                var path = Path.Combine(m_SearchPath[i], name);
                if (File.Exists(path))
                {
                    str = File.ReadAllText(path);
                    break;
                }
            }
            return str;
        }
    }
}
