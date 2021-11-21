using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebSynthesis.TreeManipulation
{
    public struct ProseAttribute
    {
        private string _name;
        private string _value;

        public string Name => _name;
        public string Value => _value;
        private int? _cachedHashCode;

        public ProseAttribute(string name, string value)
        {
            _name = name;
            _value = value;
            _cachedHashCode = null;
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
            if(!_cachedHashCode.HasValue)
            {
                int hash = 13;
                hash = hash * 7 + _name.GetHashCode();
                hash = hash * 7 + _value.GetHashCode();
                _cachedHashCode = hash;
            }
            return _cachedHashCode.Value;
        }

        public override string ToString()
        {
            return $"{_name}: {_value}";
        }

    }

    public class ProseHtmlNode
    {
        private ProseHtmlNode _parent;
        private List<ProseHtmlNode> _childNodes;
        private Dictionary<string, ProseAttribute> _attributes;
        private string _name;
        private HtmlNodeType _type;
        private string _text;
        private int _line;
        private int _col;
        private int? _cachedHashCode;
        private List<ProseHtmlNode> _cachedDescendants;

        public IReadOnlyList<ProseHtmlNode> ChildNodes => _childNodes;
        public IEnumerable<ProseAttribute> Attributes => _attributes.Values;

        public string Name
        {
            get
            {
                return _name;
            }

            internal set
            {
                _name = value;
            }
        }
        public HtmlNodeType Type => _type;
        public string Text
        {
            get
            {
                return _text;
            }
            
            set
            {
                _text = value;
            }
        }


        public IEnumerable<ProseHtmlNode> Descendants
        {
            get
            {
                if (_cachedDescendants == null)
                {
                    _cachedDescendants = _childNodes.RecursiveSelect(x => x.ChildNodes).ToList();
                }
                return _cachedDescendants;
            }
        }

        public ProseAttribute? this[string key]
            => _attributes.TryGetValue(key, out var val) ? val : (ProseAttribute?) null;

        public ProseHtmlNode(string name)
        {
            _name = name;
            _childNodes = new List<ProseHtmlNode>();
            _attributes = new Dictionary<string, ProseAttribute>();
        }


        public static ProseHtmlNode DeserializeFromHtmlNode(HtmlNode node)
        {
            var newNode = DeserializeFromHtmlNode(node, null);

            newNode.RemoveDuplicates();
            newNode.Simplify();
            return newNode;
        }

        private static ProseHtmlNode DeserializeFromHtmlNode(HtmlNode node, ProseHtmlNode parent)
        {
            if(node.NodeType == HtmlNodeType.Comment)
            {
                return null;
            }

            var newNode = new ProseHtmlNode(node.Name)
            {
                _type = node.NodeType,
                _parent = parent
            };

            if(node is HtmlTextNode textNode)
            {
                newNode._text = textNode.Text;
            }

            foreach (var attr in node.Attributes)
            {
                newNode._attributes[attr.Name] = ProseAttribute.DeserializeFromHtmlAttribute(attr);
            }

            var children = from c in node.ChildNodes
                           where c.NodeType != HtmlNodeType.Comment
                           select DeserializeFromHtmlNode(c, newNode);

            newNode._childNodes.AddRange(children);

            newNode._line = node.Line;
            newNode._col = node.LinePosition;

            return newNode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ProseHtmlNode other))
                return false;

            // If the hash codes aren't the same, then immediately exit.
            // Hashcode calculation is quicker and is cached
            if (GetHashCode() != other.GetHashCode())
                return false;

            if (Name != other.Name) return false;

            if (Text != null && !Text.Equals(other.Text))
                return false;

            if (Attributes.Count() != other.Attributes.Count())
                return false;

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
            if (!_cachedHashCode.HasValue)
            { 
                int hash = 13;
                hash = (hash * 7) + _name.GetHashCode();
                foreach (var attr in Attributes)
                {
                    hash = (hash * 7) + attr.GetHashCode();
                }

                foreach (var child in ChildNodes)
                {
                    hash = (hash * 7) + child.GetHashCode();
                }

                if (_text != null)
                    hash = (hash * 7) + _text.GetHashCode();
                _cachedHashCode = hash;
            }

            return _cachedHashCode.Value;
        }

        public override string ToString()
        {
            return $"{Name}: Ln {_line} Col {_col}";
        }

        public void RemoveDuplicates()
        {
            var allNodes = Descendants.ToList();
            var seen = new HashSet<ProseHtmlNode>();
            var count = 0;

            foreach(var n in allNodes)
            {
                if(seen.Contains(n))
                {
                    if(n._parent != null)
                    {
                        n._parent._childNodes.Remove(n);
                        count++;
                    }
                    else
                    {
                        throw new Exception("Duplicate node has no parent");
                    }
                }
                else
                {
                    seen.Add(n);
                }
            }
            Console.WriteLine($"Removed {count} duplicates");
        }

        public void Simplify()
        {
            Traverse(x =>
            {
                // Pull the children text into the parent if the only child is just a text node
                if(x.ChildNodes.Count == 1 && x.ChildNodes[0].Name == "#text")
                {
                    x.Text = x.ChildNodes[0].Text;
                    x._childNodes.RemoveAt(0);
                    x._cachedHashCode = null;
                }
            });
        }

        public ProseHtmlNode DeepCopy()
        {
            var newNode = new ProseHtmlNode(_name)
            {
                _attributes = new Dictionary<string, ProseAttribute>(_attributes),
                _childNodes = _childNodes.Select(x => x.DeepCopy()).ToList(),
                _type = _type,
                _text = _text,
                //_line = _line,
                //_col = _col,
                _parent = null
            };
            foreach(var c in newNode.ChildNodes)
            {
                c._parent = newNode;
            }
            return newNode;
        }

        public void Traverse(Action<ProseHtmlNode> action)
        {
            action(this);
            _childNodes.ForEach(x => x.Traverse(action));
        }

        public void RandomlyOrderChildren()
        {
            var r = new Random();
            _childNodes = _childNodes.OrderBy(_ => r.NextDouble()).ToList();
        }
    }
}
