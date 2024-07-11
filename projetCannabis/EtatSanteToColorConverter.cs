using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace projetCannabis
{
    public class EtatSanteToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string etatSante)
            {
                switch (etatSante)
                {
                    case "Bonne santé":
                        return Brushes.Green;
                    case "Moyenne santé":
                        return Brushes.Yellow;
                    case "Mauvaise santé":
                        return Brushes.Orange;
                    case "Santé en danger":
                        return Brushes.Red;
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}


