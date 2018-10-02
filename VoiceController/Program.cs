using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Windows.Forms;
using System.Xml.Linq;

namespace VoiceController
{
    class Program
    {
        static SpeechRecognitionEngine sre;
        string defaultParent = string.Empty;


        static void Main(string[] args)
        {
            
            KeywordFactory.OnDefaultParentChanged += KeywordFactory_OnDefaultParentChanged;
            LoadGrammar();

            PrintLogo();
            Console.WriteLine("\nWelcome to NORSI. If you are new here, type 'help' and hit enter to learn what you can do.\n");
            Console.Write("> ");

            string response = string.Empty;
            response = Console.ReadLine();

            while (!response.Equals("exit", StringComparison.CurrentCultureIgnoreCase))
            {
                Console.WriteLine(EvaluateResponse(response));
                Console.Write("> ");
                response = Console.ReadLine();
            };
            
        }

        private static void KeywordFactory_OnDefaultParentChanged()
        {
            LoadGrammar();
        }

        private static void LoadGrammar()
        {
            if(sre != null && sre.Grammars.Count > 0)
            {
                sre.RecognizeAsyncStop();
                sre.UnloadAllGrammars();
            }

            string setDefaultWord = KeywordFactory.DefaultParent;
            string[] defaultSubChoices = null;
            string[] rootChoices = new string[KeywordFactory.Parents.Count];
            GrammarBuilder[] gbAll = new GrammarBuilder[KeywordFactory.Parents.Count];
            Choices choices = null;


            for (int p = 0; p < KeywordFactory.Parents.Count; p++)
            {
                string[] subChoices = null;
                Choices choicesSub = null;
                bool defaultExists = false;

                rootChoices[p] = KeywordFactory.Parents[p].Keyword;

                if (rootChoices[p] == setDefaultWord)
                {
                    defaultExists = true;
                    defaultSubChoices = KeywordFactory.GetChildrenNames(
                                        KeywordFactory.Parents
                                        .Where(parent => parent.Keyword == setDefaultWord)
                                        .Select(parent => parent)
                                        .Single())
                                        .ToArray();
                }
                else
                {
                    choices = new Choices(rootChoices[p]);
                    subChoices = new string[KeywordFactory.Parents[p].Children.Count];

                    for (int c = 0; c < KeywordFactory.Parents[p].Children.Count; c++)
                    {
                        subChoices[c] = KeywordFactory.Parents[p].Children[c].Keyword;
                    }
                    choicesSub = new Choices(subChoices);
                }

                // Create grammar builder
                if (!defaultExists)
                {
                    gbAll[p] = new GrammarBuilder(rootChoices[p])
                    {
                        Culture = new System.Globalization.CultureInfo("en-GB")
                    };


                    gbAll[p].Append(choicesSub);
                }
                else
                {
                    gbAll[p] = new GrammarBuilder(new Choices(defaultSubChoices))
                    {
                        Culture = new System.Globalization.CultureInfo("en-GB")
                    };
                }

                // New - testing for quantity
                string[] quantifiers = KeywordFactory.GetQuantifiers();
                Choices quantiferChoices = new Choices(quantifiers);

                gbAll[p].Append(quantiferChoices, 0, 1);
                // End
            }

            Choices allChoices = new Choices();
            allChoices.Add(gbAll);

            Grammar gram = new Grammar(allChoices)
            {
                Name = "Actions",
                Enabled = true
            };

            try
            {

                sre = new SpeechRecognitionEngine();
                sre.SetInputToDefaultAudioDevice();

                sre.LoadGrammar(gram);
                gram.SpeechRecognized += OnSpeechRecognized;

                sre.RecognizeAsync(RecognizeMode.Multiple);

                Console.WriteLine("*** Recognition Engine Ready! ***\n");
            }
            catch(Exception e)
            {
                Console.WriteLine($"Uh oh:\n\n {e.Message}");
            }
        }

