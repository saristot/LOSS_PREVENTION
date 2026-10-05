using System.Xml.Linq;

namespace LossPrevention.Application.Interfaces.Data
{
    public interface IXmlEnrichmentService
    {
        public XElement Enrich(XElement xml);
    }
}