using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VoiceController
{
    public static class KeywordFactory
    {
        public delegate void DefaultParentChangedEventHandler();
        public static event DefaultParentChangedEventHandler OnDefaultParentChanged;

        private static string _defaultParent;
        public static string DefaultParent
        {
            get
            {
                return _defaultParent;
            }
            set
            {
                _defaultParent = value;
                OnDefaultParentChanged?.Invoke();
            }
        }
        private static List<ParentKeyword> _parents;
        private static string[] _quantifiers;

        public static List<ParentKeyword> Parents
        {
            get { return _parents ?? null; }
            set { _parents = value; }
        }

        public static string[] Quantifiers
        {
            get { return _quantifiers ?? null; }
            set { _quantifiers = value; }
        }

        static KeywordFactory()
        {
            _defaultParent = string.Empty;

            LoadParentsAndChildren();

            _quantifiers = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        }

        private static void LoadParentsAndChildren()
        {
            Parents = new List<ParentKeyword>();
            XDocument doc = XDocument.Load($@"Data\KeywordData.xml");

            Parents = (from e in doc.Element("Parents")?.Descendants("Parent")
                       select new ParentKeyword()
                       {
                           Keyword = e?.Attribute("name")?.Value,
                           Children = (from c in e.Descendants("child")
                                       select new ChildKeyword()
                                       {
                                           Keyword = c?.Attribute("name")?.Value,
                                           KeySequence = c?.Attribute("sequence")?.Value
                                       }).ToList()
                       }).ToList();
        }

        public static string RemoveParent()
        {
            DefaultParent = string.Empty;
            return "Default keyword reset.";
        }

        public static IEnumerable<string> GetParentNames(bool withChildren = false)
        {
            if (!withChildren)
            {
                return (from p in Parents
                        select p.Keyword).ToList();
            }
            else
            {
                List<string> ParentsWithChildren = new List<string>();
                foreach (var p in Parents)
                {
                    ParentsWithChildren.Add(p.Keyword);
                    foreach (var c in p.Children)
                    {
                        ParentsWithChildren.Add("\t" + c.Keyword);
                    }
                }
                return ParentsWithChildren;
            }
        }
        public static List<ParentKeyword> GetParents()
        {
            return Parents;
        }
        public static ParentKeyword GetParent(string name)
        {
            return Parents.Where(p => p.Keyword == name).SingleOrDefault();
        }

        public static string[] GetQuantifiers()
        {
            return Quantifiers ?? null;
        }

        public static IEnumerable<string> GetChildrenNames(ParentKeyword p)
        {
            return (from c in p.Children
                    select c.Keyword).ToList();
        }
        public static IEnumerable<ChildKeyword> GetChildren(ParentKeyword p)
        {
            var children = (from parent in Parents
                            where parent == p
                            select parent.Children).FirstOrDefault();

            return children;
        }
    }
}

