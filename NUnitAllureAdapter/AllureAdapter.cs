using NUnit.Core.Extensibility;

namespace NUnitAllureAdapter
{
    [NUnitAddin(Name = "Allure adapter", Type = ExtensionType.Core)]
    public class AllureAdapter : IAddin
    {
        public bool Install(IExtensionHost host)
        {
            IExtensionPoint listeners = host.GetExtensionPoint("EventListeners");
            listeners.Install(new AllureEventListener());
            return true;
        }
    }
}
