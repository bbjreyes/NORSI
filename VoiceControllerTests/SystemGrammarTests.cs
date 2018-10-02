using System;
using System.Collections.Generic;
using System.Speech.Recognition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VoiceController;

namespace VoiceControllerTests
{
    [TestClass]
    public class SystemGrammarTests
    {
        [TestMethod]
        public void SystemAsDefaultGrammar()
        {
            // Preparation
            SpeechRecognitionEngine sre = new SpeechRecognitionEngine();
            GrammarBuilder builder = new GrammarBuilder("System");
            Choices choices = new Choices(KeywordFactory.GetParentNames() as string[]);

            builder.Append(choices);

            Grammar grammar = new Grammar((GrammarBuilder)builder);
            grammar.Name = "SystemGrammar";
            grammar.Enabled = true;

            System.Diagnostics.Debug.Print(builder.DebugShowPhrases.Length.ToString());

            //sre.SetInputToDefaultAudioDevice();
            //sre.LoadGrammar(grammar);
            //sre.RecognizeAsync();
        }
    }
}
