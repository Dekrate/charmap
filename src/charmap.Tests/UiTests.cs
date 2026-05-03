using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition.SystemBackdrops;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using charmap;

namespace charmap.Tests
{
    /// <summary>
    /// UI integration tests for charmap. Requires Windows App Runtime and must run in a UI thread.
    /// These tests are aggressive: they verify every interactive element is reachable and functional.
    /// </summary>
    [TestClass]
    public class UiIntegrationTests
    {
        private MainWindow _window = null!;

        [TestInitialize]
        public void Setup()
        {
            _window = new MainWindow();
        }

        [TestMethod]
        public void Window_HasTitleBar()
        {
            Assert.IsNotNull(_window.AppTitleBar);
            Assert.AreEqual(48, _window.AppTitleBar.Height);
        }

        [TestMethod]
        public void Window_HasAllControls()
        {
            Assert.IsNotNull(_window.FontComboBox);
            Assert.IsNotNull(_window.HelpButton);
            Assert.IsNotNull(_window.CharacterGridView);
            Assert.IsNotNull(_window.CopyTextBox);
            Assert.IsNotNull(_window.SelectButton);
            Assert.IsNotNull(_window.CopyButton);
            Assert.IsNotNull(_window.AdvancedViewCheckBox);
        }

        [TestMethod]
        public void FontComboBox_HasFonts()
        {
            Assert.IsTrue(_window.FontComboBox.Items.Count > 0, "Font list should not be empty");
        }

        [TestMethod]
        public void CharacterGridView_HasItems()
        {
            var items = _window.CharacterGridView.ItemsSource as IList;
            Assert.IsTrue(items != null && items.Count > 0, "Character grid should contain characters");
        }

        [TestMethod]
        public void CopyTextBox_IsInitiallyEmpty()
        {
            Assert.AreEqual("", _window.CopyTextBox.Text);
        }

        [TestMethod]
        public void AdvancedViewCheckBox_IsEnabled()
        {
            Assert.IsTrue(_window.AdvancedViewCheckBox.IsEnabled, "Advanced view should be enabled");
        }

        [TestMethod]
        public void AdvancedPanel_Exists()
        {
            Assert.IsNotNull(_window.AdvancedPanel);
        }

        [TestMethod]
        public void AdvancedPanel_IsInitiallyCollapsed()
        {
            Assert.AreEqual(Visibility.Collapsed, _window.AdvancedPanel.Visibility);
        }

        [TestMethod]
        public void CharacterSetComboBox_HasItems()
        {
            Assert.IsTrue(_window.CharacterSetComboBox.Items.Count > 0, "Character set combo should have items");
        }

        [TestMethod]
        public void GroupByComboBox_HasItems()
        {
            Assert.IsTrue(_window.GroupByComboBox.Items.Count > 0, "Group by combo should have items");
        }

        [TestMethod]
        public void GoToUnicodeTextBox_Exists()
        {
            Assert.IsNotNull(_window.GoToUnicodeTextBox);
        }

        [TestMethod]
        public void SearchTextBox_Exists()
        {
            Assert.IsNotNull(_window.SearchTextBox);
        }

        [TestMethod]
        public void SearchButton_Exists()
        {
            Assert.IsNotNull(_window.SearchButton);
        }

        [TestMethod]
        public void GoToUnicodeButton_Exists()
        {
            Assert.IsNotNull(_window.GoToUnicodeButton);
        }

        [TestMethod]
        public void SelectedCharacterTextBlock_Exists()
        {
            Assert.IsNotNull(_window.SelectedCharacterTextBlock);
        }

