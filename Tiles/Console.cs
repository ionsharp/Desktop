using Imagin.Core;
using Imagin.Core.Controls;
using Imagin.Core.Storage;
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Xml.Serialization;

namespace Imagin.Apps.Desktop
{
    [DisplayName("Console"), Serializable]
    public class ConsoleTile : Tile, IFrameworkReference
    {
        [field: NonSerialized]
        public static readonly ReferenceKey<Core.Controls.Console> ConsoleReferenceKey = new();

        [Hidden, XmlIgnore]
        public Core.Controls.Console Console { get; private set; }

        ConsoleOptions consoleOptions = new();
        public ConsoleOptions ConsoleOptions
        {
            get => consoleOptions;
            set => this.Change(ref consoleOptions, value);
        }

        string path = StoragePath.Root;
        [Hidden]
        public string Path
        {
            get => path;
            set => this.Change(ref path, value);
        }

        [Hidden, XmlIgnore]
        public override string Title
        {
            get => base.Title;
            set => base.Title = value;
        }

        public ConsoleTile() : base() { }

        public ConsoleTile(string path) : base()
        {
            Path = path;
        }

        void IFrameworkReference.SetReference(IFrameworkKey key, FrameworkElement element)
        {
            if (key == ConsoleReferenceKey)
                Console = (Core.Controls.Console)element;
        }

        public override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(Path):
                    OnChanged();
                    break;
            }
        }
    }
}