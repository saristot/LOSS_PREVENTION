using LossPrevention.Application.Interfaces.Data;
using System.Xml.Linq;

namespace LossPrevention.Application.Services.DataIngestion
{
    public sealed class XmlEnrichmentService : IXmlEnrichmentService
    {
        private readonly List<IXmlEnrichmentRule> _rules;

        public XmlEnrichmentService(IEnumerable<IXmlEnrichmentRule> rules)
        {
            _rules = rules.ToList();
        }

        public XElement Enrich(XElement transaction)
        {
            foreach (var rule in _rules)
            {
                rule.Apply(transaction);
            }

            return transaction;
        }
    }

}
