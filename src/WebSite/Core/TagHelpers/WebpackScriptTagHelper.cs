using System;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebSite.Core.TagHelpers
{
    using Config;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// <see cref="ITagHelper"/> implementation targeting &lt;script&gt; elements that supports fallback src paths.
    /// </summary>
    /// <remarks>
    /// The tag helper won't process for cases with just the 'src' attribute.
    /// </remarks>
    [HtmlTargetElement("script", Attributes = WebpackSrcAttributeName)]
    [HtmlTargetElement("script", Attributes = AppendVersionAttributeName)]
    public class WebpackScriptTagHelper : UrlResolutionTagHelper
    {
        private const string WebpackSrcAttributeName = "webpack-src";
        private const string SrcAttributeName = "src";
        private const string AppendVersionAttributeName = "asp-append-version";

        private IFileVersionProvider? _fileVersionProvider;
        private StringWriter? _stringWriter;

        private readonly DevelopmentSettings devSettings;
        private readonly WebpackTagHelpersSettings tagHelpersSettings;

        /// <summary>
        /// Creates a new <see cref="ScriptTagHelper"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
        /// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
        /// <param name="devSettingsOptions">Development settings.</param>
        /// <param name="tagHelpersOptions">Webpack Tag Helpers settings.</param>
        public WebpackScriptTagHelper(
            IHostEnvironment hostingEnvironment,
            IMemoryCache cache,
            HtmlEncoder htmlEncoder,
            IUrlHelperFactory urlHelperFactory,
            IOptions<DevelopmentSettings> devSettingsOptions,
            IOptions<WebpackTagHelpersSettings> tagHelpersOptions)
            : base(urlHelperFactory, htmlEncoder)
        {
            HostingEnvironment = hostingEnvironment;
            Cache = cache;
            devSettings = devSettingsOptions.Value;
            tagHelpersSettings = tagHelpersOptions.Value;
        }

        /// <inheritdoc />
        public override int Order => -1000;

        /// <summary>
        /// Address of the external script to use.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(SrcAttributeName)]
        public string? Src { get; set; }

        /// <summary>
        /// A Webpack-served JavaScript file to load.
        /// The JavaScript file is assessed relative to the application's 'webroot' setting.
        /// </summary>
        [HtmlAttributeName(WebpackSrcAttributeName)]
        public string? WebpackSrc { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to src urls.
        /// </summary>
        /// <remarks>
        /// A query string "v" with the encoded content of the file is added.
        /// </remarks>
        [HtmlAttributeName(AppendVersionAttributeName)]
        public bool? AppendVersion { get; set; }

        protected IHostEnvironment HostingEnvironment { get; }

        protected IMemoryCache Cache { get; }

        // Shared writer for determining the string content of a TagHelperAttribute's Value.
        private StringWriter StringWriter
        {
            get
            {
                if (_stringWriter == null)
                {
                    _stringWriter = new StringWriter();
                }

                return _stringWriter;
            }
        }

        /// <inheritdoc />
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            // Pass through attribute that is also a well-known HTML attribute.
            if (Src != null)
            {
                output.CopyHtmlAttribute(SrcAttributeName, context);
            }

            // If there's no "src" attribute in output.Attributes this will noop.
            ProcessUrlAttribute(SrcAttributeName, output);

            // Retrieve the TagHelperOutput variation of the "src" attribute in case other TagHelpers in the
            // pipeline have touched the value. If the value is already encoded this ScriptTagHelper may
            // not function properly.
            Src = output.Attributes[SrcAttributeName]?.Value as string;

            if (ShouldAppendVersion())
            {
                EnsureFileVersionProvider();

                if (Src != null)
                {
                    var index = output.Attributes.IndexOfName(SrcAttributeName);
                    var existingAttribute = output.Attributes[index];
                    output.Attributes[index] = new TagHelperAttribute(
                        existingAttribute.Name,
                        _fileVersionProvider?.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, Src),
                        existingAttribute.ValueStyle);
                }
            }

            var builder = output.PostElement;
            builder.Clear();

            if (!string.IsNullOrEmpty(WebpackSrc))
            {
                BuildWebpackScriptTag(output.Attributes, builder);

                if (string.IsNullOrEmpty(Src))
                {
                    // Only SrcInclude is specified. Don't render the original tag.
                    output.TagName = null;
                    output.Content.SetContent(string.Empty);
                }
            }
        }

        private void BuildWebpackScriptTag(
            TagHelperAttributeList attributes,
            TagHelperContent builder)
        {
            // Don't build duplicate script tag for the original source url.
            if (string.Equals(Src, WebpackSrc, StringComparison.OrdinalIgnoreCase))
                return;

            var webpackSrc = HostingEnvironment.IsDevelopment() ? string.Format("//localhost:{0}{1}", devSettings.WebpackDevServerPort, WebpackSrc) : WebpackSrc;

            BuildScriptTag(webpackSrc, attributes, builder); ;
        }

        private string? GetVersionedSrc(string? srcValue)
        {
            if (ShouldAppendVersion() && !HostingEnvironment.IsDevelopment())
            {
                srcValue = _fileVersionProvider?.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, srcValue);
            }

            return srcValue;
        }

        private void AppendVersionedSrc(
            string srcName,
            string? srcValue,
            HtmlAttributeValueStyle valueStyle,
            IHtmlContentBuilder builder)
        {
            srcValue = GetVersionedSrc(srcValue);

            builder.AppendHtml(" ");
            var attribute = new TagHelperAttribute(srcName, srcValue, valueStyle);
            attribute.CopyTo(builder);
        }

        private void EnsureFileVersionProvider()
        {
            if (_fileVersionProvider == null)
            {
                _fileVersionProvider = ViewContext.HttpContext.RequestServices.GetRequiredService<IFileVersionProvider>();
            }
        }

        private bool ShouldAppendVersion()
        {
            if (AppendVersion == false)
                return false;

            return AppendVersion == true || tagHelpersSettings.AutoAppendFileVersions;
        }

        private void BuildScriptTag(
            string? src,
            TagHelperAttributeList attributes,
            TagHelperContent builder)
        {
            builder.AppendHtml("<script");

            var addSrc = true;

            // Perf: Avoid allocating enumerator
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];
                if (!attribute.Name.Equals(SrcAttributeName, StringComparison.OrdinalIgnoreCase))
                {
                    builder.AppendHtml(" ");
                    attribute.CopyTo(builder);
                }
                else
                {
                    addSrc = false;
                    AppendVersionedSrc(attribute.Name, src, attribute.ValueStyle, builder);
                }
            }

            if (addSrc)
            {
                AppendVersionedSrc(SrcAttributeName, src, HtmlAttributeValueStyle.DoubleQuotes, builder);
            }

            builder.AppendHtml("></script>");
        }
    }
}