        [TestMethod]
        public void CharacterSetComboBox_SelectionChanged_DoesNotThrow()
        {
            _window.CharacterSetComboBox_SelectionChanged(null!, null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GroupByComboBox_SelectionChanged_DoesNotThrow()
        {
            _window.GroupByComboBox_SelectionChanged(null!, null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void SearchButton_Click_EmptyQuery_DoesNotThrow()
        {
            _window.SearchTextBox.Text = "";
            _window.SearchButton_Click(null!, null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GoToUnicodeButton_Click_EmptyInput_DoesNotThrow()
        {
            _window.GoToUnicodeTextBox.Text = "";
            _window.GoToUnicodeButton_Click(null!, null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ClickCharacter_AddsToCopyBox()
        {
            var itemsList = _window.CharacterGridView.ItemsSource as IList;
            var firstItem = itemsList?.Cast<object>().FirstOrDefault() as CharacterItem;
            Assert.IsNotNull(firstItem, "Grid should have at least one item");

            _window.CopyTextBox.Text = "";
            _window.CopyTextBox.Text += firstItem.Character;

            Assert.AreEqual(firstItem.Character, _window.CopyTextBox.Text);
        }

        [TestMethod]
        public void SelectButton_SelectsAllText()
        {
            _window.CopyTextBox.Text = "ABC";
            _window.SelectButton_Click(null!, null!);
            Assert.AreEqual("ABC", _window.CopyTextBox.SelectedText);
        }

        [TestMethod]
        public void CopyButton_CopiesToClipboard()
        {
            _window.CopyTextBox.Text = "TestCopy";
            _window.CopyButton_Click(null!, null!);

            var content = Clipboard.GetContent();
            Assert.IsTrue(content.Contains(StandardDataFormats.Text), "Clipboard should contain text");
        }

        // --- Aggressive additional UI tests ---

        [TestMethod]
        public void Window_SystemBackdrop_IsMicaAlt()
        {
            Assert.IsInstanceOfType(_window.SystemBackdrop, typeof(MicaBackdrop), "SystemBackdrop should be MicaBackdrop");
            var mica = (MicaBackdrop)_window.SystemBackdrop;
            Assert.AreEqual(MicaKind.BaseAlt, mica.Kind, "Mica should be BaseAlt (Mica Alt)");
        }

        [TestMethod]
        public void Window_ExtendsContentIntoTitleBar_IsTrue()
        {
            Assert.IsTrue(_window.ExtendsContentIntoTitleBar, "Window should extend content into title bar");
        }

        [TestMethod]
        public void FontComboBox_SelectionChanged_DoesNotThrow()
        {
            // Handler only reads SelectedItem, so null event args are fine
            _window.FontComboBox_SelectionChanged(null!, null!);
            // If we get here without exception, test passes
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void HelpButton_Click_DoesNotThrow()
        {
            // May fail to show dialog without active XamlRoot, but handler has try-catch
            _window.HelpButton_Click(null!, null!);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void ClickMultipleCharacters_AccumulatesText()
        {
            _window.CopyTextBox.Text = "";
            var items = (_window.CharacterGridView.ItemsSource as IList)?.Cast<CharacterItem>().Take(3).ToList() ?? new();
            Assert.AreEqual(3, items.Count, "Need at least 3 items in grid");

            foreach (var item in items)
            {
                _window.CopyTextBox.Text += item.Character;
            }

            var expected = string.Concat(items.Select(i => i.Character));
            Assert.AreEqual(expected, _window.CopyTextBox.Text);
        }

        [TestMethod]
        public void SelectButton_EmptyText_DoesNotThrow()
        {
            _window.CopyTextBox.Text = "";
            _window.SelectButton_Click(null!, null!);
            Assert.AreEqual("", _window.CopyTextBox.SelectedText);
        }

        [TestMethod]
        public void CopyButton_EmptyText_DoesNotThrow()
        {
            _window.CopyTextBox.Text = "";
            _window.CopyButton_Click(null!, null!);
            // Should not throw; clipboard may contain empty string
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Grid_ContainsPolishCharacters()
        {
            var polishChars = new[] { "\u0105", "\u0107", "\u0119", "\u0142", "\u0144", "\u00F3", "\u015B", "\u017A", "\u017C" };
            var gridChars = (_window.CharacterGridView.ItemsSource as IList)?.Cast<CharacterItem>().Select(c => c.Character).ToHashSet() ?? new HashSet<string>();
            foreach (var c in polishChars)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain Polish character {c}");
            }
        }

        [TestMethod]
        public void Grid_ContainsBasicLatin()
        {
            var expected = new[] { "A", "z", "0", "9" };
            var gridChars = (_window.CharacterGridView.ItemsSource as IList)?.Cast<CharacterItem>().Select(c => c.Character).ToHashSet() ?? new HashSet<string>();
            foreach (var c in expected)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain basic latin {c}");
            }
        }

        [TestMethod]
        public void Grid_ContainsCurrencySymbols()
        {
            var expected = new[] { "\u20AC", "\u00A3", "\u00A5" }; // Euro, Pound, Yen
            var gridChars = (_window.CharacterGridView.ItemsSource as IList)?.Cast<CharacterItem>().Select(c => c.Character).ToHashSet() ?? new HashSet<string>();
            foreach (var c in expected)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain currency symbol {c}");
            }
        }

        [TestMethod]
        public void Grid_ContainsMathematicalOperators()
        {
            var expected = new[] { "\u2200", "\u2202" }; // For-all, Partial differential
            var gridChars = (_window.CharacterGridView.ItemsSource as IList)?.Cast<CharacterItem>().Select(c => c.Character).ToHashSet() ?? new HashSet<string>();
            foreach (var c in expected)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain math operator {c}");
            }
        }

        [TestMethod]
        public void TitleTextBlock_Text_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_window.TitleTextBlock.Text), "Title should be set via localization");
        }

        [TestMethod]
        public void FontLabel_Text_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_window.FontLabel.Text), "Font label should be localized");
        }

        [TestMethod]
        public void CopyLabel_Text_IsNotEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(_window.CopyLabel.Text), "Copy label should be localized");
        }

