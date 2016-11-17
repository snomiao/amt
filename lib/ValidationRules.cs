using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace YTY
{
  public class GreaterThanRule : ValidationRule
  {
    private int minimum;

    public int Min
    {
      set { minimum = value; }
    }

    public GreaterThanRule() { }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
      int val;
      if (!int.TryParse(value as string, out val))
        return new ValidationResult(false, "Not numeric");
      if (val < minimum)
        return new ValidationResult(false, "Not greater than");
      return new ValidationResult(true, "Success");
    }
  }
}
