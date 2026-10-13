using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Core.Localization.Abstraction;

namespace Core.Localization.Resource.Yaml.DependencyInjection;

public static class ServiceCollectionResourceLocalizationManagerExtension
{
	/// <summary>
	/// Adds <see cref="ResourceLocalizationManager"/> as <see cref="ILocalizationService"/> to <see cref="IServiceCollection"/>.
	/// <list type="bullet">
	///    <item>
	///        <description>Reads all yaml files in the specified assembly's Features/{featureName}/Resources/Locales/ directory. Yaml file names must be like {uniqueKeySectionName}.{culture}.yaml.</description>
	///    </item>
	///    <item>
	///        <description>If you don't want separate locale files with sections, create Features/Index/Resources/Locales/index.{culture}.yaml.</description>
	///    </item>
	/// </list>
	/// </summary>
	public static IServiceCollection AddYamlResourceLocalization(
		this IServiceCollection services,
		Assembly assembly
	)
	{
		services.AddScoped<ILocalizationService, ResourceLocalizationManager>(_ =>
		{
			// <locale, <featureName, resourceDir>>
			Dictionary<string, Dictionary<string, string>> resources = [];

			string assemblyDirectory =
				Path.GetDirectoryName(assembly.Location)
				?? throw new InvalidOperationException(
					$"Could not determine the directory of assembly '{assembly.FullName}'."
				);

			string featuresDirectory =
				Path.Combine(assemblyDirectory, "Features");

			if (!Directory.Exists(featuresDirectory))
			{
				throw new DirectoryNotFoundException(
					$"Features directory not found: {featuresDirectory}"
				);
			}

			string[] featureDirs =
				Directory.GetDirectories(featuresDirectory);

			foreach (string featureDir in featureDirs)
			{
				string featureName = Path.GetFileName(featureDir);

				string localeDir =
					Path.Combine(featureDir, "Resources", "Locales");

				if (!Directory.Exists(localeDir))
					continue;

				string[] localeFiles =
					Directory.GetFiles(localeDir, "*.yaml");

				foreach (string localeFile in localeFiles)
				{
					string localeName =
						Path.GetFileNameWithoutExtension(localeFile);

					int separatorIndex =
						localeName.IndexOf('.');

					if (separatorIndex < 0)
						continue;

					string localeCulture =
						localeName[(separatorIndex + 1)..];

					if (!resources.ContainsKey(localeCulture))
						resources.Add(localeCulture, []);

					resources[localeCulture].Add(
						featureName,
						localeFile
					);
				}
			}

			return new ResourceLocalizationManager(resources);
		});

		return services;
	}
}