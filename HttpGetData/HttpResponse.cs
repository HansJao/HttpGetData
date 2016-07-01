using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace HttpGetData
{
    public class HttpResponse
    {
        public static void RetrunXmlData(System.Web.UI.Page ContextPage, int StateCode, string StateMessage)
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Remove(0, strBuilder.Length);
            strBuilder.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            strBuilder.Append("<response>");
            strBuilder.Append("<StateCode><![CDATA[" + StateCode + "]]></StateCode>");
            strBuilder.Append("<StateMessage>" + StateMessage + "</StateMessage>");
            strBuilder.Append("</response>");

            ContextPage.Response.ContentType = "text/xml";
            ContextPage.Response.AppendHeader("Cache-Control", "no-cache");
            ContextPage.Response.Write(strBuilder.ToString());
            ContextPage.Response.Flush();
        }

        public static void RetrunJsonData(System.Web.UI.Page ContextPage, int StateCode, XmlDocument StateMessage)
        {

            XmlDocument xDoc = new XmlDocument();
            ContextPage.Response.ContentType = "text/json";
            ContextPage.Response.AppendHeader("Cache-Control", "no-cache");

            XmlElement eleResult = xDoc.CreateElement("response");
            xDoc.AppendChild(eleResult);

            eleResult = xDoc.CreateElement("StateCode");
            eleResult.InnerText = StateCode.ToString();
            xDoc.DocumentElement.AppendChild(eleResult);

            eleResult = xDoc.CreateElement("StateMessage");
            eleResult.InnerXml = StateMessage.OuterXml.Replace("<![CDATA[",string.Empty).Replace("]]>",string.Empty);
            xDoc.DocumentElement.AppendChild(eleResult);

            string responseJson = XmlToJSONSetContentArray(xDoc, new string[] { "Content" });

            ContextPage.Response.Write(responseJson);
            //ContextPage.Response.Write(json)
            ContextPage.Response.Flush();
        }
        private static IList ExcludeList;
        public static string XmlToJSONSetContentArray(XmlDocument xmlDoc, string[] RepeatNodeName)
        {
            StringBuilder sbJson = new StringBuilder();
            sbJson.Append("{ ");
            XmlToJSONnodeSetContentArray(sbJson, xmlDoc.DocumentElement, true, RepeatNodeName);
            sbJson.Append(" }");
            return sbJson.ToString();
        }

        private static void XmlToJSONnodeSetContentArray(StringBuilder sbJSON, XmlElement node, bool showNodeName, string[] RepeatNodeName)
        {
            if (showNodeName)
                sbJSON.Append("\"" + SafeJSON(node.Name) + "\": ");
            sbJSON.Append("{");
            // Build a sorted list of key-value pairs
            //  where   key is case-sensitive nodeName
            //          value is an ArrayList of string or XmlElement
            //  so that we know whether the nodeName is an array or not.
            SortedList childNodeNames = new SortedList();

            //  Add in all node attributes
            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes)
                    StoreChildNode(childNodeNames, attr.Name, attr.InnerText);

            //  Add in all nodes
            foreach (XmlNode cnode in node.ChildNodes)
            {
                if (cnode is XmlText)
                    StoreChildNode(childNodeNames, "value", cnode.InnerText);
                else if (cnode is XmlElement)
                    StoreChildNode(childNodeNames, cnode.Name, cnode);
            }

            // Now output all stored info
            foreach (string childname in childNodeNames.Keys)
            {
                //判別是否為重複的節點
                bool notRepeatNode = true;
                foreach (string repeatNode in RepeatNodeName)
                {
                    if (repeatNode.Equals(childname))
                    {
                        notRepeatNode = false;
                        break;
                    }
                }

                ArrayList alChild = (ArrayList)childNodeNames[childname];
                if (alChild.Count == 1 && notRepeatNode)
                {
                    OutputNodeSetContentArray(childname, alChild[0], sbJSON, true, RepeatNodeName);
                }
                else
                {
                    sbJSON.Append(" \"" + SafeJSON(childname) + "\": [ ");
                    foreach (object Child in alChild)
                        OutputNodeSetContentArray(childname, Child, sbJSON, false, RepeatNodeName);//解決陣列階層中含有陣列的情況
                    sbJSON.Remove(sbJSON.Length - 2, 2);
                    sbJSON.Append(" ], ");
                }
            }
            sbJSON.Remove(sbJSON.Length - 2, 2);
            sbJSON.Append(" }");
        }

        private static void OutputNodeSetContentArray(string childname, object alChild, StringBuilder sbJSON, bool showNodeName, string[] RepeatNodeName)
        {
            if (ExcludeList != null && ExcludeList.Contains(childname))
            {
                return;
            }

            if (alChild == null)
            {
                if (showNodeName)
                    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                sbJSON.Append("null");
            }
            else if (alChild is string)
            {
                if (showNodeName)
                    sbJSON.Append("\"" + SafeJSON(childname) + "\": ");
                string sChild = (string)alChild;
                sChild = sChild.Trim();
                sbJSON.Append("\"" + SafeJSON(sChild) + "\"");
            }
            else
                XmlToJSONnodeSetContentArray(sbJSON, (XmlElement)alChild, showNodeName, RepeatNodeName);
            sbJSON.Append(", ");
        }

        private static void StoreChildNode(SortedList childNodeNames, string nodeName, object nodeValue)
        {
            // Pre-process contraction of XmlElement-s
            if (nodeValue is XmlElement)
            {
                // Convert  <aa></aa> into "aa":null
                //          <aa>xx</aa> into "aa":"xx"
                XmlNode cnode = (XmlNode)nodeValue;
                if (cnode.Attributes.Count == 0)
                {
                    XmlNodeList children = cnode.ChildNodes;
                    if (children.Count == 0)
                        nodeValue = null;
                    else if (children.Count == 1 && (children[0] is XmlText))
                        nodeValue = ((XmlText)(children[0])).InnerText;
                }
            }
            // Add nodeValue to ArrayList associated with each nodeName
            // If nodeName doesn't exist then add it
            object oValuesAL = childNodeNames[nodeName];
            ArrayList ValuesAL;
            if (oValuesAL == null)
            {
                ValuesAL = new ArrayList();
                childNodeNames[nodeName] = ValuesAL;
            }
            else
                ValuesAL = (ArrayList)oValuesAL;
            ValuesAL.Add(nodeValue);
        }

        private static string SafeJSON(string sIn)
        {
            StringBuilder sbOut = new StringBuilder(sIn.Length);
            foreach (char ch in sIn)
            {
                if (Char.IsControl(ch) || ch == '\'')
                {
                    int ich = (int)ch;
                    sbOut.Append(@"\u" + ich.ToString("x4"));
                    continue;
                }
                else if (ch == '\"' || ch == '\\' || ch == '/')
                {
                    sbOut.Append('\\');
                }
                sbOut.Append(ch);
            }
            return sbOut.ToString();
        }

        public static string JsonToHtmlDecode(string strJson)
        {
            strJson = strJson.Replace("＆#", "&#")
                .Replace("＆＃", "&#")
                .Replace("&#34;", "\\\"")
                .Replace("&#92;", "\\\\")
                .Replace("&quot;", "\\\"");

            return System.Web.HttpUtility.HtmlDecode(strJson);
        }
    }
}