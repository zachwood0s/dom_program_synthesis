using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TreeManipulation
{
    public class ProseAttribute
    {
        private string _name;
        private string _value;

        public string Name => _name;
        public string Value => _value;

        public ProseAttribute(string name, string value)
        {
            _name = name;
            _value = value;
        }

        public static ProseAttribute DeserializeFromHtmlAttribute(HtmlAttribute att)
        {
            var newAttr = new ProseAttribute(att.Name, att.Value);
            return newAttr;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProseAttribute other))
                return false;

            return _name.Equals(other._name) && _value.Equals(other._value);
        }
        public override int GetHashCode()
        {
            int hash = 13;
            hash = hash * 7 + _name.GetHashCode();
            hash = hash * 7 + _value.GetHashCode();
            return hash;
        }

    }

    public class ProseHtmlNode
    {
        private List<ProseHtmlNode> _childNodes;
        private Dictionary<string, ProseAttribute> _attributes;
        private string _name;

        public IReadOnlyList<ProseHtmlNode> ChildNodes => _childNodes;
        public IEnumerable<ProseAttribute> Attributes => _attributes.Values;
        public string Name => _name;

        public IEnumerable<ProseHtmlNode> Descendants 
            => _childNodes.RecursiveSelect(x => x.ChildNodes);

        public ProseAttribute this[string key]
            => _attributes.TryGetValue(key, out var val) ? val : null;

        public ProseHtmlNode(string name)
        {
            _name = name;
            _childNodes = new List<ProseHtmlNode>();
            _attributes = new Dictionary<string, ProseAttribute>();
        }


        public static ProseHtmlNode DeserializeFromHtmlNode(HtmlNode node)
        {
            var newNode = new ProseHtmlNode(node.Name);

            foreach(var attr in node.Attributes)
            {
                newNode._attributes[attr.Name] = ProseAttribute.DeserializeFromHtmlAttribute(attr);
            }

            var children = from c in node.ChildNodes
                           select DeserializeFromHtmlNode(c);

            newNode._childNodes.AddRange(children);

            return newNode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProseHtmlNode other))
                return false;

            if (Name != other.Name) return false;

            foreach(var attr in Attributes)
            {
                if (attr.Value != other[attr.Name]?.Value)
                    return false;
            }

            if (ChildNodes.Count != other.ChildNodes.Count)
                return false;

            foreach (var (childA, childB) in ChildNodes.Zip(other.ChildNodes, Tuple.Create))
            {
                if(!childA.Equals(childB))
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + _name.GetHashCode();
            foreach(var attr in Attributes)
            {
                hash = (hash * 7) + attr.GetHashCode();
            }

            foreach(var child in ChildNodes)
            {
                hash = (hash * 7) + child.GetHashCode();
            }
            return hash;
        }
    }
}
