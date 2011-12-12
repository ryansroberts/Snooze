using System.Collections.Generic;
using Spark;
using Spark.FileSystem;

namespace Snooze.MSpec
{
	public class TestSparkSettings : ISparkSettings
	{
		IEnumerable<IViewFolderSettings> folders;
		IEnumerable<string> namespaces;
		IEnumerable<string> assemblies;

		public class FolderSettings : IViewFolderSettings
		{
			public string Name { get; set; }
			public ViewFolderType FolderType { get; set; }
			public string Type { get; set; }
			public string Subfolder { get; set; }
			public IDictionary<string, string> Parameters { get; set; }
		}

		public TestSparkSettings(IEnumerable<IViewFolderSettings> folders, IEnumerable<string> namespaces, IEnumerable<string> assemblies)
		{
			this.folders = folders;
			this.namespaces = namespaces;
			this.assemblies = assemblies;
			PageBaseType = typeof(TestSparkView).FullName;
		}

		public bool AutomaticEncoding
		{
			get { return true; }
		}

		public string StatementMarker
		{
			get { return "#"; }
		}

		public bool Debug
		{
			get { return false; }
		}

		public NullBehaviour NullBehaviour
		{
			get { return NullBehaviour.Strict; }
		}

		public string Prefix
		{
			get { return ""; }
		}

		public string PageBaseType { get; set; }

		public LanguageType DefaultLanguage
		{
			get { return LanguageType.CSharp; }
		}

		public IEnumerable<string> UseNamespaces
		{
			get { return namespaces; }
		}

		public IEnumerable<string> UseAssemblies
		{
			get { return assemblies; }
		}

		public IEnumerable<IResourceMapping> ResourceMappings
		{
			get { return new IResourceMapping[] { }; }
		}

		public IEnumerable<IViewFolderSettings> ViewFolders
		{
			get { return folders; }
		}

		public bool ParseSectionTagAsSegment
		{
			get { return false; }
		}
	}
}