        private static string EvaluateResponse(string response)
        {
            string temp = string.Empty;
            response = response.ToLower();

            if (response == "help")
            {
                foreach (string s in CoreCommands.Commands.Keys)
                {
                    temp += "- " + s + "\n\t" + CoreCommands.Commands[s] + "\n";
                }
                return temp;
            }
            else if (CoreCommands.Commands.Keys.Contains(response, StringComparer.CurrentCultureIgnoreCase))
            {
                if (response != "setdefault")
                {
                    IEnumerable<string> result = CoreCommands.EvaluateCommand(response);
                    foreach (var i in result)
                    {
                        temp += i + "\n";
                    }
                    return temp;
                }
                else
                {
                    Console.WriteLine("Which parent would you like to set as default? (Choices are case sensitive.)");

                    foreach (var p in KeywordFactory.Parents)
                    {
                        Console.WriteLine("> -- " + p.Keyword);
                    }

                    Console.Write("> ");
                    string parent = Console.ReadLine();

                    if (KeywordFactory.Parents.Exists(p => p.Keyword == parent))
                    {
                        KeywordFactory.DefaultParent = parent;
                        return "Default parent set successfully.";
                    }
                    else
                    {
                        return "Parent not found.";
                    }
                }
            }
            return "Command not found.";
        }


        private static void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            List<ChildKeyword> children = null;
            int childIndex = -1;
            ParentKeyword parent = null;
            bool defaultParentIsUsed = !(KeywordFactory.GetParentNames().Any(s => s == e.Result.Words[0].Text));

            if (!string.IsNullOrWhiteSpace(KeywordFactory.DefaultParent) && defaultParentIsUsed)
            {
                parent = KeywordFactory.GetParent(KeywordFactory.DefaultParent);
            }
            else
            {
                parent = KeywordFactory.GetParent(e.Result.Words[0].Text);
            }

            if (parent != null)
            {
                children = KeywordFactory.GetChildren(parent).ToList();
                string wordToCheck = GetChildKeywordToCheck(e, !string.IsNullOrEmpty(KeywordFactory.DefaultParent) &&
                                                                    defaultParentIsUsed);

                childIndex = children.FindIndex(c => c.Keyword == wordToCheck);
            }

            if (children != null && childIndex > -1)
            {
                string sequence = children[childIndex].KeySequence;

                int result = 0;
                if (int.TryParse(e.Result.Words[e.Result.Words.Count - 1].Text, out result))
                {
                    int index = sequence.IndexOf('}');
                    sequence = sequence.Insert(index, " " + result.ToString());
                }
                SendKeys.SendWait(sequence);
            }
        }

        private static string GetChildKeywordToCheck(SpeechRecognizedEventArgs e, bool accountForDefault)
        {
            string wordToCheck = string.Empty;
            int startIndex = accountForDefault ? 0 : 1;

            for (int c = startIndex; c < e.Result.Words.Count; c++)
            {
                if(!e.Result.Words[c].Text.Any(char.IsDigit))
                   wordToCheck += e.Result.Words[c].Text + " ";
            }
            wordToCheck = wordToCheck.Trim();
            return wordToCheck;
        }

        private static void PrintLogo()
        {
            Console.WriteLine("===============================================================================================");
            Console.Write("||\\    ||  ||||||  ||||||||  ||||||  ||"); Console.Write("                  //\n");
            Console.Write("|| \\   || ||    || ||    || ||       ||"); Console.Write("     =======xxx=== ----\n");
            Console.Write("||  \\  || ||    || || ||||   ||||||  ||"); Console.Write("                   ------\n");
            Console.Write("||   \\ || ||    || ||  ||         || ||"); Console.Write("                   -----\n");
            Console.Write("||    \\||  ||||||  ||    ||  ||||||  ||"); Console.Write("     ============= ---\n");
            Console.WriteLine("===============================================================================================");
        }
    }
}
