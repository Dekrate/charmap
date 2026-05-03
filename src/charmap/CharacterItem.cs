using System.ComponentModel;

namespace charmap
{
    public class CharacterItem : INotifyPropertyChanged
    {
        public string Character { get; set; } = "";
        public int CodePoint { get; set; }

        private string _fontFamily = "Arial";
        public string FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily != value)
                {
                    _fontFamily = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FontFamily)));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
