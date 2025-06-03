using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

using PgManager.Constants;

using Wpf.Ui.Controls;

namespace PgManager.Converters
{
    internal class KeyToIconConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case DbTreeNodeKeys.Database:
                    return SymbolRegular.Database20;
                case DbTreeNodeKeys.Shema:
                    return SymbolRegular.Connected20;
                case DbTreeNodeKeys.Table:
                    return SymbolRegular.Table20;
                case DbTreeNodeKeys.Column:
                    return SymbolRegular.TableInsertColumn20;
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
