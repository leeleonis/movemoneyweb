using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace BitcoinTransactionManagement.Helpers
{
    public static class MyHelpers
    {
        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo)
        {
            return htmlHelper.CheckBoxList(name, listInfo, null);
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo, object htmlAttributes)
        {
            return htmlHelper.CheckBoxList(name, listInfo, new RouteValueDictionary(htmlAttributes));
        }

        public static MvcHtmlString CheckBoxList(this HtmlHelper htmlHelper, string name, List<CheckBoxListInfo> listInfo, IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (listInfo == null)
                throw new ArgumentNullException("listInfo");
            if (listInfo.Count < 1)
                throw new ArgumentException("The list must contain at least one value", "listInfo");

            StringBuilder sb = new StringBuilder();

            foreach (CheckBoxListInfo info in listInfo)
            {
                TagBuilder input = new TagBuilder("input");
                if (info.IsChecked) input.MergeAttribute("checked", "checked");
                input.MergeAttributes<string, object>(htmlAttributes);
                input.MergeAttribute("type", "checkbox");
                input.MergeAttribute("value", info.Value);
                input.MergeAttribute("name", name);
                input.InnerHtml = info.DisplayText;

                TagBuilder builder = new TagBuilder("label")
                {
                    InnerHtml = input.ToString(TagRenderMode.Normal)
                };
                builder.MergeAttribute("class", "checkbox-inline");
                sb.Append(builder.ToString(TagRenderMode.Normal));
            }
            return MvcHtmlString.Create(sb.ToString());
        }

    }

    public class CheckBoxListInfo
    {
        public CheckBoxListInfo(string Value, string DisplayText, bool IsChecked)
        {
            this.Value = Value;
            this.DisplayText = DisplayText;
            this.IsChecked = IsChecked;
        }

        public string Value { get; set; }
        public string DisplayText { get; set; }
        public bool IsChecked { get; set; }
    }
}