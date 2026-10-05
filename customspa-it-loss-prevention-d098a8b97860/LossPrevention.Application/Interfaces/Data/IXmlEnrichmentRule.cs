using System.Xml.Linq;

namespace LossPrevention.Application.Interfaces.Data
{
    public interface IXmlEnrichmentRule
    {
        bool ShouldApply(XElement xml);
        void Apply(XElement xml);
    }

}
