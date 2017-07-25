﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Collections.Generic;
using System.Linq;

namespace Bundler.Taghelpers
{
    [HtmlTargetElement("script", Attributes = "asp-bundle")]
    public class ScriptTagHelper : BaseTagHelper
    {
        public ScriptTagHelper(IHostingEnvironment env)
            : base(env)
        { }

        [HtmlAttributeName("asp-bundle")]
        public string Bundle { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!string.IsNullOrEmpty(Bundle))
            {
                if (Extensions.Options.Enabled)
                {
                    var transform = Extensions.Options.Transforms.FirstOrDefault(t => t.Path.Equals(Bundle));
                    string href = $"{Bundle}?v={GenerateHash(transform)}";
                    output.Attributes.SetAttribute("src", href);
                }
                else
                {
                    WriteIndividualTags(output);
                }
            }

            base.Process(context, output);
        }

        private void WriteIndividualTags(TagHelperOutput output)
        {
            var transform = Extensions.Options.Transforms.FirstOrDefault(t => t.Path.Equals(Bundle));
            output.SuppressOutput();

            var attrs = new List<string>();

            foreach (var item in output.Attributes)
            {
                string attr = item.Name;

                if (item.ValueStyle != HtmlAttributeValueStyle.Minimized)
                {
                    var quote = GetQuote(item.ValueStyle);
                    attr += "=" + quote + item.Value + quote;
                }

                attrs.Add(attr);
            }

            foreach (string file in transform.SourceFiles)
            {
                string src = $"{file}?v={GenerateHash(file)}";
                output.PostElement.AppendHtml($"<script src=\"{src}\" {string.Join(" ", attrs)}></script>");
            }
        }
    }
}