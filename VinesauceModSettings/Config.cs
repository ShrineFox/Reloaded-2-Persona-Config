using BorangeModSettings.Template.Configuration;
using System.ComponentModel;

namespace BorangeModSettings.Configuration
{
	public class Config : Configurable<Config>
	{
       
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
	public class ConfiguratorMixin : ConfiguratorMixinBase
	{
		// 
	}
}