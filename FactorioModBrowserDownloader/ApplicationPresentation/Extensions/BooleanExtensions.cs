using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace FactorioNexus.ApplicationPresentation.Extensions
{
    public class True : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => true;
    }
}
