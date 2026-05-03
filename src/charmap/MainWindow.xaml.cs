using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Composition.SystemBackdrops;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;

namespace charmap
{
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<CharacterItem> _characters = new();
        private List<string> _fontFamilies = new();

        public MainWindow()
        {
            try
            {
                AppLogger.Info("MainWindow constructor starting");
                this.InitializeComponent();
                SetupTitleBar();
                SetupMica();
                LoadFonts();
                SetupLocalization();
                LoadCharacters();
                AppLogger.Info("MainWindow constructor completed successfully");
            }
            catch (Exception ex)
            {
                AppLogger.Fatal("MainWindow constructor failed", ex);
                throw;
            }
        }

        private void SetupTitleBar()
        {
            try
            {
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
                var title = GetString("AppTitle", "Tablica znak\u00F3w");
                this.Title = title;
                TitleTextBlock.Text = title;
                AppLogger.Info("TitleBar setup completed");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupTitleBar failed", ex);
                throw;
            }
        }

        private void SetupMica()
        {
            try
            {
                SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
                AppLogger.Info("Mica Alt backdrop applied");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupMica failed", ex);
                throw;
            }
        }

        private void LoadFonts()
        {
            try
            {
                AppLogger.Info("Loading system fonts...");
                _fontFamilies = CanvasTextFormat.GetSystemFontFamilies()
                    .OrderBy(f => f)
                    .ToList();

                if (_fontFamilies.Count == 0)
                {
                    AppLogger.Info("No system fonts found, using fallback list");
                    _fontFamilies = new List<string> { "Arial", "Segoe UI", "Calibri", "Times New Roman", "Consolas" };
                }

                FontComboBox.ItemsSource = _fontFamilies;
                FontComboBox.SelectedItem = _fontFamilies.Contains("Arial") ? "Arial" : _fontFamilies.First();
                FontComboBox.SelectionChanged += FontComboBox_SelectionChanged;
                AppLogger.Info($"Loaded {_fontFamilies.Count} fonts");
            }
            catch (Exception ex)
            {
                AppLogger.Error("LoadFonts failed, using emergency fallback", ex);
                _fontFamilies = new List<string> { "Arial", "Segoe UI", "Calibri" };
                FontComboBox.ItemsSource = _fontFamilies;
                FontComboBox.SelectedItem = "Arial";
                FontComboBox.SelectionChanged += FontComboBox_SelectionChanged;
            }
        }

        internal void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (FontComboBox.SelectedItem is string fontName)
                {
                    CharacterGridView.FontFamily = new FontFamily(fontName);
                    AppLogger.Info($"Font changed to: {fontName}");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("FontComboBox_SelectionChanged failed", ex);
            }
        }

        private void LoadCharacters()
        {
            try
            {
                AppLogger.Info("Loading Unicode characters...");
                _characters.Clear();
                var ranges = new (int start, int end)[]
                {
                    (0x21, 0x7E),      // Basic Latin
                    (0xA1, 0xFF),      // Latin-1 Supplement
                    (0x100, 0x17F),    // Latin Extended-A
                    (0x180, 0x24F),    // Latin Extended-B
                    (0x250, 0x2AF),    // IPA Extensions
                    (0x2B0, 0x2FF),    // Spacing Modifier Letters
                    (0x300, 0x36F),    // Combining Diacritical Marks
                    (0x370, 0x3FF),    // Greek and Coptic
                    (0x400, 0x4FF),    // Cyrillic
                    (0x1E00, 0x1EFF),  // Latin Extended Additional
                    (0x2000, 0x206F),  // General Punctuation
                    (0x2070, 0x209F),  // Superscripts and Subscripts
                    (0x20A0, 0x20CF),  // Currency Symbols
                    (0x2100, 0x214F),  // Letterlike Symbols
                    (0x2150, 0x218F),  // Number Forms
                    (0x2190, 0x21FF),  // Arrows
                    (0x2200, 0x22FF),  // Mathematical Operators
                    (0x2300, 0x23FF),  // Miscellaneous Technical
                    (0x2500, 0x257F),  // Box Drawing
                    (0x2580, 0x259F),  // Block Elements
                    (0x25A0, 0x25FF),  // Geometric Shapes
                    (0x2600, 0x26FF),  // Miscellaneous Symbols
                    (0x2700, 0x27BF),  // Dingbats
                    (0x3000, 0x303F),  // CJK Symbols and Punctuation
                };

                int count = 0;
                foreach (var (start, end) in ranges)
                {
                    for (int i = start; i <= end; i++)
                    {
                        try
                        {
                            _characters.Add(new CharacterItem
                            {
                                Character = char.ConvertFromUtf32(i),
                                CodePoint = i
                            });
                            count++;
                        }
                        catch (Exception ex)
                        {
                            AppLogger.Error($"Failed to add character at U+{i:X4}", ex);
                        }
                    }
                }

                CharacterGridView.ItemsSource = _characters;
                AppLogger.Info($"Loaded {count} characters into grid");
            }
            catch (Exception ex)
            {
                AppLogger.Error("LoadCharacters failed", ex);
                throw;
            }
        }

        private void CharacterGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (e.ClickedItem is CharacterItem item)
                {
                    CopyTextBox.Text += item.Character;
                    AppLogger.Info($"Character clicked: U+{item.CodePoint:X4} '{item.Character}'");
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("CharacterGridView_ItemClick failed", ex);
            }
        }

        internal void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CopyTextBox.SelectAll();
                CopyTextBox.Focus(FocusState.Programmatic);
                AppLogger.Info("SelectButton clicked");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SelectButton_Click failed", ex);
            }
        }

        internal void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var package = new DataPackage();
                package.SetText(CopyTextBox.Text);
                Clipboard.SetContent(package);
                AppLogger.Info("CopyButton clicked, text copied to clipboard");
            }
            catch (Exception ex)
            {
                AppLogger.Error("CopyButton_Click failed", ex);
            }
        }

        internal void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = GetString("HelpTitle", "Pomoc"),
                    Content = GetString("HelpContent", "Wybierz czcionk\u0119 i kliknij znak, aby doda\u0107 go do pola do skopiowania. U\u017Cyj przycisku Kopiuj, aby skopiowa\u0107 znaki do schowka."),
                    CloseButtonText = GetString("CloseButton", "Zamknij"),
                    XamlRoot = this.Content.XamlRoot
                };
                _ = dialog.ShowAsync();
                AppLogger.Info("Help dialog opened");
            }
            catch (Exception ex)
            {
                AppLogger.Error("HelpButton_Click failed", ex);
            }
        }

        private void SetupLocalization()
        {
            try
            {
                FontLabel.Text = GetString("FontLabel", "Czcionka:");
                HelpButton.Content = GetString("HelpButton", "Pomoc");
                CopyLabel.Text = GetString("CopyLabel", "Znaki do skopiowania:");
                SelectButton.Content = GetString("SelectButton", "Wybierz");
                CopyButton.Content = GetString("CopyButton", "Kopiuj");
                AdvancedViewCheckBox.Content = GetString("AdvancedView", "Widok zaawansowany");
                AppLogger.Info("Localization setup completed");
            }
            catch (Exception ex)
            {
                AppLogger.Error("SetupLocalization failed", ex);
                throw;
            }
        }

        private string GetString(string key, string fallback)
        {
            try
            {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                var value = loader.GetString(key);
                if (string.IsNullOrEmpty(value))
                {
                    AppLogger.Info($"Resource key '{key}' returned empty, using fallback");
                    return fallback;
                }
                return value;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"GetString failed for key '{key}', using fallback", ex);
                return fallback;
            }
        }
    }
}
