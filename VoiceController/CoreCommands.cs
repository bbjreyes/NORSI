using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceController
{
    public static class CoreCommands
    {
        private static Dictionary<string,string> _commands;
        public static Dictionary<string, string> Commands { get { return _commands ?? null; } set { _commands = value; } }

        static CoreCommands()
        {
            _commands = new Dictionary<string, string>()
            {
                { "ShowParents", "Displays the first level keyword available without their child counterparts. Without also stating a child, no operation will be performed." },
                { "ShowParents -c", "Displays elements (children) against each grouping (parent). A parent keyword followed by a child's keyword constitutes a valid voice command."},
                { "GetDefault", "Shows the currently selected default parent." },
                { "SetDefault", "Sets the current parent, allowing you to omit the parent keyword in a voice command." },
                { "RemoveDefault", "Removes the currently set default parent." }
            };
        }

        internal static IEnumerable<string> EvaluateCommand(string response)
        {
            string temp = string.Empty;

            switch (response.ToLower())
            {
                case "showparents":
                    return KeywordFactory.GetParentNames();
                case "showparents -c":
                    return KeywordFactory.GetParentNames(true);
                case "getdefault":
                    return (!string.IsNullOrWhiteSpace(KeywordFactory.DefaultParent)) ? new string[]{ KeywordFactory.DefaultParent } : new string[] { "Not Set" };
                case "removedefault":
                    return new string[] { KeywordFactory.RemoveParent() };
                default:
                    return new string[] { "Command not found." };
            }
        }
    }
}
