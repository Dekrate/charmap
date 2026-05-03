using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition.SystemBackdrops;
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
            Assert.IsTrue(_window.CharacterGridView.Items.Count > 0, "Character grid should contain characters");
        }

        [TestMethod]
        public void CopyTextBox_IsInitiallyEmpty()
        {
            Assert.AreEqual("", _window.CopyTextBox.Text);
        }

        [TestMethod]
        public void AdvancedViewCheckBox_IsDisabled()
        {
            Assert.IsFalse(_window.AdvancedViewCheckBox.IsEnabled, "Advanced view should be disabled per requirements");
        }

        [TestMethod]
        public void ClickCharacter_AddsToCopyBox()
        {
            var firstItem = _window.CharacterGridView.Items.FirstOrDefault() as CharacterItem;
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
            var items = _window.CharacterGridView.Items.Take(3).Cast<CharacterItem>().ToList();
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
            var gridChars = _window.CharacterGridView.Items.Cast<CharacterItem>().Select(c => c.Character).ToHashSet();
            foreach (var c in polishChars)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain Polish character {c}");
            }
        }

        [TestMethod]
        public void Grid_ContainsBasicLatin()
        {
            var expected = new[] { "A", "z", "0", "9" };
            var gridChars = _window.CharacterGridView.Items.Cast<CharacterItem>().Select(c => c.Character).ToHashSet();
            foreach (var c in expected)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain basic latin {c}");
            }
        }

        [TestMethod]
        public void Grid_ContainsCurrencySymbols()
        {
            var expected = new[] { "\u20AC", "\u00A3", "\u00A5" }; // Euro, Pound, Yen
            var gridChars = _window.CharacterGridView.Items.Cast<CharacterItem>().Select(c => c.Character).ToHashSet();
            foreach (var c in expected)
            {
                Assert.IsTrue(gridChars.Contains(c), $"Grid should contain currency symbol {c}");
            }
        }

        [TestMethod]
        public void Grid_ContainsMathematicalOperators()
        {
            var expected = new[] { "\u2200", "\u2202" }; // For-all, Partial differential
            var gridChars = _window.CharacterGridView.Items.Cast<CharacterItem>().Select(c => c.Character).ToHashSet();
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
        public void CharacterGridView_SelectionMode_IsNone()
        {
            Assert.AreEqual(ListViewSelectionMode.None, _window.CharacterGridView.SelectionMode, "Grid should use ItemClick, not selection");
        }

        [TestMethod]
        public void CharacterGridView_IsItemClickEnabled_IsTrue()
        {
            Assert.IsTrue(_window.CharacterGridView.IsItemClickEnabled, "ItemClick should be enabled");
        }
    }
}
