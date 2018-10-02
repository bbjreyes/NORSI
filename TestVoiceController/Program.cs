using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech;
using System.Speech.Recognition;
using VoiceController;

namespace TestVoiceController
{
    class Program
    {
        static void Main(string[] args)
        {
            SpeechRecognitionEngine sre = new SpeechRecognitionEngine();

            string[] wordsDefault = null;
            string setDefaultWord = "System";

            // Load parents into an object that can have its elements removed
            List<string> tempRootChoices = KeywordFactory.GetParentNames().ToList();
            if(tempRootChoices.Any(s => s == setDefaultWord))
            {
                tempRootChoices.Remove(setDefaultWord);

                wordsDefault = KeywordFactory.GetChildrenNames(
                    KeywordFactory.Parents
                    .Where(p => p.Keyword == setDefaultWord)
                    .Select(p => p)
                    .Single())
                    .ToArray();

                for (int i = 0; i < wordsDefault.Length; i++)
                {
                    tempRootChoices.Add(wordsDefault[i]);
                }
            }

            // Convert to array for Choices object
            string[] rootChoices = tempRootChoices.ToArray(); 

            Choices choices = new Choices(rootChoices);
            GrammarBuilder mainBuilder = new GrammarBuilder();
            mainBuilder.Append(choices);

            if (wordsDefault == null)
            {
                // This area loads the second level words
                string[] systemWords = KeywordFactory.GetChildrenNames((ParentKeyword)KeywordFactory.Parents[0]).ToArray();
                Choices systemChoices = new Choices(systemWords);
                GrammarBuilder subBuilder = systemChoices.ToGrammarBuilder();

                // This root level builder appends the words
                mainBuilder.Append(subBuilder);
            }
            
            Grammar grammar = new Grammar(mainBuilder);
            grammar.Name = "SystemGrammar";
            grammar.Enabled = true;

            sre.LoadGrammar(grammar);
            sre.SetInputToDefaultAudioDevice();
            sre.SpeechRecognized += Sre_SpeechRecognized;
            sre.RecognizeAsync(RecognizeMode.Multiple);

            Console.WriteLine(mainBuilder.DebugShowPhrases);
            Console.WriteLine("Go");
            Console.ReadLine();
        }

        private static void Sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine("Recognition made");
        }
    }
}
