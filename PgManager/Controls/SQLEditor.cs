using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

using Wpf.Ui.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;

namespace PgManager.Controls
{
    public class SQLEditor : Wpf.Ui.Controls.RichTextBox
    {
        private bool _isUpdating = false;
        Popup AutoCompletePopup;
        ListBox SuggestionListBox;
        private readonly string[] sqlKeywords = new[]
        {
            "SELECT", "FROM", "WHERE","ORDER","BY", "INSERT", "INTO", "VALUES", "UPDATE", "DELETE", "CREATE",
            "TABLE", "DROP", "ALTER", "AND", "OR", "NOT", "NULL", "JOIN", "INNER", "LEFT", "RIGHT", "ON"
        };

        private readonly Regex stringRegex = new Regex(@"'[^']*'", RegexOptions.Compiled);
        private readonly Regex commentLineRegex = new Regex(@"--.*?$", RegexOptions.Multiline | RegexOptions.Compiled);
        private readonly Regex commentBlockRegex = new Regex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly Regex numberRegex = new Regex(@"\b\d+(\.\d+)?\b", RegexOptions.Compiled);


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            AutoCompletePopup = new Popup();
            SuggestionListBox = new ListBox();
            SuggestionListBox.MouseDoubleClick += SuggestionListBox_MouseDoubleClick;

            AutoCompletePopup.Child = SuggestionListBox;

            TextChanged += SQLEditor_TextChanged;
            PreviewKeyDown += SqlEditor_PreviewKeyDown;
        }

        private void SQLEditor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isUpdating) return;
            _isUpdating = true;

            var caretPos = this.CaretPosition;

            TextRange fullRange = new TextRange(this.Document.ContentStart, this.Document.ContentEnd);
            string text = fullRange.Text;
            fullRange.ClearAllProperties();


            foreach (var keyword in sqlKeywords)
            {
                foreach (Match match in Regex.Matches(text, $@"\b{Regex.Escape(keyword)}\b", RegexOptions.IgnoreCase))
                {
                    
                    ApplyFormat(match.Index, match.Length, StringToBrush("#2e95d3"), FontWeights.Bold);
                }
            }


            foreach (Match match in stringRegex.Matches(text))
                ApplyFormat(match.Index, match.Length, StringToBrush("#00a67d"));


            foreach (Match match in commentLineRegex.Matches(text))
                ApplyFormat(match.Index, match.Length, Brushes.Green);


            foreach (Match match in commentBlockRegex.Matches(text))
                ApplyFormat(match.Index, match.Length, Brushes.Green);


            foreach (Match match in numberRegex.Matches(text))
                ApplyFormat(match.Index, match.Length, StringToBrush("#df3079"));

            ShowAutoComplete();

            this.CaretPosition = caretPos;
            this.Focus();
            _isUpdating = false;
        }

  
        private void ApplyFormat(int startIndex, int length, Brush color, FontWeight? weight = null)
        {
            var start = GetTextPositionAtOffset(this.Document.ContentStart, startIndex);
            var end = GetTextPositionAtOffset(start, length);

            if (start != null && end != null)
            {
                var range = new TextRange(start, end);
                range.ApplyPropertyValue(TextElement.ForegroundProperty, color);
                if (weight.HasValue)
                    range.ApplyPropertyValue(TextElement.FontWeightProperty, weight.Value);
            }
        }

        private TextPointer GetTextPositionAtOffset(TextPointer start, int offset)
        {
            int count = 0;
            TextPointer current = start;

            while (current != null)
            {
                if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    string text = current.GetTextInRun(LogicalDirection.Forward);
                    if (count + text.Length >= offset)
                        return current.GetPositionAtOffset(offset - count);
                    count += text.Length;
                }
                current = current.GetNextContextPosition(LogicalDirection.Forward);
            }

            return null;
        }

        private void ShowAutoComplete()
        {
            var word = GetCurrentWord();

            if (string.IsNullOrWhiteSpace(word))
            {
                AutoCompletePopup.IsOpen = false;
                return;
            }

            var matches = sqlKeywords
                .Where(k => k.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                .OrderBy(k => k)
                .ToList();

            if (matches.Count == 0)
            {
                AutoCompletePopup.IsOpen = false;
                return;
            }

            SuggestionListBox.ItemsSource = matches;
            SuggestionListBox.SelectedIndex = 0;

            var caret = this.CaretPosition;
            var wordStart = GetWordStart(caret) ?? caret;

           
            Rect charRect = wordStart.GetCharacterRect(LogicalDirection.Forward);

            Point relativeToRTB = this.PointFromScreen(this.PointToScreen(new Point(charRect.X, charRect.Y)));

            AutoCompletePopup.Placement = PlacementMode.RelativePoint;
            AutoCompletePopup.PlacementTarget = this;
            AutoCompletePopup.HorizontalOffset = relativeToRTB.X;
            AutoCompletePopup.VerticalOffset = relativeToRTB.Y + charRect.Height + 5;

            AutoCompletePopup.IsOpen = true;
        }

        private string GetCurrentWord()
        {
            var caret = this.CaretPosition;
            var wordStart = GetWordStart(caret);
            var wordEnd = caret;

            if (wordStart == null || wordEnd == null) return "";

            var range = new TextRange(wordStart, wordEnd);
            return range.Text;
        }

        private TextPointer GetWordStart(TextPointer position)
        {
            while (position != null &&
                   position.GetPointerContext(LogicalDirection.Backward) == TextPointerContext.Text)
            {
                var text = position.GetTextInRun(LogicalDirection.Backward);
                if (string.IsNullOrWhiteSpace(text) || !char.IsLetterOrDigit(text.Last()))
                    break;
                position = position.GetPositionAtOffset(-1, LogicalDirection.Backward);
            }
            return position;
        }
        private void SqlEditor_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!AutoCompletePopup.IsOpen) return;

            if (e.Key == Key.Down)
            {
                SuggestionListBox.SelectedIndex = Math.Min(SuggestionListBox.SelectedIndex + 1, SuggestionListBox.Items.Count - 1);
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                SuggestionListBox.SelectedIndex = Math.Max(SuggestionListBox.SelectedIndex - 1, 0);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter || e.Key == Key.Tab)
            {
                CommitSuggestion();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                AutoCompletePopup.IsOpen = false;
            }
        }
        private void SuggestionListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            CommitSuggestion();
        }

        private void CommitSuggestion()
        {
            if (SuggestionListBox.SelectedItem is not string selected) return;

            var caret = this.CaretPosition;
            var wordStart = GetWordStart(caret);
            var wordEnd = caret;

            if (wordStart != null && wordEnd != null)
            {
                var range = new TextRange(wordStart, wordEnd);
                range.Text = selected;
                this.CaretPosition = range.End;
            }

            AutoCompletePopup.IsOpen = false;
        }

        SolidColorBrush StringToBrush(string hex)
        {
            return new BrushConverter().ConvertFromString(hex) as SolidColorBrush;
        }
    }
}
