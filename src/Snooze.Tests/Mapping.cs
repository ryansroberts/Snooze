using System;
using Machine.Specifications;

namespace Snooze
{



	public class SourceType
	{
		public string Mapped { get; set; }

		public Guid Convertable { get; set; }

		public string NotMapped { get; set; }
	}

	public class DestType
	{
		public string Mapped { get;  set; }

		public string Convertable { get; set; }
	}

	public class MappingController : ResourceController
	{
		 
	}

	public class SelfMappingType
	{
		public string Mapped { get; protected set; }

		public SelfMappingType(Func<SourceType, LeftMappingConfigurator<SourceType>> map, SourceType sourceType)
		{
			map(sourceType).To(s => this).Run();
		}
	}


	public class self_mapping_type
	{
		static SelfMappingType instance;
		Because of = () => instance = new SelfMappingType(new MappingController().Map, new SourceType() {Mapped = "Mapped"});

		It has_mapped_property = () => instance.Mapped.ShouldEqual("Mapped");
	}

	public class mapping_to_type
	{
		static DestType mapped;
		Because of = () => mapped = new MappingController()
			.Map(new SourceType(){Mapped = "Mapped",Convertable = Guid.NewGuid()})
			.To<DestType>()
			.Convert<Guid,string>(g => g.ToString())
			.Item;

		It has_mapped = () => mapped.Mapped.ShouldEqual("Mapped");

		It has_converted = () => mapped.Convertable.ShouldNotBeNull();
	}


	public class mapping_to_to_expression
	{
		static DestType mapped;
		static DestType instance;

		Because of = () => mapped = new MappingController()
			.Map(new SourceType() { Mapped = "Mapped", })
			.To(s => instance = new DestType()).Item;

		It has_mapped = () => mapped.Mapped.ShouldEqual("Mapped");



		It is_same_object = () => instance.ShouldBeTheSameAs(instance);
	}




}