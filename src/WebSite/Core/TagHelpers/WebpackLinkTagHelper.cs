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
    /// <see cref="ITagHelper"/> implementation targeting &lt;link&gt; elements that supports Webpack href paths.
    /// </summary>
    /// <remarks>
    /// The tag helper won't process for cases with just the 'href' attribute.
    /// </remarks>
    [HtmlTargetElement("link", Attributes = WebpackHrefAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("link", Attributes = AppendVersionAttributeName, TagStructure = TagStructure.WithoutEndTag)]
    public class WebpackLinkTagHelper : UrlResolutionTagHelper
    {
        private const string WebpackHrefAttributeName = "webpack-href";
        private const string AppendVersionAttributeName = "asp-append-version";
        private const string HrefAttributeName = "href";

        private IFileVersionProvider? _fileVersionProvider;
        private StringWriter? _stringWriter;

        private readonly DevelopmentSettings? devSettings;
        private readonly WebpackTagHelpersSettings? tagHelpersSettings;

        /// <summary>
        /// Creates a new <see cref="LinkTagHelper"/>.
        /// </summary>
        /// <param name="hostingEnvironment">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="cache">The <see cref="IMemoryCache"/>.</param>
        /// <param name="htmlEncoder">The <see cref="HtmlEncoder"/>.</param>
        /// <param name="urlHelperFactory">The <see cref="IUrlHelperFactory"/>.</param>
        /// <param name="devSettingsOptions">Development settings.</param>
        /// <param name="tagHelpersOptions">Webpack Tag Helpers settings.</param>
        public WebpackLinkTagHelper(
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
        /// Address of the linked resource.
        /// </summary>
        /// <remarks>
        /// Passed through to the generated HTML in all cases.
        /// </remarks>
        [HtmlAttributeName(HrefAttributeName)]
        public string? Href { get; set; }

        /// <summary>
        /// A Webpack-served CSS file to load.
        /// The CSS file is assessed relative to the application's 'webroot' setting.
        /// </summary>
        [HtmlAttributeName(WebpackHrefAttributeName)]
        public string? WebpackHref { get; set; }

        /// <summary>
        /// Value indicating if file version should be appended to the href urls.
        /// </summary>
        /// <remarks>
        /// If <c>true</c> then a query string "v" with the encoded content of the file is added.
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
            if (Href != null)
            {
                output.CopyHtmlAttribute(HrefAttributeName, context);
            }

            // If there's no "href" attribute in output.Attributes this will noop.
            ProcessUrlAttribute(HrefAttributeName, output);

            // Retrieve the TagHelperOutput variation of the "href" attribute in case other TagHelpers in the
            // pipeline have touched the value. If the value is already encoded this LinkTagHelper may
            // not function properly.
            Href = output.Attributes[HrefAttributeName]?.Value as string;

            if (ShouldAppendVersion())
            {
                EnsureFileVersionProvider();

                if (Href != null)
                {
                    var index = output.Attributes.IndexOfName(HrefAttributeName);
                    var existingAttribute = output.Attributes[index];
                    output.Attributes[index] = new TagHelperAttribute(
                        existingAttribute.Name,
                        _fileVersionProvider?.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, Href),
                        existingAttribute.ValueStyle);
                }
            }

            var builder = output.PostElement;
            builder.Clear();

            if (!string.IsNullOrEmpty(WebpackHref))
            {
                BuildWebpackLinkTag(output.Attributes, builder);

                if (string.IsNullOrEmpty(Href))
                {
                    // Only HrefInclude is specified. Don't render the original tag.
                    output.TagName = null;
                    output.Content.SetHtmlContent(HtmlString.Empty);
                }
            }
        }

        private void BuildWebpackLinkTag(TagHelperAttributeList attributes, TagHelperContent builder)
        {
            // Don't build duplicate link tag for the original href url.
            if (string.Equals(Href, WebpackHref, StringComparison.OrdinalIgnoreCase))
                return;

            var webpackHref = HostingEnvironment.IsDevelopment() ? string.Format("//localhost:{0}{1}", devSettings?.WebpackDevServerPort, WebpackHref) : WebpackHref;

            BuildLinkTag(webpackHref, attributes, builder);
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

            if (tagHelpersSettings == null)
                return false;

            return AppendVersion == true || tagHelpersSettings.AutoAppendFileVersions;
        }

        private void BuildLinkTag(string? href, TagHelperAttributeList attributes, TagHelperContent builder)
        {
            builder.AppendHtml("<link ");

            var addHref = true;

            // Perf: Avoid allocating enumerator
            for (var i = 0; i < attributes.Count; i++)
            {
                var attribute = attributes[i];

                if (string.Equals(attribute.Name, HrefAttributeName, StringComparison.OrdinalIgnoreCase))
                {
                    addHref = false;

                    AppendVersionedHref(attribute.Name, href, builder);
                }
                else
                {
                    attribute.CopyTo(builder);
                    builder.AppendHtml(" ");
                }
            }

            if (addHref)
            {
                AppendVersionedHref(HrefAttributeName, href, builder);
            }

            builder.AppendHtml("/>");
        }

        private void AppendVersionedHref(string hrefName, string? hrefValue, TagHelperContent builder)
        {
            if (ShouldAppendVersion() && !HostingEnvironment.IsDevelopment())
            {
                hrefValue = _fileVersionProvider?.AddFileVersionToPath(ViewContext.HttpContext.Request.PathBase, hrefValue);
            }

            builder
                .AppendHtml(hrefName)
                .AppendHtml("=\"")
                .Append(hrefValue)
                .AppendHtml("\" ");
        }
    }
}