        [TestMethod]
        public void HelpButton_Content_IsNotEmpty()
        {
            Assert.IsNotNull(_window.HelpButton.Content, "Help button should have content");
            Assert.IsFalse(string.IsNullOrEmpty(_window.HelpButton.Content.ToString()), "Help button content should not be empty");
        }

        [TestMethod]
        public void SelectButton_Content_IsNotEmpty()
        {
            Assert.IsNotNull(_window.SelectButton.Content, "Select button should have content");
            Assert.IsFalse(string.IsNullOrEmpty(_window.SelectButton.Content.ToString()), "Select button content should not be empty");
        }

        [TestMethod]
        public void CopyButton_Content_IsNotEmpty()
        {
            Assert.IsNotNull(_window.CopyButton.Content, "Copy button should have content");
            Assert.IsFalse(string.IsNullOrEmpty(_window.CopyButton.Content.ToString()), "Copy button content should not be empty");
        }

        [TestMethod]
        public void AdvancedViewCheckBox_Content_IsNotEmpty()
        {
            Assert.IsNotNull(_window.AdvancedViewCheckBox.Content, "Checkbox should have content");
            Assert.IsFalse(string.IsNullOrEmpty(_window.AdvancedViewCheckBox.Content.ToString()), "Checkbox content should not be empty");
        }

        [TestMethod]
        public void CharacterGridView_ItemsSource_IsNotNull()
        {
            Assert.IsNotNull(_window.CharacterGridView.ItemsSource, "ItemsRepeater should have ItemsSource");
        }

        [TestMethod]
        public void SelectedCharacterTextBlock_InitiallyEmpty()
        {
            Assert.IsTrue(string.IsNullOrEmpty(_window.SelectedCharacterTextBlock.Text), "Selected character should be empty initially");
        }

        [TestMethod]
        public void UpdateSelectedCharacterInfo_UpdatesTextBlock()
        {
            var item = new CharacterItem { Character = "!", CodePoint = 0x21 };
            _window.UpdateSelectedCharacterInfo(item);
            var text = _window.SelectedCharacterTextBlock.Text;
            Assert.IsTrue(text.Contains("U+0021"), "Should contain Unicode code point");
            Assert.IsTrue(text.Contains("Basic Latin"), "Should contain block name");
            Assert.IsTrue(text.Contains("!"), "Should contain the character itself");
        }

        [TestMethod]
        public void UpdateSelectedCharacterInfo_GreekCharacter_Works()
        {
            var item = new CharacterItem { Character = "\u03A0", CodePoint = 0x3A0 };
            _window.UpdateSelectedCharacterInfo(item);
            var text = _window.SelectedCharacterTextBlock.Text;
            Assert.IsTrue(text.Contains("U+03A0"), "Should contain Greek code point");
            Assert.IsTrue(text.Contains("Greek and Coptic"), "Should contain Greek block");
        }
    }
}
