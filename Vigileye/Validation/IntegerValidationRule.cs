using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Vigileye.Validation
{
    public class IntegerValidationRule : ValidationRule
    {
        // Surcharge de la méthode Validate pour implémenter la logique de validation
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            // Vérifie si la valeur est une chaîne non nulle et tente de la convertir en entier
            if (value is string stringValue && int.TryParse(stringValue, out int intValue))
            {
                // Vérifie si l'entier est positif
                if (intValue >= 0)
                {
                    return ValidationResult.ValidResult; // La validation est réussie
                }
                else
                {
                    return new ValidationResult(false, "Doit être un nombre entier positif."); // L'entier est négatif
                }
            }
            else
            {
                return new ValidationResult(false, "Doit être un nombre entier."); // La conversion a échoué
            }
        }
    }

}
