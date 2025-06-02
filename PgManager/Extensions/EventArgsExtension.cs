using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PgManager.Extensions
{
    public sealed class EventArgsExtension : MarkupExtension
    {

        public PropertyPath? Path { get; set; }

        public IValueConverter? Converter { get; set; }

        public object? ConverterParameter { get; set; }


        public Type ConverterTargetType { get; set; } = typeof(object);


        [TypeConverter(typeof(CultureInfoIetfLanguageTagConverter))]
        public CultureInfo? ConverterCulture { get; set; }


        public EventArgsExtension() { }

        public EventArgsExtension(string path)
        {
            Path = new PropertyPath(path);
        }


        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        internal object? GetArgumentValue(EventArgs eventArgs, XmlLanguage? language)
        {
            if (Path == null)
                return eventArgs;

            object? value = PropertyPathHelper.Evaluate(Path, eventArgs);

            if (Converter != null)
                value = Converter.Convert(value, ConverterTargetType, ConverterParameter, ConverterCulture ?? language?.GetSpecificCulture() ?? CultureInfo.CurrentUICulture);

            return value;
        }
        internal static class PropertyPathHelper
        {
            private static readonly object s_fallbackValue = new object();

            public static object? Evaluate(PropertyPath path, object source)
            {
                var target = new DependencyTarget();
                var binding = new Binding() { Path = path, Source = source, Mode = BindingMode.OneTime, FallbackValue = s_fallbackValue };
                BindingOperations.SetBinding(target, DependencyTarget.ValueProperty, binding);

                if (target.Value == s_fallbackValue)
                    throw new ArgumentException($"Could not resolve property path '{path.Path}' on source object type '{source.GetType()}'.");

                return target.Value;
            }

            private class DependencyTarget : DependencyObject
            {
                public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(DependencyTarget));

                public object? Value
                {
                    get => GetValue(ValueProperty);
                    set => SetValue(ValueProperty, value);
                }
            }
        }
    }
    public sealed class EventSenderExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
