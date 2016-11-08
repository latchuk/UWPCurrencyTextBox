using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace UWPCurrencyTextBox
{
    public sealed class CurrencyTextBox : TextBox
    {
        private static StringFormatConverter _stringFormatConverter = new StringFormatConverter();

        public decimal Value
        {
            get { return (decimal)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(decimal), typeof(CurrencyTextBox), new PropertyMetadata(0M));

        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StringFormat.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StringFormatProperty =
            DependencyProperty.Register("StringFormat", typeof(string), typeof(CurrencyTextBox), new PropertyMetadata("{0:C}", StringFormatPropertyChanged));

        private static void StringFormatPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((CurrencyTextBox)obj).UpdateTextBinding();
        }

        public CurrencyTextBox()
        {
            this.DefaultStyleKey = typeof(CurrencyTextBox);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            UpdateTextBinding();

            SelectionStart = Text.Length;

            TextChanged += CurrencyTextBox_TextChanged;
            SelectionChanged += CurrencyTextBox_SelectionChanged;
        }

        private void CurrencyTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (SelectionStart != Text.Length)
            {
                SelectionStart = Text.Length;
                SelectionLength = 0;
            }
        }

        private void UpdateTextBinding()
        {
            // Update the Text binding with the actual StringFormat
            var textBinding = new Binding();
            textBinding.Path = new PropertyPath("Value");
            textBinding.RelativeSource = new RelativeSource() { Mode = RelativeSourceMode.Self };
            textBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            textBinding.Converter = _stringFormatConverter;
            textBinding.ConverterParameter = StringFormat;

            BindingOperations.SetBinding(this, CurrencyTextBox.TextProperty, textBinding);
        }

        private void CurrencyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as CurrencyTextBox;

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Value = 0;
            }
            else
            {
                tb.SelectionStart = tb.Text.Length;
            }
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            e.Handled = true;

            if (IsNumericKey(e.Key))
            {
                // Push the new number from the right
                if (this.Value < 0)
                {
                    Value = (Value * 10M) - (GetDigitFromKey(e.Key) / 100M);
                }
                else
                {
                    Value = (Value * 10M) + (GetDigitFromKey(e.Key) / 100M);
                }
            }
            else if (IsBackspaceKey(e.Key))
            {
                // Remove the right-most digit
                Value = (Value - (Value % 0.1M)) / 10M;
            }
            else if (IsDeleteKey(e.Key))
            {
                // Reset Value
                Value = 0M;
            }
            else if (IsSubtractKey(e.Key))
            {
                // Change Value to negative or positive
                Value *= -1;
            }
            else if (IsIgnoredKey(e.Key))
            {
                e.Handled = false;
                base.OnKeyDown(e);
            }
            else if (IsCopyKey(e.Key))
            {
                // Copy Value to Clipboard
                var dataPackage = new DataPackage();
                dataPackage.SetText(Value.ToString("0.00"));
                Clipboard.SetContent(dataPackage);
            }
        }

        private decimal GetDigitFromKey(VirtualKey key)
        {
            switch (key)
            {
                case VirtualKey.Number0:
                case VirtualKey.NumberPad0: return 0M;
                case VirtualKey.Number1:
                case VirtualKey.NumberPad1: return 1M;
                case VirtualKey.Number2:
                case VirtualKey.NumberPad2: return 2M;
                case VirtualKey.Number3:
                case VirtualKey.NumberPad3: return 3M;
                case VirtualKey.Number4:
                case VirtualKey.NumberPad4: return 4M;
                case VirtualKey.Number5:
                case VirtualKey.NumberPad5: return 5M;
                case VirtualKey.Number6:
                case VirtualKey.NumberPad6: return 6M;
                case VirtualKey.Number7:
                case VirtualKey.NumberPad7: return 7M;
                case VirtualKey.Number8:
                case VirtualKey.NumberPad8: return 8M;
                case VirtualKey.Number9:
                case VirtualKey.NumberPad9: return 9M;
                default: throw new ArgumentOutOfRangeException("Invalid key: " + key.ToString());
            }
        }

        private bool IsNumericKey(VirtualKey key)
        {
            return key == VirtualKey.Number0 ||
                key == VirtualKey.Number1 ||
                key == VirtualKey.Number2 ||
                key == VirtualKey.Number3 ||
                key == VirtualKey.Number4 ||
                key == VirtualKey.Number5 ||
                key == VirtualKey.Number6 ||
                key == VirtualKey.Number7 ||
                key == VirtualKey.Number8 ||
                key == VirtualKey.Number9 ||
                key == VirtualKey.NumberPad0 ||
                key == VirtualKey.NumberPad1 ||
                key == VirtualKey.NumberPad2 ||
                key == VirtualKey.NumberPad3 ||
                key == VirtualKey.NumberPad4 ||
                key == VirtualKey.NumberPad5 ||
                key == VirtualKey.NumberPad6 ||
                key == VirtualKey.NumberPad7 ||
                key == VirtualKey.NumberPad8 ||
                key == VirtualKey.NumberPad9;
        }

        private bool IsBackspaceKey(VirtualKey key)
        {
            return key == VirtualKey.Back;
        }

        private bool IsDeleteKey(VirtualKey key)
        {
            return key == VirtualKey.Delete;
        }

        private bool IsIgnoredKey(VirtualKey key)
        {
            return key == VirtualKey.Up ||
                key == VirtualKey.Down ||
                key == VirtualKey.Tab ||
                key == VirtualKey.Shift ||
                key == VirtualKey.Enter;
        }

        private bool IsSubtractKey(VirtualKey key)
        {
            return key == VirtualKey.Subtract;
        }

        private bool IsCopyKey(VirtualKey key)
        {
            return Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && key == VirtualKey.C;
        }

    }
}
