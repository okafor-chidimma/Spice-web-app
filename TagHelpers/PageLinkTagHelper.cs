using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Spice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spice.TagHelpers
{
    [HtmlTargetElement("div", Attributes = "page-model")]
    public class PageLinkTagHelper : TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;

        //this exposes the urlHelper() used to build out urls
        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        //ViewContext attr ==> this tells the mvc that the property that has it as an attribute should be assihned the view context object value. what does this mean? this means that the property with name ViewContext will contain information as a view context object about the currently rendered page where the instance of the tag helper is used

        //HtmlAttributeNotBound ==> this tells mvc that the value of this property is not from attribute value assigned to this tag helper. what does this mean? this means that, normally when properties are declared in a tag helper class, its values are gotten form values passed to the attribute with same name as the property name but here this HtmlAttributeNotBound attribute is telling mvc that even if this taghelper has an attribute with same name, that it should not assign the value to this property

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        //from all this properties listed here, it means that when ever an instance of this tag class is instantiated, that it is expected that attributes listed below will be passed to it with values assigned to the attributes
        //the attributes are
        //1. page-model ==> which is going to be a PagingInfo object and its value assigned to PageModel property
        //2. page-action ==> which is going to be a string and its value assigned to the PageAction property
        //3. page-classes-enabled ==> which is going to be a boolean and its value assigned to the PageClassesEnabled property
        //4. page-class ==> which is going to be a string and its value assigned to the PageClass property
        //5. page-class-normal ==> which is going to be a string and its value assigned to the PageClassNormal property
        //6. page-class-selected ==> which is going to be a string and its value assigned to the PageClassSelected property
       
        public PagingInfo PageModel { get; set; }
        public string PageAction { get; set; }
        public bool PageClassesEnabled { get; set; }
        public string PageClass { get; set; }
        public string PageClassNormal { get; set; }
        public string PageClassSelected { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            //create an instance of urlHelper by passing the ViewContext to the GetUrlHelper
            //this urlHelper instance helps us in building out urls using the view context information
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);

            //i create a new div tag 
            TagBuilder result = new TagBuilder("div");

            for (int i = 1; i <= PageModel.totalPage; i++)
            {
                //create a new a tag
                TagBuilder tag = new TagBuilder("a");
                //replace : in the urlparam being passed as a paging info object property with i
                string url = PageModel.urlParam.Replace(":", i.ToString());
                //on my a tag, add a href attribute and assign its value to the url above
                tag.Attributes["href"] = url;
                if (PageClassesEnabled)
                {
                    tag.AddCssClass(PageClass);
                    tag.AddCssClass(i == PageModel.CurrentPage ? PageClassSelected : PageClassNormal);
                }
                //adds a text content to the a tag
                tag.InnerHtml.Append(i.ToString());
                //adds the a tag created to the div tag that was created above
                result.InnerHtml.AppendHtml(tag);
            }

            //appends what ever that is inside the created div which in our case is all the a tags created to the div target element
            output.Content.AppendHtml(result.InnerHtml);

        }

    }
}
