using System.Reflection;
using System.Runtime.InteropServices;
using log4net.Config;

using Xunit;

[assembly: AssemblyTitle("CentauroTech.Log4net.ElasticSearch.Tests")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("CentauroTech.Log4net.ElasticSearch.Tests")]
[assembly: AssemblyCopyright("Copyright ©  2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("5b0b6737-b33b-436e-aeab-33f32554855b")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Original UT are poorly designed and use static classes with state for clocks. This causes race conditions when run in parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)] 

[assembly: XmlConfigurator(ConfigFile = "logConfig.xml", Watch = true)]
