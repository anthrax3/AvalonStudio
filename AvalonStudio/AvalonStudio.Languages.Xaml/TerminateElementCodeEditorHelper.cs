﻿using Avalonia.Ide.CompletionEngine;
using Avalonia.Input;
using AvalonStudio.Editor;
using AvalonStudio.Extensibility.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvalonStudio.Languages.Xaml
{
    class TerminateElementCodeEditorHelper : ICodeEditorInputHelper
    {
        public void AfterTextInput(ILanguageService languageServivce, ITextDocument document, TextInputEventArgs args)
        {
            if (args.Text == "/")
            {
                var textBefore = document.Text.Substring(0, Math.Max(0, document.Caret - 1));
                if (textBefore.Length > 2 && textBefore[textBefore.Length - 1] != '/')
                {
                    var state = XmlParser.Parse(textBefore);
                    if (state.State == XmlParser.ParserState.InsideElement
                        || state.State == XmlParser.ParserState.StartElement
                        || state.State == XmlParser.ParserState.AfterAttributeValue)
                    {
                        var caret = document.Caret;
                        document.Replace(caret, 0, $">");
                        document.Caret = caret + 1;
                    }
                }
            }
        }

        public void BeforeTextInput(ILanguageService languageService, ITextDocument document, TextInputEventArgs args)
        {            
        }
    }

    class InsertQuotesForPropertyValueCodeEditorHelper : ICodeEditorInputHelper
    {
        public void AfterTextInput(ILanguageService languageServivce, ITextDocument document, TextInputEventArgs args)
        {
            if (args.Text == "=")
            {
                var textBefore = document.Text.Substring(0, Math.Max(0, document.Caret - 1));
                if (textBefore.Length > 2 && textBefore[textBefore.Length - 1] != '/')
                {
                    var state = XmlParser.Parse(textBefore);
                    if (state.State == XmlParser.ParserState.StartAttribute)
                    {
                        var caret = document.Caret;
                        document.Replace(caret, 0, "\"\" ");
                        document.Caret = caret + 1;
                    }
                }
            }
        }

        public void BeforeTextInput(ILanguageService languageService, ITextDocument document, TextInputEventArgs args)
        {
        }
    }

    class XamlIndentationCodeEditorHelper : ICodeEditorInputHelper
    {
        public void AfterTextInput(ILanguageService languageServivce, ITextDocument document, TextInputEventArgs args)
        {
            if (args.Text == "\n")
            {
                //Check if we are not inside a tag
                var textBefore = document.Text.Substring(0, Math.Max(0, document.Caret - 1));
                var state = XmlParser.Parse(textBefore);
                if (state.State == XmlParser.ParserState.None)
                {
                    //Find latest tag end
                    var idx = textBefore.LastIndexOf('>');
                    if (idx != -1)
                    {
                        state = XmlParser.Parse(textBefore.Substring(0, Math.Max(0, idx - 1)));
                        if (state.TagName.StartsWith('/'))
                        {
                            //TODO: find matching starting tag. XmlParser can't do that right now.
                            return;
                        }
                        //Find starting '<'
                        bool insideAttribute = false;
                        for (; idx >= 0; idx--)
                        {
                            var ch = textBefore[idx];
                            if (ch == '"')
                                insideAttribute = !insideAttribute;
                            if (ch == '<' && !insideAttribute)
                            {
                                var textBeforeTag = textBefore.Substring(0, idx);
                                var lineStartIdx = textBeforeTag.LastIndexOf('\n');
                                if (lineStartIdx != -1)
                                {
                                    //TODO: Do something about '\t' characters
                                    var prefixLength = (idx - lineStartIdx) - 1;
                                    document.Replace(document.Caret, 0,
                                        new string(' ', prefixLength));
                                }
                                return;
                            }
                        }
                    }
                }
            }
        }

        public void BeforeTextInput(ILanguageService languageService, ITextDocument document, TextInputEventArgs args)
        {
        }
    }
}